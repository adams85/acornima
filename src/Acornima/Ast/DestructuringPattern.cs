namespace Acornima.Ast;

public abstract class DestructuringPattern : Node, IDestructuringElement
{
    private protected DestructuringPattern(NodeType type)
        : base(type) { }
}
