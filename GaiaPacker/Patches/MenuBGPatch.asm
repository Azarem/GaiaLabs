?BANK 02

?INCLUDE 'chunk_028000'

!SPTR		$3E

-----------------------------------------------------

func_028C30 {
    PHP 
    JSR $&sub_028CE7
    STA $066A
    LDX #$003E
    JSR $&sub_028D8F
    REP #$20
    LDA [$3E]
    STA $00
    INC $3E
    INC $3E
    LDA [$3E]
    XBA 
    ORA $00
    INC $3E
    INC $3E
    SEP #$20
    JSL $%func_0281D1
    REP #$20
    STA $00
    XBA 
    ASL 
    ASL 
    ASL 
    STA $0666
    LDA [$3E]
    INC $3E
    INC $3E
    CMP #$0000
    BEQ code_028C81
    BMI code_028C81
    STA $78
    SEP #$20
    LDX #$7000
    STX $7A
    JSL $%func_028270
    LDX #$7000
    STX $3E
    LDA #$7E
    STA $40
    BRA code_028C87
}

code_028C81 {
    SEP #$20
}
