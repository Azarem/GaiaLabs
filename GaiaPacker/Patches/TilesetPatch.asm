?BANK 02

?INCLUDE 'chunk_028000'

!SPTR		$3E
!DCMP_SIZE	$78

-----------------------------------------------------

code_028768 {
    LDA $066A
    BEQ code_0287BA
    LDA [$3E]
    STA $78
    INC $3E
    INC $3E
    CMP #$0000
    BEQ code_02878A
    BMI code_02878A
    LDX #$7000
    STX $7A
    JSL $%func_028270
    LDX #$7000
    STX $3E
    LDA #$007E
    STA $40
}
