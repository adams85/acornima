using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Imported), nameof(Local) })]
public sealed partial class ImportSpecifier : ImportDeclarationSpecifier
{
    public ImportSpecifier(Identifier local, Expression imported) : base(local, NodeType.ImportSpecifier)
    {
        Imported = imported;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="Literal"/> (string)
    /// </remarks>
    public Expression Imported { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextImportSpecifier(Imported, Local);

    private ImportSpecifier Rewrite(Expression imported, Identifier local)
    {
        return new ImportSpecifier(local, imported);
    }
}
