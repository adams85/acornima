using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Id), nameof(Init) })]
public sealed partial class VariableDeclarator : Node
{
    public VariableDeclarator(Node id, Expression? init)
        : base(NodeType.VariableDeclarator)
    {
        Id = id;
        Init = init;
    }

    /// <remarks>
    /// In the case of var, let or const variable declarations: <see cref="Identifier"/> | <see cref="ArrayPattern"/> | <see cref="ObjectPattern"/><br />
    /// In the case of using and await using const variable declarations: <see cref="Identifier"/>
    /// </remarks>
    public Node Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? Init { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private VariableDeclarator Rewrite(Node id, Expression? init)
    {
        return new VariableDeclarator(id, init);
    }
}
