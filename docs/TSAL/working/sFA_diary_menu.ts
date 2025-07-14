import {
    code, label, byte, word, long, pointerTable, list, Code, DataChunk
} from "./platform";
import {
    LDA, STA, STZ, AND, ORA, EOR, CMP, CPY, CPX, BIT,
    LDX, LDY,
    JSR, JMP, JSL, RTL, RTS,
    BEQ, BNE, BCS, BCC, BMI, BPL, BVS, BVC,
    CLC, SEC,
    INC, DEC, DEX, DEY, INX, INY,
    ADC, SBC,
    PHA, PLA, PHX, PLX, PHY, PLY, PHD, PLD,
    TYA, TAY,
    ASL, LSR, ROL, ROR,
    XBA, TCD,
    REP, SEP,
    PEA, BRA, TRB, TSB
} from "./op";
import { mem, COP, h_actor, WideString, WideStringObject } from "./game";

// --- Placeholders for external dependencies ---
const system_strings = code(ctx => { });
const strings_0BF706 = code(ctx => { });
const chunk_03BAE1 = code(ctx => { });
const func_02F06A = code(ctx => { });
const chunk_028000 = code(ctx => { });
const asciistring_01EADC = code(ctx => { });
const func_03D954 = code(ctx => { });
const func_03D9B8 = code(ctx => { });
const func_03D994 = code(ctx => { });
const func_028191 = code(ctx => { });
const func_0281A2 = code(ctx => { });

const sub_0BED64 = code(ctx => { });
const sub_0BEBF9 = code(ctx => { });
const sub_0BE673 = code(ctx => { });
const sub_0BE840 = code(ctx => { });
const sub_0BE87C = code(ctx => { });
const sub_0BECFB = code(ctx => { });
const sub_0BECB1 = code(ctx => { });
const sub_0BECD9 = code(ctx => { });
const func_02F076 = code(ctx => { });


// --- Forward declarations for local functions/labels ---
// Necessary because of circular dependencies between code blocks.
let code_0BE2CA: Code,
    code_0BE2D0: Code,
    code_0BE2F6: Code,
    code_0BE30F: Code,
    code_0BE32B: Code,
    code_list_0BE34C: DataChunk,
    code_0BE354: Code,
    code_0BEA55: Code,
    func_0BE8A8: Code,
    func_0BE6BA: Code,
    code_0BE398: Code,
    code_0BE3B1: Code,
    code_0BE3CD: Code,
    code_0BE3DF: Code,
    code_0BE462: Code,
    code_0BE47D: Code,
    code_0BE498: Code,
    code_0BE4EE: Code,
    code_0BE527: Code,
    code_0BEB8B: Code,
    code_0BEA86: Code,
    code_0BEA9F: Code,
    code_0BE6FD: Code,
    code_0BE6EB: Code,
    code_0BE75B: Code,
    code_0BE774: Code,
    func_0BE2CC: Code,
    code_list_0BEB46: DataChunk,
    table_0BED3C: DataChunk,
    word_0BED44: DataChunk,
    word_0BED4E: DataChunk,
    word_0BED54: DataChunk,
    word_0BED5c: DataChunk;


// --- Data Tables & Strings ---
const widestring_0BF63F = WideString`1[DLG:8,10]A[DLG:8,12]B[DLG:8,14]SEL[DLG:8,16]Y`;
const widestring_0BF653 = WideString`2[DLG:8,10]B[DLG:8,12]Y[DLG:8,14]SEL[DLG:8,16]A`;
const table_0BF63B = pointerTable<WideStringObject>([widestring_0BF63F, widestring_0BF653]);

const widestring_0BF66B = WideString`Stereo`;
const widestring_0BF672 = WideString`Mono  `;
const table_0BF667 = pointerTable<WideStringObject>([widestring_0BF66B, widestring_0BF672]);

const widestring_0BF4C3 = WideString`[DLG:2,C][::][SKP:2]HP[SKP:1][BCD:2,D9A][SKP:2]STR[SKP:1][BCD:2,D9C][SKP:2]DEF[SKP:1][BCD:2,D9E]`;
const widestring_0BF4EA = WideString`[DLG:2,10][::][SKP:2]HP[SKP:1][BCD:2,D9A][SKP:2]STR[SKP:1][BCD:2,D9C][SKP:2]DEF[SKP:1][BCD:2,D9E]`;
const widestring_0BF511 = WideString`[DLG:2,14][::][SKP:2]HP[SKP:1][BCD:2,D9A][SKP:2]STR[SKP:1][BCD:2,D9C][SKP:2]DEF[SKP:1][BCD:2,D9E]`;
const table_0BF6AD = pointerTable<WideStringObject>([widestring_0BF4C3, widestring_0BF4EA, widestring_0BF511]);

const widestring_0BF6DF = WideString`Diary1 [ADR:${strings_0BF706},D74]`;
const widestring_0BF6EC = WideString`Diary2 [ADR:${strings_0BF706},D76]`;
const widestring_0BF6F9 = WideString`Diary3 [ADR:${strings_0BF706},D78]`;
const table_0BF6D9 = pointerTable<WideStringObject>([widestring_0BF6DF, widestring_0BF6EC, widestring_0BF6F9]);

const widestring_0BF3F4 = WideString`[DLG:6,A][SIZ:A,4]Start Journey[N]Erase Trip Diary[N]Copy Trip Diary[N][LU1:B]Snd/Buttons`;
const widestring_0BF437 = WideString`[DLG:2,8][SIZ:E,7]Which Diary?[N][::] Diary1 [ADR:${strings_0BF706},D74][N][N] Diary2 [ADR:${strings_0BF706},D76][N][N] Diary3 [ADR:${strings_0BF706},D78]`;
const widestring_0BF476 = WideString`[DLG:2,8][SIZ:E,7][LU1:B]Snd/Button[N][JMP:${widestring_0BF437}+M]`;
const widestring_0BF48C = WideString`[DLG:2,8][SIZ:E,7]Move which Diary?[N][JMP:${widestring_0BF437}+M]`;
const widestring_0BF4A7 = WideString`[DLG:2,8][SIZ:E,7]Erase which Diary?[N][JMP:${widestring_0BF437}+M]`;
const widestring_0BF538 = WideString`[DLG:6,8][SIZ:A,8][SKP:2][LU1:B]Snd/Buttons[N]End Changes[N]Sound[N][LU2:2]Type[N][SKP:5]   :Attack/Talk[N][SKP:5]   :Item/Cancel[N][SKP:5]   :Item palette[N][SKP:5]   :Not used`;
const widestring_0BF5AD = WideString`[DLG:6,8][SIZ:A,8]Arrangement  OK?[N]Start Journey[N]Sound[N][LU2:2]Type[N][SKP:5]   :Attack/Talk[N][SKP:5]   :Item/Cancel[N][SKP:5]   :Item palette[N][SKP:5]   :Not used`;
const widestring_0BF625 = WideString`[DLG:D,C][SFX:0][ADR:${table_0BF667},D90]`;
const widestring_0BF630 = WideString`[DLG:11,E][SFX:0][ADR:${table_0BF63B},D8E]`;
const widestring_0BF679 = WideString`[DLG:4,15][SIZ:C,2][DLY:FF]Diary not empty[N]Erase and select[FIN][CLD]`;
const widestring_0BF6A4 = WideString`[DLG:4,A][ADR:${table_0BF6AD},D94]`;
const widestring_0BF6B3 = WideString`[DLG:4,8][SIZ:D,5][ADR:${table_0BF6D9},D94][N][N]Erase diary? [N] No [N] Yes `;


// --- Module Content ---

export const h_sFA_diary_menu = h_actor(0x00, 0x00, 0x28);

export const e_sFA_diary_menu = code(ctx => {
    ctx.emit([
        LDA.imm(word(0x0000)),
        STA.long(0x7F0A00),
        SEP(0x20),
        STA.abs(mem._TM),
        REP(0x20),
        LDA.imm(word(0xFFFF)),
        STA.abs(0x0D92),
        STA.abs(0x0D96),
        STA.abs(0x0D98),
        LDA.imm(word(0x4001)),
        TSB.abs(0x09EC),
        SEP(0x20),
        LDA.imm(byte(0x88)),
        STA.abs(mem.W34SEL),
        LDA.imm(byte(0x22)),
        STA.abs(mem.WOBJSEL),
        REP(0x20),
        LDA.imm(word(0x0000)),
        STA.long(0x7F0A00),
        STA.abs(0x0B04),
        LDA.imm(word(0x0001)),
        STA.abs(0x00EE),
        SEP(0x20),
        LDA.imm(byte(0x01)),
        STA.abs(mem._TM),
        LDA.imm(byte(0x04)),
        STA.abs(mem._TS),
        LDA.imm(byte(0x82)),
        STA.abs(mem.CGWSEL),
        LDA.imm(byte(0x41)),
        STA.abs(mem.CGADSUB),
        REP(0x20),
        LDA.imm(word(0x0080)),
        STA.abs(0x068A),
        STA.abs(0x06BE),
        LDA.imm(word(0x0300)),
        STA.abs(0x068E),
        STA.abs(0x06C2),
        LDA.imm(word(0x3000)),
        TSB.abs(mem.joypad_mask_std),
        LDA.imm(word(0x2800)),
        TSB.abs(mem.abs.player_flags),
        COP[0xBD](asciistring_01EADC),
        COP[0x6B](widestring_0BF3F4),
        JSR(sub_0BED64),
        LDA.imm(word(0x0F00)),
        STA.abs(mem.joypad_mask_inv),
        STZ.abs(0x18),
        COP[0xC1](),
        LDA.abs(0x0654),
        BNE(code_0BE2CA),
        RTL(),
    ]);
});

code_0BE2CA = code(ctx => { ctx.emit([ BRA(code_0BE2D0) ]); });

func_0BE2CC = code(ctx => { ctx.emit([ COP[0x6B](widestring_0BF3F4) ]); });

code_0BE2D0 = code(ctx => {
    const code_0BE2DC = label();
    ctx.emit([
        LDA.imm(word(0xFFFF)),
        STA.abs(0x0D92),
        LDA.imm(word(0x0000)),
        STA.abs(0x0D98),

        code_0BE2DC,
        COP[0xBD](asciistring_01EADC),
        COP[0xC1](),
        COP[0x40](word(0x0800), code_0BE2F6),
        COP[0x40](word(0x0400), code_0BE30F),
        COP[0x40](word(0x0080), code_0BE32B),
        RTL(),
    ]);
});

code_0BE2F6 = code(ctx => {
    const code_0BE302 = label();
    ctx.emit([
        COP[0x06](byte(10)),
        LDA.abs(0x0D98),
        DEC.acc(),
        BPL(code_0BE302),
        LDA.imm(word(0x0003)),
        code_0BE302,
        STA.abs(0x0D98),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        RTL(),
    ]);
});

code_0BE30F = code(ctx => {
    const code_0BE31E = label();
    ctx.emit([
        COP[0x06](byte(10)),
        LDA.abs(0x0D98),
        INC.acc(),
        CMP.imm(word(0x0004)),
        BCC(code_0BE31E),
        LDA.imm(word(0x0000)),
        code_0BE31E,
        STA.abs(0x0D98),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        RTL(),
    ]);
});

code_0BE32B = code(ctx => {
    ctx.emit([
        COP[0x06](byte(11)),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        LDA.abs(0x0D98),
        AND.imm(word(0x0003)),
        STA.dp(0x00),
        LDA.imm(word(0xFFFF)),
        STA.abs(0x0D98),
        COP[0xD9](word(0x0000), code_list_0BE34C),
    ]);
});

code_0BE354 = code(ctx => {
    const code_0BE36D = label();
    ctx.emit([
        JSR(sub_0BEBF9),
        COP[0x6B](widestring_0BF437),
        COP[0xC8](code_0BEB8B),
        COP[0xBD](asciistring_01EADC),
        LDA.abs(0x0D8C),
        AND.imm(word(0x0003)),
        STA.abs(0x0D92),
        code_0BE36D,
        COP[0xC2](),
        LDA.imm(word(0x000C)),
        STA.abs_x(0x7F101C),
        COP[0xC8](code_0BE527),
        COP[0xE2](code_0BE36D),
        COP[0x40](word(0x0800), code_0BE398),
        COP[0x40](word(0x0400), code_0BE3B1),
        COP[0x40](word(0x0080), code_0BE3DF),
        COP[0x40](word(0x8000), code_0BE3CD),
        RTL(),
    ]);
});

code_0BE398 = code(ctx => {
    const code_0BE3A4 = label();
    ctx.emit([
        COP[0x06](byte(10)),
        LDA.abs(0x0D92),
        DEC.acc(),
        BPL(code_0BE3A4),
        LDA.imm(word(0x0002)),
        code_0BE3A4,
        STA.abs(0x0D92),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        RTL(),
    ]);
});

code_0BE3B1 = code(ctx => {
    const code_0BE3C0 = label();
    ctx.emit([
        COP[0x06](byte(10)),
        LDA.abs(0x0D92),
        INC.acc(),
        CMP.imm(word(0x0003)),
        BCC(code_0BE3C0),
        LDA.imm(word(0x0000)),
        code_0BE3C0,
        STA.abs(0x0D92),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        RTL(),
    ]);
});

code_0BE3CD = code(ctx => {
    ctx.emit([
        COP[0x06](byte(0x0D)),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        JSR(sub_0BEBF9),
        JMP(func_0BE2CC),
    ]);
});

code_0BE3DF = code(ctx => {
    const code_0BE433 = label();
    ctx.emit([
        COP[0x06](byte(11)),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        LDA.abs(0x0D92),
        STA.abs(0x0D8C),
        LDA.imm(word(0xFFFF)),
        STA.abs(0x0D92),
        LDA.abs(0x0D8C),
        STA.long(0x306000),
        JSL(func_03D954),
        BCS(code_0BE433),
        JSR(sub_0BE673),
        LDA.abs(0x0AB2),
        STA.abs(0x0AAC),
        LDA.imm(word(0x00E6)),
        STA.abs(mem.scene_next),
        LDA.imm(word(0x0078)),
        STA.abs(0x064C),
        LDA.imm(word(0x0090)),
        STA.abs(0x064E),
        LDA.imm(word(0x0003)),
        STA.abs(0x0650),
        LDA.imm(word(0x1100)),
        STA.abs(0x0652),
        LDA.imm(word(0x2800)),
        TRB.abs(mem.abs.player_flags),
        COP[0xE0](),
        code_0BE433,
        LDA.abs(0x0D92),
        STA.abs(0x0D94),
        LDA.imm(word(0xFFFF)),
        STA.abs(0x0D92),
        LDA.imm(word(0x0000)),
        STA.abs(0x0D8E),
        STA.abs(0x0D90),
        JSR(sub_0BEBF9),
        COP[0x6B](widestring_0BF5AD),
        COP[0x6B](widestring_0BF625),
        COP[0x6B](widestring_0BF630),
        COP[0xBD](asciistring_01EADC),
        LDA.imm(word(0x0000)),
        STA.abs(0x0D98),
        BRA(code_0BE462),
    ]);
});

code_0BE462 = code(ctx => {
    ctx.emit([
        COP[0xC2](),
        COP[0x40](word(0x0380), code_0BE498),
        COP[0x40](word(0x0800), code_0BE75B),
        COP[0x40](word(0x0400), code_0BE774),
        COP[0x40](word(0x8000), code_0BE47D),
        RTL(),
    ]);
});

code_0BE47D = code(ctx => {
    ctx.emit([
        COP[0x06](byte(0x0D)),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        LDA.imm(word(0xFFFF)),
        STA.abs(0x0D98),
        LDA.abs(0x0D94),
        STA.abs(0x0D92),
        JMP(code_0BE354),
    ]);
});

code_0BE498 = code(ctx => {
    const code_0BE4E7 = label();
    const code_0BE4C5 = label();
    ctx.emit([
        LDA.imm(word(0x0380)),
        TSB.abs(0x0658),
        LDA.abs(0x0D98),
        BEQ(code_0BE4E7),
        DEC.acc(),
        BNE(code_0BE4C5),
        COP[0x06](byte(0x0D)),
        LDA.abs(0x0D90),
        INC.acc(),
        AND.imm(word(0x0001)),
        STA.abs(0x0D90),
        COP[0x6B](widestring_0BF625),
        COP[0xBD](asciistring_01EADC),
        LDA.imm(word(0x0380)),
        TSB.abs(0x0658),
        JMP(code_0BE462),
        code_0BE4C5,
        DEC.acc(),
        BNE(code_0BE4E7),
        COP[0x06](byte(0x0D)),
        LDA.abs(0x0D8E),
        INC.acc(),
        AND.imm(word(0x0001)),
        STA.abs(0x0D8E),
        COP[0x6B](widestring_0BF630),
        COP[0xBD](asciistring_01EADC),
        LDA.imm(word(0x0380)),
        TSB.abs(0x0658),
        JMP(code_0BE462),
        code_0BE4E7,
        COP[0x40](word(0x0080), code_0BE4EE),
        RTL(),
    ]);
});

code_0BE4EE = code(ctx => {
    ctx.emit([
        COP[0x06](byte(11)),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        LDA.imm(word(0xFFFF)),
        STA.abs(0x0D98),
        LDA.abs(0x0D90),
        STA.abs(0x0B24),
        LDA.abs(0x0D8E),
        STA.abs(0x0B26),
        JSR(sub_0BE673),
        LDA.imm(word(0x0008)),
        STA.abs(mem.scene_next),
        COP[0x26](byte(0x08), word(0x0050), word(0x00A0), byte(0x00), word(0x1200)),
        LDA.imm(word(0x2800)),
        TRB.abs(mem.abs.player_flags),
        COP[0xE0](),
    ]);
});

code_0BE527 = code(ctx => {
    const code_0BE53D = label();
    const code_0BE56E = label();
    const code_0BE57D = label();
    const code_0BE590 = label();
    const code_0BE596 = label();
    const code_0BE5A3 = label();
    const code_0BE5A7 = label();
    const code_0BE610 = label();
    const code_0BE612 = label();
    const code_0BE5D1 = label();
    const code_0BE5FF = label();
    const code_0BE632 = label();
    const code_0BE660 = label();
    const code_0BE671 = label();
    ctx.emit([
        PHX(),
        LDA.abs(0x0D92),
        ASL.acc(),
        TAY(),
        LDA.abs_y(0x0D74),
        ASL.acc(),
        TAX(),
        LDA.long_x(strings_0BF706),
        SEC(),
        SBC.imm(word(strings_0BF706)),
        TAX(),
        SEP(0x20),
        code_0BE53D,
        LDA.long_x(strings_0BF706),
        INX(),
        CMP.imm(byte(0xCA)),
        BNE(code_0BE53D),
        REP(0x20),
        LDA.long_x(strings_0BF706),
        TAY(),
        LDA.long_x(strings_0BF706 + 2),
        PLX(),
        STA.abs_x(0x7F100E),
        TYA(),
        STA.abs_x(0x7F100C),
        SEC(),
        SBC.abs(0x06BE),
        BMI(code_0BE56E),
        STA.abs_x(0x7F100C),
        LDA.imm(word(0x0001)),
        STA.abs_x(0x7F0000),
        BRA(code_0BE57D),
        code_0BE56E,
        EOR.imm(word(0xFFFF)),
        INC.acc(),
        STA.abs_x(0x7F100C),
        LDA.imm(word(0xFFFF)),
        STA.abs_x(0x7F0000),
        code_0BE57D,
        LDA.abs_x(0x7F100E),
        SEC(),
        SBC.abs(0x06C2),
        BMI(code_0BE596),
        STA.abs_x(0x7F100E),
        BEQ(code_0BE590),
        LDA.imm(word(0x0001)),
        code_0BE590,
        STA.abs_x(0x7F0002),
        BRA(code_0BE5A7),
        code_0BE596,
        EOR.imm(word(0xFFFF)),
        INC.acc(),
        STA.abs_x(0x7F100E),
        BEQ(code_0BE5A3),
        LDA.imm(word(0xFFFF)),
        code_0BE5A3,
        STA.abs_x(0x7F0002),
        code_0BE5A7,
        LDA.abs_x(0x7F100C),
        CMP.abs_x(0x7F100E),
        BCC(code_0BE612),
        LDA.abs_x(0x7F100C),
        BEQ(code_0BE610),
        STA.abs_x(0x7F1014),
        LSR.acc(),
        STA.abs_x(0x7F1010),
        LDA.abs_x(0x7F100E),
        STA.abs_x(0x7F1012),
        COP[0xC1](),
        LDA.abs_x(0x7F101C),
        STA.dp(0x00),
        code_0BE5D1,
        LDA.abs_x(0x7F0000),
        CLC(),
        ADC.abs(0x06BE),
        STA.abs(0x06BE),
        LDA.abs_x(0x7F1010),
        SEC(),
        SBC.abs_x(0x7F1012),
        STA.abs_x(0x7F1010),
        BPL(code_0BE5FF),
        CLC(),
        ADC.abs_x(0x7F100C),
        STA.abs_x(0x7F1010),
        LDA.abs_x(0x7F0002),
        CLC(),
        ADC.abs(0x06C2),
        STA.abs(0x06C2),
        code_0BE5FF,
        LDA.abs_x(0x7F1014),
        DEC.acc(),
        STA.abs_x(0x7F1014),
        BEQ(code_0BE610),
        DEC.dp(0x00),
        BNE(code_0BE5D1),
        RTL(),
        code_0BE610,
        COP[0xC5](),
        code_0BE612,
        LDA.abs_x(0x7F100E),
        BEQ(code_0BE671),
        STA.abs_x(0x7F1014),
        LSR.acc(),
        STA.abs_x(0x7F1012),
        LDA.abs_x(0x7F100C),
        STA.abs_x(0x7F1010),
        COP[0xC1](),
        LDA.abs_x(0x7F101C),
        STA.dp(0x00),
        code_0BE632,
        LDA.abs_x(0x7F0002),
        CLC(),
        ADC.abs(0x06C2),
        STA.abs(0x06C2),
        LDA.abs_x(0x7F1012),
        SEC(),
        SBC.abs_x(0x7F1010),
        STA.abs_x(0x7F1012),
        BPL(code_0BE660),
        CLC(),
        ADC.abs_x(0x7F100E),
        STA.abs_x(0x7F1012),
        LDA.abs_x(0x7F0000),
        CLC(),
        ADC.abs(0x06BE),
        STA.abs(0x06BE),
        code_0BE660,
        LDA.abs_x(0x7F1014),
        DEC.acc(),
        STA.abs_x(0x7F1014),
        BEQ(code_0BE671),
        DEC.dp(0x00),
        BNE(code_0BE632),
        RTL(),
        code_0BE671,
        COP[0xC5](),
    ]);
});

sub_0BE673 = code(ctx => {
    const code_0BE68C = label();
    const code_0BE695 = label();
    const code_0BE6B3 = label();
    ctx.emit([
        LDA.imm(word(0x0000)),
        STA.abs(0x0B04),
        STZ.abs(0x00EE),
        LDA.abs(0x0B24),
        BNE(code_0BE68C),
        SEP(0x20),
        LDA.imm(byte(0x91)),
        STA.abs(mem.APUIO0),
        REP(0x20),
        BRA(code_0BE695),
        code_0BE68C,
        SEP(0x20),
        LDA.imm(byte(0x90)),
        STA.abs(mem.APUIO0),
        REP(0x20),
        code_0BE695,
        LDA.abs(0x0B26),
        BNE(code_0BE6B3),
        LDA.imm(word(0x8000)),
        STA.abs(0x0DAC),
        LDA.imm(word(0x4000)),
        STA.abs(0x0DAA),
        LDA.imm(word(0x0000)),
        STA.abs(0x0DB0),
        LDA.imm(word(0x0040)),
        STA.abs(0x0DAE),
        RTS(),
        code_0BE6B3,
        LDA.imm(word(0x0000)),
        STA.abs(0x0DB0),
        RTS(),
    ]);
});

const code_0BE740 = label();

func_0BE6BA = code(ctx => {
    const code_0BE6D0 = label();
    const code_0BE6EB = label();
    const code_0BE6FD = label();
    const code_0BE790 = label();
    const code_0BE7A5 = label();
    const code_0BE7F5 = label();
    const code_0BE714 = label();

    ctx.emit([
        JSR(sub_0BEBF9),
        COP[0x6B](widestring_0BF476),
        COP[0xC8](code_0BEB8B),
        COP[0xBD](asciistring_01EADC),
        LDA.imm(word(0x0000)),
        STA.abs(0x0D92),

        code_0BE6D0,
        COP[0xC2](),
        COP[0x40](word(0x0800), code_0BEA86),
        COP[0x40](word(0x0400), code_0BEA9F),
        COP[0x40](word(0x0080), code_0BE6FD),
        COP[0x40](word(0x8000), code_0BE6EB),
        RTL(),

        code_0BE6EB,
        COP[0x06](byte(0x0D)),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        JSR(sub_0BEBF9),
        JMP(func_0BE2CC),

        code_0BE6FD,
        COP[0x06](byte(12)),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        LDA.abs(0x0D92),
        ASL.acc(),
        TAY(),
        LDA.abs_y(0x0D74),
        BNE(code_0BE714),
        RTL(),

        code_0BE714,
        COP[0x06](byte(11)),
        LDA.abs(0x0D92),
        STA.abs(0x0D94),
        JSR(sub_0BE840),
        LDA.imm(word(0xFFFF)),
        STA.abs(0x0D92),
        JSR(sub_0BEBF9),
        COP[0x6B](widestring_0BF538),
        COP[0x6B](widestring_0BF625),
        COP[0x6B](widestring_0BF630),
        COP[0xBD](asciistring_01EADC),
        LDA.imm(word(0x0000)),
        STA.abs(0x0D98),

        code_0BE740,
        COP[0xC2](),
        COP[0x40](word(0x0380), code_0BE7A5),
        COP[0x40](word(0x0800), code_0BE75B),
        COP[0x40](word(0x0400), code_0BE774),
        COP[0x40](word(0x8000), code_0BE790),
        RTL(),
    ]);
});

code_0BE75B = code(ctx => {
    const code_0BE770 = label();
    ctx.emit([
        COP[0x06](byte(10)),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        LDA.abs(0x0D98),
        DEC.acc(),
        BPL(code_0BE770),
        LDA.imm(word(0x0002)),
        code_0BE770,
        STA.abs(0x0D98),
        RTL(),
    ]);
});

code_0BE774 = code(ctx => {
    const code_0BE78C = label();
    ctx.emit([
        COP[0x06](byte(10)),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        LDA.abs(0x0D98),
        INC.acc(),
        CMP.imm(word(0x0003)),
        BCC(code_0BE78C),
        LDA.imm(word(0x0000)),
        code_0BE78C,
        STA.abs(0x0D98),
        RTL(),
    ]);
});

const code_0BE790 = code(ctx => {
    ctx.emit([
        COP[0x06](byte(0x0D)),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        LDA.imm(word(0xFFFF)),
        STA.abs(0x0D98),
        JMP(func_0BE6BA),
    ]);
});

const code_0BE7A5 = code(ctx => {
    const code_0BE7EE = label();
    const code_0BE7CC = label();
    ctx.emit([
        LDA.abs(0x0D98),
        BEQ(code_0BE7EE),
        DEC.acc(),
        BNE(code_0BE7CC),
        COP[0x06](byte(10)),
        LDA.abs(0x0D90),
        INC.acc(),
        AND.imm(word(0x0001)),
        STA.abs(0x0D90),
        COP[0x6B](widestring_0BF625),
        COP[0xBD](asciistring_01EADC),
        LDA.imm(word(0x0380)),
        TSB.abs(0x0658),
        JMP(code_0BE740),

        code_0BE7CC,
        DEC.acc(),
        BNE(code_0BE7EE),
        COP[0x06](byte(10)),
        LDA.abs(0x0D8E),
        INC.acc(),
        AND.imm(word(0x0001)),
        STA.abs(0x0D8E),
        COP[0x6B](widestring_0BF630),
        COP[0xBD](asciistring_01EADC),
        LDA.imm(word(0x0380)),
        TSB.abs(0x0658),
        JMP(code_0BE740),

        code_0BE7EE,
        COP[0x40](word(0x0080), code_0BE7F5),
        RTL(),
    ]);
});

const code_0BE7F5 = code(ctx => {
    ctx.emit([
        COP[0x06](byte(11)),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        LDA.imm(word(0xFFFF)),
        STA.abs(0x0D98),
        LDA.abs(0x0D94),
        JSR(sub_0BE87C),
        PHX(),
        LDA.abs(0x0D94),
        XBA(),
        ASL.acc(),
        TAX(),
        JSL(func_03D9B8),
        LDA.dp(0x18),
        STA.long_x(0x3063FC),
        LDA.dp(0x1C),
        STA.long_x(0x3063FE),
        PLX(),
        JSR(sub_0BEBF9),
        COP[0x6B](widestring_0BF476),
        COP[0xC8](code_0BEB8B),
        COP[0xBD](asciistring_01EADC),
        LDA.abs(0x0D94),
        STA.abs(0x0D92),
        JMP(code_0BE6D0),
    ]);
});

sub_0BE840 = code(ctx => {
    const code_0BE85C = label();
    const code_0BE876 = label();
    ctx.emit([
        PHX(),
        XBA(),
        ASL.acc(),
        TAX(),
        PHX(),
        LDA.stk(0x01),
        CLC(),
        ADC.imm(word(0x0B24)),
        SEC(),
        SBC.imm(word(0x0A00)),
        TAX(),
        LDA.long_x(0x306200),
        CMP.imm(word(0x0002)),
        BCC(code_0BE85C),
        LDA.imm(word(0x0000)),
        code_0BE85C,
        STA.abs(0x0D90),
        LDA.stk(0x01),
        CLC(),
        ADC.imm(word(0x0B26)),
        SEC(),
        SBC.imm(word(0x0A00)),
        TAX(),
        LDA.long_x(0x306200),
        CMP.imm(word(0x0002)),
        BCC(code_0BE876),
        LDA.imm(word(0x0000)),
        code_0BE876,
        STA.abs(0x0D8E),
        PLX(),
        PLX(),
        RTS(),
    ]);
});

sub_0BE87C = code(ctx => {
    ctx.emit([
        PHX(),
        XBA(),
        ASL.acc(),
        TAX(),
        PHX(),
        LDA.stk(0x01),
        CLC(),
        ADC.imm(word(0x0B24)),
        SEC(),
        SBC.imm(word(0x0A00)),
        TAX(),
        LDA.abs(0x0D90),
        STA.long_x(0x306200),
        LDA.stk(0x01),
        CLC(),
        ADC.imm(word(0x0B26)),
        SEC(),
        SBC.imm(word(0x0A00)),
        TAX(),
        LDA.abs(0x0D8E),
        STA.long_x(0x306200),
        PLX(),
        PLX(),
        RTS(),
    ]);
});

func_0BE8A8 = code(ctx => {
    const code_0BE8D4 = label();
    ctx.emit([
        LDA.abs(0x0D74),
        BEQ(code_0BE8D4),
        LDA.abs(0x0D76),
        BEQ(code_0BE8D4),
        LDA.abs(0x0D78),
        BEQ(code_0BE8D4),
        LDA.imm(word(0x0002)),
        STA.abs(0x0D98),
        STZ.abs(0x00EE),
        COP[0x6B](widestring_0BF679),
        LDA.imm(word(0x0001)),
        STA.abs(0x00EE),
        JSR(sub_0BEBF9),
        COP[0x6B](widestring_0BF3F4),
        JMP(code_0BE2DC),
        code_0BE8D4,
        JSR(sub_0BEBF9),
        LDA.imm(word(0x0000)),
        STA.abs(0x0D92),
    ]);
});

const code_0BE8DD = code(ctx => {
    ctx.emit([
        COP[0x6B](widestring_0BF48C),
        COP[0xC8](code_0BEB8B),
        COP[0xBD](asciistring_01EADC),
        COP[0xC2](),
        COP[0x40](word(0x0800), code_0BE905),
        COP[0x40](word(0x0400), code_0BE91E),
        COP[0x40](word(0x0080), code_0BE949),
        COP[0x40](word(0x8000), code_0BE93A),
        RTL(),
    ]);
});

const code_0BE905 = code(ctx => {
    const code_0BE911 = label();
    ctx.emit([
        COP[0x06](byte(10)),
        LDA.abs(0x0D92),
        DEC.acc(),
        BPL(code_0BE911),
        LDA.imm(word(0x0002)),
        code_0BE911,
        STA.abs(0x0D92),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        RTL(),
    ]);
});

const code_0BE91E = code(ctx => {
    const code_0BE92D = label();
    ctx.emit([
        COP[0x06](byte(10)),
        LDA.abs(0x0D92),
        INC.acc(),
        CMP.imm(word(0x0003)),
        BCC(code_0BE92D),
        LDA.imm(word(0x0000)),
        code_0BE92D,
        STA.abs(0x0D92),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        RTL(),
    ]);
});

const code_0BE93A = code(ctx => {
    ctx.emit([
        COP[0x06](byte(0x0D)),
        LDA.imm(word(0x8000)),
        TSB.abs(0x0658),
        JSR(sub_0BEBF9),
        JMP(func_0BE2CC),
    ]);
});

const code_0BE949 = code(ctx => {
    const code_0BE960 = label();
    ctx.emit([
        COP[0x06](byte(11)),
        LDA.abs(0x0656),
        ORA.abs(0x0658),
        STA.abs(0x0658),
        LDA.abs(0x0D92),
        ASL.acc(),
        TAY(),
        LDA.abs_y(0x0D74),
        BNE(code_0BE960),
        RTL(),
    ]);
});

code_list_0BE34C = pointerTable<Code>([ code_0BE354, code_0BEA55, func_0BE8A8, func_0BE6BA ]);

export const layout = list([
    h_sFA_diary_menu,
    e_sFA_diary_menu,
    code_0BE2CA,
    func_0BE2CC,
    code_0BE2D0,
    code_0BE2F6,
    code_0BE30F,
    code_0BE32B,
    code_list_0BE34C,
    code_0BE354,
    code_0BE398,
    code_0BE3B1,
    code_0BE3CD,
    code_0BE3DF,
    code_0BE462,
    code_0BE47D,
    code_0BE498,
    code_0BE4EE,
    code_0BE527,
    sub_0BE673,
    func_0BE6BA,
    code_0BE75B,
    code_0BE774,
    code_0BE790,
    code_0BE7A5,
    code_0BE7F5,
    sub_0BE840,
    sub_0BE87C,
    func_0BE8A8,
    code_0BE8DD,
    code_0BE905,
    code_0BE91E,
    code_0BE93A,
    code_0BE949,
    // ... all other blocks
]); 