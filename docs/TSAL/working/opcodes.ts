// AUTO-GENERATED - DO NOT EDIT
// Generated from data source at 2025-07-13T08:29:11.741Z

import { AddressingMode, Op, Operand, Instruction, Byte, Word, Direct, Absolute, AbsoluteLong, IndexMode } from './platform';

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
}

function opDef(code: OpCode, mode: AddressingMode, size: number): Op {
  return { code, mode, size };
}

export const OpDef = {
  LDA: {
    imm: opDef(OpCode.LDA_imm, AddressingMode.Immediate, 2),
    abs: opDef(OpCode.LDA_abs, AddressingMode.Absolute, 3),
    long: opDef(OpCode.LDA_long, AddressingMode.AbsoluteLong, 4),
    dp: opDef(OpCode.LDA_dp, AddressingMode.DirectPage, 2),
    dp_ind: opDef(OpCode.LDA_dp_ind, AddressingMode.DirectPageIndirect, 2),
    dp_indl: opDef(OpCode.LDA_dp_indl, AddressingMode.DirectPageIndirectLong, 2),
    abs_x: opDef(OpCode.LDA_abs_x, AddressingMode.AbsoluteIndexedX, 3),
    long_x: opDef(OpCode.LDA_long_x, AddressingMode.AbsoluteLongIndexedX, 4),
    abs_y: opDef(OpCode.LDA_abs_y, AddressingMode.AbsoluteIndexedY, 3),
    dp_x: opDef(OpCode.LDA_dp_x, AddressingMode.DirectPageIndexedX, 2),
    dp_x_ind: opDef(OpCode.LDA_dp_x_ind, AddressingMode.DirectPageIndexedIndirectX, 2),
    dp_ind_y: opDef(OpCode.LDA_dp_ind_y, AddressingMode.DirectPageIndirectIndexedY, 2),
    dp_indl_y: opDef(OpCode.LDA_dp_indl_y, AddressingMode.DirectPageIndirectLongIndexedY, 2),
    stkr: opDef(OpCode.LDA_stkr, AddressingMode.StackRelative, 2),
    stkr_ind_y: opDef(OpCode.LDA_stkr_ind_y, AddressingMode.StackRelativeIndirectIndexedY, 2),
  },
  LDX: {
    imm: opDef(OpCode.LDX_imm, AddressingMode.Immediate, 2),
    abs: opDef(OpCode.LDX_abs, AddressingMode.Absolute, 3),
    dp: opDef(OpCode.LDX_dp, AddressingMode.DirectPage, 2),
    abs_y: opDef(OpCode.LDX_abs_y, AddressingMode.AbsoluteIndexedY, 3),
    dp_y: opDef(OpCode.LDX_dp_y, AddressingMode.DirectPageIndexedY, 2),
  },
  LDY: {
    imm: opDef(OpCode.LDY_imm, AddressingMode.Immediate, 2),
    abs: opDef(OpCode.LDY_abs, AddressingMode.Absolute, 3),
    dp: opDef(OpCode.LDY_dp, AddressingMode.DirectPage, 2),
    abs_x: opDef(OpCode.LDY_abs_x, AddressingMode.AbsoluteIndexedX, 3),
    dp_x: opDef(OpCode.LDY_dp_x, AddressingMode.DirectPageIndexedX, 2),
  },
  STA: {
    abs: opDef(OpCode.STA_abs, AddressingMode.Absolute, 3),
    long: opDef(OpCode.STA_long, AddressingMode.AbsoluteLong, 4),
    dp: opDef(OpCode.STA_dp, AddressingMode.DirectPage, 2),
    dp_ind: opDef(OpCode.STA_dp_ind, AddressingMode.DirectPageIndirect, 2),
    dp_indl: opDef(OpCode.STA_dp_indl, AddressingMode.DirectPageIndirectLong, 2),
    abs_x: opDef(OpCode.STA_abs_x, AddressingMode.AbsoluteIndexedX, 3),
    long_x: opDef(OpCode.STA_long_x, AddressingMode.AbsoluteLongIndexedX, 4),
    abs_y: opDef(OpCode.STA_abs_y, AddressingMode.AbsoluteIndexedY, 3),
    dp_x: opDef(OpCode.STA_dp_x, AddressingMode.DirectPageIndexedX, 2),
    dp_x_ind: opDef(OpCode.STA_dp_x_ind, AddressingMode.DirectPageIndexedIndirectX, 2),
    dp_ind_y: opDef(OpCode.STA_dp_ind_y, AddressingMode.DirectPageIndirectIndexedY, 2),
    dp_indl_y: opDef(OpCode.STA_dp_indl_y, AddressingMode.DirectPageIndirectLongIndexedY, 2),
    stkr: opDef(OpCode.STA_stkr, AddressingMode.StackRelative, 2),
    stkr_ind_y: opDef(OpCode.STA_stkr_ind_y, AddressingMode.StackRelativeIndirectIndexedY, 2),
  },
  STX: {
    abs: opDef(OpCode.STX_abs, AddressingMode.Absolute, 3),
    dp: opDef(OpCode.STX_dp, AddressingMode.DirectPage, 2),
    dp_y: opDef(OpCode.STX_dp_y, AddressingMode.DirectPageIndexedY, 2),
  },
  STY: {
    abs: opDef(OpCode.STY_abs, AddressingMode.Absolute, 3),
    dp: opDef(OpCode.STY_dp, AddressingMode.DirectPage, 2),
    dp_x: opDef(OpCode.STY_dp_x, AddressingMode.DirectPageIndexedX, 2),
  },
  STZ: {
    abs: opDef(OpCode.STZ_abs, AddressingMode.Absolute, 3),
    dp: opDef(OpCode.STZ_dp, AddressingMode.DirectPage, 2),
    abs_x: opDef(OpCode.STZ_abs_x, AddressingMode.AbsoluteIndexedX, 3),
    dp_x: opDef(OpCode.STZ_dp_x, AddressingMode.DirectPageIndexedX, 2),
  },
};

export const OpLookup: Record<OpCode, Op> = {
  [OpCode.LDA_imm]: OpDef.LDA.imm,
  [OpCode.LDA_abs]: OpDef.LDA.abs,
  [OpCode.LDA_long]: OpDef.LDA.long,
  [OpCode.LDA_dp]: OpDef.LDA.dp,
  [OpCode.LDA_dp_ind]: OpDef.LDA.dp_ind,
  [OpCode.LDA_dp_indl]: OpDef.LDA.dp_indl,
  [OpCode.LDA_abs_x]: OpDef.LDA.abs_x,
  [OpCode.LDA_long_x]: OpDef.LDA.long_x,
  [OpCode.LDA_abs_y]: OpDef.LDA.abs_y,
  [OpCode.LDA_dp_x]: OpDef.LDA.dp_x,
  [OpCode.LDA_dp_x_ind]: OpDef.LDA.dp_x_ind,
  [OpCode.LDA_dp_ind_y]: OpDef.LDA.dp_ind_y,
  [OpCode.LDA_dp_indl_y]: OpDef.LDA.dp_indl_y,
  [OpCode.LDA_stkr]: OpDef.LDA.stkr,
  [OpCode.LDA_stkr_ind_y]: OpDef.LDA.stkr_ind_y,
  [OpCode.LDX_imm]: OpDef.LDX.imm,
  [OpCode.LDX_abs]: OpDef.LDX.abs,
  [OpCode.LDX_dp]: OpDef.LDX.dp,
  [OpCode.LDX_abs_y]: OpDef.LDX.abs_y,
  [OpCode.LDX_dp_y]: OpDef.LDX.dp_y,
  [OpCode.LDY_imm]: OpDef.LDY.imm,
  [OpCode.LDY_abs]: OpDef.LDY.abs,
  [OpCode.LDY_dp]: OpDef.LDY.dp,
  [OpCode.LDY_abs_x]: OpDef.LDY.abs_x,
  [OpCode.LDY_dp_x]: OpDef.LDY.dp_x,
  [OpCode.STA_abs]: OpDef.STA.abs,
  [OpCode.STA_long]: OpDef.STA.long,
  [OpCode.STA_dp]: OpDef.STA.dp,
  [OpCode.STA_dp_ind]: OpDef.STA.dp_ind,
  [OpCode.STA_dp_indl]: OpDef.STA.dp_indl,
  [OpCode.STA_abs_x]: OpDef.STA.abs_x,
  [OpCode.STA_long_x]: OpDef.STA.long_x,
  [OpCode.STA_abs_y]: OpDef.STA.abs_y,
  [OpCode.STA_dp_x]: OpDef.STA.dp_x,
  [OpCode.STA_dp_x_ind]: OpDef.STA.dp_x_ind,
  [OpCode.STA_dp_ind_y]: OpDef.STA.dp_ind_y,
  [OpCode.STA_dp_indl_y]: OpDef.STA.dp_indl_y,
  [OpCode.STA_stkr]: OpDef.STA.stkr,
  [OpCode.STA_stkr_ind_y]: OpDef.STA.stkr_ind_y,
  [OpCode.STX_abs]: OpDef.STX.abs,
  [OpCode.STX_dp]: OpDef.STX.dp,
  [OpCode.STX_dp_y]: OpDef.STX.dp_y,
  [OpCode.STY_abs]: OpDef.STY.abs,
  [OpCode.STY_dp]: OpDef.STY.dp,
  [OpCode.STY_dp_x]: OpDef.STY.dp_x,
  [OpCode.STZ_abs]: OpDef.STZ.abs,
  [OpCode.STZ_dp]: OpDef.STZ.dp,
  [OpCode.STZ_abs_x]: OpDef.STZ.abs_x,
  [OpCode.STZ_dp_x]: OpDef.STZ.dp_x,
};

type LDAFactory = {
  (op: Operand, index?: IndexMode): Instruction;

  imm: (val: Byte | Word) => Instruction;
  abs: (addr: Absolute) => Instruction;
  long: (addr: AbsoluteLong) => Instruction;
  dp: (addr: Direct) => Instruction;
  dp_ind: (addr: Direct) => Instruction;
  dp_indl: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  long_x: (addr: AbsoluteLong) => Instruction;
  abs_y: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
  dp_x_ind: (addr: Direct) => Instruction;
  dp_ind_y: (addr: Direct) => Instruction;
  dp_indl_y: (addr: Direct) => Instruction;
  stkr: (val: Byte) => Instruction;
  stkr_ind_y: (val: Byte) => Instruction;
};

type LDXFactory = {
  (op: Operand, index?: IndexMode): Instruction;

  imm: (val: Byte | Word) => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_y: (addr: Absolute) => Instruction;
  dp_y: (addr: Direct) => Instruction;
};

type LDYFactory = {
  (op: Operand, index?: IndexMode): Instruction;

  imm: (val: Byte | Word) => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
};

type STAFactory = {
  (op: Operand, index?: IndexMode): Instruction;

  abs: (addr: Absolute) => Instruction;
  long: (addr: AbsoluteLong) => Instruction;
  dp: (addr: Direct) => Instruction;
  dp_ind: (addr: Direct) => Instruction;
  dp_indl: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  long_x: (addr: AbsoluteLong) => Instruction;
  abs_y: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
  dp_x_ind: (addr: Direct) => Instruction;
  dp_ind_y: (addr: Direct) => Instruction;
  dp_indl_y: (addr: Direct) => Instruction;
  stkr: (val: Byte) => Instruction;
  stkr_ind_y: (val: Byte) => Instruction;
};

type STXFactory = {
  (op: Operand, index?: IndexMode): Instruction;

  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  dp_y: (addr: Direct) => Instruction;
};

type STYFactory = {
  (op: Operand, index?: IndexMode): Instruction;

  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  dp_x: (addr: Direct) => Instruction;
};

type STZFactory = {
  (op: Operand, index?: IndexMode): Instruction;

  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
};

export const LDA: LDAFactory = Object.assign(
  (op: Operand, index?: IndexMode): Instruction => {
    // Smart routing logic based on operand type and index mode
    // ... (implementation details)
    throw new Error('Not implemented yet');
  },
  {
  imm: (val: Byte | Word) => new Instruction(OpCode.LDA_imm, val),
  abs: (addr: Absolute) => new Instruction(OpCode.LDA_abs, addr),
  long: (addr: AbsoluteLong) => new Instruction(OpCode.LDA_long, addr),
  dp: (addr: Direct) => new Instruction(OpCode.LDA_dp, addr),
  dp_ind: (addr: Direct) => new Instruction(OpCode.LDA_dp_ind, addr),
  dp_indl: (addr: Direct) => new Instruction(OpCode.LDA_dp_indl, addr),
  abs_x: (addr: Absolute) => new Instruction(OpCode.LDA_abs_x, addr),
  long_x: (addr: AbsoluteLong) => new Instruction(OpCode.LDA_long_x, addr),
  abs_y: (addr: Absolute) => new Instruction(OpCode.LDA_abs_y, addr),
  dp_x: (addr: Direct) => new Instruction(OpCode.LDA_dp_x, addr),
  dp_x_ind: (addr: Direct) => new Instruction(OpCode.LDA_dp_x_ind, addr),
  dp_ind_y: (addr: Direct) => new Instruction(OpCode.LDA_dp_ind_y, addr),
  dp_indl_y: (addr: Direct) => new Instruction(OpCode.LDA_dp_indl_y, addr),
  stkr: (val: Byte) => new Instruction(OpCode.LDA_stkr, val),
  stkr_ind_y: (val: Byte) => new Instruction(OpCode.LDA_stkr_ind_y, val),
  }
);

export const LDX: LDXFactory = Object.assign(
  (op: Operand, index?: IndexMode): Instruction => {
    // Smart routing logic based on operand type and index mode
    // ... (implementation details)
    throw new Error('Not implemented yet');
  },
  {
  imm: (val: Byte | Word) => new Instruction(OpCode.LDX_imm, val),
  abs: (addr: Absolute) => new Instruction(OpCode.LDX_abs, addr),
  dp: (addr: Direct) => new Instruction(OpCode.LDX_dp, addr),
  abs_y: (addr: Absolute) => new Instruction(OpCode.LDX_abs_y, addr),
  dp_y: (addr: Direct) => new Instruction(OpCode.LDX_dp_y, addr),
  }
);

export const LDY: LDYFactory = Object.assign(
  (op: Operand, index?: IndexMode): Instruction => {
    // Smart routing logic based on operand type and index mode
    // ... (implementation details)
    throw new Error('Not implemented yet');
  },
  {
  imm: (val: Byte | Word) => new Instruction(OpCode.LDY_imm, val),
  abs: (addr: Absolute) => new Instruction(OpCode.LDY_abs, addr),
  dp: (addr: Direct) => new Instruction(OpCode.LDY_dp, addr),
  abs_x: (addr: Absolute) => new Instruction(OpCode.LDY_abs_x, addr),
  dp_x: (addr: Direct) => new Instruction(OpCode.LDY_dp_x, addr),
  }
);

export const STA: STAFactory = Object.assign(
  (op: Operand, index?: IndexMode): Instruction => {
    // Smart routing logic based on operand type and index mode
    // ... (implementation details)
    throw new Error('Not implemented yet');
  },
  {
  abs: (addr: Absolute) => new Instruction(OpCode.STA_abs, addr),
  long: (addr: AbsoluteLong) => new Instruction(OpCode.STA_long, addr),
  dp: (addr: Direct) => new Instruction(OpCode.STA_dp, addr),
  dp_ind: (addr: Direct) => new Instruction(OpCode.STA_dp_ind, addr),
  dp_indl: (addr: Direct) => new Instruction(OpCode.STA_dp_indl, addr),
  abs_x: (addr: Absolute) => new Instruction(OpCode.STA_abs_x, addr),
  long_x: (addr: AbsoluteLong) => new Instruction(OpCode.STA_long_x, addr),
  abs_y: (addr: Absolute) => new Instruction(OpCode.STA_abs_y, addr),
  dp_x: (addr: Direct) => new Instruction(OpCode.STA_dp_x, addr),
  dp_x_ind: (addr: Direct) => new Instruction(OpCode.STA_dp_x_ind, addr),
  dp_ind_y: (addr: Direct) => new Instruction(OpCode.STA_dp_ind_y, addr),
  dp_indl_y: (addr: Direct) => new Instruction(OpCode.STA_dp_indl_y, addr),
  stkr: (val: Byte) => new Instruction(OpCode.STA_stkr, val),
  stkr_ind_y: (val: Byte) => new Instruction(OpCode.STA_stkr_ind_y, val),
  }
);

export const STX: STXFactory = Object.assign(
  (op: Operand, index?: IndexMode): Instruction => {
    // Smart routing logic based on operand type and index mode
    // ... (implementation details)
    throw new Error('Not implemented yet');
  },
  {
  abs: (addr: Absolute) => new Instruction(OpCode.STX_abs, addr),
  dp: (addr: Direct) => new Instruction(OpCode.STX_dp, addr),
  dp_y: (addr: Direct) => new Instruction(OpCode.STX_dp_y, addr),
  }
);

export const STY: STYFactory = Object.assign(
  (op: Operand, index?: IndexMode): Instruction => {
    // Smart routing logic based on operand type and index mode
    // ... (implementation details)
    throw new Error('Not implemented yet');
  },
  {
  abs: (addr: Absolute) => new Instruction(OpCode.STY_abs, addr),
  dp: (addr: Direct) => new Instruction(OpCode.STY_dp, addr),
  dp_x: (addr: Direct) => new Instruction(OpCode.STY_dp_x, addr),
  }
);

export const STZ: STZFactory = Object.assign(
  (op: Operand, index?: IndexMode): Instruction => {
    // Smart routing logic based on operand type and index mode
    // ... (implementation details)
    throw new Error('Not implemented yet');
  },
  {
  abs: (addr: Absolute) => new Instruction(OpCode.STZ_abs, addr),
  dp: (addr: Direct) => new Instruction(OpCode.STZ_dp, addr),
  abs_x: (addr: Absolute) => new Instruction(OpCode.STZ_abs_x, addr),
  dp_x: (addr: Direct) => new Instruction(OpCode.STZ_dp_x, addr),
  }
);
