?BANK 03


;Button flags
;8000 - (B) Attack/Talk
;4000 - (Y) Item/Cancel
;2000 - (SEL) Menu
;1000 - (STA) Pause
;0800 - UP
;0400 - DOWN
;0200 - LEFT
;0100 - RIGHT
;0080 - (A) Nothing
;0040 - (X) Nothing
;0020 - (L) Spin
;0010 - (R) Spin

--------------------------------------------

global_script_hook:
  ;Add global scripts to run with actor code here
  JSL %RunButton

  LDA $56
  BEQ $03
  JMP $CB1E
  JMP $CB90

----------------------------------------

bcd_hex_fix:
  LDA $0000, Y
  AND #$0F
  CMP #$0A
  BMI numeric1

  CLC
  ADC #$37
  BRA store1

numeric1:
  ORA #$30

store1:
  REP #$20
  DEX 
  DEX 
  STA $7F0200, X
  SEP #$20
  DEC $000E
  BEQ bcd_end
  LDA $0000, Y
  INY 
  ;AND #$F0
  LSR 
  LSR 
  LSR 
  LSR
  CMP #$0A
  BMI numeric2

  CLC
  ADC #$37
  BRA store2

numeric2:
  ORA #$30

store2:
  REP #$20
  DEX 
  DEX 
  STA $7F0200, X
  SEP #$20
  DEC $000E
  BNE bcd_hex_fix

bcd_end:
  JMP $EDC1

--------------------------------------------

;Prevent drawing of HUD BG layers
03DECD:
  RTL

;Fix for string code 05 BCD processing to include hex chars
03ED90:
  JMP bcd_hex_fix

;DMA size (two lines)
;02B065:
;  LDX #$0080

;VRAM DMA arguments (when copying BG3 layer)
;02B078:
;  LDX #$7820
;  STX $2116
;  LDX #$0240

;Entry point for actor scripts
03CB1A:
  JMP global_script_hook

;|[CUR:46,0][NHM:8][HP][CUR:9C,0][NHM:14][BCD:1,AD8][CUR:A0,0][NUM:AD6][CUR:5E,0][BCD:2,644][CUR:7A,0][BCD:3,9A2][CUR:BA,0][BCD:3,9A4]|
;|[NHM:4][CUR:66,0][HE]|
