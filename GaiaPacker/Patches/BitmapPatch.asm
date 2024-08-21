?BANK 02

?INCLUDE 'chunk_028000'

-- Patch for bitmap loading which adds support for no compression

!LSAMPLE	$10
!LOOPNUM	$12
!SPTR		$3E
!DPTR		$5E
!TPTR1		$72
!TPTR2		$75
!DCMP_SIZE	$78


--------------------------------------------------

code_028503 {
    LDA [$3E]
    INC $3E
    INC $3E
    STA $78
    CMP #$0000
    BEQ mode7check
    BMI mode7check
  
    CPX #$066C
    BNE jmp_8520
    LDA $06EE
    BIT #$0800
    BEQ jmp_8520
    JMP func_0285DB

jmp_8520:
    JMP code_028520

jmp_8560:
    JMP func_028560

  mode7check:
    CPX #$066C
    BNE jmp_8560
    LDA $06EE
    BIT #$0800
    BEQ jmp_8560

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

  JSR  sub_02868C	-- this call changes SPTR to reference WRAM

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

  JMP  code_028645+18  -- finish remaining process
  
}

--------------------------------------------------

func_028592 {
    LDA [$3E]
    STA $78
    INC $3E
    INC $3E
    CMP #$0000
    BEQ code_0285B2
    BMI code_0285B2
    LDX #$7000
    STX $7A
    JSL $%func_028270
    LDX #$7000
    STX $3E
    LDA #$007E
    STA $40
}
