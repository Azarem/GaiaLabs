?BANK 03

?INCLUDE 'sFA_diary_menu'

---------------------------------

run_button_main:
  LDA $09AA
  BEQ run_button_end    ;Skip if player has no actor
  LDA $09B2
  BNE run_button_end    ;Skip if running already
  LDA $09B4
  BNE run_button_end    ;Skip if running already
  LDA $09AE
  BIT #$3800
  BNE run_button_end    ;Skip if movement is disabled or on a ladder or ramp
  LDA $0656
  BIT #$0080
  BEQ run_button_end    ;Skip if not pressing (A/Y)
  XBA
  LSR
  BCS run_button_e
  LSR
  BCS run_button_w
  LSR
  BCS run_button_s
  LSR
  BCS run_button_n

  ;Skip if not pressing a direction
run_button_end:
  RTL        
  
run_button_n:
  LDA #$FFFD
  BRA run_store_ns

run_button_s:
  LDA #$0003

run_store_ns:
  STA $09B4
  STZ $09B8  ;Reset brake counter
  RTL
  
run_button_w:
  LDA #$FFFD
  BRA run_store_ew

run_button_e:
  LDA #$0003

run_store_ew:
  STA $09B2
  STZ $09B8  ;Reset brake counter
  RTL

--------------------------------------

;Fix Y button mapping for style 1
code_0BE695 {
    LDA #$0040
    STA $0DB0
    LDA $0B26
    BNE code_0BE6B3
    LDA #$8000
    STA $0DAC
    LDA #$4000
    STA $0DAA
    LDA #$0080   ;This
    STA $0DAE
}

code_0BE6B3 {
    RTS 
}

widestring_0BF538 `[DLG:6,8][SIZ:A,8][SKP:2][LU1:B]Snd/Buttons[N]End Changes[N]Sound[N][LU2:2]Type[N][SKP:5]   :Attack/Talk[N][SKP:5]   :Item/Cancel[N][SKP:5]   :Item Menu[N][SKP:5]   :Sprint`
widestring_0BF5AD `[DLG:6,8][SIZ:A,8]Arrangement  OK?[N]Start Journey[N]Sound[N][LU2:2]Type[N][SKP:5]   :Attack/Talk[N][SKP:5]   :Item/Cancel[N][SKP:5]   :Item Menu[N][SKP:5]   :Sprint`
