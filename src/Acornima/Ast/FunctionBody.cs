using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Body) })]
public sealed partial class FunctionBody : BlockStatement, IHoistingScope
{
    public FunctionBody(in NodeList<Statement> body, bool strict)
        : base(NodeType.BlockStatement, body)
    {
        Strict = strict;
    }

    public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public new FunctionBody UpdateWith(in NodeList<Statement> body)
    {
        return (FunctionBody)base.UpdateWith(body);
    }

    protected override BlockStatement Rewrite(in NodeList<Statement> body)
    {
        return new FunctionBody(body, Strict);
    }
}
