# GaiaPacker

A console application for building and managing Illusion of Gaia ROM modifications. GaiaPacker serves as a command-line interface to the GaiaLib library, providing tools to compile custom ROM patches with modified assets, code patches, and data transforms.

## Features

- **ROM Building**: Compile modified ROMs with custom assets, patches, and transforms
- **Database Extraction**: Unpack and dump ROM data for analysis and modification
- **Asset Management**: Handle graphics, palettes, spritemaps, and other game assets
- **Patch Integration**: Apply assembly code patches to modify game behavior
- **Data Transforms**: Modify game data structures, music assignments, and scene properties

## Usage

### Building a ROM
```bash
GaiaPacker [project_path]
```

### Extracting/Unpacking Data
```bash
GaiaPacker --unpack [project_path]
```

## Requirements

- .NET 8.0 Runtime ([Download](https://dotnet.microsoft.com/en-us/download/dotnet/8.0))
- An unheadered Illusion of Gaia (USA) ROM
- GaiaLib project files

## How It Works

GaiaPacker operates on project directories that contain:
1. **Asset files** (graphics, palettes, spritemaps) that replace original game data
2. **Assembly patches** that modify game code and add new functionality
3. **Transform files** that contain regex fixes for generated assembly code
4. **Project configuration** that defines how everything fits together

The build process integrates all these components into a modified ROM file, while the unpack process extracts data from existing projects for analysis.

## Project Structure

A complete GaiaPacker project follows this structure:

```
project/
├── project.json          # Project configuration and ROM paths
├── database.json         # Complete ROM data structure definitions
├── database_jp.json      # Japanese ROM variant data
├── database_dm.json      # Diamond Mine specific data
├── asm/                  # Disassembled game code organized by area
│   ├── functions/        # Core game functions
│   ├── south_cape/       # Area-specific code
│   ├── angel_village/    # Character and scene scripts
│   └── [other areas]/    # All game locations
├── graphics/             # Binary graphics data (.bin files)
├── palettes/             # Color palette data (.pal files)
├── spritemaps/           # Sprite mapping data (.bin files)
├── patches/              # Assembly code modifications (.asm files)
├── transforms/           # JSON-based data transformations
│   ├── scene_meta.json   # Global scene metadata changes
│   ├── scene_actors.json # Actor placement modifications
│   └── [area folders]/   # Location-specific transforms
├── music/                # Audio files and music data
├── sfx/                  # Sound effect data
├── tilesets/             # Tileset graphics and data
├── tilemaps/             # Level layout data
└── misc/                 # Miscellaneous game data
```

## Transform System

Transforms are JSON files that contain regular expression-based fixes applied to the generated assembly files **after** the unpacking process. They do not affect ROM creation - instead, they correct issues and inconsistencies in the disassembled source code output.

### Transform Types

1. **Replace**: Regex-based text replacement in assembly files
2. **Lookup**: Key-value pair transformations for consistent naming

### Transform Application Process

1. ROM data is unpacked and disassembled into assembly files
2. **Global transforms** (scene_meta.json, scene_actors.json) are applied to fix common issues
3. **Area-specific transforms** correct location-specific assembly problems  
4. **Character/object transforms** fix individual actor and object references

### Transform Examples

**Assembly Code Fix:**
```json
{
  "type": "Replace",
  "key": "music < #06, #00, @bgm_ominous_whispers >",
  "value": "music < #04, #00, @bgm_ominous_whispers >"
}
```
*Fixes incorrect music reference in generated assembly*

**Code Pattern Addition:**
```json
{
  "type": "Replace", 
  "key": "actor < #08, #0C, #03, @h_hidden_red_jewel >",
  "value": "$&\r\n    actor < #0C, #2F, #00, @SkyDeliveryman >"
}
```
*Adds missing actor definition after existing code*

**Label Reference Correction:**
```json
{
  "type": "Replace",
  "key": "av6A_lance_destroy", 
  "value": "av69_lance_destroy"
}
```
*Fixes incorrect scene label reference in assembly*

The `$&` symbol represents the original matched text, allowing transforms to preserve existing code while making additions or corrections.

## Included Patches

GaiaPacker includes a comprehensive set of assembly patches that enhance the base game with modern features and quality-of-life improvements:

### Loading Enhancements
- **Decompressed Asset Loading**: Patches enable uncompressed graphics, music, and other assets for faster loading times
- **Scene Load Optimization**: Improved transition speeds between areas
- **Bitmap/Tilemap/Tileset Patches**: Enhanced asset loading for visual elements

### Audio Enhancements
- **MSU-1 Support**: Full CD-quality audio streaming capability
- **Music Transitions**: Smooth audio transitions between scenes
- **APU Wait Fixes**: Audio processing optimizations to prevent glitches
- **SFX Table**: Enhanced sound effect management

### Gameplay Features
- **Run Button**: Sprint functionality using Y button for faster movement
- **Item Stacking**: Inventory counters for Red Jewels, Herbs, and Crystals 
- **Equipped Item Indicator**: Visual indicator showing currently equipped item
- **Item Hotswapping**: Quick item switching using shoulder buttons
- **New Game Plus**: Start new playthrough while retaining certain progress

### Inventory & Interface
- **Inventory Management**: Enhanced inventory system with improved organization
- **Menu Background Patches**: Visual improvements to menu systems
- **Equipped Icon Display**: Shows equipped items in inventory screen

### Experimental Features
- **Teleporter System**: Fast travel between discovered locations
- **Expanded Health Bars**: Enhanced health display system
- **Pixel Converter**: Color theme utility

## MSU-1 Audio Support

The MSU-1 (Media Streaming Unit) is an enhancement chip that enables CD-quality 16-bit stereo audio at 44.1kHz. This allows replacement of the original compressed SPC audio with high-quality music tracks.

**🎵 Download Ready-Made Music Packs: [Zeldix MSU-1 Database](https://www.zeldix.net/t1604-illusion-of-gaiatime-gaia-trilogy-ii)**

### MSU-1 Setup Requirements
1. **Compatible Emulator**: BSNES, Snes9x v1.55+, or SD2SNES/FXPak flash cart
2. **Audio Files**: Tracks in `.pcm` format with proper naming convention
3. **File Structure**: 
   ```
   project/
   ├── game.sfc          # Patched ROM file
   ├── game.msu          # MSU-1 data file (can be empty)
   ├── game-1.pcm        # Track 1 audio
   ├── game-2.pcm        # Track 2 audio
   └── [additional tracks]
   ```

### Conversion Process
Original SPC audio tracks can be enhanced to CD quality using tools like `wav2msu` to convert high-quality source audio (orchestral recordings, remastered tracks, etc.) into the MSU-1 PCM format.

### Pre-made Music Packs
High-quality orchestral and remastered music packs are available for download at the [Zeldix MSU-1 Database](https://www.zeldix.net/t1604-illusion-of-gaiatime-gaia-trilogy-ii). This community resource provides multiple professionally-created audio sets including:

- **Orchestral arrangements** with full symphony recordings
- **Remastered original tracks** with enhanced audio quality  
- **Alternative musical interpretations** by community composers

These packs include properly formatted `.pcm` files ready for use with MSU-1 patches, eliminating the need for manual audio conversion.

## Related Projects

- **[GaiaLib](../GaiaLib/)** - Core ROM hacking library
- **[Decompressed ROM Project](https://github.com/Azarem/GaiaLabs/releases)** - Base ROM with removed compression for faster loading

---

*GaiaPacker is part of the GaiaLabs toolkit for Illusion of Gaia ROM hacking.*

