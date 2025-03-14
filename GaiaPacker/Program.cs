using GaiaLib;
using GaiaLib.Rom;

string? path = null;
var isUnpack = false;
foreach (var a in args)
{
    if (a.StartsWith("--"))
    {
        var b = a[2..].ToLower();
        switch (b)
        {
            case "unpack":
                isUnpack = true;
                break;
        }
    }
    else
        path = a;
}

var project = ProjectRoot.Load(path);

if (isUnpack)
    using (var reader = new RomReader(project.RomPath, project.DatabasePath))
        reader.DumpDatabase(project.BaseDir);
else
    project.Build();
