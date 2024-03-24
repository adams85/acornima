namespace Acornima.Ast;

public abstract class Declaration : Statement
{
    protected Declaration(NodeType type)
        : base(type) { }
}
