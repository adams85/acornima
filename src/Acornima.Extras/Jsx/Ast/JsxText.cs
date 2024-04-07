using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima.Jsx.Ast;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor))]
public sealed partial class JsxText : JsxNode
{
    public JsxText(string value, string raw)
        : base(JsxNodeType.Text)
    {
        Value = value;
        Raw = raw;
    }

    public string Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public string Raw { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
