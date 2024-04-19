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
    public struct HexString
    {
        public uint Value;

        public static implicit operator uint(HexString hex) => hex.Value;
        public static implicit operator HexString(uint hex) => new() { Value = hex };

        public static implicit operator string(HexString hex) => hex.ToString();
        public static implicit operator HexString(string hex) => Parse(hex);

        public static HexString Parse(string str) => uint.Parse(str, NumberStyles.HexNumber, null);

        public override readonly string ToString() => Value.ToString("x");
        public override readonly int GetHashCode() => Value.GetHashCode();
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is string str)
                return str == ToString();
            else if (obj is uint ui)
                return ui == Value;
            else if (obj is HexString hex)
                return hex.Value == Value;
            return false;
        }
    }

    public class HexStringConverter : JsonConverter<HexString>
    {
        public HexStringConverter() { }

        public override HexString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => HexString.Parse(reader.GetString());

        public override void Write(Utf8JsonWriter writer, HexString value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
}
