using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Id), nameof(Params), nameof(Body) })]
public sealed partial class FunctionExpression : Expression, IFunction
{
    private readonly NodeList<Node> _params;

    public FunctionExpression(
        Identifier? id,
        in NodeList<Node> parameters,
        FunctionBody body,
        bool generator,
        bool strict,
        bool async) :
        base(NodeType.FunctionExpression)
    {
        Id = id;
        _params = parameters;
        Body = body;
        Generator = generator;
        Strict = strict;
        Async = async;
    }

    public Identifier? Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    /// <remarks>
    /// { <see cref="Identifier"/> | <see cref="ArrayPattern"/> | <see cref="ObjectPattern"/> | <see cref="AssignmentPattern"/> | <see cref="RestElement"/> }
    /// </remarks>
    public ref readonly NodeList<Node> Params { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _params; }

    public FunctionBody Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    StatementOrExpression IFunction.Body => Body;

    public bool Generator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    bool IFunction.Expression => false;
    public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private FunctionExpression Rewrite(Identifier? id, in NodeList<Node> @params, FunctionBody body)
    {
        return new FunctionExpression(id, @params, body, Generator, Strict, Async);
    }
}
