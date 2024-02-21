using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed partial class AwaitExpression : Expression
{
    public AwaitExpression(Expression argument) : base(NodeType.AwaitExpression)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private AwaitExpression Rewrite(Expression argument)
    {
        return new AwaitExpression(argument);
    }
}
