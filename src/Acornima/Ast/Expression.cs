namespace Acornima.Ast;

public abstract class Expression : StatementOrExpression
{
    protected Expression(NodeType type) : base(type)
    {
    }
}
