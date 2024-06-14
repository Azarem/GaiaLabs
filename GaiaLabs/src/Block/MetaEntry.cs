
using GaiaLabs.src;
using GaiaLib;
using GaiaLib.Rom;
using GaiaLib.Structs;
using System.Reflection;

namespace GaiaLabs
{
    public class MetaEntry : DataEntry
    {
        public byte Command { get; set; }
        public byte Value1 { get; set; }
        public byte Value2 { get; set; }
        public byte Value3 { get; set; }
        public byte Value4 { get; set; }

        public DataReference Reference { get; set; }

        public MetaEntry() { }

        public override bool Unpack(RomLoader loader)
        {
            Command = loader.ReadByte();

            switch (Command)
            {
                case 0: return false;
                case 2:
                    var s2 = loader.ReadStruct<Meta2>();
                    Value1 = s2.Value;
                    break;
                case 3:
                    var s3 = loader.ReadStruct<Meta3>();
                    Value1 = s3.Unknown1;
                    Value2 = s3.Unknown2;
                    Value3 = s3.Unknown3;
                    Value4 = s3.Unknown4;
                    var tex = loader.GetReference<TextureEntry>((Location)s3.Address);
                    if(Value2 == 0x20)
                    {
                        tex.Height = 32 * 8;
                    }
                    Reference = new DataReference((Address)Location, 3, 3, tex);
                    tex.References.Add(Reference);
                    break;
                case 4:
                    var s4 = loader.ReadStruct<Meta4>();
                    Value1 = s4.Unknown1;
                    Value2 = s4.Unknown2;
                    Value3 = s4.Unknown3;
                    //var tex = loader.GetReference<TextureEntry>(s4.Address);
                    Reference = new DataReference((Address)Location, 3, 3);
                    //tex.References.Add(Reference);
                    break;
                case 5:
                    var s5 = loader.ReadStruct<Meta5>();
                    Value1 = s5.Unknown1;
                    Value2 = s5.Unknown2;
                    Value3 = s5.Unknown3;
                    Value4 = s5.Unknown4;
                    //var tex = loader.GetReference<TextureEntry>(s4.Address);
                    Reference = new DataReference((Address)Location, 3, 4);
                    //tex.References.Add(Reference);
                    break;
                case 6:
                    var s6 = loader.ReadStruct<Meta6>();
                    Value1 = s6.Unknown1;
                    //var tex = loader.GetReference<TextureEntry>(s4.Address);
                    Reference = new DataReference((Address)Location, 3, 1);
                    //tex.References.Add(Reference);
                    break;
                case 0x10:
                    var s10 = loader.ReadStruct<Meta10>();
                    Value1 = s10.Unknown1;
                    Value2 = s10.Unknown2;
                    Value3 = s10.Unknown3;
                    //var tex = loader.GetReference<TextureEntry>(s4.Address);
                    Reference = new DataReference((Address)Location, 3, 3);
                    //tex.References.Add(Reference);
                    break;
                case 0x11:
                    var s11 = loader.ReadStruct<Meta11>();
                    Value1 = s11.Unknown1;
                    Value2 = s11.Unknown2;
                    //var tex = loader.GetReference<TextureEntry>(s4.Address);
                    Reference = new DataReference((Address)Location, 3, 2);
                    //tex.References.Add(Reference);
                    break;
                case 0x13:
                    var s13 = loader.ReadStruct<Meta13>();
                    Value1 = s13.Unknown1;
                    Value2 = s13.Unknown2;
                    break;
                case 0x14:
                    var s14 = loader.ReadStruct<Meta14>();
                    Value1 = s14.Value;
                    break;
                case 0x15:
                    var s15 = loader.ReadStruct<Meta15>();
                    Value1 = s15.Value;
                    break;
                case 0x17:
                    var s17 = loader.ReadStruct<Meta17>();
                    Value1 = s17.Unknown1;
                    Value2 = s17.Unknown2;
                    //var tex = loader.GetReference<TextureEntry>(s4.Address);
                    Reference = new DataReference((Address)Location, 3, 2);
                    //tex.References.Add(Reference);
                    break;
                default:
                    Event.TriggerWarning($"Unknown Meta type {Command}");
                    return false;
            }

            return base.Unpack(loader);
        }
    }
}
