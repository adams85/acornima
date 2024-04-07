namespace Acornima.Ast;

/// <summary>
/// Represents a possible element of an optional chaining expression (<see cref="ChainExpression"/>),
/// i.e. a <see cref="MemberExpression"/> or a <see cref="CallExpression"/>.
/// </summary>
public interface IChainElement : INode
{
    bool Optional { get; }
}
