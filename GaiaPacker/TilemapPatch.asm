-- Patch for tilemap loading which adds support for no compression

#BANK		$02
#SPTR		$3E
#DCMP_SIZE	$78

----------------------------------------

main1:
  LDA  [SPTR]
  INC  SPTR
  INC  SPTR
  CMP  #$0000
  BEQ  jmpno1
  BMI  jmpno1
  STA  DCMP_SIZE
  STA  $0666
  JMP  $884F

jmpno1:
  JMP  $88B5

02883F:
  JMP  main1

---------------------------------------------

main2:
  LDA  [SPTR]
  INC  SPTR
  INC  SPTR
  CMP  #$0000
  BEQ  jmpno2
  BMI  jmpno2
  STA  DCMP_SIZE
  JMP  $8943

jmpno2:
  JMP  $8935

028928:
  JMP  main2

-------------------------------------------------

main3:
  LDA  $01
  STA  $0693, X		-- copy stored width
  LDA  $03
  STA  $0697, X		-- copy stored height
  LDA  $0667
  STA  $069B, X		-- copy stored multiply result (used by 0 index)
  REP  #$20
  STZ  $0668		-- zero dest offset
  JMP  $8920

028914:
  JMP main3
