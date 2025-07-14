// Generated TypeScript OpCode definitions
// Source: Complete 65C816 Instruction Set
// Generated: 2025-07-14T12:52:35.473Z

export type Byte = number;
export type Word = number;
export type Absolute = number;
export type Direct = number;
export type Label = string;
export type Immediate = number;
export type AbsoluteLong = number;
export type BankPair = { src: number; dest: number };
export type InterruptVector = number;

export interface OpDef {
  opcode: OpCode;
  mnemonic: string;
  size: number | 'flag-dependent';
  addressingMode: string;
}

export enum OpCode {
  LDA_imm = 0xA9,
  LDA_abs = 0xAD,
  LDA_long = 0xAF,
  LDA_dp = 0xA5,
  LDA_dp_ind = 0xB2,
  LDA_dp_indl = 0xA7,
  LDA_abs_x = 0xBD,
  LDA_long_x = 0xBF,
  LDA_abs_y = 0xB9,
  LDA_dp_x = 0xB5,
  LDA_dp_x_ind = 0xA1,
  LDA_dp_ind_y = 0xB1,
  LDA_dp_indl_y = 0xB7,
  LDA_stkr = 0xA3,
  LDA_stkr_ind_y = 0xB3,
  LDX_imm = 0xA2,
  LDX_abs = 0xAE,
  LDX_dp = 0xA6,
  LDX_abs_y = 0xBE,
  LDX_dp_y = 0xB6,
  LDY_imm = 0xA0,
  LDY_abs = 0xAC,
  LDY_dp = 0xA4,
  LDY_abs_x = 0xBC,
  LDY_dp_x = 0xB4,
  STA_abs = 0x8D,
  STA_long = 0x8F,
  STA_dp = 0x85,
  STA_dp_ind = 0x92,
  STA_dp_indl = 0x87,
  STA_abs_x = 0x9D,
  STA_long_x = 0x9F,
  STA_abs_y = 0x99,
  STA_dp_x = 0x95,
  STA_dp_x_ind = 0x81,
  STA_dp_ind_y = 0x91,
  STA_dp_indl_y = 0x97,
  STA_stkr = 0x83,
  STA_stkr_ind_y = 0x93,
  STX_abs = 0x8E,
  STX_dp = 0x86,
  STX_dp_y = 0x96,
  STY_abs = 0x8C,
  STY_dp = 0x84,
  STY_dp_x = 0x94,
  STZ_abs = 0x9C,
  STZ_dp = 0x64,
  STZ_abs_x = 0x9E,
  STZ_dp_x = 0x74,
  ADC_imm = 0x69,
  ADC_abs = 0x6D,
  ADC_long = 0x6F,
  ADC_dp = 0x65,
  ADC_dp_ind = 0x72,
  ADC_dp_indl = 0x67,
  ADC_abs_x = 0x7D,
  ADC_long_x = 0x7F,
  ADC_abs_y = 0x79,
  ADC_dp_x = 0x75,
  ADC_dp_x_ind = 0x61,
  ADC_dp_ind_y = 0x71,
  ADC_dp_indl_y = 0x77,
  ADC_stkr = 0x63,
  ADC_stkr_ind_y = 0x73,
  SBC_imm = 0xE9,
  SBC_abs = 0xED,
  SBC_long = 0xEF,
  SBC_dp = 0xE5,
  SBC_dp_ind = 0xF2,
  SBC_dp_indl = 0xE7,
  SBC_abs_x = 0xFD,
  SBC_long_x = 0xFF,
  SBC_abs_y = 0xF9,
  SBC_dp_x = 0xF5,
  SBC_dp_x_ind = 0xE1,
  SBC_dp_ind_y = 0xF1,
  SBC_dp_indl_y = 0xF7,
  SBC_stkr = 0xE3,
  SBC_stkr_ind_y = 0xF3,
  CMP_imm = 0xC9,
  CMP_abs = 0xCD,
  CMP_long = 0xCF,
  CMP_dp = 0xC5,
  CMP_dp_ind = 0xD2,
  CMP_dp_indl = 0xC7,
  CMP_abs_x = 0xDD,
  CMP_long_x = 0xDF,
  CMP_abs_y = 0xD9,
  CMP_dp_x = 0xD5,
  CMP_dp_x_ind = 0xC1,
  CMP_dp_ind_y = 0xD1,
  CMP_dp_indl_y = 0xD7,
  CMP_stkr = 0xC3,
  CMP_stkr_ind_y = 0xD3,
  CPX_imm = 0xE0,
  CPX_abs = 0xEC,
  CPX_dp = 0xE4,
  CPY_imm = 0xC0,
  CPY_abs = 0xCC,
  CPY_dp = 0xC4,
  INC_acc = 0x1A,
  INC_abs = 0xEE,
  INC_dp = 0xE6,
  INC_abs_x = 0xFE,
  INC_dp_x = 0xF6,
  DEC_acc = 0x3A,
  DEC_abs = 0xCE,
  DEC_dp = 0xC6,
  DEC_abs_x = 0xDE,
  DEC_dp_x = 0xD6,
  INX_imp = 0xE8,
  INY_imp = 0xC8,
  DEX_imp = 0xCA,
  DEY_imp = 0x88,
  AND_imm = 0x29,
  AND_abs = 0x2D,
  AND_long = 0x2F,
  AND_dp = 0x25,
  AND_dp_ind = 0x32,
  AND_dp_indl = 0x27,
  AND_abs_x = 0x3D,
  AND_long_x = 0x3F,
  AND_abs_y = 0x39,
  AND_dp_x = 0x35,
  AND_dp_x_ind = 0x21,
  AND_dp_ind_y = 0x31,
  AND_dp_indl_y = 0x37,
  AND_stkr = 0x23,
  AND_stkr_ind_y = 0x33,
  ORA_imm = 0x09,
  ORA_abs = 0x0D,
  ORA_long = 0x0F,
  ORA_dp = 0x05,
  ORA_dp_ind = 0x12,
  ORA_dp_indl = 0x07,
  ORA_abs_x = 0x1D,
  ORA_long_x = 0x1F,
  ORA_abs_y = 0x19,
  ORA_dp_x = 0x15,
  ORA_dp_x_ind = 0x01,
  ORA_dp_ind_y = 0x11,
  ORA_dp_indl_y = 0x17,
  ORA_stkr = 0x03,
  ORA_stkr_ind_y = 0x13,
  EOR_imm = 0x49,
  EOR_abs = 0x4D,
  EOR_long = 0x4F,
  EOR_dp = 0x45,
  EOR_dp_ind = 0x52,
  EOR_dp_indl = 0x47,
  EOR_abs_x = 0x5D,
  EOR_long_x = 0x5F,
  EOR_abs_y = 0x59,
  EOR_dp_x = 0x55,
  EOR_dp_x_ind = 0x41,
  EOR_dp_ind_y = 0x51,
  EOR_dp_indl_y = 0x57,
  EOR_stkr = 0x43,
  EOR_stkr_ind_y = 0x53,
  BIT_imm = 0x89,
  BIT_abs = 0x2C,
  BIT_dp = 0x24,
  BIT_abs_x = 0x3C,
  BIT_dp_x = 0x34,
  TRB_abs = 0x1C,
  TRB_dp = 0x14,
  TSB_abs = 0x0C,
  TSB_dp = 0x04,
  ASL_acc = 0x0A,
  ASL_abs = 0x0E,
  ASL_dp = 0x06,
  ASL_abs_x = 0x1E,
  ASL_dp_x = 0x16,
  LSR_acc = 0x4A,
  LSR_abs = 0x4E,
  LSR_dp = 0x46,
  LSR_abs_x = 0x5E,
  LSR_dp_x = 0x56,
  ROL_acc = 0x2A,
  ROL_abs = 0x2E,
  ROL_dp = 0x26,
  ROL_abs_x = 0x3E,
  ROL_dp_x = 0x36,
  ROR_acc = 0x6A,
  ROR_abs = 0x6E,
  ROR_dp = 0x66,
  ROR_abs_x = 0x7E,
  ROR_dp_x = 0x76,
  JMP_abs = 0x4C,
  JMP_abs_ind = 0x6C,
  JMP_abs_x_ind = 0x7C,
  JML_long = 0x5C,
  JML_abs_indl = 0xDC,
  JSR_abs = 0x20,
  JSR_abs_x_ind = 0xFC,
  JSL_long = 0x22,
  RTS_stk = 0x60,
  RTL_stk = 0x6B,
  RTI_stk = 0x40,
  BCC_rel = 0x90,
  BCS_rel = 0xB0,
  BEQ_rel = 0xF0,
  BNE_rel = 0xD0,
  BMI_rel = 0x30,
  BPL_rel = 0x10,
  BVC_rel = 0x50,
  BVS_rel = 0x70,
  BRA_rel = 0x80,
  BRL_rel16 = 0x82,
  NOP_imp = 0xEA,
  BRK_stk_int = 0x00,
  COP_stk_int = 0x02,
  SEP_imm = 0xE2,
  REP_imm = 0xC2,
  WDM_imp = 0x42,
  CLI_imp = 0x58,
  SEI_imp = 0x78,
  STP_imp = 0xDB,
  WAI_imp = 0xCB,
  PHA_stk = 0x48,
  PHX_stk = 0xDA,
  PHY_stk = 0x5A,
  PLA_stk = 0x68,
  PLX_stk = 0xFA,
  PLY_stk = 0x7A,
  PHB_stk = 0x8B,
  PHD_stk = 0x0B,
  PHK_stk = 0x4B,
  PHP_stk = 0x08,
  PLB_stk = 0xAB,
  PLD_stk = 0x2B,
  PLP_stk = 0x28,
  PEA_abs = 0xF4,
  PEI_dp_ind = 0xD4,
  PER_rel16 = 0x62,
  TAX_imp = 0xAA,
  TAY_imp = 0xA8,
  TXA_imp = 0x8A,
  TYA_imp = 0x98,
  TSX_imp = 0xBA,
  TXS_imp = 0x9A,
  TXY_imp = 0x9B,
  TYX_imp = 0xBB,
  TCD_imp = 0x5B,
  TCS_imp = 0x1B,
  TDC_imp = 0x7B,
  TSC_imp = 0x3B,
  MVN_src_dest = 0x54,
  MVP_src_dest = 0x44,
  XBA_imp = 0xEB,
  XCE_imp = 0xFB,
  CLC_imp = 0x18,
  CLD_imp = 0xD8,
  CLV_imp = 0xB8,
  SEC_imp = 0x38,
  SED_imp = 0xF8
}

export const LDA_imm: OpDef = {
  opcode: OpCode.LDA_imm,
  size: 'flag-dependent',
  addressingMode: 'Immediate',
  mnemonic: 'LDA'
};

export const LDA_abs: OpDef = {
  opcode: OpCode.LDA_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'LDA'
};

export const LDA_long: OpDef = {
  opcode: OpCode.LDA_long,
  size: 4,
  addressingMode: 'AbsoluteLong',
  mnemonic: 'LDA'
};

export const LDA_dp: OpDef = {
  opcode: OpCode.LDA_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'LDA'
};

export const LDA_dp_ind: OpDef = {
  opcode: OpCode.LDA_dp_ind,
  size: 2,
  addressingMode: 'DirectPageIndirect',
  mnemonic: 'LDA'
};

export const LDA_dp_indl: OpDef = {
  opcode: OpCode.LDA_dp_indl,
  size: 2,
  addressingMode: 'DirectPageIndirectLong',
  mnemonic: 'LDA'
};

export const LDA_abs_x: OpDef = {
  opcode: OpCode.LDA_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'LDA'
};

export const LDA_long_x: OpDef = {
  opcode: OpCode.LDA_long_x,
  size: 4,
  addressingMode: 'AbsoluteLongIndexedX',
  mnemonic: 'LDA'
};

export const LDA_abs_y: OpDef = {
  opcode: OpCode.LDA_abs_y,
  size: 3,
  addressingMode: 'AbsoluteIndexedY',
  mnemonic: 'LDA'
};

export const LDA_dp_x: OpDef = {
  opcode: OpCode.LDA_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'LDA'
};

export const LDA_dp_x_ind: OpDef = {
  opcode: OpCode.LDA_dp_x_ind,
  size: 2,
  addressingMode: 'DirectPageIndexedIndirectX',
  mnemonic: 'LDA'
};

export const LDA_dp_ind_y: OpDef = {
  opcode: OpCode.LDA_dp_ind_y,
  size: 2,
  addressingMode: 'DirectPageIndirectIndexedY',
  mnemonic: 'LDA'
};

export const LDA_dp_indl_y: OpDef = {
  opcode: OpCode.LDA_dp_indl_y,
  size: 2,
  addressingMode: 'DirectPageIndirectLongIndexedY',
  mnemonic: 'LDA'
};

export const LDA_stkr: OpDef = {
  opcode: OpCode.LDA_stkr,
  size: 2,
  addressingMode: 'StackRelative',
  mnemonic: 'LDA'
};

export const LDA_stkr_ind_y: OpDef = {
  opcode: OpCode.LDA_stkr_ind_y,
  size: 2,
  addressingMode: 'StackRelativeIndirectIndexedY',
  mnemonic: 'LDA'
};

export const LDX_imm: OpDef = {
  opcode: OpCode.LDX_imm,
  size: 'flag-dependent',
  addressingMode: 'Immediate',
  mnemonic: 'LDX'
};

export const LDX_abs: OpDef = {
  opcode: OpCode.LDX_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'LDX'
};

export const LDX_dp: OpDef = {
  opcode: OpCode.LDX_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'LDX'
};

export const LDX_abs_y: OpDef = {
  opcode: OpCode.LDX_abs_y,
  size: 3,
  addressingMode: 'AbsoluteIndexedY',
  mnemonic: 'LDX'
};

export const LDX_dp_y: OpDef = {
  opcode: OpCode.LDX_dp_y,
  size: 2,
  addressingMode: 'DirectPageIndexedY',
  mnemonic: 'LDX'
};

export const LDY_imm: OpDef = {
  opcode: OpCode.LDY_imm,
  size: 'flag-dependent',
  addressingMode: 'Immediate',
  mnemonic: 'LDY'
};

export const LDY_abs: OpDef = {
  opcode: OpCode.LDY_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'LDY'
};

export const LDY_dp: OpDef = {
  opcode: OpCode.LDY_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'LDY'
};

export const LDY_abs_x: OpDef = {
  opcode: OpCode.LDY_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'LDY'
};

export const LDY_dp_x: OpDef = {
  opcode: OpCode.LDY_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'LDY'
};

export const STA_abs: OpDef = {
  opcode: OpCode.STA_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'STA'
};

export const STA_long: OpDef = {
  opcode: OpCode.STA_long,
  size: 4,
  addressingMode: 'AbsoluteLong',
  mnemonic: 'STA'
};

export const STA_dp: OpDef = {
  opcode: OpCode.STA_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'STA'
};

export const STA_dp_ind: OpDef = {
  opcode: OpCode.STA_dp_ind,
  size: 2,
  addressingMode: 'DirectPageIndirect',
  mnemonic: 'STA'
};

export const STA_dp_indl: OpDef = {
  opcode: OpCode.STA_dp_indl,
  size: 2,
  addressingMode: 'DirectPageIndirectLong',
  mnemonic: 'STA'
};

export const STA_abs_x: OpDef = {
  opcode: OpCode.STA_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'STA'
};

export const STA_long_x: OpDef = {
  opcode: OpCode.STA_long_x,
  size: 4,
  addressingMode: 'AbsoluteLongIndexedX',
  mnemonic: 'STA'
};

export const STA_abs_y: OpDef = {
  opcode: OpCode.STA_abs_y,
  size: 3,
  addressingMode: 'AbsoluteIndexedY',
  mnemonic: 'STA'
};

export const STA_dp_x: OpDef = {
  opcode: OpCode.STA_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'STA'
};

export const STA_dp_x_ind: OpDef = {
  opcode: OpCode.STA_dp_x_ind,
  size: 2,
  addressingMode: 'DirectPageIndexedIndirectX',
  mnemonic: 'STA'
};

export const STA_dp_ind_y: OpDef = {
  opcode: OpCode.STA_dp_ind_y,
  size: 2,
  addressingMode: 'DirectPageIndirectIndexedY',
  mnemonic: 'STA'
};

export const STA_dp_indl_y: OpDef = {
  opcode: OpCode.STA_dp_indl_y,
  size: 2,
  addressingMode: 'DirectPageIndirectLongIndexedY',
  mnemonic: 'STA'
};

export const STA_stkr: OpDef = {
  opcode: OpCode.STA_stkr,
  size: 2,
  addressingMode: 'StackRelative',
  mnemonic: 'STA'
};

export const STA_stkr_ind_y: OpDef = {
  opcode: OpCode.STA_stkr_ind_y,
  size: 2,
  addressingMode: 'StackRelativeIndirectIndexedY',
  mnemonic: 'STA'
};

export const STX_abs: OpDef = {
  opcode: OpCode.STX_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'STX'
};

export const STX_dp: OpDef = {
  opcode: OpCode.STX_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'STX'
};

export const STX_dp_y: OpDef = {
  opcode: OpCode.STX_dp_y,
  size: 2,
  addressingMode: 'DirectPageIndexedY',
  mnemonic: 'STX'
};

export const STY_abs: OpDef = {
  opcode: OpCode.STY_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'STY'
};

export const STY_dp: OpDef = {
  opcode: OpCode.STY_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'STY'
};

export const STY_dp_x: OpDef = {
  opcode: OpCode.STY_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'STY'
};

export const STZ_abs: OpDef = {
  opcode: OpCode.STZ_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'STZ'
};

export const STZ_dp: OpDef = {
  opcode: OpCode.STZ_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'STZ'
};

export const STZ_abs_x: OpDef = {
  opcode: OpCode.STZ_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'STZ'
};

export const STZ_dp_x: OpDef = {
  opcode: OpCode.STZ_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'STZ'
};

export const ADC_imm: OpDef = {
  opcode: OpCode.ADC_imm,
  size: 'flag-dependent',
  addressingMode: 'Immediate',
  mnemonic: 'ADC'
};

export const ADC_abs: OpDef = {
  opcode: OpCode.ADC_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'ADC'
};

export const ADC_long: OpDef = {
  opcode: OpCode.ADC_long,
  size: 4,
  addressingMode: 'AbsoluteLong',
  mnemonic: 'ADC'
};

export const ADC_dp: OpDef = {
  opcode: OpCode.ADC_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'ADC'
};

export const ADC_dp_ind: OpDef = {
  opcode: OpCode.ADC_dp_ind,
  size: 2,
  addressingMode: 'DirectPageIndirect',
  mnemonic: 'ADC'
};

export const ADC_dp_indl: OpDef = {
  opcode: OpCode.ADC_dp_indl,
  size: 2,
  addressingMode: 'DirectPageIndirectLong',
  mnemonic: 'ADC'
};

export const ADC_abs_x: OpDef = {
  opcode: OpCode.ADC_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'ADC'
};

export const ADC_long_x: OpDef = {
  opcode: OpCode.ADC_long_x,
  size: 4,
  addressingMode: 'AbsoluteLongIndexedX',
  mnemonic: 'ADC'
};

export const ADC_abs_y: OpDef = {
  opcode: OpCode.ADC_abs_y,
  size: 3,
  addressingMode: 'AbsoluteIndexedY',
  mnemonic: 'ADC'
};

export const ADC_dp_x: OpDef = {
  opcode: OpCode.ADC_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'ADC'
};

export const ADC_dp_x_ind: OpDef = {
  opcode: OpCode.ADC_dp_x_ind,
  size: 2,
  addressingMode: 'DirectPageIndexedIndirectX',
  mnemonic: 'ADC'
};

export const ADC_dp_ind_y: OpDef = {
  opcode: OpCode.ADC_dp_ind_y,
  size: 2,
  addressingMode: 'DirectPageIndirectIndexedY',
  mnemonic: 'ADC'
};

export const ADC_dp_indl_y: OpDef = {
  opcode: OpCode.ADC_dp_indl_y,
  size: 2,
  addressingMode: 'DirectPageIndirectLongIndexedY',
  mnemonic: 'ADC'
};

export const ADC_stkr: OpDef = {
  opcode: OpCode.ADC_stkr,
  size: 2,
  addressingMode: 'StackRelative',
  mnemonic: 'ADC'
};

export const ADC_stkr_ind_y: OpDef = {
  opcode: OpCode.ADC_stkr_ind_y,
  size: 2,
  addressingMode: 'StackRelativeIndirectIndexedY',
  mnemonic: 'ADC'
};

export const SBC_imm: OpDef = {
  opcode: OpCode.SBC_imm,
  size: 'flag-dependent',
  addressingMode: 'Immediate',
  mnemonic: 'SBC'
};

export const SBC_abs: OpDef = {
  opcode: OpCode.SBC_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'SBC'
};

export const SBC_long: OpDef = {
  opcode: OpCode.SBC_long,
  size: 4,
  addressingMode: 'AbsoluteLong',
  mnemonic: 'SBC'
};

export const SBC_dp: OpDef = {
  opcode: OpCode.SBC_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'SBC'
};

export const SBC_dp_ind: OpDef = {
  opcode: OpCode.SBC_dp_ind,
  size: 2,
  addressingMode: 'DirectPageIndirect',
  mnemonic: 'SBC'
};

export const SBC_dp_indl: OpDef = {
  opcode: OpCode.SBC_dp_indl,
  size: 2,
  addressingMode: 'DirectPageIndirectLong',
  mnemonic: 'SBC'
};

export const SBC_abs_x: OpDef = {
  opcode: OpCode.SBC_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'SBC'
};

export const SBC_long_x: OpDef = {
  opcode: OpCode.SBC_long_x,
  size: 4,
  addressingMode: 'AbsoluteLongIndexedX',
  mnemonic: 'SBC'
};

export const SBC_abs_y: OpDef = {
  opcode: OpCode.SBC_abs_y,
  size: 3,
  addressingMode: 'AbsoluteIndexedY',
  mnemonic: 'SBC'
};

export const SBC_dp_x: OpDef = {
  opcode: OpCode.SBC_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'SBC'
};

export const SBC_dp_x_ind: OpDef = {
  opcode: OpCode.SBC_dp_x_ind,
  size: 2,
  addressingMode: 'DirectPageIndexedIndirectX',
  mnemonic: 'SBC'
};

export const SBC_dp_ind_y: OpDef = {
  opcode: OpCode.SBC_dp_ind_y,
  size: 2,
  addressingMode: 'DirectPageIndirectIndexedY',
  mnemonic: 'SBC'
};

export const SBC_dp_indl_y: OpDef = {
  opcode: OpCode.SBC_dp_indl_y,
  size: 2,
  addressingMode: 'DirectPageIndirectLongIndexedY',
  mnemonic: 'SBC'
};

export const SBC_stkr: OpDef = {
  opcode: OpCode.SBC_stkr,
  size: 2,
  addressingMode: 'StackRelative',
  mnemonic: 'SBC'
};

export const SBC_stkr_ind_y: OpDef = {
  opcode: OpCode.SBC_stkr_ind_y,
  size: 2,
  addressingMode: 'StackRelativeIndirectIndexedY',
  mnemonic: 'SBC'
};

export const CMP_imm: OpDef = {
  opcode: OpCode.CMP_imm,
  size: 'flag-dependent',
  addressingMode: 'Immediate',
  mnemonic: 'CMP'
};

export const CMP_abs: OpDef = {
  opcode: OpCode.CMP_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'CMP'
};

export const CMP_long: OpDef = {
  opcode: OpCode.CMP_long,
  size: 4,
  addressingMode: 'AbsoluteLong',
  mnemonic: 'CMP'
};

export const CMP_dp: OpDef = {
  opcode: OpCode.CMP_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'CMP'
};

export const CMP_dp_ind: OpDef = {
  opcode: OpCode.CMP_dp_ind,
  size: 2,
  addressingMode: 'DirectPageIndirect',
  mnemonic: 'CMP'
};

export const CMP_dp_indl: OpDef = {
  opcode: OpCode.CMP_dp_indl,
  size: 2,
  addressingMode: 'DirectPageIndirectLong',
  mnemonic: 'CMP'
};

export const CMP_abs_x: OpDef = {
  opcode: OpCode.CMP_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'CMP'
};

export const CMP_long_x: OpDef = {
  opcode: OpCode.CMP_long_x,
  size: 4,
  addressingMode: 'AbsoluteLongIndexedX',
  mnemonic: 'CMP'
};

export const CMP_abs_y: OpDef = {
  opcode: OpCode.CMP_abs_y,
  size: 3,
  addressingMode: 'AbsoluteIndexedY',
  mnemonic: 'CMP'
};

export const CMP_dp_x: OpDef = {
  opcode: OpCode.CMP_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'CMP'
};

export const CMP_dp_x_ind: OpDef = {
  opcode: OpCode.CMP_dp_x_ind,
  size: 2,
  addressingMode: 'DirectPageIndexedIndirectX',
  mnemonic: 'CMP'
};

export const CMP_dp_ind_y: OpDef = {
  opcode: OpCode.CMP_dp_ind_y,
  size: 2,
  addressingMode: 'DirectPageIndirectIndexedY',
  mnemonic: 'CMP'
};

export const CMP_dp_indl_y: OpDef = {
  opcode: OpCode.CMP_dp_indl_y,
  size: 2,
  addressingMode: 'DirectPageIndirectLongIndexedY',
  mnemonic: 'CMP'
};

export const CMP_stkr: OpDef = {
  opcode: OpCode.CMP_stkr,
  size: 2,
  addressingMode: 'StackRelative',
  mnemonic: 'CMP'
};

export const CMP_stkr_ind_y: OpDef = {
  opcode: OpCode.CMP_stkr_ind_y,
  size: 2,
  addressingMode: 'StackRelativeIndirectIndexedY',
  mnemonic: 'CMP'
};

export const CPX_imm: OpDef = {
  opcode: OpCode.CPX_imm,
  size: 'flag-dependent',
  addressingMode: 'Immediate',
  mnemonic: 'CPX'
};

export const CPX_abs: OpDef = {
  opcode: OpCode.CPX_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'CPX'
};

export const CPX_dp: OpDef = {
  opcode: OpCode.CPX_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'CPX'
};

export const CPY_imm: OpDef = {
  opcode: OpCode.CPY_imm,
  size: 'flag-dependent',
  addressingMode: 'Immediate',
  mnemonic: 'CPY'
};

export const CPY_abs: OpDef = {
  opcode: OpCode.CPY_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'CPY'
};

export const CPY_dp: OpDef = {
  opcode: OpCode.CPY_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'CPY'
};

export const INC_acc: OpDef = {
  opcode: OpCode.INC_acc,
  size: 1,
  addressingMode: 'Accumulator',
  mnemonic: 'INC'
};

export const INC_abs: OpDef = {
  opcode: OpCode.INC_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'INC'
};

export const INC_dp: OpDef = {
  opcode: OpCode.INC_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'INC'
};

export const INC_abs_x: OpDef = {
  opcode: OpCode.INC_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'INC'
};

export const INC_dp_x: OpDef = {
  opcode: OpCode.INC_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'INC'
};

export const DEC_acc: OpDef = {
  opcode: OpCode.DEC_acc,
  size: 1,
  addressingMode: 'Accumulator',
  mnemonic: 'DEC'
};

export const DEC_abs: OpDef = {
  opcode: OpCode.DEC_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'DEC'
};

export const DEC_dp: OpDef = {
  opcode: OpCode.DEC_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'DEC'
};

export const DEC_abs_x: OpDef = {
  opcode: OpCode.DEC_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'DEC'
};

export const DEC_dp_x: OpDef = {
  opcode: OpCode.DEC_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'DEC'
};

export const INX_imp: OpDef = {
  opcode: OpCode.INX_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'INX'
};

export const INY_imp: OpDef = {
  opcode: OpCode.INY_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'INY'
};

export const DEX_imp: OpDef = {
  opcode: OpCode.DEX_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'DEX'
};

export const DEY_imp: OpDef = {
  opcode: OpCode.DEY_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'DEY'
};

export const AND_imm: OpDef = {
  opcode: OpCode.AND_imm,
  size: 'flag-dependent',
  addressingMode: 'Immediate',
  mnemonic: 'AND'
};

export const AND_abs: OpDef = {
  opcode: OpCode.AND_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'AND'
};

export const AND_long: OpDef = {
  opcode: OpCode.AND_long,
  size: 4,
  addressingMode: 'AbsoluteLong',
  mnemonic: 'AND'
};

export const AND_dp: OpDef = {
  opcode: OpCode.AND_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'AND'
};

export const AND_dp_ind: OpDef = {
  opcode: OpCode.AND_dp_ind,
  size: 2,
  addressingMode: 'DirectPageIndirect',
  mnemonic: 'AND'
};

export const AND_dp_indl: OpDef = {
  opcode: OpCode.AND_dp_indl,
  size: 2,
  addressingMode: 'DirectPageIndirectLong',
  mnemonic: 'AND'
};

export const AND_abs_x: OpDef = {
  opcode: OpCode.AND_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'AND'
};

export const AND_long_x: OpDef = {
  opcode: OpCode.AND_long_x,
  size: 4,
  addressingMode: 'AbsoluteLongIndexedX',
  mnemonic: 'AND'
};

export const AND_abs_y: OpDef = {
  opcode: OpCode.AND_abs_y,
  size: 3,
  addressingMode: 'AbsoluteIndexedY',
  mnemonic: 'AND'
};

export const AND_dp_x: OpDef = {
  opcode: OpCode.AND_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'AND'
};

export const AND_dp_x_ind: OpDef = {
  opcode: OpCode.AND_dp_x_ind,
  size: 2,
  addressingMode: 'DirectPageIndexedIndirectX',
  mnemonic: 'AND'
};

export const AND_dp_ind_y: OpDef = {
  opcode: OpCode.AND_dp_ind_y,
  size: 2,
  addressingMode: 'DirectPageIndirectIndexedY',
  mnemonic: 'AND'
};

export const AND_dp_indl_y: OpDef = {
  opcode: OpCode.AND_dp_indl_y,
  size: 2,
  addressingMode: 'DirectPageIndirectLongIndexedY',
  mnemonic: 'AND'
};

export const AND_stkr: OpDef = {
  opcode: OpCode.AND_stkr,
  size: 2,
  addressingMode: 'StackRelative',
  mnemonic: 'AND'
};

export const AND_stkr_ind_y: OpDef = {
  opcode: OpCode.AND_stkr_ind_y,
  size: 2,
  addressingMode: 'StackRelativeIndirectIndexedY',
  mnemonic: 'AND'
};

export const ORA_imm: OpDef = {
  opcode: OpCode.ORA_imm,
  size: 'flag-dependent',
  addressingMode: 'Immediate',
  mnemonic: 'ORA'
};

export const ORA_abs: OpDef = {
  opcode: OpCode.ORA_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'ORA'
};

export const ORA_long: OpDef = {
  opcode: OpCode.ORA_long,
  size: 4,
  addressingMode: 'AbsoluteLong',
  mnemonic: 'ORA'
};

export const ORA_dp: OpDef = {
  opcode: OpCode.ORA_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'ORA'
};

export const ORA_dp_ind: OpDef = {
  opcode: OpCode.ORA_dp_ind,
  size: 2,
  addressingMode: 'DirectPageIndirect',
  mnemonic: 'ORA'
};

export const ORA_dp_indl: OpDef = {
  opcode: OpCode.ORA_dp_indl,
  size: 2,
  addressingMode: 'DirectPageIndirectLong',
  mnemonic: 'ORA'
};

export const ORA_abs_x: OpDef = {
  opcode: OpCode.ORA_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'ORA'
};

export const ORA_long_x: OpDef = {
  opcode: OpCode.ORA_long_x,
  size: 4,
  addressingMode: 'AbsoluteLongIndexedX',
  mnemonic: 'ORA'
};

export const ORA_abs_y: OpDef = {
  opcode: OpCode.ORA_abs_y,
  size: 3,
  addressingMode: 'AbsoluteIndexedY',
  mnemonic: 'ORA'
};

export const ORA_dp_x: OpDef = {
  opcode: OpCode.ORA_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'ORA'
};

export const ORA_dp_x_ind: OpDef = {
  opcode: OpCode.ORA_dp_x_ind,
  size: 2,
  addressingMode: 'DirectPageIndexedIndirectX',
  mnemonic: 'ORA'
};

export const ORA_dp_ind_y: OpDef = {
  opcode: OpCode.ORA_dp_ind_y,
  size: 2,
  addressingMode: 'DirectPageIndirectIndexedY',
  mnemonic: 'ORA'
};

export const ORA_dp_indl_y: OpDef = {
  opcode: OpCode.ORA_dp_indl_y,
  size: 2,
  addressingMode: 'DirectPageIndirectLongIndexedY',
  mnemonic: 'ORA'
};

export const ORA_stkr: OpDef = {
  opcode: OpCode.ORA_stkr,
  size: 2,
  addressingMode: 'StackRelative',
  mnemonic: 'ORA'
};

export const ORA_stkr_ind_y: OpDef = {
  opcode: OpCode.ORA_stkr_ind_y,
  size: 2,
  addressingMode: 'StackRelativeIndirectIndexedY',
  mnemonic: 'ORA'
};

export const EOR_imm: OpDef = {
  opcode: OpCode.EOR_imm,
  size: 'flag-dependent',
  addressingMode: 'Immediate',
  mnemonic: 'EOR'
};

export const EOR_abs: OpDef = {
  opcode: OpCode.EOR_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'EOR'
};

export const EOR_long: OpDef = {
  opcode: OpCode.EOR_long,
  size: 4,
  addressingMode: 'AbsoluteLong',
  mnemonic: 'EOR'
};

export const EOR_dp: OpDef = {
  opcode: OpCode.EOR_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'EOR'
};

export const EOR_dp_ind: OpDef = {
  opcode: OpCode.EOR_dp_ind,
  size: 2,
  addressingMode: 'DirectPageIndirect',
  mnemonic: 'EOR'
};

export const EOR_dp_indl: OpDef = {
  opcode: OpCode.EOR_dp_indl,
  size: 2,
  addressingMode: 'DirectPageIndirectLong',
  mnemonic: 'EOR'
};

export const EOR_abs_x: OpDef = {
  opcode: OpCode.EOR_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'EOR'
};

export const EOR_long_x: OpDef = {
  opcode: OpCode.EOR_long_x,
  size: 4,
  addressingMode: 'AbsoluteLongIndexedX',
  mnemonic: 'EOR'
};

export const EOR_abs_y: OpDef = {
  opcode: OpCode.EOR_abs_y,
  size: 3,
  addressingMode: 'AbsoluteIndexedY',
  mnemonic: 'EOR'
};

export const EOR_dp_x: OpDef = {
  opcode: OpCode.EOR_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'EOR'
};

export const EOR_dp_x_ind: OpDef = {
  opcode: OpCode.EOR_dp_x_ind,
  size: 2,
  addressingMode: 'DirectPageIndexedIndirectX',
  mnemonic: 'EOR'
};

export const EOR_dp_ind_y: OpDef = {
  opcode: OpCode.EOR_dp_ind_y,
  size: 2,
  addressingMode: 'DirectPageIndirectIndexedY',
  mnemonic: 'EOR'
};

export const EOR_dp_indl_y: OpDef = {
  opcode: OpCode.EOR_dp_indl_y,
  size: 2,
  addressingMode: 'DirectPageIndirectLongIndexedY',
  mnemonic: 'EOR'
};

export const EOR_stkr: OpDef = {
  opcode: OpCode.EOR_stkr,
  size: 2,
  addressingMode: 'StackRelative',
  mnemonic: 'EOR'
};

export const EOR_stkr_ind_y: OpDef = {
  opcode: OpCode.EOR_stkr_ind_y,
  size: 2,
  addressingMode: 'StackRelativeIndirectIndexedY',
  mnemonic: 'EOR'
};

export const BIT_imm: OpDef = {
  opcode: OpCode.BIT_imm,
  size: 'flag-dependent',
  addressingMode: 'Immediate',
  mnemonic: 'BIT'
};

export const BIT_abs: OpDef = {
  opcode: OpCode.BIT_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'BIT'
};

export const BIT_dp: OpDef = {
  opcode: OpCode.BIT_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'BIT'
};

export const BIT_abs_x: OpDef = {
  opcode: OpCode.BIT_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'BIT'
};

export const BIT_dp_x: OpDef = {
  opcode: OpCode.BIT_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'BIT'
};

export const TRB_abs: OpDef = {
  opcode: OpCode.TRB_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'TRB'
};

export const TRB_dp: OpDef = {
  opcode: OpCode.TRB_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'TRB'
};

export const TSB_abs: OpDef = {
  opcode: OpCode.TSB_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'TSB'
};

export const TSB_dp: OpDef = {
  opcode: OpCode.TSB_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'TSB'
};

export const ASL_acc: OpDef = {
  opcode: OpCode.ASL_acc,
  size: 1,
  addressingMode: 'Accumulator',
  mnemonic: 'ASL'
};

export const ASL_abs: OpDef = {
  opcode: OpCode.ASL_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'ASL'
};

export const ASL_dp: OpDef = {
  opcode: OpCode.ASL_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'ASL'
};

export const ASL_abs_x: OpDef = {
  opcode: OpCode.ASL_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'ASL'
};

export const ASL_dp_x: OpDef = {
  opcode: OpCode.ASL_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'ASL'
};

export const LSR_acc: OpDef = {
  opcode: OpCode.LSR_acc,
  size: 1,
  addressingMode: 'Accumulator',
  mnemonic: 'LSR'
};

export const LSR_abs: OpDef = {
  opcode: OpCode.LSR_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'LSR'
};

export const LSR_dp: OpDef = {
  opcode: OpCode.LSR_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'LSR'
};

export const LSR_abs_x: OpDef = {
  opcode: OpCode.LSR_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'LSR'
};

export const LSR_dp_x: OpDef = {
  opcode: OpCode.LSR_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'LSR'
};

export const ROL_acc: OpDef = {
  opcode: OpCode.ROL_acc,
  size: 1,
  addressingMode: 'Accumulator',
  mnemonic: 'ROL'
};

export const ROL_abs: OpDef = {
  opcode: OpCode.ROL_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'ROL'
};

export const ROL_dp: OpDef = {
  opcode: OpCode.ROL_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'ROL'
};

export const ROL_abs_x: OpDef = {
  opcode: OpCode.ROL_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'ROL'
};

export const ROL_dp_x: OpDef = {
  opcode: OpCode.ROL_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'ROL'
};

export const ROR_acc: OpDef = {
  opcode: OpCode.ROR_acc,
  size: 1,
  addressingMode: 'Accumulator',
  mnemonic: 'ROR'
};

export const ROR_abs: OpDef = {
  opcode: OpCode.ROR_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'ROR'
};

export const ROR_dp: OpDef = {
  opcode: OpCode.ROR_dp,
  size: 2,
  addressingMode: 'DirectPage',
  mnemonic: 'ROR'
};

export const ROR_abs_x: OpDef = {
  opcode: OpCode.ROR_abs_x,
  size: 3,
  addressingMode: 'AbsoluteIndexedX',
  mnemonic: 'ROR'
};

export const ROR_dp_x: OpDef = {
  opcode: OpCode.ROR_dp_x,
  size: 2,
  addressingMode: 'DirectPageIndexedX',
  mnemonic: 'ROR'
};

export const JMP_abs: OpDef = {
  opcode: OpCode.JMP_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'JMP'
};

export const JMP_abs_ind: OpDef = {
  opcode: OpCode.JMP_abs_ind,
  size: 3,
  addressingMode: 'AbsoluteIndirect',
  mnemonic: 'JMP'
};

export const JMP_abs_x_ind: OpDef = {
  opcode: OpCode.JMP_abs_x_ind,
  size: 3,
  addressingMode: 'AbsoluteIndexedIndirect',
  mnemonic: 'JMP'
};

export const JML_long: OpDef = {
  opcode: OpCode.JML_long,
  size: 4,
  addressingMode: 'AbsoluteLong',
  mnemonic: 'JML'
};

export const JML_abs_indl: OpDef = {
  opcode: OpCode.JML_abs_indl,
  size: 3,
  addressingMode: 'AbsoluteIndirectLong',
  mnemonic: 'JML'
};

export const JSR_abs: OpDef = {
  opcode: OpCode.JSR_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'JSR'
};

export const JSR_abs_x_ind: OpDef = {
  opcode: OpCode.JSR_abs_x_ind,
  size: 3,
  addressingMode: 'AbsoluteIndexedIndirect',
  mnemonic: 'JSR'
};

export const JSL_long: OpDef = {
  opcode: OpCode.JSL_long,
  size: 4,
  addressingMode: 'AbsoluteLong',
  mnemonic: 'JSL'
};

export const RTS_stk: OpDef = {
  opcode: OpCode.RTS_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'RTS'
};

export const RTL_stk: OpDef = {
  opcode: OpCode.RTL_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'RTL'
};

export const RTI_stk: OpDef = {
  opcode: OpCode.RTI_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'RTI'
};

export const BCC_rel: OpDef = {
  opcode: OpCode.BCC_rel,
  size: 2,
  addressingMode: 'PCRelative',
  mnemonic: 'BCC'
};

export const BCS_rel: OpDef = {
  opcode: OpCode.BCS_rel,
  size: 2,
  addressingMode: 'PCRelative',
  mnemonic: 'BCS'
};

export const BEQ_rel: OpDef = {
  opcode: OpCode.BEQ_rel,
  size: 2,
  addressingMode: 'PCRelative',
  mnemonic: 'BEQ'
};

export const BNE_rel: OpDef = {
  opcode: OpCode.BNE_rel,
  size: 2,
  addressingMode: 'PCRelative',
  mnemonic: 'BNE'
};

export const BMI_rel: OpDef = {
  opcode: OpCode.BMI_rel,
  size: 2,
  addressingMode: 'PCRelative',
  mnemonic: 'BMI'
};

export const BPL_rel: OpDef = {
  opcode: OpCode.BPL_rel,
  size: 2,
  addressingMode: 'PCRelative',
  mnemonic: 'BPL'
};

export const BVC_rel: OpDef = {
  opcode: OpCode.BVC_rel,
  size: 2,
  addressingMode: 'PCRelative',
  mnemonic: 'BVC'
};

export const BVS_rel: OpDef = {
  opcode: OpCode.BVS_rel,
  size: 2,
  addressingMode: 'PCRelative',
  mnemonic: 'BVS'
};

export const BRA_rel: OpDef = {
  opcode: OpCode.BRA_rel,
  size: 2,
  addressingMode: 'PCRelative',
  mnemonic: 'BRA'
};

export const BRL_rel16: OpDef = {
  opcode: OpCode.BRL_rel16,
  size: 3,
  addressingMode: 'PCRelativeLong',
  mnemonic: 'BRL'
};

export const NOP_imp: OpDef = {
  opcode: OpCode.NOP_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'NOP'
};

export const BRK_stk_int: OpDef = {
  opcode: OpCode.BRK_stk_int,
  size: 2,
  addressingMode: 'StackInterrupt',
  mnemonic: 'BRK'
};

export const COP_stk_int: OpDef = {
  opcode: OpCode.COP_stk_int,
  size: 2,
  addressingMode: 'StackInterrupt',
  mnemonic: 'COP'
};

export const SEP_imm: OpDef = {
  opcode: OpCode.SEP_imm,
  size: 2,
  addressingMode: 'Immediate',
  mnemonic: 'SEP'
};

export const REP_imm: OpDef = {
  opcode: OpCode.REP_imm,
  size: 2,
  addressingMode: 'Immediate',
  mnemonic: 'REP'
};

export const WDM_imp: OpDef = {
  opcode: OpCode.WDM_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'WDM'
};

export const CLI_imp: OpDef = {
  opcode: OpCode.CLI_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'CLI'
};

export const SEI_imp: OpDef = {
  opcode: OpCode.SEI_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'SEI'
};

export const STP_imp: OpDef = {
  opcode: OpCode.STP_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'STP'
};

export const WAI_imp: OpDef = {
  opcode: OpCode.WAI_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'WAI'
};

export const PHA_stk: OpDef = {
  opcode: OpCode.PHA_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PHA'
};

export const PHX_stk: OpDef = {
  opcode: OpCode.PHX_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PHX'
};

export const PHY_stk: OpDef = {
  opcode: OpCode.PHY_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PHY'
};

export const PLA_stk: OpDef = {
  opcode: OpCode.PLA_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PLA'
};

export const PLX_stk: OpDef = {
  opcode: OpCode.PLX_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PLX'
};

export const PLY_stk: OpDef = {
  opcode: OpCode.PLY_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PLY'
};

export const PHB_stk: OpDef = {
  opcode: OpCode.PHB_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PHB'
};

export const PHD_stk: OpDef = {
  opcode: OpCode.PHD_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PHD'
};

export const PHK_stk: OpDef = {
  opcode: OpCode.PHK_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PHK'
};

export const PHP_stk: OpDef = {
  opcode: OpCode.PHP_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PHP'
};

export const PLB_stk: OpDef = {
  opcode: OpCode.PLB_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PLB'
};

export const PLD_stk: OpDef = {
  opcode: OpCode.PLD_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PLD'
};

export const PLP_stk: OpDef = {
  opcode: OpCode.PLP_stk,
  size: 1,
  addressingMode: 'Stack',
  mnemonic: 'PLP'
};

export const PEA_abs: OpDef = {
  opcode: OpCode.PEA_abs,
  size: 3,
  addressingMode: 'Absolute',
  mnemonic: 'PEA'
};

export const PEI_dp_ind: OpDef = {
  opcode: OpCode.PEI_dp_ind,
  size: 2,
  addressingMode: 'DirectPageIndirect',
  mnemonic: 'PEI'
};

export const PER_rel16: OpDef = {
  opcode: OpCode.PER_rel16,
  size: 3,
  addressingMode: 'PCRelativeLong',
  mnemonic: 'PER'
};

export const TAX_imp: OpDef = {
  opcode: OpCode.TAX_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'TAX'
};

export const TAY_imp: OpDef = {
  opcode: OpCode.TAY_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'TAY'
};

export const TXA_imp: OpDef = {
  opcode: OpCode.TXA_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'TXA'
};

export const TYA_imp: OpDef = {
  opcode: OpCode.TYA_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'TYA'
};

export const TSX_imp: OpDef = {
  opcode: OpCode.TSX_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'TSX'
};

export const TXS_imp: OpDef = {
  opcode: OpCode.TXS_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'TXS'
};

export const TXY_imp: OpDef = {
  opcode: OpCode.TXY_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'TXY'
};

export const TYX_imp: OpDef = {
  opcode: OpCode.TYX_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'TYX'
};

export const TCD_imp: OpDef = {
  opcode: OpCode.TCD_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'TCD'
};

export const TCS_imp: OpDef = {
  opcode: OpCode.TCS_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'TCS'
};

export const TDC_imp: OpDef = {
  opcode: OpCode.TDC_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'TDC'
};

export const TSC_imp: OpDef = {
  opcode: OpCode.TSC_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'TSC'
};

export const MVN_src_dest: OpDef = {
  opcode: OpCode.MVN_src_dest,
  size: 3,
  addressingMode: 'BlockMove',
  mnemonic: 'MVN'
};

export const MVP_src_dest: OpDef = {
  opcode: OpCode.MVP_src_dest,
  size: 3,
  addressingMode: 'BlockMove',
  mnemonic: 'MVP'
};

export const XBA_imp: OpDef = {
  opcode: OpCode.XBA_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'XBA'
};

export const XCE_imp: OpDef = {
  opcode: OpCode.XCE_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'XCE'
};

export const CLC_imp: OpDef = {
  opcode: OpCode.CLC_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'CLC'
};

export const CLD_imp: OpDef = {
  opcode: OpCode.CLD_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'CLD'
};

export const CLV_imp: OpDef = {
  opcode: OpCode.CLV_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'CLV'
};

export const SEC_imp: OpDef = {
  opcode: OpCode.SEC_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'SEC'
};

export const SED_imp: OpDef = {
  opcode: OpCode.SED_imp,
  size: 1,
  addressingMode: 'Implied',
  mnemonic: 'SED'
};

export const OpLookup = {
  [OpCode.LDA_imm]: LDA_imm,
  [OpCode.LDA_abs]: LDA_abs,
  [OpCode.LDA_long]: LDA_long,
  [OpCode.LDA_dp]: LDA_dp,
  [OpCode.LDA_dp_ind]: LDA_dp_ind,
  [OpCode.LDA_dp_indl]: LDA_dp_indl,
  [OpCode.LDA_abs_x]: LDA_abs_x,
  [OpCode.LDA_long_x]: LDA_long_x,
  [OpCode.LDA_abs_y]: LDA_abs_y,
  [OpCode.LDA_dp_x]: LDA_dp_x,
  [OpCode.LDA_dp_x_ind]: LDA_dp_x_ind,
  [OpCode.LDA_dp_ind_y]: LDA_dp_ind_y,
  [OpCode.LDA_dp_indl_y]: LDA_dp_indl_y,
  [OpCode.LDA_stkr]: LDA_stkr,
  [OpCode.LDA_stkr_ind_y]: LDA_stkr_ind_y,
  [OpCode.LDX_imm]: LDX_imm,
  [OpCode.LDX_abs]: LDX_abs,
  [OpCode.LDX_dp]: LDX_dp,
  [OpCode.LDX_abs_y]: LDX_abs_y,
  [OpCode.LDX_dp_y]: LDX_dp_y,
  [OpCode.LDY_imm]: LDY_imm,
  [OpCode.LDY_abs]: LDY_abs,
  [OpCode.LDY_dp]: LDY_dp,
  [OpCode.LDY_abs_x]: LDY_abs_x,
  [OpCode.LDY_dp_x]: LDY_dp_x,
  [OpCode.STA_abs]: STA_abs,
  [OpCode.STA_long]: STA_long,
  [OpCode.STA_dp]: STA_dp,
  [OpCode.STA_dp_ind]: STA_dp_ind,
  [OpCode.STA_dp_indl]: STA_dp_indl,
  [OpCode.STA_abs_x]: STA_abs_x,
  [OpCode.STA_long_x]: STA_long_x,
  [OpCode.STA_abs_y]: STA_abs_y,
  [OpCode.STA_dp_x]: STA_dp_x,
  [OpCode.STA_dp_x_ind]: STA_dp_x_ind,
  [OpCode.STA_dp_ind_y]: STA_dp_ind_y,
  [OpCode.STA_dp_indl_y]: STA_dp_indl_y,
  [OpCode.STA_stkr]: STA_stkr,
  [OpCode.STA_stkr_ind_y]: STA_stkr_ind_y,
  [OpCode.STX_abs]: STX_abs,
  [OpCode.STX_dp]: STX_dp,
  [OpCode.STX_dp_y]: STX_dp_y,
  [OpCode.STY_abs]: STY_abs,
  [OpCode.STY_dp]: STY_dp,
  [OpCode.STY_dp_x]: STY_dp_x,
  [OpCode.STZ_abs]: STZ_abs,
  [OpCode.STZ_dp]: STZ_dp,
  [OpCode.STZ_abs_x]: STZ_abs_x,
  [OpCode.STZ_dp_x]: STZ_dp_x,
  [OpCode.ADC_imm]: ADC_imm,
  [OpCode.ADC_abs]: ADC_abs,
  [OpCode.ADC_long]: ADC_long,
  [OpCode.ADC_dp]: ADC_dp,
  [OpCode.ADC_dp_ind]: ADC_dp_ind,
  [OpCode.ADC_dp_indl]: ADC_dp_indl,
  [OpCode.ADC_abs_x]: ADC_abs_x,
  [OpCode.ADC_long_x]: ADC_long_x,
  [OpCode.ADC_abs_y]: ADC_abs_y,
  [OpCode.ADC_dp_x]: ADC_dp_x,
  [OpCode.ADC_dp_x_ind]: ADC_dp_x_ind,
  [OpCode.ADC_dp_ind_y]: ADC_dp_ind_y,
  [OpCode.ADC_dp_indl_y]: ADC_dp_indl_y,
  [OpCode.ADC_stkr]: ADC_stkr,
  [OpCode.ADC_stkr_ind_y]: ADC_stkr_ind_y,
  [OpCode.SBC_imm]: SBC_imm,
  [OpCode.SBC_abs]: SBC_abs,
  [OpCode.SBC_long]: SBC_long,
  [OpCode.SBC_dp]: SBC_dp,
  [OpCode.SBC_dp_ind]: SBC_dp_ind,
  [OpCode.SBC_dp_indl]: SBC_dp_indl,
  [OpCode.SBC_abs_x]: SBC_abs_x,
  [OpCode.SBC_long_x]: SBC_long_x,
  [OpCode.SBC_abs_y]: SBC_abs_y,
  [OpCode.SBC_dp_x]: SBC_dp_x,
  [OpCode.SBC_dp_x_ind]: SBC_dp_x_ind,
  [OpCode.SBC_dp_ind_y]: SBC_dp_ind_y,
  [OpCode.SBC_dp_indl_y]: SBC_dp_indl_y,
  [OpCode.SBC_stkr]: SBC_stkr,
  [OpCode.SBC_stkr_ind_y]: SBC_stkr_ind_y,
  [OpCode.CMP_imm]: CMP_imm,
  [OpCode.CMP_abs]: CMP_abs,
  [OpCode.CMP_long]: CMP_long,
  [OpCode.CMP_dp]: CMP_dp,
  [OpCode.CMP_dp_ind]: CMP_dp_ind,
  [OpCode.CMP_dp_indl]: CMP_dp_indl,
  [OpCode.CMP_abs_x]: CMP_abs_x,
  [OpCode.CMP_long_x]: CMP_long_x,
  [OpCode.CMP_abs_y]: CMP_abs_y,
  [OpCode.CMP_dp_x]: CMP_dp_x,
  [OpCode.CMP_dp_x_ind]: CMP_dp_x_ind,
  [OpCode.CMP_dp_ind_y]: CMP_dp_ind_y,
  [OpCode.CMP_dp_indl_y]: CMP_dp_indl_y,
  [OpCode.CMP_stkr]: CMP_stkr,
  [OpCode.CMP_stkr_ind_y]: CMP_stkr_ind_y,
  [OpCode.CPX_imm]: CPX_imm,
  [OpCode.CPX_abs]: CPX_abs,
  [OpCode.CPX_dp]: CPX_dp,
  [OpCode.CPY_imm]: CPY_imm,
  [OpCode.CPY_abs]: CPY_abs,
  [OpCode.CPY_dp]: CPY_dp,
  [OpCode.INC_acc]: INC_acc,
  [OpCode.INC_abs]: INC_abs,
  [OpCode.INC_dp]: INC_dp,
  [OpCode.INC_abs_x]: INC_abs_x,
  [OpCode.INC_dp_x]: INC_dp_x,
  [OpCode.DEC_acc]: DEC_acc,
  [OpCode.DEC_abs]: DEC_abs,
  [OpCode.DEC_dp]: DEC_dp,
  [OpCode.DEC_abs_x]: DEC_abs_x,
  [OpCode.DEC_dp_x]: DEC_dp_x,
  [OpCode.INX_imp]: INX_imp,
  [OpCode.INY_imp]: INY_imp,
  [OpCode.DEX_imp]: DEX_imp,
  [OpCode.DEY_imp]: DEY_imp,
  [OpCode.AND_imm]: AND_imm,
  [OpCode.AND_abs]: AND_abs,
  [OpCode.AND_long]: AND_long,
  [OpCode.AND_dp]: AND_dp,
  [OpCode.AND_dp_ind]: AND_dp_ind,
  [OpCode.AND_dp_indl]: AND_dp_indl,
  [OpCode.AND_abs_x]: AND_abs_x,
  [OpCode.AND_long_x]: AND_long_x,
  [OpCode.AND_abs_y]: AND_abs_y,
  [OpCode.AND_dp_x]: AND_dp_x,
  [OpCode.AND_dp_x_ind]: AND_dp_x_ind,
  [OpCode.AND_dp_ind_y]: AND_dp_ind_y,
  [OpCode.AND_dp_indl_y]: AND_dp_indl_y,
  [OpCode.AND_stkr]: AND_stkr,
  [OpCode.AND_stkr_ind_y]: AND_stkr_ind_y,
  [OpCode.ORA_imm]: ORA_imm,
  [OpCode.ORA_abs]: ORA_abs,
  [OpCode.ORA_long]: ORA_long,
  [OpCode.ORA_dp]: ORA_dp,
  [OpCode.ORA_dp_ind]: ORA_dp_ind,
  [OpCode.ORA_dp_indl]: ORA_dp_indl,
  [OpCode.ORA_abs_x]: ORA_abs_x,
  [OpCode.ORA_long_x]: ORA_long_x,
  [OpCode.ORA_abs_y]: ORA_abs_y,
  [OpCode.ORA_dp_x]: ORA_dp_x,
  [OpCode.ORA_dp_x_ind]: ORA_dp_x_ind,
  [OpCode.ORA_dp_ind_y]: ORA_dp_ind_y,
  [OpCode.ORA_dp_indl_y]: ORA_dp_indl_y,
  [OpCode.ORA_stkr]: ORA_stkr,
  [OpCode.ORA_stkr_ind_y]: ORA_stkr_ind_y,
  [OpCode.EOR_imm]: EOR_imm,
  [OpCode.EOR_abs]: EOR_abs,
  [OpCode.EOR_long]: EOR_long,
  [OpCode.EOR_dp]: EOR_dp,
  [OpCode.EOR_dp_ind]: EOR_dp_ind,
  [OpCode.EOR_dp_indl]: EOR_dp_indl,
  [OpCode.EOR_abs_x]: EOR_abs_x,
  [OpCode.EOR_long_x]: EOR_long_x,
  [OpCode.EOR_abs_y]: EOR_abs_y,
  [OpCode.EOR_dp_x]: EOR_dp_x,
  [OpCode.EOR_dp_x_ind]: EOR_dp_x_ind,
  [OpCode.EOR_dp_ind_y]: EOR_dp_ind_y,
  [OpCode.EOR_dp_indl_y]: EOR_dp_indl_y,
  [OpCode.EOR_stkr]: EOR_stkr,
  [OpCode.EOR_stkr_ind_y]: EOR_stkr_ind_y,
  [OpCode.BIT_imm]: BIT_imm,
  [OpCode.BIT_abs]: BIT_abs,
  [OpCode.BIT_dp]: BIT_dp,
  [OpCode.BIT_abs_x]: BIT_abs_x,
  [OpCode.BIT_dp_x]: BIT_dp_x,
  [OpCode.TRB_abs]: TRB_abs,
  [OpCode.TRB_dp]: TRB_dp,
  [OpCode.TSB_abs]: TSB_abs,
  [OpCode.TSB_dp]: TSB_dp,
  [OpCode.ASL_acc]: ASL_acc,
  [OpCode.ASL_abs]: ASL_abs,
  [OpCode.ASL_dp]: ASL_dp,
  [OpCode.ASL_abs_x]: ASL_abs_x,
  [OpCode.ASL_dp_x]: ASL_dp_x,
  [OpCode.LSR_acc]: LSR_acc,
  [OpCode.LSR_abs]: LSR_abs,
  [OpCode.LSR_dp]: LSR_dp,
  [OpCode.LSR_abs_x]: LSR_abs_x,
  [OpCode.LSR_dp_x]: LSR_dp_x,
  [OpCode.ROL_acc]: ROL_acc,
  [OpCode.ROL_abs]: ROL_abs,
  [OpCode.ROL_dp]: ROL_dp,
  [OpCode.ROL_abs_x]: ROL_abs_x,
  [OpCode.ROL_dp_x]: ROL_dp_x,
  [OpCode.ROR_acc]: ROR_acc,
  [OpCode.ROR_abs]: ROR_abs,
  [OpCode.ROR_dp]: ROR_dp,
  [OpCode.ROR_abs_x]: ROR_abs_x,
  [OpCode.ROR_dp_x]: ROR_dp_x,
  [OpCode.JMP_abs]: JMP_abs,
  [OpCode.JMP_abs_ind]: JMP_abs_ind,
  [OpCode.JMP_abs_x_ind]: JMP_abs_x_ind,
  [OpCode.JML_long]: JML_long,
  [OpCode.JML_abs_indl]: JML_abs_indl,
  [OpCode.JSR_abs]: JSR_abs,
  [OpCode.JSR_abs_x_ind]: JSR_abs_x_ind,
  [OpCode.JSL_long]: JSL_long,
  [OpCode.RTS_stk]: RTS_stk,
  [OpCode.RTL_stk]: RTL_stk,
  [OpCode.RTI_stk]: RTI_stk,
  [OpCode.BCC_rel]: BCC_rel,
  [OpCode.BCS_rel]: BCS_rel,
  [OpCode.BEQ_rel]: BEQ_rel,
  [OpCode.BNE_rel]: BNE_rel,
  [OpCode.BMI_rel]: BMI_rel,
  [OpCode.BPL_rel]: BPL_rel,
  [OpCode.BVC_rel]: BVC_rel,
  [OpCode.BVS_rel]: BVS_rel,
  [OpCode.BRA_rel]: BRA_rel,
  [OpCode.BRL_rel16]: BRL_rel16,
  [OpCode.NOP_imp]: NOP_imp,
  [OpCode.BRK_stk_int]: BRK_stk_int,
  [OpCode.COP_stk_int]: COP_stk_int,
  [OpCode.SEP_imm]: SEP_imm,
  [OpCode.REP_imm]: REP_imm,
  [OpCode.WDM_imp]: WDM_imp,
  [OpCode.CLI_imp]: CLI_imp,
  [OpCode.SEI_imp]: SEI_imp,
  [OpCode.STP_imp]: STP_imp,
  [OpCode.WAI_imp]: WAI_imp,
  [OpCode.PHA_stk]: PHA_stk,
  [OpCode.PHX_stk]: PHX_stk,
  [OpCode.PHY_stk]: PHY_stk,
  [OpCode.PLA_stk]: PLA_stk,
  [OpCode.PLX_stk]: PLX_stk,
  [OpCode.PLY_stk]: PLY_stk,
  [OpCode.PHB_stk]: PHB_stk,
  [OpCode.PHD_stk]: PHD_stk,
  [OpCode.PHK_stk]: PHK_stk,
  [OpCode.PHP_stk]: PHP_stk,
  [OpCode.PLB_stk]: PLB_stk,
  [OpCode.PLD_stk]: PLD_stk,
  [OpCode.PLP_stk]: PLP_stk,
  [OpCode.PEA_abs]: PEA_abs,
  [OpCode.PEI_dp_ind]: PEI_dp_ind,
  [OpCode.PER_rel16]: PER_rel16,
  [OpCode.TAX_imp]: TAX_imp,
  [OpCode.TAY_imp]: TAY_imp,
  [OpCode.TXA_imp]: TXA_imp,
  [OpCode.TYA_imp]: TYA_imp,
  [OpCode.TSX_imp]: TSX_imp,
  [OpCode.TXS_imp]: TXS_imp,
  [OpCode.TXY_imp]: TXY_imp,
  [OpCode.TYX_imp]: TYX_imp,
  [OpCode.TCD_imp]: TCD_imp,
  [OpCode.TCS_imp]: TCS_imp,
  [OpCode.TDC_imp]: TDC_imp,
  [OpCode.TSC_imp]: TSC_imp,
  [OpCode.MVN_src_dest]: MVN_src_dest,
  [OpCode.MVP_src_dest]: MVP_src_dest,
  [OpCode.XBA_imp]: XBA_imp,
  [OpCode.XCE_imp]: XCE_imp,
  [OpCode.CLC_imp]: CLC_imp,
  [OpCode.CLD_imp]: CLD_imp,
  [OpCode.CLV_imp]: CLV_imp,
  [OpCode.SEC_imp]: SEC_imp,
  [OpCode.SED_imp]: SED_imp
};