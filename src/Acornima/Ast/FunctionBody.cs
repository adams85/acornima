using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public sealed class FunctionBody : BlockStatement, IHoistingScope
{
    public FunctionBody(in NodeList<Statement> body, bool strict)
        : base(NodeType.BlockStatement, body)
    {
        Strict = strict;
    }

    public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected override BlockStatement Rewrite(in NodeList<Statement> body)
    {
        return new FunctionBody(body, Strict);
    }
}
