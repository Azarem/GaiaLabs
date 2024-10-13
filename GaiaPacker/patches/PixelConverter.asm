
!convert1                       09FA
!convert2                       09FD
!dest_offset1                   0002
!dest_offset2                   0004

------------------------------------------------

convert_pixels {
    LDY #$convert1
    LDX #$dest_offset1
    JSR convert_pixel

    LDY #$convert2
    LDX #$dest_offset2
    JSR convert_pixel

    RTL
}

convert_color {
    CMP #80
    BEQ $0A
    BPL $05
    CLC
    ADC $01
    BRA $03
    SEC
    SBC $01
    LSR
    LSR
    LSR
    RTS
}

convert_pixel {
    LDA $0000, Y
    PHA
    LDA $0001, Y
    ASL
    ASL
    AND #$7C00
    STA $7F0A00, X
    PLA
    TAY
    AND #$001F
    ORA $7F0A00, X
    STA $7F0A00, X
    TYA
    LSR
    LSR
    LSR
    AND #$03E0
    ORA $7F0A00, X
    STA $7F0A00, X
    RTS
}



