?BANK 03

?INCLUDE 'chunk_028000'
?INCLUDE 'chunk_038000'
?INCLUDE 'chunk_03BAE1'
?INCLUDE 'system_strings'
?INCLUDE 'sE6_gaia'
?INCLUDE 'sFB_actor_0BC8BA'

!token                          00E6
!joypad_mask_std                065A
!camera_offset_x                06D6
!camera_offset_y                06D8
!camera_bounds_x                06DA
!camera_bounds_y                06DC
!player_flags                   09AE
!VMADDL                         2116
!A1T0L                          4302
!DAS0L                          4305

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
    JSL @ItemSwapping
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

pause_debug_print |[NHM:14][CUR:C0,6]S:[BCD:2,644] X:[BCD:3,9A2] Y:[BCD:3,9A4] C:[BCD:4,E6]|

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
;code_03DED5 {
;    PHP
;    REP #$20
;    LDA #$0001
;    TSB $09EC
;    PLP
;    RTL
;}


word_03DF0A [
  #$2CCE   ;06
  #$2CCF   ;07
  #$0000   ;08
  #$0008   ;09
  #$ECEF   ;0A
  #$2CDA   ;0B
  #$2CDB   ;0C
  #$2CDC   ;0D
  #$2CDD   ;0D
  #$ACE9   ;0D
  #$ECE9   ;0D
  #$6CDD   ;0D
  #$6CDC   ;10
  #$6CDB   ;11
  #$6CDA   ;12
  #$ACEF   ;13
  #$0000   ;14
  #$0008   ;15
  #$6CCF   ;16
  #$6CCE   ;17
  #$2CDE   ;1A
  #$0000   ;1B
  #$000A   ;1C
  #$2CEA   ;1D
  #$2CD9   ;1E
  #$0000   ;1F
  #$0001   ;20
  #$2CED   ;21
  #$0000   ;1F
  #$0002   ;20
  #$6CED   ;22
  #$0000   ;23
  #$0002   ;24
  #$6CEA   ;25
  #$0000   ;26
  #$000A   ;27
  #$6CDE   ;28
  #$2CEE   ;2B
  #$0000   ;2C
  #$000A   ;2D
  #$2CFA   ;2E
  #$2CFB   ;2F
  #$2CFC   ;30
  #$2CFD   ;31
  #$2CE9   ;33
  #$6CE9   ;33
  #$6CFD   ;32
  #$6CFC   ;32
  #$6CFB   ;34
  #$6CFA   ;35
  #$0000   ;36
  #$000A   ;37
  #$6CEE   ;38
  #$0000   ;39
  #$0000   ;3A
]


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

asciistring_01E7F6 |[CUR:42,0][NHM:8][HP][CUR:5A,0][NHM:14][BCD:1,AD8][CUR:64,0][NUM:AD6]|

asciistring_01E818 |[NHM:4][CUR:6A,0][HE]|

-------------------------------------------------
;Print debug string on radar screen

code_03808B {
    LDA $camera_offset_x+1
    AND #$0F
    STA $token
    LDA $camera_offset_y+1
    ASL 
    ASL 
    ASL 
    ASL 
    ORA $token
    STA $token
    LDA $camera_bounds_x+1
    AND #$0F
    STA $token+1
    LDA $camera_bounds_y+1
    ASL 
    ASL 
    ASL 
    ASL 
    ORA $token+1
    STA $token+1

    COP [BD] ( @pause_debug_print )
    LDX #$0000
    PHX 
}

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

-----------------------------------------------
?INCLUDE 'sF7_credits'

credits_09F2FA `[PAL:0][DLG:44,1]     Built With GaiaLabs[N][PAL:4]         By Kassiven[N][PAL:0]            Ǫįņţ[N]            ęťĔŇ[END]`

-----------------------------------------------
;Auto-size and center scene titles

code_02A12C {
    PHP 
    PHB 
    
    PEA #$CA01
    INC
    LSR
    PHA
    PEA #$C707

    CLC
    SBC #$10
    EOR #$FF
    PHA

    LDA #$C1
    PHA

    TSC
    LDY $3E
    PHY 
    TAY
    INY

    LDA $40
    PHA
    
    LDA #$E0
    STA $00B4
    STZ $00B5
    
    REP #$20
    JSL $@sub_03E255
    PLB
    PLY

    PLA
    PLA
    PLA
    JSL $@sub_03E255

    PLB
    PLB
    PLP
    RTL
}

--------------------------------------------
?INCLUDE 'inventory_menu'
--------------------------------------------
;Force some palette colors for cleaner font
h_inventory_menu [
  h_actor < #00, #00, #28 >   ;00
    LDA #$4063
    STA $7F0A06
    LDA #$2180
    STA $7F0A2E
]

;Make flashing cursor show blank when hidden
sub_02EC46 {
    LDA #$0001
    TSB $09EC
    LDA #$2060
    STA $7F0896
    STA $7F0916
    RTS 
}

;Make flashing cursor show blank when hidden
sub_02ECE8 {
    LDA #$0001
    TSB $09EC
    LDA #$2060
    STA $7F0784
    STA $7F0804
    STA $7F0884
    STA $7F0904
    RTS 
}
