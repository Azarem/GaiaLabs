/**
 * CPU Instruction Factories
 *
 * This module provides factory functions for creating SnesLayoutable objects
 * that represent individual 65c816 CPU instructions.
 */
import { ILayoutable, EmitContext } from "./tsal";
import { OpCode, OpDef, Byte, Word } from "./generated-opcodes";

export type Operand = Byte | Word | number | string | any[];

export enum AddressingMode {
    Accumulator = 'Accumulator',
    Immediate = 'Immediate',
    Absolute = 'Absolute',
    AbsoluteIndirect = 'AbsoluteIndirect',
    AbsoluteIndirectLong = 'AbsoluteIndirectLong',
    DirectPage = 'DirectPage',
    AbsoluteIndexedX = 'AbsoluteIndexedX',
    AbsoluteIndexedY = 'AbsoluteIndexedY',
    AbsoluteIndexedIndirect = 'AbsoluteIndexedIndirect',
    DirectPageIndexedX = 'DirectPageIndexedX',
    DirectPageIndexedY = 'DirectPageIndexedY',
    DirectPageIndexedIndirectX = 'DirectPageIndexedIndirectX',
    Implied = 'Implied',
    StackRelative = 'StackRelative',
    StackRelativeIndirectIndexedY = 'StackRelativeIndirectIndexedY',
    DirectPageIndirect = 'DirectPageIndirect',
    AbsoluteLong = 'AbsoluteLong',
    AbsoluteLongIndexedX = 'AbsoluteLongIndexedX',
    DirectPageIndirectLong = 'DirectPageIndirectLong',
    DirectPageIndirectLongIndexedY = 'DirectPageIndirectLongIndexedY',
    DirectPageIndirectIndexedY = 'DirectPageIndirectIndexedY',
    BlockMove = 'BlockMove',
    PCRelative = 'PCRelative',
    PCRelativeLong = 'PCRelativeLong',
    StackInterrupt = 'StackInterrupt',
    Stack = 'Stack'
  }

export enum IndexMode {
    X = 'X',
    Y = 'Y',
    Indirect = 'Indirect',
    IndirectX = 'IndirectX',
    IndirectLong = 'IndirectLong',
    IndirectLongY = 'IndirectLongY',
    XIndirect = 'XIndirect',
    IndirectY = 'IndirectY',
    S = 'S',
    SIndirectY = 'SIndirectY'
}

export class Instruction implements ILayoutable {
    public readonly size: number;

    constructor(
        public readonly opDef: OpDef,
        public readonly operand?: Operand
    ) {
        if (typeof opDef.size === 'number') {
            this.size = opDef.size;
        } else { // 'flag-dependent'
            // This logic might need to be state-aware in a real assembler.
            // For now, we'll assume Word (3 bytes total) if not a Byte.
            this.size = 3;
            if (this.operand && typeof this.operand === 'number' && this.operand < 256) {
                this.size = 2;
            }
        }
    }

    getChildren() { return []; }

    getSize() {
        if (this.opDef.size === 'flag-dependent') {
            if (this.operand && typeof this.operand === 'number' && this.operand > 255) {
                return 3;
            }
            return 2;
        }
        return this.opDef.size;
    }

    emit(ctx: EmitContext) {
        ctx.emitBytes([this.opDef.opcode]);
        console.log(`Emitting 0x${this.opDef.opcode.toString(16)} for ${this.opDef.mnemonic}`);
        // ... more complex operand emission logic will go here ...
    }
} 