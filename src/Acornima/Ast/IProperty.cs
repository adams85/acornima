namespace Acornima.Ast;

public interface IProperty : INode
{
    PropertyKind Kind { get; }
    Expression Key { get; }
    Node? Value { get; }
    bool Computed { get; }
}
