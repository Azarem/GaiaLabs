?BANK 02

?INCLUDE 'chunk_028000'

!SPTR		$3E

-----------------------------------------------------

func_028BE4 {
    PHP 
    REP #$20
    LDA [$3A], Y
    STA $0666
    SEP #$20
    INY 
    INY 
    INY 
    LDX #$003E
    JSR $&sub_028D8F
    LDX #$0684
    JSR $&sub_028DC1
    BCC code_028C19
    REP #$20
    LDA [$3E]
    INC $3E
    INC $3E
    CMP #$0000
    BEQ code_028C1B
    BMI code_028C1B
    STA $78
    SEP #$20
    LDX #$4000
    STX $7A
    JSL $%func_028270
}
