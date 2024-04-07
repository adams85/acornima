using Acornima.Ast;

namespace Acornima.Jsx.Ast;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor))]
public sealed partial class JsxOpeningFragment : JsxOpeningTag
{
    public JsxOpeningFragment()
        : base(JsxNodeType.OpeningFragment, selfClosing: false) { }
}
