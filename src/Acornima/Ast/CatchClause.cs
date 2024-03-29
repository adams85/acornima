using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Param), nameof(Body) })]
public sealed partial class CatchClause : Node
{
    public CatchClause(Node? param, NestedBlockStatement body)
        : base(NodeType.CatchClause)
    {
        Param = param;
        Body = body;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="ArrayPattern"/> | <see cref="ObjectPattern"/> | <see langword="null"/>
    /// </remarks>
    public Node? Param { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public NestedBlockStatement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private CatchClause Rewrite(Node? param, NestedBlockStatement body)
    {
        return new CatchClause(param, body);
    }
}
