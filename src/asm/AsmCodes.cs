using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace GaiaLabs
{
    public class Asm
    {
        public static readonly Dictionary<byte, OpCode> OpCodes = new()
        {
            {0x69, new OpCode{ Mnem = "ADC", Code = 0x69, Mode = AddressingMode.Immediate, Size = -2 } },
            {0x6D, new OpCode{ Mnem = "ADC", Code = 0x6D, Mode = AddressingMode.Absolute, Size = 3 } },
            {0x6F, new OpCode{ Mnem = "ADC", Code = 0x6F, Mode = AddressingMode.AbsoluteLong, Size = 4 } },
            {0x65, new OpCode{ Mnem = "ADC", Code = 0x65, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0x72, new OpCode{ Mnem = "ADC", Code = 0x72, Mode = AddressingMode.DirectPageIndirect, Size = 2 } },
            {0x67, new OpCode{ Mnem = "ADC", Code = 0x67, Mode = AddressingMode.DirectPageIndirectLong, Size = 2 } },
            {0x7D, new OpCode{ Mnem = "ADC", Code = 0x7D, Mode = AddressingMode.AbsoluteIndexedX, Size = 3 } },
            {0x7F, new OpCode{ Mnem = "ADC", Code = 0x7F, Mode = AddressingMode.AbsoluteLongIndexedX, Size = 4 } },
            {0x79, new OpCode{ Mnem = "ADC", Code = 0x79, Mode = AddressingMode.AbsoluteIndexedY, Size = 3 } },
            {0x75, new OpCode{ Mnem = "ADC", Code = 0x75, Mode = AddressingMode.DirectPageIndexedX, Size = 2 } },
            {0x61, new OpCode{ Mnem = "ADC", Code = 0x61, Mode = AddressingMode.DirectPageIndexedIndirectX, Size = 2 } },
            {0x71, new OpCode{ Mnem = "ADC", Code = 0x71, Mode = AddressingMode.DirectPageIndirectIndexedY, Size = 2 } },
            {0x77, new OpCode{ Mnem = "ADC", Code = 0x77, Mode = AddressingMode.DirectPageIndirectLongIndexedY, Size = 2 } },
            {0x63, new OpCode{ Mnem = "ADC", Code = 0x63, Mode = AddressingMode.StackRelative, Size = 2 } },
            {0x73, new OpCode{ Mnem = "ADC", Code = 0x73, Mode = AddressingMode.StackRelativeIndirectIndexedY, Size = 2 } },

            {0x29, new OpCode{ Mnem = "AND", Code = 0x29, Mode = AddressingMode.Immediate, Size = -2 } },
            {0x2D, new OpCode{ Mnem = "AND", Code = 0x2D, Mode = AddressingMode.Absolute, Size = 3 } },
            {0x2F, new OpCode{ Mnem = "AND", Code = 0x2F, Mode = AddressingMode.AbsoluteLong, Size = 4 } },
            {0x25, new OpCode{ Mnem = "AND", Code = 0x25, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0x32, new OpCode{ Mnem = "AND", Code = 0x32, Mode = AddressingMode.DirectPageIndirect, Size = 2 } },
            {0x27, new OpCode{ Mnem = "AND", Code = 0x27, Mode = AddressingMode.DirectPageIndirectLong, Size = 2 } },
            {0x3D, new OpCode{ Mnem = "AND", Code = 0x3D, Mode = AddressingMode.AbsoluteIndexedX, Size = 3 } },
            {0x3F, new OpCode{ Mnem = "AND", Code = 0x3F, Mode = AddressingMode.AbsoluteLongIndexedX, Size = 4 } },
            {0x39, new OpCode{ Mnem = "AND", Code = 0x39, Mode = AddressingMode.AbsoluteIndexedY, Size = 3 } },
            {0x35, new OpCode{ Mnem = "AND", Code = 0x35, Mode = AddressingMode.DirectPageIndexedX, Size = 2 } },
            {0x21, new OpCode{ Mnem = "AND", Code = 0x21, Mode = AddressingMode.DirectPageIndexedIndirectX, Size = 2 } },
            {0x31, new OpCode{ Mnem = "AND", Code = 0x31, Mode = AddressingMode.DirectPageIndirectIndexedY, Size = 2 } },
            {0x37, new OpCode{ Mnem = "AND", Code = 0x37, Mode = AddressingMode.DirectPageIndirectLongIndexedY, Size = 2 } },
            {0x23, new OpCode{ Mnem = "AND", Code = 0x23, Mode = AddressingMode.StackRelative, Size = 2 } },
            {0x33, new OpCode{ Mnem = "AND", Code = 0x33, Mode = AddressingMode.StackRelativeIndirectIndexedY, Size = 2 } },

            {0x0A, new OpCode{ Mnem = "ASL", Code = 0x0A, Mode = AddressingMode.Accumulator, Size = 1 } },
            {0x0E, new OpCode{ Mnem = "ASL", Code = 0x0E, Mode = AddressingMode.Absolute, Size = 3 } },
            {0x06, new OpCode{ Mnem = "ASL", Code = 0x06, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0x1E, new OpCode{ Mnem = "ASL", Code = 0x1E, Mode = AddressingMode.AbsoluteIndexedX, Size = 3 } },
            {0x16, new OpCode{ Mnem = "ASL", Code = 0x16, Mode = AddressingMode.DirectPageIndexedX, Size = 2 } },

            {0x10, new OpCode{ Mnem = "BPL", Code = 0x10, Mode = AddressingMode.PCRelative, Size = 2 } },
            {0x30, new OpCode{ Mnem = "BMI", Code = 0x30, Mode = AddressingMode.PCRelative, Size = 2 } },
            {0x80, new OpCode{ Mnem = "BRA", Code = 0x80, Mode = AddressingMode.PCRelative, Size = 2 } },
            {0x82, new OpCode{ Mnem = "BRL", Code = 0x82, Mode = AddressingMode.PCRelativeLong, Size = 3 } },
            {0x90, new OpCode{ Mnem = "BCC", Code = 0x90, Mode = AddressingMode.PCRelative, Size = 2 } },
            {0xB0, new OpCode{ Mnem = "BCS", Code = 0xB0, Mode = AddressingMode.PCRelative, Size = 2 } },
            {0xD0, new OpCode{ Mnem = "BNE", Code = 0xD0, Mode = AddressingMode.PCRelative, Size = 2 } },
            {0xF0, new OpCode{ Mnem = "BEQ", Code = 0xF0, Mode = AddressingMode.PCRelative, Size = 2 } },
            {0x50, new OpCode{ Mnem = "BVC", Code = 0x50, Mode = AddressingMode.PCRelative, Size = 2 } },
            {0x70, new OpCode{ Mnem = "BVS", Code = 0x70, Mode = AddressingMode.PCRelative, Size = 2 } },

            {0x89, new OpCode{ Mnem = "BIT", Code = 0x89, Mode = AddressingMode.Immediate, Size = -2 } },
            {0x2C, new OpCode{ Mnem = "BIT", Code = 0x2C, Mode = AddressingMode.Absolute, Size = 3 } },
            {0x24, new OpCode{ Mnem = "BIT", Code = 0x24, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0x3C, new OpCode{ Mnem = "BIT", Code = 0x3C, Mode = AddressingMode.AbsoluteIndexedX, Size = 3 } },
            {0x34, new OpCode{ Mnem = "BIT", Code = 0x34, Mode = AddressingMode.DirectPageIndexedX, Size = 2 } },

            {0x00, new OpCode{ Mnem = "BRK", Code = 0x00, Mode = AddressingMode.StackInterrupt, Size = 2 } },

            {0x18, new OpCode{ Mnem = "CLC", Code = 0x18, Mode = AddressingMode.Implied, Size = 1 } },
            {0xD8, new OpCode{ Mnem = "CLD", Code = 0xD8, Mode = AddressingMode.Implied, Size = 1 } },
            {0x58, new OpCode{ Mnem = "CLI", Code = 0x58, Mode = AddressingMode.Implied, Size = 1 } },
            {0xB8, new OpCode{ Mnem = "CLV", Code = 0xB8, Mode = AddressingMode.Implied, Size = 1 } },

            {0xC9, new OpCode{ Mnem = "CMP", Code = 0xC9, Mode = AddressingMode.Immediate, Size = -2 } },
            {0xCD, new OpCode{ Mnem = "CMP", Code = 0xCD, Mode = AddressingMode.Absolute, Size = 3 } },
            {0xCF, new OpCode{ Mnem = "CMP", Code = 0xCF, Mode = AddressingMode.AbsoluteLong, Size = 4 } },
            {0xC5, new OpCode{ Mnem = "CMP", Code = 0xC5, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0xD2, new OpCode{ Mnem = "CMP", Code = 0xD2, Mode = AddressingMode.DirectPageIndirect, Size = 2 } },
            {0xC7, new OpCode{ Mnem = "CMP", Code = 0xC7, Mode = AddressingMode.DirectPageIndirectLong, Size = 2 } },
            {0xDD, new OpCode{ Mnem = "CMP", Code = 0xDD, Mode = AddressingMode.AbsoluteIndexedX, Size = 3 } },
            {0xDF, new OpCode{ Mnem = "CMP", Code = 0xDF, Mode = AddressingMode.AbsoluteLongIndexedX, Size = 4 } },
            {0xD9, new OpCode{ Mnem = "CMP", Code = 0xD9, Mode = AddressingMode.AbsoluteIndexedY, Size = 3 } },
            {0xD5, new OpCode{ Mnem = "CMP", Code = 0xD5, Mode = AddressingMode.DirectPageIndexedX, Size = 2 } },
            {0xC1, new OpCode{ Mnem = "CMP", Code = 0xC1, Mode = AddressingMode.DirectPageIndexedIndirectX, Size = 2 } },
            {0xD1, new OpCode{ Mnem = "CMP", Code = 0xD1, Mode = AddressingMode.DirectPageIndirectIndexedY, Size = 2 } },
            {0xD7, new OpCode{ Mnem = "CMP", Code = 0xD7, Mode = AddressingMode.DirectPageIndirectLongIndexedY, Size = 2 } },
            {0xC3, new OpCode{ Mnem = "CMP", Code = 0xC3, Mode = AddressingMode.StackRelative, Size = 2 } },
            {0xD3, new OpCode{ Mnem = "CMP", Code = 0xD3, Mode = AddressingMode.StackRelativeIndirectIndexedY, Size = 2 } },

            {0x02, new OpCode{ Mnem = "COP", Code = 0x02, Mode = AddressingMode.StackInterrupt, Size = 2 } },

            {0xE0, new OpCode{ Mnem = "CPX", Code = 0xE0, Mode = AddressingMode.Immediate, Size = -2 } },
            {0xEC, new OpCode{ Mnem = "CPX", Code = 0xEC, Mode = AddressingMode.Absolute, Size = 3 } },
            {0xE4, new OpCode{ Mnem = "CPX", Code = 0xE4, Mode = AddressingMode.DirectPage, Size = 2 } },

            {0xC0, new OpCode{ Mnem = "CPY", Code = 0xC0, Mode = AddressingMode.Immediate, Size = -2 } },
            {0xCC, new OpCode{ Mnem = "CPY", Code = 0xCC, Mode = AddressingMode.Absolute, Size = 3 } },
            {0xC4, new OpCode{ Mnem = "CPY", Code = 0xC4, Mode = AddressingMode.DirectPage, Size = 2 } },

            {0x3A, new OpCode{ Mnem = "DEC", Code = 0x3A, Mode = AddressingMode.Accumulator, Size = 1 } },
            {0xCE, new OpCode{ Mnem = "DEC", Code = 0xCE, Mode = AddressingMode.Absolute, Size = 3 } },
            {0xC6, new OpCode{ Mnem = "DEC", Code = 0xC6, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0xDE, new OpCode{ Mnem = "DEC", Code = 0xDE, Mode = AddressingMode.AbsoluteIndexedX, Size = 3 } },
            {0xD6, new OpCode{ Mnem = "DEC", Code = 0xD6, Mode = AddressingMode.DirectPageIndexedX, Size = 2 } },

            {0xCA, new OpCode{ Mnem = "DEX", Code = 0xCA, Mode = AddressingMode.Implied, Size = 1 } },
            {0x88, new OpCode{ Mnem = "DEY", Code = 0x88, Mode = AddressingMode.Implied, Size = 1 } },

            {0x49, new OpCode{ Mnem = "EOR", Code = 0x49, Mode = AddressingMode.Immediate, Size = -2 } },
            {0x4D, new OpCode{ Mnem = "EOR", Code = 0x4D, Mode = AddressingMode.Absolute, Size = 3 } },
            {0x4F, new OpCode{ Mnem = "EOR", Code = 0x4F, Mode = AddressingMode.AbsoluteLong, Size = 4 } },
            {0x45, new OpCode{ Mnem = "EOR", Code = 0x45, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0x52, new OpCode{ Mnem = "EOR", Code = 0x52, Mode = AddressingMode.DirectPageIndirect, Size = 2 } },
            {0x47, new OpCode{ Mnem = "EOR", Code = 0x47, Mode = AddressingMode.DirectPageIndirectLong, Size = 2 } },
            {0x5D, new OpCode{ Mnem = "EOR", Code = 0x5D, Mode = AddressingMode.AbsoluteIndexedX, Size = 3 } },
            {0x5F, new OpCode{ Mnem = "EOR", Code = 0x5F, Mode = AddressingMode.AbsoluteLongIndexedX, Size = 4 } },
            {0x59, new OpCode{ Mnem = "EOR", Code = 0x59, Mode = AddressingMode.AbsoluteIndexedY, Size = 3 } },
            {0x55, new OpCode{ Mnem = "EOR", Code = 0x55, Mode = AddressingMode.DirectPageIndexedX, Size = 2 } },
            {0x41, new OpCode{ Mnem = "EOR", Code = 0x41, Mode = AddressingMode.DirectPageIndexedIndirectX, Size = 2 } },
            {0x51, new OpCode{ Mnem = "EOR", Code = 0x51, Mode = AddressingMode.DirectPageIndirectIndexedY, Size = 2 } },
            {0x57, new OpCode{ Mnem = "EOR", Code = 0x57, Mode = AddressingMode.DirectPageIndirectLongIndexedY, Size = 2 } },
            {0x43, new OpCode{ Mnem = "EOR", Code = 0x43, Mode = AddressingMode.StackRelative, Size = 2 } },
            {0x53, new OpCode{ Mnem = "EOR", Code = 0x53, Mode = AddressingMode.StackRelativeIndirectIndexedY, Size = 2 } },

            {0x1A, new OpCode{ Mnem = "INC", Code = 0x1A, Mode = AddressingMode.Accumulator, Size = 1 } },
            {0xEE, new OpCode{ Mnem = "INC", Code = 0xEE, Mode = AddressingMode.Absolute, Size = 3 } },
            {0xE6, new OpCode{ Mnem = "INC", Code = 0xE6, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0xFE, new OpCode{ Mnem = "INC", Code = 0xFE, Mode = AddressingMode.AbsoluteIndexedX, Size = 3 } },
            {0xF6, new OpCode{ Mnem = "INC", Code = 0xF6, Mode = AddressingMode.DirectPageIndexedX, Size = 2 } },

            {0xE8, new OpCode{ Mnem = "INX", Code = 0xE8, Mode = AddressingMode.Implied, Size = 1 } },
            {0xC8, new OpCode{ Mnem = "INY", Code = 0xC8, Mode = AddressingMode.Implied, Size = 1 } },

            {0x4C, new OpCode{ Mnem = "JMP", Code = 0x4C, Mode = AddressingMode.Absolute, Size = 3 } },
            {0x6C, new OpCode{ Mnem = "JMP", Code = 0x6C, Mode = AddressingMode.AbsoluteIndirect, Size = 3 } },
            {0x7C, new OpCode{ Mnem = "JMP", Code = 0x7C, Mode = AddressingMode.AbsoluteIndexedIndirect, Size = 3 } },
            {0x5C, new OpCode{ Mnem = "JMP", Code = 0x5C, Mode = AddressingMode.AbsoluteLong, Size = 4 } },
            {0xDC, new OpCode{ Mnem = "JMP", Code = 0xDC, Mode = AddressingMode.AbsoluteIndirectLong, Size = 3 } },

            {0x20, new OpCode{ Mnem = "JSR", Code = 0x20, Mode = AddressingMode.Absolute, Size = 3 } },
            {0xFC, new OpCode{ Mnem = "JSR", Code = 0xFC, Mode = AddressingMode.AbsoluteIndexedIndirect, Size = 3 } },
            {0x22, new OpCode{ Mnem = "JSR", Code = 0x22, Mode = AddressingMode.AbsoluteLong, Size = 4 } },

            {0xA9, new OpCode{ Mnem = "LDA", Code = 0xA9, Mode = AddressingMode.Immediate, Size = -2 } },
            {0xAD, new OpCode{ Mnem = "LDA", Code = 0xAD, Mode = AddressingMode.Absolute, Size = 3 } },
            {0xAF, new OpCode{ Mnem = "LDA", Code = 0xAF, Mode = AddressingMode.AbsoluteLong, Size = 4 } },
            {0xA5, new OpCode{ Mnem = "LDA", Code = 0xA5, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0xB2, new OpCode{ Mnem = "LDA", Code = 0xB2, Mode = AddressingMode.DirectPageIndirect, Size = 2 } },
            {0xA7, new OpCode{ Mnem = "LDA", Code = 0xA7, Mode = AddressingMode.DirectPageIndirectLong, Size = 2 } },
            {0xBD, new OpCode{ Mnem = "LDA", Code = 0xBD, Mode = AddressingMode.AbsoluteIndexedX, Size = 3 } },
            {0xBF, new OpCode{ Mnem = "LDA", Code = 0xBF, Mode = AddressingMode.AbsoluteLongIndexedX, Size = 4 } },
            {0xB9, new OpCode{ Mnem = "LDA", Code = 0xB9, Mode = AddressingMode.AbsoluteIndexedY, Size = 3 } },
            {0xB5, new OpCode{ Mnem = "LDA", Code = 0xB5, Mode = AddressingMode.DirectPageIndexedX, Size = 2 } },
            {0xA1, new OpCode{ Mnem = "LDA", Code = 0xA1, Mode = AddressingMode.DirectPageIndexedIndirectX, Size = 2 } },
            {0xB1, new OpCode{ Mnem = "LDA", Code = 0xB1, Mode = AddressingMode.DirectPageIndirectIndexedY, Size = 2 } },
            {0xB7, new OpCode{ Mnem = "LDA", Code = 0xB7, Mode = AddressingMode.DirectPageIndirectLongIndexedY, Size = 2 } },
            {0xA3, new OpCode{ Mnem = "LDA", Code = 0xA3, Mode = AddressingMode.StackRelative, Size = 2 } },
            {0xB3, new OpCode{ Mnem = "LDA", Code = 0xB3, Mode = AddressingMode.StackRelativeIndirectIndexedY, Size = 2 } },

            {0xA2, new OpCode{ Mnem = "LDX", Code = 0xA2, Mode = AddressingMode.Immediate, Size = -2 } },
            {0xAE, new OpCode{ Mnem = "LDX", Code = 0xAE, Mode = AddressingMode.Absolute, Size = 3 } },
            {0xA6, new OpCode{ Mnem = "LDX", Code = 0xA6, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0xBE, new OpCode{ Mnem = "LDX", Code = 0xBE, Mode = AddressingMode.AbsoluteIndexedY, Size = 3 } },
            {0xB6, new OpCode{ Mnem = "LDX", Code = 0xB6, Mode = AddressingMode.DirectPageIndexedY, Size = 2 } },

            {0xA0, new OpCode{ Mnem = "LDY", Code = 0xA0, Mode = AddressingMode.Immediate, Size = -2 } },
            {0xAC, new OpCode{ Mnem = "LDY", Code = 0xAC, Mode = AddressingMode.Absolute, Size = 3 } },
            {0xA4, new OpCode{ Mnem = "LDY", Code = 0xA4, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0xBC, new OpCode{ Mnem = "LDY", Code = 0xBC, Mode = AddressingMode.AbsoluteIndexedX, Size = 3 } },
            {0xB4, new OpCode{ Mnem = "LDY", Code = 0xB4, Mode = AddressingMode.DirectPageIndexedX, Size = 2 } },

            {0x4A, new OpCode{ Mnem = "LSR", Code = 0x4A, Mode = AddressingMode.Accumulator, Size = 1 } },
            {0x4E, new OpCode{ Mnem = "LSR", Code = 0x4E, Mode = AddressingMode.Absolute, Size = 3 } },
            {0x46, new OpCode{ Mnem = "LSR", Code = 0x46, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0x5E, new OpCode{ Mnem = "LSR", Code = 0x5E, Mode = AddressingMode.AbsoluteIndexedX, Size = 3 } },
            {0x56, new OpCode{ Mnem = "LSR", Code = 0x56, Mode = AddressingMode.DirectPageIndexedX, Size = 2 } },

            {0x54, new OpCode{ Mnem = "MVN", Code = 0x54, Mode = AddressingMode.BlockMove, Size = 3 } },
            {0x44, new OpCode{ Mnem = "MVP", Code = 0x44, Mode = AddressingMode.BlockMove, Size = 3 } },

            {0xEA, new OpCode{ Mnem = "NOP", Code = 0xEA, Mode = AddressingMode.Implied, Size = 1 } },

            {0x09, new OpCode{ Mnem = "ORA", Code = 0x09, Mode = AddressingMode.Immediate, Size = -2 } },
            {0x0D, new OpCode{ Mnem = "ORA", Code = 0x0D, Mode = AddressingMode.Absolute, Size = 3 } },
            {0x0F, new OpCode{ Mnem = "ORA", Code = 0x0F, Mode = AddressingMode.AbsoluteLong, Size = 4 } },
            {0x05, new OpCode{ Mnem = "ORA", Code = 0x05, Mode = AddressingMode.DirectPage, Size = 2 } },
            {0x12, new OpCode{ Mnem = "ORA", Code = 0x12, Mode = AddressingMode.DirectPageIndirect, Size = 2 } },
            {0x07, new OpCode{ Mnem = "ORA", Code = 0x07, Mode = AddressingMode.DirectPageIndirectLong, Size = 2 } },
            {0x1D, new OpCode{ Mnem = "ORA", Code = 0x1D, Mode = AddressingMode.AbsoluteIndexedX, Size = 3 } },
            {0x1F, new OpCode{ Mnem = "ORA", Code = 0x1F, Mode = AddressingMode.AbsoluteLongIndexedX, Size = 4 } },
            {0x19, new OpCode{ Mnem = "ORA", Code = 0x19, Mode = AddressingMode.AbsoluteIndexedY, Size = 3 } },
            {0x15, new OpCode{ Mnem = "ORA", Code = 0x15, Mode = AddressingMode.DirectPageIndexedX, Size = 2 } },
            {0x01, new OpCode{ Mnem = "ORA", Code = 0x01, Mode = AddressingMode.DirectPageIndexedIndirectX, Size = 2 } },
            {0x11, new OpCode{ Mnem = "ORA", Code = 0x11, Mode = AddressingMode.DirectPageIndirectIndexedY, Size = 2 } },
            {0x17, new OpCode{ Mnem = "ORA", Code = 0x17, Mode = AddressingMode.DirectPageIndirectLongIndexedY, Size = 2 } },
            {0x03, new OpCode{ Mnem = "ORA", Code = 0x03, Mode = AddressingMode.StackRelative, Size = 2 } },
            {0x13, new OpCode{ Mnem = "ORA", Code = 0x13, Mode = AddressingMode.StackRelativeIndirectIndexedY, Size = 2 } },

            {0xF4, new OpCode{ Mnem = "PEA", Code = 0xF4, Size = 3, Mode = AddressingMode.Absolute } },
            {0xD4, new OpCode{ Mnem = "PEI", Code = 0xD4, Size = 2, Mode = AddressingMode.DirectPageIndirect } },
            {0x62, new OpCode{ Mnem = "PER", Code = 0x62, Size = 3, Mode = AddressingMode.PCRelativeLong } },

            {0x48, new OpCode{ Mnem = "PHA", Code = 0x48, Size = 1, Mode = AddressingMode.Stack } },
            {0x8B, new OpCode{ Mnem = "PHB", Code = 0x8B, Size = 1, Mode = AddressingMode.Stack } },
            {0x0B, new OpCode{ Mnem = "PHD", Code = 0x0B, Size = 1, Mode = AddressingMode.Stack } },
            {0x4B, new OpCode{ Mnem = "PHK", Code = 0x4B, Size = 1, Mode = AddressingMode.Stack } },
            {0x08, new OpCode{ Mnem = "PHP", Code = 0x08, Size = 1, Mode = AddressingMode.Stack } },
            {0xDA, new OpCode{ Mnem = "PHX", Code = 0xDA, Size = 1, Mode = AddressingMode.Stack } },
            {0x5A, new OpCode{ Mnem = "PHY", Code = 0x5A, Size = 1, Mode = AddressingMode.Stack } },

            {0x68, new OpCode{ Mnem = "PLA", Code = 0x68, Size = 1, Mode = AddressingMode.Stack } },
            {0xAB, new OpCode{ Mnem = "PLB", Code = 0xAB, Size = 1, Mode = AddressingMode.Stack } },
            {0x2B, new OpCode{ Mnem = "PLD", Code = 0x2B, Size = 1, Mode = AddressingMode.Stack } },
            {0x28, new OpCode{ Mnem = "PLP", Code = 0x28, Size = 1, Mode = AddressingMode.Stack } },
            {0xFA, new OpCode{ Mnem = "PLX", Code = 0xFA, Size = 1, Mode = AddressingMode.Stack } },
            {0x7A, new OpCode{ Mnem = "PLY", Code = 0x7A, Size = 1, Mode = AddressingMode.Stack } },

            {0xC2, new OpCode{ Mnem = "REP", Code = 0xC2, Size = 2, Mode = AddressingMode.Immediate } },

            {0x2A, new OpCode{ Mnem = "ROL", Code = 0x2A, Size = 1, Mode = AddressingMode.Accumulator } },
            {0x2E, new OpCode{ Mnem = "ROL", Code = 0x2E, Size = 3, Mode = AddressingMode.Absolute } },
            {0x26, new OpCode{ Mnem = "ROL", Code = 0x26, Size = 2, Mode = AddressingMode.DirectPage } },
            {0x3E, new OpCode{ Mnem = "ROL", Code = 0x3E, Size = 3, Mode = AddressingMode.AbsoluteIndexedX } },
            {0x36, new OpCode{ Mnem = "ROL", Code = 0x36, Size = 2, Mode = AddressingMode.DirectPageIndexedX } },

            {0x6A, new OpCode{ Mnem = "ROR", Code = 0x6A, Size = 1, Mode = AddressingMode.Accumulator } },
            {0x6E, new OpCode{ Mnem = "ROR", Code = 0x6E, Size = 3, Mode = AddressingMode.Absolute } },
            {0x66, new OpCode{ Mnem = "ROR", Code = 0x66, Size = 2, Mode = AddressingMode.DirectPage } },
            {0x7E, new OpCode{ Mnem = "ROR", Code = 0x7E, Size = 3, Mode = AddressingMode.AbsoluteIndexedX } },
            {0x76, new OpCode{ Mnem = "ROR", Code = 0x76, Size = 2, Mode = AddressingMode.DirectPageIndexedX } },

            {0x40, new OpCode{ Mnem = "RTI", Code = 0x40, Size = 1, Mode = AddressingMode.Stack } },
            {0x6B, new OpCode{ Mnem = "RTL", Code = 0x6B, Size = 1, Mode = AddressingMode.Stack } },
            {0x60, new OpCode{ Mnem = "RTS", Code = 0x60, Size = 1, Mode = AddressingMode.Stack } },

            {0xE9, new OpCode{ Mnem = "SBC", Code = 0xE9, Size = -2, Mode = AddressingMode.Immediate } },
            {0xED, new OpCode{ Mnem = "SBC", Code = 0xED, Size = 3, Mode = AddressingMode.Absolute } },
            {0xEF, new OpCode{ Mnem = "SBC", Code = 0xEF, Size = 4, Mode = AddressingMode.AbsoluteLong } },
            {0xE5, new OpCode{ Mnem = "SBC", Code = 0xE5, Size = 2, Mode = AddressingMode.DirectPage } },
            {0xF2, new OpCode{ Mnem = "SBC", Code = 0xF2, Size = 2, Mode = AddressingMode.DirectPageIndirect } },
            {0xE7, new OpCode{ Mnem = "SBC", Code = 0xE7, Size = 2, Mode = AddressingMode.DirectPageIndirectLong } },
            {0xFD, new OpCode{ Mnem = "SBC", Code = 0xFD, Size = 3, Mode = AddressingMode.AbsoluteIndexedX } },
            {0xFF, new OpCode{ Mnem = "SBC", Code = 0xFF, Size = 4, Mode = AddressingMode.AbsoluteLongIndexedX } },
            {0xF9, new OpCode{ Mnem = "SBC", Code = 0xF9, Size = 3, Mode = AddressingMode.AbsoluteIndexedY } },
            {0xF5, new OpCode{ Mnem = "SBC", Code = 0xF5, Size = 2, Mode = AddressingMode.DirectPageIndexedX } },
            {0xE1, new OpCode{ Mnem = "SBC", Code = 0xE1, Size = 2, Mode = AddressingMode.DirectPageIndexedIndirectX } },
            {0xF1, new OpCode{ Mnem = "SBC", Code = 0xF1, Size = 2, Mode = AddressingMode.DirectPageIndirectIndexedY } },
            {0xF7, new OpCode{ Mnem = "SBC", Code = 0xF7, Size = 2, Mode = AddressingMode.DirectPageIndirectLongIndexedY } },
            {0xE3, new OpCode{ Mnem = "SBC", Code = 0xE3, Size = 2, Mode = AddressingMode.StackRelative } },
            {0xF3, new OpCode{ Mnem = "SBC", Code = 0xF3, Size = 2, Mode = AddressingMode.StackRelativeIndirectIndexedY } },

            {0x38, new OpCode{ Mnem = "SEC", Code = 0x38, Size = 1, Mode = AddressingMode.Implied } },
            {0xF8, new OpCode{ Mnem = "SED", Code = 0xF8, Size = 1, Mode = AddressingMode.Implied } },
            {0x78, new OpCode{ Mnem = "SEI", Code = 0x78, Size = 1, Mode = AddressingMode.Implied } },
            {0xE2, new OpCode{ Mnem = "SEP", Code = 0xE2, Size = 2, Mode = AddressingMode.Immediate } },

            {0x8D, new OpCode{ Mnem = "STA", Code = 0x8D, Size = 3, Mode = AddressingMode.Absolute } },
            {0x8F, new OpCode{ Mnem = "STA", Code = 0x8F, Size = 4, Mode = AddressingMode.AbsoluteLong } },
            {0x85, new OpCode{ Mnem = "STA", Code = 0x85, Size = 2, Mode = AddressingMode.DirectPage } },
            {0x92, new OpCode{ Mnem = "STA", Code = 0x92, Size = 2, Mode = AddressingMode.DirectPageIndirect } },
            {0x87, new OpCode{ Mnem = "STA", Code = 0x87, Size = 2, Mode = AddressingMode.DirectPageIndirectLong } },
            {0x9D, new OpCode{ Mnem = "STA", Code = 0x9D, Size = 3, Mode = AddressingMode.AbsoluteIndexedX } },
            {0x9F, new OpCode{ Mnem = "STA", Code = 0x9F, Size = 4, Mode = AddressingMode.AbsoluteLongIndexedX } },
            {0x99, new OpCode{ Mnem = "STA", Code = 0x99, Size = 3, Mode = AddressingMode.AbsoluteIndexedY } },
            {0x95, new OpCode{ Mnem = "STA", Code = 0x95, Size = 2, Mode = AddressingMode.DirectPageIndexedX } },
            {0x81, new OpCode{ Mnem = "STA", Code = 0x81, Size = 2, Mode = AddressingMode.DirectPageIndexedIndirectX } },
            {0x91, new OpCode{ Mnem = "STA", Code = 0x91, Size = 2, Mode = AddressingMode.DirectPageIndirectIndexedY } },
            {0x97, new OpCode{ Mnem = "STA", Code = 0x97, Size = 2, Mode = AddressingMode.DirectPageIndirectLongIndexedY } },
            {0x83, new OpCode{ Mnem = "STA", Code = 0x83, Size = 2, Mode = AddressingMode.StackRelative } },
            {0x93, new OpCode{ Mnem = "STA", Code = 0x93, Size = 2, Mode = AddressingMode.StackRelativeIndirectIndexedY } },

            {0xDB, new OpCode{ Mnem = "STP", Code = 0xDB, Size = 1, Mode = AddressingMode.Implied } },

            {0x8E, new OpCode{ Mnem = "STX", Code = 0x8E, Size = 3, Mode = AddressingMode.Absolute } },
            {0x86, new OpCode{ Mnem = "STX", Code = 0x86, Size = 2, Mode = AddressingMode.DirectPage } },
            {0x96, new OpCode{ Mnem = "STX", Code = 0x96, Size = 2, Mode = AddressingMode.DirectPageIndexedY } },

            {0x8C, new OpCode{ Mnem = "STY", Code = 0x8C, Size = 3, Mode = AddressingMode.Absolute } },
            {0x84, new OpCode{ Mnem = "STY", Code = 0x84, Size = 2, Mode = AddressingMode.DirectPage } },
            {0x94, new OpCode{ Mnem = "STY", Code = 0x94, Size = 2, Mode = AddressingMode.DirectPageIndexedX } },

            {0x9C, new OpCode{ Mnem = "STZ", Code = 0x9C, Size = 3, Mode = AddressingMode.Absolute } },
            {0x64, new OpCode{ Mnem = "STZ", Code = 0x64, Size = 2, Mode = AddressingMode.DirectPage } },
            {0x9E, new OpCode{ Mnem = "STZ", Code = 0x9E, Size = 3, Mode = AddressingMode.AbsoluteIndexedX } },
            {0x74, new OpCode{ Mnem = "STZ", Code = 0x74, Size = 2, Mode = AddressingMode.DirectPageIndexedX } },

            {0xAA, new OpCode{ Mnem = "TAX", Code = 0xAA, Size = 1, Mode = AddressingMode.Implied } },
            {0xA8, new OpCode{ Mnem = "TAY", Code = 0xA8, Size = 1, Mode = AddressingMode.Implied } },

            {0x5B, new OpCode{ Mnem = "TCD", Code = 0x5B, Size = 1, Mode = AddressingMode.Implied } },
            {0x1B, new OpCode{ Mnem = "TCS", Code = 0x1B, Size = 1, Mode = AddressingMode.Implied } },
            {0x7B, new OpCode{ Mnem = "TDC", Code = 0x7B, Size = 1, Mode = AddressingMode.Implied } },

            {0x1C, new OpCode{ Mnem = "TRB", Code = 0x1C, Size = 3, Mode = AddressingMode.Absolute } },
            {0x14, new OpCode{ Mnem = "TRB", Code = 0x14, Size = 2, Mode = AddressingMode.DirectPage } },
            {0x0C, new OpCode{ Mnem = "TSB", Code = 0x0C, Size = 3, Mode = AddressingMode.Absolute } },
            {0x04, new OpCode{ Mnem = "TSB", Code = 0x04, Size = 2, Mode = AddressingMode.DirectPage } },

            {0x3B, new OpCode{ Mnem = "TSC", Code = 0x3B, Size = 1, Mode = AddressingMode.Implied } },
            {0xBA, new OpCode{ Mnem = "TSX", Code = 0xBA, Size = 1, Mode = AddressingMode.Implied } },

            {0x8A, new OpCode{ Mnem = "TXA", Code = 0x8A, Size = 1, Mode = AddressingMode.Implied } },
            {0x9A, new OpCode{ Mnem = "TXS", Code = 0x9A, Size = 1, Mode = AddressingMode.Implied } },
            {0x9B, new OpCode{ Mnem = "TXY", Code = 0x9B, Size = 1, Mode = AddressingMode.Implied } },

            {0x98, new OpCode{ Mnem = "TYA", Code = 0x98, Size = 1, Mode = AddressingMode.Implied } },
            {0xBB, new OpCode{ Mnem = "TYX", Code = 0xBB, Size = 1, Mode = AddressingMode.Implied } },

            {0xCB, new OpCode{ Mnem = "WAI", Code = 0xCB, Size = 1, Mode = AddressingMode.Implied } },
            {0x42, new OpCode{ Mnem = "WDM", Code = 0x42, Size = 1, Mode = AddressingMode.Implied } },

            {0xEB, new OpCode{ Mnem = "XBA", Code = 0xEB, Size = 1, Mode = AddressingMode.Implied } },
            {0xFB, new OpCode{ Mnem = "XCE", Code = 0xFB, Size = 1, Mode = AddressingMode.Implied } },
        };


        public static unsafe Op Parse(byte* rom, Location loc, Registers reg, DbRoot db)
        {
            var ptr = rom + loc;
            if (!OpCodes.TryGetValue(*ptr, out var code))
                throw new("Unknown OpCode");

            ptr++;
            List<uint> operands = [];
            List<Location> refs = [];
            int size = 1;

            switch (code.Mode)
            {
                case AddressingMode.Immediate:

                    if (size == -2) //Handle variable-size operand
                        if ((code.Code & 0xF) == 0x9) //Accumulator operations?
                            if ((reg.StatusFlags & StatusFlags.AccumulatorMode) == 0) goto bit16; //Check status of m flag
                            else goto bit8;
                        else if ((reg.StatusFlags & StatusFlags.IndexMode) == 0) goto bit16; //Check status of x flag
                        else goto bit8;

                    break;

                bit16:
                    operands.Add(*(ushort*)ptr);
                    size = 3;
                    ptr += 2;
                    break;

                bit8:
                    operands.Add(*ptr++);
                    size += 1;
                    break;

                case AddressingMode.StackInterrupt:
                    var cmd = *ptr++;
                    operands.Add(cmd);
                    if (code.Mnem == "COP")
                    {
                        var cop = db.CopLib.Codes[cmd];
                        var parts = cop.Parts;
                        size = cop.Size + 2;
                        for (int i = 0; i < parts.Length; i++)
                            switch (parts[i])
                            {
                                case 'b':
                                    operands.Add(*ptr++);
                                    break;
                                case 'c':
                                    refs.Add((loc & ~0xFFFFu) | *(ushort*)ptr);
                                    goto case 'w';
                                case 'o':
                                case 'w':
                                    operands.Add(*(ushort*)ptr);
                                    ptr += 2;
                                    break;
                                case 'C':
                                    refs.Add(*(ushort*)ptr | ((uint)ptr[2] << 16));
                                    goto case 'O';
                                case 'O':
                                    operands.Add(*(ushort*)ptr | ((uint)ptr[2] << 16));
                                    ptr += 3;
                                    break;
                            }
                    }
                    break;
            }

            return new Op { Code = code, Size = (byte)size, References = refs };
        }

    }

    // [ n v m x d i z c ]
    // n = Negative
    // v = Overflow
    // m = Accumulator Mode
    // x = Index Mode
    // d = Decimal Mode
    // i = IRQ Disable
    // z = Zero
    // c = Carry

    [Flags]
    public enum StatusFlags
    {
        /// <summary>
        /// Clear before starting addition or subtraction.
        /// Arithmetic overflow:
        /// [addition - carry out of high bit:]
        /// 0 = no carry
        /// 1 = carry
        /// [subtraction - borrow required to subtract:]
        /// 0 = borrow required
        /// 1 = no borrow required
        /// [Logic:]
        /// receives bit shifted or rotated out;
        /// source of bit rotated in
        /// </summary>
        Carry = 0x01,
        /// <summary>
        /// Indicates zero or non-zero result:
        /// 0 = non-zero result
        /// 1 = zero result
        /// </summary>
        Zero = 0x02,
        /// <summary>
        /// Enables or disables processor's IRQ interrupt line:
        /// Set to disable interrupts by masking the IRQ line
        /// Clear to enable IRQ interrupts
        /// </summary>
        IrqDisable = 0x04,
        /// <summary>
        /// Determines mode for add/subtract (not increment/decrement, though):
        /// Set to force decimal operation (BCD)
        /// Clear to return to binary operation
        /// </summary>
        DecimalMode = 0x08,
        IndexMode = 0x10,
        AccumulatorMode = 0x20,
        /// <summary>
        /// Clear to reverse "set-overflow" hardware input.
        /// Indicates invalid carry into high bit of arithmetic
        /// result (two's-complement overflow):
        /// 0 = two's-complement result ok
        /// 1 = error if two's-complement arithmetic
        /// </summary>
        Overflow = 0x40,
        /// <summary>
        /// Reflects most significant bit of result
        /// (the sign of a two's-complement binary number):
        /// 0 = high bit clear (positive result)
        /// 1 = high bit set (negative result)
        /// </summary>
        Negative = 0x80
    }

    public class Registers
    {
        public StatusFlags StatusFlags { get; set; }
        public byte Bank { get; set; }
    }

    public class Op
    {
        public OpCode Code { get; set; }
        public Location Location { get; set; }
        //public int Operand { get; set; }
        public int[] Operands { get; set; }
        public byte Size { get; set; }

        private Op _prev;
        public Op Prev { get => _prev; set { if (_prev != value && (_prev = value) != null) value._next = this; } }

        private Op _next;
        public Op Next { get => _next; set { if (_next != value && (_next = value) != null) value._prev = this; } }
        //public Op Reference { get; set; }

        internal IEnumerable<Location> References { get; set; }

        public override string ToString()
        {
            var fmt = GetFormat();
            var mnem = Code.Mnem;
            var op = Operands;
            return (op?.Length ?? 0) switch
            {
                1 => string.Format(fmt, mnem, op[0]),
                2 => string.Format(fmt, mnem, op[0], op[1]),
                3 => string.Format(fmt, mnem, op[0], op[1], op[2]),
                4 => string.Format(fmt, mnem, op[0], op[1], op[2], op[3]),
                5 => string.Format(fmt, mnem, op[0], op[1], op[2], op[3], op[4]),
                6 => string.Format(fmt, mnem, op[0], op[1], op[2], op[3], op[4], op[5]),
                7 => string.Format(fmt, mnem, op[0], op[1], op[2], op[3], op[4], op[5], op[6]),
                8 => string.Format(fmt, mnem, op[0], op[1], op[2], op[3], op[4], op[5], op[6], op[7]),
                _ => string.Format(fmt, mnem)
            };
        }

        public string GetFormat(DbRoot db = null)
        {
            db ??= RomLoader.Current.DbRoot;
            if (!db.CopLib.Formats.TryGetValue(Code.Mode, out var str))
            {
                if (Code.Mnem == "COP")
                {
                    var cop = db.CopLib.Codes[(byte)Operands[0]];
                    str = cop.GetFormat();
                }
                else
                    str = "{0}";
            }

            return str;
        }
    }

    //public class Ref
    //{
    //    private Ref _prev;
    //    public Ref Prev { get => _prev; set { if (_prev != value && (_prev = value) != null) value._next = this; } }

    //    private Ref _next;
    //    public Ref Next { get => _next; set { if (_next != value && (_next = value) != null) value._prev = this; } }
    //}

    public class OpCode
    {
        public byte Code;
        public string Mnem;
        public AddressingMode Mode;
        public sbyte Size;
    }

    public enum AddressingMode
    {
        Accumulator,
        Immediate,
        Absolute,
        AbsoluteIndirect,
        AbsoluteIndirectLong,
        DirectPage,
        AbsoluteIndexedX,
        AbsoluteIndexedY,
        AbsoluteIndexedIndirect,
        DirectPageIndexedX,
        DirectPageIndexedY,
        DirectPageIndexedIndirectX,
        Implied,
        StackRelative,
        StackRelativeIndirectIndexedY,
        DirectPageIndirect,
        AbsoluteLong,
        AbsoluteLongIndexedX,
        DirectPageIndirectLong,
        DirectPageIndirectLongIndexedY,
        DirectPageIndirectIndexedY,
        BlockMove,
        PCRelative,
        PCRelativeLong,
        StackInterrupt,
        Stack
    }
}
