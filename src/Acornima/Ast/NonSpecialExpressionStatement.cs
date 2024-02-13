namespace Acornima.Ast;

public sealed class NonSpecialExpressionStatement : ExpressionStatement
{
    public NonSpecialExpressionStatement(Expression expression) : base(expression)
    {
    }

    protected override ExpressionStatement Rewrite(Expression expression)
    {
        return new NonSpecialExpressionStatement(expression);
    }
}
