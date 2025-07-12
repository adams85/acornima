using System;
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
        bool async)
        : base(NodeType.ArrowFunctionExpression)
    {
        _params = parameters;
        Body = body;
#pragma warning disable CS0618 // Type or member is obsolete
        Expression = expression;
#pragma warning restore CS0618 // Type or member is obsolete
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
    [Obsolete("This property is deprecated in the ESTree specification and therefore will be removed from the public API in a future major version.")]
    public bool Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private ArrowFunctionExpression Rewrite(in NodeList<Node> @params, StatementOrExpression body)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return new ArrowFunctionExpression(@params, body, Expression, Async);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
