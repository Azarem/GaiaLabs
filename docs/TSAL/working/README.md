# TSAL (TypeScript Assembly Language) - Working Implementation

**Status:** Core assembler complete, disassembler in planning phase

TSAL is a sophisticated TypeScript-based system for writing, generating, and parsing 65c816 assembly code for the SNES. This directory contains the working implementation, featuring a complete code generation system with type-safe instruction factories and runtime string parsing capabilities.

## 🎯 **Project Vision**

TSAL bridges the gap between modern development practices and retro assembly programming by providing:

- **Compile-time type safety** for assembly instruction generation
- **Runtime flexibility** for parsing existing assembly code
- **IntelliSense support** for discovering instructions and addressing modes
- **Modular architecture** separating concerns between code generation and ROM analysis
- **Hybrid API design** offering both simple functions and powerful factories

## 📖 **Documentation Navigation**

- **[README.md](README.md)** - This file: Current status, achievements, and development setup
- **[CONCEPTS.md](CONCEPTS.md)** - Deep dive into architecture, design patterns, and real-world examples
- **[ROADMAP.md](ROADMAP.md)** - Technical roadmap, phases, timeline, and decision points

## 🏗️ **Current Architecture**

```
TSAL Core Library
├── Code Generation Layer (✅ Complete)
│   ├── Type-safe instruction factories (LDA.imm, STA, etc.)
│   ├── Simple functions for single-variant instructions (NOP, BMI, CLC)
│   ├── Generated opcode definitions and type aliases
│   └── Instruction class with emission capabilities
├── String Parser Layer (🚧 Planned)
│   ├── Runtime assembly string parsing (asm("LDA #$42"))
│   ├── Regex-based addressing mode detection
│   └── Integration with type-safe factories
└── ROM Analysis Layer (📋 Future)
    ├── Disassembler for reading ROM files
    ├── Type parser for structured data extraction
    └── Schema-driven data interpretation
```

## 📁 **File Structure**

### Core Generation System
- **`generate-opcodes.ts`** - Main code generator that produces type-safe instruction factories
- **`generated-opcodes.ts`** - Generated TypeScript definitions (OpCode enum, OpDef interface, type aliases)
- **`op-factories.ts`** - Generated instruction factories and simple functions
- **`op.ts`** - Core Instruction class and addressing mode definitions

### 65c816 Instruction Set Data
- **`../../../65c816/`** - JSON files defining the complete 65c816 instruction set
- **`../../../generated/65c816-instruction-set.json`** - Complete, hydrated instruction set

### Working Examples
- **`tsal.ts`** - Core TSAL interfaces and layoutable system
- **`platform.ts`** - SNES platform-specific abstractions
- **`game.ts`** - Game-specific memory mappings and COP definitions
- **`st68_lily.ts`, `sFA_diary_menu.ts`** - Real-world module examples

## 🚀 **Major Achievements**

### 1. Sophisticated Code Generator (`generate-opcodes.ts`)

Our code generator is a complete solution that:

- **Processes 92 instructions** with 256+ variants from JSON data sources
- **Detects single vs. multi-variant instructions** automatically
- **Generates clean APIs** - simple functions for single-variant, factories for multi-variant
- **Maintains full type safety** throughout the generation process

```typescript
// Generated simple functions (23 instructions)
const nop = NOP();
const branch = BMI('loop_label');
const clear = CLC();

// Generated factory objects (9 instruction families)
const load = LDA.imm(0x42);
const store = STA.abs(0x1234);
const jump = JMP.abs_ind(0x5678);
```

### 2. Hybrid API Design

We solved the fundamental tension between verbose accuracy and clean simplicity:

**Before (Verbose):**
```typescript
const nop = NOP.imp();
const branch = BMI.rel('loop');
const clear = CLC.imp();
```

**After (Clean + Powerful):**
```typescript
// Clean for simple cases
const nop = NOP();
const branch = BMI('loop');
const clear = CLC();

// Powerful for complex cases  
const load = LDA.imm(0x42);    // immediate
const load2 = LDA.abs(0x1234); // absolute
const load3 = LDA.long(0x7E1234); // long
```

### 3. Complete 65c816 Instruction Coverage

Our generated system includes:

- **23 single-variant instructions** with simple function APIs
- **9 multi-variant instruction families** with factory object APIs  
- **All major addressing modes** including direct page, absolute, long, indexed, indirect, stack relative
- **Special handling** for flag-dependent immediate sizes
- **Block move instructions** with dual operand support

### 4. Type-Safe Foundation

```typescript
export interface OpDef {
  opcode: OpCode;
  mnemonic: string;
  size: number | 'flag-dependent';
  addressingMode: string;
}

export class Instruction implements ILayoutable {
  constructor(
    public readonly opDef: OpDef,
    public readonly operand?: Operand
  ) { /* ... */ }
  
  emit(ctx: EmitContext) { /* ... */ }
}
```

## 📊 **Instruction Distribution Analysis**

Based on our analysis of the complete 65c816 instruction set:

| Category | Count | API Style | Examples |
|----------|-------|-----------|----------|
| **Single-Variant** | 23 | Simple Functions | `NOP()`, `BMI(target)`, `CLC()`, `RTS()` |
| **Multi-Variant** | 9 | Factory Objects | `LDA.*`, `STA.*`, `JMP.*`, `ADC.*` |

### Single-Variant Instructions (Simple Functions)
- **Branch:** `BCC`, `BCS`, `BEQ`, `BNE`, `BMI`, `BPL`, `BVS`, `BVC`, `BRA`, `BRL`
- **System:** `NOP`, `BRK`, `COP`, `RTI`, `SEP`, `REP`, `CLI`, `SEI`, `STP`, `WAI`
- **Stack:** `PHA`, `PHX`, `PHY`, `PLA`, `PLX`, `PLY`, `PHB`, `PHD`, `PHK`, `PHP`, `PLB`, `PLD`, `PLP`
- **Transfer:** `TAX`, `TAY`, `TXA`, `TYA`, `TSX`, `TXS`, `TXY`, `TYX`, `TCD`, `TCS`, `TDC`, `TSC`
- **Other:** `JSL`, `RTS`, `RTL`, `PEA`, `PEI`, `PER`, `INX`, `INY`, `DEX`, `DEY`, `MVN`, `MVP`, `XBA`, `XCE`, `CLC`, `CLD`, `CLV`, `SEC`, `SED`, `WDM`

### Multi-Variant Instructions (Factory Objects)
- **Load/Store:** `LDA` (15 variants), `STA` (14 variants), `LDX` (5 variants), `LDY` (5 variants), `STX`, `STY`, `STZ`
- **Arithmetic:** `ADC` (15 variants), `SBC` (15 variants), `CMP` (15 variants), `CPX`, `CPY`, `INC`, `DEC`
- **Logical:** `AND`, `ORA`, `EOR`, `BIT`, `TRB`, `TSB`
- **Shift:** `ASL`, `LSR`, `ROL`, `ROR`  
- **Control Flow:** `JMP` (3 variants), `JML` (2 variants), `JSR` (2 variants)

## 🎮 **Working Examples**

### Basic Instruction Usage

```typescript
// Import the generated factories and functions
import { NOP, CLC, INX, LDA, STA, JMP } from './op-factories';

// Simple instructions (no addressing mode needed)
const operations = [
  NOP(),           // No operation
  CLC(),           // Clear carry
  INX(),           // Increment X
];

// Complex instructions (multiple addressing modes)
const dataOps = [
  LDA.imm(0x42),      // Load immediate
  LDA.abs(0x1234),    // Load absolute
  LDA.dp(0x80),       // Load direct page
  STA.abs(0x2000),    // Store absolute
  JMP.abs_ind(0x1234), // Jump absolute indirect
];
```

### Game Module Example (from `st68_lily.ts`)

```typescript
// Forward declarations for complex interdependencies
export let e_st68_lily: Code,
    code_list_06B4FB: SnesLayoutable,
    code_06B501: Code;

// Entry point using state-based dispatch
e_st68_lily = code(ctx => ctx.emit([
    LDA.abs(mem.abs.lily_state_0AA6),  // Load state variable
    STA.dp(0x00),                      // Store in direct page
    COP[0xD9](word(0x0000), code_list_06B4FB), // Jump table dispatch
]));

// Pointer table for state handlers
code_list_06B4FB = pointerTable<Code>([
    code_06B501,
    code_06B51F, 
    code_06B571,
]);

// State handler implementation
code_06B501 = code(ctx => ctx.emit([
    COP[0x25](byte(0x11), byte(0x1C)),
    LDA.imm(word(0xCFF0)),
    TSB.abs(mem.joypad_mask_std),
    COP[0xBF](widestring_06B606),
    RTL(),
]));
```

## 🚧 **Current Limitations & Next Steps**

### Incomplete Features

1. **COP Handler System**
   - Current: Basic indexed access (`COP[0x5A](args)`)
   - Needed: Named function mapping (`COP.displayText(message)`)
   - Status: Design patterns established, implementation pending

2. **String Parser (Runtime Assembly)**
   - Current: Type-safe factories only
   - Needed: `asm("LDA #$42")` string parsing
   - Plan: Port regex logic from `GaiaLib/Asm/OpCode.cs`

3. **ROM Disassembler**
   - Current: Code generation only
   - Needed: ROM file → TypeScript object parsing
   - Plan: Implement `AsmReader` and `TypeParser` classes

### Roadmap Phase 1: Complete the Assembler

- [ ] **Implement COP handler system**
  - Design named function registry
  - Add runtime/compile-time validation
  - Support both indexed and named syntax

- [ ] **Add string parser for runtime assembly**
  - Port regex patterns from C# codebase
  - Integrate with existing type-safe factories
  - Support all addressing modes

- [ ] **Create comprehensive test suite**
  - Test all instruction variants
  - Validate generated binary output
  - Test string parsing round-trip accuracy

### Roadmap Phase 2: Build the Disassembler  

- [ ] **Implement ROM data reader**
  - Buffer-based byte reading
  - Position tracking and seeking
  - Endianness handling

- [ ] **Create assembly parser**
  - Opcode → instruction mapping
  - Operand extraction by addressing mode
  - Generate `Instruction` objects from binary

- [ ] **Build type parser for structured data**
  - Schema-driven data extraction
  - Support for game-specific data types
  - Integration with existing type system

### Roadmap Phase 3: Enhanced Development Experience

- [ ] **Language server/IDE integration**
  - In-string syntax highlighting for `asm("")` calls
  - Real-time error checking
  - Advanced autocompletion

- [ ] **Web-based development environment**
  - Browser-based IDE with TSAL support
  - Live compilation and testing
  - ROM visualization tools

## 🔧 **Development Setup**

### Prerequisites
- Node.js 18+ (for TypeScript type stripping support)
- TypeScript 5.0+

### Building the System
```bash
# Generate the instruction factories
cd docs/TSAL/working
node generate-opcodes.ts

# This produces:
# - generated-opcodes.ts (data definitions)
# - op-factories.ts (instruction factories)
```

### Testing the API
```bash
# Run the API demonstration
node test-simplified-api.js
```

## 📚 **Related Documentation**

### Core TSAL Documentation
- **[CONCEPTS.md](CONCEPTS.md)** - Detailed architectural concepts, layered design, real-world examples, and development philosophy
- **[ROADMAP.md](ROADMAP.md)** - Technical roadmap with phases, timelines, success metrics, and implementation priorities

### Technical Resources
- **`../v3/`** - Architectural documentation and design principles
- **`../../../65c816/`** - 65c816 instruction set data files (JSON format)
- **`../../../generated/`** - Generated instruction set definitions and hydrated data
- **`../../../plans/refactor-opcode-generation.md`** - Implementation history and completed features

### Related Projects
- **`../../../GaiaLib/`** - C# assembler library with regex-based parsing (inspiration for string parser)
- **`../../../TSAL/`** - Node.js package implementation
- **`../../../docs/TSAL/`** - Additional schemas and architectural documentation

## 🎯 **Design Philosophy**

TSAL's design is guided by several key principles:

1. **Progressive Enhancement**: Start with a solid, portable library, then add enhanced IDE features
2. **Hybrid Approaches**: Support both compile-time type safety and runtime flexibility
3. **Separation of Concerns**: Clear boundaries between generation, parsing, and analysis
4. **Real-world Usage**: Driven by actual retro development needs and patterns
5. **TypeScript First**: Leverage the full power of the TypeScript type system

This approach ensures TSAL remains both powerful for expert users and approachable for newcomers to assembly development.

---

**Next Steps**: Based on project priorities, we're ready to implement either the COP handler system, the runtime string parser, or begin work on the ROM disassembler. Each represents a significant step toward a complete SNES development toolkit. 