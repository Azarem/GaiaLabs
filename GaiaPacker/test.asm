
main:
  LDA  [D, 3E]
  STA  D, 78
  INC  D, 3E
  INC  D, 3E

  CMP  $0
  BEQ  return

  JML  828777

return:
  JML  82878A

02876D:
  JML  main