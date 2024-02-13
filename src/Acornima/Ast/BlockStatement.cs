using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Body) })]
public abstract partial class BlockStatement : Statement, IBlock
{
    private readonly NodeList<Statement> _body;

    private protected BlockStatement(NodeType type, in NodeList<Statement> body) : base(type)
    {
        _body = body;
    }

    public ref readonly NodeList<Statement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

    protected abstract BlockStatement Rewrite(in NodeList<Statement> body);
}
