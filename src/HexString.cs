using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

namespace GaiaLabs
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

        public static implicit operator uint(HexString hex) => hex.Value;
        public static implicit operator HexString(uint hex) => new() { Value = hex };

        public static implicit operator string(HexString hex) => hex.ToString();
        public static implicit operator HexString(string hex) => Parse(hex);

        public static HexString Parse(string str) => uint.Parse(str, NumberStyles.HexNumber, null);
        //public static bool TryParse(string str, out HexString result)
        //{
        //    if (uint.TryParse(str, NumberStyles.HexNumber, null, out uint res))
        //    { result = res; return true; }

        //    result = null;
        //    return false;
        //}

        public override readonly string ToString() => Value.ToString("x");
        public override readonly int GetHashCode() => Value.GetHashCode();
        public override readonly bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is string str)
                return str == ToString();
            else if (obj is uint ui)
                return ui == Value;
            else if (obj is HexString hex)
                return hex.Value == Value;
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
            => reader.GetString();

        public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] HexString value, JsonSerializerOptions options)
            => writer.WritePropertyName(value);
    }
}
