namespace Acornima.Ast;

public interface IBlock : INode
{
    ref readonly NodeList<Statement> Body { get; }
}
