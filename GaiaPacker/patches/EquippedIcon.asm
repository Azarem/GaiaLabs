
!player_flags                   09AE
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

func_03DECD {
    LDA $09ED
    BIT #$40
    BEQ code_03DED5
    STZ $00DA
    RTL
}

--------------------------------------------------------
?INCLUDE 'palette_bundles'
--------------------------------------------------------

;Aura barrier flame
bundle_16C1BD [
  bundle < #01, &word_16C1EE, #F1, #0F, #01 >   ;00
  bundle < #01, &word_16C1FE, #F1, #0F, #01 >   ;01
  bundle < #01, &word_16C20E, #F1, #0F, #01 >   ;02
  bundle < #01, &word_16C21E, #F1, #0F, #01 >   ;03
  bundle < #01, &word_16C22E, #F1, #0F, #01 >   ;04
  bundle < #01, &word_16C23E, #F1, #0F, #01 >   ;05
  bundle < #01, &word_16C24E, #F1, #0F, #01 >   ;06
  bundle < #01, &word_16C25E, #F1, #0F, #01 >   ;07
]

//Firebird
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


------------------------------------------------------
?INCLUDE 'sp58_actor_068111'
------------------------------------------------------
;Force flying scene  flag 4000

h_sp58_actor_068111 [
  h_actor < #00, #02, #18 >
    LDA #$4000
    TSB $09EC
    COP [4F] ( $7F0200, #$7800, #$0100 ) ;For some reason there is a situation where the BG3 data isn't refreshed
]

------------------------------------------------------
?INCLUDE 's59_actor_03A0AA'
------------------------------------------------------
;Force plane crash scene flag 4000

h_s59_actor_03A0AA [
  h_actor < #00, #00, #18 >
    LDA #$4000
    TSB $09EC
]

------------------------------------------------------
?INCLUDE 'btDC_plane_jumping'
------------------------------------------------------
;Force babel flying scene flag 4000

h_btDC_plane_jumping [
  h_actor < #06, #00, #18 >
    LDA #$4000
    TSB $09EC
    COP [4F] ( $7F0200, #$7800, #$0100 ) ;For some reason there is a situation where the BG3 data isn't refreshed
]

------------------------------------------------------
?INCLUDE 'actor_02B7B3'
------------------------------------------------------
;Fix for copied palette during aura barrier

code_02B9C0 {
    COP [50] ( @fx_palette_198090, #00, #F9, #07 ) ; <-
    COP [8E] ( #06 )
    COP [80] ( #02 )
    COP [89]
    COP [A5] ( @func_02C322, #00, #00, #$2400 )
    TYA 
    STA $7F0012, X
    COP [84] ( #03, #02 )
    COP [8A]
    LDA #$0001
    TRB $player_flags
    COP [A5] ( @code_02BA17, #00, #F0, #$2600 )
    COP [84] ( #03, #0A )
    COP [8A]
    JSR $&sub_02C21C
}

;Fix for copied palette during dark friar
code_02BB77 {
    COP [50] ( @fx_palette_198070, #00, #F0, #10 )
    COP [3B] ( #4A, @func_00B519 )
    COP [48]
    AND #$0003
    STA $0000
    COP [D9] ( #$0000, &code_list_02BB93 )
}

------------------------------------------------------
?INCLUDE 'func_02ED02'
------------------------------------------------------

;Fix for copied palette during dark friar
code_02EED2 {
    DEC 
    BNE code_02EEF0
    LDX #$4400
    STX $VMADDL
    LDX #$&misc_fx_1CC000
    LDA #$^misc_fx_1CC000
    LDY #$0480
    JSL $@func_0283A2
    COP [50] ( @fx_palette_198070, #00, #F0, #10 )
    RTS 
}

;Fix for copied palette during aura barrier
code_02EEF0 {
    LDX #$4400
    STX $VMADDL
    LDX #$&misc_fx_1CC480
    LDA #$^misc_fx_1CC480
    LDY #$0600
    JSL $@func_0283A2
    COP [50] ( @fx_palette_198090, #00, #F9, #07 )
    RTS 
}


------------------------------------------------------
?INCLUDE 'table_17A000'
------------------------------------------------------
;Fix for sword palette during aura barrier

sprite_group_17A483 [
  sprite_group < #13, #10, #46, #00, #F8, #F0, #01, #01, #F8, #10, #F0, #10, #0E, [
    sprite_part < #01, #0B, #08, #00, #36, #$0E4E >   ;00
    sprite_part < #01, #0B, #08, #10, #26, #$0E6E >   ;01
    sprite_part < #01, #03, #10, #20, #16, #$0088 >   ;02
    sprite_part < #00, #04, #17, #1E, #20, #$0043 >   ;03
    sprite_part < #01, #13, #00, #20, #16, #$008A >   ;04
    sprite_part < #00, #1A, #01, #1E, #20, #$4043 >   ;05
    sprite_part < #01, #0B, #08, #30, #06, #$0084 >   ;06
    sprite_part < #01, #0B, #08, #36, #00, #$0086 >   ;07
    sprite_part < #00, #0E, #0D, #39, #05, #$006A >   ;08
    sprite_part < #01, #0B, #08, #1A, #1C, #$0000 >   ;09
    sprite_part < #01, #00, #13, #26, #10, #$0068 >   ;0A
    sprite_part < #01, #13, #00, #27, #0F, #$006A >   ;0B
    sprite_part < #01, #00, #13, #35, #01, #$0027 >   ;0C
    sprite_part < #01, #13, #00, #35, #01, #$0029 >   ;0D
  ] >   ;00
]

