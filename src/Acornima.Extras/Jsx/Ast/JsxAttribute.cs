using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima.Jsx.Ast;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(Name), nameof(Value) })]
public sealed partial class JsxAttribute : JsxAttributeLike
{
    public JsxAttribute(JsxName name, Expression? value)
        : base(JsxNodeType.Attribute)
    {
        Name = name;
        Value = value;
    }

    /// <remarks>
    /// <see cref="JsxIdentifier"/> | <see cref="JsxNamespacedName"/>
    /// </remarks>
    public JsxName Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private JsxAttribute Rewrite(JsxName name, Expression? value)
    {
        return new JsxAttribute(name, value);
    }
}
