?BANK 03

?INCLUDE 'chunk_008000'
?INCLUDE 'chunk_03BAE1'
?INCLUDE 'chunk_028000'

!meta_next_id                   $0642
!meta_current_id                $0644
!token                          $00E6
!msu_flag                       $0D73
!INIDISP                        2100
!MOSAIC                         2106
!APUIO0                         2140

---------------------------------------

bgm_table [
  #FF #1C #1C #1C #1C #1C #1C #1C #1C #FF #05 #1B #06 #06 #06 #06  ;0F
  #06 #06 #06 #06 #FF #03 #03 #03 #03 #04 #04 #06 #04 #07 #07 #07  ;1F
  #07 #07 #07 #07 #07 #07 #07 #07 #07 #07 #11 #04 #04 #04 #04 #15  ;2F
  #1B #1B #02 #02 #02 #02 #02 #02 #02 #02 #02 #02 #04 #06 #06 #06  ;3F
  #06 #06 #06 #06 #06 #06 #06 #04 #FF #03 #FF #04 #08 #08 #08 #08  ;4F
  #08 #08 #08 #08 #08 #08 #08 #FF #FF #FF #04 #04 #04 #04 #04 #09  ;5F
  #09 #09 #09 #04 #09 #09 #04 #04 #04 #03 #03 #03 #03 #06 #06 #06  ;6F
  #06 #06 #06 #1D #04 #04 #FF #FF #02 #02 #02 #06 #02 #02 #02 #15  ;7F
  #FF #FF #0A #0A #FF #0A #0A #0A #0A #FF #0A #0A #14 #FF #FF #FF  ;8F
  #FF #02 #02 #02 #02 #05 #05 #02 #02 #02 #02 #02 #02 #04 #FF #FF  ;9F
  #06 #06 #06 #06 #06 #06 #06 #06 #06 #06 $FF #FF #04 #FF #FF #FF  ;AF
  #04 #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #04  ;BF
  #11 #FF #FF #02 #02 #04 #02 #02 #02 #02 #FF #FF #04 #04 #0C #0C  ;CF
  #0C #0C #0C #0C #0C #0C #0C #0C #0C #0C #0C #0C #02 #0F #04 #04  ;DF
  #04 #04 #04 #04 #0E #0E #16 #FF #FF #06 #FF #FF #FF #FF #FF #FF  ;EF
  #1B #FF #0F #0F #0F #0F #0F #1B #FF #13 #12 #1B #14 #1B #12 #FF  ;FF
]

bgm_track_map [
  #00 #06 #06 #07 #05 #09 #0A #0C #0F #10 #0B #08 #11 #06 #13 #12
  #14 #0D #02 #15 #16 #0E #04 #00 #18 #17 #19 #00 #03 #00 #1A #01
]

bgm_loop_map [
  #00 #00 #01 #01 #01 #01 #01 #01 #01 #01 #01 #01 #01 #00 #01 #01
  #01 #01 #01 #01 #00 #01 #01 #00 #00 #00 #00 #00 #01 #01 #00 #01
]


---------------------------------------
;Function that queues a music fade on a specific frame (from A)

count_check {
    PHA
    LDA token
    BEQ count_ret
    ;BRK #$00
    BMI no_msu_fade

    ;LDA $0D72
    ;BNE no_msu_fade

    ;LDA msu_flag
    ;BEQ no_msu_fade
    
    LDA $01, S
    DEC
    BEQ msu_reset

    ASL
    ASL
    ASL
    ASL
    ORA #0F
    STA $2006
    BRA count_ret

  msu_reset:
    STZ $2006
    STZ $2007
    STZ token
    BRA count_ret
    
  no_msu_fade:
    LDA $01, S
    CMP #06
    BNE count_ret

    LDA #F2
    STA $APUIO0
    STZ token

  count_ret:
    PLA
    RTS
}

---------------------------------------

cop_handler_0A_0087C9 {
    SEP #$20
    LDA msu_flag
    BEQ cop0A_normal           ;Normal process when no MSU

    LDA $0D72
    CMP #$1B
    BNE cop0A_normal           ;Normal process when current track is not 1B
    
    LDA [$0A]
    ;INC $0A
    ;AND #$00FF
    CMP #$7F
    BEQ cop0A_msu_pause        ;Stop playback on 7F
    
    LDX $06F2
    LDA @bgm_loop_map, X
    BEQ $04

    LDA #$03
    BRA $02

    LDA #$01

    STA $2007                   ;Start playback on other commands (only 01)
    BRA cop0A_exit

  cop0A_msu_pause:
    STZ $2007
    BRA cop0A_exit

  cop0A_normal:
    LDA [$0A]
    STA $APUIO0

  cop0A_exit:
    REP #$20
    TYX
    LDA $0A
    INC
    STA $02, S
    RTI 
}

--------------------------------------
;Hook for SPC init

func_02908E {
    ;BRK #$00
    REP #$20

    LDA $2002
    CMP #$2D53
    BNE msu_unavailable
    LDA $2004
    CMP #$534D
    BNE msu_unavailable
    LDA $2006
    CMP #$3155
    BNE msu_unavailable
    
    SEP #$20
    LDA #01
    STA msu_flag
    BRA init_complete

  msu_unavailable:
    SEP #$20
    STZ msu_flag
    
  init_complete:
    LDX #$&binary_029210
    STX $46
    LDA #$^binary_029210
    STA $48
    JSR $&sub_02919B
    RTL 
}

--------------------------------------
;Hook for checking track changes before screen transition

func_03D9F6 {
    LDA $0654
    BMI code_03DA03
    BEQ code_03DA00
    
  ;do_check:
    ;BRK #$00
    PHX
    PHY
    STZ token
    LDA $0D72           ;Is music playing?
    BEQ change_return          ;Should only be on boot
    LDA $0D5A
    BNE change_accept          ;Scenario where map movement is staged before transition
    LDX meta_next_id
    BEQ change_accept
    LDA @bgm_table, X
    BMI change_return          ;This is important
    TAY
    LDX meta_current_id
    LDA @bgm_table, X
    BMI change_return
    TYA
    CMP @bgm_table, X
    BEQ change_return

  change_accept:
    LDA $0D72
    CMP #1B
    BNE do_f2

    LDA msu_flag
    BEQ change_return

    LDA #6B             ;Flag an MSU fade with a positive value (any value works)
    BRA $02

  do_f2:
    LDA #F2
    STA token          ;Store value #F2 in token to flag a music fade
  
  change_return:
    PLY
    PLX
    JSR $&sub_03DABB
}

-----------------------------------------------

code_03DADC {
    JSL $@func_00811E
    LDA $0DB6
    BEQ code_03DAF1
    DEX 
    BPL code_03DADC
    STA $INIDISP

    JSR count_check
    DEC 
    STA $0DB6
    BPL code_03DAD5
}

----------------------------------------------

code_03DAF2 {
    LDA token
    BEQ immed_skip
    BMI immed_bgm

    STZ $2007
    STZ token
    BRA immed_skip
    
  immed_bgm:
    LDA #F2
    STA $APUIO0
    STZ token

  immed_skip:
    JSL $@func_00811E
    STZ $INIDISP
    RTS 
}

-------------------------------------------
;Mosaic mode 

code_03DB05 {
    JSL $@func_00811E
    DEX 
    BPL code_03DB05
    STA $INIDISP
    PHA 
    EOR #$0F
    ASL 
    ASL 
    ASL 
    ASL 
    ORA #$03
    STA $MOSAIC
    PLA

    JSR count_check
    DEC 
    BPL code_03DAFC    ;This loops through 0
    RTS 
}

--------------------------------------------

code_03DB6F {
    LDA $006E
    STA $00
    PHA 
    JSL $@func_03E146
    LDA #$FF
    STA $6C
    PLA 
    JSR $&sub_03DBF6
    INC 
    STA $006E
    LDA $02, S
    DEC 
    STA $02, S
    BNE code_03DB6F
    LDA $01, S
    
    JSR count_check
    DEC 
    STA $01, S
    STA $INIDISP
    BNE code_03DB6A
    LDA #$00
    STA $INIDISP
    PLA 
    PLA 
    STZ $0070
    STZ $006E
    RTS 
}

-------------------------------------------------

code_03DBBC {
    INC $006E
    STA $00
    PHA 
    JSL $@func_03E146
    LDA #$FF
    STA $6C
    PLA 
    JSR $&sub_03DBF6
    JSL $@func_00811E
    LDA $01, S
    DEC 
    STA $01, S
    BNE code_03DBBC
    LDA $064A
    STA $01, S
    LDA $02, S

    JSR count_check
    DEC 
    STA $02, S
    STA $INIDISP
    BNE code_03DBBC
    LDA #$00
    STA $INIDISP
    PLA 
    PLA 
    STZ $0070
    STZ $006E
    RTS 
}

---------------------------------------------
;Hook for fading music via COP 05

func_03E1AA {
    SEP #$20
    LDA msu_flag
    BEQ cop_fade_normal     ;Assume normal process when no MSU
    
    REP #$20
    LDA #$00FF
    STA $24
    COP [C1]
    LDA $24
    BEQ func_03E1D6
    DEC
    SEP #$20
    STA $2006
    REP #$20
    STA $24
    RTL

  cop_fade_normal:
    LDA #$F1
    STA $APUIO0
    REP #$20
    COP [C2]
    LDA $APUIO0
    AND #$00FF
    CMP #$00F1
    BEQ code_03E1C1
    RTL 
}


code_03E1C1 {
    SEP #$20
    LDA #$01
    STA $APUIO0
    REP #$20
    COP [C2]
    SEP #$20
    LDA $APUIO0
    REP #$20
    BEQ func_03E1D6
    RTL 
}

---------------------------------------------
;Hook for stopping music via COP 04/05

func_03E1D6 {
    SEP #$20
    ;BRK #$00
    LDA msu_flag
    BEQ bgm_load_wait   ;Always skip APU silent since we are always branching to the standard load process

    ;LDA $0D72
    ;CMP #$1B
    ;BNE bgm_load_wait   ;Normal process when current track is not 1B
    
    LDA #$00
    STA $2007
    BRA bgm_load_wait     ;Stop playback, then trigger music load

  ;cop_stop_normal:
    ;LDA #$F0
    ;STA $APUIO0
    ;REP #$20
    ;COP [C2]
    ;SEP #$20
    ;LDA $APUIO0
    ;REP #$20
    ;BEQ code_03E1EB
    ;RTL 
}

code_03E1EB {
    ;COP [C2]
    ;SEP #$20
    ;LDA #$FF
    ;STA $APUIO0

  bgm_load_wait:
    REP #$20
    LDA $7F000A, X
    STA $06FA
    COP [C2]
    LDA $06FA
    CMP #$FFFF
    BEQ code_03E208
    RTL 
}

code_03E208 {
    COP [DA] ( #01 )
    ;SEP #$20
    ;LDA #$01
    ;STA $APUIO0
    ;REP #$20
    COP [C2]
    STZ $06F8
    STZ $06FA
    COP [E0]
}

---------------------------------------------

func_03E21E {
    LDX $06FA
    BEQ code_03E254
    BMI code_03E254

  ;immed_load_normal:
    REP #$20
    TXA 
    ASL 
    CLC 
    ADC $06FA
    TAX 
    
    ;LDA msu_flag
    ;AND #$00FF
    ;BEQ immed_load_continue
    
    LDA $@music_array_01CBA6-3, X
    STA $3E
    STA $0687
    LDA $@music_array_01CBA6-2, X
    STA $3F
    STA $0688

    LDA $06FA
    STA $06F2
    PEA &bgm_load_exit-1
    SEP #$20
    JML @code_028B91

  ;immed_load_continue:
    ;LDA $@music_array_01CBA6-3, X
    ;STA $46
    ;STA $0687
    ;LDA $@music_array_01CBA6-2, X
    ;STA $47
    ;STA $0688
    ;JSL $@func_028191
    ;JSL $@func_02909B
    ;JSL $@func_0281A2
    
    ;LDA $06FA
    ;STA $06F2
    ;SEP #$20
    ;STA $0D72
    ;LDA #$01
    ;STA $APUIO0
    ;REP #$20
    
    ;LDA #$FFFF
    ;STA $06FA
}


code_03E254 {
    RTL 
}

---------------------------------------------
;Hook for music asset loading (from scene_meta)

func_028B6D {
    JSR $&sub_028CE7
    PHA
    JSR $&sub_028CE7
    STA $06F4
    LDX #$003E
    JSR $&sub_028D8F
    LDA $06F6
    CMP $06F4
    BEQ code_028B88
    PLA
    RTS 
}

code_028B88 {
    PLA
    STA $06F2
    LDX #$0687
    JSR $&sub_028DC1
    BCS code_028B91
    RTS 
}

code_028B91 {
    ;BRK #$00

    LDA $06F2
    BEQ bgm_check               ;Always branch to halt when reset is loading

    LDA msu_flag
    BNE msu_begin_load

  bgm_check:
    LDA $06F2
    CMP $0D72
    BNE $01
    RTS                         ;Check against current track, if no change then quit

    STA $0D72

  bgm_halt:
    LDA #$01
    JSL $@func_0281C9
    LDA #$F0
    STA $APUIO0
    LDA #$03
    JSL $@func_0281C9           ;For some reason this is required
    LDA #$FF
    STA $APUIO0
    LDA #$02
    JSL $@func_0281C9
    
  bgm_init:
    LDA $0D72
    CMP #1B
    BEQ bgm_load_empty

    LDX $3E
    STX $46
    LDX $40
    STX $48
    ;LDA $06F2
    ;STA $0D72

  bgm_load:
    ;LDX $46
    ;STX $0687
    ;LDX $47
    ;STX $0688
    JSL $@func_02909B
    LDA #$03
    JSL $@func_0281C9
    LDA $06F2
    BEQ $02
    LDA #$01
    STA $APUIO0
    RTS

  bgm_load_empty:
    LDX #&bgm_no_music
    STX $46
    LDX #*bgm_no_music
    STX $48
    ;LDA #$1B
    ;STA $0D72
    BRA bgm_load

  msu_begin_load:
    LDX $06F2                   
    CPX #$001B
    BNE msu_begin_continue      ;If 1B is not requested, continue

    STZ $2007                   ;Stop playback when 1B is requested

    LDA $0D72
    CMP #$1B
    BNE $01
    RTS                         ;Exit if 1B is already playing

    LDA #$1B
    STA $0D72
    BRA bgm_halt                ;Set loading track to 1B and begin halt

  msu_begin_continue:
    LDA @bgm_track_map, X
    STZ $2006
    STA $2004
    STZ $2005

  msu_busy_wait:
    BIT $2000 
    BVS msu_busy_wait           ;Wait for data busy to clear

    LDA $2000
    AND #08
    BEQ msu_start_playback
    LDA $06F2
    STA $0D72
    JMP bgm_halt                ;Track not found, revert to normal process

  msu_start_playback:

    LDA @bgm_loop_map, X
    BEQ msu_start
    LDA #$03
    BRA msu_write

  msu_start:
    LDA #01

  msu_write:
    STA $2007                   ;Start playback
    LDA #FF                     ;Max volume
    STA $2006
    LDA $0D72
    CMP #$1B
    BEQ msu_return

    LDA #1B
    STA $0D72
    JMP bgm_halt

  msu_return:
    RTS

}

bgm_load_exit {
    REP #$20
    LDA #$FFFF
    STA $06FA
    SEP #$20
    RTL
}

----------------------------------------------

