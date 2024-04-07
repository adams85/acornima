namespace Acornima.Ast;

/// <summary>
/// Represents an object property, a property of an object destructuring pattern or a class property,
/// i.e. a <see cref="ObjectProperty"/>, an <see cref="AssignmentProperty"/>, a <see cref="PropertyDefinition"/>, an <see cref="AccessorProperty"/> or a <see cref="MethodDefinition"/>.
/// </summary>
public interface IProperty : INode
{
    PropertyKind Kind { get; }
    Expression Key { get; }
    Node? Value { get; }
    bool Computed { get; }
}
