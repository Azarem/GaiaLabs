
#BANK		$02
#SPTR		$3E
#DCMP_SIZE	$78

-----------------------------------------------------

main:
  LDA  [SPTR]
  INC  SPTR
  INC  SPTR
  CMP  #$0000
  BEQ  jmpno
  BMI  jmpno
  STA  DCMP_SIZE
  JMP  $8777

jmpno:
  JMP  $878A

02876D:
  JMP  main
