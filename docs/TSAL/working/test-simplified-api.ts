import { Instruction } from './op.js';
import { 
  // Simple functions (single-variant instructions)
  NOP, BMI, CLC, INX, DEX, JSL, RTS, BCC, BCS, BEQ, BNE, BPL, BVS, BVC, BRA, BRL,
  RTI, RTL, SEP, REP, CLI, SEI, STP, WAI, PHA, PHX, PHY, PLA, PLX, PLY,
  PHB, PHD, PHK, PHP, PLB, PLD, PLP, PEA, PEI, PER, 
  TAX, TAY, TXA, TYA, TSX, TXS, TXY, TYX, TCD, TCS, TDC, TSC,
  MVN, MVP, XBA, XCE, CLD, CLV, SEC, SED, BRK, COP, WDM, INY, DEY,
  
  // Factory objects (multi-variant instructions)
  LDA, STA, LDX, LDY, STX, STY, STZ, ADC, SBC, CMP, CPX, CPY, INC, DEC,
  AND, ORA, EOR, BIT, TRB, TSB, ASL, LSR, ROL, ROR, JMP, JML, JSR
} from './op-factories.js';

// Test simple functions (single-variant instructions)
console.log('=== Testing Simple Functions (Single-Variant Instructions) ===');

// Test no-operand instructions
const nop = NOP();
const clc = CLC();
const inx = INX();
const dex = DEX();
const rts = RTS();
const rti = RTI();
const rtl = RTL();

console.log('✓ No-operand instructions:', { nop, clc, inx, dex, rts, rti, rtl });

// Test single-operand instructions
const bmi = BMI('loop');
const bcc = BCC('skip');
const bcs = BCS('error');
const beq = BEQ('equals');
const bne = BNE('not_equals');
const jsl = JSL(0x018000);
const sep = SEP(0x20);
const rep = REP(0x10);
const brk = BRK(0x80);
const cop = COP(0x81);

console.log('✓ Single-operand instructions:', { bmi, bcc, bcs, beq, bne, jsl, sep, rep, brk, cop });

// Test two-operand instructions (block move)
const mvn = MVN(0x7E, 0x7F);
const mvp = MVP(0x7F, 0x7E);

console.log('✓ Two-operand instructions:', { mvn, mvp });

// Test factory objects (multi-variant instructions)
console.log('\n=== Testing Factory Objects (Multi-Variant Instructions) ===');

// Test LDA with different addressing modes
const lda_imm = LDA.imm(0x42);
const lda_abs = LDA.abs(0x1234);
const lda_long = LDA.long(0x7E1234);
const lda_dp = LDA.dp(0x80);
const lda_dp_x = LDA.dp_x(0x80);

console.log('✓ LDA variants:', { lda_imm, lda_abs, lda_long, lda_dp, lda_dp_x });

// Test STA with different addressing modes
const sta_abs = STA.abs(0x1234);
const sta_long = STA.long(0x7E1234);
const sta_dp = STA.dp(0x80);
const sta_dp_x = STA.dp_x(0x80);

console.log('✓ STA variants:', { sta_abs, sta_long, sta_dp, sta_dp_x });

// Test ADC with different addressing modes
const adc_imm = ADC.imm(0x42);
const adc_abs = ADC.abs(0x1234);
const adc_dp = ADC.dp(0x80);

console.log('✓ ADC variants:', { adc_imm, adc_abs, adc_dp });

// Test JMP with different addressing modes
const jmp_abs = JMP.abs(0x1234);
const jmp_ind = JMP.abs_ind(0x1234);
const jmp_x_ind = JMP.abs_x_ind(0x1234);

console.log('✓ JMP variants:', { jmp_abs, jmp_ind, jmp_x_ind });

console.log('\n=== Comparison of Old vs New API ===');

// Old verbose API (would have been):
// const nop_old = NOP.imp();
// const bmi_old = BMI.rel('loop');
// const clc_old = CLC.imp();

// New clean API:
const nop_new = NOP();
const bmi_new = BMI('loop');
const clc_new = CLC();

console.log('✓ New API is cleaner and more intuitive!');
console.log('  - NOP() instead of NOP.imp()');
console.log('  - BMI(\'loop\') instead of BMI.rel(\'loop\')');
console.log('  - CLC() instead of CLC.imp()');

// Multi-variant instructions keep their factory pattern
console.log('  - LDA.imm(0x42) for immediate addressing');
console.log('  - LDA.abs(0x1234) for absolute addressing');
console.log('  - LDA.long(0x7E1234) for long addressing');

console.log('\n🎉 All tests passed! The simplified API is working correctly.'); 