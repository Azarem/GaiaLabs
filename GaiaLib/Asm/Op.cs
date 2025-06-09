using GaiaLib.Database;

namespace GaiaLib.Asm
{
    public class Op
    {
        public OpCode Code { get; set; }
        public int Location { get; set; }

        //public int Operand { get; set; }
        public object[] Operands { get; set; }
        public byte Size { get; set; }
        public CopDef CopDef { get; set; }

        //private Op _prev;
        //public Op Prev { get => _prev; set { if (_prev != value && (_prev = value) != null) value._next = this; } }

        //private Op _next;
        //public Op Next { get => _next; set { if (_next != value && (_next = value) != null) value._prev = this; } }
        //public Op Reference { get; set; }

        //internal IEnumerable<Location> References { get; set; }

        //public override string ToString()
        //{
        //    var fmt = GetFormat();
        //    var mnem = Code.Mnem;
        //    var op = Operands;
        //    return (op?.Length ?? 0) switch
        //    {
        //        1 => string.Format(fmt, mnem, op[0]),
        //        2 => string.Format(fmt, mnem, op[0], op[1]),
        //        3 => string.Format(fmt, mnem, op[0], op[1], op[2]),
        //        4 => string.Format(fmt, mnem, op[0], op[1], op[2], op[3]),
        //        5 => string.Format(fmt, mnem, op[0], op[1], op[2], op[3], op[4]),
        //        6 => string.Format(fmt, mnem, op[0], op[1], op[2], op[3], op[4], op[5]),
        //        7 => string.Format(fmt, mnem, op[0], op[1], op[2], op[3], op[4], op[5], op[6]),
        //        8 => string.Format(fmt, mnem, op[0], op[1], op[2], op[3], op[4], op[5], op[6], op[7]),
        //        _ => string.Format(fmt, mnem)
        //    };
        //}

        //public string GetFormat(DbRoot db = null)
        //{
        //    db ??= RomLoader.Current.DbRoot;
        //    if (!db.CopLib.Formats.TryGetValue(Code.Mode, out var str))
        //    {
        //        if (Code.Mnem == "COP")
        //        {
        //            if (!db.CopLib.Codes.TryGetValue((byte)Operands[0], out var cop))
        //                str = db.CopLib.Formats[AddressingMode.Immediate];
        //            //var cop = db.CopLib.Codes[(byte)Operands[0]];
        //            else
        //                str = cop.GetFormat();
        //        }
        //        else
        //            str = "{0}";
        //    }

        //    return str;
        //}
    }
}
