namespace Acornima.Ast;

/// <summary>
/// A JavaScript expression. 
/// </summary>
public abstract class Expression : StatementOrExpression
{
    protected Expression(NodeType type) : base(type)
    {
    }
}
