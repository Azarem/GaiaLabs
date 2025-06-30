using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;
using System.Globalization;

namespace GaiaLib.Rom.Rebuild
{
    internal class AssemblerState
    {
        private readonly DbStruct? dbStruct;
        private readonly DbStruct? parentStruct;
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

            dbStruct = structType == null ? null
                : _root.Structs.Values.FirstOrDefault(x =>
                    x.Name.Equals(structType, StringComparison.CurrentCultureIgnoreCase)
                );

            parentStruct = dbStruct?.Parent == null ? null
                : _root.Structs.Values.FirstOrDefault(x =>
                    x.Name.Equals(dbStruct.Parent, StringComparison.CurrentCultureIgnoreCase)
                );

            discriminator = parentStruct?.Discriminator;
            delimiter = dbStruct?.Delimiter;
            memberOffset = 0;
            dataOffset = 0;
            memberTypes = dbStruct?.Types;
            currentType = memberTypes?[memberOffset];

            if (saveDelimiter)
                assembler.lastDelimiter = dbStruct?.Delimiter ?? parentStruct?.Delimiter;
        }



        private void CheckDisc()
        {
            if (discriminator == dataOffset)
            {
                //Discriminator is always 1 byte
                assembler.currentBlock.ObjList.Add((byte)dbStruct.Discriminator.Value);
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
            assembler._lineBuffer = assembler._lineBuffer[3..].TrimStart(RomProcessingConstants.CommaSpace);
            if (assembler._lineBuffer.StartsWith('$'))
                assembler._lineBuffer = assembler._lineBuffer[1..];

            string hex;
            var endIx = assembler._lineBuffer.IndexOfAny(RomProcessingConstants.CommaSpace);
            if (endIx >= 0)
            {
                hex = assembler._lineBuffer[..endIx];
                assembler._lineBuffer = assembler._lineBuffer[(endIx + 1)..].TrimStart(RomProcessingConstants.CommaSpace);
            }
            else
            {
                hex = assembler._lineBuffer;
                assembler._lineBuffer = "";
            }

            var location = int.Parse(hex, NumberStyles.HexNumber);

            assembler.blocks.Add(assembler.currentBlock = new AsmBlock { Location = location });
            assembler.blockIndex++;
        }

        private static string DoMath(string operand)
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
                IsString = _root.StringDelimiters.Contains(operand[0]),
            };

            //Add block to assembler
            assembler.blocks.Add(newBlock);

            //Set as current
            assembler.currentBlock = newBlock;

            //Increment current block index
            assembler.blockIndex++;

            //Remove label marker
            if (labelChar == ':')
                assembler._lineBuffer = assembler._lineBuffer[1..];
            else if (labelChar == '[' || labelChar == '{')
            {
                assembler._lineBuffer = operand[1..].TrimStart(RomProcessingConstants.CommaSpace);

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
            CheckDisc();

            while (!assembler.eof)
            {
                if (!assembler.GetLine())
                    return;

                if (assembler._lineBuffer.StartsWith("DB"))
                {
                    string hex = OpCode.HexRegex().Replace(assembler._lineBuffer[2..], "");
                    var data = Convert.FromHexString(hex);
                    assembler.currentBlock.ObjList.Add(data);
                    assembler.currentBlock.Size += data.Length;
                    assembler._lineBuffer = "";
                    continue;
                }

                string? mnemonic = null, operand = null, operand2 = null;

                while (assembler._lineBuffer.Length > 0)
                {
                    var lineSymbol = assembler._lineBuffer[0];

                    //Process strings
                    //if (RomProcessingConstants.StringSpace.Contains(lineSymbol))
                    if(_root.StringDelimiters.Contains(lineSymbol))
                    {
                        assembler._stringProcessor.ConsumeString();
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
                            assembler._lineBuffer = assembler._lineBuffer[1..].TrimStart(RomProcessingConstants.CommaSpace);
                            CheckDisc();
                        }
                        return;
                    }

                    //Array Close
                    if (lineSymbol == ']')
                    {
                        assembler._lineBuffer = assembler._lineBuffer[1..].TrimStart(RomProcessingConstants.CommaSpace);

                        delimiter ??= assembler.lastDelimiter;

                        //Apply delimiter if set
                        if (delimiter != null)
                            if (delimiter >= 0x100)
                            {
                                //When over the word boundary use two bytes
                                assembler.currentBlock.ObjList.Add((ushort)delimiter);
                                assembler.currentBlock.Size += 2;
                            }
                            else
                            {
                                //Otherwise default to single byte
                                assembler.currentBlock.ObjList.Add((byte)delimiter);
                                assembler.currentBlock.Size += 1;
                            }

                        return;
                    }

                    //Block close
                    if (lineSymbol == '}')
                    {
                        if (openTag == '{')
                            assembler._lineBuffer = assembler._lineBuffer[1..].TrimStart(RomProcessingConstants.CommaSpace);
                        return;
                    }

                    //Array Open
                    if (lineSymbol == '[')
                    {
                        assembler._lineBuffer = assembler._lineBuffer[1..].TrimStart(RomProcessingConstants.CommaSpace);
                        var state = new AssemblerState(assembler, currentType);
                        state.ProcessText('[');
                        AdvancePart();
                        continue;
                    }

                    //Process origin tags
                    if (assembler._lineBuffer.StartsWith("ORG"))
                    {
                        ProcessOrigin();
                        continue;
                    }

                    //Separate instructions into mnemonic and operand parts
                    var symbolIndex = assembler._lineBuffer.IndexOfAny(RomProcessingConstants.SymbolSpace);
                    if (symbolIndex > 0)
                    {
                        mnemonic = assembler._lineBuffer[..symbolIndex];
                        operand = assembler._lineBuffer[symbolIndex..].TrimStart(RomProcessingConstants.CommaSpace);

                        //Process object tags
                        if (operand.StartsWith('<'))
                        {
                            assembler._lineBuffer = operand[1..].TrimStart(RomProcessingConstants.CommaSpace);
                            var state = new AssemblerState(assembler, mnemonic, openTag == '[' && currentType == null);
                            state.ProcessText('<');
                            mnemonic = null;
                            continue;
                        }

                        assembler._lineBuffer = operand;
                    }
                    else
                    {
                        mnemonic = assembler._lineBuffer;
                        assembler._lineBuffer = "";
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
                    assembler._lineBuffer = "";

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
                    operand = DoMath(operand);

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
                            throw new($"Unable to determine mode/code line {assembler.lineCount}: '{assembler._lineBuffer}'");
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
