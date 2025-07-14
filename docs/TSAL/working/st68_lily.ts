import { word, code, byte, SnesLayoutable, pointerTable, Code } from './platform';
import { mem, WideString, h_actor, COP } from './game';
import { LDA, STA, TRB, TSB, BRA, RTL, JSL, INC } from './op';

// --- Module Metadata ---
export const moduleInfo = {
  name: 'ST68Lily',
  dependencies: [],
  description: 'Seaside Tunnel, Lilly NPC logic and dialogue'
};

// --- Forward Declarations & Placeholders ---
const func_00C6E4 = code(ctx => {}); // Placeholder for external function (this should be an import)

// --- Reference Context: Declarations ---

// Forward declare all code blocks defined in this file
export let e_st68_lily: Code,
    code_list_06B4FB: SnesLayoutable,
    code_06B501: Code,
    code_06B51F: Code,
    code_06B571: Code,
    code_06B578: Code,
    widestring_06B57D: SnesLayoutable,
    widestring_06B606: SnesLayoutable,
    widestring_06B661: SnesLayoutable,
    widestring_06B67B: SnesLayoutable;


export const h_st68_lily = h_actor(0x23, 0x00, 0x10);

widestring_06B57D = WideString`[TPL:B][TPL:2]Lilly:[N][LU1:3D]are strange...[FIN]
I am afraid the longer [N]we travel in [LU1:D6]tunnel, [N]the easier it [LU1:EF]be to [N][LU2:6A]why we are [LU2:74][FIN]
[LU1:30]all [LU1:6F][N][LU1:C1][LU1:F2][LU1:D7]way.[PAL:0][END]`;

widestring_06B606 = WideString`[TPL:A][TPL:0]Eighth day in the [N]tunnel.[FIN]
Unable to sleep. [N]I stared at an  [N]underground river.....[PAL:0][END]`;

widestring_06B661 = WideString`[TPL:A][TPL:2]Lilly:[N]Can't sleep?[PAL:0][END]`;

widestring_06B67B = WideString`[PAU:1E][TPL:B][TPL:0]
Will: No. [N]I'm [LU1:AB]for [LU1:B7][N]mushrooms, [LU1:A3]kidding. [FIN]
[TPL:2][LU1:2A]Will. [N][LU1:67][LU1:7F]during [N][LU1:D6]journey. [FIN]
[LU1:49]you've [N]grown up. [FIN]
[TPL:0]Will: [N]I [LU1:82][LU1:E9]it [N]myself, but.... [FIN]
I can use [LU1:D0]strange[N]power, and my [LU1:72]has[N][LU1:7F]to the body[N]of a warrior.[FIN]
The [LU2:52][LU1:CC]to [N][LU1:98][LU1:CB][LU1:F6]my [N][LU2:67][LU2:C1]to the [N]Tower of Babel. [FIN]
I'm [LU1:A3]starting to[N][LU1:E9][LU1:D7]power.[FIN]
Why did you join this[N]dangerous expedition?[FIN]
[TPL:2][LU1:2A]At [LU2:61]it was [N][LU1:A3]for fun. But now [N][LU1:9F]a secret. Heh heh. [FIN]
We [LU1:EF]walk all day [N]again tomorrow... [N][LU1:2B]get [LU1:D0]sleep...[PAL:0][END]`;


// --- Generation Context: Code & Data ---

// Define the code blocks first, in an order that respects dependencies.
code_06B578 = code(ctx => ctx.emit([
    COP[0xBF](widestring_06B57D),
    RTL(),
]));

code_06B501 = code(ctx => ctx.emit([
    COP[0xD2](byte(0x01), byte(0x01)),
    COP[0xDA](byte(0x3F)),
    COP[0x80](byte(0x25)),
    COP[0x89](),
    COP[0x0B](),
    COP[0xC0](code_06B578),
    COP[0xD2](byte(0x04), byte(0x01)),
    COP[0x80](byte(0x23)),
    COP[0x89](),
    COP[0xC1](),
    RTL(),
]));

code_06B51F = code(ctx => ctx.emit([
    COP[0x25](byte(0x11), byte(0x1C)),
    LDA.imm(word(0xCFF0)),
    TSB.abs(mem.joypad_mask_std),
    COP[0xDA](byte(0x1D)),
    COP[0xBF](widestring_06B606),
    COP[0x85](byte(0x28), byte(0x02), byte(0x12)),
    COP[0x8A](),
    COP[0x84](byte(0x24), byte(0x28)),
    COP[0x8A](),
    COP[0x85](byte(0x28), byte(0x02), byte(0x02)),
    COP[0x8A](),
    COP[0x84](byte(0x24), byte(0x1E)),
    COP[0x8A](),
    COP[0xBF](widestring_06B661),
    LDA.imm(word(0x0003)),
    JSL(func_00C6E4),
    COP[0xC2](),
    COP[0xBF](widestring_06B67B),
    INC.abs(mem.abs.lily_state_0AA6),
    LDA.imm(word(0x0404)),
    STA.abs(mem.abs.some_var_064A),
    COP[0x26](byte(0x68), word(0x0160), word(0x01C0), byte(0x00), word(0x2211)),
    COP[0xC1](),
    RTL(),
]));

code_06B571 = code(ctx => ctx.emit([
    COP[0x25](byte(0x19), byte(0x1D)),
    COP[0xC1](),
    RTL(),
]));

// Now define the list that references them
code_list_06B4FB = pointerTable<Code>([
    code_06B501,
    code_06B51F,
    code_06B571,
]);

// Finally, define the entry point that uses the list
e_st68_lily = code(ctx => ctx.emit([
    LDA.abs(mem.abs.lily_state_0AA6),
    STA.dp(0x00),
    COP[0xD9](word(0x0000), code_list_06B4FB),
]));


// --- Module Layout Definition ---
export const layout = code((ctx) => ctx.emit([
    h_st68_lily,
    e_st68_lily,
    code_list_06B4FB,
    code_06B501,
    code_06B51F,
    code_06B571,
    code_06B578,
    widestring_06B57D,
    widestring_06B606,
    widestring_06B661,
    widestring_06B67B,
])); 