

-------------------------------------------------
?INCLUDE 'chunk_03BAE1'
-------------------------------------------------

item_swapping_main:
  LDA $09EC
  BIT #$4000
  BNE item_swapping_end
  LDA $09AA
  BEQ item_swapping_end    ;Skip if player has no actor
  LDA $0656
  BIT #$0040
  BEQ item_swapping_reset    ;Skip if not pressing X
  BIT #$0010
  BNE item_swapping_right
  BIT #$0020
  BNE item_swapping_left

item_swapping_reset:
  LDA #$0002
  TRB $09EC

item_swapping_end:
  RTL    

item_swapping_left:
  LDA $09EC
  BIT #$0002
  BNE item_swapping_end

  LDY $0AC4
  BPL item_swap_left_begin
  LDY #$0010

item_swap_left_begin:
  LDA #$0000

item_swap_left_top:
  DEY
  BMI item_swap_store
  LDA $0AB4, Y
  AND #$00FF
  BEQ item_swap_left_top

item_swap_store:
  STY $0AC4
  STA $0AC6
  LDA #$0002
  TSB $09EC
  COP [07] ( #10 )
  JSL @EquippedIcon
  RTL


item_swapping_right:
  LDA $09EC
  BIT #$0002
  BNE item_swapping_end

  LDY $0AC4
  BPL item_swap_right_begin
  LDY #$FFFF

item_swap_right_begin:
  LDA #$0000

item_swap_right_top:
  INY
  CPY #$0010
  BCS item_swap_right_nothing
  LDA $0AB4, Y
  AND #$00FF
  BEQ item_swap_right_top
  BRA item_swap_store

item_swap_right_nothing:
  LDY #$FFFF
  BRA item_swap_store
