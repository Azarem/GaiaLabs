using Godot;
using System;
using System.Runtime.InteropServices;

namespace GaiaLabs
{
    public class ImageConverter
    {
        private const float _sample4to5 = 31.3f / 15f;
        private const float _sample4to6 = 63.3f / 15f;
        private const float _sample4to8 = 255.3f / 15f;


        public static unsafe Image ReadImage(nint data, int width, int height, byte bpp, nint palette, bool compressed = false)
        {
            int sectionCount = (bpp + 1) >> 1;
            int indexCount = bpp << 1;
            int tileStride = bpp << 3;
            int tilesX = (width + 7) >> 3;
            int tilesY = (height + 7) >> 3;
            int stride = width << 2;
            int rowStride = 8;

            byte*[] ptrBuffer = new byte*[sectionCount];
            byte[] indexBuffer = new byte[indexCount];
            byte[] buffer = new byte[stride * height];

            fixed (byte* pBuf = buffer)
            {
                byte* src = (byte*)data;

                switch (bpp)
                {
                    case 1: break;
                    case 2: break;

                    case 4:

                        for (int ty = 0; ty < tilesY; ty++)
                            for (int tx = 0; tx < tilesX; tx++)
                            {
                                //Fill ptr buffer with section offsets
                                for (int s = 0; s < sectionCount; s++)
                                    ptrBuffer[s] = src + (s << 4);

                                //Advance to next tile
                                src += tileStride;

                                //Read the data
                                for (int row = 0; row < rowStride; row++)
                                {
                                    //Clear index buffer
                                    Array.Clear(indexBuffer);

                                    //Rotate bits from samples
                                    for (byte plane = 0, planeBit = 1; plane < bpp; plane++, planeBit <<= 1)
                                        for (byte i = 0, testBit = 0x80, sample = *ptrBuffer[plane >> 1]++; i < indexCount; i++, testBit >>= 1)
                                            if ((sample & testBit) != 0)
                                                indexBuffer[i] |= planeBit;

                                    //Output data
                                    //var swap = height >= 256 && (ty & 1) == 1;
                                    RGBA* dst = (RGBA*)(pBuf + (((ty * rowStride) + row) * stride) + (tx * 32));
                                    for (byte i = 0, sample; i < indexCount; i++)
                                        if ((sample = indexBuffer[i]) == 0) //Transparent pixel
                                            *dst++ = RGBA.Transparent;
                                        else
                                            *dst++ = new((byte)Mathf.RoundToInt(sample * _sample4to8));
                                }
                            }
                        break;

                    case 8: break;
                }
            }

            return Image.CreateFromData(width, height, false, Image.Format.Rgba8, buffer);
        }
        public static unsafe Image ReadImage(byte[] data, int width, int height, byte bpp, nint palette, bool compressed = false)
        {
            fixed (byte* ptr = data)
                return ReadImage((nint)ptr, width, height, bpp, palette, compressed);
        }

    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RGBA
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public RGBA() { R = 0; G = 0; B = 0; A = 0; }
        public RGBA(byte w, byte a = 255)
        { R = w; G = w; B = w; A = a; }
        public RGBA(byte r, byte g, byte b, byte a = 255)
        { R = r; G = g; B = b; A = a; }

        public static readonly RGBA Transparent = new();
    }
}
