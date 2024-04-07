using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima.Jsx.Ast;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(OpeningElement), nameof(Children), nameof(ClosingElement) })]
public sealed partial class JsxElement : JsxElementOrFragment
{
    public JsxElement(JsxOpeningElement openingElement, in NodeList<JsxNode> children, JsxClosingElement? closingElement)
        : base(JsxNodeType.Element, children)
    {
        OpeningElement = openingElement;
        ClosingElement = closingElement;
    }

    public JsxOpeningElement OpeningElement { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public JsxClosingElement? ClosingElement { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator)
    {
        // It'd be nice to also generate this state machine but for that we'd need to make significant changes to the source generator...
        // See also the related comment in VisitationBoilerplateGenerator.GetVisitableNodeInfo

        switch (enumerator._propertyIndex)
        {
            case 0:
                enumerator._propertyIndex++;
                return OpeningElement;
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
                if (ClosingElement is null)
                {
                    goto default;
                }
                return ClosingElement;
            default:
                return null;
        }
    }

    private JsxElement Rewrite(JsxOpeningElement openingElement, in NodeList<JsxNode> children, JsxClosingElement? closingElement)
    {
        return new JsxElement(openingElement, children, closingElement);
    }
}
