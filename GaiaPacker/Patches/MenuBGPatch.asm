?BANK 02

!SPTR		$3E

-----------------------------------------------------

main:
  LDA  [SPTR]
  INC  SPTR
  INC  SPTR
  CMP  #$0000
  BEQ  jmpno
  BMI  jmpno
  JMP  $8C69

jmpno:
  JMP  $8C85

028C61:
  JMP  main
