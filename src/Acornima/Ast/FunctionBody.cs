namespace Acornima.Ast;

public sealed class FunctionBody : BlockStatement
{
    public FunctionBody(in NodeList<Statement> body) : base(NodeType.BlockStatement, body)
    {
    }

    protected override BlockStatement Rewrite(in NodeList<Statement> body)
    {
        return new FunctionBody(body);
    }
}
