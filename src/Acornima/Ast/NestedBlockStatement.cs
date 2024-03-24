namespace Acornima.Ast;

public sealed class NestedBlockStatement : BlockStatement
{
    public NestedBlockStatement(in NodeList<Statement> body)
        : base(NodeType.BlockStatement, body) { }

    protected override BlockStatement Rewrite(in NodeList<Statement> body)
    {
        return new NestedBlockStatement(body);
    }
}
