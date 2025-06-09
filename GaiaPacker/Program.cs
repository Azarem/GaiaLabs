using GaiaLib;

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
    await project.DumpDatabase();
else
    project.Build();
