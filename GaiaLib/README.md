# GaiaLib - Illusion of Gaia ROM Modification Library

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

GaiaLib is a comprehensive C# library designed for modifying and analyzing ROM files of the classic SNES game **Illusion of Gaia** (known as *Gaia Gensouki* in Japan). This library provides powerful tools for ROM hackers, translators, and game modders to extract, modify, and rebuild game assets with precision and efficiency.

## 🎮 About Illusion of Gaia

Illusion of Gaia is a beloved action RPG developed by Quintet for the Super Nintendo Entertainment System. Released in 1993 in Japan and 1994 in North America, it's part of Quintet's "Creation Trilogy" alongside Soul Blazer and Terranigma. The game follows Will, a young psychic adventurer, as he explores ancient ruins and uncovers the mysteries of human civilization.

## ✨ Features

### Core Functionality
- **Complete ROM Analysis**: Parse and understand the structure of Illusion of Gaia ROM files
- **Asset Extraction**: Extract graphics, music, sound effects, tilemaps, palettes, and assembly code
- **Compression Support**: Built-in decompression and compression algorithms for game assets
- **Database-Driven Architecture**: Comprehensive JSON databases for different game regions (US, JP, DM)
- **Assembly Code Processing**: Advanced 65816 assembly code analysis and manipulation

### Asset Types Supported
- 🎨 **Graphics**: Bitmaps, sprites, and sprite animations
- 🗺️ **Maps**: Tilemaps and tilesets
- 🎨 **Palettes**: Color palette data
- 🎵 **Audio**: Music (BGM) and sound effects (SFX)
- ⚙️ **Code**: Assembly code blocks and patches
- 📊 **Data**: Game data structures and configuration

### Advanced Capabilities
- **Multi-Region Support**: Works with US, Japanese, and other regional versions
- **Intelligent String Processing**: Handle game text with encoding support
- **Memory Layout Analysis**: Understand ROM memory organization
- **Patch Generation**: Create and apply modifications via BPS patches
- **Resource Management**: Organized extraction to categorized folders

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 or later
- A legitimate Illusion of Gaia ROM file
- [Floating IPS](https://www.romhacking.net/utilities/1040/) or similar patching tool (optional)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/your-username/GaiaLib.git
cd GaiaLib
```

2. Build the project:
```bash
dotnet build
```

### Basic Usage

#### Initialize a Project
```csharp
// Load project configuration
var project = ProjectRoot.Load("path/to/project.json");

// Or create a new project
var project = new ProjectRoot
{
    Name = "My IoG Mod",
    RomPath = @"C:\Games\Illusion of Gaia.smc",
    Database = "us", // or "jp" for Japanese version
    BaseDir = @"C:\MyModProject"
};
```

#### Extract ROM Assets
```csharp
// Dump all assets from ROM
var database = await project.DumpDatabase();

// This extracts:
// - Graphics to /graphics/*.bin
// - Music to /music/*.bgm  
// - Palettes to /palettes/*.pal
// - Assembly code to /asm/*.asm
// - And more...
```

#### Rebuild Modified ROM
```csharp
// After modifying extracted assets
project.Build(); // Creates a new ROM with your changes
```

## 📁 Project Structure

```
GaiaLib/
├── Database/           # Database management and JSON parsing
├── Rom/
│   ├── Extraction/     # ROM reading and asset extraction
│   └── Rebuild/        # ROM assembly and building
├── Types/              # Core data types and utilities
├── Sprites/            # Sprite and animation handling
├── Enum/               # Enumerations for various data types
├── Asm/                # Assembly code processing
├── Compression.cs      # Compression/decompression algorithms
├── ProjectRoot.cs      # Main project management
└── db/                 # Region-specific databases
    ├── us/             # US version data
    ├── jp/             # Japanese version data  
    └── dm/             # Other regional data
```

## 🔧 Configuration

The library uses a `project.json` configuration file:

```json
{
  "name": "GaiaLabs",
  "romPath": "C:\\Games\\Illusion of Gaia.smc",
  "baseDir": "",
  "database": "us",
  "flipsPath": "C:\\Games\\flips.exe",
  "resources": {
    "Bitmap": { "folder": "graphics", "extension": "bin" },
    "Palette": { "folder": "palettes", "extension": "pal" },
    "Music": { "folder": "music", "extension": "bgm" },
    "Tileset": { "folder": "tilesets", "extension": "set" },
    "Assembly": { "folder": "asm", "extension": "asm" }
  }
}
```

## 🎯 Use Cases

### ROM Translation Projects
- Extract and modify game text and dialogue
- Support for multiple languages and character encodings
- Automated script processing and insertion

### Graphics Modifications  
- Replace sprites, backgrounds, and UI elements
- Modify color palettes for visual themes
- Create custom animations and sprite sets

### Music and Audio Hacking
- Extract original music for remixing
- Insert custom background music
- Modify sound effects

### Gameplay Modifications
- Alter game logic through assembly patches
- Modify character stats and abilities  
- Change level layouts and progression

### Research and Analysis
- Study the technical structure of classic SNES games
- Understand compression algorithms used in retro games
- Analyze memory layout and data organization

## 🛠️ Advanced Features

### Database System
The library includes comprehensive databases for different game regions:
- **Mnemonics**: 65816 assembly instruction definitions
- **String Commands**: Text processing and encoding rules  
- **File Definitions**: ROM file structure and locations
- **Block Definitions**: Memory layout and data organization

### Compression Engine
Built-in support for the game's custom compression format:
```csharp
// Decompress game data
byte[] decompressed = Compression.Expand(compressedData);

// Compress modified data  
byte[] compressed = Compression.Compact(modifiedData);
```

### Assembly Processing
Advanced 65816 assembly code analysis:
- Instruction parsing and validation
- Address resolution and relocation
- Code block identification and extraction
- Patch generation and application

## 🤝 Contributing

We welcome contributions! Whether you're:
- 🐛 Reporting bugs
- 💡 Suggesting features  
- 📖 Improving documentation
- 🔧 Submitting code changes

Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

## 📚 Documentation

For detailed documentation, see:
- [API Reference](docs/api-reference.md)
- [Database Format](docs/database-format.md)
- [ROM Structure Guide](docs/rom-structure.md)
- [Compression Algorithm](docs/compression.md)

## 🎮 Related Projects

- **[IOG Retranslation](https://github.com/Azarem/IOGRetranslation)**: Complete retranslation of Illusion of Gaia
- **[GaiaLabs Tools](https://github.com/your-username/gaialabs-tools)**: GUI tools built on GaiaLib
- **[IoG Randomizer](https://github.com/your-username/iog-randomizer)**: Randomizer built with GaiaLib

## ⚖️ Legal Notice

This library is for educational and research purposes. You must own a legitimate copy of Illusion of Gaia to use this software. This project does not distribute copyrighted ROM files or game assets.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Quintet** - Original developers of Illusion of Gaia
- **L Thammy** - Retranslation work that inspired database improvements  
- **GaiaLabs Community** - Contributors and testers
- **ROM Hacking Community** - Tools, techniques, and knowledge sharing

---

*Made with ❤️ for the Illusion of Gaia community* 