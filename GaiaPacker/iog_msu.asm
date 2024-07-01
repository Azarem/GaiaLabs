?BANK 00
; Patch by Conn
; apply on Illusion of Gaia (US) without header

; header ; remove semicolon before header if you apply on headered rom

; $0100 track number

; hirom

; --------------------free space code
; org $c7e930 ;free rom space

track_table: ; ORG $c7fe00 ; track table for each room 
;   0   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
db $40,$03,$03,$03,$03,$03,$03,$03,$03,$03,$09,$00,$0A,$0A,$0A,$0A ; rooms 00-0f 
db $0A,$0A,$0A,$0A,$03,$07,$07,$07,$07,$05,$05,$0A,$05,$0C,$0C,$0C ; rooms 10-1f
db $0C,$0C,$0C,$0C,$0C,$0C,$0C,$0C,$0C,$0C,$0D,$40,$05,$05,$05,$0E ; rooms 20-2f
db $00,$00,$06,$06,$06,$06,$06,$06,$06,$06,$06,$06,$05,$0A,$0A,$0A ; rooms 30-3f 
db $0A,$0A,$0A,$0A,$0A,$0A,$0A,$05,$40,$07,$40,$05,$0F,$0F,$0F,$0F ; rooms 40-4f 
db $0F,$0F,$0F,$0F,$0F,$0F,$0F,$40,$40,$06,$05,$05,$05,$05,$05,$10 ; rooms 50-5f
db $10,$10,$10,$05,$10,$10,$05,$05,$05,$07,$07,$07,$07,$0A,$0A,$0A ; rooms 60-6f
db $0A,$0A,$0A,$1b,$05,$05,$40,$40,$06,$06,$06,$0A,$06,$06,$06,$0E ; rooms 70-7f 
db $40,$40,$0B,$0B,$40,$0B,$0B,$0B,$0B,$40,$0B,$0B,$40,$40,$40,$40 ; rooms 80-8f
db $40,$06,$06,$06,$06,$09,$09,$06,$06,$06,$06,$06,$06,$05,$40,$40 ; rooms 90-9f
db $0A,$0A,$0A,$0A,$0A,$0A,$0A,$0A,$0A,$0A,$40,$40,$05,$05,$05,$40 ; rooms A0-Af
db $05,$08,$08,$08,$08,$08,$08,$08,$08,$08,$08,$08,$08,$08,$08,$05 ; rooms B0-Bf 
db $0D,$40,$40,$06,$06,$05,$06,$06,$06,$06,$40,$40,$05,$05,$11,$11 ; rooms C0-Cf
db $11,$11,$11,$11,$11,$11,$11,$11,$11,$11,$11,$11,$06,$12,$05,$05 ; rooms D0-Df
db $05,$05,$05,$05,$13,$13,$04,$40,$40,$0A,$40,$40,$40,$40,$40,$40 ; rooms E0-Ef
db $00,$40,$12,$12,$12,$12,$12,$40,$40,$40,$02,$00,$01,$00,$02,$40 ; rooms F0-Ff


loop_table: ; ORG $c7ff00 ; loop table for each room 00: mute, 03:loop, 01 non-loop
db $00,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03 ; tracks 00-0f 
db $03,$03,$03,$03,$03,$03,$03,$01,$01,$01,$01,$03,$01,$01,$01,$01 ; tracks 10-1f
db $03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03 ; tracks 20-2f
db $03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03 ; tracks 30-3f 
db $03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03,$03 ; tracks 40-4f 


loadTrack:
	STZ $0642
	STA $0644
	PHA
	LDA $2002
	CMP #$53
	BEQ $02
	PLA
	RTL
	PLA
	CMP #$2C ;ghostship check 1
	BNE $07
	LDA $0100
	CMP #$07
	BEQ noload
	CMP #$2E  ; ghostship check 2
	BNE $07
	LDA $0100
	CMP #$07
	BEQ noload
	LDA $0644
	PHX
	LDX #$0000
	TAX
	LDA @track_table,x
	PLX
	CMP #$40   ; music does not change
	BEQ noload
	STA $0100
	CMP $0101
	BEQ noload
	CMP #$00
	BNE $05
	LDA #$41
	STA $0101
	STZ $0102
	STZ $2006
	STA $2004
	STZ $2005

noload:
	LDA $0644
	RTL	

muteSPC:
	LDA #$0000
	PHP
	SEP #$20
	LDA $2002
	CMP #$53
	BNE $03
	JSR playTrack	
	PLP
	LDA #$0002
	AND $066A
	RTL

playTrack:
	LDA $0101
	CMP $0100
	BEQ trackRunsAlready

msuready:
	Bit $2000
	bvs msuready ; check if msu is ready
	PHX
	LDX #$0000
	LDA $0100
	STA $0101
	TAX
	LDA @loop_table,x
	STA $2007
	LDA #$FF
	STA $2006
	STA $0103
	PLX

trackRunsAlready:
	RTS

preventResume:
	LDA $2002
	CMP #$53
	BEQ $05
	LDA #$91
	STA $2140
	RTL

eventHook:
	PHA
	PHP
	LDA #$0000
	SEP #$20
	LDA $2002
	CMP #$53
	BEQ $07
	PLP
	PLA
	JSL $82909B
	RTL
	CPX #$0051
	BNE $05
	LDA #$00     ; mute 
	JSR playEvent
	CPX #$0054
	BNE $05
	LDA #$03     ; unmute cave town music
	JSR playEvent
	CPX #$002a
	BNE $05
	LDA #$13     ; Rest at Gaia
	JSR playEvent
	CPX #$0012
	BNE $05
	LDA #$0A     ; Dungeon
	JSR playEvent
	CPX #$004B
	BNE $05
	LDA #$17     ; Lola's Theme
	JSR playEvent
	CPX #$0048
	BNE $05
	LDA #$18     ; Receive item
	JSR playEvent
	CPX #$000F
	BNE $05
	LDA #$09     ; Castle
	JSR playEvent
	CPX #$0009
	BNE noVillage
	LDA $0644
	CMP #$2E     ; got Statue
	BNE $04
	LDA #$00
	BRA $02
	LDA #$07     ; Village
	JSR playEvent

noVillage:
	CPX #$0042
	BNE $05
	LDA #$04     ; Dark Space
	JSR playEvent
	CPX #$000C
	BNE $05
	LDA #$05     ; Ghost Ship
	JSR playEvent
	CPX #$0003
	BNE $05
	LDA #$06     ; Town 2 (no gulls)
	JSR playEvent
	CPX #$0006
	BNE $05
	LDA #$06     ; Town 2 (no gulls)
	JSR playEvent
	CPX #$0015
	BNE $05
	LDA #$0C     ; Larai Cliffs
	JSR playEvent
	CPX #$002D
	BNE $05
	LDA #$12     ; Boss Battle
	JSR playEvent
	CPX #$004e
	BNE $05
	LDA #$19     ; Wind Melody
	JSR playEvent
	CPX #$005a
	BNE $05
	LDA #$1a     ; Memory Melody
	JSR playEvent
	CPX #$002a
	BNE $05
	LDA #$13     ; Returned Memeroy
	JSR playEvent
	CPX #$0018
	BNE $05
	LDA #$0F     ; Sky Garden
	JSR playEvent
	CPX #$001b
	BNE $05
	LDA #$10     ; Mu
	JSR playEvent
	CPX #$003f
	BNE $05
	LDA #$0E     ; Shipwreck
	JSR playEvent
	CPX #$0033
	BNE $05
	LDA #$0D     ; Meeting Mother
	JSR playEvent
	CPX #$0021
	BNE $05
	LDA #$08     ; Angkor Wat
	JSR playEvent
	CPX #$0024
	BNE $05
	LDA #$11     ; Pyramid
	JSR playEvent
	CPX #$0030
	BNE $05
	LDA #$14     ; Final Battle
	JSR playEvent
	CPX #$0039
	BNE $05
	LDA #$15     ; Ending1
	JSR playEvent
	CPX #$003C
	BNE $05
	LDA #$16     ; Ending2
	JSR playEvent

	PLP
	PLA
	JSL $82909B
	RTL

playEvent:	
	STA $0100
	CMP $0101
	BEQ noEvent
	STZ $2006
	STZ $0102
	STA $2004
	STZ $2005
	LDA #$01
	STA $0102  ;busy flag

noEvent:
	RTS

nmi:
	LDA #$0000
	SEP #$20
	LDA $0102
	BNE $03

endNMI:
	LDA #$81
	RTL
	CMP #$02
	BEQ fade
	Bit $2000
	bvs endNMI
	LDA $0101
	CMP $0100
	BNE $05
	STZ $0102
	BRA endNMI
	PHX
	LDX #$0000
	LDA $0100
	STA $0101
	TAX
	LDA @loop_table,x
	STA $2007
	LDA #$FF
	STA $2006
	STA $0103
	PLX 
	STZ $0102
	BRA endNMI

fade:
	lda $0103
	cmp #$10
	bcc endFade
	dec
	dec
	cmp #$10
	bcc endFade
	sta $2006
	sta $0103
	BRA endNMI

endFade:
	stz $2006
	stz $0103
	stz $0102
	BRA endNMI
	
fadeCommand:
	LDA #$F1
	STA $2140
	LDA $2002
	CMP #$53
	BEQ $01
	RTL
	LDA #$02
	STA $0102
	RTL
	
zeroBoot:
	LDA #$00
	STA $0100
	STA $0101
	STA $0102
	STA $0103
	JSL $829E44
	RTL




; --------------------hardcore apu mute
org $c29569
	db $00,$00

; --------------------hooks
org $c28752
	JSL muteSPC
	NOP
	NOP

org $cBE683
	JSL preventResume
	NOP

org $c3DA4D
	JSL loadTrack
	NOP
	NOP 

org $c3e244
	JSL eventHook

org $C08300
	JSL nmi

org $C3E1AC
	JSL fadeCommand
	NOP

org $C0802E
	JSL zeroBoot


;---------------------------------------------------------------------------
; THEME MAPPING
; hex dec tag                 loop youtube
; $00 00 mute                  n
; $01 01 Title                  y   https://www.youtube.com/watch?v=iJ95GUWak_E 
; $02 02 World                  y   https://www.youtube.com/watch?v=LjSK5TnpU2U
; $03 03 Town Theme 1(gulls)    y   https://www.youtube.com/watch?v=PopG6TAZq6M
; $04 04 Dark Space             y   https://www.youtube.com/watch?v=_vJKkkiMUqA
; $05 05 Dead Gold Ship         y   https://www.youtube.com/watch?v=xvxYivEFb-U
; $06 06 Town Theme 2(no gulls) y   https://www.youtube.com/watch?v=-a3XBqa1fdA
; $07 07 Village                y   https://www.youtube.com/watch?v=W7pqeqBw6kU  
; $08 08 Ankor Wat              y   https://www.youtube.com/watch?v=57hzpBiBdCE
; $09 09 Castle                 y   https://www.youtube.com/watch?v=CY00EHElRWM
; $0A 10 Dungeon                y   https://www.youtube.com/watch?v=7laPa-4blQk
; $0B 11 Great Wall             y   https://www.youtube.com/watch?v=W-HFQbs00DQ
; $0C 12 Larai Cliffs           y   https://www.youtube.com/watch?v=-H7waJPFd78
; $0D 13 Meeting Mother         y   https://www.youtube.com/watch?v=oyLJxUahjMY
; $0E 14 Shipwreck              y   https://www.youtube.com/watch?v=t38JSQuMbwk
; $0F 15 Sky Garden             y   https://www.youtube.com/watch?v=0SSPeNRdXMM
; $10 16 Mu                     y   https://www.youtube.com/watch?v=QOsT8R1POEA
; $11 17 Pyramid                y   https://www.youtube.com/watch?v=AywodvTQPuM
; $12 18 Boss Battle            y   https://www.youtube.com/watch?v=BlWyDZVsGvM
; $13 19 Returned Memory        y   https://www.youtube.com/watch?v=_CqZv5u8hgE
; $14 20 Final Battle           y   https://www.youtube.com/watch?v=f0SNFbcEhtE
; $15 21 Ending 1               y   https://www.youtube.com/watch?v=gHed9GSNXg8
; $16 22 Ending 2               y   https://www.youtube.com/watch?v=Sn5nC7ji-Qw

; SFX
; $17 23 Lola's Song            n   https://www.youtube.com/watch?v=j9ncd-kILUk
; $18 24 Receive Item           n   https://www.youtube.com/watch?v=ZmBBroOjVQw
; $19 25 Wind  Melody           n   https://www.youtube.com/watch?v=72EChRq--d0
; $1A 26 Memory Melody          n   https://www.youtube.com/watch?v=Q_UIdgbzjME
; $1B 27 Wind SFX Angel Village y   https://youtu.be/4JDNMqtESpo?t=3872
; note: this is special since the first seconds the sfx plays, then it mutes since it is not repeated due to apu mute

; Not needed:
; $1C 28 Transformation         n   https://www.youtube.com/watch?v=7kKbq8XfjpQ
; $1D 29 Phenomenon             n   https://www.youtube.com/watch?v=iLuk9smhcFw
; $1E 30 Unused Title           n   https://www.youtube.com/watch?v=679TIf9MlkA

; Not known
; $1F 31 Temple Song            n   https://www.youtube.com/watch?v=BuA3CzC8hzE

; $40 keep current music playing
