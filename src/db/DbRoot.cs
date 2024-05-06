using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GaiaLabs
{
    public class DbRoot
    {
        //public Dictionary<string, Dictionary<string, Type>> Types { get; set; }
        public CopLib CopLib { get; set; }
        private IEnumerable<DbBlock> _blocks;
        public IEnumerable<DbBlock> Blocks
        {
            get => _blocks;
            set { _blocks = value; foreach (var p in _blocks) p.Root = this; }
        }

        public IEnumerable<DbFile> Files { get; set; }
        public IEnumerable<DbObject> Objects { get; set; }
        public string[] CharMap { get; set; }
        public string[] WideMap { get; set; }

        public IDictionary<string, DbStruct> Structs { get; set; }
        public IDictionary<HexString, DbStringCommand> StringCommands { get; set; }
        public IDictionary<HexString, DbStringCommand> WideCommands { get; set; }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        public static DbRoot FromFile(string dbFile)
        {
            using var file = File.OpenRead(dbFile);
            return JsonSerializer.Deserialize<DbRoot>(file, _jsonOptions);
        }
    }

    public class DbBlock
    {
        internal DbRoot Root;

        public string Name { get; set; }

        private IEnumerable<DbPart> _parts;
        public IEnumerable<DbPart> Parts
        {
            get => _parts;
            set { _parts = value; foreach (var p in _parts) p.Block = this; }
        }

        public bool IsOutside(Location loc, out DbPart part)
        {
            if (Parts.All(x => x.IsOutside(loc)))
                foreach (var b in Root.Blocks) //Find chunk this reference belongs to
                    if (b != this && b.IsInside(loc, out part))
                        return true;

            part = null;
            return false;
        }

        public bool IsInside(Location loc, out DbPart part)
        {
            foreach (var p in Parts)
                if (!p.IsOutside(loc))
                { part = p; return true; }

            part = null;
            return false;
        }

        public IEnumerable<DbBlock> GetIncludes()
        {
            return Parts.SelectMany(x => x.Includes).Select(x => x.Block).Distinct();
        }
    }

    public class DbPart
    {
        internal DbBlock Block;
        internal Op Head;
        internal HashSet<DbPart> Includes;
        //internal object[] Table;
        internal object ObjectRoot;

        public string Name { get; set; }
        public PartType Type { get; set; }
        public Location Start { get; set; }
        public Location End { get; set; }
        public string Struct { get; set; }

        public bool IsInside(Location loc) => loc >= Start && loc < End;
        public bool IsOutside(Location loc) => loc < Start || loc >= End;
    }

    public class DbFile
    {
        public string Name { get; set; }
        public BinType Type { get; set; }
        public Location Start { get; set; }
        public Location End { get; set; }
        public bool Compressed { get; set; }
    }

    public class DbObject
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Struct { get; set; }
        public Location Start { get; set; }
        public Location End { get; set; }
    }

    public class CopLib
    {
        public Dictionary<HexString, CopDef> Codes { get; set; }
        public Dictionary<AddressingMode, string> Formats { get; set; }
    }

    public class CopDef
    {
        public string Mnem { get; set; }
        public HexString Code { get; set; }
        public byte Size { get; set; }
        public string Parts { get; set; }
        public bool Halt { get; set; }

        public string GetFormat()
        {
            var len = Parts.Length;
            var builder = new StringBuilder();

            builder.Append("{0} [{1:X2}]"); //Append instruction
            if (len > 0)
            {
                for (int i = 0; i < len; i++)
                {
                    var p = Parts[i];
                    builder.Append(i == 0 ? " ( " : ", "); //Separator
                    if (p == 'b' || p == 'w') builder.Append('#'); //Imm symbol
                    builder.Append($"{{{i + 2}"); //Operand

                    if (p == 'b') builder.Append(":X2"); //Hex byte
                    else if (p == 'w') builder.Append(":X4"); //Hex word

                    builder.Append('}'); //End operand
                }
                builder.Append(" )"); //End
            }

            return builder.ToString();
        }
    }

    public class DbStruct
    {
        public string Name { get; set; }
        public string[] Parts { get; set; }
        public string[] Types { get; set; }
        public string Parent { get; set; }
        public HexString? Delimiter { get; set; }

        public HexString? Descriminator { get; set; }

        //private int? _size;
        //internal int Size
        //{
        //    get => _size ??= Types.Sum(x => x switch
        //        {
        //            MemberType.Byte => 1,
        //            MemberType.Word or
        //            MemberType.Offset => 2,
        //            MemberType.Address => 3,
        //            _ => 1
        //        });
        //}
    }

    public class DbStringCommand
    {
        public HexString Code { get; set; }
        public string Value { get; set; }
        public MemberType[] Types { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PartType
    {
        Code,
        Subroutine,
        Table,
        Array
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MemberType
    {
        Byte,
        Word,
        Offset,
        Address,
        Binary,
        String,
        CompString,
        WideString
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BinType
    {
        Bitmap,
        Tilemap,
        Tileset,
        Palette,
        Sound,
        Music,
        Unknown
    }

    public enum ObjectType
    {
        Lookup,
        Array
    }
}
