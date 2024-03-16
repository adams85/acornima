using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Key), nameof(Value) })]
public sealed partial class ImportAttribute : Node
{
    public ImportAttribute(Expression key, StringLiteral value) : base(NodeType.ImportAttribute)
    {
        Key = key;
        Value = value;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="StringLiteral"/>
    /// </remarks>
    public Expression Key { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public StringLiteral Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private ImportAttribute Rewrite(Expression key, StringLiteral value)
    {
        return new ImportAttribute(key, value);
    }
}
