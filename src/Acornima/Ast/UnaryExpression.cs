using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) }, SealOverrideMethods = true)]
public abstract partial class UnaryExpression : Expression
{
    private protected UnaryExpression(NodeType type, Operator op, Expression arg)
        : base(type)
    {
        Operator = op;
        Argument = arg;
    }

    public Operator Operator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Prefix { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetPrefix(); }
    private protected abstract bool GetPrefix();

    protected abstract UnaryExpression Rewrite(Expression argument);
}
