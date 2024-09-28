?BANK 03

?INCLUDE 'chunk_03BAE1'

!scene_current                  0644
!advance_button_mask            C080

--------------------------------------------------------

border_lookup [
    &border_index_3
    &border_index_1
    &border_index_2
    &dlg_borders_03E4CE
]

border_index_1 #1320142013601520156013A014A013E0
border_index_2 #1620172016601820186016A017A016E0
border_index_3 #19201A2019601B201B6019A01AA019E0

cmd_d9 {
    LDA $0000, Y
    AND #$0003
    STA $09F8
    INY
    RTS
}


----------------------------------------------------

wide_cmd_table_03E2C3 [
  &cmd_c0_03E2F5   ;00
  &cmd_c1_03E30F   ;01
  &cmd_c2_03E335   ;02
  &cmd_c3_03E35B   ;03
  &cmd_c4_03E36B   ;04
  &cmd_c5_03E36F   ;05
  &cmd_c6_03E393   ;06
  &cmd_c7_03E43F   ;07
  &cmd_c8_03E579   ;08
  &cmd_c9_03E5EB   ;09
  &code_03E307   ;0A
  &cmd_cb_03E5F8   ;0B
  &cmd_cc_03E61E   ;0C
  &cmd_cd_03E636   ;0D
  &cmd_ce_03E656   ;0E
  &cmd_cf_03E6A4   ;0F
  &cmd_d0_03E6D2   ;10
  &cmd_d1_03E6E7   ;11
  &cmd_d2_03E6EC   ;12
  &cmd_d3_03E6F7   ;13
  &cmd_d4_03E721   ;14
  &cmd_d5_03E736   ;15
  &cmd_d6_03E743   ;16
  &cmd_d7_03E769   ;17
  &cmd_d8_03E78F   ;18
  &cmd_d9
]



----------------------------------------------------
;Entry point for command 7 (setting up dialog borders)

code_03E453 {
    PHY 
    PHX 
    LDA $0B04
    STA $007E
    LDA #$0010
    STA $0996
    STZ $00DC
    STZ $099C
    LDA $097A
    STA $097E
    LDA $097C
    STA $0980
    XBA 
    LSR 
    LSR 
    CLC 
    ADC $097A
    CLC 
    ADC $097A
    STA $099A
    STZ $0986

    LDA $09F8
    AND #$0003
    ASL
    TAX
    LDA @border_lookup, X
    STA $3E
    LDA #*border_lookup
    STA $40

    ;LDA #$*dlg_borders_03E4CE
    ;STA $40
    ;LDA #$&dlg_borders_03E4CE
    ;STA $3E

    LDA $0982
    ASL 
    STA $18
    PHA 
    LDA $0984
    ASL 
    STA $1C
    LDA $0998
    DEC 
    DEC 
    SEC 
    SBC #$0040
    STA $00
    TAX 
    JSR $&sub_03E4DE
    PLX 
    PHX 
    STX $18
    JSR $&sub_03E505
    PLY 
    STY $18
    JSR $&sub_03E4DE
    LDA #$0001
    TSB $09EC
    LDA $scene_current
    AND #$00FF
    CMP #$00FA
    BEQ code_03E4CB
    JSR $&sub_03E7B2
}

----------------------------------------------------
;Entry point for command 8 (clear dialog) to support border styles

cmd_c8_03E579 {
    PHY 
    PHB 
    STZ $09F8
    LDA $099A
    STA $0998
    LDA $0982
    ASL 
    INC 
    STA $00
    STA $18
    LDA $0984
    INC 
    ASL 
    STA $1C
    LDA $099A
    SEC 
    SBC #$0042
    STA $099A
    TAX 

}

------------------------------------

cmd_cf_03E6A4 {
    LDA #$advance_button_mask
    TSB $0658

  code_03E6AA:
    JSR $&sub_03E7B2
    LDA $0656
    AND #$advance_button_mask
    BNE code_03E6C1
    SEC 
    JSR $&sub_03E80C
    LDA #$0001
    TSB $09EC
    BRA code_03E6AA
}

---------------------------------------------

cmd_d0_03E6D2 {
    LDA #$advance_button_mask
    TSB $0658

  code_03E6D8:
    JSR $&sub_03E7B2
    LDA $0656
    AND #$advance_button_mask
    BEQ code_03E6D8
    STA $0658
    RTS 
}