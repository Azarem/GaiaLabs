?BANK 02

-- Patch for bitmap loading which adds support for no compression

!LSAMPLE	$10
!LOOPNUM	$12
!SPTR		$3E
!DPTR		$5E
!TPTR1		$72
!TPTR2		$75
!DCMP_SIZE	$78

--------------------------------------------------

main1:
  LDA  [SPTR]
  INC  SPTR
  INC  SPTR
  CMP  #$0000
  BEQ  mode7check
  BMI  mode7check
  STA  DCMP_SIZE
  JMP  $8510

mode7check:
  CPX  #$066C
  BNE  jmpdma
  LDA  $06EE
  BIT  #$0800
  BEQ  jmpdma
  BRA  mode7

jmpdma:
  JMP  $8560

mode7:
  PHY				-- Y will be our read offset
  PHB				-- data bank will be changed

  LDA  SPTR			-- init offset for temp pointers
  STA  TPTR1
  CLC
  ADC  #$0010
  STA  TPTR2

  SEP  #$20	

  LDA  SPTR+2		-- init bank for temp pointers
  STA  TPTR1+2
  STA  TPTR2+2

  LDA  #$7E			-- set data bank to $7E
  PHA
  PLB

  STZ  $0E			-- ??

  LDX  #$A000		-- init DPTR with WRAM offset
  STX  DPTR

  JSR  $868C		-- this call changes SPTR to reference WRAM

  LDY  #$0000		-- init read offset to 0

readlookup:
  LDA  #$07			-- init sample counter
  STA  LOOPNUM

  LDA  (SPTR)		-- read lookup sample and increment pointer
  STA  LSAMPLE
  INC  SPTR
  BNE  readsample
  INC  SPTR+1
  
readsample:
  LDA  [TPTR1], Y
  STA  $00
  LDA  [TPTR2], Y
  STA  $04
  INY
  LDA  [TPTR1], Y
  STA  $02
  LDA  [TPTR2], Y
  STA  $06
  INY

  LDX  #$0007		-- init rotate counter

rotatebits:
  LDA  #$00
  ROL  $06
  ROL
  ROL  $04
  ROL
  ROL  $02
  ROL
  ROL  $00
  ROL
  ORA  LSAMPLE

  STA  (DPTR)		-- store result and increment pointer
  INC  DPTR
  BNE  donext
  INC  DPTR+1

donext:
  DEX
  BPL  rotatebits	-- continue rotate (8 times)

  DEC  LOOPNUM
  BPL  readsample	-- continue sample (8 times)

  REP  #$20			-- increment read counter by $10
  TYA
  CLC
  ADC  #$0010
  TAY
  SEP  #$20

  CPY  #$2000
  BCC  readlookup	-- continue read ($2000 bytes)

  STZ  $0671		-- clear 'last loaded' banks so next resource loads
  STZ  $067D
  STZ  $0680
  STZ  $0683

  JMP  $865D		-- finish remaining process
  
028503:
  JMP  main1
  
--------------------------------------------------

main2:
  LDA  [SPTR]
  INC  SPTR
  INC  SPTR
  CMP  #$0000		-- what to use for comments
  BEQ  return2		-- this kinda works
  BMI  return2		-- this also works
  STA  DCMP_SIZE
  JMP  $859F

return2: 
  JMP  $85B2

028592:
  JMP  main2
