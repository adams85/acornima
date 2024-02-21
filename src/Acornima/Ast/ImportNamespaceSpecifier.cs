using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Local) })]
public sealed partial class ImportNamespaceSpecifier : ImportDeclarationSpecifier
{
    public ImportNamespaceSpecifier(Identifier local) : base(local, NodeType.ImportNamespaceSpecifier)
    {
    }

    private ImportNamespaceSpecifier Rewrite(Identifier local)
    {
        return new ImportNamespaceSpecifier(local);
    }
}
