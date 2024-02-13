namespace Acornima.Ast;

public interface INode
{
    NodeType Type { get; }
    ChildNodes ChildNodes { get; }
    int Start { get; }
    int End { get; }
    Range Range { get; }
    SourceLocation Location { get; }
    object? UserData { get; }
}
