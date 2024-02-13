namespace Acornima.Ast;

public sealed class NullLiteral : Literal
{
    public NullLiteral(string raw) : base(TokenKind.NullLiteral, raw) { }

    protected override object? GetValue() => null;
}
