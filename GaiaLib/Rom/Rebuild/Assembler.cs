
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

    internal string _lineBuffer = "";
    public HashSet<string> includes = [];
    public List<AsmBlock> blocks = [];
    SortedDictionary<string, string?> tags = new(StringSizeComparer.Instance);
    internal AsmBlock? currentBlock = null;
    internal int lineCount = 0;
    internal int blockIndex = 0;
    internal int? lastDelimiter = null;
    internal byte? reqBank = null;
    internal bool eof = false;


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

    public (List<AsmBlock> blocks, HashSet<string> includes, byte? reqBank) ParseAssembly()
    {
        using var reader = new StreamReader(inStream);

        //Initialize root block (no label, location 0) and set as current
        blocks.Add(currentBlock = new ());

        //Initialize state machine and process text
        var state = new AssemblerState(this);
        state.ProcessText();

        return (blocks, includes, reqBank);
    }

    public bool GetLine()
    {
        //This can happen
        if (eof)
            return false;

        //Keep processing what we already have
        if (_lineBuffer.Length > 0)
            return true;

        Read:
        var rawLine = reader.ReadLine();

        //This can happen
        if (rawLine == null)
            if (_lineBuffer.Length == 0)
            {
                eof = true;
                return false;
            }
            else
                rawLine = "";
        else
            _lineBuffer += rawLine;

        lineCount++;

        //Ignore comments
        TrimComments("--");
        TrimComments(";");
        TrimComments("//");

        //Trim
        _lineBuffer = _lineBuffer.Trim(RomProcessingConstants.CommaSpace);

        //This can happen
        if (_lineBuffer.Length == 0)
            goto Read;

        //Process hard line continuations
        if (_lineBuffer.EndsWith('\\'))
        {
            _lineBuffer = _lineBuffer[..^1];
            goto Read;
        }

        //Process directives
        if (_lineBuffer[0] == '?')
        {
            ProcessDirectives();
            _lineBuffer = "";
            goto Read;
        }

        //Process tags
        if (_lineBuffer[0] == '!')
        {
            ProcessTags();
            _lineBuffer = "";
            goto Read;
        }

        ResolveTags();

        return true;
    }

    private void TrimComments(string sequence)
    {
        //Look for the first instance of the comment sequence
        var index = _lineBuffer.IndexOf(sequence);
        if (index >= 0)
        {
            //Make sure it's not inside a string
            var strIndex = _lineBuffer.IndexOfAny(_root.StringDelimiters);
            //This works with line continuations because the string ending will not be on this line yet
            //If no string, string starts after comment, or last delimiter is before comment, trim it
            if (strIndex < 0 || strIndex > index || _lineBuffer.LastIndexOf(_lineBuffer[strIndex]) < index)
                _lineBuffer = _lineBuffer[..index];
        }
    }


    private void ProcessDirectives()
    {
        //Find end of directive
        var endIx = _lineBuffer.IndexOfAny(RomProcessingConstants.CommaSpace);

        //Default to line length if no end characters
        if (endIx < 0)
            endIx = _lineBuffer.Length;

        var value = _lineBuffer[endIx..].TrimStart(RomProcessingConstants.CommaSpace);

        switch (_lineBuffer[1..endIx].ToUpper())
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
        _lineBuffer = _lineBuffer[1..].TrimStart(RomProcessingConstants.CommaSpace);

        //Process as many pairs as we can
        while (_lineBuffer.Length > 0)
        {
            //Default name and value
            string name = _lineBuffer;
            string? value = null;

            //Find first index of comma/space/tab
            var endIx = _lineBuffer.IndexOfAny(RomProcessingConstants.CommaSpace);

            if (endIx >= 0)
            {
                //Split into name and value
                name = _lineBuffer[..endIx];
                value = _lineBuffer[(endIx + 1)..].TrimStart(RomProcessingConstants.CommaSpace);

                //Find next index of comma/space/tab inside value
                var nextIx = value.IndexOfAny(RomProcessingConstants.CommaSpace);
                if (nextIx >= 0)
                {
                    //Line buffer then take the remainder
                    _lineBuffer = value[(nextIx + 1)..].TrimStart(RomProcessingConstants.CommaSpace);
                    //Value takes up to next index
                    value = value[..nextIx];
                }
                else
                    _lineBuffer = ""; //No more pairs, clear buffer
            }
            else
                _lineBuffer = ""; //Take all as name, clear buffer

            //Assign name/value pair to tags
            tags[name] = value;
        }
    }

    private void ResolveTags()
    {
        int ix;
        //Tags are sorted by length descending so longer tags are replaced first (avoids minor conflicts)
        foreach (var tag in tags)
            while ((ix = _lineBuffer.IndexOf(tag.Key, StringComparison.CurrentCultureIgnoreCase)) >= 0)
                _lineBuffer = _lineBuffer[..ix] + tag.Value + _lineBuffer[(ix + tag.Key.Length)..];
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
        if (_lineBuffer[0] == '#')
            _lineBuffer = _lineBuffer[1..];

        //Reverse binary marker
        if (_lineBuffer[0] == '$')
        {
            reverse = true;
            _lineBuffer = _lineBuffer[1..];
        }

        string hex;
        var symbolIx = _lineBuffer.IndexOfAny(RomProcessingConstants.SymbolSpace);
        if (symbolIx >= 0)
        {
            hex = _lineBuffer[..symbolIx];
            _lineBuffer = _lineBuffer[symbolIx..].TrimStart(RomProcessingConstants.CommaSpace);
        }
        else
        {
            hex = _lineBuffer;
            _lineBuffer = "";
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

                    //Explicitly change two-byte data into ushort for easier processing later
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
