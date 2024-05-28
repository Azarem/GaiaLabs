
using System;

namespace GaiaLabs
{
    public unsafe class CodeReference : DataReference
    {
        public CodeReference(Address address) : base(address)
        {
            Offset = 1;
        }
    }
}
