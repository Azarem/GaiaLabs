?BANK 03

---------------------------------

main:
  LDA $09AA
  BEQ end    ;Skip if player has no actor
  LDA $09B2
  BNE end    ;Skip if running already
  LDA $09B4
  BNE end    ;Skip if running already
  LDA $09AE
  BIT #$2800
  BNE end    ;Skip if movement is disabled or on a ladder
  LDA $0656
  BIT #$0080
  BEQ end    ;Skip if not pressing (A/Y)
  XBA
  LSR
  BCS run_e
  LSR
  BCS run_w
  LSR
  BCS run_s
  LSR
  BCS run_n

  ;Skip if not pressing a direction
end:
  RTL        
  
run_n:
  LDA #$FFFD
  BRA store_ns

run_s:
  LDA #$0003

store_ns:
  STA $09B4
  STZ $09B8  ;Reset brake counter
  RTL
  
run_w:
  LDA #$FFFD
  BRA store_ew

run_e:
  LDA #$0003

store_ew:
  STA $09B2
  STZ $09B8  ;Reset brake counter
  RTL

-------------------------------------------
;These disable run with double-tap

;02C4E9:
;  BRA $0A
  
;02C531:
;  BRA $0A

;02C57A:
;  BRA $0A

;02C5C4:
;  BRA $0A

--------------------------------------

0BE6AD  #$0080  ;Fix Y button mapping for style 1

0BF538 `[DLG:6,8][SIZ:A,8][SKP:2][LU1:B]Snd/Buttons[N]End Changes[N]Sound[N][LU2:2]Type[N][SKP:5]   :Attack/Talk[N][SKP:5]   :Item/Cancel[N][SKP:5]   :Item Menu[N][SKP:5]   :Sprint`
0BF5AD `[DLG:6,8][SIZ:A,8]Arrangement  OK?[N]Start Journey[N]Sound[N][LU2:2]Type[N][SKP:5]   :Attack/Talk[N][SKP:5]   :Item/Cancel[N][SKP:5]   :Item Menu[N][SKP:5]   :Sprint`
