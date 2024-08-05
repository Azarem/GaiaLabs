?BANK 03

!page_1        0AB4
!page_2        0B34  ;16 items per page

!state_1       0B54
!herb_count    0B55
!jewel_count   0B56
!crystal_count 0B57


--------------------------------

give_item_handler:
  CMP #$01
  BEQ give_jewel
  CMP #$06
  BEQ give_herb
  CMP #$0E
  BEQ give_crystal

  JSR check_pages
  BCS exit_fail
  JMP $F074

exit_fail:
  JMP $F081

give_jewel:
  LDA $jewel_count
  BNE $05
  LDA #01
  JSR check_pages
  INC $jewel_count
  LDA #01
  JMP $F074

give_herb:
  LDA $herb_count
  BNE $07
  LDA #$06
  JSR check_pages
  BCS exit_fail
  INC $herb_count
  LDA #06
  JMP $F074
  
give_crystal:
  LDA $crystal_count
  BNE $07
  LDA #$0E
  JSR check_pages
  BCS exit_fail
  INC $crystal_count
  LDA #0E
  JMP $F074

--------------------------------

check_pages:
  PHA
  LDY #$page_1

check_page_1:
  LDA $0000, Y
  BEQ give_item
  INY
  CPY #$page_1+10
  BNE check_page_1

  LDY #$page_2

check_page_2:
  LDA $0000, Y
  BEQ give_item
  INY
  CPY #$page_2+10
  BNE check_page_2

no_room:
  PLA
  SEC
  RTS

give_item:
  PLA
  STA $0000, Y
  CLC
  RTS

--------------------------------

remove_item_handler:
  PHX
  LDX $0AC4
  SEP #$20
  LDA $page_1, X
  CMP #$01
  BEQ remove_jewel
  CMP #$06
  BEQ remove_herb
  CMP #$0E
  BEQ remove_crystal
  BRA remove_item

remove_jewel:
  SED
  REP #$20
  LDA $0AB0

add_jewel_count:
  CLC
  ADC #$0001

  SEP #$20
  DEC $jewel_count
  BMI remove_jewel_minus
  REP #$20
  BEQ finish_jewel_item
  BRA add_jewel_count

remove_jewel_minus:
  STZ $jewel_count
  REP #$20
  
finish_jewel_item:
  STA $0AB0
  CLD
  SEP #$20
  BRA remove_item

remove_herb:
  DEC $herb_count
  BMI remove_herb_minus
  BEQ remove_item
  PLX
  RTS

remove_herb_minus:
  STZ $herb_count
  BRA remove_item
  
remove_crystal:
  DEC $crystal_count
  BMI remove_crystal_minus
  BEQ remove_item
  PLX
  RTS

remove_crystal_minus:
  STZ $crystal_count

remove_item:
  STZ $page_1, X
  REP #$20
  LDA #$0000
  STA $0AC6
  DEC
  STA $0AC4
  PLX
  RTS

--------------------------------

;Prevent gem count increase on use handler
0384DC:
  BRA $0A

039FB2:
  JMP remove_item_handler

;Entry point for adding an item to the inventory
03EF9E:
  JMP give_item_handler
