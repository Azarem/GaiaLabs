using GaiaLabs.src;
using GaiaLib;
using GaiaLib.Rom;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GaiaLabs
{
    public class ArrayBlock<T> : DataBlock where T : struct
    {
        public int Length;

        public IEnumerable<T> Items { get; set; }

        public ArrayBlock() : base() { }

        public ArrayBlock(Address address, string label, int length = 0, params DataReference[] references)
            : base(address, label)
        {
            Length = length;
            References = references;
        }

        public override bool Unpack(RomLoader loader)
        {
            var ptr = loader._basePtr + (int)Location.Offset;
            var width = Marshal.SizeOf<T>();
            var list = new List<T>();
            var len = Length;

            for(int i = 0; len == 0 || i < len; i++)
            {
                if (Marshal.ReadByte(ptr) == 0xFF)
                {
                    Length = i;
                    break;
                }

                list.Add(Marshal.PtrToStructure<T>(ptr));
                ptr += width;
            }

            Items = list;

            return base.Unpack(loader);
        }
    }
}
