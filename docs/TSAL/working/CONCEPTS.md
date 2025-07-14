# TSAL Concepts & Working Examples

This document explains the core concepts behind TSAL by walking through the actual working examples in this directory. It bridges the theoretical architecture outlined in `../v3/` with the concrete implementation found in the working files.

## 🏗️ **Architecture Deep Dive**

TSAL is built in layers, each solving a specific problem in the SNES development process.

### Layer 1: Foundation (`platform.ts`)

The foundation layer provides the core abstractions that are tied to SNES hardware.

```typescript
// Core interface - anything that goes into the ROM must implement this
interface SnesLayoutable {
  layout(ctx: EmitContext): void;
}

// Sized primitives ensure type safety at the byte level
const value8 = byte(0x42);     // Guaranteed 8-bit value
const value16 = word(0x1234);  // Guaranteed 16-bit value  
const value24 = long(0x123456); // Guaranteed 24-bit value

// Code blocks are the fundamental unit of assembly organization
const myCode = code(ctx => ctx.emit([
  LDA.imm(value8),
  STA.abs(0x2000),
  RTL()
]));
```

**Key Insight**: Unlike traditional assemblers that work with text, TSAL works with typed objects from the ground up. This enables the TypeScript compiler to catch entire classes of bugs before runtime.

### Layer 2: CPU Instructions (`op.ts` & Generated Files)

The CPU layer provides type-safe access to all 65c816 instructions through our sophisticated generation system.

#### Simple Functions (Single-Variant Instructions)
```typescript
// Instructions with only one addressing mode get simple functions
const operations = [
  NOP(),           // Implied mode only
  CLC(),           // Implied mode only  
  BMI('label'),    // PC Relative mode only
  RTS(),           // Stack mode only
];
```

#### Factory Objects (Multi-Variant Instructions)
```typescript
// Instructions with multiple addressing modes get factory objects
const loadInstructions = [
  LDA.imm(0x42),      // Immediate mode
  LDA.abs(0x1234),    // Absolute mode
  LDA.dp(0x80),       // Direct page mode
  LDA.long(0x7E1234), // Long mode
  LDA.abs_x(0x1234),  // Absolute indexed X mode
  // ... 10 more addressing modes available
];
```

**Key Insight**: The API shape reflects the underlying hardware. Instructions that naturally have one form get simple calls, while instructions that have many forms get rich factory objects. This makes the common case simple and the complex case powerful.

### Layer 3: Game-Specific Abstractions (`game.ts`)

The game layer builds meaningful abstractions for the specific title being developed.

#### Memory Layout
```typescript
// Instead of magic numbers, we have structured memory maps
const badCode = LDA.abs(0x00A6);  // What is 0x00A6?
const goodCode = LDA.abs(mem.abs.lily_state_0AA6); // Clear intent!

// The memory map provides structure and documentation
export const mem = {
  dp: {
    actor: {
      flags10: 0x10,
      animation_state: 0x12,
      // ... more fields
    }
  },
  abs: {
    lily_state_0AA6: 0x00A6,
    joypad_mask_std: 0xCFF0,
    // ... more addresses
  }
};
```

#### COP (Co-Processor) Commands  
```typescript
// Game-specific instructions are handled through COP commands
// Two modes: indexed (flexible) and named (type-safe)

// Indexed mode - for discovery and reverse engineering
COP[0x5A](byte(0x11), word(0x1234));

// Named mode - for production code (NOT YET IMPLEMENTED)
// COP.displayText(widestring_06B606);
// COP.jumpTable(word(0x0000), code_list_06B4FB);
```

**Key Insight**: The COP system provides a bridge between the standard 65c816 instruction set and game-specific opcodes. The indexed syntax allows for exploration, while the named syntax (when implemented) will provide type safety for known functions.

## 🎮 **Real-World Examples**

### Managing Complex State Machines

The `st68_lily.ts` file demonstrates how TSAL handles complex, stateful NPCs.

```typescript
// Forward declarations solve circular dependency problems
export let e_st68_lily: Code,
    code_list_06B4FB: SnesLayoutable,
    code_06B501: Code,
    code_06B51F: Code,
    code_06B571: Code;

// Entry point: state-based dispatcher
e_st68_lily = code(ctx => ctx.emit([
    LDA.abs(mem.abs.lily_state_0AA6),  // Load current state
    STA.dp(0x00),                      // Store for quick access
    COP[0xD9](word(0x0000), code_list_06B4FB), // Jump table based on state
]));

// Jump table maps states to handlers
code_list_06B4FB = pointerTable<Code>([
    code_06B501,  // State 0 handler
    code_06B51F,  // State 1 handler  
    code_06B571,  // State 2 handler
]);

// Individual state handlers
code_06B501 = code(ctx => ctx.emit([
    COP[0x25](byte(0x11), byte(0x1C)),  // Some game function
    LDA.imm(word(0xCFF0)),              // Load mask value
    TSB.abs(mem.joypad_mask_std),       // Set joypad mask bits
    COP[0xBF](widestring_06B606),       // Display text
    RTL(),                              // Return
]));
```

**Design Patterns Demonstrated:**
1. **Forward Declarations**: Solve circular dependencies while maintaining type safety
2. **State Machines**: Clean separation between dispatcher and handlers
3. **Type-Safe Pointer Tables**: Prevent accidental mixing of code/data pointers
4. **Named Memory Locations**: Self-documenting memory access

### Text and Data Management

TSAL provides structured approaches to handling game text and data.

```typescript
// Wide strings use template literals for readability
const dialogue = widestring`
  Lily: Welcome to the seaside tunnel!
  This passage leads to the next area.
`;

// Data tables are type-safe and self-documenting
const spriteData = dataTable<SpriteFrame>([
  { x: 16, y: 16, tile: 0x20, palette: 0 },
  { x: 32, y: 16, tile: 0x21, palette: 0 },
  { x: 16, y: 32, tile: 0x30, palette: 0 },
]);

// Pointer tables ensure type correctness
const codePointers = pointerTable<Code>([
  initRoutine,
  updateRoutine, 
  cleanupRoutine
]);
```

**Key Insight**: TSAL's type system prevents common bugs like accidentally putting a sprite pointer in a code pointer table, or mixing 8-bit and 16-bit values inappropriately.

## 🔧 **Advanced Patterns**

### Module Layout Management

Every TSAL module explicitly defines its layout, eliminating implicit ordering bugs.

```typescript
// Explicit layout prevents ordering bugs common in traditional assembly
export const layout = code(ctx => ctx.emit([
    // Header/entry points first
    h_st68_lily,
    e_st68_lily,
    
    // Jump tables and data structures
    code_list_06B4FB,
    
    // Code blocks in dependency order
    code_06B501,
    code_06B51F,
    code_06B571,
    
    // Data at the end
    widestring_06B606,
    spriteData_06B620,
]));
```

### Control Flow Abstractions (Future)

TSAL's foundation enables powerful control flow abstractions that would be difficult or error-prone in traditional assembly.

```typescript
// Hypothetical high-level constructs built on TSAL primitives
const conditionalCode = ifEqual(
  // Then block
  code(ctx => ctx.emit([
    LDA.imm(byte(1)),
    STA.dp(mem.dp.success_flag)
  ])),
  // Else block  
  code(ctx => ctx.emit([
    LDA.imm(byte(0)),
    STA.dp(mem.dp.success_flag)
  ]))
);

const loopCode = whileNonZero(
  // Condition
  code(ctx => ctx.emit([DEX()])),
  // Body
  code(ctx => ctx.emit([
    LDA.abs_x(dataArray),
    STA.abs_x(outputArray)
  ]))
);
```

**Key Insight**: These abstractions completely eliminate common assembly bugs like forgetting branch instructions or creating infinite loops, while maintaining the performance characteristics of hand-optimized assembly.

## 📋 **Current vs. Traditional Assembly**

### Traditional Assembly Challenges
```asm
; Prone to typos, no autocompletion
lda lily_state_0AA6
sta $00

; Easy to use wrong addressing mode
lda #$1234,x        ; ERROR: immediate mode doesn't take index

; Jump tables require manual address calculation
jmp (jump_table,x)
jump_table:
  .addr handler1
  .addr handler2
  .addr handler3    ; Easy to get count wrong
```

### TSAL Solutions
```typescript
// Autocompletion prevents typos
LDA.abs(mem.abs.lily_state_0AA6),
STA.dp(0x00),

// Type system prevents impossible combinations
// LDA.imm_x(0x1234)  // Compile error: imm_x doesn't exist

// Type-safe pointer tables prevent mismatches
const jumpTable = pointerTable<Code>([
  handler1,
  handler2,
  handler3,  // Compiler ensures all entries are Code objects
]);
```

## 🎯 **Development Philosophy** 

TSAL's design reflects several core principles:

### 1. **Fail Fast, Fail Clearly**
```typescript
// Compile-time error instead of runtime bug
const bad = LDA.nonexistent(0x42);  // Error: nonexistent doesn't exist
```

### 2. **Discoverability Through IntelliSense**
```typescript
// Typing "LDA." shows all available addressing modes
LDA.  // IDE shows: imm, abs, dp, long, abs_x, abs_y, dp_x, etc.
```

### 3. **Self-Documenting Code**
```typescript
// Clear intent vs. magic numbers
LDA.abs(mem.abs.lily_state_0AA6)  // vs  LDA $00A6
```

### 4. **Composability**
```typescript
// Small pieces combine into larger structures
const npcBehavior = code(ctx => ctx.emit([
  checkPlayerDistance(),
  updateAnimation(),
  handleInteraction(),
]));
```

## 🚀 **Next Steps in Implementation**

Based on our current foundation, the immediate priorities are:

### 1. Complete COP Handler System
```typescript
// Transform this indexed syntax...
COP[0xBF](widestring_06B606)

// Into this named syntax...
COP.displayText(widestring_06B606)
```

### 2. Add Runtime String Parser
```typescript
// Support for parsing assembly strings at runtime
const instruction = asm("LDA #$42");  // Returns LDA.imm(0x42)
const block = asm(`
  LDA #$42
  STA $2000
  RTL
`);
```

### 3. Build ROM Disassembler
```typescript
// Read existing ROMs and convert to TSAL objects
const rom = await loadRom("game.smc");
const instructions = rom.disassemble(0x8000, 0x8100);
const data = rom.parseStructuredData(0x9000, gameSchema);
```

This progression maintains TSAL's hybrid philosophy: powerful when you need it, simple when you don't, and always type-safe. 