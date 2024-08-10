?BANK 02

meta_jump_table:
  #$0000
  #$0000
  &iny_1
  &iny_7
  &iny_6
  &iny_7
  &iny_4
  #$0000
  #$0000
  #$0000
  #$0000
  #$0000
  #$0000
  #$0000
  &iny_3
  #$0000
  &iny_6  ;10
  &iny_5
  &quit
  &iny_2
  &do14
  &iny_1


--------------------------------

028CF2:
  REP #$20
  LDA $0644
  ASL
  TAY
  LDA [$3A], Y
  SEC
  SBC &scene_meta
  TAY
  SEP #$20
  RTS

028D3D:
  JSR $8CE7
  PHX
  PHA 
  REP #$20
  LDA [$3A]
  CLC
  SBC $3A
  TAY
  SEP #$20
  LDA #$00
  XBA


loop_top:
  INY
  LDA [$3A], Y
  BEQ loop_top
  ASL
  TAX

  JMP (&meta_jump_table, X)
  
iny_7:
  INY

iny_6:
  INY
  
iny_5:
  INY
  
iny_4:
  INY
  
iny_3:
  INY
  
iny_2:
  INY

iny_1:
  INY
  BRA loop_top

do14:
  INY
  LDA [$3A], Y
  CMP $01, S
  BNE loop_top

quit:
  INY 
  PLA
  PLX
  RTS