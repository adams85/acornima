namespace Acornima.Ast;

/// <summary>
/// Represents a possible element of a class,
/// i.e. a <see cref="PropertyDefinition"/>, an <see cref="AccessorProperty"/>, a <see cref="MethodDefinition"/> or a <see cref="StaticBlock"/>.
/// </summary>
public interface IClassElement : INode
{
    bool Static { get; }
}
