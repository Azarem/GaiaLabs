# GaiaLabs - Illusion of Gaia Visual Editor

GaiaLabs is a comprehensive visual editing suite for Illusion of Gaia ROM hacking, built using Godot Engine with C#. This tool provides an intuitive graphical interface for editing tilemaps, tilesets, sprites, and palette bundles. It is designed to work seamlessly with [GaiaLib](../GaiaLib) (the core library) and [GaiaPacker](../GaiaPacker) (the ROM building tool) to provide a complete ROM hacking workflow.

## Features

### 🎨 **Tilemap Editor**
- **Visual tilemap editing** with real-time preview
- **Mouse interactions**:
  - Left click to place selected tiles
  - Right click to pick tiles from the map
  - Middle mouse button to pan the view
  - Mouse wheel to zoom in/out
- **Paint mode** with drag-to-paint functionality
- Dynamic tilemap resizing with width/height controls
- Support for both main and effect tilemaps

### 🧩 **Tileset Editor**
- **Tile properties editing**:
  - Horizontal/Vertical mirroring toggles
  - Priority flags
  - Block collision settings
- **Graphics selection** from VRAM data
- **Palette assignment** per tile
- Real-time tile preview with applied transformations

### 🎭 **Sprite Editor**
- **Comprehensive sprite management**:
  - Sprite sets organization
  - Frame-based sprite editing
  - Sprite group management
  - Individual sprite part editing
- **Sprite properties panel** for precise control
- **Add/Remove functionality** for all sprite components
- Visual sprite preview system coming soon!

### 🌈 **Palette Bundle Tool**
- **Interactive palette selector** with visual color representation
- **Palette animation support** through PAnimSelector
- 8-palette system with 16-color sub-palettes

### 🎮 **User Interface Controls**

#### Mouse Controls
- **Left Click**: Select/Paint tiles, select graphics, modify properties
- **Right Click**: Pick existing tiles, access context menus
- **Middle Click**: Pan view in editors
- **Mouse Wheel**: Zoom in/out in all editors
- **Drag Operations**: Paint mode in tilemap editor

#### Keyboard Controls
- Basic navigation support (implementation in progress)

## Project Structure

```
GaiaLabs/
├── src/
│   ├── control/
│   │   ├── ControlTest.cs          # Main application controller
│   │   ├── TilesetEditor.cs        # Tileset editing functionality  
│   │   ├── TilemapControl.cs       # Tilemap editing functionality
│   │   ├── GfxSelector.cs          # Graphics/sprite selection
│   │   ├── PaletteSelector.cs      # Color palette management
│   │   ├── PropertyPanel.cs        # Property editing interface
│   │   ├── Sprite/                 # Sprite editing components
│   │   │   ├── SpriteSetList.cs
│   │   │   ├── SpriteFrameList.cs
│   │   │   ├── SpriteGroupList.cs
│   │   │   └── SpritePartList.cs
│   │   └── PAnim/                  # Palette animation tools
│   │       └── PAnimSelector.cs
│   └── res/
│       └── ImageConverter.cs       # Image processing utilities
├── control.tscn                    # Main UI scene
├── project.godot                   # Godot project configuration
└── GaiaLabs.csproj                # C# project file
```

## Integration with GaiaLabs Suite

GaiaLabs is part of a comprehensive ROM hacking toolkit:

- **[GaiaLib](../GaiaLib)** - Core library providing ROM data structures, decompression, and file format handling
- **[GaiaPacker](../GaiaPacker)** - Command-line tool for building and patching ROM files

## Getting Started

### Prerequisites
- .NET 8.0 or later
- Godot Engine 4.3+ with C# support
- An Illusion of Gaia ROM file
- Database files from GaiaLib

### Setup
1. Clone the GaiaLabs repository
2. Open the project in Godot Engine
3. Build the C# project
4. Configure your ROM path in the project settings
5. Load a scene to begin editing

### Basic Workflow
1. **Load a Scene**: Select a scene from the database to edit
2. **Edit Tilemaps**: Use the tilemap editor to modify level layouts
3. **Modify Tilesets**: Edit individual tiles and their properties
4. **Adjust Sprites**: Configure sprite animations and properties
5. **Save Changes**: Use the save buttons to persist modifications
6. **Build ROM**: Use GaiaPacker to create the final ROM file

## Technical Details

### Graphics Format Support
- **SNES 4bpp tile format** with palette-based coloring
- **VRAM data processing** with automatic tile generation
- **Palette conversion** from 15-bit to 32-bit RGBA
- **Tile mirroring and transformation** support

### Data Structures
- **Tilemap data**: 8-bit tile indices with 256-tile blocks
- **Tileset data**: 16-bit tile entries with flags and palette info
- **Sprite data**: Hierarchical sprite sets, frames, groups, and parts
- **Palette data**: 16-color palettes with animation support

## Contributing

Contributions are welcome! Please ensure your code follows the existing patterns and includes appropriate documentation.

## License

This project is part of the GaiaLabs suite. See the main repository for license information.

## Links

- **Main Repository**: [GaiaLabs](https://github.com/Azarem/GaiaLabs)
- **Core Library**: [GaiaLib](../GaiaLib)
- **ROM Packer**: [GaiaPacker](../GaiaPacker)