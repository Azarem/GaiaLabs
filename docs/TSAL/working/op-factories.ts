import { Instruction } from './op';
import * as defs from './generated-opcodes';
import { Byte, Word, Absolute, AbsoluteLong, Direct, Label, Immediate, BankPair, InterruptVector } from './generated-opcodes';

// === SINGLE-VARIANT INSTRUCTIONS (Simple Functions) ===

/**
 * INX - Implied mode
 * Creates a new INX instruction with no operands
 */
export function INX(): Instruction {
  return new Instruction(defs.INX_imp);
}

/**
 * INY - Implied mode
 * Creates a new INY instruction with no operands
 */
export function INY(): Instruction {
  return new Instruction(defs.INY_imp);
}

/**
 * DEX - Implied mode
 * Creates a new DEX instruction with no operands
 */
export function DEX(): Instruction {
  return new Instruction(defs.DEX_imp);
}

/**
 * DEY - Implied mode
 * Creates a new DEY instruction with no operands
 */
export function DEY(): Instruction {
  return new Instruction(defs.DEY_imp);
}

/**
 * JSL - AbsoluteLong mode
 * Creates a new JSL instruction with the specified operand
 */
export function JSL(addr: AbsoluteLong): Instruction {
  return new Instruction(defs.JSL_long, addr);
}

/**
 * RTS - Stack mode
 * Creates a new RTS instruction with no operands
 */
export function RTS(): Instruction {
  return new Instruction(defs.RTS_stk);
}

/**
 * RTL - Stack mode
 * Creates a new RTL instruction with no operands
 */
export function RTL(): Instruction {
  return new Instruction(defs.RTL_stk);
}

/**
 * RTI - Stack mode
 * Creates a new RTI instruction with no operands
 */
export function RTI(): Instruction {
  return new Instruction(defs.RTI_stk);
}

/**
 * BCC - PCRelative mode
 * Creates a new BCC instruction with the specified operand
 */
export function BCC(target: Label): Instruction {
  return new Instruction(defs.BCC_rel, target);
}

/**
 * BCS - PCRelative mode
 * Creates a new BCS instruction with the specified operand
 */
export function BCS(target: Label): Instruction {
  return new Instruction(defs.BCS_rel, target);
}

/**
 * BEQ - PCRelative mode
 * Creates a new BEQ instruction with the specified operand
 */
export function BEQ(target: Label): Instruction {
  return new Instruction(defs.BEQ_rel, target);
}

/**
 * BNE - PCRelative mode
 * Creates a new BNE instruction with the specified operand
 */
export function BNE(target: Label): Instruction {
  return new Instruction(defs.BNE_rel, target);
}

/**
 * BMI - PCRelative mode
 * Creates a new BMI instruction with the specified operand
 */
export function BMI(target: Label): Instruction {
  return new Instruction(defs.BMI_rel, target);
}

/**
 * BPL - PCRelative mode
 * Creates a new BPL instruction with the specified operand
 */
export function BPL(target: Label): Instruction {
  return new Instruction(defs.BPL_rel, target);
}

/**
 * BVC - PCRelative mode
 * Creates a new BVC instruction with the specified operand
 */
export function BVC(target: Label): Instruction {
  return new Instruction(defs.BVC_rel, target);
}

/**
 * BVS - PCRelative mode
 * Creates a new BVS instruction with the specified operand
 */
export function BVS(target: Label): Instruction {
  return new Instruction(defs.BVS_rel, target);
}

/**
 * BRA - PCRelative mode
 * Creates a new BRA instruction with the specified operand
 */
export function BRA(target: Label): Instruction {
  return new Instruction(defs.BRA_rel, target);
}

/**
 * BRL - PCRelativeLong mode
 * Creates a new BRL instruction with the specified operand
 */
export function BRL(target: Label): Instruction {
  return new Instruction(defs.BRL_rel16, target);
}

/**
 * NOP - Implied mode
 * Creates a new NOP instruction with no operands
 */
export function NOP(): Instruction {
  return new Instruction(defs.NOP_imp);
}

/**
 * BRK - StackInterrupt mode
 * Creates a new BRK instruction with the specified operand
 */
export function BRK(vector: Byte): Instruction {
  return new Instruction(defs.BRK_stk_int, vector);
}

/**
 * COP - StackInterrupt mode
 * Creates a new COP instruction with the specified operand
 */
export function COP(vector: Byte): Instruction {
  return new Instruction(defs.COP_stk_int, vector);
}

/**
 * SEP - Immediate mode
 * Creates a new SEP instruction with the specified operand
 */
export function SEP(val: Byte | Word): Instruction {
  return new Instruction(defs.SEP_imm, val);
}

/**
 * REP - Immediate mode
 * Creates a new REP instruction with the specified operand
 */
export function REP(val: Byte | Word): Instruction {
  return new Instruction(defs.REP_imm, val);
}

/**
 * WDM - Implied mode
 * Creates a new WDM instruction with no operands
 */
export function WDM(): Instruction {
  return new Instruction(defs.WDM_imp);
}

/**
 * CLI - Implied mode
 * Creates a new CLI instruction with no operands
 */
export function CLI(): Instruction {
  return new Instruction(defs.CLI_imp);
}

/**
 * SEI - Implied mode
 * Creates a new SEI instruction with no operands
 */
export function SEI(): Instruction {
  return new Instruction(defs.SEI_imp);
}

/**
 * STP - Implied mode
 * Creates a new STP instruction with no operands
 */
export function STP(): Instruction {
  return new Instruction(defs.STP_imp);
}

/**
 * WAI - Implied mode
 * Creates a new WAI instruction with no operands
 */
export function WAI(): Instruction {
  return new Instruction(defs.WAI_imp);
}

/**
 * PHA - Stack mode
 * Creates a new PHA instruction with no operands
 */
export function PHA(): Instruction {
  return new Instruction(defs.PHA_stk);
}

/**
 * PHX - Stack mode
 * Creates a new PHX instruction with no operands
 */
export function PHX(): Instruction {
  return new Instruction(defs.PHX_stk);
}

/**
 * PHY - Stack mode
 * Creates a new PHY instruction with no operands
 */
export function PHY(): Instruction {
  return new Instruction(defs.PHY_stk);
}

/**
 * PLA - Stack mode
 * Creates a new PLA instruction with no operands
 */
export function PLA(): Instruction {
  return new Instruction(defs.PLA_stk);
}

/**
 * PLX - Stack mode
 * Creates a new PLX instruction with no operands
 */
export function PLX(): Instruction {
  return new Instruction(defs.PLX_stk);
}

/**
 * PLY - Stack mode
 * Creates a new PLY instruction with no operands
 */
export function PLY(): Instruction {
  return new Instruction(defs.PLY_stk);
}

/**
 * PHB - Stack mode
 * Creates a new PHB instruction with no operands
 */
export function PHB(): Instruction {
  return new Instruction(defs.PHB_stk);
}

/**
 * PHD - Stack mode
 * Creates a new PHD instruction with no operands
 */
export function PHD(): Instruction {
  return new Instruction(defs.PHD_stk);
}

/**
 * PHK - Stack mode
 * Creates a new PHK instruction with no operands
 */
export function PHK(): Instruction {
  return new Instruction(defs.PHK_stk);
}

/**
 * PHP - Stack mode
 * Creates a new PHP instruction with no operands
 */
export function PHP(): Instruction {
  return new Instruction(defs.PHP_stk);
}

/**
 * PLB - Stack mode
 * Creates a new PLB instruction with no operands
 */
export function PLB(): Instruction {
  return new Instruction(defs.PLB_stk);
}

/**
 * PLD - Stack mode
 * Creates a new PLD instruction with no operands
 */
export function PLD(): Instruction {
  return new Instruction(defs.PLD_stk);
}

/**
 * PLP - Stack mode
 * Creates a new PLP instruction with no operands
 */
export function PLP(): Instruction {
  return new Instruction(defs.PLP_stk);
}

/**
 * PEA - Absolute mode
 * Creates a new PEA instruction with the specified operand
 */
export function PEA(addr: Absolute): Instruction {
  return new Instruction(defs.PEA_abs, addr);
}

/**
 * PEI - DirectPageIndirect mode
 * Creates a new PEI instruction with the specified operand
 */
export function PEI(addr: Direct): Instruction {
  return new Instruction(defs.PEI_dp_ind, addr);
}

/**
 * PER - PCRelativeLong mode
 * Creates a new PER instruction with the specified operand
 */
export function PER(target: Label): Instruction {
  return new Instruction(defs.PER_rel16, target);
}

/**
 * TAX - Implied mode
 * Creates a new TAX instruction with no operands
 */
export function TAX(): Instruction {
  return new Instruction(defs.TAX_imp);
}

/**
 * TAY - Implied mode
 * Creates a new TAY instruction with no operands
 */
export function TAY(): Instruction {
  return new Instruction(defs.TAY_imp);
}

/**
 * TXA - Implied mode
 * Creates a new TXA instruction with no operands
 */
export function TXA(): Instruction {
  return new Instruction(defs.TXA_imp);
}

/**
 * TYA - Implied mode
 * Creates a new TYA instruction with no operands
 */
export function TYA(): Instruction {
  return new Instruction(defs.TYA_imp);
}

/**
 * TSX - Implied mode
 * Creates a new TSX instruction with no operands
 */
export function TSX(): Instruction {
  return new Instruction(defs.TSX_imp);
}

/**
 * TXS - Implied mode
 * Creates a new TXS instruction with no operands
 */
export function TXS(): Instruction {
  return new Instruction(defs.TXS_imp);
}

/**
 * TXY - Implied mode
 * Creates a new TXY instruction with no operands
 */
export function TXY(): Instruction {
  return new Instruction(defs.TXY_imp);
}

/**
 * TYX - Implied mode
 * Creates a new TYX instruction with no operands
 */
export function TYX(): Instruction {
  return new Instruction(defs.TYX_imp);
}

/**
 * TCD - Implied mode
 * Creates a new TCD instruction with no operands
 */
export function TCD(): Instruction {
  return new Instruction(defs.TCD_imp);
}

/**
 * TCS - Implied mode
 * Creates a new TCS instruction with no operands
 */
export function TCS(): Instruction {
  return new Instruction(defs.TCS_imp);
}

/**
 * TDC - Implied mode
 * Creates a new TDC instruction with no operands
 */
export function TDC(): Instruction {
  return new Instruction(defs.TDC_imp);
}

/**
 * TSC - Implied mode
 * Creates a new TSC instruction with no operands
 */
export function TSC(): Instruction {
  return new Instruction(defs.TSC_imp);
}

/**
 * MVN - BlockMove mode
 * Creates a new MVN instruction with source and destination banks
 */
export function MVN(srcBank: Byte, destBank: Byte): Instruction {
  return new Instruction(defs.MVN_src_dest, [srcBank, destBank]);
}

/**
 * MVP - BlockMove mode
 * Creates a new MVP instruction with source and destination banks
 */
export function MVP(srcBank: Byte, destBank: Byte): Instruction {
  return new Instruction(defs.MVP_src_dest, [srcBank, destBank]);
}

/**
 * XBA - Implied mode
 * Creates a new XBA instruction with no operands
 */
export function XBA(): Instruction {
  return new Instruction(defs.XBA_imp);
}

/**
 * XCE - Implied mode
 * Creates a new XCE instruction with no operands
 */
export function XCE(): Instruction {
  return new Instruction(defs.XCE_imp);
}

/**
 * CLC - Implied mode
 * Creates a new CLC instruction with no operands
 */
export function CLC(): Instruction {
  return new Instruction(defs.CLC_imp);
}

/**
 * CLD - Implied mode
 * Creates a new CLD instruction with no operands
 */
export function CLD(): Instruction {
  return new Instruction(defs.CLD_imp);
}

/**
 * CLV - Implied mode
 * Creates a new CLV instruction with no operands
 */
export function CLV(): Instruction {
  return new Instruction(defs.CLV_imp);
}

/**
 * SEC - Implied mode
 * Creates a new SEC instruction with no operands
 */
export function SEC(): Instruction {
  return new Instruction(defs.SEC_imp);
}

/**
 * SED - Implied mode
 * Creates a new SED instruction with no operands
 */
export function SED(): Instruction {
  return new Instruction(defs.SED_imp);
}

// === MULTI-VARIANT INSTRUCTIONS (Factory Objects) ===

export interface LDAFactory {
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
}

export interface LDXFactory {
  imm: (val: Byte | Word) => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_y: (addr: Absolute) => Instruction;
  dp_y: (addr: Direct) => Instruction;
}

export interface LDYFactory {
  imm: (val: Byte | Word) => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
}

export interface STAFactory {
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
}

export interface STXFactory {
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  dp_y: (addr: Direct) => Instruction;
}

export interface STYFactory {
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  dp_x: (addr: Direct) => Instruction;
}

export interface STZFactory {
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
}

export interface ADCFactory {
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
}

export interface SBCFactory {
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
}

export interface CMPFactory {
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
}

export interface CPXFactory {
  imm: (val: Byte | Word) => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
}

export interface CPYFactory {
  imm: (val: Byte | Word) => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
}

export interface INCFactory {
  acc: () => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
}

export interface DECFactory {
  acc: () => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
}

export interface ANDFactory {
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
}

export interface ORAFactory {
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
}

export interface EORFactory {
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
}

export interface BITFactory {
  imm: (val: Byte | Word) => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
}

export interface TRBFactory {
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
}

export interface TSBFactory {
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
}

export interface ASLFactory {
  acc: () => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
}

export interface LSRFactory {
  acc: () => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
}

export interface ROLFactory {
  acc: () => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
}

export interface RORFactory {
  acc: () => Instruction;
  abs: (addr: Absolute) => Instruction;
  dp: (addr: Direct) => Instruction;
  abs_x: (addr: Absolute) => Instruction;
  dp_x: (addr: Direct) => Instruction;
}

export interface JMPFactory {
  abs: (addr: Absolute) => Instruction;
  abs_ind: (addr: Absolute) => Instruction;
  abs_x_ind: (addr: Absolute) => Instruction;
}

export interface JMLFactory {
  long: (addr: AbsoluteLong) => Instruction;
  abs_indl: (addr: Absolute) => Instruction;
}

export interface JSRFactory {
  abs: (addr: Absolute) => Instruction;
  abs_x_ind: (addr: Absolute) => Instruction;
}

export const LDA: LDAFactory = {
  imm: (val: Byte | Word) => new Instruction(defs.LDA_imm, val),
  abs: (addr: Absolute) => new Instruction(defs.LDA_abs, addr),
  long: (addr: AbsoluteLong) => new Instruction(defs.LDA_long, addr),
  dp: (addr: Direct) => new Instruction(defs.LDA_dp, addr),
  dp_ind: (addr: Direct) => new Instruction(defs.LDA_dp_ind, addr),
  dp_indl: (addr: Direct) => new Instruction(defs.LDA_dp_indl, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.LDA_abs_x, addr),
  long_x: (addr: AbsoluteLong) => new Instruction(defs.LDA_long_x, addr),
  abs_y: (addr: Absolute) => new Instruction(defs.LDA_abs_y, addr),
  dp_x: (addr: Direct) => new Instruction(defs.LDA_dp_x, addr),
  dp_x_ind: (addr: Direct) => new Instruction(defs.LDA_dp_x_ind, addr),
  dp_ind_y: (addr: Direct) => new Instruction(defs.LDA_dp_ind_y, addr),
  dp_indl_y: (addr: Direct) => new Instruction(defs.LDA_dp_indl_y, addr),
  stkr: (val: Byte) => new Instruction(defs.LDA_stkr, val),
  stkr_ind_y: (val: Byte) => new Instruction(defs.LDA_stkr_ind_y, val)
};

export const LDX: LDXFactory = {
  imm: (val: Byte | Word) => new Instruction(defs.LDX_imm, val),
  abs: (addr: Absolute) => new Instruction(defs.LDX_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.LDX_dp, addr),
  abs_y: (addr: Absolute) => new Instruction(defs.LDX_abs_y, addr),
  dp_y: (addr: Direct) => new Instruction(defs.LDX_dp_y, addr)
};

export const LDY: LDYFactory = {
  imm: (val: Byte | Word) => new Instruction(defs.LDY_imm, val),
  abs: (addr: Absolute) => new Instruction(defs.LDY_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.LDY_dp, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.LDY_abs_x, addr),
  dp_x: (addr: Direct) => new Instruction(defs.LDY_dp_x, addr)
};

export const STA: STAFactory = {
  abs: (addr: Absolute) => new Instruction(defs.STA_abs, addr),
  long: (addr: AbsoluteLong) => new Instruction(defs.STA_long, addr),
  dp: (addr: Direct) => new Instruction(defs.STA_dp, addr),
  dp_ind: (addr: Direct) => new Instruction(defs.STA_dp_ind, addr),
  dp_indl: (addr: Direct) => new Instruction(defs.STA_dp_indl, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.STA_abs_x, addr),
  long_x: (addr: AbsoluteLong) => new Instruction(defs.STA_long_x, addr),
  abs_y: (addr: Absolute) => new Instruction(defs.STA_abs_y, addr),
  dp_x: (addr: Direct) => new Instruction(defs.STA_dp_x, addr),
  dp_x_ind: (addr: Direct) => new Instruction(defs.STA_dp_x_ind, addr),
  dp_ind_y: (addr: Direct) => new Instruction(defs.STA_dp_ind_y, addr),
  dp_indl_y: (addr: Direct) => new Instruction(defs.STA_dp_indl_y, addr),
  stkr: (val: Byte) => new Instruction(defs.STA_stkr, val),
  stkr_ind_y: (val: Byte) => new Instruction(defs.STA_stkr_ind_y, val)
};

export const STX: STXFactory = {
  abs: (addr: Absolute) => new Instruction(defs.STX_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.STX_dp, addr),
  dp_y: (addr: Direct) => new Instruction(defs.STX_dp_y, addr)
};

export const STY: STYFactory = {
  abs: (addr: Absolute) => new Instruction(defs.STY_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.STY_dp, addr),
  dp_x: (addr: Direct) => new Instruction(defs.STY_dp_x, addr)
};

export const STZ: STZFactory = {
  abs: (addr: Absolute) => new Instruction(defs.STZ_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.STZ_dp, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.STZ_abs_x, addr),
  dp_x: (addr: Direct) => new Instruction(defs.STZ_dp_x, addr)
};

export const ADC: ADCFactory = {
  imm: (val: Byte | Word) => new Instruction(defs.ADC_imm, val),
  abs: (addr: Absolute) => new Instruction(defs.ADC_abs, addr),
  long: (addr: AbsoluteLong) => new Instruction(defs.ADC_long, addr),
  dp: (addr: Direct) => new Instruction(defs.ADC_dp, addr),
  dp_ind: (addr: Direct) => new Instruction(defs.ADC_dp_ind, addr),
  dp_indl: (addr: Direct) => new Instruction(defs.ADC_dp_indl, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.ADC_abs_x, addr),
  long_x: (addr: AbsoluteLong) => new Instruction(defs.ADC_long_x, addr),
  abs_y: (addr: Absolute) => new Instruction(defs.ADC_abs_y, addr),
  dp_x: (addr: Direct) => new Instruction(defs.ADC_dp_x, addr),
  dp_x_ind: (addr: Direct) => new Instruction(defs.ADC_dp_x_ind, addr),
  dp_ind_y: (addr: Direct) => new Instruction(defs.ADC_dp_ind_y, addr),
  dp_indl_y: (addr: Direct) => new Instruction(defs.ADC_dp_indl_y, addr),
  stkr: (val: Byte) => new Instruction(defs.ADC_stkr, val),
  stkr_ind_y: (val: Byte) => new Instruction(defs.ADC_stkr_ind_y, val)
};

export const SBC: SBCFactory = {
  imm: (val: Byte | Word) => new Instruction(defs.SBC_imm, val),
  abs: (addr: Absolute) => new Instruction(defs.SBC_abs, addr),
  long: (addr: AbsoluteLong) => new Instruction(defs.SBC_long, addr),
  dp: (addr: Direct) => new Instruction(defs.SBC_dp, addr),
  dp_ind: (addr: Direct) => new Instruction(defs.SBC_dp_ind, addr),
  dp_indl: (addr: Direct) => new Instruction(defs.SBC_dp_indl, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.SBC_abs_x, addr),
  long_x: (addr: AbsoluteLong) => new Instruction(defs.SBC_long_x, addr),
  abs_y: (addr: Absolute) => new Instruction(defs.SBC_abs_y, addr),
  dp_x: (addr: Direct) => new Instruction(defs.SBC_dp_x, addr),
  dp_x_ind: (addr: Direct) => new Instruction(defs.SBC_dp_x_ind, addr),
  dp_ind_y: (addr: Direct) => new Instruction(defs.SBC_dp_ind_y, addr),
  dp_indl_y: (addr: Direct) => new Instruction(defs.SBC_dp_indl_y, addr),
  stkr: (val: Byte) => new Instruction(defs.SBC_stkr, val),
  stkr_ind_y: (val: Byte) => new Instruction(defs.SBC_stkr_ind_y, val)
};

export const CMP: CMPFactory = {
  imm: (val: Byte | Word) => new Instruction(defs.CMP_imm, val),
  abs: (addr: Absolute) => new Instruction(defs.CMP_abs, addr),
  long: (addr: AbsoluteLong) => new Instruction(defs.CMP_long, addr),
  dp: (addr: Direct) => new Instruction(defs.CMP_dp, addr),
  dp_ind: (addr: Direct) => new Instruction(defs.CMP_dp_ind, addr),
  dp_indl: (addr: Direct) => new Instruction(defs.CMP_dp_indl, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.CMP_abs_x, addr),
  long_x: (addr: AbsoluteLong) => new Instruction(defs.CMP_long_x, addr),
  abs_y: (addr: Absolute) => new Instruction(defs.CMP_abs_y, addr),
  dp_x: (addr: Direct) => new Instruction(defs.CMP_dp_x, addr),
  dp_x_ind: (addr: Direct) => new Instruction(defs.CMP_dp_x_ind, addr),
  dp_ind_y: (addr: Direct) => new Instruction(defs.CMP_dp_ind_y, addr),
  dp_indl_y: (addr: Direct) => new Instruction(defs.CMP_dp_indl_y, addr),
  stkr: (val: Byte) => new Instruction(defs.CMP_stkr, val),
  stkr_ind_y: (val: Byte) => new Instruction(defs.CMP_stkr_ind_y, val)
};

export const CPX: CPXFactory = {
  imm: (val: Byte | Word) => new Instruction(defs.CPX_imm, val),
  abs: (addr: Absolute) => new Instruction(defs.CPX_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.CPX_dp, addr)
};

export const CPY: CPYFactory = {
  imm: (val: Byte | Word) => new Instruction(defs.CPY_imm, val),
  abs: (addr: Absolute) => new Instruction(defs.CPY_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.CPY_dp, addr)
};

export const INC: INCFactory = {
  acc: () => new Instruction(defs.INC_acc),
  abs: (addr: Absolute) => new Instruction(defs.INC_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.INC_dp, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.INC_abs_x, addr),
  dp_x: (addr: Direct) => new Instruction(defs.INC_dp_x, addr)
};

export const DEC: DECFactory = {
  acc: () => new Instruction(defs.DEC_acc),
  abs: (addr: Absolute) => new Instruction(defs.DEC_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.DEC_dp, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.DEC_abs_x, addr),
  dp_x: (addr: Direct) => new Instruction(defs.DEC_dp_x, addr)
};

export const AND: ANDFactory = {
  imm: (val: Byte | Word) => new Instruction(defs.AND_imm, val),
  abs: (addr: Absolute) => new Instruction(defs.AND_abs, addr),
  long: (addr: AbsoluteLong) => new Instruction(defs.AND_long, addr),
  dp: (addr: Direct) => new Instruction(defs.AND_dp, addr),
  dp_ind: (addr: Direct) => new Instruction(defs.AND_dp_ind, addr),
  dp_indl: (addr: Direct) => new Instruction(defs.AND_dp_indl, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.AND_abs_x, addr),
  long_x: (addr: AbsoluteLong) => new Instruction(defs.AND_long_x, addr),
  abs_y: (addr: Absolute) => new Instruction(defs.AND_abs_y, addr),
  dp_x: (addr: Direct) => new Instruction(defs.AND_dp_x, addr),
  dp_x_ind: (addr: Direct) => new Instruction(defs.AND_dp_x_ind, addr),
  dp_ind_y: (addr: Direct) => new Instruction(defs.AND_dp_ind_y, addr),
  dp_indl_y: (addr: Direct) => new Instruction(defs.AND_dp_indl_y, addr),
  stkr: (val: Byte) => new Instruction(defs.AND_stkr, val),
  stkr_ind_y: (val: Byte) => new Instruction(defs.AND_stkr_ind_y, val)
};

export const ORA: ORAFactory = {
  imm: (val: Byte | Word) => new Instruction(defs.ORA_imm, val),
  abs: (addr: Absolute) => new Instruction(defs.ORA_abs, addr),
  long: (addr: AbsoluteLong) => new Instruction(defs.ORA_long, addr),
  dp: (addr: Direct) => new Instruction(defs.ORA_dp, addr),
  dp_ind: (addr: Direct) => new Instruction(defs.ORA_dp_ind, addr),
  dp_indl: (addr: Direct) => new Instruction(defs.ORA_dp_indl, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.ORA_abs_x, addr),
  long_x: (addr: AbsoluteLong) => new Instruction(defs.ORA_long_x, addr),
  abs_y: (addr: Absolute) => new Instruction(defs.ORA_abs_y, addr),
  dp_x: (addr: Direct) => new Instruction(defs.ORA_dp_x, addr),
  dp_x_ind: (addr: Direct) => new Instruction(defs.ORA_dp_x_ind, addr),
  dp_ind_y: (addr: Direct) => new Instruction(defs.ORA_dp_ind_y, addr),
  dp_indl_y: (addr: Direct) => new Instruction(defs.ORA_dp_indl_y, addr),
  stkr: (val: Byte) => new Instruction(defs.ORA_stkr, val),
  stkr_ind_y: (val: Byte) => new Instruction(defs.ORA_stkr_ind_y, val)
};

export const EOR: EORFactory = {
  imm: (val: Byte | Word) => new Instruction(defs.EOR_imm, val),
  abs: (addr: Absolute) => new Instruction(defs.EOR_abs, addr),
  long: (addr: AbsoluteLong) => new Instruction(defs.EOR_long, addr),
  dp: (addr: Direct) => new Instruction(defs.EOR_dp, addr),
  dp_ind: (addr: Direct) => new Instruction(defs.EOR_dp_ind, addr),
  dp_indl: (addr: Direct) => new Instruction(defs.EOR_dp_indl, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.EOR_abs_x, addr),
  long_x: (addr: AbsoluteLong) => new Instruction(defs.EOR_long_x, addr),
  abs_y: (addr: Absolute) => new Instruction(defs.EOR_abs_y, addr),
  dp_x: (addr: Direct) => new Instruction(defs.EOR_dp_x, addr),
  dp_x_ind: (addr: Direct) => new Instruction(defs.EOR_dp_x_ind, addr),
  dp_ind_y: (addr: Direct) => new Instruction(defs.EOR_dp_ind_y, addr),
  dp_indl_y: (addr: Direct) => new Instruction(defs.EOR_dp_indl_y, addr),
  stkr: (val: Byte) => new Instruction(defs.EOR_stkr, val),
  stkr_ind_y: (val: Byte) => new Instruction(defs.EOR_stkr_ind_y, val)
};

export const BIT: BITFactory = {
  imm: (val: Byte | Word) => new Instruction(defs.BIT_imm, val),
  abs: (addr: Absolute) => new Instruction(defs.BIT_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.BIT_dp, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.BIT_abs_x, addr),
  dp_x: (addr: Direct) => new Instruction(defs.BIT_dp_x, addr)
};

export const TRB: TRBFactory = {
  abs: (addr: Absolute) => new Instruction(defs.TRB_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.TRB_dp, addr)
};

export const TSB: TSBFactory = {
  abs: (addr: Absolute) => new Instruction(defs.TSB_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.TSB_dp, addr)
};

export const ASL: ASLFactory = {
  acc: () => new Instruction(defs.ASL_acc),
  abs: (addr: Absolute) => new Instruction(defs.ASL_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.ASL_dp, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.ASL_abs_x, addr),
  dp_x: (addr: Direct) => new Instruction(defs.ASL_dp_x, addr)
};

export const LSR: LSRFactory = {
  acc: () => new Instruction(defs.LSR_acc),
  abs: (addr: Absolute) => new Instruction(defs.LSR_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.LSR_dp, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.LSR_abs_x, addr),
  dp_x: (addr: Direct) => new Instruction(defs.LSR_dp_x, addr)
};

export const ROL: ROLFactory = {
  acc: () => new Instruction(defs.ROL_acc),
  abs: (addr: Absolute) => new Instruction(defs.ROL_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.ROL_dp, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.ROL_abs_x, addr),
  dp_x: (addr: Direct) => new Instruction(defs.ROL_dp_x, addr)
};

export const ROR: RORFactory = {
  acc: () => new Instruction(defs.ROR_acc),
  abs: (addr: Absolute) => new Instruction(defs.ROR_abs, addr),
  dp: (addr: Direct) => new Instruction(defs.ROR_dp, addr),
  abs_x: (addr: Absolute) => new Instruction(defs.ROR_abs_x, addr),
  dp_x: (addr: Direct) => new Instruction(defs.ROR_dp_x, addr)
};

export const JMP: JMPFactory = {
  abs: (addr: Absolute) => new Instruction(defs.JMP_abs, addr),
  abs_ind: (addr: Absolute) => new Instruction(defs.JMP_abs_ind, addr),
  abs_x_ind: (addr: Absolute) => new Instruction(defs.JMP_abs_x_ind, addr)
};

export const JML: JMLFactory = {
  long: (addr: AbsoluteLong) => new Instruction(defs.JML_long, addr),
  abs_indl: (addr: Absolute) => new Instruction(defs.JML_abs_indl, addr)
};

export const JSR: JSRFactory = {
  abs: (addr: Absolute) => new Instruction(defs.JSR_abs, addr),
  abs_x_ind: (addr: Absolute) => new Instruction(defs.JSR_abs_x_ind, addr)
};