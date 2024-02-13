namespace Acornima.Ast;

[VisitableNode]
public sealed partial class Super : Expression
{
    public Super() : base(NodeType.Super)
    {
    }
}
