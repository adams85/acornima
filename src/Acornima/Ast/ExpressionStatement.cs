using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Expression) }, SealOverrideMethods = true)]
public abstract partial class ExpressionStatement : Statement
{
    private protected ExpressionStatement(Expression expression)
        : base(NodeType.ExpressionStatement)
    {
        Expression = expression;
    }

    public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected abstract ExpressionStatement Rewrite(Expression expression);
}
