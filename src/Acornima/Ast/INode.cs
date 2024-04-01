namespace Acornima.Ast;

public interface INode
{
    NodeType Type { get; }
    ChildNodes ChildNodes { get; }
    int Start { get; }
    int End { get; }
    Range Range { get; }
    ref readonly Range RangeRef { get; }
    SourceLocation Location { get; }
    ref readonly SourceLocation LocationRef { get; }
    object? UserData { get; }
}
