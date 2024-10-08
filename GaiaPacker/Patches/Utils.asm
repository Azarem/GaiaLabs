﻿?BANK 03

?INCLUDE 'chunk_028000'
?INCLUDE 'chunk_038000'
?INCLUDE 'chunk_03BAE1'
?INCLUDE 'system_strings'

!player_flags                   09AE
!VMADDL                         2116
!A1T0L                          4302
!DAS0L                          4305
!convert1                       09FA
!convert2                       09FD
!dest_offset1                   0002
!dest_offset2                   0004

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

convert_color {
    CMP #80
    BEQ $0A
    BPL $05
    CLC
    ADC $01
    BRA $03
    SEC
    SBC $01
    LSR
    LSR
    LSR
    RTS
}

convert_pixel {
    SEP #$20
    LDA #$00
    XBA
    LDA $0000, Y
    JSR convert_color
    PHA
    LDA $0001, Y
    JSR convert_color
    PHA
    LDA $0002, Y
    JSR convert_color
    REP #$20
    STA $7F0A00, X
    PLA
    TAY
    AND #$1F00
    ASL
    ASL
    ORA $7F0A00, X
    STA $7F0A00, X
    TYA
    AND #$001F
    ASL
    ASL
    ASL
    ASL
    ASL
    ORA $7F0A00, X
    STA $7F0A00, X
    RTS
}

---------------------------------------------------------
;Global scripts
global_scripts {
    JSL @RunButton



    RTS
}

-----------------------------------------------------------

global_thinkers {
    ;PHX
    ;PHY

    ;LDY #$convert1
    ;LDX #$dest_offset1
    ;JSR convert_pixel

    ;LDY #$convert2
    ;LDX #$dest_offset2
    ;JSR convert_pixel

    ;PLY
    ;PLX
    RTS
}

------------------------------------------------------------
;Hook for RGB convert

;func_028043 {

;    PHP 
;    PHX
;    PHY

;    LDY #$convert1
;    LDX #$dest_offset1
;    JSL convert_pixel

;    LDY #$convert2
;    LDX #$dest_offset2
;    JSL convert_pixel

;    PLY
;    PLX

;    REP #$20
;    PHA 
;    SEP #$20
;    LDA $804210
;}

func_03D1C2 {
    PHP 
    PHD 
    JSR global_thinkers
    REP #$20
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

