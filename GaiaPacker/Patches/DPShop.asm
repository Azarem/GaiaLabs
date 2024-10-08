﻿?BANK 0C

!token1   00E6
!token2   00E8
!dp_owned 0AD6


;This is a sample actor that behaves like a shop which takes DP
;To use this, replace an actor in scene events with this
;Example for Euro market: actor < #1A, #40, #00, %DPShop >   ;1E


-------------------------------

  h_actor < #02, #00, #10 >

main:
  LDA $0E
  ASL 
  ASL 
  ASL 
  CLC 
  ADC #$0002
  STA $28
  STZ $002A
  COP [C1]
  COP [8B]
  LDA #$3000
  STA $0E
  COP [0B]
  COP [C0] ( &on_interact )
  COP [C1]
  RTL 

------------------------------------------------

reward_table:
  #$0006 #$0081 #$0082 #$001A #$001D #$0012

price_table:
  #$0019 #$0032 #$0032 #$0032 #$0004 #$0005

------------------------------------------------

on_interact:
  COP [BF] ( &dialog_intro )

  LDA $dp_owned
  JSR double_dabble
  STA $token1
  
  COP [BF] ( &dialog_setup )

  ;COP [D0] ( #E9, #00, &print1 )
  ;COP [BF] ( &palette )

print1:
  LDA #$0000
  JSR print_entry
  COP [D1] ( #$0180, #00, &print2 )
  COP [BF] ( &palette )

print2:
  LDA #$0001
  JSR print_entry
  COP [D1] ( #$0181, #00, &print3 )
  COP [BF] ( &palette )
  
print3:
  LDA #$0002
  JSR print_entry
  COP [D1] ( #$0182, #00, &print4 )
  COP [BF] ( &palette )

print4:
  LDA #$0003
  JSR print_entry
;  COP [D0] ( #ED, #00, &print5 )
;  COP [BF] ( &palette )

;print5:
;  LDA #$0004
;  JSR print_entry
;  COP [D0] ( #EE, #00, &print6 )
;  COP [BF] ( &palette )

;print6:
;  LDA #$0005
;  JSR print_entry

  COP [BE] ( #04, #01, &selection_table )

  
-----------------------------------------------

print_entry:
  PHX
  ASL
  TAX
  LDA %price_table, X
  JSR double_dabble
  STA $token2		;Store price for string
  LDA %reward_table, X
  PLX
  
  BIT #$0080
  BEQ show_item
  SEC
  SBC #$0080
  BEQ show_hp
  DEC
  BEQ show_str
  DEC
  BEQ show_def
  
  COP [BF] ( &heal )
  BRA do_price

show_item:
  STA $token1		;Store item ID for string
  JSL GemShop
  BRA do_price
  
;show_nothing:
;  COP [BF] ( &nothing )
;  BRA do_price
  
show_hp:
  COP [BF] ( &health )
  BRA do_price

show_str:
  COP [BF] ( &strength )
  BRA do_price

show_def:
  COP [BF] ( &defense )
  
do_price:
  LDA $0998
  AND #$FFC0
  ORA #$0030
  STA $0998
  COP [BF] ( &price )
  RTS


--------------------------------------------

give_reward:
  PHX
  ASL
  TAX
  LDA $dp_owned
  STA $0000
  SEC
  SBC %price_table, X
  BPL has_enough
  
  PLX
  COP [BF] ( &not_enough )
  SEC
  RTS

has_enough:
  STA $dp_owned
  LDA %reward_table, X
  PLX

  JSL $83EF97
  BCS no_room
  RTS

no_room:
  LDA $0000
  STA $dp_owned
  COP [BF] ( &inv_full )
  SEC
  RTS

--------------------------------------------

selection_table:
  &on_cancel
  &on_select1
  &on_select2
  &on_select3
  &on_select4
  ;&on_select5
  ;&on_select6

on_cancel:
  COP [BF] ( &farewell )
  RTL

on_select1:
  ;COP [D0] ( #E9, #01, &on_reselect )
  LDA #$0000
  JSR give_reward
  BCS no_reward
  ;COP [CC] ( #E9 )
  BRA on_finish

on_select2:
  COP [D1] ( #0180, #01, &on_reselect )
  LDA #$0001
  JSR give_reward
  BCS no_reward
  COP [CD] ( #0180 )
  BRA on_finish

on_select3:
  COP [D1] ( #0181, #01, &on_reselect )
  LDA #$0002
  JSR give_reward
  BCS no_reward
  COP [CD] ( #$0181 )
  BRA on_finish

on_select4:
  COP [D1] ( #$0182, #01, &on_reselect )
  LDA #$0003
  JSR give_reward
  BCS no_reward
  COP [CD] ( #$0182 )
  BRA on_finish

;on_select5:
;  COP [D0] ( #04, #01, &on_reselect )
;  LDY #$0004
;  JSR give_reward
;  BCC no_reward
;  COP [CC] ( #ED )
;  BRA on_finish

;on_select6:
;  COP [D0] ( #EE, #01, &on_reselect )
;  LDY #$0005
;  JSR give_reward
;  BCC no_reward
;  COP [CC] ( #EE )
;  BRA on_finish

no_reward:
  RTL

on_reselect:
  COP [BF] ( &reselect )
  RTL

on_finish:
  COP [BF] ( &thankyou )
  RTL

-------------------------------------

double_dabble:
  PHX
  PHY
  TAX
  LDY #$0000
  LDA #$000F
  STA $token1
  BRA dabble_next

carry_first:
  TYA
  AND #$0F00
  CMP #$0500
  BMI carry_second
  TYA
  CLC
  ADC #$0300
  TAY

carry_second:
  TYA
  AND #$00F0
  CMP #$0050
  BMI carry_third
  TYA
  CLC
  ADC #$0030
  TAY

carry_third:
  TYA
  AND #$000F
  CMP #$0005
  BMI dabble_next
  TYA
  CLC
  ADC #$0003
  TAY
  
dabble_next:
  TXA
  ROL
  TAX
  TYA
  ROL
  TAY
  DEC $token1
  BPL carry_first

dabble_end:
  TYA
  PLY
  PLX
  RTS

----------------------------------------------------

dialog_intro `[DEF]Welcome to my shop.[N]Take a look around![END]`
dialog_setup `[DLG:3,D][SIZ:D,5][CLR][SFX:0]    Dark Points: [BCD:3,token1]`
not_enough   `[CLD][DEF]You don't have enough[N]Dark Points.[END]`
farewell     `[CLD][DEF]Come back soon![END]`
thankyou     `[CLD][DEF]Thank you![END]`
reselect     `[CLD][DEF]You already bought this.[END]`
inv_full     `[CLD][DEF]Your inventory is full![END]`

price        `  [BCD:2,token2][PAL:0]`
strength     `[N] Strength`
defense      `[N] Defense`
health       `[N] Health`
heal         `[N] Heal`
;nothing      `[N] Nothing`
palette      `[PAL:4]`
