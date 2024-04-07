using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima.Jsx.Ast;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(Object), nameof(Property) })]
public sealed partial class JsxMemberExpression : JsxName
{
    public JsxMemberExpression(JsxName obj, JsxIdentifier property)
        : base(JsxNodeType.MemberExpression)
    {
        Object = obj;
        Property = property;
    }

    /// <remarks>
    /// <see cref="JsxIdentifier"/> | <see cref="JsxNamespacedName"/> (only if <see cref="JsxParserOptions.JsxAllowNamespacedObjects"/> is enabled) | <see cref="JsxMemberExpression"/>
    /// </remarks>
    public JsxName Object { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public JsxIdentifier Property { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private JsxMemberExpression Rewrite(JsxName @object, JsxIdentifier property)
    {
        return new JsxMemberExpression(@object, property);
    }
}
