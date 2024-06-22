namespace Acornima.Ast;

/// <summary>
/// Defines the base interface of AST nodes.
/// </summary>
public interface INode
{
    Node Node { get; }
    NodeType Type { get; }
    ChildNodes ChildNodes { get; }
    int Start { get; }
    int End { get; }
    Range Range { get; }
    ref readonly Range RangeRef { get; }
    SourceLocation Location { get; }
    ref readonly SourceLocation LocationRef { get; }
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    object? UserData { get; }
}
