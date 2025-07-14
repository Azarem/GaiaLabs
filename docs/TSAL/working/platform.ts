/**
 * Mock Platform Definition File
 * 
 * This file provides mock implementations for the target platform context
 * and platform-specific data types.
 */
import { Platform, LayoutFunction, Layoutable, Locational, EmitContext } from './tsal';

/**
 * Represents the SNES platform context.
 * This object could hold platform-specific configuration in a real implementation.
 */
export interface SnesPlatform extends Platform {
    name: 'snes';
}


// --- Type-safe Data Size Wrappers ---
// These functions wrap raw numbers to provide type safety and clarity,
// ensuring that the correct data size is used for an instruction.

export interface SizedNumber extends Layoutable {
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

export interface Pointer {
    readonly isPointer: true;
}

export interface Direct extends Byte, Pointer { }

export interface Absolute extends Word, Pointer { }

export interface AbsoluteLong extends Long, Pointer { }

/** Represents a layoutable chunk of raw data bytes. */
export interface DataChunk extends Layoutable, Partial<Locational> {
    readonly values: (number | Locational)[];
    readonly bytesPerElement: 1 | 2 | 3;
}

/** Internal helper to create a DataChunk with the correct layout logic. */
function createDataChunk(values: (number | Locational)[], bytesPerElement: 1 | 2 | 3): DataChunk {
    return {
        values,
        bytesPerElement,
        layout: () => {
            const bytes: number[] = [];
            for (const val of values) {
                let numVal: number;
                if (typeof val === 'number') {
                    numVal = val;
                } else if (val.location !== undefined) {
                    // This is the key change: resolve the location of a Code/Label object.
                    numVal = val.location;
                } else {
                    // This can happen if layout order is incorrect.
                    throw new Error(`Could not resolve location for data value. Ensure it is included in the final layout before being referenced.`);
                }

                switch (bytesPerElement) {
                    case 1:
                        bytes.push(numVal & 0xFF);
                        break;
                    case 2:
                        bytes.push(numVal & 0xFF, (numVal >> 8) & 0xFF); // Little-endian
                        break;
                    case 3:
                        bytes.push(numVal & 0xFF, (numVal >> 8) & 0xFF, (numVal >> 16) & 0xFF); // Little-endian
                        break;
                }
            }
            // By using the emitBytes primitive, we respect the abstraction.
            // This function doesn't know about romData or currentLocation, only the public API.
            ctx.emitBytes(bytes);
        }
    };
}

// --- Pointer & Address Types ---


/**
 * Creates a 2-byte pointer to a target. The pointer's value (the target's address)
 * is resolved during the layout phase.
 * @param target The Locational object to point to (e.g., a Code block or a Label).
 */
export const pointer = <T extends Locational>(target: T): Pointer<T> => ({
    _tag: 'Pointer',
    target,
    layout: null,
    emit: (ctx: EmitContext) => {
        if (target.location === undefined || target.location < 0) {
            throw new Error(`Could not resolve location for pointer target. Ensure the target is included in the final layout before being referenced.`);
        }
        // Emit a 2-byte word in little-endian format.
        ctx.emitBytes([target.location & 0xFF, (target.location >> 8) & 0xFF]);
    }
});

/**
 * Creates a 3-byte pointer to a target. The pointer's value (the target's address)
 * is resolved during the layout phase.
 * @param target The Locational object to point to (e.g., a Code block or a Label).
 */
export const longPointer = <T extends Locational>(target: T): LongPointer<T> => ({
    _tag: 'LongPointer',
    target,
    layout: (ctx: SnesContext) => {
        if (target.location === undefined || target.location < 0) {
            throw new Error(`Could not resolve location for long pointer target. Ensure the target is included in the final layout before being referenced.`);
        }
        // Emit a 3-byte long in little-endian format.
        ctx.emitBytes([target.location & 0xFF, (target.location >> 8) & 0xFF, (target.location >> 16) & 0xFF]);
    }
});


// --- List & Table Factories ---

export interface List<T extends Layoutable> extends Layoutable {
    readonly items: T[];
}

/**
 * Creates a layoutable list from an array of SnesLayoutable items.
 * When this list is laid out, it will in turn lay out each of its items in sequence.
 * @param items An array of SnesLayoutable items.
 */
export const list = <T extends Layoutable>(items: T[]): List<T> => ({
    items,
    layout: null,
    optimize: null,
    emit: (ctx: EmitContext) => {
        for (const item of items) {
            if(item.emit !== null) {
                item.emit(ctx);
            }
        }
    }
});

/**
 * Creates a layoutable list of 2-byte pointers to the given items.
 * This is the standard way to create jump tables or address lists.
 * @param items An array of Locational items to create pointers for.
 */
export const pointerTable = <T extends Locational>(items: T[]): List<Pointer<T>> => {
    return list(items.map(item => pointer(item)));
};

/**
 * Creates a layoutable list of 3-byte pointers to the given items.
 * @param items An array of Locational items to create pointers for.
 */
export const longPointerTable = <T extends Locational>(items: T[]): List<LongPointer<T>> => {
    return list(items.map(item => longPointer(item)));
};


// --- Operand & Data Primitives ---

/** A union type representing any valid operand for a CPU instruction. */
export type Operand = Byte | Word | Long;

/**
 * Defines a 1-byte value for an instruction operand, OR a sequence of raw bytes for ROM data.
 * @param values A single value for an operand, or multiple values for a data chunk.
 */
export function byte(value: number): Byte;
export function byte(...values: (number|Locational)[]): DataChunk;
export function byte(...values: any[]): Byte | DataChunk {
    if (values.length === 1 && typeof values[0] === 'number') {
        const value = values[0];
        return { _tag: 'SizedNumber', size: 1, value, emit: (ctx: EmitContext) => ctx.emitBytes([value & 0xFF]) };
    }
    return createDataChunk(values, 1);
}

/**
 * Defines a 2-byte value for an instruction operand, OR a sequence of raw words for ROM data.
 * @param values A single value for an operand, or multiple values for a data chunk.
 */
export function word(value: number): Word;
export function word(...values: (number|Locational)[]): DataChunk;
export function word(...values: any[]): Word | DataChunk {
    if (values.length === 1 && typeof values[0] === 'number') {
        const value = values[0];
        return { _tag: 'SizedNumber', size: 2, value, emit: (ctx: EmitContext) => ctx.emitBytes([value & 0xFF, (value >> 8) & 0xFF]) };
    }
    return createDataChunk(values, 2);
}

/**
 * Defines a 3-byte value for an instruction operand, OR a sequence of raw longs for ROM data.
 * @param values A single value for an operand, or multiple values for a data chunk.
 */
export function long(value: number): Long;
export function long(...values: (number|Locational)[]): DataChunk;
export function long(...values: any[]): Long | DataChunk {
    if (values.length === 1 && typeof values[0] === 'number') {
        const value = values[0];
        return { _tag: 'SizedNumber', size: 3, value, emit: (ctx: EmitContext) => ctx.emitBytes([value & 0xFF, (value >> 8) & 0xFF, (value >> 16) & 0xFF]) };
    }
    return createDataChunk(values, 3);
}


export interface Code extends Layoutable, Locational {
    readonly layout: LayoutFunction;
};

export function code(func: LayoutFunction): Code {
    return {
        layout: func,
        location: -1 // Default location, to be updated by the layout engine
    };
}

export interface Label extends Layoutable, Locational {};

export function label() : Label {
    return {
        // A label has no binary output, it's just a marker.
        // Its location is resolved by the toolchain.
        layout: () => {},
        location: -1 // Default location, to be updated by the layout engine
    };
}