using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima.Jsx.Ast;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(Name), nameof(Namespace) })]
public sealed partial class JsxNamespacedName : JsxName
{
    public JsxNamespacedName(JsxIdentifier @namespace, JsxIdentifier name)
        : base(JsxNodeType.NamespacedName)
    {
        Namespace = @namespace;
        Name = name;
    }

    public JsxIdentifier Namespace { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public JsxIdentifier Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private JsxNamespacedName Rewrite(JsxIdentifier name, JsxIdentifier @namespace)
    {
        return new JsxNamespacedName(@namespace, name);
    }
}
