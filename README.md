# GaiaLabs - Complete Illusion of Gaia ROM Hacking Toolkit

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-GPL--3.0-green.svg)](LICENSE.txt)

**GaiaLabs** is a comprehensive suite of tools for **Illusion of Gaia** (Gaia Gensouki) ROM hacking, reverse engineering, and modification. This toolkit enables complete game analysis, asset extraction, editing, and ROM rebuilding with modern enhancements including **decompressed loading**, **MSU-1 audio support**, and **quality-of-life improvements**.

## 🎮 Featured Project

**[Illusion of Gaia: Retranslated](https://github.com/Azarem/IOGRetranslation)** - A complete retranslation of Illusion of Gaia featuring L Thammy's acclaimed translation work, built using the GaiaLabs toolkit. This project showcases the full capabilities of the toolset with enhanced dialogue, restored Japanese assets, MSU-1 audio support, and numerous gameplay improvements.

## 🛠️ Toolkit Components

### Core Libraries

#### [GaiaLib](GaiaLib/) - ROM Modification Library ⭐
The foundational C# library powering the entire toolkit:
- **Complete ROM Analysis** - Parse and understand Illusion of Gaia ROM structure
- **Asset Extraction** - Graphics, music, palettes, tilemaps, and assembly code
- **Multi-Region Support** - US, Japanese, and other regional ROM variants
- **Advanced Compression** - Built-in decompression/compression algorithms
- **Assembly Processing** - 65816 code analysis and manipulation
- **Database Architecture** - JSON-driven ROM structure definitions

#### [GaiaLibPy](GaiaLibPy/) - Python Library
Python port of core GaiaLib functionality

### Development Tools

#### [GaiaLabs](GaiaLabs/) - Visual Editor Suite 🎨
Complete Godot-based visual editing environment:
- **Tilemap Editor** - Visual level editing with mouse/keyboard controls
- **Tileset Editor** - Tile property editing with mirroring and collision
- **Sprite Editor** - Comprehensive sprite and animation management
- **Palette Bundle Tool** - Interactive color palette editing
- **Real-time Preview** - Immediate visual feedback for all edits

#### [GaiaPacker](GaiaPacker/) - ROM Builder & Patcher 🔧
Command-line tool for ROM compilation and asset management:
- **ROM Building** - Compile modified ROMs with custom assets
- **Patch Integration** - Apply assembly code modifications
- **Transform System** - Regex-based assembly code fixes
- **Asset Management** - Handle all game resource types
- **MSU-1 Integration** - CD-quality audio streaming support

#### [GaiaCompressor](GaiaCompressor/) - Compression Utility
Standalone compression tool implementing the game's custom algorithm:
- File compression using original game format
- Batch processing support
- Compression analysis and optimization

#### [GaiaApi](GaiaApi/) - Web API Interface
RESTful API for remote ROM manipulation:
- HTTP endpoints for ROM operations
- Swagger documentation
- Integration support for web applications

## 🚀 Quick Start

### Prerequisites
- .NET 8.0+ Runtime ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- Unheadered Illusion of Gaia (USA) ROM file

### Installation
1. **Download** the latest BaseROM Toolkit from [GaiaLabs Releases](https://github.com/Azarem/GaiaLabs/releases)
2. **Extract** the release file to your desired location
3. **Update** the `project.json` file with your ROM location
4. **Run** `GaiaPacker --unpack` to extract game content

### Basic Workflow
1. **Extract ROM Data**: Use `GaiaPacker --unpack` to extract all game assets
2. **Edit Assets**: Use GaiaLabs visual editor or modify files directly
3. **Apply Patches**: Include assembly modifications for new features
4. **Build ROM**: Use `GaiaPacker` to compile everything into a modified ROM file
5. **Test & Play**: Enjoy your customized Illusion of Gaia experience

## ✨ Enhanced Features Available

### Loading & Performance
- **🚀 Decompressed Assets** - Eliminate loading times with uncompressed data
- **⚡ Scene Transitions** - Near-instant area transitions
- **💾 4MB ROM Support** - Expanded ROM size with 1MB free space

### Audio Enhancements
- **🎵 MSU-1 Support** - CD-quality 16-bit stereo audio at 44.1kHz
- **🎶 Music Packs** - [Download professional orchestral arrangements](https://www.zeldix.net/t1604-illusion-of-gaiatime-gaia-trilogy-ii)
- **🔊 Audio Optimization** - Improved BGM loading and processing

### Quality of Life
- **🏃 Run Button** - Sprint using Y button for faster movement
- **📦 Item Stacking** - Visual counters for Red Jewels, Herbs, and Crystals
- **⚡ Item Hotswapping** - Quick switching with shoulder buttons
- **🎒 Equipped Indicators** - Visual item status in inventory
- **🔄 New Game Plus** - Enhanced replay functionality

## 📊 Technical Capabilities

### Supported Asset Types
| Type | Format | Features |
|------|--------|----------|
| **Graphics** | 4bpp SNES tiles | Sprites, backgrounds, UI elements |
| **Palettes** | 15-bit RGB | 8 palettes × 16 colors, animation support |
| **Tilemaps** | 8-bit indices | Level layouts, collision, properties |
| **Audio** | SPC/MSU-1 | Original + CD-quality streaming |
| **Code** | 65816 Assembly | Game logic, patches, enhancements |
| **Text** | Multi-encoding | Dialogue, menus, story content |

## 🎯 Use Cases

### Translation Projects
- Extract and modify game text with encoding support
- Support multiple languages and character sets
- Automated script processing and insertion

### Visual Modifications
- Replace sprites, backgrounds, and UI elements
- Create custom animations and sprite sets
- Modify color palettes for visual themes

### Audio Enhancement
- Insert custom background music via MSU-1
- Extract original tracks for analysis/remixing
- Modify sound effects and audio cues

### Gameplay Modifications
- Alter game logic through assembly patches
- Add new features like run buttons and item stacking
- Modify character stats, abilities, and progression
- Create custom areas and level layouts

### Research & Analysis
- Study classic SNES game architecture
- Understand compression algorithms and data structures
- Analyze memory organization and technical implementation

## 🏗️ Project Structure

```
GaiaLabs/
├── GaiaLib/           # Core ROM modification library (.NET)
├── GaiaLabs/          # Visual editor suite (Godot + C#)
├── GaiaPacker/        # ROM builder and patcher (CLI)
├── GaiaApi/           # Web API interface
└── docs/              # Technical documentation
```

## 🤝 Community & Support

- **💬 Discord**: [Join our community](https://discord.gg/gyyqDHKgPe)
- **📧 Issues**: [GitHub Issues](https://github.com/Azarem/GaiaLabs/issues)
- **🎵 MSU-1 Music**: [Zeldix Database](https://www.zeldix.net/t1604-illusion-of-gaiatime-gaia-trilogy-ii)

## 🎉 Success Stories

The **[Illusion of Gaia: Retranslated](https://github.com/Azarem/IOGRetranslation)** project demonstrates the full power of GaiaLabs:
- Complete story retranslation with improved coherence
- Restored Japanese visual assets and animations  
- MSU-1 audio support with orchestral music packs
- Quality-of-life improvements and modern features
- Enhanced accessibility and user experience

*"This project showcases what's possible when passionate ROM hackers have access to comprehensive, well-designed tools."*

## 📜 Legal Notice

GaiaLabs is designed for use with legally obtained ROM files. Users must own a legitimate copy of Illusion of Gaia to use this toolkit. This project is not affiliated with Square Enix, Nintendo, or the original developers.

## 🌟 Getting Started

Ready to begin your Illusion of Gaia ROM hacking journey?

1. **🔗 Check out the [Retranslation Project](https://github.com/Azarem/IOGRetranslation)** to see what's possible
2. **📥 Download the [BaseROM Toolkit](https://github.com/Azarem/GaiaLabs/releases)** to get started
3. **📚 Read the [GaiaLib documentation](GaiaLib/)** to understand the core library
4. **🎨 Try the [Visual Editor](GaiaLabs/)** for immediate hands-on experience
5. **⚙️ Use [GaiaPacker](GaiaPacker/)** to build your first modified ROM
6. **💬 Join our [Discord community](https://discord.gg/gyyqDHKgPe)** for support and collaboration

---

*Rediscover the world of Illusion of Gaia with modern tools and unlimited possibilities.*



