/**
 * Mock TSAL Core Definitions
 *
 * This file provides the core type definitions for the TSAL language itself,
 * such as the execution context and layout functions.
 */


// A placeholder for any kind of data or instruction that can be laid out in the ROM.
export interface Layoutable<T extends Platform> {
    readonly layout: LayoutFunction<T>;
}



export interface Locational {
    location: number;
}

/**
 * Represents a target platform for code generation.
 */
export interface Platform {
    name: string;
}

/**
 * The ExecutionContext (`ctx`) is the core of the code generation engine.
 * It provides methods for emitting CPU instructions and data.
 * The context is typed to a specific platform (`T`) to ensure that only
 * valid instructions for that platform can be called.
 */
export interface ExecutionContext<T extends Platform> {
    /** Lays out a single layoutable object. */
    (data: Layoutable<T>): void;
    /** Lays out a sequence of layoutable objects in order. */
    (data: Layoutable<T>[]): void;

    /** A primitive for emitting a sequence of raw bytes into the ROM. */
    emitBytes: (bytes: number[]) => void;

    // Direct methods for CPU instructions
    LDA: (operand: any) => void;
}

/**
 * A LayoutFunction is any function that takes an ExecutionContext and uses it
 * to emit code or data. This is the fundamental building block of a TSAL module.
 */
export type LayoutFunction<T extends Platform> = (ctx: ExecutionContext<T>) => void; 