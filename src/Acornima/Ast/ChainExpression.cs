using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Expression) })]
public sealed partial class ChainExpression : Expression
{
    public ChainExpression(Expression expression) : base(NodeType.ChainExpression)
    {
        Expression = expression;
    }

    /// <remarks>
    /// <see cref="CallExpression"/> | <see cref="MemberExpression"/>
    /// </remarks>
    public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ChainExpression Rewrite(Expression expression)
    {
        return new ChainExpression(expression);
    }
}
