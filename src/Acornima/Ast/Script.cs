using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public sealed class Script : Program
{
    public Script(in NodeList<Statement> body, bool strict)
        : base(SourceType.Script, body)
    {
        Strict = strict;
    }

    public new bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    private protected override bool GetStrict() => Strict;

    protected override Program Rewrite(in NodeList<Statement> body)
    {
        return new Script(body, Strict);
    }
}
