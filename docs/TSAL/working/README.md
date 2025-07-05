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

---

## How TSAL Replaces Traditional Assembly Macros

In traditional assembly, macros are a primary tool for abstraction and code reuse. TSAL achieves the same goals not by generating `.asm` text files, but by using standard TypeScript functions and constants to generate binary code directly.

Below are examples from `na4B_spirit.ts` translated into what they would look like as traditional assembly macros.

### Example 1: Reusable Logic Block

A TSAL `code` block is a self-contained, reusable piece of logic, directly equivalent to a macro or a labeled subroutine.

**TSAL (`na4B_spirit.ts`)**
```typescript
export const e_na4B_spirit = code((ctx) => ctx.emit([
    COP.D2(0x06, 0x01),
    COP.D2(0x07, 0x01),
    LDA.imm(word(0x2000)),
    TRB.dp(mem.dp.actor.flags10),
    LDA.imm(word(0x0200)),
    TSB.dp(mem.dp.actor.flags12),
    COP.CA(0x03)
]));
```

**Traditional Assembly Macro Equivalent**
```asm
; A helper macro to simplify the COP instruction format.
; This mirrors the abstraction that TSAL's `COP` object provides.
cop_macro: macro op, params...
    db $02, op, params... ; $02 is the 65c816 opcode for COP
endm

; The e_na4B_spirit logic, defined as a labeled subroutine.
e_na4B_spirit:
    cop_macro $D2, $06, $01      ; TSAL: COP.D2(0x06, 0x01)
    cop_macro $D2, $07, $01      ; TSAL: COP.D2(0x07, 0x01)
    lda #$2000                   ; TSAL: LDA.imm(word(0x2000))
    trb dp_actor_flags10         ; TSAL: TRB.dp(mem.dp.actor.flags10)
    lda #$0200                   ; TSAL: LDA.imm(word(0x0200))
    tsb dp_actor_flags12         ; TSAL: TSB.dp(mem.dp.actor.flags12)
    cop_macro $CA, $03           ; TSAL: COP.CA(0x03)
    rtl                          ; Return from long subroutine
```

### Example 2: Control Flow and Pointers

This example demonstrates more advanced concepts like an infinite loop (`BRA`) and passing a code block's address as a parameter, which is conceptually similar to a function pointer.

**TSAL (`na4B_spirit.ts`)**
```typescript
export const code_05F307 = code((ctx) => ctx.emit([
    COP[0x80](0x32),
    COP[0x89](),
    COP[0x21](0x02, code_05F313), // Pass the address of code_05F313
    BRA(code_05F307)             // Create an infinite loop
]));
```

**Traditional Assembly Macro Equivalent**
In this case, the assembler's ability to resolve a label to its address is crucial. TSAL's toolchain handles this reference management automatically.

```asm
; Assume cop_macro is defined as above

code_05F307:
    cop_macro $80, $32               ; TSAL: COP[0x80](0x32)
    cop_macro $89                    ; TSAL: COP[0x89]()

    ; The assembler resolves 'code_05F313' to its 16-bit address,
    ; effectively passing a function pointer.
    cop_macro $21, $02, .word(code_05F313)

    ; Branch back to the start of this label.
    bra code_05F307

; The target subroutine would be defined elsewhere.
code_05F313:
    ; ... (contents of the code_05F313 block) ...
    rtl
```

These examples illustrate how TSAL provides the same expressive power as traditional macros but enhances it with a type-safe, modern development environment. 

---

## Enhanced Macros: Building with `label()`

The introduction of an explicit `label()` primitive unlocks the ability to create powerful, high-level abstractions that mirror control flow from modern languages. These "enhanced macros" are simply TypeScript functions that organize instructions and labels into safe, reusable patterns.

### Example 3: `if/else` Blocks

In assembly, creating an `if/else` structure requires careful management of conditional branches and jumps to avoid accidentally executing both code paths. TSAL can abstract this completely.

Let's imagine we have a helper function called `ifEqual` that takes a "then" block and an optional "else" block.

**TSAL (`macros.ts`)**
```typescript
/**
 * Executes a block of code if the Zero flag is set (e.g., after a CMP
 * where the values were equal).
 * @param thenBlock The code to execute if the condition is met.
 * @param elseBlock The optional code to execute if it is not.
 */
function ifEqual(thenBlock: SnesLayoutable, elseBlock?: SnesLayoutable) {
    const elseLabel = label();
    const endLabel = label();

    if (elseBlock) {
        return code(ctx => ctx.emit([
            BNE(elseLabel),  // If Not Equal, branch to the 'else' part
            thenBlock,
            BRA(endLabel),   // Unconditionally jump over the 'else' part
            elseLabel,
            elseBlock,
            endLabel
        ]));
    } else {
        return code(ctx => ctx.emit([
            BNE(endLabel),   // If Not Equal, branch to the end
            thenBlock,
            endLabel
        ]));
    }
}
```

**Using the Macro (`na4B_spirit.ts`)**
```typescript
code(ctx => ctx.emit([
    LDA.dp(mem.dp.someValue),
    CMP.imm(byte(10)), // Check if the value is 10
    // If it was equal, run the first block. Otherwise, run the second.
    ifEqual(
        code(ctx => ctx.emit(STA.dp(mem.dp.isTen))),
        code(ctx => ctx.emit(STA.dp(mem.dp.isNotTen)))
    )
]));
```

**Traditional Assembly Equivalent**
```asm
    lda dp_someValue
    cmp #10
    bne if_else      ; Branch if Not Equal
    sta dp_isTen     ; This is the "then" block
    bra if_end       ; Skip the "else" block
if_else:
    sta dp_isNotTen  ; This is the "else" block
if_end:
    ; execution continues
```
The TSAL version is not only more readable, but it completely prevents the common bug of forgetting the `bra if_end` instruction, which would cause both the "then" and "else" blocks to execute.

### Example 4: `while` Loops

A `while` loop is another fundamental structure that can be cleanly abstracted. The following TSAL function creates a standard `while` loop that checks a condition at the top.

**TSAL (`macros.ts`)**
```typescript
/**
 * Creates a while loop.
 * @param condition A block of code that sets processor flags.
 * @param branchToExit The branch instruction that will EXIT the loop.
 * @param body The code to execute inside the loop.
 */
function createWhileLoop(condition: SnesLayoutable, branchToExit: (target: Label) => Instruction, body: SnesLayoutable) {
    const loopStart = label();
    const loopEnd = label();

    return code(ctx => ctx.emit([
        loopStart,
        condition,
        branchToExit(loopEnd), // e.g., BEQ(loopEnd) to exit when a value is zero
        body,
        BRA(loopStart),        // Jump back to the top to re-check the condition
        loopEnd
    ]));
}
```

**Using the Macro (`na4B_spirit.ts`)**
This example creates a loop that counts down from 10 (in the X register) and clears a block of memory.
```typescript
code(ctx => ctx.emit([
    LDX.imm(byte(10)),
    // The loop will continue as long as X is not zero.
    createWhileLoop(
        code(ctx => ctx.emit(DEX)), // Condition: Decrement X. Sets Z flag when X is 0.
        BEQ,                       // Exit Condition: Branch if Equal (to zero)
        code(ctx => ctx.emit(STA.abs_x(0x3000))) // Body: Store A at address $3000 + X
    )
]));
```

**Traditional Assembly Equivalent**
```asm
    ldx #10
while_start:
    dex                ; Condition: Decrement X
    beq while_end      ; Exit if X is zero
    sta $3000,x        ; Body: Store A
    bra while_start    ; Loop
while_end:
    ; execution continues
```
Again, the TSAL function encapsulates the entire loop structure—labels and branches—into a single, declarative call, making the programmer's intent obvious and the code much safer to write and maintain. 