using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Specifiers), nameof(Source), nameof(Attributes) })]
public sealed partial class ImportDeclaration : ImportOrExportDeclaration
{
    private readonly NodeList<ImportDeclarationSpecifier> _specifiers;
    private readonly NodeList<ImportAttribute> _attributes;

    public ImportDeclaration(
        in NodeList<ImportDeclarationSpecifier> specifiers,
        Literal source,
        in NodeList<ImportAttribute> attributes)
        : base(NodeType.ImportDeclaration)
    {
        _specifiers = specifiers;
        Source = source;
        _attributes = attributes;
    }

    /// <remarks>
    /// <see cref="ImportSpecifier"/> | <see cref="ImportDefaultSpecifier "/> | <see cref="ImportNamespaceSpecifier "/>
    /// </remarks>
    public ref readonly NodeList<ImportDeclarationSpecifier> Specifiers { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _specifiers; }
    public Literal Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ImportAttribute> Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _attributes; }

    private ImportDeclaration Rewrite(in NodeList<ImportDeclarationSpecifier> specifiers, Literal source, in NodeList<ImportAttribute> attributes)
    {
        return new ImportDeclaration(specifiers, source, attributes);
    }
}
