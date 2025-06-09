using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaiaLib.Rom.Rebuild
{
    public class AssemblerState
    {
        private readonly DbStruct? dbs;
        private readonly DbStruct? parent;
        private readonly DbRoot _root;
        private readonly int? discriminator;
        private int? delimiter;
        private int memberOffset;
        private int dataOffset;
        private readonly string[]? memberTypes;
        private string? currentType;
        private readonly Assembler assembler;
        private int debugLine;

        public AssemblerState(Assembler assembler, string? structType = null, bool saveDelimiter = false)
        {
            this.assembler = assembler;
            _root = assembler._root;

            dbs = structType == null ? null
                : _root.Structs.Values.FirstOrDefault(x =>
                    x.Name.Equals(structType, StringComparison.CurrentCultureIgnoreCase)
                );

            parent = dbs?.Parent == null ? null
                : _root.Structs.Values.FirstOrDefault(x =>
                    x.Name.Equals(dbs.Parent, StringComparison.CurrentCultureIgnoreCase)
                );

            discriminator = parent?.Descriminator;
            delimiter = dbs?.Delimiter;
            memberOffset = 0;
            dataOffset = 0;
            memberTypes = dbs?.Types;
            currentType = memberTypes?[memberOffset];

            if (saveDelimiter)
                assembler.lastDelimiter = dbs?.Delimiter ?? parent?.Delimiter;
        }



        private void CheckDesc()
        {
            if (discriminator == dataOffset)
            {
                //Discriminator is always 1 byte
                assembler.currentBlock.ObjList.Add((byte)dbs.Descriminator.Value);
                assembler.currentBlock.Size += 1;
                dataOffset += 1;
            }
        }

        private void AdvancePart()
        {
            if (currentType != null && discriminator != null)
                dataOffset += RomProcessingConstants.GetSize(currentType);

            if (memberTypes != null && memberOffset + 1 < memberTypes.Length)
                currentType = memberTypes[++memberOffset];
        }

        private void ProcessOrigin()
        {
            assembler.line = assembler.line[3..].TrimStart(RomProcessingConstants.CommaSpace);
            if (assembler.line.StartsWith('$'))
                assembler.line = assembler.line[1..];

            string hex;
            var endIx = assembler.line.IndexOfAny(RomProcessingConstants.CommaSpace);
            if (endIx >= 0)
            {
                hex = assembler.line[..endIx];
                assembler.line = assembler.line[(endIx + 1)..].TrimStart(RomProcessingConstants.CommaSpace);
            }
            else
            {
                hex = assembler.line;
                assembler.line = "";
            }

            var location = int.Parse(hex, NumberStyles.HexNumber);

            assembler.blocks.Add(assembler.currentBlock = new AsmBlock { Location = location });
            assembler.blockIndex++;
        }

        private string DoMaths(string operand)
        {
            var ix = operand.IndexOfAny(RomProcessingConstants.Operators);
            if (ix >= 0)
            {
                var op = operand[ix];

                int vix = operand.LastIndexOf('$', ix) + 1;

                if (uint.TryParse(operand[vix..ix], NumberStyles.HexNumber, null, out var value))
                {
                    var endIx = operand.IndexOfAny(RomProcessingConstants.SymbolSpace, ix + 1);

                    if (endIx < 0)
                        endIx = operand.Length;

                    var number = uint.Parse(operand[(ix + 1)..endIx], NumberStyles.HexNumber);

                    if (op == '-')
                        value -= number;
                    else
                        value += number;

                    var len = (ix - vix) switch
                    {
                        1 or 2 => 2,
                        3 or 4 => 4,
                        5 or 6 => 6,
                        _ => throw new("Invalid size"),
                    };

                    operand = operand[..vix] + value.ToString($"X{len}") + operand[endIx..];
                }
            }
            return operand;
        }

        private bool TryCreateLabel(string mnemonic, string operand)
        {
            //Check that the operand has content
            if (!(operand?.Length > 0))
                return false;

            var labelChar = operand[0];

            //If our operand starts with a label space character, create a label
            if (!RomProcessingConstants.LabelSpace.Contains(labelChar))
                return false;

            //Remove leading address symbol? This doesn't happen...
            //if (mnemonic.StartsWith('$'))
            //    mnemonic = mnemonic[1..];

            //Create new block for this label
            var newBlock = new AsmBlock()
            {
                Location = assembler.currentBlock.Location + assembler.currentBlock.Size,
                Label = mnemonic,
                IsString = RomProcessingConstants.StringSpace.Contains(operand[0]),
            };

            //Add block to assembler
            assembler.blocks.Add(newBlock);

            //Set as current
            assembler.currentBlock = newBlock;

            //Increment current block index
            assembler.blockIndex++;

            //Remove label marker
            if (labelChar == ':')
                assembler.line = assembler.line[1..];
            else if (labelChar == '[' || labelChar == '{')
            {
                assembler.line = operand[1..].TrimStart(RomProcessingConstants.CommaSpace);

                //Process the next 
                var state = new AssemblerState(assembler, currentType);
                state.ProcessText(labelChar);

                //Advance to next part
                AdvancePart();
            }

            return true;
        }

        public void ProcessText(char? openTag = null)
        {
            CheckDesc();

            while (assembler.line != null)
            {
                assembler.GetLine();
                if (assembler.line == null)
                    return;

                if (assembler.line.StartsWith("DB"))
                {
                    string hex = OpCode.HexRegex().Replace(assembler.line[2..], "");
                    var data = Convert.FromHexString(hex);
                    assembler.currentBlock.ObjList.Add(data);
                    assembler.currentBlock.Size += data.Length;
                    assembler.line = "";
                    continue;
                }

                string? mnemonic = null, operand = null, operand2 = null;

                while (assembler.line.Length > 0)
                {
                    var lineSymbol = assembler.line[0];

                    //Process strings
                    if (RomProcessingConstants.StringSpace.Contains(lineSymbol))
                    {
                        assembler.ConsumeString();
                        AdvancePart();
                        continue;
                    }

                    //Process raw data
                    if (RomProcessingConstants.AddressSpace.Contains(lineSymbol))
                    {
                        assembler.ProcessRawData();

                        if (openTag == '[')
                            assembler.lastDelimiter = null;

                        AdvancePart();
                        continue;
                    }

                    if (lineSymbol == '>')
                    {
                        if (openTag == '<')
                        {
                            assembler.line = assembler.line[1..].TrimStart(RomProcessingConstants.CommaSpace);
                            CheckDesc();
                        }
                        return;
                    }

                    //Array Close
                    if (lineSymbol == ']')
                    {
                        assembler.line = assembler.line[1..].TrimStart(RomProcessingConstants.CommaSpace);

                        delimiter ??= assembler.lastDelimiter;

                        if (delimiter != null)
                            if (delimiter >= 0x100)
                            {
                                assembler.currentBlock.ObjList.Add((ushort)delimiter);
                                assembler.currentBlock.Size += 2;
                            }
                            else
                            {
                                assembler.currentBlock.ObjList.Add((byte)delimiter);
                                assembler.currentBlock.Size += 1;
                            }

                        return;
                    }

                    //Block close
                    if (lineSymbol == '}')
                    {
                        if (openTag == '{')
                            assembler.line = assembler.line[1..].TrimStart(RomProcessingConstants.CommaSpace);
                        return;
                    }

                    //Array Open
                    if (lineSymbol == '[')
                    {
                        assembler.line = assembler.line[1..].TrimStart(RomProcessingConstants.CommaSpace);
                        var state = new AssemblerState(assembler, currentType);
                        state.ProcessText('[');
                        AdvancePart();
                        continue;
                    }

                    //Process origin tags
                    if (assembler.line.StartsWith("ORG"))
                    {
                        ProcessOrigin();
                        continue;
                    }

                    //Separate instructions into mnemonic and operand parts
                    var symbolIndex = assembler.line.IndexOfAny(RomProcessingConstants.SymbolSpace);
                    if (symbolIndex > 0)
                    {
                        mnemonic = assembler.line[..symbolIndex];
                        operand = assembler.line[symbolIndex..].TrimStart(RomProcessingConstants.CommaSpace);

                        //Process object tags
                        if (operand.StartsWith('<'))
                        {
                            assembler.line = operand[1..].TrimStart(RomProcessingConstants.CommaSpace);
                            var state = new AssemblerState(assembler, mnemonic, openTag == '[' && currentType == null);
                            state.ProcessText('<');
                            mnemonic = null;
                            continue;
                        }

                        assembler.line = operand;
                    }
                    else
                    {
                        mnemonic = assembler.line;
                        assembler.line = "";
                    }

                    break;
                }

                if (mnemonic?.Length > 0)
                {
                    //Get list of opcodes from mnemonic
                    var codes = _root.OpLookup[mnemonic.ToUpper()];

                    //If no codes were found, try to create a label
                    if (codes?.Any() != true)
                    {
                        if (TryCreateLabel(mnemonic, operand))
                            continue;

                        //If label creation fails, throw exception
                        throw new($"Unknown instruction line {assembler.lineCount}: '{mnemonic}'");
                    }

                    //Reset current assembler line?
                    assembler.line = "";

                    //No operand instructions
                    if (string.IsNullOrEmpty(operand))
                    {
                        assembler.currentBlock.ObjList.Add(new Op
                        {
                            Code = codes.Single(x => x.Size == 1),
                            Operands = [],
                            Size = 1,
                        });
                        assembler.currentBlock.Size++;
                        continue;
                    }

                    //Do maths before regex processing
                    operand = DoMaths(operand);

                    ///COP processing
                    OpCode? opCode = codes.FirstOrDefault();
                    if (opCode.Mnem == "COP")
                    {
                        var parts = operand.Split(RomProcessingConstants.CopSplitChars, StringSplitOptions.RemoveEmptyEntries);

                        if (!_root.CopLookup.TryGetValue(parts[0], out var cop))
                            throw new($"Unknown COP command {parts[0]}");

                        assembler.currentBlock.ObjList.Add(new Op()
                        {
                            Code = opCode,
                            CopDef = cop,
                            Operands = [(byte)cop.Code, .. parts[1..]],
                            Size = (byte)(cop.Size + 2),
                        });

                        assembler.currentBlock.Size += cop.Size + 2;

                        continue;
                    }

                    opCode = null;
                    foreach (var code in codes)
                    {
                        //Keep branch operands until all blocks are processed (for labels)
                        if (code.Mode == AddressingMode.PCRelative
                            || code.Mode == AddressingMode.PCRelativeLong)
                        {
                            opCode = code;
                            break;
                        }

                        //Regex parse operand based on addressing mode
                        if (OpCode.AddressingRegex.TryGetValue(code.Mode, out var regex))
                        {
                            var match = regex.Match(operand);
                            if (match.Success)
                            {
                                //Keep the current code
                                opCode = code;

                                //Operand is the "first" matched group
                                operand = match.Groups[1].Value;

                                //Support for second operand (MVN/MVP)
                                if (match.Groups.Count > 2)
                                    operand2 = match.Groups[2].Value;

                                break;
                            }
                        }
                    }

                    if (operand[0] == '#')
                        operand = operand[1..];

                    if (operand[0] == '$')
                        operand = operand[1..];

                    if (opCode == null)
                    {
                        var addrIx = operand.IndexOfAny(RomProcessingConstants.AddressSpace);
                        if (addrIx >= 0)
                        {
                            var eix = operand.IndexOfAny([' ', '\t', ',', ']', ')'], addrIx);
                            if (eix >= 0)
                                operand = operand[addrIx..eix];
                        }

                        opCode =
                            codes.SingleOrDefault(x => x.Mode == AddressingMode.Immediate)
                            ?? codes.SingleOrDefault(x => x.Mode == AddressingMode.AbsoluteLong)
                            ?? codes.SingleOrDefault(x => x.Mode == AddressingMode.Absolute);

                        if (opCode == null)
                            throw new($"Unable to determine mode/code line {assembler.lineCount}: '{assembler.line}'");
                    }

                    object opnd1 = assembler.ParseOperand(operand);

                    int size = opCode.Size;
                    if (size == -2)
                        size = operand[0] == '^' || opnd1 is byte ? 2 : 3;

                    object[] opnds =
                        operand2 != null ? [opnd1, assembler.ParseOperand(operand2)] : [opnd1];

                    assembler.currentBlock.ObjList.Add(new Op()
                    {
                        Code = opCode,
                        Operands = opnds,
                        Size = (byte)size,
                    });

                    assembler.currentBlock.Size += size;
                }
            }
        }
    }
}
