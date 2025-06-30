using GaiaLib.Asm;
using GaiaLib.Types;

namespace GaiaLib.Types;

public static class RomProcessingConstants
{
    // Core constants
    public const int PageSize = 0x8000;
    public const int SNESHeaderSize = 0x50;

    // Dictionary constants
    public static readonly string[] Dictionaries = ["dictionary_01EBA8", "dictionary_01F54D"];
    public static readonly byte[] DictCommands = [0xD6, 0xD7];
    public static readonly byte[] EndChars = [0xC0, 0xCA, 0xD1];

    // Character arrays for parsing
    public static readonly char[] Whitespace = [' ', '\t'];
    public static readonly char[] Operators = ['-', '+'];
    public static readonly char[] CommaSpace = [',', ' ', '\t'];
    public static readonly char[] AddressSpace = ['@', '&', '^', '#', '$', '%', '*'];
    public static readonly char[] SymbolSpace = [',', ' ', '\t', '<', '>', '(', ')', ':', '[', ']', '{', '}', '`', '~', '|'];
    public static readonly char[] LabelSpace = ['[', '{', '#', '`', '~', '|', ':'];
    public static readonly char[] ObjectSpace = ['<', '['];
    public static readonly char[] CopSplitChars = [' ', '\t', ',', '(', ')', '[', ']', '$', '#'];
    //public static readonly char[] StringSpace = ['~', '`', '|'];

    // BlockReader specific constants
    public static class BlockReader
    {
        public const int RefSearchMaxRange = 0x1A0;
        public const int BankMaskCheck = 0x40;
        public const int ByteDelimiterThreshold = 0x100;
        public const byte BankHighMemory1 = 0x7E;
        public const byte BankHighMemory2 = 0x7F;
        public static readonly char[] PointerCharacters = ['&', '@'];
        public const string WideStringType = "WideString";
        public const string BinaryType = "Binary";
        public const string CodeType = "Code";
        
        // Format strings
        public const string LocationFormat = "loc_{0:X6}";
        public const string TypeNameFormat = "{0}_{1:X6}";
        public const string OffsetFormat = "+{0:X}";
        public const string MarkerFormat = "+M";
        public const string NegativeOffsetFormat = "-{0:X}";
        public const string NegativeMarkerFormat = "-M";
    }

    /// <summary>
    /// Gets the size of an object for processing purposes
    /// </summary>
    /// <param name="obj">The object to get the size for</param>
    /// <returns>Size in bytes</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to determine size</exception>
    public static int GetSize(object obj)
    {
        if (obj is Op op)
            return op.Size;
        else if (obj is byte[] arr)
            return arr.Length;
        //else if (obj is StringEntry se)
        //    return se.Data.Length;
        else if (obj is byte)
            return 1;
        else if (obj is ushort)
            return 2;
        else if (obj is uint)
            return 3;
        else if (obj is string str)
        {
            if (str.Length > 0)
            {
                switch (str[0])
                {
                    case '@':
                        return 3;
                    case '*':
                        return 2;
                    case '&':
                        return 2;
                    case '^':
                        return 1;
                }

                switch (str)
                {
                    case "Byte":
                        return 1;
                    case "Word":
                        return 2;
                    case "Offset":
                        return 2;
                    case "Address":
                        return 3;
                }
            }
        }

        throw new InvalidOperationException($"Unable to get size for operand '{obj}'");
    }
} 