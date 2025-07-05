/**
 * Mock TSAL Toolchain/Runtime
 *
 * This file provides a concrete implementation of the SnesContext. It acts as
 * a mock "compiler" or "ROM builder" that processes TSAL modules and data
 * structures to generate a byte array representing the final ROM.
 */
import { SnesContext, Operand, SnesLayoutable } from "./platform";
//import { Layoutable } from "./tsal";

/**
 * A concrete implementation of the SnesContext for building a ROM.
 */
export class RomBuilder implements SnesContext {
    // --- State Management ---
    public romData: number[] = [];
    public currentLocation: number = 0; // Represents the current PC/address in the ROM stream.
    
    // --- Interface Implementation ---

    // The core layout function, overloaded to handle single objects or arrays.
    public emit(data: SnesLayoutable | SnesLayoutable[]): void {
        if (Array.isArray(data)) {
            // If it's an array, process each element sequentially.
            // This is the implementation of your pseudo-code.
            for (const l of data) {
                this.processLayoutable(l);
            }
        } else {
            // If it's a single object, just process it.
            this.processLayoutable(data);
        }
    }

    // This is the private helper that contains your core logic.
    private processLayoutable(l: SnesLayoutable) {
        // First, check if the object is Locational and needs its address set.
        // We use a type guard to see if 'location' is a property.
        // We cast to 'any' to make the property mutable for this assignment.
        if ('location' in l) {
            (l as any).location = this.currentLocation;
        }

        // Now, call the object's own layout function, passing this context.
        // This is the key step that translates the high-level object into low-level bytes.
        l.layout(this);
    }

    /** The primitive for writing raw bytes to the ROM stream. */
    emitBytes(bytes: number[]): void {
        this.romData.push(...bytes);
        this.currentLocation += bytes.length;
    }
    
    // --- COP Implementation ---
    // We use a Proxy to dynamically handle `ctx.COP[0xCA]` calls.
    // This now returns a SnesLayoutable object, consistent with the declarative model.
    COP = new Proxy({}, {
        get: (target, prop) => {
            const copOpcode = Number(prop);
            if (isNaN(copOpcode)) return undefined;

            // Return a function that, when called with parameters,
            // returns a layoutable object for that specific COP instruction.
            return (...params: number[]): SnesLayoutable => ({
                layout: (ctx: SnesContext) => {
                    // The layout function simply emits the COP opcode, its own specific
                    // opcode, and any parameters.
                    ctx.emitBytes([0x02, copOpcode, ...params.flat()]);
                }
            });
        }
    }) as SnesContext['COP'];
} 