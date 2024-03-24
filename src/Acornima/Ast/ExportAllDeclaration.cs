using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Exported), nameof(Source), nameof(Attributes) })]
public sealed partial class ExportAllDeclaration : ExportDeclaration
{
    private readonly NodeList<ImportAttribute> _attributes;

    public ExportAllDeclaration(
        Expression? exported,
        StringLiteral source,
        in NodeList<ImportAttribute> attributes)
        : base(NodeType.ExportAllDeclaration)
    {
        Exported = exported;
        Source = source;
        _attributes = attributes;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="StringLiteral"/> | <see langword="null"/>
    /// </remarks>
    public Expression? Exported { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public StringLiteral Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ImportAttribute> Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _attributes; }

    private ExportAllDeclaration Rewrite(Expression? exported, StringLiteral source, in NodeList<ImportAttribute> attributes)
    {
        return new ExportAllDeclaration(exported, source, attributes);
    }
}
