using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Body) }, SealOverrideMethods = true)]
public abstract partial class Program : Node, IBlock
{
    private readonly NodeList<Statement> _body;

    private protected Program(SourceType sourceType, in NodeList<Statement> body, bool strict) : base(NodeType.Program)
    {
        SourceType = sourceType;
        Strict = strict;
        _body = body;
    }

    public ref readonly NodeList<Statement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

    public SourceType SourceType { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected abstract Program Rewrite(in NodeList<Statement> body);
}
