using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed partial class ReturnStatement : Statement
{
    public ReturnStatement(Expression? argument)
        : base(NodeType.ReturnStatement)
    {
        Argument = argument;
    }

    public Expression? Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private ReturnStatement Rewrite(Expression? argument)
    {
        return new ReturnStatement(argument);
    }
}
