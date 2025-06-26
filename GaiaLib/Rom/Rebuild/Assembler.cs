
using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Types;
using System.Globalization;

namespace GaiaLib.Rom.Rebuild;

public class Assembler : IDisposable
{
    public readonly DbRoot _root;
    private readonly FileStream inStream;
    private readonly StreamReader reader;
    internal readonly StringProcessor _stringProcessor;

    internal string? line = "";
    public HashSet<string> includes = new();
    SortedDictionary<string, string?> tags = new(StringSizeComparer.Instance);
    public List<AsmBlock> blocks = [];
    internal AsmBlock currentBlock;
    internal int lineCount;
    internal int blockIndex;
    internal int? lastDelimiter;
    internal byte? reqBank;


    public Assembler(DbRoot dbRoot, string filePath)
    {
        _root = dbRoot;
        inStream = File.OpenRead(filePath);
        reader = new StreamReader(inStream);
        _stringProcessor = new StringProcessor(this);
    }

    public void Dispose()
    {
        reader.Dispose();
        inStream.Dispose();
        _stringProcessor.Dispose();
        GC.SuppressFinalize(this);
    }

    public (List<AsmBlock> blocks, HashSet<string> includes, byte? reqBank) ParseAssembly(int startLoc)
    {
        using var reader = new StreamReader(inStream);

        includes = new HashSet<string>();
        blocks = new List<AsmBlock>();
        currentBlock = null;
        //AsmBlock? target;
        tags = new SortedDictionary<string, string?>(StringSizeComparer.Instance);
        //memStream.Position = 0;

        blockIndex = 0;
        lineCount = 0;

        reqBank = null;
        //string? lastStruct = null;
        lastDelimiter = null;

        blocks.Add(currentBlock = new AsmBlock { Location = startLoc });

        var state = new AssemblerState(this);
        state.ProcessText();

        return (blocks, includes, reqBank);
    }

    public string? GetLine()
    {
        //This can happen
        if (line == null)
            return null;

        //Keep processing what we already have
        if (line.Length > 0)
            return line;

        Read:
        line = reader.ReadLine();

        //This can happen
        if (line == null)
            return null;

        Clean:
        lineCount++;

        //Ignore comments
        TrimComments("--");
        TrimComments(";");
        TrimComments("//");

        //Trim
        line = line.Trim(RomProcessingConstants.CommaSpace);

        //This can happen
        if (line.Length == 0)
            goto Read;

        if (line.EndsWith('\\'))
        {
            line = line[..^1] + (reader.ReadLine() ?? "");
            goto Clean;
        }

        //Process directives
        if (line[0] == '?')
        {
            ProcessDirectives();
            goto Read;
        }

        //Process tags
        if (line[0] == '!')
        {
            ProcessTags();
            goto Read;
        }

        ResolveTags();

        return line;
    }

    private void TrimComments(string sequence)
    {
        var index = line.IndexOf(sequence);
        if (index >= 0)
        {
            var cix = line.IndexOfAny(_root.StringDelimiters);
            if (cix < 0 || cix > index || (cix = line.LastIndexOf(line[cix])) < index)
                line = line[..index];
        }
    }


    private void ProcessDirectives()
    {
        //Find end of directive
        var endIx = line.IndexOfAny(RomProcessingConstants.CommaSpace);

        //Default to line length if no end characters
        if (endIx < 0)
            endIx = line.Length;

        var value = line[endIx..].TrimStart(RomProcessingConstants.CommaSpace);

        switch (line[1..endIx].ToUpper())
        {
            //This is taken care of by the loader
            case "BANK":
                reqBank = byte.Parse(value, NumberStyles.HexNumber);
                break;

            case "INCLUDE":
                if (value.Length > 0)
                    includes.Add(value.ToUpper().Replace("'", ""));
                break;
        }
    }

    private void ProcessTags()
    {
        //Remove the '!' prefix and trim leading spaces
        line = line[1..].TrimStart(RomProcessingConstants.CommaSpace);

        //Process as many pairs as we can
        while (line.Length > 0)
        {
            string name = line;
            string? value = null;
            var endIx = line.IndexOfAny(RomProcessingConstants.CommaSpace);

            if (endIx >= 0)
            {
                name = line[..endIx];
                value = line[(endIx + 1)..].TrimStart(RomProcessingConstants.CommaSpace);
                if ((endIx = value.IndexOfAny(RomProcessingConstants.CommaSpace)) >= 0)
                {
                    line = value[(endIx + 1)..].TrimStart(RomProcessingConstants.CommaSpace);
                    value = value[..endIx];
                }
                else
                    line = "";
            }
            else
                line = "";

            tags[name] = value;
        }
    }

    private void ResolveTags()
    {
        int ix;
        foreach (var tag in tags)
            while ((ix = line.IndexOf(tag.Key, StringComparison.CurrentCultureIgnoreCase)) >= 0)
                line = line[..ix] + tag.Value + line[(ix + tag.Key.Length)..];
    }




    public object ParseOperand(string opnd) =>
        opnd.Length switch
        {
            2 when byte.TryParse(opnd, NumberStyles.HexNumber, null, out var b) => b,
            4 when ushort.TryParse(opnd, NumberStyles.HexNumber, null, out var s) => s,
            6 when uint.TryParse(opnd, NumberStyles.HexNumber, null, out var i) => i,
            _ => opnd,
        };

    public void ProcessRawData()
    {
        bool reverse = false;

        //Immediate binary marker
        if (line[0] == '#')
            line = line[1..];

        //Reverse binary marker
        if (line[0] == '$')
        {
            reverse = true;
            line = line[1..];
        }

        string hex;
        var symbolIx = line.IndexOfAny(RomProcessingConstants.SymbolSpace);
        if (symbolIx >= 0)
        {
            hex = line[..symbolIx];
            line = line[symbolIx..].TrimStart(RomProcessingConstants.CommaSpace);
        }
        else
        {
            hex = line;
            line = "";
        }

        if (hex.Length > 0)
        {
            //Keep string values of address markers so they can be resolved later
            if (RomProcessingConstants.AddressSpace.Contains(hex[0]))
            {
                currentBlock.ObjList.Add(hex);
                currentBlock.Size += RomProcessingConstants.GetSize(hex);
            }
            else
            {
                var data = Convert.FromHexString(hex);
                if (data.Length == 1)
                {
                    currentBlock.ObjList.Add(data[0]);
                    currentBlock.Size++;
                }
                else
                {
                    if (reverse)
                        for (int x = 0, y = data.Length; --y > x;)
                        {
                            var sample = data[x];
                            data[x++] = data[y];
                            data[y] = sample;
                        }

                    if (data.Length == 2)
                    {
                        currentBlock.ObjList.Add((ushort)(data[0] | data[1] << 8));
                        currentBlock.Size += 2;
                    }
                    else
                    {
                        currentBlock.ObjList.Add(data);
                        currentBlock.Size += data.Length;
                    }
                }
            }
        }
    }


}
