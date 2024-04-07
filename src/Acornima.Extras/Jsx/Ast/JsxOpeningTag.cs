using System.Runtime.CompilerServices;

namespace Acornima.Jsx.Ast;

public abstract class JsxOpeningTag : JsxNode
{
    private protected JsxOpeningTag(JsxNodeType type)
        : base(type) { }

    public bool SelfClosing { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetSelfClosing(); }
    private protected abstract bool GetSelfClosing();
}
