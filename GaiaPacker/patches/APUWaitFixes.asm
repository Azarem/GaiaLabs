

!joypad_mask_std                065A
!player_actor                   09AA
!APUIO1                         2141


-----------------------------------------
?INCLUDE 'chunk_028000'
-----------------------------------------

code_02A07D {
    COP [9C] ( @func_02A0E5, #$2000 )
    LDA $20
    STA $0020, Y
    LDA $22
    STA $0022, Y
    LDA $0012, Y
    ORA #$1000
    STA $0012, Y
    
    LDA $0D73
    AND #$00FF
    BEQ chunk_halt_test

    LDA $0D72
    AND #$00FF
    CMP #$001B
    BNE chunk_halt_test

    COP [C1]
    SEP #$20
    LDA $2000
    AND #$10
    REP #$20
    BEQ code_02A0A9
    RTL

  chunk_halt_test:
    COP [C1]
    SEP #$20
    LDA $APUIO1
    REP #$20
    AND #$00FF
    CMP #$00FF
    BEQ code_02A0A9
    RTL 
}

-----------------------------------------

code_02A813 {
    COP [9C] ( @func_02A893, #$2000 )
    LDA $24
    STA $0024, Y
    LDA $0012, Y
    ORA #$1000
    STA $0012, Y
    LDA $20
    STA $0020, Y
    
    LDA $0D73
    AND #$00FF
    BEQ chunk_halt_test2

    LDA $0D72
    AND #$00FF
    CMP #$001B
    BNE chunk_halt_test2

    COP [C1]
    SEP #$20
    LDA $2000
    AND #$10
    REP #$20
    BEQ code_02A83F
    RTL

  chunk_halt_test2:
    COP [C1]
    SEP #$20
    LDA $APUIO1
    REP #$20
    AND #$00FF
    CMP #$00FF
    BEQ code_02A83F
    RTL 
}

-----------------------------------------
?INCLUDE 'chunk_038000'
-----------------------------------------

code_03A029 {
    LDA $0D73
    AND #$00FF
    BEQ chunk_halt_test3

    LDA $0D72
    AND #$00FF
    CMP #$001B
    BNE chunk_halt_test3

    COP [C1]
    SEP #$20
    LDA $2000
    AND #$10
    REP #$20
    BEQ code_03A03B
    RTL

  chunk_halt_test3:
    COP [C1]
    SEP #$20
    LDA $APUIO1
    REP #$20
    AND #$00FF
    CMP #$00FF
    BEQ code_03A03B
    RTL 
}

-----------------------------------------
?INCLUDE 'sc06_lola'
-----------------------------------------

code_049985 {
    LDA #$FFF0
    TSB $joypad_mask_std
    COP [CC] ( #35 )
    COP [04] ( #19 )
    COP [DA] ( #59 )
    COP [BF] ( &widestring_049D29 )

    LDA $0D73
    AND #$00FF
    BEQ lola_halt_test

    LDA $0D72
    AND #$00FF
    CMP #$001B
    BNE lola_halt_test

    COP [C1]
    SEP #$20
    LDA $2000
    AND #$10
    REP #$20
    BEQ code_0499AA
    RTL

  lola_halt_test:
    COP [C1]
    SEP #$20
    LDA $APUIO1
    REP #$20
    AND #$00FF
    CMP #$00FF
    BEQ code_0499AA
    RTL 
}

-----------------------------------------
?INCLUDE 'dm47_sam'
-----------------------------------------

code_05D24B {
    COP [C0] ( &code_05D29E )
    COP [D2] ( #5E, #01 )
    LDA #$FFF0
    TSB $joypad_mask_std
    COP [04] ( #1E )
    COP [DA] ( #77 )
    
    LDA $0D73
    AND #$00FF
    BEQ sam_halt_test

    LDA $0D72
    AND #$00FF
    CMP #$001B
    BNE sam_halt_test

    COP [C1]
    SEP #$20
    LDA $2000
    AND #$10
    REP #$20
    BEQ code_05D271
    RTL

  sam_halt_test:
    COP [C1]
    SEP #$20
    LDA $APUIO1
    REP #$20
    AND #$00FF
    CMP #$00FF
    BEQ code_05D271
    RTL
}

-----------------------------------------
?INCLUDE 'sp5C_stone_coffin'
-----------------------------------------

code_0691BE {
    COP [D2] ( #02, #01 )
    LDA #$CFF0
    TSB $joypad_mask_std
    LDA #$2000
    TRB $10
    COP [88] ( @table_0EE000 )
    LDY $player_actor
    LDA $0014, Y
    STA $14
    LDA $0016, Y
    SEC 
    SBC #$0010
    STA $16
    COP [85] ( #33, #04, #14 )
    COP [8A]
    COP [84] ( #33, #04 )
    COP [8A]
    LDA #$02C0
    STA $7F0018, X
    LDA #$009C
    STA $7F001A, X
    COP [22] ( #33, #01 )
    LDA #$2000
    TSB $10
    COP [DA] ( #3B )
    COP [32] ( #3B )
    COP [33]
    COP [CD] ( #$013B )
    COP [DA] ( #3B )
    LDA #$02C0
    STA $14
    LDA #$00A0
    STA $16
    LDA #$2000
    TRB $10
    JSL $@func_02A10A
    BCS coffin_halt_jump
    COP [D4] ( #11, &code_069293 )
    COP [BF] ( &widestring_069377 )
    LDA #$0080
    TSB $09EC
    COP [19] ( #17, @widestring_06939E )
    COP [DA] ( #03 )
    
    LDA $0D73
    AND #$00FF
    BEQ coffin_halt_test

    LDA $0D72
    AND #$00FF
    CMP #$001B
    BNE coffin_halt_test

    COP [C1]
    LDA #$CFF0
    TSB $joypad_mask_std
    SEP #$20
    LDA $2000
    AND #$10
    REP #$20
    BEQ code_06925B
    RTL

  coffin_halt_test:
    COP [C1]
    LDA #$CFF0
    TSB $joypad_mask_std
    SEP #$20
    LDA $APUIO1
    REP #$20
    AND #$00FF
    CMP #$00FF
    BEQ code_06925B
    RTL 

  coffin_halt_jump:
    JMP code_069297
}

-----------------------------------------
?INCLUDE 'sE6_gaia'
-----------------------------------------

code_08DCAF {
    COP [D6] ( #24, &code_08DCEC )
    COP [D4] ( #24, &code_08DCF3 )
    COP [BF] ( &widestring_08E66C )
    LDA #$FFF0
    TSB $joypad_mask_std
    COP [04] ( #18 )
    COP [DA] ( #59 )
    COP [BF] ( &widestring_08E7E7 )
    
    LDA $0D73
    AND #$00FF
    BEQ gaia_halt_test

    LDA $0D72
    AND #$00FF
    CMP #$001B
    BNE gaia_halt_test

    COP [C1]
    SEP #$20
    LDA $2000
    AND #$10
    REP #$20
    BEQ code_08DCDF
    RTL

  gaia_halt_test:
    COP [C1]
    SEP #$20
    LDA $APUIO1
    REP #$20
    AND #$00FF
    CMP #$00FF
    BEQ code_08DCDF
    RTL 
}

---------------------------------------------

code_08EAAA {
    COP [CA] ( #08 )
    LDA #$FFF0
    TSB $joypad_mask_std
    COP [C1]
    COP [8B]
    COP [CB]
    LDA $24
    ORA $0AA2
    STA $0AA2
    COP [86] ( #0A, #03, #14 )
    COP [8A]
    COP [84] ( #0A, #03 )
    COP [8A]
    LDY $player_actor
    LDA $0014, Y
    STA $7F0018, X
    LDA $0016, Y
    SEC 
    SBC #$0010
    STA $7F001A, X
    COP [22] ( #0A, #01 )
    LDA #$2000
    TSB $10
    LDA #$0800
    TRB $10
    COP [04] ( #18 )
    COP [DA] ( #59 )
    COP [BF] ( &widestring_08EB68 )

    LDA $0D73
    AND #$00FF
    BEQ gaia_halt_test2

    LDA $0D72
    AND #$00FF
    CMP #$001B
    BNE gaia_halt_test2

    COP [C1]
    SEP #$20
    LDA $2000
    AND #$10
    REP #$20
    BEQ code_08EB0D
    RTL

  gaia_halt_test2:
    COP [C1]
    SEP #$20
    LDA $APUIO1
    REP #$20
    AND #$00FF
    CMP #$00FF
    BEQ code_08EB0D
    RTL
}

-----------------------------------------

code_08EF4E {
    LDA #$FFF0
    TSB $joypad_mask_std
    COP [DA] ( #05 )
    COP [D4] ( #24, &code_08EFC9 )
    COP [86] ( #0A, #03, #14 )
    COP [8A]
    COP [84] ( #0A, #03 )
    COP [8A]
    LDY $player_actor
    LDA $0014, Y
    STA $7F0018, X
    LDA $0016, Y
    SEC 
    SBC #$0010
    STA $7F001A, X
    COP [22] ( #0A, #01 )
    LDA #$2000
    TSB $10
    LDA #$0800
    TRB $10
    LDA #$0080
    TSB $09EC
    COP [19] ( #17, @widestring_08EFEF )
    COP [DA] ( #03 )
    
    LDA $0D73
    AND #$00FF
    BEQ gaia_halt_test3

    LDA $0D72
    AND #$00FF
    CMP #$001B
    BNE gaia_halt_test3

    COP [C1]
    SEP #$20
    LDA $2000
    AND #$10
    REP #$20
    BEQ code_08EFAD
    RTL

  gaia_halt_test3:
    COP [C1]
    SEP #$20
    LDA $APUIO1
    REP #$20
    AND #$00FF
    CMP #$00FF
    BEQ code_08EFAD
    RTL
}

------------------------------------------
?INCLUDE 'ir1D_wind_melody'
------------------------------------------

code_09C58D {
    LDA #$EFF0
    TSB $joypad_mask_std
    COP [DA] ( #1D )
    COP [04] ( #1B )
    COP [DA] ( #77 )
    COP [07] ( #16 )
    COP [BF] ( &widestring_09C5D5 )
    COP [04] ( #1A )
    COP [DA] ( #77 )
    
    LDA $0D73
    AND #$00FF
    BEQ wind_halt_test

    LDA $0D72
    AND #$00FF
    CMP #$001B
    BNE wind_halt_test

    COP [C1]
    SEP #$20
    LDA $2000
    AND #$10
    REP #$20
    BEQ code_09C5BB
    RTL

  wind_halt_test:
    COP [C1]
    SEP #$20
    LDA $APUIO1
    REP #$20
    AND #$00FF
    CMP #$00FF
    BEQ code_09C5BB
    RTL
}