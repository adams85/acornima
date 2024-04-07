namespace Acornima.Ast;

/// <summary>
/// Represents a possible element of a destructuring pattern, i.e. an <see cref="Identifier"/>, a <see cref="MemberExpression"/>,
/// an <see cref="ArrayPattern"/>, an <see cref="ObjectPattern"/>, an <see cref="AssignmentPattern"/> or a <see cref="RestElement"/>.
/// </summary>
public interface IDestructuringPatternElement : INode { }
