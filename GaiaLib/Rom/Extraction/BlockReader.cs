using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Rom.Extraction;
using GaiaLib.Types;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GaiaLib.Rom;

internal partial class BlockReader
{
    #region Constants
    private const int RefSearchMaxRange = 0x1A0;
    private const int BankMaskCheck = 0x40;
    private const int ByteDelimiterThreshold = 0x100;
    private const byte BankHighMemory1 = 0x7E;
    private const byte BankHighMemory2 = 0x7F;
    private static readonly char[] _pointerCharacters = ['&', '@'];
    #endregion

    #region Core Dependencies
    internal readonly DbRoot _root;
    internal readonly Extraction.StringReader _stringReader;
    internal readonly AsmReader _asmReader;
    internal readonly TypeParser _typeParser;
    internal readonly RomDataReader _romDataReader;
    internal readonly ProcessorStateManager _stateManager;
    internal readonly ReferenceManager _referenceManager;
    #endregion

    #region Backward Compatibility Properties
    //internal byte[] RomData => _romDataReader.RomData;
    //internal int _romPosition
    //{
    //    get => _romDataReader.Position;
    //    set => _romDataReader.Position = value;
    //}

    // Backward compatibility for state tracking
    internal Dictionary<int, bool?> AccumulatorFlags => _stateManager._accumulatorFlags;
    internal Dictionary<int, bool?> IndexFlags => _stateManager._indexFlags;
    internal Dictionary<int, byte?> BankNotes => _stateManager._bankNotes;
    internal Dictionary<int, byte> StackPosition => _stateManager._stackPositions;

    // Direct access to reference management collections
    internal Dictionary<int, string> _structTable => _referenceManager._structTable;
    internal Dictionary<int, int> _markerTable => _referenceManager._markerTable;
    internal Dictionary<int, string> _nameTable => _referenceManager._nameTable;
    #endregion

    #region Current Processing State
    internal DbBlock _currentBlock;
    internal DbPart? _currentPart;
    internal int _partEnd = 0;
    #endregion

    #region Constructor and Initialization
    public BlockReader(byte[] romData, DbRoot root)
    {
        _romDataReader = new(romData);
        _stateManager = new();
        _referenceManager = new(root);
        _root = root ?? throw new ArgumentNullException(nameof(root));

        _stringReader = new(this);
        _asmReader = new(this);
        _typeParser = new(this);

        InitializeOverrides();
        InitializeFileReferences();
    }

    /// <summary>
    /// Processes predefined overrides for registers and bank notes
    /// </summary>
    private void InitializeOverrides()
    {
        foreach (var over in _root.Overrides.Values)
        {
            switch (over.Register)
            {
                case RegisterType.M:
                    _stateManager.SetAccumulatorFlag(over.Location, over.Value != 0u);
                    break;
                case RegisterType.X:
                    _stateManager.SetIndexFlag(over.Location, over.Value != 0u);
                    break;
                case RegisterType.B:
                    _stateManager.SetBankNote(over.Location, (byte)over.Value.Value);
                    break;
            }
        }
    }

    /// <summary>
    /// Processes predefined file references
    /// </summary>
    private void InitializeFileReferences()
    {
        foreach (var file in _root.Files)
            _referenceManager.TryAddName(file.Start, file.Name);
    }
    #endregion

    #region Generated Regex
    [GeneratedRegex("_([A-Fa-f0-9]{6})")]
    public static partial Regex LocationRegex();
    #endregion

    #region Data Reading Methods (Delegated to RomDataReader)
    //internal byte ReadByte() => _romDataReader.ReadByte();
    //internal sbyte ReadSByte() => _romDataReader.ReadSByte();
    //internal ushort ReadUShort() => _romDataReader.ReadUShort();
    //internal short ReadShort() => _romDataReader.ReadShort();
    //internal int ReadAddress() => _romDataReader.ReadAddress();
    //internal int ReadInt() => _romDataReader.ReadInt();
    //internal int PeekByte() => _romDataReader.PeekByte();
    //internal int PeekShort() => _romDataReader.PeekShort();
    //internal int PeekAddress() => _romDataReader.PeekAddress();
    #endregion

    #region Mnemonic Resolution
    internal void ResolveMnemonic(Address addr)
    {
        //If the address is in high memory (ROM only), skip processing
        if ((addr.Bank & Address.DataBankFlag) != 0)
            return;

        int offset = addr.Offset;

        if (!_root.Mnemonics.TryGetValue(offset, out var label))
            return;

        var ix = label.IndexOfAny(RomProcessingConstants.Operators);

        if (ix >= 0)
        {
            var opnd = int.Parse(label[(ix + 1)..], NumberStyles.HexNumber);

            var op = label[ix];
            if (op == '-')
                opnd = -opnd;

            offset -= opnd;

            label = label[..ix];
        }

        _currentBlock.Mnemonics[offset] = label;
    }
    #endregion

    #region Name Resolution (Delegated to ReferenceManager)
    internal string ResolveName(int location, AddressType type, bool isBranch)
    {
        return _referenceManager.ResolveName(location, type, isBranch);
    }
    #endregion

    #region Include Resolution
    internal void ResolveInclude(int loc, bool isBranch)
    {
        if (_currentBlock.IsOutside(loc, out var p) && p != null)
            _currentPart.Includes.Add(p);
        else if (isBranch && !_referenceManager.TryGetName(loc, out _))
            _referenceManager.TryAddName(loc, string.Format(RomProcessingConstants.BlockReader.LocationFormat, loc));
    }
    #endregion

    #region Type and Chunk Management
    internal string NoteType(int loc, string type, bool silent = false, Registers? reg = null)
    {
        _referenceManager.TryAddStruct(loc, type);

        if (!_referenceManager.TryGetName(loc, out var name))
        {
            name = _referenceManager.CreateTypeName(type, loc);
            _referenceManager.TryAddName(loc, name);
        }

        if (!silent && type == RomProcessingConstants.BlockReader.CodeType && reg != null)
        {
            UpdateRegisterState(loc, reg);
        }

        return name;
    }

    private void UpdateRegisterState(int loc, Registers reg)
    {
        if (reg.AccumulatorFlag != null)
            _stateManager.TryAddAccumulatorFlag(loc, reg.AccumulatorFlag);
        if (reg.IndexFlag != null)
            _stateManager.TryAddIndexFlag(loc, reg.IndexFlag);
        if (reg.Stack.Location > 0)
            _stateManager.TryAddStackPosition(loc, (byte)reg.Stack.Location);
    }

    internal bool DelimiterReached(int? delimiter)
    {
        if (delimiter == null)
            return false;

        if (delimiter >= RomProcessingConstants.BlockReader.ByteDelimiterThreshold)
        {
            if (_romDataReader.PeekShort() == delimiter)
            {
                _romDataReader.Position += 2;
                return true;
            }
        }
        else if (_romDataReader.PeekByte() == delimiter)
        {
            _romDataReader.Position++;
            return true;
        }

        return false;
    }

    internal bool PartCanContinue()
        => _romDataReader.Position < _partEnd && !_referenceManager.ContainsStruct(_romDataReader.Position);
    #endregion

    #region Main Analysis Methods
    public void AnalyzeAndResolve()
    {
        AnalyzeBlocks();
        ResolveReferences();
    }

    private void AnalyzeBlocks()
    {
        InitializeBlocksAndParts();

        foreach (var block in _root.Blocks)
        {
            _currentBlock = block;
            foreach (var part in block.Parts)
            {
                ProcessPart(part);
            }
        }
    }

    private void InitializeBlocksAndParts()
    {
        foreach (var block in _root.Blocks)
            foreach (var part in block.Parts)
            {
                part.Includes = new();
                _referenceManager.TryAddStruct(part.Start, part.Struct);
                _referenceManager.TryAddName(part.Start, part.Name);
            }
    }

    private void ProcessPart(DbPart part)
    {
        _currentPart = part;
        _romDataReader.Position = part.Start;
        _partEnd = part.End;

        var current = part.Struct ?? RomProcessingConstants.BlockReader.BinaryType;
        var chunks = new List<TableEntry>();
        var reg = new Registers();
        byte? bank = part.Bank != null ? (byte)part.Bank.Value.Value : null;
        TableEntry? last = null;

        while (_romDataReader.Position < _partEnd)
        {
            if (_referenceManager.TryGetStruct(_romDataReader.Position, out var value))
            {
                current = value!;
            }
            else if (last != null)
            {
                ProcessContinuousEntry(current, reg, bank, last);
                continue;
            }

            chunks.Add(last = new(_romDataReader.Position));
            ProcessNewEntry(current, reg, bank, last);
        }

        part.ObjectRoot = chunks;
    }

    private void ProcessContinuousEntry(string current, Registers reg, byte? bank, TableEntry last)
    {
        var obj = _typeParser.ParseType(current, reg, 0, bank);
        if (last.Object is not List<object> list)
            last.Object = list = [last.Object];
        list.Add(obj);
    }

    private void ProcessNewEntry(string current, Registers reg, byte? bank, TableEntry last)
    {
        var res = _typeParser.ParseType(current, reg, 0, bank);
        if (RomProcessingConstants.BlockReader.PointerCharacters.Contains(current[0]) && res is not List<object>)
            res = new List<object>() { res };

        last.Object = res;
    }

    private void ResolveReferences()
    {
        foreach (var block in _root.Blocks)
        {
            _currentBlock = block;
            foreach (var part in block.Parts)
            {
                _currentPart = part;
                ResolveObject(part.ObjectRoot, false);
            }
        }
    }

    private void ResolveObject(object obj, bool isBranch)
    {
        switch (obj)
        {
            case string:
                // String objects don't need resolution
                break;
            case IEnumerable arr:
                foreach (var o in arr)
                    ResolveObject(o, isBranch);
                break;
            case Address addr:
                ResolveMnemonic(addr);
                break;
            case int loc:
                ResolveInclude(loc, isBranch);
                break;
            case LocationWrapper lw:
                ResolveInclude(lw.Location, isBranch);
                break;
            case StringWrapper sw:
                _stringReader.ResolveString(sw, isBranch);
                break;
            case StructDef sdef:
                ResolveObject(sdef.Parts, isBranch);
                break;
            case TableEntry tab:
                ResolveObject(tab.Object, isBranch);
                break;
            case Op op:
                ResolveOperationObject(op);
                break;
        }
    }

    private void ResolveOperationObject(Op op)
    {
        var branch = IsBranchOperation(op);
        foreach (var opnd in op.Operands)
            ResolveObject(opnd, branch);
    }

    private static bool IsBranchOperation(Op op)
    {
        return op.Code.Mode == AddressingMode.PCRelative
            || op.Code.Mode == AddressingMode.PCRelativeLong
            || op.Code.Mnem[0] == 'J';
    }
    #endregion

    public void HydrateRegisters(Registers reg) => _stateManager.HydrateRegisters(_romDataReader.Position, reg);
}

