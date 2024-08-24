?BANK 02

?INCLUDE 'chunk_028000'

-- Patch for tilemap loading which adds support for no compression

!sptr		                    3E
!DCMP_SIZE	                    78
!META_SIZE                      0666
!DST_OFF                        0668
!map_bounds_x                   0692
!map_bounds_y                   0696
!A1T0L                          4302
!A1B0                           4304

---------------------------------------------

code_02883D {
    REP #$20
    
    LDA [$sptr]
    INC $sptr
    INC $sptr
    CMP #$0000
    BEQ func_0288B5
    BMI func_0288B5
    STA $DCMP_SIZE
    STA $META_SIZE

    SEP #$20
    LDA $066A
    BIT #$01
    BEQ code_02888E
    LDX #$0000
    JSR $&sub_028895
    LDA $066A
    BIT #$02
    BEQ code_028894
    LDX #$A000
    STX $3E
    LDA #$7E
    STA $40
    LDX #$C000
    STX $42
    LDA #$7E
    STA $44
    JSR $&sub_028DEA
    LDA $01
    STA $0695
    XBA 
    LDA $03
    STA $0699
    JSL $%func_0281D1
    STA $069D
    BRA code_028894
}

---------------------------------------------

sub_028914 {
    REP #$20
    LDA $00
    STA $map_bounds_x, X    -- copy stored width
    LDA $02
    STA $map_bounds_y, X    -- copy stored height
    LDA $META_SIZE
    STA $069A, X            -- copy stored multiply result (used by 0 index)
    STZ $DST_OFF             -- zero dest offset

    JSR $&sub_028DEA
    SEP #$20
    RTS
}

---------------------------------------------

func_028926 {
    REP #$20
    LDA [$sptr]
    INC $sptr
    INC $sptr
    CMP #$0000
    BMI do_copy
    BNE do_copy
    STA $DCMP_SIZE
    BRA code_028943
    
  do_copy:
    SEP #$20
    LDX $sptr
    STX $A1T0L
    LDA $40
    STA $A1B0
    BRA code_028959
}
