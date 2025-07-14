/**
 * Mock Game Definition File
 * 
 * This file provides mock implementations for game-specific data structures
 * and helpers that are imported by TSAL modules.
 */

import { byte, SnesContext, SnesLayoutable } from './platform';
import { Locational } from './tsal';

// A mock Address type for type-safety in this example.
// In a real system, this might be a more complex object.


// 1. Mock 'mem' object
// This provides type-safe access to memory locations.
export const mem = {
    dp: {
        actor: {
            flags10: 0x10,
            flags12: 0x12
        }
    },
    abs: {
        inventory_slots: 0x0AB4,
        inventory_equipped_index: 0x0AC4,
        inventory_equipped_type: 0x0AC6,
        lily_state_0AA6: 0x0AA6,
        some_var_064A: 0x064A,
        player_actor: 0x09AA,
        player_flags: 0x09AE,
    },
    // System addresses from sFA_diary_menu
    scene_next: 0x0642,
    joypad_mask_std: 0x065A,
    joypad_mask_inv: 0x065C,
    M7A: 0x211B,
    M7B: 0x211C,
    M7C: 0x211D,
    M7D: 0x211E,
    M7X: 0x211F,
    M7Y: 0x2120,
    W34SEL: 0x2124,
    WOBJSEL: 0x2125,
    _TM: 0x212C,
    _TS: 0x212D,
    CGWSEL: 0x2130,
    CGADSUB: 0x2131,
    APUIO0: 0x2140,
    RDNMI: 0x4210,
    JOY2L: 0x421A,
};

// 2. Mock 'WideString' template literal tag
// This is now a factory that returns a full SnesLayoutable object.
export interface WideStringObject extends SnesLayoutable, Locational {
    readonly content: string;
    readonly location: number;
}

/**
 * Creates a layoutable object representing a game-specific string.
 * The layout function for this object contains a mock parser that converts
 * control codes (e.g., [END]) and characters into their byte representations.
 * @param strings The template literal strings array.
 * @param values The interpolated values.
 * @returns A SnesLayoutable object.
 */
export const WideString = (strings: TemplateStringsArray, ...values: any[]): WideStringObject => {
    const raw = strings.raw.reduce((acc, str, i) => acc + str + (values[i] || ''), '');
    return {
        content: raw,
        layout: (ctx: SnesContext) => {
            const bytes: number[] = [];
            // This is a mock parser. A real implementation would be more robust.
            let i = 0;
            while (i < raw.length) {
                if (raw[i] === '[') {
                    const end = raw.indexOf(']', i);
                    if (end !== -1) {
                        const command = raw.substring(i + 1, end);
                        // Map string commands to their byte values
                        switch (command.toUpperCase()) {
                            case 'DEF': bytes.push(0xFA); break;
                            case 'N': bytes.push(0xFB); break; // Newline
                            case 'END': bytes.push(0xFF); break;
                            default: break; // Ignore unknown commands
                        }
                        i = end + 1;
                        continue;
                    }
                }
                // A real implementation would use a game-specific character map here.
                bytes.push(raw.charCodeAt(i));
                i++;
            }
            ctx.emitBytes(bytes);
        }
    };
};


// 3. Mock 'h_actor' helper function
// This would create a data structure representing an actor header.
interface ActorHeader extends SnesLayoutable, Partial<Locational> {
    p1: number; p2: number; p3: number;
}
export const h_actor = (p1: number, p2: number, p3: number): ActorHeader => ({
    p1, p2, p3,
    layout: (ctx: SnesContext) => {
        // We use the compositional 'emit' function, which is cleaner than
        // emitting bytes directly from a high-level component.
        ctx.emit(byte(p1, p2, p3));
    }
});

// --- 4. COP (Coprocessor) Module ---
// This mock demonstrates how to support both named (`COP.D2`) and numeric (`COP[0xD2]`)
// access for coprocessor instructions, as per the architectural goal.

/**
 * This represents the data structure for a single COP instruction call.
 * The toolchain's `ctx()` function would know how to process this object.
 */
export interface CopInstruction extends SnesLayoutable {
    readonly opcode: number;
    readonly params: any[];
}

// A function that generates a CopInstruction object.
type CopInstructionCreator = (...params: any[]) => CopInstruction;

// Interface for named, type-safe handlers for known COP codes.
interface NamedCopHandlers {
    /** An example of a documented, type-safe COP function. */
    D2: (param1: number, param2: number) => CopInstruction;
    CA: (param1: number) => CopInstruction;
    // Add other known, named handlers here for type-safety and auto-complete.
}

// Interface for numeric lookups, e.g., `COP[0xCA]`.
interface IndexedCopHandlers {
    [code: number]: CopInstructionCreator;
}

const copHandlerRegistry: Record<string | number, CopInstructionCreator> = {};

function createCopCreator(opcode: number): CopInstructionCreator {
    return (...params: any[]): CopInstruction => ({
        opcode,
        params,
        layout: (ctx: SnesContext) => {
            // The layout logic is now part of the object itself.
            ctx.emitBytes([0x02, opcode, ...params.flat()]);
        }
    });
}

/** Registers a COP handler so it can be accessed by name and number. */
function registerCopHandler(opcode: number, name?: string) {
    const creator = createCopCreator(opcode);
    copHandlerRegistry[opcode] = creator;
    if (name) {
        copHandlerRegistry[name] = creator;
    }
}

// Register all COP commands from the na4B_spirit.ts example.
registerCopHandler(0xD2, 'D2');
registerCopHandler(0xCA, 'CA');
registerCopHandler(0x80);
registerCopHandler(0x89);
registerCopHandler(0x21);
registerCopHandler(0xBC);
registerCopHandler(0xCB);
registerCopHandler(0xDA);
registerCopHandler(0xBF);
registerCopHandler(0xCC);
registerCopHandler(0x87);
registerCopHandler(0x8A);
registerCopHandler(0xE0);
// New handlers from inventory_menu
registerCopHandler(0x88);
registerCopHandler(0xBD);
registerCopHandler(0x9C);
registerCopHandler(0xC2);
registerCopHandler(0xD9);
registerCopHandler(0x06);
registerCopHandler(0x40);
registerCopHandler(0xA7);
registerCopHandler(0xA9);
registerCopHandler(0xC1);
registerCopHandler(0x8B);
registerCopHandler(0x8D);
// New handlers from st68_lily
registerCopHandler(0xDA);
registerCopHandler(0x0B);
registerCopHandler(0xC0);
registerCopHandler(0x25);
registerCopHandler(0x85);
registerCopHandler(0x84);
registerCopHandler(0x26);
// New handlers from sFA_diary_menu
registerCopHandler(0x6B);
registerCopHandler(0xC8);
registerCopHandler(0xE2);
registerCopHandler(0xC5);

// A Proxy creates a dynamic COP object that handles both named properties (COP.D2)
// and numeric indices (COP[0xD2]), fulfilling the design for interchangeable syntax.
const copProxy = new Proxy({}, {
    get(target, prop) {
        const propKey = String(prop);

        // Check for a registered named handler (e.g., 'D2').
        if (copHandlerRegistry[propKey]) {
            return copHandlerRegistry[propKey];
        }

        // Try to parse as a number for numeric index access (e.g., '210' for 0xD2).
        const asNumber = Number(propKey);
        if (!isNaN(asNumber) && copHandlerRegistry[asNumber]) {
            return copHandlerRegistry[asNumber];
        }
        
        // For flexibility, create a generic handler on the fly for unregistered codes.
        if (!isNaN(asNumber)) {
             return createCopCreator(asNumber);
        }

        // Return undefined for other properties (like `toString`).
        return undefined;
    }
});

/**
 * The exported `COP` object.
 * It is typed to combine the named and indexed handlers, providing
 * IDE auto-complete for known commands while remaining flexible.
 */
export const COP: NamedCopHandlers & IndexedCopHandlers = copProxy as any; 