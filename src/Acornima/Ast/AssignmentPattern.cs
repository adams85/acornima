using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Left), nameof(Right) })]
public sealed partial class AssignmentPattern : Node, IBindingPattern
{
    public AssignmentPattern(Node left, Expression right) : base(NodeType.AssignmentPattern)
    {
        Left = left;
        Right = right;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="MemberExpression"/> (in assignment contexts only) | <see cref="ArrayPattern"/> | <see cref="ObjectPattern"/>
    /// </remarks>
    public Node Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private AssignmentPattern Rewrite(Node left, Expression right)
    {
        return new AssignmentPattern(left, right);
    }
}
