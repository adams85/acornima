using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima.Jsx.Ast;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(Name), nameof(Attributes) })]
public sealed partial class JsxOpeningElement : JsxOpeningTag
{
    private readonly NodeList<JsxAttributeBase> _attributes;

    public JsxOpeningElement(JsxName name, in NodeList<JsxAttributeBase> attributes, bool selfClosing)
        : base(JsxNodeType.OpeningElement)
    {
        Name = name;
        _attributes = attributes;
        SelfClosing = selfClosing;
    }

    /// <remarks>
    /// <see cref="JsxIdentifier"/> | <see cref="JsxNamespacedName"/> | <see cref="JsxMemberExpression"/>
    /// </remarks>
    public JsxName Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<JsxAttributeBase> Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _attributes; }
    public new bool SelfClosing { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    private protected override bool GetSelfClosing() => SelfClosing;

    private JsxOpeningElement Rewrite(JsxName name, in NodeList<JsxAttributeBase> attributes)
    {
        return new JsxOpeningElement(name, attributes, SelfClosing);
    }
}
