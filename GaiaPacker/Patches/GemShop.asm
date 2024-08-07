?BANK 01

!gem_id	       01
!item_count    06
!token1        00E6
!token2	       00E8
!page_1        0AB4
!page_2        0B34
!jewel_count   0B56

---------------------------------------------

main:
  COP [BF] ( &item_string )
  RTL


item_string	`[N] [ADR:&itemDictionary,token1]`

---------------------------------------------

08CEA3:
  COP [D0] ( #E8, #01, &destroy )
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

destroy:
  COP [E0]

---------------------------------------------

reward_table:  ;(8000 for 'my secrets')
  #$0006 #$0081 #$0082 #$0015 #$000E #$0010

price_table:   ;(BCD format)
  #$0002 #$0003 #$0003 #$0004 #$0004 #$0004
  
---------------------------------------------

on_interact:
  COP [BF] ( &dialog_intro )
  
  SEP #$20
  LDA #$00
  XBA
  LDA $jewel_count
  BEQ search_inventory
  STZ $jewel_count
  REP #$20
  STA $26
  SED
  LDA $0AB0

search_add_top:
  CLC
  ADC #$0001
  DEC $26
  BEQ search_store_gems
  BRA search_add_top
  
search_store_gems:
  STA $0AB0
  CLD
  SEP #$20

search_inventory:
  LDA #$gem_id
  LDY #$000F

search_top_1:
  CMP $page_1, Y
  BNE search_next_1
  PHA
  LDA #$00
  STA $page_1, Y		
  PLA 

search_next_1:
  DEY
  BPL search_top_1
  LDY #$000F

search_top_2:
  CMP $page_2, Y
  BNE search_next_2
  PHA
  LDA #$00
  STA $page_2, Y		
  PLA 

search_next_2:
  DEY
  BPL search_top_2

  REP #$20

  COP [BF] ( &dialog_setup )

  COP [D0] ( #E9, #00, &print1 )
  COP [BF] ( &palette )

print1:
  LDA #$0000
  JSR print_entry
  COP [D0] ( #EA, #00, &print2 )
  COP [BF] ( &palette )

print2:
  LDA #$0001
  JSR print_entry
  COP [D0] ( #EB, #00, &print3 )
  COP [BF] ( &palette )
  
print3:
  LDA #$0002
  JSR print_entry
  COP [D0] ( #EC, #00, &print4 )
  COP [BF] ( &palette )

print4:
  LDA #$0003
  JSR print_entry
  COP [D0] ( #ED, #00, &print5 )
  COP [BF] ( &palette )

print5:
  LDA #$0004
  JSR print_entry
  COP [D0] ( #EE, #00, &print6 )
  COP [BF] ( &palette )

print6:
  LDA #$0005
  JSR print_entry
  COP [BE] ( #06, #01, &selection_table )

---------------------------------------------

print_entry:
  PHX
  ASL
  TAX
  LDA %price_table, X
  STA $token2		;Store price for string
  LDA %reward_table, X
  BMI show_secrets
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
  JSL main
  BRA do_price

show_secrets:
  PLX
  COP [BF] ( &secrets )
  BRA do_price
  
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

---------------------------------------------

give_reward:
  PHX
  ASL
  TAX
  LDA $0AB0
  STA $0000
  SED
  SEC
  SBC %price_table, X
  CLD
  BPL has_enough
  
  PLX
  COP [BF] ( &not_enough )
  SEC
  RTS

has_enough:
  STA $0AB0
  LDA %reward_table, X
  BMI give_secrets

  PLX
  JSL $83EF97
  BCS no_room
  RTS

give_secrets:
  PLX
  PLA             ;Pull the return address from the stack so we can exit properly
  COP [BF] ( &secret_reward )
  LDA #$0202
  STA $0648
  LDA #$0404
  STA $064A
  COP [26] ( #E9, #$0330, #$03D0, #80, #$4400 )
  RTL

no_room:
  LDA $0000
  STA $0AB0
  COP [BF] ( &inv_full )
  SEC
  RTS

---------------------------------------------

selection_table:
  &on_cancel
  &on_select1
  &on_select2
  &on_select3
  &on_select4
  &on_select5
  &on_select6

on_cancel:
  COP [BF] ( &farewell )
  RTL

on_select1:
  COP [D0] ( #E9, #01, &on_reselect )
  LDA #$0000
  JSR give_reward
  BCS no_reward
  COP [CC] ( #E9 )
  BRA on_finish

on_select2:
  COP [D0] ( #EA, #01, &on_reselect )
  LDA #$0001
  JSR give_reward
  BCS no_reward
  COP [CC] ( #EA )
  BRA on_finish

on_select3:
  COP [D0] ( #EB, #01, &on_reselect )
  LDA #$0002
  JSR give_reward
  BCS no_reward
  COP [CC] ( #EB )
  BRA on_finish

on_select4:
  COP [D0] ( #EC, #01, &on_reselect )
  LDA #$0003
  JSR give_reward
  BCS no_reward
  COP [CC] ( #EC )
  BRA on_finish

on_select5:
  COP [D0] ( #ED, #01, &on_reselect )
  LDA #$0004
  JSR give_reward
  BCS no_reward
  COP [CC] ( #ED )
  BRA on_finish

on_select6:
  COP [D0] ( #EE, #01, &on_reselect )
  LDA #$0005
  JSR give_reward
  BCS no_reward
  COP [CC] ( #EE )
  BRA on_finish

no_reward:
  RTL

on_reselect:
  COP [BF] ( &reselect )
  RTL

on_finish:
  COP [BF] ( &thankyou )
  RTL

---------------------------------------------

dialog_intro  `[DEF]I am the jeweler, Gem.[N]I control the Seven[N]Seas.[END]`
dialog_setup  `[DLG:3,B][SIZ:D,7][CLR][SFX:0]      Red Jewels: [BCD:2,AB0]`
not_enough    `[CLD][DEF]Come back when you[N]have more Red Jewels.[END]`
farewell      `[CLD][DEF]See me any time.[END]`
thankyou      `[CLD][DEF]I appreciate your[N]business, come back[N]soon.[END]`
reselect      `[CLD][DEF]You already purchased[N]this I see.[END]`
inv_full      `[CLD][DEF]Your inventory is full![END]`
secret_reward `[CLD][DEF]The [LU1:DF]has [LU1:79]to[N][LU2:AD]you [LU1:D0]of[N]my secrets.[FIN]Follow me!![END]`


price         `  [BCD:2,token2][PAL:0]`
strength      `[N] Strength`
defense       `[N] Defense`
health        `[N] Health`
heal          `[N] Heal`
secrets       `[N] My Secrets`
;nothing       `[N] Nothing`
palette       `[PAL:4]`
