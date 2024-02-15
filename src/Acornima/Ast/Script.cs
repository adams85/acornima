namespace Acornima.Ast;

public sealed class Script : Program
{
    public Script(in NodeList<Statement> body, bool strict)
        : base(SourceType.Script, body, strict)
    {
    }

    protected override Program Rewrite(in NodeList<Statement> body)
    {
        return new Script(body, Strict);
    }
}
