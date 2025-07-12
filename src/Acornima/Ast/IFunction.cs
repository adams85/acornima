using System;

namespace Acornima.Ast;

/// <summary>
/// Represents either a <see cref="FunctionDeclaration"/>, a <see cref="FunctionExpression"/> or an <see cref="ArrowFunctionExpression"/>.
/// </summary>
public interface IFunction : INode
{
    Identifier? Id { get; }
    ref readonly NodeList<Node> Params { get; }
    StatementOrExpression Body { get; }
    [Obsolete("This property is deprecated in the ESTree specification and therefore will be removed from the public API in a future major version.")]
    bool Expression { get; }
    bool Generator { get; }
    bool Async { get; }
}
