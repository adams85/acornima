namespace Acornima.Ast;

[VisitableNode]
public sealed partial class DebuggerStatement : Statement
{
    public DebuggerStatement()
        : base(NodeType.DebuggerStatement) { }
}
