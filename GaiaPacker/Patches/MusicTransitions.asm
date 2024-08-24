?BANK 03

?INCLUDE 'chunk_03BAE1'
?INCLUDE 'chunk_028000'

!meta_next_id       $0642
!meta_current_id    $0644
!token              $00E6
!INIDISP                        2100
!MOSAIC                         2106
!APUIO0                         2140

---------------------------------------

bgm_table [
  #FF #1D #1D #1D #1D #1D #1D #1D #1D #FF #05 #1B #06 #06 #06 #06  ;0F
  #06 #06 #06 #06 #FF #03 #03 #03 #03 #04 #04 #04 #04 #07 #07 #07  ;1F
  #07 #07 #07 #07 #07 #07 #07 #07 #07 #07 #11 #04 #04 #04 #04 #15  ;2F
  #1B #1B #02 #02 #02 #02 #02 #02 #02 #02 #02 #02 #06 #06 #06 #06  ;3F
  #06 #06 #06 #06 #06 #06 #06 #04 #FF #03 #FF #04 #08 #08 #08 #08  ;4F
  #08 #08 #08 #08 #08 #08 #08 #FF #FF #FF #04 #04 #04 #04 #04 #09  ;5F
  #09 #09 #09 #04 #09 #09 #04 #04 #04 #03 #03 #03 #03 #06 #06 #06  ;6F
  #06 #06 #06 #1D #04 #04 #FF #FF #02 #02 #02 #06 #02 #02 #02 #15  ;7F
  #FF #FF #0A #0A #FF #0A #0A #0A #0A #FF #0A #0A #FF #FF #FF #FF  ;8F
  #FF #02 #02 #02 #02 #05 #05 #02 #02 #02 #02 #02 #02 #04 #FF #FF  ;9F
  #06 #06 #06 #06 #06 #06 #06 #06 #06 #06 $FF #FF #04 #FF #FF #FF  ;AF
  #04 #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #04  ;BF
  #11 #FF #FF #02 #02 #04 #02 #02 #02 #02 #FF #FF #04 #04 #0C #0C  ;CF
  #0C #0C #0C #0C #0C #0C #0C #0C #0C #0C #0C #0C #02 #0F #04 #04  ;DF
  #04 #04 #04 #04 #0E #0E #16 #FF #FF #06 #FF #FF #FF #FF #FF #FF  ;EF
  #00 #FF #0F #0F #0F #0F #0F #1B #FF #13 #00 #1B #14 #1B #12 #FF  ;FF
]

count_check:
  CMP #06
  BNE count_ret
  PHA
  LDA #F2
  CMP token
  BNE count_fix
  STA $APUIO0
  STZ token

count_fix:
  PLA

count_ret:
  RTS

--------------------------------------

func_03D9F6 {
    LDA $0654
    BMI code_03DA03
    BEQ code_03DA00
    
  do_check:
    PHX
    PHY
    LDX meta_next_id
    LDA @bgm_table, X
    BMI return
    TAY
    LDX meta_current_id
    LDA @bgm_table, X
    BMI return
    TYA
    CMP @bgm_table, X
    BEQ return
    LDA $0D72
    BEQ return
    LDA #F2
    STA token
  
  return:
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
    JSL $@func_00811E
    JSR count_check+4
    STZ $INIDISP
    RTS 
}

-------------------------------------------

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
    BPL code_03DAFC
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

code_028B91 {
    LDA #$01
    JSL $@func_0281C9
    LDA #$F0
    STA $APUIO0
    LDA #$01
    JSL $@func_0281C9         ;For some reason this is required
    BRA code_028BAC+5
}

----------------------------------------------





