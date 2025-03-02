
!inventory_slots                0AB4
!inventory_equipped_index       0AC4
!inventory_equipped_type        0AC6
!VMADDL                         2116
!CGADD                          2121
!MDMAEN                         420B
!DMAP0                          4300
!A1T0L                          4302
!DAS0L                          4305


?INCLUDE 'chunk_028000'
--------------------------------------------

item_icon_generate {
    PHP
  item_icon_stub:
    REP #$20
    LDA $09EC
    BIT #$4000
    BNE item_icon_quit
    LDA $0AC6
    ;STA $09FA
    ASL
    TAX
    SEP #$20
    JMP ($&item_icon_list, X)
}

item_icon_quit {
    PLP
    RTL
}

item_icon_list [
    &item_icon_nothing
    &item_icon_jewel
    &item_icon_prison_key
    &item_icon_inca_statue_a
    &item_icon_inca_statue_b
    &item_icon_inca_melody
    &item_icon_herb
    &item_icon_diamond_block
    &item_icon_wind_melody
    &item_icon_lolas_melody
    &item_icon_roast
    &item_icon_mine_key_a
    &item_icon_mine_key_b
    &item_icon_memory_melody
    &item_icon_crystal
    &item_icon_elevator_key
    &item_icon_palace_key
    &item_icon_purification_stone
    &item_icon_hope_statue
    &item_icon_rama_statue
    &item_icon_magic_dust
    &item_icon_blue_journal
    &item_icon_lances_letter
    &item_icon_necklace_stones
    &item_icon_will
    &item_icon_tearpot
    &item_icon_mushroom_drops
    &item_icon_gold_bag
    &item_icon_glasses
    &item_icon_gorgon_flower
    &item_icon_hieroglyph_1
    &item_icon_hieroglyph_2
    &item_icon_hieroglyph_3
    &item_icon_hieroglyph_4
    &item_icon_hieroglyph_5
    &item_icon_hieroglyph_6
    &item_icon_aura
    &item_icon_lolas_letter
    &item_icon_fathers_journal
    &item_icon_crystal_ring
    &item_icon_apple
]


item_icon_nothing {
    PEA #$&pal_item_exprite
    PEA #$&gfx_item_exprite+202
    LDX #$&gfx_item_exprite+2
    JMP item_icon_next
}

item_icon_jewel {
    PEA #$&pal_item_exprite
    PEA #$&gfx_item_exprite+242
    LDX #$&gfx_item_exprite+42
    JMP item_icon_next
}

item_icon_prison_key {
    PEA #$&pal_item_exprite+40
    PEA #$&gfx_item_exprite+282
    LDX #$&gfx_item_exprite+82
    JMP item_icon_next
}

item_icon_inca_statue_a {
    PEA #$&pal_item_exprite+A0
    PEA #$&gfx_item_exprite+2C2
    LDX #$&gfx_item_exprite+C2
    JMP item_icon_next
}

item_icon_inca_statue_b {
    PEA #$&pal_item_exprite+A0
    PEA #$&gfx_item_exprite+302
    LDX #$&gfx_item_exprite+102
    JMP item_icon_next
}

item_icon_inca_melody {
    PEA #$&pal_item_exprite+1C0
    PEA #$&gfx_item_exprite+342
    LDX #$&gfx_item_exprite+142
    JMP item_icon_next
}

item_icon_lolas_melody {
    PEA #$&pal_item_exprite
    PEA #$&gfx_item_exprite+342
    LDX #$&gfx_item_exprite+142
    JMP item_icon_next
}

item_icon_wind_melody {
    PEA #$&pal_item_exprite+20
    PEA #$&gfx_item_exprite+342
    LDX #$&gfx_item_exprite+142
    JMP item_icon_next
}

item_icon_memory_melody {
    PEA #$&pal_item_exprite+C0
    PEA #$&gfx_item_exprite+342
    LDX #$&gfx_item_exprite+142
    JMP item_icon_next
}

item_icon_herb {
    PEA #$&pal_item_exprite+20
    PEA #$&gfx_item_exprite+382
    LDX #$&gfx_item_exprite+182
    JMP item_icon_next
}

item_icon_diamond_block {
    PEA #$&pal_item_exprite+80
    PEA #$&gfx_item_exprite+3C2
    LDX #$&gfx_item_exprite+1C2
    JMP item_icon_next
}

item_icon_roast {
    PEA #$&pal_item_exprite+E0
    PEA #$&gfx_item_exprite+602
    LDX #$&gfx_item_exprite+402
    JMP item_icon_next
}

item_icon_mine_key_a {
    PEA #$&pal_item_exprite+A0
    PEA #$&gfx_item_exprite+642
    LDX #$&gfx_item_exprite+442
    JMP item_icon_next
}

item_icon_mine_key_b {
    PEA #$&pal_item_exprite+A0
    PEA #$&gfx_item_exprite+682
    LDX #$&gfx_item_exprite+482
    JMP item_icon_next
}

item_icon_crystal {
    PEA #$&pal_item_exprite+60
    PEA #$&gfx_item_exprite+6C2
    LDX #$&gfx_item_exprite+4C2
    JMP item_icon_next
}

item_icon_elevator_key {
    PEA #$&pal_item_exprite+100
    PEA #$&gfx_item_exprite+702
    LDX #$&gfx_item_exprite+502
    JMP item_icon_next
}

item_icon_palace_key {
    PEA #$&pal_item_exprite+80
    PEA #$&gfx_item_exprite+742
    LDX #$&gfx_item_exprite+542
    JMP item_icon_next
}

item_icon_purification_stone {
    PEA #$&pal_item_exprite+C0
    PEA #$&gfx_item_exprite+782
    LDX #$&gfx_item_exprite+582
    JMP item_icon_next
}

item_icon_hope_statue {
    PEA #$&pal_item_exprite+40
    PEA #$&gfx_item_exprite+7C2
    LDX #$&gfx_item_exprite+5C2
    JMP item_icon_next
}

item_icon_rama_statue {
    PEA #$&pal_item_exprite+60
    PEA #$&gfx_item_exprite+A02
    LDX #$&gfx_item_exprite+802
    JMP item_icon_next
}

item_icon_magic_dust {
    PEA #$&pal_item_exprite+120
    PEA #$&gfx_item_exprite+A42
    LDX #$&gfx_item_exprite+842
    JMP item_icon_next
}

item_icon_blue_journal {
    PEA #$&pal_item_exprite+140
    PEA #$&gfx_item_exprite+A82
    LDX #$&gfx_item_exprite+882
    JMP item_icon_next
}

item_icon_lances_letter {
    PEA #$&pal_item_exprite+160
    PEA #$&gfx_item_exprite+AC2
    LDX #$&gfx_item_exprite+8C2
    JMP item_icon_next
}

item_icon_necklace_stones {
    PEA #$&pal_item_exprite+A0
    PEA #$&gfx_item_exprite+B02
    LDX #$&gfx_item_exprite+902
    JMP item_icon_next
}

item_icon_will {
    PEA #$&pal_item_exprite+160
    PEA #$&gfx_item_exprite+B42
    LDX #$&gfx_item_exprite+942
    JMP item_icon_next
}

item_icon_tearpot {
    PEA #$&pal_item_exprite+80
    PEA #$&gfx_item_exprite+B82
    LDX #$&gfx_item_exprite+982
    JMP item_icon_next
}

item_icon_mushroom_drops {
    PEA #$&pal_item_exprite+180
    PEA #$&gfx_item_exprite+BC2
    LDX #$&gfx_item_exprite+9C2
    JMP item_icon_next
}

item_icon_gold_bag {
    PEA #$&pal_item_exprite+1A0
    PEA #$&gfx_item_exprite+E02
    LDX #$&gfx_item_exprite+C02
    JMP item_icon_next
}

item_icon_glasses {
    PEA #$&pal_item_exprite+1C0
    PEA #$&gfx_item_exprite+E42
    LDX #$&gfx_item_exprite+C42
    JMP item_icon_next
}

item_icon_gorgon_flower {
    PEA #$&pal_item_exprite+1E0
    PEA #$&gfx_item_exprite+E82
    LDX #$&gfx_item_exprite+C82
    JMP item_icon_next
}

item_icon_hieroglyph_1 {
    PEA #$&pal_item_exprite+120
    PEA #$&gfx_item_exprite+EC2
    LDX #$&gfx_item_exprite+CC2
    JMP item_icon_next
}

item_icon_hieroglyph_2 {
    PEA #$&pal_item_exprite+120
    PEA #$&gfx_item_exprite+F02
    LDX #$&gfx_item_exprite+D02
    JMP item_icon_next
}

item_icon_hieroglyph_3 {
    PEA #$&pal_item_exprite+120
    PEA #$&gfx_item_exprite+F42
    LDX #$&gfx_item_exprite+D42
    JMP item_icon_next
}

item_icon_hieroglyph_4 {
    PEA #$&pal_item_exprite+120
    PEA #$&gfx_item_exprite+F82
    LDX #$&gfx_item_exprite+D82
    JMP item_icon_next
}

item_icon_hieroglyph_5 {
    PEA #$&pal_item_exprite+120
    PEA #$&gfx_item_exprite+FC2
    LDX #$&gfx_item_exprite+DC2
    JMP item_icon_next
}

item_icon_hieroglyph_6 {
    PEA #$&pal_item_exprite+120
    PEA #$&gfx_item_exprite+1202
    LDX #$&gfx_item_exprite+1002
    JMP item_icon_next
}

item_icon_aura {
    PEA #$&pal_item_exprite+80
    PEA #$&gfx_item_exprite+1242
    LDX #$&gfx_item_exprite+1042
    JMP item_icon_next
}

item_icon_lolas_letter {
    PEA #$&pal_item_exprite+120
    PEA #$&gfx_item_exprite+1282
    LDX #$&gfx_item_exprite+1082
    JMP item_icon_next
}

item_icon_fathers_journal {
    PEA #$&pal_item_exprite2
    PEA #$&gfx_item_exprite+12C2
    LDX #$&gfx_item_exprite+10C2
    JMP item_icon_next
}

item_icon_crystal_ring {
    PEA #$&pal_item_exprite2+40
    PEA #$&gfx_item_exprite+1302
    LDX #$&gfx_item_exprite+1102
    JMP item_icon_next
}

item_icon_apple {
    PEA #$&pal_item_exprite2+20
    PEA #$&gfx_item_exprite+1342
    LDX #$&gfx_item_exprite+1142
    JMP item_icon_next
}

item_icon_next {
    LDY #$4EC0
    STY $VMADDL
    LDY #$0040
    LDA #$^gfx_item_exprite
    JSL $@func_0283A2
    PLX
    LDY #$4FC0
    STY $VMADDL
    LDY #$0040
    LDA #$^gfx_item_exprite
    JSL $@func_0283A2
    
    REP #$20
    LDA #$1000
    TSB $06EE
    LDA #$0004
    STA $00DA
    LDA #$0378
    STA $7F1FC0
    LDA #$34EC
    STA $7F1FC2

    PLX
    PHX
    LDY #$0B40
    LDA #$001F
    PHB
    MVN #$7F, #$^pal_item_exprite
    PLB
    
    SEP #$20
    
    LDA #$A0
    STA $CGADD
    STZ $DMAP0
    LDA #$22
    STA $BBAD0
    PLX
    STX $A1T0L
    LDA #$^pal_item_exprite
    STA $A1B0
    LDX #$0020
    STX $DAS0L
    LDA #$01
    STA $MDMAEN

    PLP 
    RTL
}

---------------------------------
?INCLUDE 'chunk_03BAE1'
--------------------------------------------------
;Allow both static sprites and temporary sprites

func_03C78B {
    LDX #$0000
    TXY 

    LDA $06EE
    BIT #$1000
    BEQ code_03C797

    JSR code_03C7F0
    LDX #$0000
    
  code_03C797:
    LDA $7F3100, X
    BPL code_03C79E
    RTS 
}

--------------------------------------------------------
;Make system sprite page loading also load item icon

code_03E04E {
    JML item_icon_stub
}

--------------------------------------------------------
?INCLUDE 'palette_bundles'
--------------------------------------------------------

bundle_16CE8F [
  bundle < #01, &word_16CEC0, #98, #0F, #02 >   ;00
  bundle < #01, &word_16CED0, #98, #0F, #02 >   ;01
  bundle < #01, &word_16CEE0, #98, #0F, #02 >   ;02
  bundle < #01, &word_16CEF0, #98, #0F, #02 >   ;03
  bundle < #01, &word_16CF00, #98, #0F, #02 >   ;04
  bundle < #01, &word_16CF10, #98, #0F, #02 >   ;05
  bundle < #01, &word_16CF20, #98, #0F, #02 >   ;06
  bundle < #01, &word_16CF30, #98, #0F, #02 >   ;07
]

--------------------------------------------------------
?INCLUDE 'table_17D000'
--------------------------------------------------------

sprite_group_17D060 [
  sprite_group < #17, #0F, #17, #0C, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #09, [
    sprite_part < #01, #16, #00, #13, #00, #$4244 >   ;00
    sprite_part < #01, #06, #10, #13, #00, #$4246 >   ;01
    sprite_part < #00, #00, #1E, #06, #15, #$4263 >   ;02
    sprite_part < #00, #00, #1E, #0E, #0D, #$4264 >   ;03
    sprite_part < #01, #06, #10, #06, #0D, #$4242 >   ;04
    sprite_part < #01, #13, #03, #06, #0D, #$4240 >   ;05
    sprite_part < #00, #0E, #10, #00, #1B, #$4261 >   ;06
    sprite_part < #00, #06, #18, #00, #1B, #$4262 >   ;07
    sprite_part < #00, #16, #08, #00, #1B, #$4260 >   ;08
  ] >   ;00
]

sprite_group_17D0AC [
  sprite_group < #16, #11, #18, #0D, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #09, [
    sprite_part < #01, #17, #00, #15, #00, #$4244 >   ;00
    sprite_part < #01, #07, #10, #15, #00, #$4246 >   ;01
    sprite_part < #00, #00, #1F, #07, #16, #$4263 >   ;02
    sprite_part < #00, #00, #1F, #0F, #0E, #$4264 >   ;03
    sprite_part < #01, #07, #10, #07, #0E, #$4242 >   ;04
    sprite_part < #01, #15, #02, #07, #0E, #$4240 >   ;05
    sprite_part < #00, #0F, #10, #00, #1D, #$4261 >   ;06
    sprite_part < #00, #07, #18, #00, #1D, #$4262 >   ;07
    sprite_part < #00, #17, #08, #00, #1D, #$4260 >   ;08
  ] >   ;00
]

sprite_group_17D0F8 [
  sprite_group < #15, #13, #1A, #0F, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #09, [
    sprite_part < #01, #18, #00, #19, #00, #$4244 >   ;00
    sprite_part < #01, #08, #10, #19, #00, #$4246 >   ;01
    sprite_part < #00, #00, #20, #09, #18, #$4263 >   ;02
    sprite_part < #00, #00, #20, #11, #10, #$4264 >   ;03
    sprite_part < #01, #08, #10, #09, #10, #$4242 >   ;04
    sprite_part < #01, #18, #00, #09, #10, #$4240 >   ;05
    sprite_part < #00, #10, #10, #01, #20, #$4261 >   ;06
    sprite_part < #00, #08, #18, #01, #20, #$4262 >   ;07
    sprite_part < #00, #18, #08, #00, #21, #$4260 >   ;08
  ] >   ;00
]

sprite_group_17D144 [
  sprite_group < #0F, #17, #17, #0C, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #09, [
    sprite_part < #01, #00, #16, #13, #00, #$0244 >   ;00
    sprite_part < #01, #10, #06, #13, #00, #$0246 >   ;01
    sprite_part < #00, #1E, #00, #06, #15, #$0263 >   ;02
    sprite_part < #00, #1E, #00, #0E, #0D, #$0264 >   ;03
    sprite_part < #01, #10, #06, #06, #0D, #$0242 >   ;04
    sprite_part < #01, #03, #13, #06, #0D, #$0240 >   ;05
    sprite_part < #00, #10, #0E, #00, #1B, #$0261 >   ;06
    sprite_part < #00, #18, #06, #00, #1B, #$0262 >   ;07
    sprite_part < #00, #08, #16, #00, #1B, #$0260 >   ;08
  ] >   ;00
]

sprite_group_17D190 [
  sprite_group < #11, #16, #18, #0D, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #09, [
    sprite_part < #01, #00, #17, #15, #00, #$0244 >   ;00
    sprite_part < #01, #10, #07, #15, #00, #$0246 >   ;01
    sprite_part < #00, #1F, #00, #07, #16, #$0263 >   ;02
    sprite_part < #00, #1F, #00, #0F, #0E, #$0264 >   ;03
    sprite_part < #01, #10, #07, #07, #0E, #$0242 >   ;04
    sprite_part < #01, #02, #15, #07, #0E, #$0240 >   ;05
    sprite_part < #00, #10, #0F, #00, #1D, #$0261 >   ;06
    sprite_part < #00, #18, #07, #00, #1D, #$0262 >   ;07
    sprite_part < #00, #08, #17, #00, #1D, #$0260 >   ;08
  ] >   ;00
]

sprite_group_17D1DC [
  sprite_group < #13, #15, #1A, #0F, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #09, [
    sprite_part < #01, #00, #18, #19, #00, #$0244 >   ;00
    sprite_part < #01, #10, #08, #19, #00, #$0246 >   ;01
    sprite_part < #00, #20, #00, #09, #18, #$0263 >   ;02
    sprite_part < #00, #20, #00, #11, #10, #$0264 >   ;03
    sprite_part < #01, #10, #08, #09, #10, #$0242 >   ;04
    sprite_part < #01, #00, #18, #09, #10, #$0240 >   ;05
    sprite_part < #00, #10, #10, #01, #20, #$0261 >   ;06
    sprite_part < #00, #18, #08, #01, #20, #$0262 >   ;07
    sprite_part < #00, #08, #18, #00, #21, #$0260 >   ;08
  ] >   ;00
]

sprite_group_17D228 [
  sprite_group < #15, #15, #18, #12, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #09, [
    sprite_part < #01, #0D, #0D, #00, #1A, #$0248 >   ;00
    sprite_part < #01, #0D, #0D, #0D, #0D, #$024A >   ;01
    sprite_part < #01, #0D, #0D, #1A, #00, #$024C >   ;02
    sprite_part < #01, #1A, #00, #0D, #0D, #$024E >   ;03
    sprite_part < #01, #00, #1A, #0D, #0D, #$424E >   ;04
    sprite_part < #00, #1A, #08, #05, #1D, #$0268 >   ;05
    sprite_part < #00, #21, #01, #05, #1D, #$0269 >   ;06
    sprite_part < #00, #01, #21, #05, #1D, #$4269 >   ;07
    sprite_part < #00, #08, #1A, #05, #1D, #$4268 >   ;08
  ] >   ;00
]

sprite_group_17D274 [
  sprite_group < #16, #16, #1B, #11, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #09, [
    sprite_part < #01, #0E, #0E, #00, #1C, #$0248 >   ;00
    sprite_part < #01, #0E, #0E, #0E, #0E, #$024A >   ;01
    sprite_part < #01, #0E, #0E, #1C, #00, #$024C >   ;02
    sprite_part < #01, #1C, #00, #0E, #0E, #$024E >   ;03
    sprite_part < #01, #00, #1C, #0E, #0E, #$424E >   ;04
    sprite_part < #00, #1C, #08, #06, #1E, #$0268 >   ;05
    sprite_part < #00, #24, #00, #06, #1E, #$0269 >   ;06
    sprite_part < #00, #00, #24, #06, #1E, #$4269 >   ;07
    sprite_part < #00, #08, #1C, #06, #1E, #$4268 >   ;08
  ] >   ;00
]

sprite_group_17D2C0 [
  sprite_group < #18, #18, #20, #10, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #09, [
    sprite_part < #01, #10, #10, #00, #20, #$0248 >   ;00
    sprite_part < #01, #10, #10, #10, #10, #$024A >   ;01
    sprite_part < #01, #10, #10, #20, #00, #$024C >   ;02
    sprite_part < #01, #20, #00, #10, #10, #$024E >   ;03
    sprite_part < #01, #00, #20, #10, #10, #$424E >   ;04
    sprite_part < #00, #20, #08, #08, #20, #$0268 >   ;05
    sprite_part < #00, #28, #00, #08, #20, #$0269 >   ;06
    sprite_part < #00, #00, #28, #08, #20, #$4269 >   ;07
    sprite_part < #00, #08, #20, #08, #20, #$4268 >   ;08
  ] >   ;00
]

sprite_group_17D30C [
  sprite_group < #15, #15, #12, #18, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #09, [
    sprite_part < #01, #0D, #0D, #1A, #00, #$8248 >   ;00
    sprite_part < #01, #0D, #0D, #0D, #0D, #$824A >   ;01
    sprite_part < #01, #0D, #0D, #00, #1A, #$824C >   ;02
    sprite_part < #01, #1A, #00, #0D, #0D, #$824E >   ;03
    sprite_part < #01, #00, #1A, #0D, #0D, #$C24E >   ;04
    sprite_part < #00, #1A, #08, #1D, #05, #$8268 >   ;05
    sprite_part < #00, #21, #01, #1D, #05, #$8269 >   ;06
    sprite_part < #00, #01, #21, #1D, #05, #$C269 >   ;07
    sprite_part < #00, #08, #1A, #1D, #05, #$C268 >   ;08
  ] >   ;00
]

sprite_group_17D358 [
  sprite_group < #16, #16, #11, #1B, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #09, [
    sprite_part < #01, #0E, #0E, #1C, #00, #$8248 >   ;00
    sprite_part < #01, #0E, #0E, #0E, #0E, #$824A >   ;01
    sprite_part < #01, #0E, #0E, #00, #1C, #$824C >   ;02
    sprite_part < #01, #1C, #00, #0E, #0E, #$824E >   ;03
    sprite_part < #01, #00, #1C, #0E, #0E, #$C24E >   ;04
    sprite_part < #00, #1C, #08, #1E, #06, #$8268 >   ;05
    sprite_part < #00, #24, #00, #1E, #06, #$8269 >   ;06
    sprite_part < #00, #00, #24, #1E, #06, #$C269 >   ;07
    sprite_part < #00, #08, #1C, #1E, #06, #$C268 >   ;08
  ] >   ;00
]

sprite_group_17D3A4 [
  sprite_group < #18, #18, #10, #20, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #09, [
    sprite_part < #01, #10, #10, #20, #00, #$8248 >   ;00
    sprite_part < #01, #10, #10, #10, #10, #$824A >   ;01
    sprite_part < #01, #10, #10, #00, #20, #$824C >   ;02
    sprite_part < #01, #20, #00, #10, #10, #$824E >   ;03
    sprite_part < #01, #00, #20, #10, #10, #$C24E >   ;04
    sprite_part < #00, #20, #08, #20, #08, #$8268 >   ;05
    sprite_part < #00, #28, #00, #20, #08, #$8269 >   ;06
    sprite_part < #00, #00, #28, #20, #08, #$C269 >   ;07
    sprite_part < #00, #08, #20, #20, #08, #$C268 >   ;08
  ] >   ;00
]

------------------------------------------------------
?INCLUDE 'table_018000'
------------------------------------------------------
;Enable engine flag 4000

unk7_01805E [
  unk7 < #17, #00, #80, #00, #24, #85, #09, #80, #00, #00 >   ;Statue reward scene
]

unk7_0181D0 [
  unk7 < #17, #00, #80, #03, #E4, #80, #09, #80, #00, #00 >   ;Epilogue scene
]
