namespace Acornima.Ast;

[VisitableNode]
public sealed partial class EmptyStatement : Statement
{
    public EmptyStatement() : base(NodeType.EmptyStatement)
    {
    }
}
