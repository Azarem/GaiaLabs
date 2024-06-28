![logo_small](https://github.com/Azarem/GaiaLabs/assets/7395229/cbb7c720-ebec-4cf9-8110-edd6f2fbe057)

# GaiaLabs
Illusion of Gaia (dis)assembler with COProcessor call awareness and data struct support. Adding more features regularly.
The goal is to re-assemble all the pieces and create new rom files. This will allow you to potentially change any part of the game.

## Potential Uses
- ~~Expand rom to 4mb and remove file compression to improve load times~~ (complete)
- Write new code modules and apply them to the rom (75%)
- Add completely new areas to the game
- Enhancements to the world map

## Decompressed ROM
After six months of hard work, and many bug fixes later, I am pleased to announce that the decompressed ROM project is a success! A BPS patch is available in the Releases section, which has been fully tested using BSNES Accuracy and an original console. The patch features a handful of ASM changes that were introduced in order to support decompressed asset loading. Also included are some audio (SPC) processing 'hacks' that were used to significantly increase BGM load times. As a result of all these changes, the screen transitions and loading times between areas is very fast! This means less time spent waiting for things to decompress during gameplay. There is 1MB of free space in banks $30 - $3F that you can use to add your own assets, without the need to compress your data. I will continue to test and debug this as my base ROM as I work on other projects. Stay tuned!
  
## File Dump
As a part of ongoing work, the Illusion of Gaia ROM has been extracted, decompressed, and disassembled. You can now explore any part of the game using your file system!
https://github.com/Azarem/GaiaLabs/releases

## Patreon
Support the project on Patreon!
https://www.patreon.com/GaiaLabs

## Sample
![Screenshot 2024-04-25 210500](https://github.com/Azarem/GaiaLabs/assets/7395229/acb007df-34eb-4384-a8ea-628eccea861b)
![Screenshot 2024-04-25 205955](https://github.com/Azarem/GaiaLabs/assets/7395229/acb1481b-f38c-4a73-91ae-763dc15c5b0d)



