?BANK 03

?INCLUDE 'chunk_038000'
?INCLUDE 'chunk_03BAE1'
?INCLUDE 'jeweler_gem'

!player_actor                   09AA
!jewels_collected               0AB0
!inventory_slots                0AB4
!inventory_equipped_index       0AC4
!inventory_equipped_type        0AC6
;!page_2                         0B34  ;16 items per page

!state_1                        0B54
!herb_count                     0B55
!jewel_count                    0B56
!crystal_count                  0B57

--------------------------------------------------------

inv_check_pages {
    PHA
    LDY #inventory_slots

  inv_check_page_1:
    LDA $0000, Y
    BEQ inv_give_item
    INY
    CPY #$inventory_slots+10
    BNE inv_check_page_1

    ;LDY #$page_2

  ;inv_check_page_2:
    ;LDA $0000, Y
    ;BEQ inv_give_item
    ;INY
    ;CPY #$page_2+10
    ;BNE inv_check_page_2

  inv_no_room:
    PLA
    SEC
    RTS

  inv_give_item:
    PLA
    STA $0000, Y
    CLC
    RTS
}

------------------------------------------------

  inv_remove_stub:
    SEP #$20
    LDA $inventory_slots, X
    CMP #$01
    BEQ inv_remove_jewel
    CMP #$06
    BEQ inv_remove_herb
    CMP #$0E
    BEQ inv_remove_crystal
    BRA inv_remove_item

  inv_remove_jewel:
    SED
    REP #$20
    LDA $jewels_collected

  inv_add_jewel_count:
    CLC
    ADC #$0001 

    SEP #$20
    DEC $jewel_count
    BMI inv_remove_jewel_minus
    REP #$20
    BEQ inv_finish_jewel_item
    BRA inv_add_jewel_count

  inv_remove_jewel_minus:
    STZ $jewel_count
    REP #$20
   
  inv_finish_jewel_item:
    STA $jewels_collected
    CLD
    SEP #$20
    BRA inv_remove_item

  inv_remove_herb:
    DEC $herb_count
    BMI inv_remove_herb_minus
    BEQ inv_remove_item
    RTS

  inv_remove_herb_minus:
    STZ $herb_count
    BRA inv_remove_item
  
  inv_remove_crystal:
    DEC $crystal_count
    BMI inv_remove_crystal_minus
    BEQ inv_remove_item
    RTS

  inv_remove_crystal_minus:
    STZ $crystal_count

  inv_remove_item:
    STZ $inventory_slots, X
    CPX $inventory_equipped_index
    BNE inv_remove_finish
    REP #$20
    LDA #$0000
    STA $inventory_equipped_type
    DEC
    STA $inventory_equipped_index
    LDA #$1000
    TRB $06EE

  inv_remove_finish:
    RTS

------------------------------------------------
;Entry point for gem use (prevent increase, this is done elsewhere)
func_0384D5 {
    COP [BF] ( &widestring_038517 )
    JSR $&sub_039FB2

;    SED 
;    LDA $jewels_collected
;    CLC 
;    ADC #$0001
;    STA $jewels_collected
;    CLD 

    PHX 
    PHD 
    LDA $player_actor
    TCD 
    TAX 
    COP [A5] ( @code_038566, #00, #00, #$2000 )
    TYX 
    LDA #$0000
    STA $0012, X
    LDA #$3000
    STA $000E, X
    LDY $player_actor
    LDA $0014, Y
    STA $0014, X
    LDA $0016, Y
    STA $0016, X
    PLD 
    PLX 
    RTS 
}

---------------------------------------------
;Entry point for removing an item from the inventory upon use

sub_039FB2 {
    PHX
    LDX $inventory_equipped_index
    JSR inv_remove_stub
    PLX
    REP #$20
    RTS
}

--------------------------------------------------
;Entry point for adding an item to the inventory

func_03EF97 {
    PHP 
    SEP #$20
    BIT #$80
    BNE code_03EFB3

  inv_give_item_handler:
    CMP #$01
    BEQ inv_give_jewel
    CMP #$06
    BEQ inv_give_herb
    CMP #$0E
    BEQ inv_give_crystal

    JSR inv_check_pages
    BCS inv_exit_fail
    JMP code_03F070+4

  inv_exit_fail:
    JMP code_03F080+1

  inv_give_jewel:
    LDA $jewel_count
    BNE $05
    LDA #01
    JSR inv_check_pages
    INC $jewel_count
    LDA #01
    JMP code_03F070+4 

  inv_give_herb:
    LDA $herb_count
    BNE $07
    LDA #$06
    JSR inv_check_pages
    BCS inv_exit_fail
    INC $herb_count
    LDA #06
    JMP code_03F070+4
  
  inv_give_crystal:
    LDA $crystal_count
    BNE $07
    LDA #$0E
    JSR inv_check_pages
    BCS inv_exit_fail
    INC $crystal_count
    LDA #0E
    JMP code_03F070+4

}

---------------------------------------------
;Entry point for checking inventory for item

code_03F0B9 {
    CMP $inventory_slots, Y
    BEQ code_03F0C7
    INY 
    CPY #$0010
    BNE code_03F0B9
    ;LDY #$0000

  ;inv_search_page2:
    ;CMP $page_2, Y
    ;BEQ code_03F0C7
    ;INY 
    ;CPY #$0010
    ;BNE inv_search_page2

    PLP 
    SEC 
    RTL 
}

---------------------------------------------
;Route COP item removal through common process

code_03F0A0 {
    PHX
    TYX
    JSR inv_remove_stub
    PLX
    REP #$20
}

-----------------------------------------
;Entry point for giving Jewels to Gem

code_08CF68 {
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
    LDA #$01
    LDY #$000F

  search_top_1:
    CMP $inventory_slots, Y
    BNE search_next_1
    PHA
    LDA #$00
    STA $inventory_slots, Y		
    PLA

  search_next_1:
    DEY
    BPL search_top_1
    ;LDY #$000F

  ;search_top_2:
    ;CMP $page_2, Y
    ;BNE search_next_2
    ;PHA
    ;LDA #$00
    ;STA $page_2, Y		
    ;PLA 

  ;search_next_2:
  ;  DEY
  ;  BPL search_top_2

    REP #$20
}

code_08CF74 {
}

code_08CF84 {
}
