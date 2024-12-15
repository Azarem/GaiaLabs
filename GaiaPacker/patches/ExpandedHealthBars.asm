?BANK 3

?INCLUDE 'chunk_03BAE1'

!joypad_mask_std                065A

---------------------------------------------
;Max enemy health is 255 (limitation on stats file storage)

player_damage_palette [
    #$7F78
    #$7923
    #$2460
]

enemy_damage_palette [
    #$5ADF
    #$0C3A
    #$042B

    #$575F
    #$05D9
    #$08A9

    #$47FF
    #$2AF7
    #$0108

    #$5FF7
    #$0B42
    #$0541

    #$7F57
    #$7E4D
    #$28A5

    #$7F3E
    #$69F9
    #$2007

    #$7FFF
    #$56B5
    #$2529
]

----------------------------------------------
;Hook for player health bar

cmd_0A_03EB1B {
    PHY 
    BRK #$13
    STZ $08
    LDA $0ACA
    STA $0AAE
    LDA $0ACE
    STA $0AD2
    
  cmd0A_hp_check:
    LDA $0AD2
    CMP #$0029
    BMI cmd0A_cycle_end
    SEC
    SBC #$0028
    STA $0AD2

    LDA $0AAE
    SEC
    SBC #$0028
    STA $0AAE
    
    INC $08
    BRA cmd0A_hp_check
    
  cmd0A_cycle_end:
    LDA $0AAE
    CMP #$0029
    BMI $06
    LDA #$0028
    STA $0AAE
    
    PHX
    LDA $08
    ASL
    CLC
    ADC $08
    ASL
    TAX
    LDA @player_damage_palette, X
    STA $7F0A12
    INX
    INX
    LDA @player_damage_palette, X
    STA $7F0A14
    INX
    INX
    LDA @player_damage_palette, X
    STA $7F0A16
    STZ $08
    PLX
}

code_03EB3A {
    LDA $0AD2
    LSR 
    STA $00
    BCC code_03EB44
    INC $08
}

code_03EB44 {
    ASL 
    CLC 
    ADC $08
    SEC 
    SBC $0AAE
    EOR #$FFFF
    INC 
    LSR 
    STA $02
    LDA $08
    CLC 
    ADC $02
    CLC 
    ADC $00
    ASL 
    SEC 
    SBC $0AAE
    BCS code_03EB64
    INC $02
}

-----------------------------------------------
;Hook for enemy health bar

cmd_0B_03EBFC {
    PHY
    STZ $08

  cmd0B_hp_check:
    LDA $09E6
    CMP #$0029
    BMI cmd0B_cycle_end
    SEC
    SBC #$0028
    STA $09E6

    LDA $09E4
    SEC
    SBC #$0028
    STA $09E4
    
    INC $08
    BRA cmd0B_hp_check
    
  cmd0B_cycle_end:
    LDA $09E4
    CMP #$0029
    BMI $06
    LDA #$0028
    STA $09E4
    
    PHX
    LDA $08
    ASL
    CLC
    ADC $08
    ASL
    TAX
    LDA @enemy_damage_palette, X
    STA $7F0A0A
    INX
    INX
    LDA @enemy_damage_palette, X
    STA $7F0A0C
    INX
    INX
    LDA @enemy_damage_palette, X
    STA $7F0A0E
    STZ $08
    PLX
}

----------------------------------------------
?INCLUDE 'sE6_gaia'
----------------------------------------------

code_08DB14 {
    LDA $0ACE
    CMP $0ACA
    BEQ code_08DB37
    COP [BF] ( &widestring_08DE4D )
    LDA #$FFF0
    TSB $joypad_mask_std
    LDA $0ACA
    SEC 
    SBC $0ACE
    STA $0B22
    COP [C1]
    LDA $0ACE
    CMP $0ACA
    BEQ code_08DB37
    RTL 
}
