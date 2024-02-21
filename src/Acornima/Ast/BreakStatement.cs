using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Label) })]
public sealed partial class BreakStatement : Statement
{
    public BreakStatement(Identifier? label) : base(NodeType.BreakStatement)
    {
        Label = label;
    }

    public Identifier? Label { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private BreakStatement Rewrite(Identifier? label)
    {
        return new BreakStatement(label);
    }
}
