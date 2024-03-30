namespace Acornima.Ast;

public interface IHoistingScope : INode
{
    ref readonly NodeList<Statement> Body { get; }
    bool Strict { get; }
}
