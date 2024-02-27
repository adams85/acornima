namespace Acornima.Ast;

public interface IVarScope : INode
{
    ref readonly NodeList<Statement> Body { get; }
    bool Strict { get; }
}
