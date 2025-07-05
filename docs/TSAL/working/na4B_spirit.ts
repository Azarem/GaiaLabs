import { word, SnesLayoutable, code } from './platform'
import { mem, WideString, h_actor, COP } from './game'
import { LDA, TRB, TSB, BRA } from './op'

// --- Module Metadata ---
export const moduleInfo = {
  name: 'Na4BSpirit',
  dependencies: [],
  description: 'Nazca area Spirit entity and dialogue'
};

// --- Reference Context: Declarations ---
export const h_na4B_spirit = h_actor(0x32, 0x00, 0x30);
export const widestring_05F349 = WideString`[DEF]Ku ku ku...[END]`;

export const e_na4B_spirit = code((ctx) => ctx.emit([
    COP.D2(0x06, 0x01),
    COP.D2(0x07, 0x01),
    LDA.imm(word(0x2000)),
    TRB.dp(mem.dp.actor.flags10),
    LDA.imm(word(0x0200)),
    TSB.dp(mem.dp.actor.flags12),
    COP.CA(0x03)
]));

export const code_05F307 = code((ctx) => ctx.emit([
    COP[0x80](0x32),
    COP[0x89](),
    COP[0x21](0x02, code_05F313),
    BRA(code_05F307)
]));

export const code_05F313 = code((ctx) => ctx.emit([
    COP[0xBC](0x70, 0x00),
    COP[0xCB](),
    LDA.imm(word(0xFFF0)),
    TSB.abs(mem.joypad_mask_std),
    COP[0xDA](0x13),
    COP[0xBF](widestring_05F349),
    LDA.imm(word(0xFFF0)),
    TRB.abs(mem.joypad_mask_std),
    COP[0xCC](0x02),
    COP[0x87](0x32, 0x02, 0x11, 0x12),
    COP[0x8A](),
    COP[0x87](0x32, 0x02, 0x01, 0x02),
    COP[0x8A](),
    COP[0x87](0x32, 0x0D, 0x03, 0x04),
    COP[0x8A](),
    COP[0xE0]()
]));

// --- Module Layout Definition ---
// This single export tells the toolchain which functions to lay out and in what order.
// This makes layout explicit and prevents unintentional ordering.
export const layout = code((ctx) => ctx.emit([
    h_na4B_spirit,
    e_na4B_spirit,
    code_05F307,
    code_05F313,
    widestring_05F349,
]));
