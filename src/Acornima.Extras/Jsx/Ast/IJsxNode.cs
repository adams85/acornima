using Acornima.Ast;

namespace Acornima.Jsx.Ast;

/// <summary>
/// Defines the base interface of JSX-related AST nodes.
/// </summary>
public interface IJsxNode : INode
{
    new JsxNodeType Type { get; }
}
