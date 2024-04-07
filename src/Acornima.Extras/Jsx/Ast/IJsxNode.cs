using Acornima.Ast;

namespace Acornima.Jsx.Ast;

public interface IJsxNode : INode
{
    new JsxNodeType Type { get; }
}
