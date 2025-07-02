
-------------------------------------------
?INCLUDE 'chunk_03BAE1'
-------------------------------------------

;Characters >= 0xE0 will use the accent table

sub_03E255 {
    PHP 
    PHD 
    PHX 
    LDA #$0000
    TCD 
    LDX $0998

  code_03E25F:
    SEP #$20
    LDA $0000, Y
    CMP #$E0
    BCS do_accent_table
    CMP #$C0
    BCC code_03E27C
    REP #$20
    INY 
    PEA $&code_03E25F-1
    AND #$001F
    ASL 
    PHX 
    TAX 
    LDA $@wide_cmd_table_03E2C3, X
    PLX 
    DEC 
    PHA 
    RTS 
}

code_03E27C {
    REP #$20
    AND #$00FF
    INY 
    STA $00
    ORA $0986
    ORA #$2100
    LDX $0998
    STA $7F0200, X
    CLC 
    ADC #$0010
    STA $7F0240, X
  character_print_stub:
    INX 
    INX 
    STX $0998
    LDA $0654
    BEQ code_03E25F
    LDA $00
    PHA 
    LDA #$0001
    TSB $09EC
    JSR $&code_03E7BA
    PLA 
    CMP #$00AC
    BEQ code_03E25F
    LDA $06F8
    AND #$FF00
    ORA $0996
    STA $06F8
    BRA code_03E25F
}

do_accent_table {
    INY
    REP #$20
    AND #$001F
    ASL
    TAX
    LDA $@accent_table, X
    SEP #$20
    LDX $0998
    STA $7F0200, X
    LDA #$21
    ORA $0987
    STA $7F0201, X
    STA $7F0241, X
    XBA
    STA $7F0240, X
    REP #$20
    BRA character_print_stub
}

accent_table [
    #E090
    #E190
    #E290
    #E390
    #E450
    #E550

    #E694
    #E794
    #E894
    #E950

    #82EA
    
    #EB5E
    #EC54
    #ED54
    #EE58
    #EF75
    
    #F098
    #F198
    #F298
    #F398
    
    #F49E
    #F59E
    #F69E
    #F79E
    #F89E
    
    #F9B5
    #FAB5
    #FBB5
    
    #FC5E
    #FD5E
    #42FE
    #FF50
]
