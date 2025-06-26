?BANK 02

!num_xoffset                      01
!num_yoffset                      F7
!scene_current                  0644
!inventory_slots                0AB4
!herb_count                     0B55
!jewel_count                    0B56
!crystal_count                  0B57

---------------------------------------------

generate_number_sprites {
    ;BRK #$70
    PHX
    LDA #$0000
    STZ $00DA
    SEP #$20

    LDY #$0010

  generate_number_top:
    DEY
    BMI generate_number_quit
    LDA $inventory_slots, Y
    CMP #$01
    BEQ generate_number_jewels
    CMP #$06
    BEQ generate_number_herbs
    CMP #$0E
    BEQ generate_number_crystals
    BRA generate_number_top
    
  generate_number_herbs:
    LDA $herb_count
    BRA generate_number_next

  generate_number_jewels:
    LDA $jewel_count
    BRA generate_number_next

  generate_number_crystals:
    LDA $crystal_count

  generate_number_next:
    PHA
    
    TYA
    ASL
    ASL
    TAX
    LDA @binary_02EB86, X
    CLC
    ADC #$num_xoffset
    STA $25

    LDA @binary_02EB86+2, X
    CLC
    ADC #$num_yoffset
    STA $26

    PLA
    CMP #$0A
    BCC generate_process_single
    STZ $24

  generate_process_top:
    ;SEC
    SBC #$0A
    INC $24
    CMP #$0A
    BCS generate_process_top

    PHA
    LDA $24
    JSR &push_number_sprite
    LDA $25
    CLC
    ADC #$04
    STA $25
    PLA
    
  generate_process_single:
    JSR &push_number_sprite

    JMP generate_number_top

  generate_number_quit:
    REP #$20
    PLX
    RTS
}

----------------------------------------------

push_number_sprite {
    LDX $00DA

  push_number_next:
    ORA #$80                   ;Bottom half
    STA $7F1FC2, X

    LDA $25
    STA $7F1FC0, X

    LDA $26
    STA $7F1FC1, X

    LDA #$21
    STA $7F1FC3, X

    INX
    INX
    INX
    INX
    STX $00DA

    RTS
}

---------------------------------------------
?INCLUDE 'inventory_menu'
?INCLUDE 'system_strings'
---------------------------------------------

e_inventory_menu {
    COP [88] ( @table_108000 )
    COP [BD] ( @asciistring_01E869 )
    LDA #$1000
    TSB $06EE
    JSR generate_number_sprites
    STZ $0AFA
    LDA #$000F
    STA $24
}

code_02E63D {
    JSR generate_number_sprites
    PHX 
    PHD 
    LDA $2C
    TAX 
    TCD 
    COP [A7]
    PLD 
    PLX 
    JMP $&code_02E544
}

code_02E6FC {
    JSR generate_number_sprites
    COP [06] ( #13 )
    JMP $&code_02E66B
}

;Enable direct sprites 
code_02EB22 {
    LDA #$1000
    TSB $06EE

    LDA #$1000
    TSB $10
    LDA #$000F
    STA $0000
    LDA $7F0010, X
    TAY 
}

;Disable direct sprites for status menu
code_02EB4D {
    LDA #$1000
    TRB $06EE

    LDA #$1000
    TRB $10
    LDA #$000F
    STA $0000
    LDA $7F0010, X
    TAY 
}


---------------------------------------------
;Cursor positions

binary_02EAB0 [
  #$004E   ;00
  #$0030   ;01
  #$0066   ;02
  #$0030   ;03
  #$007E   ;04
  #$0030   ;05
  #$0096   ;06
  #$0030   ;07
]

------------------------------------------------
?INCLUDE 'inventory_spritemap'
------------------------------------------------
;H-Mirror on the cursor

sprite_group_108BAA [
  sprite_group < #01, #0F, #18, #00, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #01, [
    sprite_part < #01, #00, #00, #00, #08, #$46E5 >
  ] >
]

------------------------------------------------
?INCLUDE 'chunk_03BAE1'
------------------------------------------------
;Enable static sprites for scene FF with a different address and size

code_03C7F6 {
    LDA $scene_current
    CMP #$00FF
    BNE direct_sprite_large
    
    PEA #$0000
    BRA scene_check_next

  direct_sprite_large:
    PEA #$00AA

  scene_check_next:
    CMP #$00FE
    BEQ scene_addr_large
    CMP #$0090
    BCS scene_addr_small
    CMP #$008C
    BCS scene_addr_large
    BRA scene_addr_small
    
  scene_addr_large:
    LDX #$0600
    BRA scene_addr_next

  scene_addr_small:
    LDX #$1FC0

  scene_addr_next:
    LDY #$0422
    LDA $00DA
    BIT #$FE00
    BEQ code_03C807
    LDA #$0200
}


code_03C807 {
    DEC 
    PHB 
    MVN #$00, #$7F
    PLB 

    LDY $00DA
    TYA
    LSR 
    LSR 
    LSR 
    LSR 
    STA $0E
    
    TYA
    LSR
    AND #$0006
    TAX

    SEP #$20

  code_03C821:
    DEC $0E
    BMI code_03C82D
    LDA $01, S
    STA ($06)
    INC $06
    BRA code_03C821
}


code_03C82D {
    LDA $01, S
    BEQ small_sprite_process
    LDA $@binary_03C841, X
    STA $00
    BRA small_sprite_continue

  small_sprite_process:
    STZ $00

  small_sprite_continue:
    LDA $@binary_03C841+1, X
    STA $0E
    REP #$20
    PLA
    RTS 
}
