namespace Acornima.Ast;

/// <summary>
/// Represents either a <see cref="FunctionDeclaration"/>, a <see cref="FunctionExpression"/> or an <see cref="ArrowFunctionExpression"/>.
/// </summary>
public interface IFunction : INode
{
    Identifier? Id { get; }
    ref readonly NodeList<Node> Params { get; }
    StatementOrExpression Body { get; }
    bool Expression { get; }
    bool Generator { get; }
    bool Async { get; }
}
