?BANK 00

?INCLUDE 'chunk_008000'

!SPTR		$3E
!DCMP_SIZE	$78

--------------------------------------------

cop_handler_51_00997B:
  PHY 
  PHD 
  LDA [$0A]
  INC $0A
  INC $0A
  STA $003E
  LDA [$0A]
  INC $0A
  AND #$00FF
  SEP #$20
  STA $0040
  REP #$20
  LDA [$0A]
  INC $0A
  INC $0A
  STA $007A
  LDA [$0A]
  INC $0A
  AND #$00FF
  LDA #$0000
  TCD

cop51_main:
  LDA [SPTR]
  BMI cop51_minus
  BEQ cop51_zero
  STA DCMP_SIZE
  INC SPTR
  INC SPTR
  JSL $@func_028270
  JSL $@zero_bytes_03D86A
  BRA cop51_end

cop51_zero:
  LDA #$2000
  BRA cop51_domvn

cop51_minus:
  LDA #$0000
  SEC
  SBC [SPTR]

cop51_domvn:
  INC SPTR
  INC SPTR

  SEC
  SBC #$0001
  PHA

  SEP #$20
  LDA #$7E
  STA $0404
  LDA SPTR+2
  STA $0405
  REP #$20

  LDX SPTR
  LDY $7A

  PLA
  JSR $0402

cop51_end:
  PLD 
  PLY 
  TYX
  LDA $0A
  STA $02, S
  RTI 

