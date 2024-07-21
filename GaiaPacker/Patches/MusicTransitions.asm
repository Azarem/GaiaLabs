?BANK 03

!meta_next_id		$0642
!meta_current_id	$0644
!token              $E6
---------------------------------------

bgm_table [
  #FF #1D #1D #1D #1D #1D #1D #1D #1D #FF #05 #1B #06 #06 #06 #06  ;0F
  #06 #06 #06 #06 #FF #03 #03 #03 #03 #04 #04 #04 #04 #07 #07 #07  ;1F
  #07 #07 #07 #07 #07 #07 #07 #07 #07 #07 #11 #04 #04 #04 #04 #15  ;2F
  #1B #1B #02 #02 #02 #02 #02 #02 #02 #02 #02 #02 #06 #06 #06 #06  ;3F
  #06 #06 #06 #06 #06 #06 #06 #04 #FF #03 #FF #04 #08 #08 #08 #08  ;4F
  #08 #08 #08 #08 #08 #08 #08 #FF #FF #FF #04 #04 #04 #04 #04 #09  ;5F
  #09 #09 #09 #04 #09 #09 #04 #04 #04 #03 #03 #03 #03 #06 #06 #06  ;6F
  #06 #06 #06 #1D #04 #04 #FF #FF #02 #02 #02 #06 #02 #02 #02 #15  ;7F
  #FF #FF #0A #0A #FF #0A #0A #0A #0A #FF #0A #0A #FF #FF #FF #FF  ;8F
  #FF #02 #02 #02 #02 #05 #05 #02 #02 #02 #02 #02 #02 #04 #FF #FF  ;9F
  #06 #06 #06 #06 #06 #06 #06 #06 #06 #06 $FF #FF #04 #FF #FF #FF  ;AF
  #04 #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #0B #04  ;BF
  #11 #FF #FF #02 #02 #04 #02 #02 #02 #02 #FF #FF #04 #04 #0C #0C  ;CF
  #0C #0C #0C #0C #0C #0C #0C #0C #0C #0C #0C #0C #02 #0F #04 #04  ;DF
  #04 #04 #04 #04 #0E #0E #16 #FF #FF #06 #FF #FF #FF #FF #FF #FF  ;EF
  #00 #FF #0F #0F #0F #0F #0F #1B #FF #13 #00 #1B #14 #1B #12 #FF  ;FF
]


check_bgm:
  PHX
  PHY
  LDX meta_next_id
  LDA %bgm_table, X
  BMI return
  TAY
  LDX meta_current_id
  LDA %bgm_table, X
  BMI return
  TYA
  CMP %bgm_table, X
  BEQ return
  LDA $0D72
  BEQ return
  LDA #F2
  STA token
  
return:
  PLY
  PLX
  LDA $0654
  JMP $D9F9

count_check:
  CMP #09
  BNE count_ret
  PHA
  LDA #F2
  CMP token
  BNE count_fix
  STA $2140
  STZ token

count_fix:
  PLA

count_ret:
  RTS

fade_check:
  JSR count_check
  DEC
  BPL $01
  RTS
  STA $0DB6
  JMP $DAD5

mosaic_check:
  JSR count_check
  DEC
  BPL $01
  RTS
  JMP $DAFC

immed_check:
  JSL $80811E
  JSR count_check+4
  STZ $2100
  RTS

stack_check1:
  JSR count_check
  DEC
  STA $01, S
  JMP $DB91

stack_check2:
  JSR count_check
  DEC
  STA $02, S
  JMP $DBE3


03D9F6:
  JMP check_bgm

03DAEB:
  JMP fade_check
  
03DAF2:
  JMP immed_check

03DB1C:
  JMP mosaic_check

03DB8E:
  JMP stack_check1

03DBE0:
  JMP stack_check2

028B91:
  LDA #01
  JSL $8281C9
  LDA #F0
  STA $2140
  LDA #01
  JSL $8281C9
  BRA $828BB1
