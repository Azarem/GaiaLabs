import { ILayoutable, ILocatable, OrganizationContext, EmitContext, AstNode } from './tsal';

// A type alias for a function that returns a list of parts.
// This is the function developers will write.
export type LayoutBuilder = () => ILayoutable[];

// --- Leaf Parts (No Children) ---

export class Label implements ILayoutable, ILocatable {
    address = -1;
    // A label has no size, no output, and no children. It's just a marker.
    getSize() { return 0; }
    emit(ctx: EmitContext) {}
    getChildren() { return []; }
}

export class BRA implements ILayoutable {
    constructor(public target: Label) {}
    getSize() { return 2; } // Opcode + 8-bit offset
    emit(ctx: EmitContext) {
        // In a real implementation, this would calculate the offset
        // using the addressMap from the context.
        console.log(`Emitting BRA to ${this.target.constructor.name}`);
        ctx.emitBytes([0x80, 0x00]); // 0x80 is BRA
    }
    getChildren() { return []; }
}

export class JMP implements ILayoutable {
    constructor(public target: Label) {}
    getSize() { return 3; } // Opcode + 16-bit address
    emit(ctx: EmitContext) {
        console.log(`Emitting JMP to ${this.target.constructor.name}`);
        ctx.emitBytes([0x4C, 0x00, 0x00]); // 0x4C is JMP
    }
    getChildren() { return []; }
}

// --- The Agnostic Part ---

export class AgnosticJump implements ILayoutable {
    constructor(public target: Label) {}

    // The 'organize' method is the key. It replaces itself with a more
    // efficient part if it can.
    organize(ctx: OrganizationContext, node: AstNode<this>): ILayoutable {
        // const distance = ctx.getDistance(node, findNodeFor(this.target));
        const isClose = true; // Placeholder for real distance check

        if (isClose) {
            console.log('Optimizing AgnosticJump to BRA');
            return new BRA(this.target); // <-- Return a new, better part!
        }
        console.log('AgnosticJump resolves to JMP');
        return new JMP(this.target);
    }
    
    // Before organization, it assumes the worst-case size.
    getSize() { return 3; }
    // This should never be called, as 'organize' will always replace it.
    emit(ctx: EmitContext) { throw new Error("AgnosticJump was not organized!"); }
    getChildren() { return []; }
}


// --- Container Part ---

/**
 * A CodeBlock is a container part. Its job is to execute a builder
 * function and expose the results as its children.
 */
export class CodeBlock implements ILayoutable {
    // The builder is now a private implementation detail.
    constructor(private builder: LayoutBuilder) {}

    // Its children are the result of executing the builder function.
    getChildren() {
        return this.builder();
    }

    // A simple container has no size or output of its own.
    // All work is done by its children.
    getSize() { return 0; }
    emit(ctx: EmitContext) {}
}

/**
 * High-level function to create an 'if' block.
 * This is the API the developer would interact with.
 */
export function If(condition: ILayoutable[], builder: LayoutBuilder) {
    const endLabel = new Label();
    return new CodeBlock(() => [
        ...condition,
        new AgnosticJump(endLabel), // Invert condition and jump past 'then'
        ...builder(),
        endLabel
    ]);
} 