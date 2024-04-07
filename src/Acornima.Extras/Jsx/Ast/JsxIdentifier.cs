using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima.Jsx.Ast;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor))]
public sealed partial class JsxIdentifier : JsxName
{
    public JsxIdentifier(string name)
        : base(JsxNodeType.Identifier)
    {
        Name = name;
    }

    public string Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
