using System;
using System.Collections.Generic;

namespace GaiaLabs
{
    public class DBType : Dictionary<string, Type>
    {
    }

    public class DBProperty 
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }
}
