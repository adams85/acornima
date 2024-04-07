using System.Runtime.CompilerServices;

namespace Acornima.Jsx.Ast;

public abstract class JsxOpeningTag : JsxNode
{
    private protected JsxOpeningTag(JsxNodeType type, bool selfClosing)
        : base(type)
    {
        SelfClosing = selfClosing;
    }

    public bool SelfClosing { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
