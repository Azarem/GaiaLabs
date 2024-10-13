?BANK 03

?INCLUDE 'chunk_028000'
?INCLUDE 'chunk_038000'
?INCLUDE 'chunk_03BAE1'
?INCLUDE 'system_strings'
?INCLUDE 'sE6_gaia'
?INCLUDE 'sFB_actor_0BC8BA'
?INCLUDE 'PixelConverter'

!player_flags                   09AE
!VMADDL                         2116
!A1T0L                          4302
!DAS0L                          4305
!joypad_mask_std                065A

------------------------------------------------

;Button flags
;8000 - (B) Attack/Talk
;4000 - (Y) Item/Cancel
;2000 - (SEL) Menu
;1000 - (STA) Pause
;0800 - UP
;0400 - DOWN
;0200 - LEFT
;0100 - RIGHT
;0080 - (A) Nothing
;0040 - (X) Nothing
;0020 - (L) Spin
;0010 - (R) Spin


---------------------------------------------------------
;Global scripts
global_scripts {
    JSL @RunButton

    RTS
}

-----------------------------------------------------------

global_thinkers {
    PHX
    PHY
    ;JSL @PixelConverter

    PLY
    PLX
    RTS
}

------------------------------------------------------------
;Hook for global thinkers

func_03D1C2 {
    PHP 
    PHD 
    REP #$20
    JSR global_thinkers
    LDA $5A
    BEQ code_03D1F2
}

----------------------------------------------------------

;Prevent drawing of HUD BG layers
code_03DED5 {
    PHP
    REP #$20
    LDA #$0001
    TSB $09EC
    PLP
    RTL
}

----------------------------------------------------------

;Fix for string code 05 BCD processing to include hex chars
code_03ED90 {
    LDA $0000, Y
    AND #$0F
    CMP #$0A
    BMI bcd_numeric1

    CLC
    ADC #$37
    BRA bcd_store1

  bcd_numeric1:
    ORA #$30

  bcd_store1:
    REP #$20
    DEX 
    DEX 
    STA $7F0200, X
    SEP #$20
    DEC $000E
    BEQ code_03EDC1
    LDA $0000, Y
    INY 
    ;AND #$F0
    LSR 
    LSR 
    LSR 
    LSR
    CMP #$0A
    BMI bcd_numeric2

    CLC
    ADC #$37
    BRA bcd_store2

  bcd_numeric2:
    ORA #$30

  bcd_store2:
    REP #$20
    DEX 
    DEX 
    STA $7F0200, X
    SEP #$20
    DEC $000E
    BNE code_03ED90
}

------------------------------------------------

;DMA size
code_02B06D {
    AND #$DF
    STA $09EC
    LDX #$0080  ;Two lines of tiles
    STX $DAS0L
}

----------------------------------------------

;VRAM DMA arguments (when copying BG3 layer)
code_02B078 {
    LDX #$7820
    STX $VMADDL
    LDX #$0240
    STX $A1T0L
}

string_01E7F6 |[CUR:46,0][NHM:8][HP][CUR:9C,0][NHM:14][BCD:1,AD8][CUR:A0,0][NUM:AD6][CUR:5E,0][BCD:2,644][CUR:7A,0][BCD:3,9A2][CUR:BA,0][BCD:3,9A4]|

string_01E818 |[NHM:4][CUR:66,0][HE]|

-------------------------------------------------
;Prevent unequipped message
func_0384BF {
    RTS 
}

------------------------------------------------
;Hook into global actor code
run_actors_03CAF5 {
    PHP 
    PHD 
    REP #$20
    JSR global_scripts
    LDA $player_flags
    BIT #$0008
    BEQ code_03CB07
    LDA #$8000
    TRB $0656
}

------------------------------------------------
;Disable continue option when saving

code_08DBB1 {
    LDA $0D8C
    JSL $@func_03D916
    COP [07] ( #29 )
    LDA #$FFF0
    TSB $joypad_mask_std
    COP [DA] ( #3B )
    LDA #$FFF0
    TRB $joypad_mask_std
    ;COP [BF] ( &widestring_08DDFE )
    ;COP [BE] ( #02, #01, &code_list_08DBD4 )
    BRA code_08DBDA
}

-----------------------------------------------
;Disable region protection

func_0BC896 {
    BRA code_0BC8EA
}
