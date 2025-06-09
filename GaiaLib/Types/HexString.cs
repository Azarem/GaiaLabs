using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

namespace GaiaLib.Types
{
    [JsonConverter(typeof(HexStringConverter))]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HexString //:
                            //IComparable,
                            //IComparable<Location>,
                            //IComparable<uint>,
                            //IConvertible,
                            //IEquatable<Location>,
                            //IEquatable<uint>,
                            //IParsable<HexString>
    {
        public uint Value;
        public TypeCode TypeCode;

        //private string _str;
        //public string String { get => _str ??= string.Format(GetTypeFormat(), Value); }

        public HexString() { }
        public HexString(object value)
        {
            TypeCode = Type.GetTypeCode(value.GetType());
            Value = Convert.ToUInt32(value);
        }

        public HexString(string str)
        {
            Value = uint.Parse(str, NumberStyles.HexNumber);
            TypeCode = str.Length switch
            {
                1 or 2 => TypeCode.Byte,
                3 or 4 => TypeCode.UInt16,
                5 or 6 => TypeCode.UInt32,
                _ => throw new($"Unsupported hex {str}")
            };
        }

        public static implicit operator HexString(byte hex) => new(hex);
        public static implicit operator HexString(ushort hex) => new(hex);

        public static implicit operator uint(HexString hex) => hex.Value;
        public static implicit operator HexString(uint hex) => new(hex);

        public static implicit operator string(HexString hex) => hex.ToString();
        public static implicit operator HexString(string hex) => new(hex);
        public static bool operator ==(HexString left, HexString right)
            => left.Value == right.Value;
        public static bool operator !=(HexString left, HexString right) => !(left == right);

        public static bool operator ==(HexString left, uint right) => left.Value == right;
        public static bool operator !=(HexString left, uint right) => left.Value != right;
        public static bool operator ==(uint left, HexString right) => left == right.Value;
        public static bool operator !=(uint left, HexString right) => left != right.Value;

        public readonly string GetTypeFormat() => TypeCode switch
        {
            TypeCode.Byte or TypeCode.SByte => "{0:X2}",
            TypeCode.UInt16 or TypeCode.Int16 => "{0:X4}",
            TypeCode.UInt32 or TypeCode.Int32 => "{0:X6}",
            _ => throw new($"Unsupported type {TypeCode}")
        };

        public readonly object ToObject()
        {
            return Convert.ChangeType(Value, TypeCode);
        }

        //public static HexString Parse(string str) => str.Length switch
        //{
        //    1 or 2 => new(byte.Parse(str, NumberStyles.HexNumber)),
        //    3 or 4 => new(ushort.Parse(str, NumberStyles.HexNumber)),
        //    5 or 6 => new(uint.Parse(str, NumberStyles.HexNumber))
        //};
        //uint.Parse(str, NumberStyles.HexNumber, null);
        //public static bool TryParse(string str, out HexString result)
        //{
        //    if (uint.TryParse(str, NumberStyles.HexNumber, null, out uint res))
        //    { result = res; return true; }

        //    result = null;
        //    return false;
        //}

        public override readonly string ToString() => string.Format(GetTypeFormat(), Value);
        public override readonly int GetHashCode() => Value.GetHashCode() ^ TypeCode.GetHashCode();
        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is string str)
                return str == ToString();
            else if (obj is uint ui)
                return ui == Value;
            else if (obj is HexString hex)
                return this == hex;
            return false;
        }



        //public static HexString Parse(string s, IFormatProvider provider) => Parse(s);

        //public static bool TryParse([NotNullWhen(true)] string s, IFormatProvider provider, [MaybeNullWhen(false)] out HexString result)
        //    => TryParse(s, out result);
    }

    public class HexStringConverter : JsonConverter<HexString>
    {
        public HexStringConverter() { }

        public override HexString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetString();

        public override void Write(Utf8JsonWriter writer, HexString value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);

        public override HexString ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Read(ref reader, typeToConvert, options);

        public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] HexString value, JsonSerializerOptions options)
            => Write(writer, value, options);
    }
}
