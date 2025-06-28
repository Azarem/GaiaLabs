# **TypeScript Assembly Language (TSAL) Concept Guide**
## Building Type-Safe Assembly Code for ROM Hacking

---

## **Table of Contents**
1. [Overview & Philosophy](#overview--philosophy)
2. [Core Syntax & Symbols](#core-syntax--symbols)
3. [Memory Addressing System](#memory-addressing-system)
4. [Struct System & Database Integration](#struct-system--database-integration)
5. [String Type System](#string-type-system)
6. [File Import & Cross-Reference System](#file-import--cross-reference-system)
7. [Instruction Set & Mnemonics](#instruction-set--mnemonics)
8. [Database-Driven Type Generation](#database-driven-type-generation)
9. [IDE Integration & Developer Experience](#ide-integration--developer-experience)
10. [Compilation & Code Generation](#compilation--code-generation)
11. [Advanced Features](#advanced-features)
12. [Implementation Roadmap](#implementation-roadmap)

---

## **Overview & Philosophy**

### **Core Principles**

**TypeScript Assembly Language (TSAL)** is a revolutionary approach to writing assembly code that combines the **visual clarity and performance control of assembly** with the **type safety and modern tooling of TypeScript**.

#### **Design Goals**
- ✅ **Assembly Familiarity**: Syntax that ROM hackers immediately recognize
- ✅ **Type Safety**: Compile-time error detection for memory access, register usage, and data types
- ✅ **Database-Driven**: All game-specific types and memory maps generated from JSON configurations
- ✅ **Visual Clarity**: Maintain assembly's scannable, aligned structure
- ✅ **IDE Integration**: Full autocomplete, error checking, and refactoring support
- ✅ **Community-Friendly**: Easy sharing of typed assembly libraries and macros

#### **Key Benefits**
1. **Catch Errors Early**: Type system prevents common assembly mistakes at compile time
2. **Intelligent Autocomplete**: IDE shows valid addressing modes, memory locations, and struct fields
3. **Refactoring Safety**: Rename memory locations and labels across entire codebase
4. **Community Libraries**: Share type-safe assembly macros and game-specific utilities
5. **Documentation Integration**: Hover tooltips show instruction effects, cycle counts, and memory layouts
6. **Cross-Platform**: Extensible to any CPU architecture through database definitions

---

## **Core Syntax & Symbols**

### **The Three Pillars: `$`, `#`, and `@`**

TSAL introduces three special symbols that provide type-safe access to assembly concepts:

#### **`$` - Memory Addressing**
The dollar sign creates **type-safe memory addresses** with intelligent addressing mode detection.

```typescript
import { LDA, STA, $, X, Y } from 'c65816';

// Basic addressing
LDA($(0x8000));           // LDA $8000 (absolute)
LDA($(0x80));             // LDA $80 (direct page, auto-detected)
STA($(0x7F0200));         // STA $7F0200 (long addressing)

// Indexed addressing  
LDA($(0x8000)[X]);        // LDA $8000,X (absolute indexed)
LDA($(0x80)[Y]);          // LDA $80,Y (direct page indexed)

// Banking support
LDA($(0x7F, 0x0200));     // LDA $7F0200 (explicit bank:offset)
LDA($(0x7F, 0x0200)[X]);  // LDA $7F0200,X (banked indexed)

// Indirect addressing
LDA($(0x80).indirect);        // LDA ($80) (indirect)
LDA($(0x80).indirect[Y]);     // LDA ($80),Y (indirect indexed)
LDA($(0x8000).indirect.long); // LDA [$8000] (indirect long)
```

**Type Safety Features:**
```typescript
// Compiler prevents invalid operations
STA($(0x2100));           // ✅ Valid - writing to PPU register
LDA($(0x2100));           // ✅ Valid - reading PPU register
BRA($(0x8000));           // ❌ Error - can't branch to data address
STA($X);                  // ❌ Error - can't store to register
```

#### **`#` - Immediate Values**
The hash symbol creates **type-safe immediate values** matching traditional assembly syntax.

```typescript
// Immediate values - exactly like assembly
LDA(#0x42);               // LDA #$42
LDX(#0);                  // LDX #$00
CPY(#255);                // CPY #$FF

// Size validation
LDA(#0x1234);             // ✅ Valid for 16-bit accumulator
LDA(#0x12345);            // ❌ Error - value too large for 16-bit

// Type checking prevents mistakes
STA(#42);                 // ❌ Error - can't store to immediate
BRA(#100);                // ❌ Error - can't branch to immediate
```

#### **`@` - Structs, Strings, and File Imports**
The at symbol provides **database-driven type safety** for complex data structures.

```typescript
// Struct constructors (generated from database)
@h_actor(#0x0C, #0x00, #0x10)           // h_actor < #0C, #00, #10 >
@sprite_part(#0x10, #0x20, #0x30, #0x40, #0x50, #0x1234)

// String types (generated from database)  
@WideString`Hello [LU1:BB] World![END]`  // Type-safe game text
@ASCIIString`Menu text here`             // Validates character map

// File imports (cross-reference system)
import actors from @scene_actors;         // Import assembly definitions
import maps from @map_data;               // Import map data
```

---

## **Memory Addressing System**

### **Intelligent Address Resolution**

TSAL automatically detects the most efficient addressing mode based on the target address and CPU context.

```typescript
interface AddressResolution {
  // Automatic mode detection
  $(0x80):      DirectPage;      // $80 (2 bytes, faster)
  $(0x8000):    Absolute;        // $8000 (3 bytes)
  $(0x7F0200):  Long;            // $7F0200 (4 bytes)
  
  // Manual override when needed
  $(0x80).absolute:  Absolute;   // Force $0080 (3 bytes)
  $(0x8000).direct:  Error;      // Invalid - address too large
}
```

### **Game-Specific Memory Maps**

Memory maps are **auto-generated from game databases**, providing intelligent autocomplete and validation.

```typescript
// Generated from Illusion of Gaia database
interface IoGMemoryMap {
  // Player data
  player_x_pos:     0x09A2;  // "Player X coordinate"
  player_y_pos:     0x09A4;  // "Player Y coordinate"  
  player_health:    0x09A6;  // "Player health points"
  player_flags:     0x09AE;  // "Player status flags"
  
  // Game state
  scene_current:    0x0644;  // "Current scene ID"
  joypad_state:     0x065A;  // "Controller input state"
  inventory_slots:  0x0AB4;  // "Inventory data array"
  
  // PPU registers
  ppu: {
    INIDISP:        0x2100;   // "Screen display register"
    BGMODE:         0x2105;   // "Background mode register"
    BG1SC:          0x2107;   // "BG1 tilemap register"
  };
}

// Usage with full IDE support
const mem = useMemoryMap<IoGMemoryMap>();

LDA($(mem.player_health));      // IDE shows: "Player health points"
STA($(mem.scene_current));      // Auto-validates address usage
LDA($(mem.ppu.INIDISP));        // Structured access to PPU registers
```

### **Type-Safe Array Access**

```typescript
// Array-like memory structures
const enemyArray = $(mem.enemy_data);
const spriteTable = $(mem.sprite_oam);

// Type-safe indexing
LDA(enemyArray[X].health);       // enemyArray + X + health_offset
STA(spriteTable[Y].x_pos);       // spriteTable + Y + x_pos_offset

// Bounds checking (optional)
LDA(enemyArray[#16].health);     // ❌ Error - index exceeds max enemies
```

---

## **Struct System & Database Integration**

### **Discriminator-Based Polymorphic Structs**

TSAL supports complex **discriminated union types** that automatically resolve based on a discriminator byte, exactly matching your ROM's data structures.

#### **Basic Struct Definition**
```typescript
// From structs.json database
interface StructDefinitions {
  // Simple structs
  h_actor(id: Byte, type: Byte, flags: Byte): HActorStruct;
  h_thinker(type: Byte, data: Byte): HThinkerStruct;
  sprite_part(x: Byte, y: Byte, tile: Byte, attr: Byte, pal: Byte, data: Word): SpritePartStruct;
  
  // Delimited structs (terminated by specific value)
  motion(dir: Byte, speed: Byte, duration: Byte, flags: Byte): MotionStruct; // delimiter: 255
  dma_data(mode: Byte, src: Byte, dst: Byte): DmaDataStruct; // delimiter: 0
}
```

#### **Polymorphic Structs with Discriminators**
```typescript
// Meta command system (discriminator-based)
type MetaCommand = 
  | PPUCommand      // discriminator: 2
  | BitmapCommand   // discriminator: 3
  | PaletteCommand  // discriminator: 4
  | TilesetCommand  // discriminator: 5
  | TilemapCommand  // discriminator: 6
  | SpritemapCommand // discriminator: 16
  | MusicCommand    // discriminator: 17;

// Dynamic struct creation
@meta(#2, #0x80);                    // Auto-resolves to PPUCommand
@meta(#3, #1, #2, #3, binaryRef, #4); // Auto-resolves to BitmapCommand

// Specific constructors for type safety
@meta.ppu(#0x80);                    // Explicit PPU command
@meta.bitmap(#1, #2, #3, binaryRef, #4); // Explicit bitmap command
```

#### **Exclusionary Discriminator Logic**
```typescript
// Context-dependent discriminator resolution
type EnemyType =
  | ActorEnemy     // discriminator: 0 (within enemy context)
  | FullEnemy;     // discriminator: 6

// Same discriminator, different meaning based on context
@enemy(#0, actorData);    // Creates ActorEnemy (discriminator 0 in enemy context)
@meta(#0, ...);           // Creates different type (discriminator 0 in meta context)
```

### **Struct Usage in Assembly**

```typescript
// Creating and using structs
const initializePlayer = () => {
  // Create typed actor data
  const playerActor = @h_actor(#0x01, #0x10, #0xFF);
  
  // Store struct data to memory
  LDA(#playerActor.id);
  STA($(mem.player_data).id);
  
  LDA(#playerActor.type);  
  STA($(mem.player_data).type);
  
  // Process meta commands
  const ppuSetup = @meta.ppu(#0x0F);
  LDA(#ppuSetup.value);
  STA($(mem.ppu.INIDISP));
};
```

### **Runtime Struct Matching**

```typescript
// Pattern matching for dynamic struct processing
const processMetaCommand = (commandAddr: Address) => {
  LDA(commandAddr);  // Load discriminator
  
  // TypeScript switch with type narrowing
  const discriminator = loadByte(commandAddr);
  switch (discriminator.value) {
    case 2: {
      // TypeScript knows this is PPUCommand
      const cmd = @meta.ppu(loadByte(commandAddr.offset(1)));
      return processPPU(cmd);
    }
    case 3: {
      // TypeScript knows this is BitmapCommand
      const cmd = @meta.bitmap(
        loadByte(commandAddr.offset(1)),
        loadByte(commandAddr.offset(2)),
        loadByte(commandAddr.offset(3)),
        loadBinary(commandAddr.offset(4)),
        loadByte(commandAddr.offset(5))
      );
      return processBitmap(cmd);
    }
  }
};
```

---

## **String Type System**

### **Database-Driven String Types**

String types are **automatically generated** from your game's string encoding definitions, providing full character map validation and encoding support.

#### **String Type Definitions**
```typescript
// Generated from stringTypes.json
interface StringTypes {
  // ASCII string type
  ASCIIString: {
    delimiter: "|";
    terminator: 0;
    characterMap: [/* 256 character mappings */];
    maxLength?: number;
  };
  
  // Wide string type (for dialogue)
  WideString: {
    delimiter: "`"; 
    terminator: 202;
    greedyTerminator: true;
    characterMap: [/* game-specific characters */];
    commands: [/* text commands like [LU1:BB] */];
  };
  
  // Sprite string type (for UI text)
  SpriteString: {
    delimiter: "~";
    terminator: 202;
    shiftType: "wh2";
    characterMap: [/* limited character set */];
  };
}
```

#### **Type-Safe String Creation**
```typescript
// Multi-line strings with validation
const dialogue = @WideString`
  [TPL:A][TPL:3]Erik: The sun is [LU2:95]
  bright. I [LU1:BB]
  noticed [LU1:D7]before.[PAL:0][END]
`;

const menuText = @ASCIIString`
  Health: ${playerHP}
  Magic:  ${playerMP}
`;

const spriteLabel = @SpriteString`PRESS START`;

// Compile-time validation
const invalidString = @ASCIIString`
  This has ♥ invalid chars  // ❌ Error: ♥ not in ASCIIString character map
`;

const tooLong = @SpriteString`
  This string exceeds the maximum length for sprite strings
`; // ❌ Error: String too long for sprite display
```

#### **String Command Validation**
```typescript
// Game-specific text commands are validated
const validCommands = @WideString`
  [TPL:A]         // ✅ Valid template command
  [LU1:BB]        // ✅ Valid lookup command
  [PAL:0]         // ✅ Valid palette command  
  [END]           // ✅ Valid terminator
`;

const invalidCommands = @WideString`
  [INVALID:123]   // ❌ Error: Unknown command
  [LU1:999]       // ❌ Error: Lookup index out of range
`;
```

#### **String Interpolation with Type Safety**
```typescript
// Dynamic string generation with validation
const createStatusMessage = (health: number, magic: number) => @WideString`
  Player Status:
  HP: ${health.toString().padStart(3, '0')}   // Type-safe number conversion
  MP: ${magic.toString().padStart(3, '0')}
  [END]
`;

// Template-based string creation
const createDialogue = (characterName: string, message: string) => @WideString`
  [TPL:A][TPL:3]${characterName}: ${message}[PAL:0][END]
`;
```

---

## **File Import & Cross-Reference System**

### **Assembly File as TypeScript Modules**

TSAL treats **assembly files as first-class TypeScript modules**, enabling type-safe cross-file references and imports.

#### **File Structure Convention**
```
project/
├── actors/
│   ├── @player_actor.ts      # Player behavior code
│   ├── @enemy_ai.ts          # Enemy AI routines  
│   └── @npc_dialogue.ts      # NPC interaction code
├── maps/
│   ├── @world_map.ts         # World map data
│   └── @dungeon_layouts.ts   # Dungeon map data
├── graphics/
│   ├── @sprite_data.ts       # Sprite definitions
│   └── @tileset_data.ts      # Tileset definitions
└── strings/
    ├── @dialogue_text.ts     # Game dialogue
    └── @menu_text.ts         # UI text strings
```

#### **Cross-File Imports**
```typescript
// Import symbols from other assembly files
import actors from @player_actor;
import maps from @world_map;  
import dialogue from @dialogue_text;
import { enemyAI, bossAI } from @enemy_ai;

// Use imported labels and data
const initializeLevel = () => {
  JSR(actors.spawnPlayer);           // Call function from @player_actor
  JSR(maps.loadCurrentRoom);         // Call function from @world_map
  JSR(enemyAI.initializeEnemies);    // Call specific AI routine
  
  // Access imported data
  LDA(#maps.currentLevel.enemyCount);
  STA($(mem.active_enemies));
  
  // Use imported strings
  displayText(dialogue.welcomeMessage);
};
```

#### **Exported Symbols**
```typescript
// @player_actor.ts - exports functions, data, and constants
export const spawnPlayer = () => {
  LDA(#0x80);
  STA($(mem.player_x_pos));
  // ... player spawn logic
};

export const movePlayer = (deltaX: number, deltaY: number) => {
  LDA($(mem.player_x_pos));
  CLC();
  ADC(#deltaX);
  STA($(mem.player_x_pos));
  // ... movement logic
};

export const constants = {
  PLAYER_SPEED: 2,
  PLAYER_HEALTH: 100,
  JUMP_HEIGHT: 16
} as const;

export const playerData = {
  defaultStats: @h_actor(#0x01, #0x10, #0xFF),
  spawnLocation: { x: 128, y: 96 }
};
```

#### **Type-Safe Label References**
```typescript
// Labels are strongly typed and validated
export const playerMovementLoop = () => {
  const moveLoop = label('player_move_loop');
  const endLoop = label('player_move_end');
  
  here(moveLoop);
  JSR(checkInput);
  JSR(updatePosition);
  JSR(checkCollisions);
  BRA(moveLoop);
  
  here(endLoop);
  RTL();
};

// Import and use labels from other files
import { playerMovementLoop } from @player_actor;

const gameMainLoop = () => {
  JSR(playerMovementLoop);  // ✅ Type-safe function call
  JSR(updateEnemies);
  JSR(renderScreen);
  BRA(gameMainLoop);
};
```

---

## **Instruction Set & Mnemonics**

### **Direct Mnemonic Imports**

TSAL provides **direct imports of assembly mnemonics**, maintaining familiar syntax while adding type safety.

```typescript
// Import specific instructions
import { 
  LDA, STA, LDX, STX, LDY, STY,
  CMP, CPX, CPY, BIT, TSB, TRB,
  ADC, SBC, AND, ORA, EOR,
  ASL, LSR, ROL, ROR,
  BEQ, BNE, BCC, BCS, BPL, BMI, BVC, BVS,
  BRA, BRL, JMP, JSR, JSL, RTL, RTS,
  PHA, PLA, PHX, PLX, PHY, PLY, PHP, PLP,
  CLC, SEC, CLI, SEI, CLD, SED, CLV,
  NOP, WDM, STP, WAI, XBA, XCE,
  REP, SEP, COP, BRK, RTI
} from 'c65816';

// Import addressing helpers
import { $, #, X, Y, S } from 'c65816';

// Import special functions
import { label, here } from 'c65816';
```

### **Type-Safe Instruction Usage**

```typescript
// Basic instruction usage with type checking
const gameLoop = () => {
  // Load/Store operations
  LDA($(mem.player_health));      // LDA $player_health
  STA($(mem.temp_value));         // STA $temp_value
  
  // Immediate values
  LDX(#0);                        // LDX #$00
  LDY(#255);                      // LDY #$FF
  
  // Indexed addressing
  LDA($(mem.enemy_data)[X]);      // LDA enemy_data,X
  STA($(mem.sprite_oam)[Y]);      // STA sprite_oam,Y
  
  // Compare operations
  CMP(#100);                      // CMP #$64
  BEQ(playerDead);                // BEQ player_dead
  
  // Stack operations
  PHA();                          // PHA
  JSR(updateGraphics);            // JSR update_graphics
  PLA();                          // PLA
  
  // Status register manipulation
  REP(#0x20);                     // REP #$20 (16-bit accumulator)
  SEP(#0x10);                     // SEP #$10 (8-bit index registers)
};
```

### **Addressing Mode Validation**

```typescript
// Type system prevents invalid addressing modes
LDA($(0x8000));           // ✅ Valid - absolute addressing
LDA($(0x8000)[X]);        // ✅ Valid - absolute indexed
LDA($(0x80).indirect);    // ✅ Valid - indirect addressing

STA(#42);                 // ❌ Error - can't store to immediate
BRA($(0x8000)[X]);        // ❌ Error - can't branch to indexed address
JSR(#0x8000);             // ❌ Error - can't jump to immediate

// Addressing mode optimization suggestions
LDA($(0x80));             // IDE suggests: "Consider direct page addressing"
LDA($(0x80).absolute);    // Forces absolute: LDA $0080
```

### **CPU Mode Awareness**

```typescript
// Instructions are aware of current CPU state
const setAccumulatorMode = () => {
  REP(#0x20);             // 16-bit accumulator mode
  
  // Compiler knows accumulator is now 16-bit
  LDA(#0x1234);           // ✅ Valid - 16-bit immediate
  LDA(#0x12);             // ⚠️ Warning - using 8-bit value in 16-bit mode
  
  SEP(#0x20);             // 8-bit accumulator mode
  
  // Compiler knows accumulator is now 8-bit  
  LDA(#0x12);             // ✅ Valid - 8-bit immediate
  LDA(#0x1234);           // ❌ Error - 16-bit value in 8-bit mode
};
```

---

## **Database-Driven Type Generation**

### **Automatic Type Generation Pipeline**

TSAL automatically generates **TypeScript types and interfaces** from your game's JSON database files.

#### **Input: Game Database Files**
```
database/
├── config.json          # Game configuration
├── memory-map.json      # Memory layout definitions
├── structs.json         # Data structure definitions  
├── string-types.json    # Text encoding definitions
├── instructions.json    # CPU instruction set
└── symbols.json         # Named memory locations
```

#### **Generated: TypeScript Type Definitions**
```typescript
// Auto-generated from database files
export interface GeneratedTypes {
  // From memory-map.json
  MemoryMap: IoGMemoryMap;
  
  // From structs.json  
  Structs: {
    h_actor: StructFactory<[Byte, Byte, Byte]>;
    sprite_part: StructFactory<[Byte, Byte, Byte, Byte, Byte, Word]>;
    meta: PolymorphicStructFactory<MetaDiscriminators>;
  };
  
  // From string-types.json
  StringTypes: {
    ASCIIString: StringTypeFactory<ASCIICharacterMap>;
    WideString: StringTypeFactory<WideCharacterMap>;
    SpriteString: StringTypeFactory<SpriteCharacterMap>;
  };
  
  // From instructions.json
  Instructions: {
    [mnemonic: string]: InstructionFactory;
  };
}
```

### **Memory Map Generation**

```typescript
// Generated from memory-map.json
export interface IoGMemoryMap {
  // Direct page variables (fast access)
  directPage: {
    temp_byte_1:      0x00;    // Temporary storage
    temp_byte_2:      0x01;    // Temporary storage  
    temp_word:        0x02;    // 16-bit temporary
    dma_source:       0x04;    // DMA source address
    dma_dest:         0x07;    // DMA destination
  };
  
  // Game state (battery-backed)
  gameState: {
    player_health:    0x09A2;  // Current health
    player_mp:        0x09A4;  // Current magic points
    current_scene:    0x0644;  // Scene identifier
    game_flags:       0x0700;  // Progress flags array
  };
  
  // Hardware registers
  ppu: {
    INIDISP:          0x2100;  // Display control
    BGMODE:           0x2105;  // Background mode
    // ... all PPU registers
  };
  
  // Work RAM regions
  workRAM: {
    enemy_data:       0x7E1000; // Enemy array
    sprite_oam:       0x7E1200;  // OAM shadow
    level_data:       0x7E2000;  // Current level
  };
}
```

### **Struct Factory Generation**

```typescript
// Generated struct factories with full type safety
export function generateStructFactories(structDefs: StructDefinition[]): StructFactories {
  return structDefs.reduce((factories, def) => {
    if (def.discriminator !== undefined) {
      // Polymorphic struct with discriminator
      factories[def.name] = generatePolymorphicFactory(def);
    } else {
      // Simple struct
      factories[def.name] = generateSimpleFactory(def);
    }
    return factories;
  }, {} as StructFactories);
}

// Example generated factory
export const @h_actor = (
  id: ImmediateByte,
  type: ImmediateByte, 
  flags: ImmediateByte
): HActorStruct => {
  return new HActorStruct({
    id: id.value,
    type: type.value,
    flags: flags.value,
    size: 3,
    delimiter: null
  });
};
```

### **String Type Generation**

```typescript
// Generated string type factories
export function generateStringTypes(stringTypeDefs: StringTypeDefinition[]): StringTypeFactories {
  return stringTypeDefs.map(def => ({
    name: def.name,
    factory: createStringTypeFactory(def),
    validator: createStringValidator(def),
    encoder: createStringEncoder(def)
  }));
}

// Example generated string type
export const @WideString = createStringType({
  delimiter: "`",
  terminator: 202,
  characterMap: [
    "Ǫ", "į", "ņ", "ţ", "ę", "ť", // ... character mappings
  ],
  commands: {
    "TPL": { params: ["number"], description: "Template command" },
    "LU1": { params: ["number"], description: "Lookup table 1" },
    "PAL": { params: ["number"], description: "Palette change" },
    "END": { params: [], description: "String terminator" }
  }
});
```

---

## **IDE Integration & Developer Experience**

### **TypeScript Language Server Integration**

TSAL provides **first-class IDE support** through TypeScript Language Server integration.

#### **Intelligent Autocomplete**

```typescript
// Memory address autocomplete
LDA($(mem.  // Shows: player_health, player_mp, current_scene, etc.

// Instruction autocomplete with parameter hints
L    // Shows: LDA, LDX, LDY, LSR, etc.
LDA( // Shows: $(address), #(immediate), mem.symbol, etc.

// Struct constructor autocomplete  
@h_  // Shows: h_actor, h_thinker, etc.
@h_actor( // Shows: (id: ImmediateByte, type: ImmediateByte, flags: ImmediateByte)
```

#### **Real-Time Error Checking**

```typescript
// Type errors shown immediately
LDA(#0x12345);           // ❌ "Value 0x12345 too large for 16-bit immediate"
STA(#42);                // ❌ "Cannot store to immediate value"  
BRA($(mem.player_data)); // ❌ "Cannot branch to data address"

// Memory validation
LDA($(0x999999));        // ❌ "Address 0x999999 outside valid memory range"
STA($(mem.ppu.INIDISP)); // ⚠️ "Writing to PPU register - ensure proper timing"
```

#### **Hover Documentation**

```typescript
// Hovering over instructions shows documentation
LDA($(mem.player_health));
//  ↑ Hover shows: "Load Accumulator
//     Loads value from memory into accumulator
//     Affects: N, Z flags
//     Cycles: 3 (absolute addressing)"

// Hovering over memory locations shows context
$(mem.player_health)
//     ↑ Hover shows: "Player Health Points
//        Location: $09A2
//        Size: 2 bytes (word)
//        Range: 0-999"
```

#### **Refactoring Support**

```typescript
// Rename symbol across entire project
const OLD_PLAYER_HP = 0x09A2;
const NEW_PLAYER_HP = 0x09A4;  // Rename this...

// All references updated automatically:
LDA($(mem.player_health));     // ← Automatically updated
STA($(mem.player_health));     // ← Automatically updated
CMP($(mem.player_health));     // ← Automatically updated
```

### **Debugging Integration**

#### **Source Maps**

```typescript
// Generated assembly includes source mapping
/*
  TypeScript Source:    LDA($(mem.player_health));
  Generated Assembly:   LDA $09A2
  Source Map:          line 42, column 8 in player_movement.ts
  Debug Symbols:       player_health = $09A2
*/
```

#### **Breakpoint Support**

```typescript
const updatePlayer = () => {
  LDA($(mem.joypad_state));     // ← Breakpoint here maps to LDA $065A
  BIT(#0x80);                   // ← Breakpoint here maps to BIT #$80
  BEQ(skipMovement);            // ← Breakpoint here maps to BEQ skip_movement
  
  // Debugger shows both TypeScript and assembly views
};
```

### **Code Navigation**

#### **Go to Definition**

```typescript
// Click on any symbol to jump to definition
JSR(actors.spawnPlayer);        // ← Jumps to spawnPlayer function in actors file
LDA($(mem.player_health));      // ← Jumps to memory map definition
@h_actor(#1, #2, #3);          // ← Jumps to struct definition
```

#### **Find All References**

```typescript
// Find all uses of a memory location or function
$(mem.player_health)  // Shows all read/write operations across project
actors.spawnPlayer    // Shows all call sites across project
```

#### **Call Hierarchy**

```typescript
// View function call relationships
spawnPlayer()
├── Called by: gameInitialization()
├── Called by: respawnAfterDeath()
└── Calls: initializePlayerStats()
    ├── Calls: resetHealth()
    └── Calls: loadDefaultEquipment()
```

---

## **Compilation & Code Generation**

### **TypeScript to Assembly Translation**

TSAL compiles TypeScript assembly code into **native assembly output** while preserving all optimizations and structure.

#### **Compilation Pipeline**

```typescript
interface CompilationPipeline {
  // Phase 1: TypeScript parsing and validation
  parse: (source: string) => TypeScriptAST;
  validate: (ast: TypeScriptAST) => ValidationResult;
  
  // Phase 2: Assembly intermediate representation
  transform: (ast: TypeScriptAST) => AssemblyIR;
  optimize: (ir: AssemblyIR) => OptimizedAssemblyIR;
  
  // Phase 3: Target assembly generation
  generate: (ir: OptimizedAssemblyIR, target: CPUTarget) => AssemblyOutput;
  assemble: (assembly: AssemblyOutput) => MachineCode;
}
```

#### **Example Compilation**

**Input TypeScript:**
```typescript
const movePlayer = (deltaX: number, deltaY: number) => {
  LDA($(mem.player_x_pos));
  CLC();
  ADC(#deltaX);
  STA($(mem.player_x_pos));
  
  LDA($(mem.player_y_pos));
  CLC();
  ADC(#deltaY);
  STA($(mem.player_y_pos));
};

movePlayer(5, -3);
```

**Generated Assembly:**
```assembly
move_player:
    LDA $09A2        ; Load player X position
    CLC              ; Clear carry flag
    ADC #$05         ; Add deltaX (5)
    STA $09A2        ; Store new X position
    
    LDA $09A4        ; Load player Y position  
    CLC              ; Clear carry flag
    ADC #$FD         ; Add deltaY (-3, as $FD)
    STA $09A4        ; Store new Y position
    
    RTS              ; Return from subroutine
```

### **Optimization Strategies**

#### **Dead Code Elimination**

```typescript
// Input with unreachable code
const example = () => {
  LDA(#42);
  RTS();
  LDA(#100);        // ← Dead code, never reached
  STA($(mem.temp));
};

// Output - dead code removed
example:
    LDA #$2A
    RTS
```

#### **Constant Folding**

```typescript
// Input with compile-time constants
const PLAYER_SPEED = 3;
const JUMP_HEIGHT = 16;

LDA(#PLAYER_SPEED + JUMP_HEIGHT);  // Computed at compile time

// Output
LDA #$13    ; 3 + 16 = 19 = $13
```

#### **Addressing Mode Optimization**

```typescript
// Input - compiler chooses optimal addressing
LDA($(0x80));        // Automatically uses direct page
LDA($(0x8000));      // Automatically uses absolute  
LDA($(0x7F0200));    // Automatically uses long

// Output - optimal addressing modes
LDA $80         ; 2 bytes, faster
LDA $8000       ; 3 bytes
LDA $7F0200     ; 4 bytes, necessary for banking
```

### **Multi-Target Support**

#### **CPU Architecture Abstraction**

```typescript
interface CPUTarget {
  architecture: '6502' | '65816' | 'Z80' | 'ARM' | 'x86';
  addressingModes: AddressingMode[];
  registerSet: Register[];
  instructionSet: Instruction[];
  memoryModel: MemoryModel;
}

// Same TypeScript code can target different CPUs
const gameCode = compileFor('65816', sourceCode);  // SNES
const portedCode = compileFor('Z80', sourceCode);  // Game Boy
```

#### **Cross-Platform Assembly Generation**

```typescript
// TypeScript source (platform-agnostic)
const clearScreen = () => {
  LDA(#0);
  LDX(#0);
  const clearLoop = label();
  here(clearLoop);
  STA($(mem.videoRAM)[X]);
  INX();
  CPX(#0x400);
  BNE(clearLoop);
};

// Generated 65816 (SNES)
clear_screen:
    LDA #$00
    LDX #$0000
clear_loop:
    STA $7F0000,X
    INX
    CPX #$0400
    BNE clear_loop
    
// Generated 6502 (NES) - automatically adapted
clear_screen:
    LDA #$00
    LDX #$00
clear_loop:
    STA $2000,X
    INX
    CPX #$FF
    BNE clear_loop
```

---

## **Advanced Features**

### **Macro System**

TSAL supports **type-safe macros** that expand into inline assembly code.

```typescript
// Reusable typed macros
const waitForVBlank = () => [
  const waitLoop = label();
  here(waitLoop);
  LDA($(mem.ppu.RDNMI));    // Read NMI status
  BPL(waitLoop);            // Wait for VBlank
];

const copyMemory = (source: Address, dest: Address, size: number) => [
  LDX(#0);
  const copyLoop = label();
  here(copyLoop);
  LDA(source[X]);
  STA(dest[X]);
  INX();
  CPX(#size);
  BNE(copyLoop);
];

// Macro expansion
const gameCode = () => {
  ...waitForVBlank(),        // Expands inline
  ...copyMemory(
    $(mem.source_buffer), 
    $(mem.dest_buffer), 
    256
  );
};
```

### **Conditional Compilation**

```typescript
// Compile-time conditionals for different ROM versions
const initializeGame = () => {
  if (ROM_VERSION === 'US') {
    LDA(#0x01);             // US-specific initialization
    STA($(mem.region_flag));
  } else if (ROM_VERSION === 'JP') {
    LDA(#0x02);             // JP-specific initialization  
    STA($(mem.region_flag));
  }
  
  if (DEBUG_MODE) {
    JSR(debugInit);         // Only included in debug builds
  }
  
  // Always included
  JSR(commonInit);
};
```

### **Template System**

```typescript
// Generic assembly templates
const createAIRoutine = <T extends EnemyType>(
  enemyType: T,
  behavior: AIBehavior<T>
) => {
  return () => {
    // Type-safe AI routine generation
    LDA($(mem.enemies)[X].type);
    CMP(#enemyType.discriminator);
    BNE(nextEnemy);
    
    // Behavior-specific code generation
    ...behavior.generateCode(enemyType);
    
    const nextEnemy = label();
    here(nextEnemy);
  };
};

// Usage
const goblinAI = createAIRoutine(goblinType, aggressiveBehavior);
const slimeAI = createAIRoutine(slimeType, passiveBehavior);
```

### **Inline Assembly Blocks**

```typescript
// Mix high-level TypeScript with low-level assembly
const optimizedFunction = () => {
  // High-level TypeScript logic
  const playerData = loadPlayerStats();
  
  if (playerData.health < 10) {
    // Drop to raw assembly for performance-critical code
    asm`
      LDA $09A2        ; Load player health directly
      CMP #$0A         ; Compare with 10
      BCC critical     ; Branch if critical health
      RTS              ; Return if not critical
      
    critical:
      JSR play_alarm   ; Play alarm sound
      JSR flash_screen ; Flash screen red
    `;
  }
  
  // Back to high-level code
  updatePlayerDisplay();
};
```

---

## **Implementation Roadmap**

### **Phase 1: Core Language Features (Months 1-3)**

#### **Milestone 1.1: Basic Syntax (Month 1)**
- ✅ Implement `$`, `#`, and `@` symbol parsing
- ✅ Basic instruction imports (LDA, STA, etc.)
- ✅ Simple addressing mode detection
- ✅ Type-safe immediate values
- ✅ Label creation and reference system

#### **Milestone 1.2: Memory System (Month 2)**
- ✅ Memory map integration from JSON
- ✅ Indexed addressing with type checking
- ✅ Address validation and optimization
- ✅ Multi-bank memory support
- ✅ PPU register type definitions

#### **Milestone 1.3: Basic Compilation (Month 3)**
- ✅ TypeScript to assembly translation
- ✅ Source map generation for debugging
- ✅ Basic optimization passes
- ✅ Error reporting with line numbers
- ✅ Assembly output formatting

### **Phase 2: Database Integration (Months 4-6)**

#### **Milestone 2.1: Struct System (Month 4)**
- ✅ JSON to TypeScript type generation
- ✅ Simple struct constructors
- ✅ Struct validation and size calculation
- ✅ Nested struct support
- ✅ Array-like struct access

#### **Milestone 2.2: Polymorphic Structs (Month 5)**
- ✅ Discriminator-based type resolution
- ✅ Exclusionary discriminator logic
- ✅ Runtime type narrowing
- ✅ Pattern matching for struct processing
- ✅ Complex inheritance hierarchies

#### **Milestone 2.3: String System (Month 6)**
- ✅ String type generation from JSON
- ✅ Character map validation
- ✅ Multi-line string support with continuation
- ✅ Text command validation
- ✅ String interpolation with type safety

### **Phase 3: Advanced Features (Months 7-9)**

#### **Milestone 3.1: File Import System (Month 7)**
- ✅ Cross-file reference resolution
- ✅ Symbol export/import system
- ✅ Dependency graph analysis
- ✅ Circular dependency detection
- ✅ Module bundling for ROM building

#### **Milestone 3.2: IDE Integration (Month 8)**
- ✅ TypeScript Language Server integration
- ✅ Intelligent autocomplete with context
- ✅ Real-time error checking and validation
- ✅ Hover documentation with instruction details
- ✅ Refactoring support (rename, extract, etc.)

#### **Milestone 3.3: Debugging Support (Month 9)**
- ✅ Source map generation and debugging
- ✅ Breakpoint mapping between TS and assembly
- ✅ Variable inspection and memory viewing
- ✅ Call stack analysis
- ✅ Performance profiling integration

### **Phase 4: Community & Ecosystem (Months 10-12)**

#### **Milestone 4.1: Community Tools (Month 10)**
- ✅ Shared macro and library system
- ✅ Package manager for assembly modules
- ✅ Community repository for game databases
- ✅ Version control integration
- ✅ Collaboration features

#### **Milestone 4.2: Multi-Platform Support (Month 11)**
- ✅ Additional CPU architecture support (Z80, 6502)
- ✅ Cross-platform compilation pipeline
- ✅ Platform-specific optimization passes
- ✅ Unified API across architectures
- ✅ Migration tools between platforms

#### **Milestone 4.3: Production Ready (Month 12)**
- ✅ Performance optimization and profiling
- ✅ Comprehensive test suite and validation
- ✅ Documentation and tutorial creation
- ✅ Community onboarding and support
- ✅ Stable API and versioning strategy

---

## **Conclusion**

**TypeScript Assembly Language (TSAL)** represents a **revolutionary approach** to writing assembly code that maintains the **performance and control** that ROM hackers demand while adding **modern type safety and tooling** that dramatically improves productivity and code quality.

### **Key Innovations**

1. **Visual Clarity Preserved**: Assembly's scannable structure maintained through direct mnemonic imports
2. **Type Safety Added**: Compile-time error detection for memory access, addressing modes, and data types  
3. **Database-Driven**: All game-specific types auto-generated from JSON configurations
4. **IDE Integration**: Full autocomplete, error checking, refactoring, and debugging support
5. **Community Focused**: Easy sharing of typed assembly libraries and cross-project collaboration
6. **Future-Proof**: Extensible to any CPU architecture through database definitions

### **Expected Impact**

TSAL will **transform ROM hacking** by:
- **Reducing Bugs**: Type system catches errors before they reach hardware
- **Improving Productivity**: IDE tooling accelerates development workflow  
- **Enabling Collaboration**: Type-safe code is easier to understand and modify
- **Lowering Barriers**: Better tooling makes assembly more accessible to newcomers
- **Preserving Knowledge**: Typed databases capture and share deep game knowledge

### **Next Steps**

The path forward involves:
1. **Prototype Development**: Build core language features and compilation pipeline
2. **Community Validation**: Test with experienced ROM hackers on real projects
3. **Tool Integration**: Integrate with existing ROM hacking workflows and tools
4. **Ecosystem Growth**: Build community around typed assembly development
5. **Platform Expansion**: Extend support to additional consoles and architectures

TSAL has the potential to become the **definitive tool for modern ROM hacking**, combining the **best of assembly programming** with the **power of modern development tools**. The result is a system that respects the precision and performance demands of assembly while providing the safety and productivity benefits that modern developers expect.

### **Revolutionary Features Summary**

TSAL introduces several groundbreaking concepts to assembly programming:

#### **1. Familiar Yet Enhanced Syntax**
- **Direct mnemonic imports**: `LDA`, `STA`, `BRA` work exactly as expected
- **Intuitive addressing**: `$(0x8000)[X]` reads like assembly but with type safety
- **Clean immediates**: `#0x42` maintains traditional assembly aesthetics
- **Smart structs**: `@h_actor(#0x0C, #0x00, #0x10)` replaces verbose assembly syntax

#### **2. Database-Driven Intelligence**
- **Auto-generated types**: All game-specific structures from JSON databases
- **Memory map integration**: IDE knows every memory location and its purpose
- **String validation**: Character maps ensure text fits within game constraints
- **Cross-platform**: Same syntax works across different CPU architectures

#### **3. Modern Development Experience**
- **Real-time error checking**: Catch mistakes before they become ROM bugs
- **Intelligent autocomplete**: IDE suggests valid addresses, registers, and operations
- **Refactoring support**: Rename memory locations across entire projects safely
- **Source debugging**: Step through TypeScript while debugging generated assembly

#### **4. Community Collaboration**
- **Typed libraries**: Share assembly macros with full type safety
- **Cross-file imports**: Reference functions and data across assembly modules
- **Version control**: Git-friendly TypeScript instead of binary assembly files
- **Knowledge preservation**: Databases capture expert knowledge for future developers

### **Technical Achievement**

TSAL solves the fundamental tension in assembly programming between **control and safety**. Traditional assembly offers complete control but no safety nets. High-level languages provide safety but sacrifice the precise control needed for ROM hacking. TSAL uniquely provides:

- **Zero-cost abstractions**: Types disappear at compile time, leaving pure assembly
- **Opt-in complexity**: Start simple, add sophisticated features as needed
- **Backward compatibility**: Generated assembly works with existing tools and emulators
- **Forward compatibility**: TypeScript ensures code remains maintainable as projects grow

### **Long-Term Vision**

TSAL represents more than just a new programming language—it's a **complete ecosystem** for modern ROM development:

#### **Educational Impact**
- **Lower learning curve**: Type safety makes assembly more approachable for newcomers
- **Better documentation**: Hover tooltips and autocomplete teach as you code
- **Structured learning**: Move from high-level concepts to low-level optimization naturally

#### **Preservation Impact**
- **Knowledge capture**: Game databases preserve reverse engineering discoveries
- **Community sharing**: Typed assembly libraries become reusable knowledge assets
- **Historical preservation**: Modern tools ensure classic games remain hackable

#### **Innovation Impact**
- **New techniques**: Type safety enables more complex ROM modifications
- **Cross-pollination**: Ideas from modern software development enhance ROM hacking
- **Tool evolution**: TSAL concepts may influence other low-level programming domains

### **Call to Action**

The ROM hacking community stands at a crossroads. We can continue with traditional tools and accept their limitations, or we can embrace a new paradigm that preserves everything we love about assembly while adding the productivity and safety benefits of modern development.

TSAL offers a path forward that:
- **Respects the past**: Maintains assembly's visual clarity and performance characteristics
- **Embraces the present**: Uses modern tooling and type safety to reduce errors
- **Enables the future**: Creates a foundation for more ambitious ROM hacking projects

The question isn't whether TSAL will transform ROM hacking—it's whether the community will embrace this transformation and help shape its evolution.

### **Getting Started**

For developers interested in contributing to or experimenting with TSAL:

1. **Study the examples**: Review the code samples throughout this document
2. **Examine existing databases**: Look at the JSON structure that drives type generation
3. **Prototype key features**: Start with basic syntax parsing and type checking
4. **Engage the community**: Share progress and gather feedback from ROM hackers
5. **Build incrementally**: Focus on core features before advanced capabilities

### **Final Thoughts**

Assembly programming has remained largely unchanged for decades, even as every other domain of software development has been revolutionized by better tools and practices. TSAL represents the first serious attempt to bring assembly programming into the modern era without sacrificing what makes it powerful.

The result is not just better tooling—it's a new way of thinking about low-level programming that could influence embedded systems development, kernel programming, and any domain where precise control over hardware is essential.

TSAL proves that we don't have to choose between safety and control, between modern tooling and performance, between accessibility and power. We can have it all.

**The future of assembly programming starts here.**