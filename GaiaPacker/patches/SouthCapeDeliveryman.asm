?BANK 0A


--------------------------------------------

h_actor < #0A, #00, #10 >

--------------------------------------------

skyd_main {
    COP [D0] ( #8D, #00, &skyd_destroy )
    COP [C0] ( &skyd_interact )
    COP [0B]
    COP [C1]
    RTL

  skyd_destroy:
    COP [E0]

  skyd_interact:
    COP [BF] ( &skyd_str_intro )
    COP [BE] ( #02, #02, &skyd_options )
    
  skyd_options [
    &skyd_cancel
    &skyd_cancel
    &skyd_confirm
  ]

  skyd_cancel:
    COP [BF] ( &skyd_str_cancel )
    RTL

  skyd_confirm:
    COP [BF] ( &skyd_str_confirm )
    LDA #$000D
    STA $0D60
    LDA #$0404
    STA $064A
    COP [65] ( #$00D4, #$03A4, #00, #23 )
    COP [26] ( #78, #$0160, #$0268, #07, #$4500 )
    RTL 
}

skyd_str_intro   `[DEF]I'm the Sky Deliveryman. [N]My tame birds [LU1:EF][LU1:E7][N]you to [LU1:85]towns. [FIN]Do you [LU1:F1]to[N]go to Watermia?[N] Quit[N] Go`
skyd_str_cancel  `[CLR]OK. In [LU1:D7]case,[N]use [LU1:E6]later.[END]`
skyd_str_confirm `[CLR][LU1:9]here, birds.[N][LU2:3F]taking [LU1:D6]person[N]to Watermia![END]`
