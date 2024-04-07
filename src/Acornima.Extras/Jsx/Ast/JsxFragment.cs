using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima.Jsx.Ast;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(OpeningFragment), nameof(Children), nameof(ClosingFragment) })]
public sealed partial class JsxFragment : JsxElementOrFragment
{
    public JsxFragment(JsxOpeningFragment openingFragment, in NodeList<JsxNode> children, JsxClosingFragment closingFragment)
        : base(JsxNodeType.Fragment, children)
    {
        OpeningFragment = openingFragment;
        ClosingFragment = closingFragment;
    }

    public JsxOpeningFragment OpeningFragment { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public JsxClosingFragment ClosingFragment { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator)
    {
        // It'd be nice to also generate this state machine but for that we'd need to make significant changes to the source generator...
        // See also the related comment in VisitationBoilerplateGenerator.GetVisitableNodeInfo

        switch (enumerator._propertyIndex)
        {
            case 0:
                enumerator._propertyIndex++;
                return OpeningFragment;
            case 1:
                if (enumerator._listIndex >= Children.Count)
                {
                    enumerator._listIndex = 0;
                    enumerator._propertyIndex++;
                    goto case 2;
                }

                Node? item = Children[enumerator._listIndex++];
                return item;
            case 2:
                enumerator._propertyIndex++;
                return ClosingFragment;
            default:
                return null;
        }
    }

    private JsxFragment Rewrite(JsxOpeningFragment openingFragment, in NodeList<JsxNode> children, JsxClosingFragment closingFragment)
    {
        return new JsxFragment(openingFragment, children, closingFragment);
    }
}
