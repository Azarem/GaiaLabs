/**
 * Mock Platform Definition File
 * 
 * This file provides mock implementations for the target platform context
 * and platform-specific data types.
 */
import { ExecutionContext, Platform, LayoutFunction, Layoutable, Locational } from './tsal';

/**
 * Represents the SNES platform context.
 * This object could hold platform-specific configuration in a real implementation.
 */
export interface SnesPlatform extends Platform {
    name: 'snes';
}

export interface SnesLayoutable {
    readonly layout: SnesLayoutFunction;
}

export type SnesLayoutFunction = (ctx: SnesContext) => void;

/**
 * A specialized context for the SNES platform. It extends the base ExecutionContext
 * with SNES-specific CPU instructions and the desired `COP` syntax.
 */
export interface SnesContext extends ExecutionContext<SnesPlatform> {
    /** Emits a single layoutable object or an array of them. */
    emit(data: SnesLayoutable | SnesLayoutable[]): void;

    /** A primitive for writing raw bytes to the ROM stream. */
    emitBytes(bytes: number[]): void;

    /** 
     * A collection of functions on the context that directly emit COP instructions.
     * This supports the `ctx.COP[0xCA](...)` syntax.
     */
    COP: {
        /** Indexed access for any COP opcode. */
        [code: number]: (...params: any[]) => void;
        /** A named, type-safe variant for a known COP opcode. */
        D2: (param1: number, param2: number) => void;
        /** A named, type-safe variant for a known COP opcode. */
        CA: (param1: number) => void;
    };
}

// --- Type-safe Data Size Wrappers ---
// These functions wrap raw numbers to provide type safety and clarity,
// ensuring that the correct data size is used for an instruction.

export interface SizedNumber {
    readonly _tag: 'SizedNumber';
    readonly size: number;
    readonly value: number;
}

/** The Byte type inherits _tag and value from SizedNumber, only narrowing the size. */
export interface Byte extends SizedNumber {
    readonly size: 1;
}

/** The Word type inherits _tag and value from SnedNumber, only narrowing the size. */
export interface Word extends SizedNumber {
    readonly size: 2;
}

export interface Long extends SizedNumber {
    readonly size: 3;
}

/** Represents a layoutable chunk of raw data bytes. */
export interface DataChunk extends SnesLayoutable, Partial<Locational> {
    readonly values: number[];
    readonly bytesPerElement: 1 | 2 | 3;
}

/** Internal helper to create a DataChunk with the correct layout logic. */
function createDataChunk(values: number[], bytesPerElement: 1 | 2 | 3): DataChunk {
    return {
        values,
        bytesPerElement,
        layout: (ctx: SnesContext) => {
            const bytes: number[] = [];
            for (const val of values) {
                switch (bytesPerElement) {
                    case 1:
                        bytes.push(val & 0xFF);
                        break;
                    case 2:
                        bytes.push(val & 0xFF, (val >> 8) & 0xFF); // Little-endian
                        break;
                    case 3:
                        bytes.push(val & 0xFF, (val >> 8) & 0xFF, (val >> 16) & 0xFF); // Little-endian
                        break;
                }
            }
            // By using the emitBytes primitive, we respect the abstraction.
            // This function doesn't know about romData or currentLocation, only the public API.
            ctx.emitBytes(bytes);
        }
    };
}

/** The Address type inherits _tag and value, only narrowing the size based on its generic type. */
export interface Address<T extends SizedNumber> extends SizedNumber {
    readonly size: T['size'];
}

export const dp = (value: number): Address<Byte> => ({
    _tag: 'SizedNumber',
    size: 1,
    value
});

export const abs = (value: number): Address<Word> => ({
    _tag: 'SizedNumber',
    size: 2,
    value
});

export const long = (value: number): Address<Long> => ({
    _tag: 'SizedNumber',
    size: 3,
    value
});

/** A union type representing any valid operand for a CPU instruction. */
export type Operand = SizedNumber | SnesLayoutable;

/**
 * Defines a 1-byte value for an instruction operand, OR a sequence of raw bytes for ROM data.
 * @param values A single value for an operand, or multiple values for a data chunk.
 */
export function byte(value: number): Byte;
export function byte(...values: number[]): DataChunk;
export function byte(...values: number[]): Byte | DataChunk {
    if (values.length === 1) {
        return { _tag: 'SizedNumber', size: 1, value: values[0] };
    }
    return createDataChunk(values, 1);
}

/**
 * Defines a 2-byte value for an instruction operand, OR a sequence of raw words for ROM data.
 * @param values A single value for an operand, or multiple values for a data chunk.
 */
export function word(value: number): Word;
export function word(...values: number[]): DataChunk;
export function word(...values:number[]): Word | DataChunk {
    if (values.length === 1) {
        return { _tag: 'SizedNumber', size: 2, value: values[0] };
    }
    return createDataChunk(values, 2);
}


export interface Code extends SnesLayoutable {
    readonly layout: SnesLayoutFunction;
};

export function code(func: SnesLayoutFunction): Code {
    return {
        layout: func
    };
}

export interface Label extends SnesLayoutable, Locational {};

export function label() : Label {
    return {
        // A label has no binary output, it's just a marker.
        // Its location is resolved by the toolchain.
        layout: (ctx: SnesContext) => {},
        location: -1 // Default location, to be updated by the layout engine
    };
}