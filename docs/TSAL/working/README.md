# Understanding the TSAL Working Examples

This document explains the concepts behind the TypeScript Assembly Language (TSAL) by walking through the example files in this directory.

## What is TSAL?

TSAL is a system designed to write assembly code for the SNES 65c816 processor using the modern features of TypeScript. Instead of writing raw `.asm` text files, developers write TypeScript code that generates the binary machine code.

The primary goals are:
*   **Type Safety**: Catch bugs at compile time, not runtime.
*   **Discoverability**: Use IDE features like autocomplete to explore available instructions and addressing modes.
*   **Organization**: Structure code into logical modules with clear dependencies.
*   **Maintainability**: Create readable, self-documenting code that is easier to modify and extend.

The diagram above illustrates the layered architecture of the system.

## File Breakdown & Core Concepts

The system is broken down into several layers, each represented by a file.

### `platform.ts`: The Foundation

This is the lowest level of the TSAL architecture. It provides the fundamental building blocks that are tied to the SNES hardware itself.

*   **`SnesLayoutable`**: A core interface. Anything that can be written into the ROM must implement this. It has a `layout()` method that the toolchain calls to get the object's binary representation.
*   **Sized Primitives**: `byte`, `word`, and `long` are factory functions that create number types with a specific size. This ensures, for example, that an instruction expecting a 16-bit word doesn't accidentally receive a 24-bit long.
*   **`code(...)`**: A function that takes a series of instructions and data and turns them into a single `SnesLayoutable` block.

### `op.ts`: The CPU Instruction Set

This file defines the standard 65c816 CPU instructions (`LDA`, `STA`, `TRB`, `TSB`, `BRA`, etc.). It makes heavy use of a namespaced factory pattern to ensure type safety for addressing modes.

**Example:**
Instead of a generic `LDA(value, mode)` function, we have specific, named functions:
```typescript
LDA.imm(word(0x2000)); // Load immediate value
LDA.dp(mem.dp.actor.flags10); // Load from direct page address
LDA.abs(0x2100); // Load from absolute address
LDA.long(0x7F0200); // Load from long address
```
This design has two key benefits:
1.  The compiler will throw an error if you try to use an addressing mode that doesn't exist for an instruction (e.g., `BRA.imm(...)`).
2.  Typing `LDA.` in the IDE will pop up a list of all valid, implemented addressing modes, making the system easy to learn and explore.

### `game.ts`: Game-Specific Abstractions

This file builds on top of the platform and CPU layers to create abstractions specific to *this* game (Illusion of Gaia).

*   **`mem`**: A structured object representing the game's memory map. This allows us to write `mem.dp.actor.flags10` instead of a magic number like `$10`, making code far more readable.
*   **`COP`**: An object for handling the SNES's Co-Processor instructions. These are special instructions that are often game-specific. TSAL supports two ways to call them:
    *   **Indexed (e.g., `COP[0xDA](...)`)**: Used for newly discovered or undocumented opcodes during reverse engineering. It's flexible but not type-safe.
    *   **Named (e.g., `COP.DisplayText(...)`)**: The preferred method for production code. Once an opcode's function is known, it's given a descriptive name and a strongly-typed signature. This is the "promotion" path for all `COP` commands.
*   **`WideString`**: A template literal for handling the game's two-byte character encoding for dialogue.

### `na4B_spirit.ts`: An Application Module

This file is a real-world example of a game module. It defines the code and data for a specific entity (the Nazca spirit) in the game. It brings all the other layers together.

Let's break down a snippet from `code_05F313`:
```typescript
export const code_05F313 = code((ctx) => ctx.emit([
    // Game-specific command to do something with parameters.
    COP[0xBC](0x70, 0x00), 

    // CPU instruction to load the immediate value 0xFFF0 into the accumulator.
    LDA.imm(word(0xFFF0)), 

    // CPU instruction to set bits in memory at a known absolute address.
    // The address is read from the game's memory map for clarity.
    TSB.abs(mem.joypad_mask_std), 

    // Game-specific command to display the text we defined earlier.
    // The compiler knows that this command expects a WideString.
    COP[0xBF](widestring_05F349), 
]));
```

### The `layout` Export

The most critical part of a module is the `layout` export at the bottom:

```typescript
export const layout = code((ctx) => ctx.emit([
    h_na4B_spirit,
    e_na4B_spirit,
    code_05F307,
    code_05F313,
    widestring_05F349,
]));
```

This explicitly tells the TSAL toolchain what pieces to include in the final ROM and, crucially, **in what order**. This explicit ordering eliminates a whole class of bugs common in traditional assembly development where the layout is implicit and fragile. The toolchain processes this array to generate the final binary output for the module. 