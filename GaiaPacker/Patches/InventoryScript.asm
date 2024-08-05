?BANK 02

!page_1        0AB4
!page_2        0B34  ;16 items per page
!state_1       0B54

;ASM for enhancing the inventory script

  
-------------------------------------------

main_handler:
  COP [C2]
  LDA $0AFA
  CMP #$0003
  BEQ main_finish                    ;Don't listen for LR press when info menu is up
  COP [40] ( #$0030, &on_press_lr )
  COP [40] ( #$0300, &main_press_lr )

main_finish:
  JSR $EC58
  JMP $E428

main_press_lr:
  LDA #$0300
  TSB $0658
  JMP on_press_lr
  
-------------------------------------------

item_handler:
  COP [C2]
  COP [40] ( #$0030, &on_item_lr )
  COP [40] ( #$C040, #$E50B )
  JMP $E49F
  
on_item_lr:
  JSL on_press_lr
  JMP $E47F
  
-------------------------------------------

arrange_handler:

arrange_callback:
  JSR $EA3D
  COP [C2]
  COP [40] ( #$0030, &on_press_lr )
  JMP $E55A
  
-------------------------------------------

second_handler:
  SEP #$20
  LDA $state_1
  BIT #$01
  BEQ second_reset
  LDA #$02
  TSB $state_1
  BRA second_next

second_reset:
  LDA #$02
  TRB $state_1

second_next:
  REP #$20

second_callback:
  JSR $EA3D
  COP [C2]
  COP [40] ( #$0030, &on_press_lr )
  JMP $E5B1

---------------------------------------------

move_process:
  LDA #$8000
  TSB $0658
  SEP #$20
  LDA $state_1
  BIT #$01
  BNE move_process_odd
  BIT #$02
  BEQ move_same_page
  BRA move_process_swap
  
move_process_odd:
  BIT #$02
  BNE move_same_page

move_process_swap:
  COP [06] ( #11 )
  PHX

  LDY $22
  LDX $2E
  LDA $page_1, Y
  XBA
  LDA $page_2, X
  XBA
  STA $page_2, X
  LDA #$00
  XBA
  STA $page_1, Y

  PLX
  REP #$20
  JMP $E61B

move_same_page:
  REP #$20
  JMP $E5E0

-------------------------------------------

discard_handler:
  COP [C2]
  COP [40] ( #$0030, &on_press_lr )
  COP [40] ( #$4040, #$E70B )
  JMP $E687

-------------------------------------------

on_press_lr:
  LDA #$0030
  TSB $0658
  PHY
  LDA #$000F
  STA $24

swap_top:
  LDY $24

  SEP #$20
  LDA $page_1, Y
  XBA
  LDA $page_2, Y
  XBA
  STA $page_2, Y
  LDA #$00
  XBA
  STA $page_1, Y
  REP #$20

  PHA
  TYA
  JSR $E9ED
  PLA
  STA $0028, Y
  
  DEC $24
  BPL swap_top

finish_swap:
  SEP #$20
  LDA $state_1
  EOR #$01
  STA $state_1
  REP #$20
  COP [06] ( #10 )

  LDY $1A
  LDA $page_1, Y
  AND #$00FF
  STA $0AC6
  STA $0AE8
  
  PLY
  RTL

---------------------------------------Hooks

02E423:
  JMP main_handler

02E497:
  JMP item_handler

02E555:
  JMP arrange_handler

02E566:
  PEA arrange_callback-1
  
02E5AC:
  JMP second_handler

02E5BD:
  PEA second_callback-1

02E5DA:
  JMP move_process

02E67F:
  JMP discard_handler