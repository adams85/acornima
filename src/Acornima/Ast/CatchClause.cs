using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Param), nameof(Body) })]
public sealed partial class CatchClause : Node
{
    public CatchClause(Node? param, BlockStatement body) :
        base(NodeType.CatchClause)
    {
        Param = param;
        Body = body;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="ArrayPattern"/> | <see cref="ObjectPattern"/> | <see langword="null"/>
    /// </remarks>
    public Node? Param { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public BlockStatement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private CatchClause Rewrite(Node? param, BlockStatement body)
    {
        return new CatchClause(param, body);
    }
}
