using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima.Jsx.Ast;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(Argument) })]
public sealed partial class JsxSpreadAttribute : JsxAttributeBase
{
    public JsxSpreadAttribute(Expression argument)
        : base(JsxNodeType.SpreadAttribute)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private JsxSpreadAttribute Rewrite(Expression argument)
    {
        return new JsxSpreadAttribute(argument);
    }
}
