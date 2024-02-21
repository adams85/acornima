using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed partial class ThrowStatement : Statement
{
    public ThrowStatement(Expression argument) : base(NodeType.ThrowStatement)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private ThrowStatement Rewrite(Expression argument)
    {
        return new ThrowStatement(argument);
    }
}
