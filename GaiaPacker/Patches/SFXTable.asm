?BANK 02

?INCLUDE 'chunk_028000'

!SAMPLE_NUM		$2A
!SAMPLE_SIZE	$2C
!SPC_CMD		$30
!SAMPLE_PTR		$4A

--------------------------------------

sfx_table:
	#@sfx00  #@sfx01  #@sfx02  #@sfx03  #@sfx04  #@sfx05  #@sfx06  #@sfx07  
	#@sfx08  #@sfx09  #@sfx0A  #@sfx0B  #@sfx0C  #@sfx0D  #@sfx0E  #@sfx0F
	#@sfx10  #@sfx11  #@sfx12  #@sfx13  #@sfx14  #@sfx15  #@sfx16  #@sfx17
	#@sfx18  #@sfx19  #@sfx1A  #@sfx1B  #@sfx1C  #@sfx1D  #@sfx1E  #@sfx1F
	#@sfx20  #@sfx21  #@sfx22  #@sfx23  #@sfx24  #@sfx25  #@sfx26  #@sfx27
	#@sfx28  #@sfx29  #@sfx2A  #@sfx2B  #@sfx2C  #@sfx2D  #@sfx2E  #@sfx2F
	#@sfx30  #@sfx31  #@sfx32  #@sfx33  #@sfx34  #@sfx35  #@sfx36  #@sfx37
	#@sfx38  #@sfx39  #@sfx3A  #@sfx3B

	
  code_0290C9:
    ;REP #$20
    LDX #&sfx_table
    STX $4A
    SEP #$20
    LDA #^sfx_table
    STA $4C
    LDY $32
    LDA [$46], Y
    INY 
    STY $32
    STA $2A
    STZ $2B
    BIT #$80
    BEQ code_0290E8
    JMP $&code_029153
	
  code_0290F1:
	LDA  SAMPLE_NUM
	ASL
	CLC
	ADC  SAMPLE_NUM
	TAY
	LDA  [SAMPLE_PTR], Y
	TAX
	INY
	INY
	SEP  #$20
	LDA  [SAMPLE_PTR], Y
	STA  SAMPLE_PTR+2
	REP  #$20
	STZ  SAMPLE_PTR
	TXY
	LDA  [SAMPLE_PTR], Y
	STA  SAMPLE_SIZE
	STA  $34				-- ??
	INY
	INY
	SEP  #$20
	LDA  SPC_CMD
	BRA  code_029160

code_02911D:
	LDA  [SAMPLE_PTR], Y
	INY

code_02912C:
	XBA
	LDA  [SAMPLE_PTR], Y
	INY

