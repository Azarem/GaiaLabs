


using GaiaLib.Compression;

foreach (var path in args)
{
    byte[] srcData;
    using (var file = File.OpenRead(path))
    {
        srcData = new byte[file.Length];
        file.Read(srcData);
    }

    var compact = Compact(srcData);
    using(var file = File.Create(path + ".compressed"))
        file.Write(compact);
}


static byte[] Compact(byte[] srcData)
{
    var lz = new QuintetLZ();
    return lz.Compact(srcData);
}