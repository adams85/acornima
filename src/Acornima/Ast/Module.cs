namespace Acornima.Ast;

public sealed class Module : Program
{
    public Module(in NodeList<Statement> body)
        : base(SourceType.Module, body) { }

    private protected override bool GetStrict() => true;

    protected override Program Rewrite(in NodeList<Statement> body)
    {
        return new Module(body);
    }
}
