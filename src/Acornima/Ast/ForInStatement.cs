using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Left), nameof(Right), nameof(Body) })]
public sealed partial class ForInStatement : Statement
{
    public ForInStatement(
        Node left,
        Expression right,
        Statement body) : base(NodeType.ForInStatement)
    {
        Left = left;
        Right = right;
        Body = body;
    }

    /// <remarks>
    /// <see cref="VariableDeclaration"/> (may have an initializer in non-strict mode) | <see cref="Identifier"/> | <see cref="MemberExpression"/> | <see cref="ArrayPattern"/> | <see cref="ObjectPattern"/>
    /// </remarks>
    public Node Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private ForInStatement Rewrite(Node left, Expression right, Statement body)
    {
        return new ForInStatement(left, right, body);
    }
}
