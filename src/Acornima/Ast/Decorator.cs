using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Expression) })]
public sealed partial class Decorator : Node
{
    public Decorator(Expression expression) : base(NodeType.Decorator)
    {
        Expression = expression;
    }

    public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private Decorator Rewrite(Expression expression)
    {
        return new Decorator(expression);
    }
}
