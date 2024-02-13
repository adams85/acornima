using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Pattern) })]
public sealed partial class ParenthesizedPattern : Node, IBindingPattern
{
    public ParenthesizedPattern(Node pattern) : base(NodeType.ParenthesizedPattern)
    {
        Pattern = pattern;
    }

    public Node Pattern { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ParenthesizedPattern Rewrite(Node pattern)
    {
        return new ParenthesizedPattern(pattern);
    }
}
