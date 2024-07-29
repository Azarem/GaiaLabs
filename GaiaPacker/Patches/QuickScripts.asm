;?BANK 01



----------------------------


04D234:
  LDA #$1000
  TRB $10
  COP [C3] ( $84D251, #$0100 )
  LDA #$1000
  TSB $10
  BRA $84D251

04DB28:         ;Free up cell door flag (02)
  COP [E0]

04DB5D:	        ;Free up moss flag (03)
  COP [E0]

04DBEC:			;Free up chain flag (04)
  COP [E0]