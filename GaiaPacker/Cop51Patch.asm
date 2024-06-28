?BANK 00

!SPTR		$3E
!DCMP_SIZE	$78

--------------------------------------------

main:
  LDA  [SPTR]
  CMP  #$0000
  BMI  minus
  BEQ  zero
  STA  DCMP_SIZE
  INC  SPTR
  INC  SPTR
  JMP  $99B0

zero:
  LDA  #$2000
  BRA  domvn

minus:
  LDA  #$0000
  SEC
  SBC  [SPTR]

domvn:
  INC  SPTR
  INC  SPTR
  PHX
  PHY

  SEC
  SBC  #$0001
  PHA

  SEP  #$20
  LDA  #$7E
  STA  $0404
  LDA  SPTR+2
  STA  $0405
  REP  #$20

  LDX  SPTR
  LDY  $7A

  PLA
  JSR  $0402
  PLY
  PLX

  JMP  $99B8

0099A8:
  JMP  main
