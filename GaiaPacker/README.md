# GaiaPacker v1.0

### Requirements

* .Net Framework 8 (https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* An unheadered Illusion of Gaia (USA) ROM

### Getting Started

1. Get the latest release from GitHub and extract the package contents to a folder on your PC. You will be using this as a project folder so make sure it is empty beforehand.
2. Open the 'project.json' file in a text editor and change the `romPath` property to reflect the full path of an unheadered Illusion of Gaia (USA) ROM file on your PC.
3. Open a command prompt in your new project directory and run `GaiaPacker.exe --unpack`; this will dump all of the game files into your project folder.
4. Make modifications to game files or patches. Included is an MSU-1 patch (credits to Conn), if you would like to use it to add MSU support simply remove the underscore `_` from the file name to be `iog_msu.asm`
5. Check out `asm/mapmeta.asm`, this file is connected to struct processing and will be rebuilt along with other files. Add more scenes or assets to this file using a text editor.
6. Go back to your command prompt and run `GaiaPacker.exe`. This will re-build the ROM along with any changes you made.

*__Using this package will allow you to make changes to nearly any part of the game, and rebuild the ROM for patch creation.__*

## Support the project on Patreon!
https://www.patreon.com/GaiaLabs
