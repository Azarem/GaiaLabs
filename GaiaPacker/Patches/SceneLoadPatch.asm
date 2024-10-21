?BANK 02

?INCLUDE 'chunk_028000'

!scene_current                  0644

----------------------------------------

meta_jump_table:
  &code_028D44
  #$0000
  &code_028D83
  &code_028D7D
  &code_028D7E
  &code_028D7D
  &code_028D80
  #$0000
  #$0000
  #$0000
  #$0000
  #$0000
  #$0000
  #$0000
  &code_028D81
  #$0000
  &code_028D7E  ;10
  &code_028D7F
  &code_028D8D
  &code_028D82
  &code_028D86
  &code_028D83


--------------------------------

sub_028CF2 {
    REP #$20
    LDA $scene_current
    ASL
    TAY
    LDA [$3A], Y
    SEC
    SBC $3A
    TAY
    SEP #$20
    RTS

  code_028CF5:
  code_028CFF:
  code_028D34:
  code_028D35:
  code_028D36:
  code_028D37:
  code_028D38:
  code_028D39:
  code_028D3A:
}

-----------------------------------------

func_028D3D {
    JSR $&sub_028CE7
    PHX
    PHA 
    REP #$20
    LDA [$3A]
    SEC
    SBC $3A
    TAY
    SEP #$20
    LDA #$00
    XBA

  code_028D44:
    ;INY 
    ;INY 

  code_028D46:
    LDA [$3A], Y
    INY
    ASL
    TAX
    JMP (&meta_jump_table, X)

  code_028D8D:
    PLA 
    PLX
    RTS 
}

