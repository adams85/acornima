using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Block), nameof(Handler), nameof(Finalizer) })]
public sealed partial class TryStatement : Statement
{
    public TryStatement(
        NestedBlockStatement block,
        CatchClause? handler,
        NestedBlockStatement? finalizer)
        : base(NodeType.TryStatement)
    {
        Block = block;
        Handler = handler;
        Finalizer = finalizer;
    }

    public NestedBlockStatement Block { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public CatchClause? Handler { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public NestedBlockStatement? Finalizer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private TryStatement Rewrite(NestedBlockStatement block, CatchClause? handler, NestedBlockStatement? finalizer)
    {
        return new TryStatement(block, handler, finalizer);
    }
}
