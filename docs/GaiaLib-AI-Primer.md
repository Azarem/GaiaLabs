# GaiaLib Technical Primer for Generative AI

## Project Overview
GaiaLib is a C# .NET 8.0 library for modifying and analyzing "Illusion of Gaia" (IoG) SNES ROM files. It provides comprehensive ROM hacking capabilities including asset extraction, compression/decompression, 65816 assembly processing, and ROM rebuilding. The library supports multiple game regions (US/JP/DM) and uses a database-driven architecture.

## Core Architecture

### Primary Classes & Responsibilities
- **ProjectRoot**: Main entry point managing project configuration, ROM paths, and orchestrating extraction/building operations
- **DbRoot**: Database management system loading region-specific JSON configurations for ROM structure definitions
- **Compression**: Static class implementing custom LZ-style compression algorithm used by the game
- **RomState**: Manages ROM file state during processing operations
- **BitStream**: Low-level bit manipulation for reading/writing binary data with bit-level precision

### Key Namespaces
- `GaiaLib.Database`: JSON-based database system for ROM structure definitions
- `GaiaLib.Rom.Extraction`: ROM asset extraction functionality (FileReader, SfxReader, BlockReader)
- `GaiaLib.Rom.Rebuild`: ROM building/patching functionality (RomWriter, BlockWriter)
- `GaiaLib.Types`: Core data structures and utilities
- `GaiaLib.Asm`: 65816 assembly code processing and analysis
- `GaiaLib.Enum`: Enumeration types for resource categorization

## Data Model & Types

### BinType Enumeration (Resource Categories)
```csharp
enum BinType { Bitmap, Tilemap, Tileset, Palette, Sound, Music, Unknown, Meta17, Spritemap, Assembly, Patch, Transform }
```

### Core Data Structures
- **Address**: Represents SNES memory addresses with LOROM/HIROM banking
- **Location**: Complex addressing system with offsets and transformations
- **ChunkFile**: Represents extracted ROM file chunks
- **CompressionEntry**: Metadata for compressed data blocks
- **StringEntry/StringMarker**: Text data extraction and processing

### Database Schema (JSON-based)
Each regional database (us/jp/dm) contains:
- **mnemonics.json**: 65816 instruction definitions
- **blocks.json**: Memory block definitions with extraction rules
- **files.json**: File location mappings within ROM
- **overrides.json**: Special case handling for extraction
- **stringCommands.json**: Text processing command definitions
- **copdef.json**: Coprocessor definitions for special instructions
- **structs.json**: Data structure definitions
- **config.json**: Regional configuration and entry points

## Compression Algorithm Details

### Custom LZ-variant Implementation
- **Dictionary Size**: 256 bytes (0x100)
- **Initial Dictionary**: Filled with 0x20 (space characters)
- **Dictionary Position**: Starts at 0xEF
- **Bit-stream based**: Uses BitStream class for bit-level I/O
- **Header**: First 2 bytes contain decompressed size (little-endian)

### Compression Logic
```
If bit=1: Copy literal byte to output and dictionary
If bit=0: Read dictionary index + length, copy sequence from dictionary
```

### Key Methods
- `Expand(byte[] srcData, int srcPosition, int srcLen)`: Decompression
- `Compact(byte[] srcData)`: Compression with optimal sequence matching

## Assembly Processing (65816)

### OpCode System
- **65816 Processor**: 16-bit extension of 6502, used in SNES
- **OpCode Class**: Represents individual assembly instructions with addressing modes
- **Mnemonics Dictionary**: Maps instruction codes to human-readable names
- **Addressing Modes**: Immediate, Direct, Absolute, Indexed, etc.

### Assembly Components
- **Registers**: A, X, Y registers with 8/16-bit modes
- **StatusFlags**: Processor status register flags (N, V, M, X, D, I, Z, C)
- **Stack**: Stack pointer and operations
- **Op**: Individual operation representation

## ROM Structure & Processing

### Extraction Pipeline
1. **FileReader**: Extracts general file data based on database definitions
2. **SfxReader**: Specialized sound effect extraction
3. **BlockReader**: Analyzes memory blocks and resolves dependencies
4. **BlockWriter**: Outputs processed blocks to organized directories

### Rebuild Pipeline
1. **RomWriter**: Orchestrates ROM reconstruction
2. Patches are applied using transforms and overrides
3. Compressed data is recompressed using Compression.Compact()
4. Assembly code is reassembled with proper addressing

### Resource Management
Resources are organized by type with configurable paths:
```json
{
  "Bitmap": { "folder": "graphics", "extension": "bin" },
  "Palette": { "folder": "palettes", "extension": "pal" },
  "Music": { "folder": "music", "extension": "bgm" },
  "Assembly": { "folder": "asm", "extension": "asm" }
}
```

## Database-Driven Architecture

### Multi-Region Support
- **US Database**: American release version
- **JP Database**: Japanese original version  
- **DM Database**: Alternate/demo versions

### Database Loading
`DbRoot.FromFolder()` aggregates JSON files into unified data structures:
- Converts lists to dictionaries for O(1) lookups
- Establishes parent-child relationships (blocks↔parts)
- Creates lookup tables for mnemonics and opcodes

### Transform System
- **DbTransform**: Defines post-processing modifications
- **DbRewrite**: Address remapping for relocated code
- **DbOverride**: Special case handling during extraction

## BitStream Implementation Details

### Bit-Level Operations
- **Bit Reading**: Maintains bit flag (0x80→0x01) for current bit position
- **Byte Boundary Handling**: Automatically advances position when bit flag resets
- **Nibble Operations**: 4-bit reads with proper bit alignment
- **Writing Operations**: Accumulates bits in write buffer until byte complete

### Critical Features
- Handles non-byte-aligned data common in compressed formats
- Supports both read and write operations on same stream
- Maintains position tracking for debugging and validation

## Memory Management & Performance

### Optimization Strategies
- Dictionary-based lookups for O(1) access to mnemonics/opcodes
- Lazy loading of ROM data to minimize memory footprint
- Streaming operations for large file processing

## Error Handling & Validation

### Defensive Programming
- Null checks and safe defaults throughout API
- Range validation for array access operations
- Graceful handling of malformed ROM data

### Configuration Validation
- Project.json schema validation
- Database consistency checks during loading
- ROM format verification before processing

## Integration & Usage Patterns

### Typical Workflow
```csharp
// 1. Load project configuration
var project = ProjectRoot.Load("project.json");

// 2. Extract all assets from ROM
var database = await project.DumpDatabase();

// 3. Modify extracted files (external tools/manual editing)

// 4. Rebuild ROM with modifications
project.Build();
```

### Extension Points
- Custom BinType for new resource types
- DbPath configuration for custom folder structures
- Transform definitions for automated post-processing
- Override system for special case handling

## Technical Limitations & Constraints

### ROM Format Dependencies
- Specifically designed for Illusion of Gaia ROM structure
- Compression algorithm is game-specific (not generic LZ)
- Memory mapping assumptions based on SNES LOROM format

### Platform Requirements
- .NET 8.0 runtime required
- Windows PowerShell for build scripts (per user rules)
- Floating IPS (FLIPS) tool for patch generation

## Security & Legal Considerations

### ROM File Handling
- Library requires legitimate ROM file (not included)
- Extraction preserves original data integrity
- Modifications are separate from original ROM

### Code Safety
- Input validation prevents buffer overflows
- File I/O operations use safe .NET APIs
- All operations use managed memory and standard .NET runtime safety features

## Future Extensibility Design

### Modular Architecture
- Clear separation between extraction and rebuild phases
- Database-driven approach allows new game support
- Plugin-style resource type system

### Version Compatibility
- JSON-based configuration allows schema evolution
- Backward compatibility through default value handling
- Region-specific databases support localization variants

This primer provides the foundational knowledge needed to understand and work with the GaiaLib codebase, covering all major architectural decisions, data flows, and technical implementation details. 