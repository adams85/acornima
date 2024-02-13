using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Left), nameof(Right) }, SealOverrideMethods = true)]
public abstract partial class BinaryExpression : Expression
{
    private protected BinaryExpression(NodeType type, Operator op, Expression left, Expression right) : base(type)
    {
        Operator = op;
        Left = left;
        Right = right;
    }

    public Operator Operator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected abstract BinaryExpression Rewrite(Expression left, Expression right);
}
