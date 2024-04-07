using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima.Jsx.Ast;

public abstract class JsxNode : Expression, IJsxNode
{
    protected JsxNode(JsxNodeType type)
        : base(NodeType.Extension)
    {
        Type = type;
    }

    public new JsxNodeType Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public override string TypeText => "JSX" + Type;

    protected internal abstract object? Accept(IJsxAstVisitor visitor);

    protected internal sealed override object? Accept(AstVisitor visitor)
    {
        return visitor is IJsxAstVisitor jsxVisitor ? Accept(jsxVisitor) : AcceptAsExtension(visitor);
    }
}
