namespace Acornima.Ast;

public abstract class StatementOrExpression : Node
{
    private protected StatementOrExpression(NodeType type)
        : base(type) { }
}
