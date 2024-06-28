?BANK $02

!SPTR		$3E

-----------------------------------------------------

main:
  LDA  [SPTR]
  INC  SPTR
  INC  SPTR
  CMP  #$0000
  BEQ  jmpno
  BMI  jmpno
  JMP  $8C0C

jmpno:
  JMP  $8C1B

028C01:
  JMP  main
