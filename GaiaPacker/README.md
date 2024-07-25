# BaseROM Toolkit

This is a development toolkit for Illusion of Gaia (SNES). Using it will allow you to browse every asset and piece of code in the game. In addition, it will also allow you to rebuild all of these assets into new ROM files. All assets are decompressed so you can enjoy lightning fast screen transitions and load times. Any files changes you make, as well as any assembly patches you create, will be integrated in this process. Please note that only a handful of files in the `asm/` folder are rebuilt into the ROM (see below).

### Requirements

* .Net Framework 8 (https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* An unheadered Illusion of Gaia (USA) ROM

### Getting Started

1. Get the latest release from GitHub and extract the package contents to a folder on your PC. You will be using this as a project folder so make sure it is empty beforehand.
2. Open the 'project.json' file in a text editor and change the `romPath` property to reflect the full path of an unheadered Illusion of Gaia (USA) ROM file on your PC.
3. Open a command prompt in your new project directory and run `GaiaPacker.exe --unpack`; this will dump all of the game files into your project folder.
4. Make modifications to game files or patches. Included is an MSU-1 patch (credits to Conn), if you would like to use it to add MSU support simply remove the underscore `_` from the file name to be `iog_msu.asm`
5. You can add or edit scene assets with `asm/scene_meta.asm`. Scene actors are located in `asm/scene_events.asm`. `asm/scene_warps.asm` contains all the screen transition triggers.
6. There are example patches like `Teleporter.asm` and `ItemRewards.asm` included in the Patches folder. Feel free to experiment with these.
7. Go back to your command prompt and run `GaiaPacker.exe`. This will re-build the ROM along with any changes you made.

### ASM Files Currently Not Supported

Some files in the `asm/` folder are not supported for rebuild, they are included for reference.

* Files beginning with `chunk_`
* binary_01ABDE
* binary_01C384
* binary_01D8BE
* table_03B401


### Support the project on Patreon!
https://www.patreon.com/GaiaLabs
