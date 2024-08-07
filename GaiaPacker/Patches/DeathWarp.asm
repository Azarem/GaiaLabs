﻿?BANK 0C

!current_scene  0644
!player_health  0ACE

;This script replaces the default death warp process with a lookup table
;This clears up $0AF0 which no longer gets utilized (fixes potential softlock when loading a save file from a modified rom and dying)
;The script also prevents the dungeon clear flags from being reset, so dying does not reset rooms. 
;If dungeon reset is to be enabled again, you would need to add a new hook for 00D6CC instead of replacing the code

----------------------------------------

main:
  LDA $09AE
  BIT #$0008
  BNE do_disabled
  LDA $current_scene
  CMP #$00E6
  BEQ do_disabled
  BRA begin_confirm

do_disabled:
  COP [BF] ( &disabled_message )
  RTL
	
begin_confirm:
  COP [BF] ( &confirm_message )
  COP [BE] ( #02, #02, &confirm_options )

confirm_options:
  &on_cancel
  &on_confirm
  &on_cancel

on_cancel:
  COP [BF] ( &close_msg )
  RTL

on_confirm:
  COP [BF] ( &close_msg )
  LDA #$0200
  TSB $09AE		;Flag player as dying
  LDY $09AA		;Get player actor
  LDA #$D62F
  STA $0000, Y  ;Set new entry point
  LDA #$0080
  STA $0002, Y
  JSL $80F3B3	;This call stops the player and disables weapon collision
  RTL


disabled_message `[DEF]The book is shut tight.[END]`

confirm_message `[DEF]Are you sure you want[N]to open the book?[N] Yes[N] No`

close_msg `[CLD]`

-----------------------------------------------------------------

warp_lookup:
;  0   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
  #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 ;00
  #00 #00 #01 #00 #00 #00 #00 #00 #00 #00 #00 #01 #00 #02 #03 #02 ;10
  #02 #02 #02 #02 #02 #02 #02 #02 #02 #03 #00 #00 #00 #00 #00 #00 ;20
  #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #04 #04 #04 ;30
  #04 #04 #04 #04 #04 #04 #04 #04 #00 #00 #00 #00 #05 #06 #06 #07 ;40
  #07 #08 #08 #09 #09 #05 #00 #00 #00 #00 #0A #0A #0A #0A #0A #0B ;50
  #0B #0B #0B #0B #0B #0B #0C #0C #00 #00 #00 #00 #00 #0D #0D #0D ;60
  #0D #0D #0D #0D #0D #0D #00 #00 #0E #00 #00 #00 #00 #00 #00 #00 ;70
  #00 #00 #0F #0F #00 #0F #0F #0F #0F #00 #10 #00 #00 #00 #00 #00 ;80
  #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 ;90
  #11 #11 #11 #11 #11 #11 #11 #11 #11 #11 #11 #00 #00 #00 #00 #00 ;A0
  #12 #12 #12 #12 #12 #12 #13 #13 #13 #13 #13 #13 #00 #00 #00 #00 ;B0
  #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #00 #14 #15 #16 #16 ;C0
  #17 #17 #18 #18 #19 #19 #1A #1A #1B #1B #1B #1B #00 #15 #00 #00 ;D0
  #1D #1D #1E #1F #00 #00 #00 #00 #22 #23 #23 #00 #00 #00 #00 #00 ;E0
  #00 #00 #1D #1D #1E #1E #1F #00 #00 #00 #00 #00 #00 #00 #00 #00 ;F0


warp_data:
  #0B #$00E8 #$00A0 #83 #$3200  ;00 - Prison cell
  #1B #$00F8 #$01C0 #80 #$2200  ;01 - Moon tribe trial
  #1D #$02A8 #$0060 #83 #$6300  ;02 - Larai cliff
  #1E #$03A8 #$0040 #83 #$2400  ;03 - Cliff base
  #3E #$00A8 #$03D0 #80 #$4200  ;04 - Diamond mine
  #4C #$0168 #$0040 #83 #$2200  ;05 - Sky Garden (Center)
  #4D #$0010 #$00E8 #87 #$4400  ;06 - Sky Garden (East)
  #4F #$0078 #$0030 #83 #$4400  ;07 - Sky Garden (SE)
  #51 #$0378 #$0030 #83 #$4400  ;08 - Sky Garden (SW)
  #53 #$03E0 #$0068 #86 #$4400  ;09 - Sky Garden (West)
  #5A #$0090 #$0070 #83 #$1400  ;0A - Seaside Palace
  #5F #$0080 #$0040 #83 #$4400  ;0B - Mu
  #66 #$00F8 #$01D8 #80 #$2200  ;0C - Mu (boss)
  #6D #$0078 #$0070 #83 #$1400  ;0D - Angel Tunnel
  #79 #$0070 #$00B0 #80 #$1100  ;0E - Watermia House
  #82 #$0020 #$0090 #87 #$1800  ;0F - Great Wall
  #8A #$0050 #$0090 #87 #$3300  ;10 - Great Wall (boss)
  #A0 #$02C8 #$01B0 #86 #$2300  ;11 - Mountain Temple
  #B0 #$01F8 #$04C0 #80 #$5400  ;12 - Ankor Wat (Entrance)
  #B6 #$02F8 #$03D0 #80 #$4600  ;13 - Ankor Wat (Garden)
  #CC #$0010 #$00D0 #87 #$4400  ;14 - Pyramid (Left)
  #CD #$0080 #$00C0 #80 #$1100  ;15 - Pyramid (Puzzle)
  #CE #$0570 #$00C0 #80 #$4600  ;16 - Pyramid 1
  #D0 #$0470 #$00C0 #80 #$5500  ;17 - Pyramid 2
  #D2 #$0070 #$00C0 #80 #$4600  ;18 - Pyramid 6
  #D4 #$0070 #$00C0 #80 #$5500  ;19 - Pyramid 5
  #D6 #$0070 #$00C0 #80 #$5500  ;1A - Pyramid 3
  #D8 #$0480 #$00C0 #80 #$5500  ;1B - Pyramid 4
  #DE #$0178 #$00C0 #80 #$1201  ;1C - Babel Comet Room
  #E0 #$0098 #$0390 #83 #$4830  ;1D - Babel (Floor 1)
  #E0 #$00D8 #$0190 #83 #$2610  ;1E - Babel (Floor 2)
  #E3 #$00D8 #$0390 #83 #$4430  ;1F - Babel (Floor 3)
  #E3 #$0280 #$01A0 #80 #$2310  ;20 - Babel (Floor 4)
  #E4 #$00F0 #$0140 #80 #$2200  ;21 - Babel (Roof)
  #E8 #$0000 #$0000 #80 #$2100  ;22 - Comet Fight
  #E9 #$0330 #$03D0 #80 #$4400  ;23 - Mansion
  
  ;#1C #$0068 #$01A0 #80 #$2200  ;Entrance to ruins
  ;#58 #$0000 #$0000 #80 #$1100  ;Landing on Plane

------------------------------------------------------

do_check:
  PHB
  LDA $player_health
  BEQ do_normal
  JML $80D6CC

do_normal:
  LDA $0AD6
  JML $80D68F

----------------------------------------------------

0090B5:     ;Entry point for COP 26 processing, skip checking the death flag
  LDA $0A
  STA $02, S
  RTI 

00D68B:     ;Entry point for start of death logic
  JML do_check

;(Exact fit, do not modify, use a hook)
00D6CC:     ;Entry point for code that executes when clear data is about to be erased
  PHX
  LDX $current_scene
  LDA %warp_lookup, X
  AND #$00FF
  PLX
  ASL
  ASL
  ASL
  CLC
  ADC #&warp_data
  TAY
  SEP #$20
  LDA #^warp_lookup
  PHA
  PLB


00D70F:     ;Do not set AF8
  NOP
  NOP
  NOP

02AADF:     ;Entry point for checking death warp flag
  PLP
  SEC
  RTS

039427:     ;Entry point for blue journal use
  JSL main
  RTS
