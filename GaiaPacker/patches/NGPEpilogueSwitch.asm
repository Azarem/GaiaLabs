
h_epilogue_thinker [
  h_thinker < #00, #08 >   ;00
]

e_epilogue_thinker {
    PHX
    LDA $0D8C
    XBA
    ASL
    TAX
    LDA $306354, X
    BIT #$0080
    BNE epilogue_init_next
    ORA #$0080
    STA $306354, X

    LDA $3063FC, X
    CLC
    ADC #$0080
    STA $3063FC, X
    
    LDA $3063FE, X
    EOR #$0080
    STA $3063FE, X

  epilogue_init_next:
    PLX
    COP [3D]
    RTL
}


------------------------------------------
?INCLUDE 'scene_thinkers'
------------------------------------------

thinker_0CEA9B [
  thinker < #74, @h_thinker_00B520 >   ;00
  thinker < #00, @h_thinker_00BCDF >   ;01
  thinker < #00, @h_thinker_00BCB3 >   ;02
  thinker < #24, @h_parallax_thinker >   ;03
  thinker < #00, @h_epilogue_thinker >   ;04
]

