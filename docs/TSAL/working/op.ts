/**
 * CPU Instruction Factories
 *
 * This module provides factory functions for creating SnesLayoutable objects
 * that represent individual 65c816 CPU instructions.
 */
import {
    SnesLayoutable, SnesContext, Operand, SizedNumber, Address,
    Code, Byte, Word, Long, dp, abs, longAddress
} from "./platform";
import { Locational } from "./tsal";

/** Base interface for a CPU instruction. */
export interface Instruction extends SnesLayoutable, Partial<Locational> {
    readonly mnemonic: string;
    readonly operand?: Operand;
    location?: number;
}

/** Internal helper to create a generic instruction. */
function createInstruction(mnemonic: string, opcode: number, operand?: Operand): Instruction {
    return {
        mnemonic,
        operand,
        layout: function (ctx: SnesContext) { // Use 'function' to get a 'this' context
            const self = this as Instruction;
            ctx.emitBytes([opcode]);

            if (operand) {
                if ('_tag' in operand && operand._tag === 'SizedNumber') {
                    const val = operand.value;
                    if (operand.size === 1) ctx.emitBytes([val & 0xFF]);
                    if (operand.size === 2) ctx.emitBytes([val & 0xFF, (val >> 8) & 0xFF]);
                    if (operand.size === 3) ctx.emitBytes([val & 0xFF, (val >> 8) & 0xFF, (val >> 16) & 0xFF]);
                } else if (typeof (operand as any).layout === 'function') {
                    // This handles branching, where the operand is a Locational Code object
                    const target = operand as Code & Locational;
                    if (self.location === undefined || target.location === undefined) {
                        throw new Error(`Branch source or target location is not defined for ${mnemonic}.`);
                    }
                    const offset = target.location - (self.location + 2); // rel8 is from next instruction
                    if (offset < -128 || offset > 127) throw new Error(`BRA target out of range.`);
                    ctx.emitBytes([offset & 0xFF]);
                }
            }
        }
    };
}

// --- Factory Functions ---

export const LDA = {
    imm: (val: Byte | Word) => createInstruction('LDA', 0xA9, val),
    dp: (addr: Address<Byte>) => createInstruction('LDA', 0xA5, addr),
    dp_x: (addr: Address<Byte>) => createInstruction('LDA', 0xB5, addr),
    abs: (addr: Address<Word>) => createInstruction('LDA', 0xAD, addr),
    abs_x: (addr: Address<Word>) => createInstruction('LDA', 0xBD, addr),
    abs_y: (addr: Address<Word>) => createInstruction('LDA', 0xB9, addr),
    long: (addr: Address<Long>) => createInstruction('LDA', 0xAF, addr),
    long_x: (addr: Address<Long>) => createInstruction('LDA', 0xBF, addr),
    // todo: add indirect modes
};

export const STA = {
    abs: (addr: Address<Word>) => createInstruction('STA', 0x8D, addr),
    // ... other STA modes
};

export const TRB = {
    dp: (addr: Address<Byte>) => createInstruction('TRB', 0x14, addr),
    abs: (addr: Address<Word>) => createInstruction('TRB', 0x1C, addr),
};

export const TSB = {
    dp: (addr: Address<Byte>) => createInstruction('TSB', 0x04, addr),
    abs: (addr: Address<Word>) => createInstruction('TSB', 0x0C, addr),
};

export const BRA = (target: Code) => createInstruction('BRA', 0x80, target);

// ... other instructions would follow the same pattern 