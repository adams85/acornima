using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Expression) })]
public sealed partial class ParenthesizedExpression : Expression
{
    public ParenthesizedExpression(Expression expression)
        : base(NodeType.ParenthesizedExpression)
    {
        Expression = expression;
    }

    public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private ParenthesizedExpression Rewrite(Expression expression)
    {
        return new ParenthesizedExpression(expression);
    }
}
