using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed partial class YieldExpression : Expression
{
    public YieldExpression(Expression? argument, bool @delegate)
        : base(NodeType.YieldExpression)
    {
        Argument = argument;
        Delegate = @delegate;
    }

    public Expression? Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Delegate { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private YieldExpression Rewrite(Expression? argument)
    {
        return new YieldExpression(argument, Delegate);
    }
}
