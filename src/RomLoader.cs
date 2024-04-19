//using ICSharpCode.SharpZipLib.GZip;
//using ICSharpCode.SharpZipLib.Zip;
using System.IO.MemoryMappedFiles;
using System.IO;
using System;
using System.Collections.Generic;
using Godot;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection.Emit;
using System.Collections.Specialized;

namespace GaiaLabs
{
    public unsafe class RomLoader : IDisposable
    {
        public static RomLoader Current { get; set; }

        public RomMap Map { get; private set; }

        private MemoryMappedFile _mappedFile;
        private MemoryMappedViewAccessor _viewAccessor;
        public RomHeader Header;
        public byte* _baseAddress;
        public IntPtr _basePtr;
        public uint _offset;
        public DbRoot DbRoot;
        public Dictionary<Location, string> RefList = new();
        //private bool _hasCopyHeader;

        public Dictionary<uint, DataEntry> Resources = new();

        protected RomLoader(string file)
        {
            LoadInternal(file);
        }

        protected void LoadInternal(string file)
        {
            long fileSize = 0;
            string extension = file.GetExtension();

            //void processZip(Stream inStream)
            //{
            //    var outStream = new FileStream(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 0, FileOptions.RandomAccess | FileOptions.DeleteOnClose);

            //    using (inStream)
            //        if (extension == "gz")
            //            GZip.Decompress(inStream, outStream, false);
            //        else
            //            inStream.CopyTo(outStream);

            //    outStream.Flush();
            //    outStream.Position = 0;
            //    fileSize = outStream.Length;

            //    _mappedFile = MemoryMappedFile.CreateFromFile(outStream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
            //};

            switch (extension)
            {
                //case "zip":
                //    using (var newFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(file))
                //        foreach (ZipEntry entry in newFile as IEnumerable)
                //            if ((extension = entry.Name.GetExtension()) == "smc" || extension == "sfc")
                //            {
                //                processZip(newFile.GetInputStream(entry.ZipFileIndex));
                //                break;
                //            }
                //    break;

                //case "gz":
                //    processZip(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read));
                //    break;

                case "smc":
                case "sfc":
                    fileSize = new FileInfo(file).Length;
                    _mappedFile = MemoryMappedFile.CreateFromFile(file, FileMode.Open, null, 0, MemoryMappedFileAccess.ReadWrite);
                    break;

                default:
                    throw new("Unable to determine file type from extension.");
            }

            long cartHeader = fileSize % 0x400;

            if (cartHeader != 0 && cartHeader != 0x200)
                throw new("Invalid file size");

            _viewAccessor = _mappedFile.CreateViewAccessor(cartHeader, fileSize - cartHeader);
            _viewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _baseAddress);

            _basePtr = (IntPtr)_baseAddress;
            Header = *(RomHeader*)(_baseAddress + 0xffc0);

            Current = this;
        }

        public static RomLoader Load(string file)
        {
            var loader = new RomLoader(file);
            loader.Map = new RomMap(loader);
            Event.TriggerLoaded(loader.Map);
            return loader;
        }

        public Location GetRelative(Location loc, byte size = 2, byte offset = 0, byte? bank = null)
            => size switch
            {
                2 => (Location)new Address(bank ?? loc.Bank, *(ushort*)(_baseAddress + loc)),
                3 => (Location)new Address(bank ?? *(_baseAddress + loc + offset + 2), *(ushort*)(_baseAddress + loc + offset)),
                _ => throw new("Invalid reference size")
            };

        public byte ReadByte() => ReadByte(_offset++);
        public byte ReadByte(uint offset) => _baseAddress[offset];
        public ushort ReadUInt16() => ReadUInt16((_offset += 2) - 2);

        public ushort ReadUInt16(uint offset) => *(ushort*)(_basePtr + offset);
        //public byte PeekByte() => ReadByte(_offset);

        public T ReadStruct<T>() where T : struct
        {
            var ptr = (T*)(_basePtr + _offset);
            _offset += (uint)sizeof(T);
            return *ptr;
        }

        public ICollection<T> ReadList<T>() where T : DataEntry, new()
        {
            var off = _offset;
            var list = new List<T>();
            var current = new T() { Location = off };
            while (current.Unpack(this))
            {
                current.Size = _offset - off;
                list.Add(current);
                if (!current.HasNext(this)) break;
                current = new T() { Location = off = _offset };
            }
            return list;
        }
        public ICollection<T> ReadList<T>(uint offset) where T : DataEntry, new()
        {
            _offset = offset;
            return ReadList<T>();
        }

        public T GetReference<T>(uint offset) where T : DataEntry, new()
        {
            if (!Resources.TryGetValue(offset, out var entry))
                Resources[offset] = entry = new T() { Location = offset };

            return entry as T;
        }

        public void DumpDatabase(string outPath, string dbFile = "database.json")
        {
            using (var file = File.OpenRead(dbFile))
                DbRoot = JsonSerializer.Deserialize<DbRoot>(file);

            RefList = new ();

            foreach (var block in DbRoot.Blocks)
                foreach (var part in block.Parts) //Analyze code and place markers
                {
                    var reg = new Registers();
                    Op prev = null;
                    RefList.Add(part.Start, part.Name); //Add reference name
                    part.Includes = new List<DbPart>(); //Initialize part
                    for (var loc = part.Start; loc < part.End; loc += prev.Size)
                    {
                        var op = Asm.Parse(_baseAddress + loc, reg, DbRoot); //Parse instruction

                        if (prev == null)
                            part.Head = op; //Set head
                        else
                            op.Prev = prev; //Set prev

                        prev = op; //Advance prev
                    }
                }

            //Add code references
            foreach (var block in DbRoot.Blocks)
                foreach (var part in block.Parts)
                    for (var op = part.Head; op != null; op = op.Next)
                        foreach (var loc in op?.References)
                        {
                            RefList.TryAdd(loc, "loc_" + loc);
                            if (block.IsOutside(loc, out var p))
                                part.Includes.Add(p);
                        }

            //Write files
            foreach (var block in DbRoot.Blocks)
            {
                var outFile = Path.Combine(outPath, block.Name + ".asm");
                using var outStream = File.Create(outFile);
                using var writer = new StreamWriter(outStream);

                foreach (var inc in block.GetIncludes())
                    writer.WriteLine("#include '{0}'", inc.Name); //Write includes

                writer.WriteLine(); //Empty line

                foreach (var part in block.Parts) //Iterate over each part
                    for (var op = part.Head; op != null; op = op.Next) //Process each instruction in sequence
                    {
                        if (RefList.TryGetValue(op.Location, out var label)) //Check for code label
                            writer.WriteLine("{0}:", label); //Write label
                        writer.WriteLine("  {0}", op); //Write instruction
                    }
            }
        }

        public void Dispose()
        {
            if (_mappedFile != null)
            {
                _mappedFile.Dispose();
                _mappedFile = null;
            }
        }


        public enum SeekSet
        {
            Beginning = 0,
            Current = 1,
            End = 2
        }


        public enum HeaderField
        {
            CartName = 0x00,
            Mapper = 0x15,
            RomType = 0x16,
            RomSize = 0x17,
            RamSize = 0x18,
            CartRegion = 0x19,
            Company = 0x1a,
            Version = 0x1b,
            Complement = 0x1c,  //inverse checksum
            Checksum = 0x1e,
            ResetVector = 0x3c,
        };

        public enum CartType
        {
            TypeNormal = 0,
            TypeBsxSlotted,
            TypeBsxBios,
            TypeBsx,
            TypeSufamiTurboBios,
            TypeSufamiTurbo,
            TypeSuperGameBoy1Bios,
            TypeSuperGameBoy2Bios,
            TypeGameBoy,
            TypeUnknown,
        };

        public enum Region
        {
            NTSC = 0,
            PAL,
        };

        public enum MemoryMapper
        {
            LoROM = 0,
            HiROM,
            ExLoROM,
            ExHiROM,
            SuperFXROM,
            SA1ROM,
            SPC7110ROM,
            BSCLoROM,
            BSCHiROM,
            BSXROM,
            STROM,
        };

        public enum DSP1MemoryMapper
        {
            DSP1Unmapped = 0,
            DSP1LoROM1MB,
            DSP1LoROM2MB,
            DSP1HiROM,
        };
    }

}
