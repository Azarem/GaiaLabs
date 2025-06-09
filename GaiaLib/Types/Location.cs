using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GaiaLib.Types
{
    [JsonConverter(typeof(LocationConverter))]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct Location(uint offset)
        : IComparable,
            IComparable<Location>,
            IComparable<uint>,
            IConvertible,
            IEquatable<Location>,
            IEquatable<uint>,
            IParsable<Location>,
            IAdditionOperators<Location, uint, Location>,
            ISubtractionOperators<Location, uint, Location>,
            IComparisonOperators<Location, uint, bool>,
            IEqualityOperators<Location, uint, bool>,
            ISubtractionOperators<Location, Location, uint>,
            IComparisonOperators<Location, Location, bool>,
            IEqualityOperators<Location, Location, bool>,
            IMinMaxValue<Location>
    {
        public readonly uint Offset = offset & 0x3FFFFFu;

        //public readonly byte Size = size;

        public byte Bank => (byte)(Offset >> 16);

        public static Location MaxValue => 0x3FFFFFu;

        public static Location MinValue => 0;

        public static implicit operator Location(uint off) => new(off);

        public static implicit operator uint(Location loc) => loc.Offset;

        public static explicit operator Address(Location loc) => new(loc.Bank, (ushort)loc.Offset);

        public static explicit operator Location(Address add) =>
            ((uint)add.Bank & 0x3F) << 16 | add.Offset;

        public static Location operator +(Location loc, uint offset) => new(loc.Offset + offset);

        public static Location operator -(Location loc, uint offset) => new(loc.Offset - offset);

        public static Location operator &(Location near, uint far) => new(near.Offset & far);

        public static Location operator |(Location near, uint far) => new(near.Offset | far);

        public static bool operator >(Location near, uint far) => near.Offset > far;

        public static bool operator <(Location near, uint far) => near.Offset < far;

        public static bool operator >=(Location near, uint far) => near.Offset >= far;

        public static bool operator <=(Location near, uint far) => near.Offset <= far;

        public static bool operator ==(Location near, uint far) => near.Offset == far;

        public static bool operator !=(Location near, uint far) => near.Offset != far;

        public static uint operator -(Location near, Location far) => near.Offset - far.Offset;

        public static bool operator >(Location near, Location far) => near.Offset > far.Offset;

        public static bool operator <(Location near, Location far) => near.Offset < far.Offset;

        public static bool operator >=(Location near, Location far) => near.Offset >= far.Offset;

        public static bool operator <=(Location near, Location far) => near.Offset <= far.Offset;

        public static bool operator ==(Location near, Location far) => near.Offset == far.Offset;

        public static bool operator !=(Location near, Location far) => near.Offset != far.Offset;

        //public static Location operator &(Location near, Location far) => new(near.Offset & far.Offset);
        //public static Location operator |(Location near, Location far) => new(near.Offset | far.Offset);

        public override string ToString() => Offset.ToString("X6");

        public override bool Equals([NotNullWhen(true)] object obj) =>
            obj is Location loc ? Offset == loc.Offset : obj is uint ui && Offset == ui;

        public bool Equals(Location other) => Offset == other.Offset;

        public bool Equals(uint other) => Offset == other;

        public override int GetHashCode() => Offset.GetHashCode();

        public int CompareTo(Location other) => Offset.CompareTo(other.Offset);

        public int CompareTo(object obj) => Offset.CompareTo(obj);

        public int CompareTo(uint other) => Offset.CompareTo(other);

        public TypeCode GetTypeCode() => TypeCode.UInt32;

        public bool ToBoolean(IFormatProvider provider) => Offset > 0;

        public byte ToByte(IFormatProvider provider) => (byte)Offset;

        public char ToChar(IFormatProvider provider) => (char)Offset;

        public DateTime ToDateTime(IFormatProvider provider) => new(Offset);

        public decimal ToDecimal(IFormatProvider provider) => Offset;

        public double ToDouble(IFormatProvider provider) => Offset;

        public short ToInt16(IFormatProvider provider) => (short)Offset;

        public int ToInt32(IFormatProvider provider) => (int)Offset;

        public long ToInt64(IFormatProvider provider) => Offset;

        public sbyte ToSByte(IFormatProvider provider) => (sbyte)Offset;

        public float ToSingle(IFormatProvider provider) => Offset;

        public string ToString(IFormatProvider provider) => ToString();

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(uint))
                return Offset;
            if (conversionType == typeof(string))
                return ToString();
            return null;
        }

        public ushort ToUInt16(IFormatProvider provider) => (ushort)Offset;

        public uint ToUInt32(IFormatProvider provider) => Offset;

        public ulong ToUInt64(IFormatProvider provider) => Offset;

        public static Location Parse(string s, IFormatProvider provider = null) =>
            uint.Parse(s, NumberStyles.HexNumber, null);

        public static bool TryParse(
            [NotNullWhen(true)] string s,
            IFormatProvider provider,
            [MaybeNullWhen(false)] out Location result
        ) =>
            (result = uint.TryParse(s, NumberStyles.HexNumber, null, out uint loc) ? loc : 0u) > 0u;
    }

    public class LocationConverter : JsonConverter<Location>
    {
        public LocationConverter() { }

        public override Location Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) =>
            uint.TryParse(reader.GetString(), NumberStyles.HexNumber, null, out uint loc) ? loc : 0;

        public override void Write(
            Utf8JsonWriter writer,
            Location value,
            JsonSerializerOptions options
        ) => writer.WriteStringValue(value.ToString());

        public override Location ReadAsPropertyName(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => Read(ref reader, typeToConvert, options);

        public override void WriteAsPropertyName(
            Utf8JsonWriter writer,
            [DisallowNull] Location value,
            JsonSerializerOptions options
        ) => Write(writer, value, options);
    }
}
