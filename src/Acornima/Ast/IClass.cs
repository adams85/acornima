namespace Acornima.Ast;

/// <summary>
/// Represents either a <see cref="ClassDeclaration"/> or an <see cref="ClassExpression"/>.
/// </summary>
public interface IClass : INode
{
    Identifier? Id { get; }
    Expression? SuperClass { get; }
    ClassBody Body { get; }
    ref readonly NodeList<Decorator> Decorators { get; }
}
