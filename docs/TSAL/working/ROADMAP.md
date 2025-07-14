# TSAL Technical Roadmap

**Current Status**: Core assembler complete with sophisticated instruction generation system  
**Next Phase**: Complete assembler features and begin disassembler implementation

This roadmap outlines the specific technical steps to evolve TSAL from its current state into a complete SNES development toolkit.

## ✅ **Phase 0: Foundation (COMPLETE)**

### Accomplished Features

- [x] **65c816 Instruction Set Implementation** (92 instructions, 256+ variants)
- [x] **Sophisticated Code Generator** (`generate-opcodes.ts`)
- [x] **Hybrid API Design** (simple functions + factory objects)
- [x] **Type-Safe Instruction Creation** (`Instruction` class, `OpDef` interface)
- [x] **Complete Addressing Mode Support** (25 modes including complex indirect variants)
- [x] **Working Examples** (real-world game modules)

### Key Architecture Decisions Made

1. **Hybrid API Approach**: 23 single-variant instructions get simple functions, 9 multi-variant instruction families get factory objects
2. **Type-Safe Generation**: All code generation maintains TypeScript type safety throughout
3. **Modular JSON Data Sources**: Instruction definitions stored as maintainable JSON files
4. **Runtime + Compile-time Support**: Foundation laid for both type-safe factories and runtime string parsing

## 🚧 **Phase 1: Complete Assembler (HIGH PRIORITY)**

### 1.1 COP Handler System
**Goal**: Transform game-specific COP commands from indexed access to named functions

#### Current State
```typescript
// Only indexed access available
COP[0xBF](widestring_06B606);
COP[0xD9](word(0x0000), code_list_06B4FB);
```

#### Target State  
```typescript
// Named access with full type safety
COP.displayText(widestring_06B606);
COP.jumpTable(word(0x0000), code_list_06B4FB);

// Both syntaxes supported for flexibility
COP[0xBF](widestring_06B606);  // Still works for unknown opcodes
```

#### Implementation Plan
1. **Create COP Registry System**
   - `COPRegistry` class to map opcodes to names and signatures
   - JSON configuration for COP command definitions
   - Runtime registration of known COP functions

2. **Generate COP Factory**
   - Extend `generate-opcodes.ts` to handle COP definitions
   - Generate typed methods for known COP commands
   - Maintain indexed access for unknown/experimental opcodes

3. **Integration with Game Schema**
   - Game-specific COP definitions in `game.ts`
   - Type-safe parameter validation
   - Documentation generation for COP commands

#### Acceptance Criteria
- [ ] Both `COP.displayText()` and `COP[0xBF]()` syntax work
- [ ] Type-safe parameters for known COP commands
- [ ] Runtime validation for unknown COP commands
- [ ] Comprehensive test coverage for COP system

### 1.2 Runtime String Parser
**Goal**: Support `asm("LDA #$42")` string parsing that integrates with type-safe factories

#### Implementation Plan
1. **Port Regex Logic from GaiaLib**
   ```csharp
   // From GaiaLib/Asm/OpCode.cs - port these patterns to TypeScript
   [GeneratedRegex("^#(\\$[A-Fa-f0-9]{2,4}|\\$?[&^*][A-Za-z0-9-+_]+)$")]
   private static partial Regex ImmediateRegex();
   
   [GeneratedRegex("^\\$([A-Fa-f0-9]{4}|&[A-Za-z0-9-+_]+)$")]
   private static partial Regex AbsoluteRegex();
   ```

2. **Create Instruction Parser**
   ```typescript
   class InstructionParser {
     parse(assemblyString: string): Instruction {
       // Parse mnemonic and operand
       // Detect addressing mode using regex
       // Call appropriate factory method
       // Return Instruction object
     }
   }
   ```

3. **Integrate with Factory System**
   ```typescript
   // asm() function that calls the appropriate factory
   function asm(instruction: string): Instruction {
     const parsed = parser.parse(instruction);
     // Uses the existing factory system internally
     return LDA.imm(parsed.operand); // example
   }
   ```

#### Acceptance Criteria
- [ ] `asm("LDA #$42")` returns equivalent of `LDA.imm(0x42)`
- [ ] Support for all 25 addressing modes
- [ ] Proper error handling for invalid assembly strings
- [ ] Round-trip accuracy: `asm(instruction.toString()) === instruction`

### 1.3 Enhanced Testing & Validation
**Goal**: Comprehensive test suite ensuring correctness of generated code

#### Testing Framework
1. **Instruction Generation Tests**
   - Verify all 256+ instruction variants generate correct opcodes
   - Test addressing mode detection accuracy
   - Validate operand size handling

2. **String Parser Tests**
   - Round-trip testing for all instruction formats
   - Error handling validation
   - Edge case coverage (symbols, labels, etc.)

3. **Integration Tests**
   - Full module compilation tests
   - Binary output validation against known-good ROMs
   - Performance benchmarks

## 🔄 **Phase 2: ROM Analysis (MEDIUM PRIORITY)**

### 2.1 ROM Data Reader
**Goal**: Low-level ROM file parsing infrastructure

#### Implementation Plan
```typescript
class RomDataReader {
  constructor(private buffer: Buffer) {}
  
  get position(): number;
  set position(value: number);
  
  readByte(): number;
  readWord(): number; // Little-endian
  readLong(): number; // 24-bit address
  
  seek(position: number): void;
  slice(start: number, length: number): Buffer;
}
```

### 2.2 Assembly Disassembler  
**Goal**: Convert ROM bytes back to `Instruction` objects

#### Implementation Plan
1. **Opcode Lookup Table**
   ```typescript
   // Generated map from opcode -> OpDef
   const opcodeMap: Map<number, OpDef> = new Map([
     [0xA9, LDA_imm_OpDef],
     [0xAD, LDA_abs_OpDef],
     // ... all 256 opcodes
   ]);
   ```

2. **Disassembler Class**
   ```typescript
   class AsmDisassembler {
     disassemble(reader: RomDataReader, length: number): Instruction[] {
       // Read opcode byte
       // Look up OpDef  
       // Parse operands based on addressing mode
       // Create Instruction objects
     }
   }
   ```

### 2.3 Structured Data Parser
**Goal**: Parse game-specific data structures using schemas

#### Based on GaiaLib Architecture
```typescript
// Port concepts from TypeParser.cs
class TypeParser {
  parseType(typeName: string, position: number): any {
    // Handle different data types
    // Support for structs, strings, addresses
    // Schema-driven parsing
  }
}
```

## 🎯 **Phase 3: Enhanced Development Experience (LOW PRIORITY)**

### 3.1 Language Server Protocol
**Goal**: In-editor support for assembly strings

#### Features
- Syntax highlighting inside `asm("")` strings
- Real-time error checking
- Autocompletion for mnemonics and addressing modes
- Hover documentation for instructions

### 3.2 Web-Based IDE Integration
**Goal**: Browser-based development environment

#### Features
- Live compilation and testing
- ROM visualization tools
- Interactive debugging
- Project template system

## 📋 **Decision Points & Architecture Questions**

### 1. COP System Design
**Question**: Should COP commands be generated from JSON or manually defined?

**Options**:
- A. JSON-driven generation (consistent with main instruction set)
- B. Manual TypeScript definitions (more flexible for complex signatures)
- C. Hybrid approach (simple commands from JSON, complex ones manual)

**Recommendation**: Start with option A for consistency, migrate to C as complexity demands.

### 2. String Parser Error Handling
**Question**: How should invalid assembly strings be handled?

**Options**:
- A. Throw exceptions (fail fast)
- B. Return error objects (Railway-oriented programming)
- C. Log warnings and attempt graceful fallbacks

**Recommendation**: Option A for development, Option B for production use.

### 3. ROM Analysis Integration
**Question**: Should the disassembler output the same `Instruction` objects as the assembler?

**Options**:
- A. Identical objects (perfect symmetry)
- B. Enhanced objects with metadata (position, original bytes, etc.)
- C. Separate object hierarchies

**Recommendation**: Option B - enhanced objects that extend the base `Instruction` interface.

## 🎖️ **Success Metrics**

### Phase 1 Success Criteria
- [ ] Complete SNES project can be built using only TSAL
- [ ] String parser handles 100% of valid 65c816 assembly syntax
- [ ] COP system supports both discovery and production workflows
- [ ] Test suite covers all instruction variants and edge cases

### Phase 2 Success Criteria  
- [ ] Can disassemble existing SNES ROMs to readable TSAL code
- [ ] Round-trip accuracy: ROM → TSAL → ROM produces identical output
- [ ] Structured data parsing handles complex game data formats

### Phase 3 Success Criteria
- [ ] Language server provides rich editing experience
- [ ] Web IDE enables productive SNES development
- [ ] Community adoption and contribution

## 🗓️ **Timeline Estimation**

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| **Phase 1.1** (COP System) | 2-3 weeks | Current foundation |
| **Phase 1.2** (String Parser) | 3-4 weeks | COP system |  
| **Phase 1.3** (Testing) | 1-2 weeks | String parser |
| **Phase 2.1** (ROM Reader) | 1-2 weeks | Phase 1 complete |
| **Phase 2.2** (Disassembler) | 4-5 weeks | ROM reader |
| **Phase 2.3** (Type Parser) | 3-4 weeks | Disassembler |
| **Phase 3** (Enhanced UX) | 6-8 weeks | Phase 2 complete |

**Total Estimated Timeline**: 20-28 weeks for complete implementation

This roadmap provides a clear path from our current sophisticated foundation to a complete, production-ready SNES development toolkit. 