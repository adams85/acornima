namespace Acornima.Ast;

/// <summary>
/// Represents a statement block to the top of which <c>var</c> variables are hoisted,
/// i.e. a <see cref="Script"/>, a <see cref="Module"/>, a <see cref="FunctionBody"/> or a <see cref="StaticBlock"/>.
/// </summary>
public interface IHoistingScope : INode
{
    ref readonly NodeList<Statement> Body { get; }
    bool Strict { get; }
}
