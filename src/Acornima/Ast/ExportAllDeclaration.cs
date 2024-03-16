using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Exported), nameof(Source), nameof(Attributes) })]
public sealed partial class ExportAllDeclaration : ExportDeclaration
{
    private readonly NodeList<ImportAttribute> _attributes;

    public ExportAllDeclaration(StringLiteral source, Expression? exported, in NodeList<ImportAttribute> attributes) : base(NodeType.ExportAllDeclaration)
    {
        Source = source;
        Exported = exported;
        _attributes = attributes;
    }

    public StringLiteral Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="StringLiteral"/> | <see langword="null"/>
    /// </remarks>
    public Expression? Exported { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ImportAttribute> Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _attributes; }

    private ExportAllDeclaration Rewrite(Expression? exported, StringLiteral source, in NodeList<ImportAttribute> attributes)
    {
        return new ExportAllDeclaration(source, exported, attributes);
    }
}
