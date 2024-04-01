namespace Acornima.Ast;

public abstract class DestructuringPattern : Node, IDestructuringPatternElement
{
    private protected DestructuringPattern(NodeType type)
        : base(type) { }
}
