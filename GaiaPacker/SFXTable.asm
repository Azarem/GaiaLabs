?BANK 02

!SAMPLE_NUM		$2A
!SAMPLE_SIZE	$2C
!SPC_CMD		$30
!SAMPLE_PTR		$4A

--------------------------------------

sfx_table:
	#@sfx00
	#@sfx01
	#@sfx02
	#@sfx03
	#@sfx04
	#@sfx05
	#@sfx06
	#@sfx07
	#@sfx08
	#@sfx09
	#@sfx0A
	#@sfx0B
	#@sfx0C
	#@sfx0D
	#@sfx0E
	#@sfx0F
	#@sfx10
	#@sfx11
	#@sfx12
	#@sfx13
	#@sfx14
	#@sfx15
	#@sfx16
	#@sfx17
	#@sfx18
	#@sfx19
	#@sfx1A
	#@sfx1B
	#@sfx1C
	#@sfx1D
	#@sfx1E
	#@sfx1F
	#@sfx20
	#@sfx21
	#@sfx22
	#@sfx23
	#@sfx24
	#@sfx25
	#@sfx26
	#@sfx27
	#@sfx28
	#@sfx29
	#@sfx2A
	#@sfx2B
	#@sfx2C
	#@sfx2D
	#@sfx2E
	#@sfx2F
	#@sfx30
	#@sfx31
	#@sfx32
	#@sfx33
	#@sfx34
	#@sfx35
	#@sfx36
	#@sfx37
	#@sfx38
	#@sfx39
	#@sfx3A
	#@sfx3B
	--#$C50000
	--#$C5065F
	--#$C506C4
	--#$C510A7
	--#$C510FA
	--#$C5144A
	--#$C5167A
	--#$C51B0E
	--#$C51B73
	--#$C520FA
	--#$C52681
	--#$C53454
	--#$C53471
	--#$C53578
	--#$C53AFF
	--#$C54DEB
	--#$C567A9
	--#$C56D54
	--#$C57014
	--#$C573F4
	--#$C607C1
	--#$C60FC7
	--#$C61B00
	--#$C622B5
	--#$C62A6A
	--#$C631B3
	--#$C638FC
	--#$C64D1A
	--#$C6518A
	--#$C6622D
	--#$C67828
	--#$C67FB0
	--#$C705C7
	--#$C7245F
	--#$C728AB
	--#$C72D90
	--#$C731C1
	--#$C73ED7
	--#$C742E4
	--#$C74DE7
	--#$C77CA2
	--#$C805FE
	--#$C80A65
	--#$C81838
	--#$C81CD5
	--#$C82AE7
	--#$C84BE3
	--#$C85C86
	--#$C87281
	--#$C9137D
	--#$C9175D
	--#$C91D74
	--#$C924FC
	--#$C9292D
	--#$C947C5
	--#$C94C35
	--#$C951E0
	--#$C9656E
	--#$C967DD
	--#$C96D64

0290CB:
	LDX  #&sfx_table

0290D2:
	LDA  #^sfx_table

0290F1:
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
	BRA  #$029160

02911D:
	LDA  [SAMPLE_PTR], Y
	INY
	XBA
	LDA  #$00
	BRA  #$02913E

029133:
	XBA
	LDA  [SAMPLE_PTR], Y
	INY

029146:
	BNE  #$029133
