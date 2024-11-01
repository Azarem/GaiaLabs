using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaiaPacker
{
    internal class ProjectRoot
    {
        public string Name { get; set; }
        public string RomPath { get; set; }
        public string BaseDir { get; set; }
        public string Database { get; set; }
        public IEnumerable<ProjectFile> Files { get; set; }
    }

    internal class ProjectFile
    {
        public string Path { get; set; }
        public string Type { get; set; }
        public IEnumerable<string> Refs { get; set; }
        public int Size;
    }
}
