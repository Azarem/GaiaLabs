?BANK 0C

!t_size   $0012
!token1    00E6
!token2    00E8


;This is an example script showing how you can create a teleporter in dark space
;Walk to either side of the screen to trigger a change in the dark space settings
;This will cause you to leave in a different place than you had entered
;To use this script, add a new entry to scene_events under ID E6
;Example: actor < #00, #01, #00, @Teleporter >

--------------------------------------------
  h_actor < #00, #00, #30 >

main:
  COP [C4] ( @location_check )

clear_check:
  LDA #$004A
  STA $token2
  COP [C2]
  DEC $token2
  BEQ clear_screen

location_check:
  COP [28] ( #$00D8, #$00C0, &is_standing_right )
  COP [28] ( #$0028, #$00C0, &is_standing_left )
  RTL

clear_screen:
  COP [BF] ( &clear_str )
  COP [C7] ( @location_check )

is_standing_right:
  LDX $09AA
  LDA #$0029
  STA $0014, X		;Set player position
  LDA #$0003
  STA $09B2			;Set running speed
  
  LDY #t_size		;Set index
  
do_prev:
  DEY
  BMI take_first

  TYA
  ASL
  ASL
  ASL
  ASL
  TAX
  LDA @space_table+A, X
  CMP $0B12
  BEQ take_next
  BMI take_next
  BRA do_prev

is_standing_left:
  LDX $09AA
  LDA #$00D7
  STA $0014, X		;Set player position
  LDA #$FFFD
  STA $09B2			;Set running speed
  
  LDY #$0000

do_next:
  CPY #t_size
  BEQ take_last

  TYA
  ASL
  ASL
  ASL
  ASL
  TAX
  LDA @space_table+A, X
  CMP $0B12
  BPL take_prev
  INY
  BRA do_next

take_prev:
  DEY
  BPL do_change

take_last:
  LDY #t_size-1
  BRA do_change

take_next:
  INY
  CPY #t_size
  BMI do_change

take_first:
  LDY #$0000

do_change:
  TYA
  ASL
  ASL
  ASL
  ASL
  TAX
  LDA @space_table, X
  STA $0B08
  LDA @space_table+2, X
  STA $0B0A
  LDA @space_table+4, X
  STA $0B0C
  LDA @space_table+6, X
  STA $0B0E
  LDA @space_table+8, X
  STA $0B10
  LDA @space_table+A, X
  STA $0B12
  ;LDA @space_table+E, X
  STY $token1
  COP [BF] ( &display_str )
  JMP clear_check

display_str `[DLG:7,6][SIZ:9,1][ADR:&string_table,token1]`
clear_str   `[CLD]`

string_table:
  &south_cape
  &castle_dungeon
  &itory
  &inca_cliff
  &frejia
  &diamond_mine
  &sky_garden
  &seaside_palace
  &angel_village
  &watermia
  &great_wall
  &euro
  &mt_temple
  &native_village
  &ankor_wat
  &dao
  &pyramid
  &babel

space_table:
  #$000E #$0001 #$0005 #$0001 #$4300 #$0001 #$0000 #$0000 ;XPos, XSize, YPos, YSize, CamFlags, Scene
  #$000A #$0001 #$004B #$0001 #$6300 #$0012 #$0000 #$0000
  #$0043 #$0001 #$0007 #$0001 #$3500 #$0015 #$0000 #$0000
  #$002C #$0001 #$0004 #$0001 #$2400 #$001E #$0000 #$0000
  #$0004 #$0001 #$0008 #$0001 #$1100 #$0034 #$0000 #$0000
  #$0027 #$0001 #$0005 #$0001 #$1302 #$0042 #$0000 #$0000
  #$0009 #$0001 #$0005 #$0001 #$2200 #$004C #$0000 #$0000
  #$002F #$0001 #$0007 #$0001 #$1400 #$005A #$0000 #$0000
  #$0019 #$0001 #$0009 #$0001 #$1201 #$006C #$0000 #$0000
  #$0004 #$0001 #$0008 #$0001 #$1100 #$007C #$0000 #$0000
  #$0015 #$0001 #$0046 #$0001 #$6300 #$0086 #$0000 #$0000
  #$000B #$0001 #$0009 #$0001 #$1100 #$0099 #$0000 #$0000
  #$001F #$0001 #$002F #$0001 #$4400 #$00A1 #$0000 #$0000
  #$001C #$0001 #$0003 #$0001 #$2200 #$00AC #$0000 #$0000
  #$0042 #$0001 #$0031 #$0001 #$4600 #$00B6 #$0000 #$0000
  #$0002 #$0001 #$0006 #$0001 #$2300 #$00C3 #$0000 #$0000
  #$0027 #$0001 #$001A #$0001 #$4400 #$00CC #$0000 #$0000
  #$0046 #$0001 #$0039 #$0001 #$4830 #$00E0 #$0000 #$0000

south_cape `South Cape`

castle_dungeon `Castle Dungeon`

itory `Itory`

inca_cliff `Inca Cliff`

frejia `Frejia`

diamond_mine `Diamond Mine`

sky_garden `Sky Garden`

seaside_palace `Seaside Palace`

angel_village `Angel Village`

watermia `Watermia`

great_wall `Great Wall`

euro `Euro`

mt_temple `Mt Temple`

native_village `Native Village`

ankor_wat `Ankor Wat`

dao `Dao`

pyramid `Pyramid`

babel `Babel`