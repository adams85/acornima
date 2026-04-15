using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima.Ast;

using static ExceptionHelper;

[VisitableNode(ChildProperties = new[] { nameof(Specifiers), nameof(Source), nameof(Attributes) })]
public sealed partial class ImportDeclaration : ImportOrExportDeclaration
{
    public static string? GetImportPhaseToken(ImportPhase phase) => phase switch
    {
        ImportPhase.None => null,
        ImportPhase.Source => "source",
        ImportPhase.Defer => "defer",
        _ => ThrowArgumentOutOfRangeException(nameof(phase), phase.ToString(), null)
    };

    private readonly NodeList<ImportDeclarationSpecifier> _specifiers;
    private readonly NodeList<ImportAttribute> _attributes;

    public ImportDeclaration(
        in NodeList<ImportDeclarationSpecifier> specifiers,
        StringLiteral source,
        in NodeList<ImportAttribute> attributes)
        : this(specifiers, source, attributes, ImportPhase.None) { }

    public ImportDeclaration(
        in NodeList<ImportDeclarationSpecifier> specifiers,
        StringLiteral source,
        in NodeList<ImportAttribute> attributes,
        ImportPhase phase)
        : base(NodeType.ImportDeclaration)
    {
        _specifiers = specifiers;
        Source = source;
        _attributes = attributes;
        Phase = phase;
    }

    /// <remarks>
    /// <see cref="ImportSpecifier"/> | <see cref="ImportDefaultSpecifier "/> | <see cref="ImportNamespaceSpecifier "/>
    /// </remarks>
    public ref readonly NodeList<ImportDeclarationSpecifier> Specifiers { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _specifiers; }
    public StringLiteral Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ImportAttribute> Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _attributes; }
    public ImportPhase Phase { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private ImportDeclaration Rewrite(in NodeList<ImportDeclarationSpecifier> specifiers, StringLiteral source, in NodeList<ImportAttribute> attributes)
    {
        return new ImportDeclaration(specifiers, source, attributes, Phase);
    }
}
