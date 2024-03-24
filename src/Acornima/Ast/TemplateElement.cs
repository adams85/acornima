using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode]
public sealed partial class TemplateElement : Node
{
    public TemplateElement(TemplateValue value, bool tail)
        : base(NodeType.TemplateElement)
    {
        Value = value;
        Tail = tail;
    }

    public TemplateValue Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Tail { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
