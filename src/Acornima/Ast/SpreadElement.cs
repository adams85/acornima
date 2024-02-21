using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed partial class SpreadElement : Expression
{
    public SpreadElement(Expression argument) : base(NodeType.SpreadElement)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private SpreadElement Rewrite(Expression argument)
    {
        return new SpreadElement(argument);
    }
}
