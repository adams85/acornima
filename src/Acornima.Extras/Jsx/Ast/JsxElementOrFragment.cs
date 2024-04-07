using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima.Jsx.Ast;

public abstract class JsxElementOrFragment : JsxNode
{
    private readonly NodeList<JsxNode> _children;

    private protected JsxElementOrFragment(JsxNodeType type, in NodeList<JsxNode> children)
        : base(type)
    {
        _children = children;
    }

    public ref readonly NodeList<JsxNode> Children { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _children; }
}
