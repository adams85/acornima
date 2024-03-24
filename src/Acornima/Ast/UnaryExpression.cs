using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) }, SealOverrideMethods = true)]
public abstract partial class UnaryExpression : Expression
{
    private protected UnaryExpression(NodeType type, Operator op, Expression arg, bool prefix)
        : base(type)
    {
        Operator = op;
        Argument = arg;
        Prefix = prefix;
    }

    public Operator Operator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Prefix { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected abstract UnaryExpression Rewrite(Expression argument);
}
