using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Params), nameof(Body) })]
public sealed partial class ArrowFunctionExpression : Expression, IFunction
{
    private readonly NodeList<Node> _params;

    public ArrowFunctionExpression(
        in NodeList<Node> parameters,
        StatementOrExpression body,
        bool expression,
        bool strict,
        bool async)
        : base(NodeType.ArrowFunctionExpression)
    {
        _params = parameters;
        Body = body;
        Expression = expression;
        Strict = strict;
        Async = async;
    }

    Identifier? IFunction.Id => null;
    /// <remarks>
    /// { <see cref="Identifier"/> | <see cref="ArrayPattern"/> | <see cref="ObjectPattern"/> | <see cref="AssignmentPattern"/> | <see cref="RestElement"/> }
    /// </remarks>
    public ref readonly NodeList<Node> Params { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _params; }
    /// <remarks>
    /// <see cref="FunctionBody"/> | <see cref="Ast.Expression"/>
    /// </remarks>
    public StatementOrExpression Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    bool IFunction.Generator => false;
    public bool Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ArrowFunctionExpression Rewrite(in NodeList<Node> @params, StatementOrExpression body)
    {
        return new ArrowFunctionExpression(@params, body, Expression, Strict, Async);
    }
}
