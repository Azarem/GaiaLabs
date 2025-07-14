/**
 * Mock TSAL Core Definitions (v3 - Generic/Delegation Model)
 *
 * This file provides the core type definitions for the TSAL language itself.
 * The core principle is that the `ILayoutable` part contains all the specific
 * logic, and the `AstNode` is a generic structural wrapper.
 */

// --- Interfaces for the Build Passes ---

/** The context passed during the Organization pass to enable optimizations. */
export interface OrganizationContext {
    getDistance(from: AstNode<any>, to: AstNode<any>): number | undefined;
}

/** The context passed to an EmitFunction, containing the final address map. */
export interface EmitContext {
    readonly addressMap: Map<ILocatable, number>;
    readonly emitBytes: (bytes: number[]) => void;
}

// --- Core Architectural Interfaces ---

/** An object that can be located at a specific address in the final ROM. */
export interface ILocatable {
    address: number;
}

/**
 * The "Brain": Represents any part of the assembly language.
 * An ILayoutable implementation holds all the specific logic for how
 * it is sized, organized, and written to the ROM.
 */
export interface ILayoutable {
    /**
     * Pass 2: An optional method to optimize this part.
     * It can return a new, more efficient ILayoutable to replace itself.
     */
    organize?(ctx: OrganizationContext, node: AstNode<this>): ILayoutable;

    /** Pass 3: Calculates the size of this part in bytes. */
    getSize(): number;

    /** Pass 4: Writes the binary representation of this part. */
    emit(ctx: EmitContext): void;
    
    /** Pass 1: Returns the direct children of this part for AST construction. */
    getChildren(): ILayoutable[];
}

/**
 * The "Skeleton": A generic node in the Abstract Syntax Tree.
 * Its only job is to wrap a part, manage children, and delegate all
 * compilation logic back to the part.
 */
export class AstNode<T extends ILayoutable> {
    part: T;
    children: AstNode<ILayoutable>[];
    parent: AstNode<ILayoutable> | null = null;

    constructor(part: T) {
        this.part = part;
        // Pass 1: Recursively build the tree.
        this.children = this.part.getChildren().map(p => {
            const childNode = new AstNode(p);
            childNode.parent = this;
            return childNode;
        });
    }

    // The node's 'organize' simply calls the part's 'organize'
    // and rebuilds its own children if the part changes itself.
    organize(ctx: OrganizationContext): void {
        if (this.part.organize) {
            this.part = this.part.organize(ctx, this) as T;
        }
        // Then, recursively organize children.
        this.children.forEach(c => c.organize(ctx));
    }

    // The node's 'getSize' delegates to the part and sums up the children.
    getSize(): number {
        const partSize = this.part.getSize();
        const childrenSize = this.children.reduce((sum, c) => sum + c.getSize(), 0);
        return partSize + childrenSize;
    }

    // The node's 'emit' delegates to the part, then tells children to emit.
    emit(ctx: EmitContext): void {
        this.part.emit(ctx);
        this.children.forEach(c => c.emit(ctx));
    }
}