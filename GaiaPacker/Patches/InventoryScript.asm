?BANK 02

?INCLUDE 'inventory_menu'
?INCLUDE 'system_strings'

!inventory_slots                0AB4
!page_1                         0AB4
!inventory_equipped_index       0AC4
!inventory_equipped_type        0AC6
!page_2                         0B34  ;16 items per page
!state_1                        0B54

;ASM for enhancing the inventory script

----------------------------------------------------

ins_main_press_lr:
  LDA #$0300
  TSB $0658
 ; JMP on_press_lr
  
ins_on_press_lr:
  LDA #$0030
  TSB $0658
  PHY
  LDA #$000F
  STA $24

ins_swap_top:
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
  JSR sub_02E9ED
  PLA
  STA $0028, Y
  
  DEC $24
  BPL ins_swap_top

ins_finish_swap:
  SEP #$20
  LDA $state_1
  EOR #$01
  STA $state_1
  AND #$01
  REP #$20
  BEQ ins_indicator_l

ins_indicator_r:
  LDA #$3407
  STA $7F06BA
  LDA #$4406
  STA $7F06BC
  BRA ins_finish_final

ins_indicator_l:
  LDA #$3407
  STA $7F06BC
  LDA #$4406
  STA $7F06BA

ins_finish_final:
  COP [06] ( #10 )

  LDY $1A
  LDA $page_1, Y
  AND #$00FF
  STA $0AC6
  STA $0AE8
  
  PLY
  RTL

-------------------------------------------

ins_on_item_lr:
  JSL ins_on_press_lr
  JMP code_02E47F
  
-------------------------------------------
;Hook for setting up indicator

e_inventory_menu {
    COP [88] ( @table_108000 )
    COP [BD] ( @string_01E869 )
    STZ $0AFA
    LDA #$000F
    STA $24

    LDA #$0001
    AND $state_1
    BEQ ins_init_l
    
    LDA #$3407
    STA $7F06BA
    LDA #$4406
    STA $7F06BC
    BRA code_02E3AB

  ins_init_l:
    LDA #$3407
    STA $7F06BC
    LDA #$4406
    STA $7F06BA
}

-------------------------------------------
;Hook for listening for lr press on main

code_02E40C {
    COP [BD] ( @string_01E90B )
    COP [BD] ( @string_01E8E9 )
    STZ $1C
    LDA #$FFFF
    STA $18
    LDA #$8000
    TSB $0658
    COP [C2]

    LDA $0AFA
    CMP #$0003
    BEQ $0C                    ;Don't listen for LR press when info menu is up
    COP [40] ( #$0030, &ins_on_press_lr )
    COP [40] ( #$0300, &ins_main_press_lr )

    JSR $&sub_02EC58
    BCS func_02E445
    LDA $0AFA
    CMP $18
    BNE code_02E432
    RTL 
}
  
-------------------------------------------
;Equip handler

code_02E47F {
    JSR $&sub_02EA73
    COP [BD] ( @string_01E912 )
    LDY $1A
    LDA $inventory_slots, Y
    AND #$00FF
    STA $0AE8
    COP [BD] ( @string_01E9D0 )
    COP [C2]
    COP [40] ( #$0030, &ins_on_item_lr )           ;This
    COP [40] ( #$C040, &code_02E50B )
    COP [40] ( #$0800, &code_02E4B8 )
    COP [40] ( #$0400, &code_02E4CE )
    COP [40] ( #$0200, &code_02E4E4 )
    COP [40] ( #$0100, &code_02E4F7 )
    RTL 
}
  
-------------------------------------------
;Arrange handler

code_02E555 {
    JSR $&sub_02EA3D
    COP [C2]
    COP [40] ( #$0030, &ins_on_press_lr )       ;This
    COP [40] ( #$4040, &code_02E64C )
    COP [40] ( #$8000, &code_02E583 )
    PEA $&code_02E555-1
    COP [40] ( #$0800, &sub_02E981 )
    COP [40] ( #$0400, &sub_02E996 )
    COP [40] ( #$0200, &sub_02E9AB )
    COP [40] ( #$0100, &sub_02E9BD )
    PLA 
    RTL 
}

-------------------------------------------
;Second handler (flag state on entry)

code_02E5AC {
    SEP #$20
    LDA $state_1
    BIT #$01
    BEQ ins_second_reset
    LDA #$02
    TSB $state_1
    BRA ins_second_next

  ins_second_reset:
    LDA #$02
    TRB $state_1

  ins_second_next:
    REP #$20

  ins_second_callback:
    JSR $&sub_02EA3D
    COP [C2]
    COP [40] ( #$0030, &ins_on_press_lr )         ;This
    COP [40] ( #$4040, &code_02E64A )
    COP [40] ( #$8000, &code_02E5DA )
    PEA $&ins_second_callback-1
    COP [40] ( #$0800, &sub_02E981 )
    COP [40] ( #$0400, &sub_02E996 )
    COP [40] ( #$0200, &sub_02E9AB )
    COP [40] ( #$0100, &sub_02E9BD )
    PLA 
    RTL 
}

---------------------------------------------
;Move handler

code_02E5DA {
    LDA #$8000
    TSB $0658
    
    SEP #$20
    LDA $state_1
    BIT #$01
    BNE ins_move_process_odd
    BIT #$02
    BEQ ins_move_same_page
    BRA ins_move_process_swap
  
  ins_move_process_odd:
    BIT #$02
    BNE ins_move_same_page

  ins_move_process_swap:
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
    BRA ins_finish_move

ins_move_same_page:
    REP #$20

    LDA $22
    CMP $2E
    BEQ code_02E5AC

    COP [06] ( #11 )
    
    LDY $22
    SEP #$20
    LDA $page_1, Y
    XBA 
    LDY $2E
    LDA $page_1, Y
    XBA 
    STA $page_1, Y
    XBA 
    LDY $22
    STA $page_1, Y
    REP #$20
    LDY $2E
    LDA $page_1, Y
    AND #$00FF
    PHA 
    TYA 
    JSR $&sub_02E9ED
    PLA 
    STA $0028, Y
    LDY $22
    LDA $inventory_slots, Y
    AND #$00FF

  ins_finish_move:
    PHA 
    TYA 
    JSR $&sub_02E9ED
    PLA 
    STA $0028, Y
    TYA 
    CMP $inventory_equipped_index
    BNE code_02E631
    LDA $2E
    STA $inventory_equipped_index
    BRA code_02E63D
}

-------------------------------------------

code_02E67C {
    JSR $&sub_02EA3D
    COP [C2]
    COP [40] ( #$0030, &ins_on_press_lr )              ;This
    COP [40] ( #$4040, &code_02E70B )
    COP [40] ( #$8000, &code_02E6AA )
    PEA $&code_02E67C-1
    COP [40] ( #$0800, &sub_02E981 )
    COP [40] ( #$0400, &sub_02E996 )
    COP [40] ( #$0200, &sub_02E9AB )
    COP [40] ( #$0100, &sub_02E9BD )
    PLA 
    RTL 
}
