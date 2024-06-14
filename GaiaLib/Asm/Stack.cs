
namespace GaiaLib.Asm
{
    public class Stack
    {
        public byte[] Bytes = new byte[60];
        public int Location = 0;

        public void Push(byte b)
            => Bytes[Location++] = b;

        public void Push(ushort b)
        {
            Bytes[Location++] = (byte)b;
            Bytes[Location++] = (byte)(b >> 8);
        }

        public byte PopByte()
            => Bytes[Location--];

        public ushort PopUInt16()
            => (ushort)(Bytes[Location--] << 8 | Bytes[Location--]);

    }
}
