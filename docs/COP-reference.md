# GaiaLabs COP Command Reference

## COP Command Format
```
COP [mnemonic] ( param1: type, param2: type, ... )
```

## Type System

### Core Types
- **byte** - 8-bit value (u8)
- **word** - 16-bit value (u16)
- **addr** - 24-bit address (u24)
- **ptr** - 16-bit pointer/offset (u16)

### Type Notation
- **type** - Direct value of that type
- **ptr<type>** - 2-byte pointer to type
- **addr<type>** - 3-byte address to type
- **ptr<addr<type>>** - 2-byte pointer to a 3-byte address of type

### Structure Types

**dma_data** - DMA transfer data structure
```
struct dma_data {
    channel: byte
    register: byte  
    data: byte
}
```

**sprite_set** - Sprite animation set
```
struct sprite_set {
    animations: array<ptr<sprite_group>>  // terminated by $FFFF
}
```

**code** - Executable code entry point

**string** - Text string data

**widestring** - Wide character text data

## COP Commands

### System & DMA Commands

**COP 00 ( )**
- Generate scaled 16-bit sine @ $7E8900-CFF for HDMA

**COP 01 ( src: addr<dma_data>, reg: byte )**
- Queue H/DMA to register `reg` on available channel
- Source points to DMA data structure

**COP 02 ( src: addr<dma_data>, reg: byte )**
- Queue DMA to register `reg` on available channel
- Source points to DMA data structure

**COP 03 ( channel: byte, src_addr: addr, reg: byte )**
- Queue H/DMA to register using specified channel (1-7)
- Direct address format (not using dma_data structure)

### Audio Commands

**COP 04 ( music_id: byte )**
- Start music

**COP 05 ( music_id: byte )**
- Fade out, then start music

**COP 06 ( sound_id: byte )**
- Play sound, second channel

**COP 07 ( sound_id: byte )**
- Play sound, first channel

**COP 08 ( sound_ids: word )**
- Play sounds on both channels
- Low byte = first channel, high byte = second channel

**COP 09 ( shift: byte )**
- Tempo modifier

**COP 0A ( raw_val: byte )**
- Explicit write to $2140 for APU programming

### Collision & Solidity Commands

**COP 0B ( )**
- Set BG tile solidity mask to $Fx (wall) here

**COP 0C ( )**
- Set BG tile solidity mask to $0x (clear) here

**COP 0D ( offs_x: byte, offs_y: byte )**
- Set BG tile solidity mask to $Fx, relative

**COP 0E ( offs_x: byte, offs_y: byte )**
- Set BG tile solidity mask to $0x, relative

**COP 0F ( abs_x: byte, abs_y: byte )**
- Set BG tile solidity mask to $Fx, absolute

**COP 10 ( abs_x: byte, abs_y: byte )**
- Set BG tile solidity mask to $0x, absolute

**COP 11 ( )**
- Set BG tile solidity mask to $00 on all tiles touched by actor

**COP 12 ( abs_x: byte, abs_y: byte )**
- Set BG tile solidity mask to $x0, absolute

### Collision Branching Commands

**COP 13 ( jmp_addr: ptr<code> )**
- Branch if this solid mask is not $00

**COP 14 ( offs_x: byte, offs_y: byte, jmp_addr: ptr<code> )**
- Branch if relative solid mask is not $00

**COP 15 ( jmp_addr: ptr<code> )**
- Branch if north solid mask is not $00

**COP 16 ( jmp_addr: ptr<code> )**
- Branch if south solid mask is not $00

**COP 17 ( jmp_addr: ptr<code> )**
- Branch if west solid mask is not $00

**COP 18 ( jmp_addr: ptr<code> )**
- Branch if east solid mask is not $00

**COP 19 ( music_id: byte, text: addr<widestring> )**
- Music and text, similar to COP 04 + COP BF

**COP 1A ( type: byte, jmp_addr: ptr<code> )**
- Branch if this solid mask is `type`

**COP 1B ( type: byte, jmp_addr: ptr<code> )**
- Branch if north solid mask is `type`

**COP 1C ( type: byte, jmp_addr: ptr<code> )**
- Branch if south solid mask is `type`

**COP 1D ( type: byte, jmp_addr: ptr<code> )**
- Branch if west solid mask is `type`

**COP 1E ( type: byte, jmp_addr: ptr<code> )**
- Branch if east solid mask is `type`

**COP 1F ( jmp_addr: ptr<code> )**
- Branch if not on gridline

### Distance & Position Checks

**COP 20 ( actor_num: byte, dist: byte, jmp_addr: ptr<code> )**
- Branch if actor number `actor_num` from the map's actor list is within `dist`

**COP 21 ( dist: byte, jmp_addr: ptr<code> )**
- Branch if player is within `dist`

**COP 22 ( sprite_id: byte, speed: byte )**
- Basic movement up to $FE pixels
- Write destination to $7F:18,$7F:1A before calling

**COP 23 ( )**
- RNG, range 0..$FF, returns 8-bit result in A
- Warning: very expensive call

**COP 24 ( max: byte )**
- RNG, range 0..`max`, returns 8-bit result in $0420
- Warning: very expensive call

**COP 25 ( abs_x: byte, abs_y: byte )**
- Set new position

**COP 26 ( map_num: byte, pos_x: word, pos_y: word, dummy: byte, target: word )**
- Queue map change at end of frame

**COP 27 ( delay: byte )**
- If off-screen, wait for `delay` frames, then check again

**COP 28 ( pos_x: word, pos_y: word, jmp_addr: ptr<code> )**
- Branch if player is at position

**COP 29 ( actor_num: byte, pos_x: word, pos_y: word, jmp_addr: ptr<code> )**
- Branch if actor number `actor_num` is at position

**COP 2A ( dist: word, west_addr: ptr<code>, here_addr: ptr<code>, east_addr: ptr<code> )**
- Branch on whether PlayerX is within `dist`, too far west, or too far east

**COP 2B ( dist: word, north_addr: ptr<code>, here_addr: ptr<code>, south_addr: ptr<code> )**
- Branch on whether PlayerY is within `dist`, too far north, or too far south

**COP 2C ( near_y_addr: ptr<code>, near_x_addr: ptr<code> )**
- Branch on whether Player is nearer in y or x dimension

### Direction Commands

**COP 2D ( )**
- Return A=DirToPlayer, 0/1/2 = N/NE/E etc.

**COP 2E ( offs_x: byte, offs_y: byte )**
- Return A=DirToPlayer from relative location

**COP 2F ( dir_to_player: byte, jmp_addr: ptr<code> )**
- Branch if DirToPlayer is...

**COP 30 ( offs_x: byte, offs_y: byte, dir_to_player: byte, jmp_addr: ptr<code> )**
- Branch if DirToPlayer from relative location is...

**COP 31 ( south_addr: ptr<code>, north_addr: ptr<code>, west_addr: ptr<code>, east_addr: ptr<code>, dummy: ptr<code> )**
- Branch on Player's facing direction

### Background & Palette Commands

**COP 32 ( bg_chg: byte )**
- Stage BG tilemap change (e.g. opening door)
- Data from $81d3ce + 8*`bg_chg`

**COP 33 ( )**
- Perform staged BG tilemap change

**COP 34 ( )**
- Castoth door macro
- Equivalent to: COP 32 : db $7F:24 : COP 08 : db $0f,$0f : COP 33

**COP 35 ( )**
- Return A=CardinalToPlayer, 0/1/2/3 = N/E/S/W

**COP 36 ( )**
- Palette handlers: Restart palette bundle

**COP 37 ( pal_bundle_index: byte )**
- Palette handlers: Start new palette bundle

**COP 38 ( pal_bundle_index: byte, iters: byte )**
- Palette handlers: Start new palette bundle and prepare to loop `iters` times

**COP 39 ( )**
- Palette handlers: Advance palette bundle, exit if more palettes remain

**COP 3A ( )**
- Palette handlers: Advance or restart palette bundle, exit if more palettes or iters remain

### Thinker Commands

**COP 3B ( param: byte, entry_ptr: addr<code> )**
- Spawn new thinker running `entry_ptr` with parameter `param`

**COP 3C ( entry_ptr: addr<code> )**
- Spawn new thinker running `entry_ptr`

**COP 3D ( )**
- Thinker only: Mark for death after thinker returns this frame

### Button Input Commands

**COP 3E ( btn_mask: word )**
- Exit if buttons in `btn_mask` are not pressed this frame
- Add 1 to `btn_mask` to include previous frame

**COP 3F ( btn_mask: word )**
- Exit if buttons in `btn_mask` are pressed this frame
- Add 1 to `btn_mask` to include previous frame

**COP 40 ( btn_mask: word, jmp_addr: ptr<code> )**
- Branch if buttons in `btn_mask` are pressed this frame
- Add 1 to include previous frame

**COP 41 ( btn_mask: word, jmp_addr: ptr<code> )**
- Branch if buttons in `btn_mask` are not pressed this frame
- Add 1 to include previous frame

### Position & Grid Commands

**COP 42 ( abs_x: byte, abs_y: byte, type: byte )**
- Set BG tile solidity mask to `type` at absolute location

**COP 43 ( )**
- Snap self to grid

**COP 44 ( x_left: byte, y_up: byte, x_right: byte, y_down: byte, jmp_addr: ptr<code> )**
- Branch if Player is in signed relative tile area

**COP 45 ( x_left: byte, y_top: byte, x_right: byte, y_bot: byte, jmp_addr: ptr<code> )**
- Branch if Player is in absolute tile area

**COP 46 ( )**
- Set position of previous actor (ID in $04) to here

**COP 47 ( )**
- Set position of next actor (ID in $06) to here

**COP 48 ( )**
- Return player facing direction, 0/1/2/3 = S/N/W/E

**COP 49 ( player_body: byte, jmp_addr: ptr<code> )**
- Branch if Player's Body is not `player_body`
- 0=Will, 1=Freedan, 2=Shadow

**COP 4A ( )**
- Utility COP for #$43, probably no ad-hoc use

### Map & VRAM Commands

**COP 4B ( pos_x: byte, pos_y: byte, metatile_index: byte )**
- Draw metatile with collision during VBlank
- Hangs the actor for ~2 frames

**COP 4C ( arg1: byte )**
- Unknown, used by world map

**COP 4D ( arg1: word )**
- Unknown, used by world map

**COP 4E ( arg1: word )**
- Unknown, used by world map

**COP 4F ( src_addr: addr, vram_word: word, xfer_size_b: word )**
- Queue ad hoc DMA of `xfer_size_b` bytes to VRAM at `vram_word`

**COP 50 ( src_addr: addr, offs_w: byte, pal_word: byte, xfer_size_w: byte )**
- MVN of `xfer_size_w` words from src_addr+2*`offs_w` to palette stage at $7F0A00+2*`pal_word`

**COP 51 ( src_addr: addr, dest_addr: addr )**
- Decompress data at `src_addr` into `dest_addr` in bank $7E

### Movement Commands

**COP 52 ( sprite_id: byte, speed: byte, max_time: byte )**
- Stage movement; write destination to $7F:18,$7F:1A before calling
- max_time<0 means no limit

**COP 53 ( )**
- Perform movement staged by COP 52

**COP 54 ( arg: addr )**
- Utility function, sets $7F0000,x = arg and $7F0003,x = $00

**COP 55 ( spr: byte, new24_25: word )**
- Resets sprite (as COP 80) and sets $24 and $25

**COP 56 ( )**
- Unknown use, advances sprite animation based on global state

### Actor Property Commands

**COP 57 ( on_death: addr<code> )**
- Set OnDeath pointer

**COP 58 ( on_hit: ptr<code> )**
- Set OnHit pointer

**COP 59 ( dodge: ptr<code> )**
- Set Dodge pointer

**COP 5A ( on_collide: ptr<code> )**
- Set OnCollide pointer

**COP 5B ( arg: word )**
- Set $7F:2A = bitwise OR of $7F:2A with arg

**COP 5C ( arg: word )**
- Set $7F:2A = bitwise AND of $7F:2A with arg

**COP 5D ( jmp_addr: ptr<code> )**
- Branch if low-priority sprite and behind wall
- Priority bits in $0e are unset and BG tile solidity mask is $xE or $xF

**COP 5E ( arg: ptr<code> )**
- Set $7F1016,x

### HDMA Sine Commands

**COP 5F ( base_addr: word, bytes_per_period: byte )**
- Initialize scaled sines for HDMA
- Must write 2*Amplitude to $7F:08 before calling
- Uses 2KB in bank $7E

**COP 60 ( delay: byte, scroll_layer: byte )**
- Advance scaled sines, offset by PPU scroll value
- BG1 (scroll_layer=0) or BG2 (scroll_layer=2)

**COP 61 ( src_addr: addr, reg: byte )**
- Queue HDMA to reg, intended for use with COP 5F : COP 60

**COP 62 ( match_tile: byte, jmp_addr: ptr<code> )**
- Duplicate of COP 1A

### Physics Commands

**COP 63 ( init_speed: byte, neg_log_a: byte, gnd_tile_pos: byte )**
- Stage gravity

**COP 64 ( )**
- Do gravity (must rtl to move)

### World Map Commands

**COP 65 ( pos_x: word, pos_y: word, dummy: byte, wmap_move_id: byte )**
- Stage world map movement from pixels
- Uses move script pointer indexed at $83ad77
- Follow with COP 26 to perform transition

**COP 66 ( pos_x: word, pos_y: word, wmap_opts_id: byte )**
- Stage world map choices from pixels
- Uses text box code pointer indexed at $83b401
- Follow with COP 26 to perform transition

**COP 67 ( dummy: byte, wmap_move_id: byte )**
- As COP 65 without setting position
- Used when already on world map

**COP 68 ( jmp_addr: ptr<code> )**
- Branch if off-screen

**COP 69 ( min: word )**
- Exit if $00E4 < min

**COP 6A ( new_addr: ptr<code> )**
- Set CodePtr of Actor06

**COP 6B ( text_addr: ptr<widestring> )**
- Text script (alt version with no screen refresh)

**COP 6C ( new12: byte, new10: byte )**
- Set $7F:12,10 = new12,new10

**COP 6D ( diameter_speed: byte, angle_speed: byte )**
- Spiral about actor whose ID is stored at $0000

### Sprite Animation Commands

**COP 80 ( spr: byte )**
- Stage new sprite `spr`
- #$8x = HMirror; spr=$FF to reset current animation

**COP 81 ( spr: byte, x_move: byte )**
- Stage sprite and X movement

**COP 82 ( spr: byte, y_move: byte )**
- Stage sprite and Y movement

**COP 83 ( spr: byte, x_move: byte, y_move: byte )**
- Stage sprite and X+Y movement

**COP 84 ( spr: byte, iters: byte )**
- Stage sprite animation loop `iters` times

**COP 85 ( spr: byte, iters: byte, x_move: byte )**
- Stage sprite loop and X movement for `iters`

**COP 86 ( spr: byte, iters: byte, y_move: byte )**
- Stage sprite loop and Y movement for `iters`

**COP 87 ( spr: byte, iters: byte, x_move: byte, y_move: byte )**
- Stage sprite loop and X+Y movement for `iters`

**COP 88 ( metasprite_addr: ptr<addr<sprite_set>> )**
- Set new metasprite data address
- Points to a sprite set structure

**COP 89 ( )**
- Animate and/or move sprite for one iteration
- Exits each frame if unfinished

**COP 8A ( )**
- Animate and/or move sprite, all staged iterations
- Exits each frame if unfinished

**COP 8B ( )**
- Animate and/or move one frame only, without exiting

**COP 8C ( spr_frame: byte )**
- Do sprite loops, but continue if at `spr_frame`

**COP 8D ( spr: byte )**
- Stage sprite as COP 80, and update hitbox size if permitted

### Player Sprite Commands

**COP 8E ( player_spr: byte )**
- Stage Player special sprite

**COP 8F ( body_spr: byte )**
- Stage Player normal sprite

**COP 90 ( body_spr: byte, x_move: byte )**
- Stage Player normal sprite with X movement

**COP 91 ( body_spr: byte, y_move: byte )**
- Stage Player normal sprite with Y movement

**COP 92 ( body_spr: byte, x_move: byte, y_move: byte )**
- Stage Player normal sprite with X+Y movement

**COP 93 ( )**
- Duplicate of COP 89

**COP 94 ( body_spr: byte, x_move: byte, y_move: byte, wall_type: byte )**
- (unused) As COP 92 but would have set wall_type for COP 96-98

**COP 95 ( )**
- As COP 8F but use value at $0000 for body_spr

**COP 96 ( btn_mask_trigger: word )**
- (unused) After COP 94, would animate and set flag if this tile solid mask were wall_type

**COP 97 ( btn_mask_trigger: word )**
- (unused) After COP 94, would animate and set flag if north tile solid mask were wall_type

**COP 98 ( btn_mask_trigger: word )**
- (unused) After COP 94, would animate and set flag if south tile solid mask were wall_type

### Actor Spawning Commands

**COP 99 ( spawn_addr: addr<code> )**
- Spawn new actor before This in list (ID in $04)
- Returns new ID in Y

**COP 9A ( spawn_addr: addr<code>, new10: word )**
- As COP 99, setting new actor's $10 = new10

**COP 9B ( spawn_addr: addr<code> )**
- Spawn new actor after This in list (ID in $06)
- Returns new ID in Y

**COP 9C ( spawn_addr: addr<code>, new10: word )**
- As COP 9B, setting new actor's $10 = new10

**COP 9D ( spawn_addr: addr<code>, offs_x: word, offs_y: word )**
- As COP 9B, spawning at relative position

**COP 9E ( spawn_addr: addr<code>, offs_x: word, offs_y: word, new10: word )**
- As COP 9C and COP 9D combined

**COP 9F ( spawn_addr: addr<code>, abs_x: word, abs_y: word )**
- As COP 9B, spawning at absolute position

**COP A0 ( spawn_addr: addr<code>, abs_x: word, abs_y: word, new10: word )**
- As COP 9C and COP 9F combined

**COP A1 ( child_addr: addr<code>, new10: word )**
- As COP 9A, marking new actor as Child

**COP A2 ( child_addr: addr<code>, new10: word )**
- As COP 9C, marking new actor as Child

**COP A3 ( child_addr: addr<code>, abs_x: word, abs_y: word, new10: word )**
- As COP A0, marking new actor as Child

**COP A4 ( child_addr: addr<code>, offs_x: byte, offs_y: byte, new10: word )**
- As COP 9E with 8-bit pixel offsets, marking new actor as Child

**COP A5 ( child_addr: addr<code>, offs_x: byte, offs_y: byte, new10: word )**
- As COP A4, placing child Last in execution order rather than Next

**COP A6 ( child_addr: addr<code>, spr: byte, offs_x: byte, offs_y: byte, new10: word )**
- Broken; would have been as COP A5, also setting Child's sprite

### Actor Death Commands

**COP A7 ( )**
- Mark actor for death after next return
- Also kills children if so flagged

**COP A8 ( )**
- Kill Actor04

**COP A9 ( )**
- Kill Actor06

### Movement Storage Commands

**COP AA ( x_move: byte )**
- Stage and save x_move

**COP AB ( y_move: byte )**
- Stage and save y_move

**COP AC ( x_move: byte, y_move: byte )**
- Stage and save X/Y move

**COP AD ( force_sw: byte )**
- Set/clear forced south/west movement

**COP AE ( force_ne: byte )**
- Set/clear forced north/east movement

**COP AF ( force_neg: byte )**
- Set/clear both, to force negative movement

**COP B0 ( x_move_l: byte, y_move_l: byte )**
- Stage and save X/Y move for Last actor

**COP B1 ( )**
- Load saved movement

### Priority & Mirror Commands

**COP B2 ( )**
- Set max collision priority flag

**COP B3 ( )**
- Set min collision priority flag

**COP B4 ( )**
- Clear max collision priority flag

**COP B5 ( )**
- Clear min collision priority flag

**COP B6 ( new_priority: byte )**
- Update sprite priority bits (in $0F)

**COP B7 ( new_palette: byte )**
- Update sprite palette bits (in $0F)

**COP B8 ( )**
- Toggle HMirror

**COP B9 ( )**
- Toggle VMirror

**COP BA ( )**
- Unset HMirror

**COP BB ( )**
- Set HMirror

**COP BC ( offs_x: byte, offs_y: byte )**
- Set new position immediate

### Text & Dialogue Commands

**COP BD ( bg3_script: addr )**
- Run BG3 script (e.g. drawing status bar or text)

**COP BE ( opt_counts: byte, skip_lines: byte, options_list: ptr<ptr<code>> )**
- Dialogue box options
- Must print box and text with COP BF then call this
- Options list structure: word cancel_addr [, word choice_addr1, ...]

**COP BF ( text_addr: ptr<widestring> )**
- Display text message

### Control Flow Commands

**COP C0 ( on_interact: ptr<code> )**
- Set EntryPtr on player chat/pickup

**COP C1 ( )**
- Set EntryPtr here, and continue

**COP C2 ( )**
- Set EntryPtr here, and exit

**COP C3 ( new_ptr: addr<code>, delay: word )**
- Set EntryPtr there, exit, and wait `delay` frames

**COP C4 ( new_ptr: addr<code> )**
- Set EntryPtr there, and exit

**COP C5 ( )**
- Restore SavedPtr

**COP C6 ( saved_ptr: ptr<code> )**
- Set SavedPtr

**COP C7 ( new_ptr: addr<code> )**
- Like JML: set EntryPtr, and continue there

**COP C8 ( sub_ptr: ptr<code> )**
- Like JSR: set SavedPtr here, EntryPtr at sub_ptr, and continue there

**COP C9 ( sub_ptr: ptr<code> )**
- Like delayed JSR: set SavedPtr here, EntryPtr at sub_ptr, and exit

**COP CA ( iters: byte )**
- Loop from here to next COP CB, `iters` times

**COP CB ( )**
- Loop end, exit if unfinished iters, otherwise continue

### Flag Commands

**COP CC ( set_flag: byte )**
- Set game flag, range 0..$FF

**COP CD ( set_flag_w: word )**
- Set game flag, range 0..$FFFF

**COP CE ( clear_flag: byte )**
- Clear flag

**COP CF ( clear_flag_w: word )**
- Clear flag

**COP D0 ( flag: byte, val: byte, if_then_addr: ptr<code> )**
- Branch if `flag` is `val` (0/1)

**COP D1 ( flag_w: word, val: byte, if_then_addr: ptr<code> )**
- Branch if `flag_w` is `val` (0/1)

**COP D2 ( flag: byte, val: byte )**
- Exit if `flag` is not `val` (0/1)

**COP D3 ( flag_w: word, val: byte )**
- Exit if `flag_w` is not `val` (0/1)

### Item Commands

**COP D4 ( add_item_id: byte, full_inv_addr: ptr<code> )**
- Give item, branching if inventory is full

**COP D5 ( remove_item_id: byte )**
- Remove item

**COP D6 ( item_id: byte, has_item_addr: ptr<code> )**
- Branch if Player has item

**COP D7 ( item_id: byte, equipped_item_addr: ptr<code> )**
- Branch if item is equipped

**COP D8 ( )**
- Set dungeon-level monster killed flag

### Miscellaneous Commands

**COP D9 ( index_addr: word, jmp_list: ptr<ptr<code>> )**
- Switch-case statement
- Equivalent to: ldx index_addr : jmp (jmp_list,x)

**COP DA ( delay: byte )**
- Exit and wait for `delay` frames, range 0..$FF

**COP DB ( delay: word )**
- Exit and wait for `delay` frames, range 0..$7FFF

**COP DC ( )**
- Unclear, conditions on obscure globals

**COP DD ( )**
- Unclear, conditions on obscure globals

**COP DE ( )**
- Unclear, conditions on obscure globals

**COP DF ( )**
- Unclear, conditions on obscure globals

**COP E0 ( )**
- Mark for death (with children, if flagged so) and return immediately

**COP E1 ( )**
- Restore SavedPtr and set A=#$FFFF

**COP E2 ( new_ptr: addr<code> )**
- Set EntryPtr, but continue here this frame

## Type Reference Quick Guide

| Notation | Size | Description | Example |
|----------|------|-------------|---------|
| byte | 8-bit | Direct byte value | `sprite_id: byte` |
| word | 16-bit | Direct word value | `delay: word` |
| ptr<T> | 16-bit | Pointer to type T | `jmp_addr: ptr<code>` |
| addr<T> | 24-bit | Long address to type T | `entry_ptr: addr<code>` |
| ptr<addr<T>> | 16-bit | Pointer to a long address | `metasprite: ptr<addr<sprite_set>>` |
| ptr<ptr<T>> | 16-bit | Pointer to pointer array | `jmp_list: ptr<ptr<code>>` |

## Notes

- Commands that include "halt: true" in the JSON will exit the current actor's execution for the frame
- Most commands expect processor state: m=0, x=0, d=0, i=1, X=ActorID, D=ActorID, DBR=$81
- The type system clearly shows pointer indirection levels and target types
- Structure types (dma_data, sprite_set, etc.) indicate complex data formats
- Code pointers can be 2-byte (ptr<code>) or 3-byte (addr<code>) depending on the command