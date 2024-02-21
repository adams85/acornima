using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Key), nameof(Value) })]
public sealed partial class ImportAttribute : Node
{
    public ImportAttribute(Expression key, Literal value) : base(NodeType.ImportAttribute)
    {
        Key = key;
        Value = value;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="Literal"/> (string or numeric)
    /// </remarks>
    public Expression Key { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Literal Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private ImportAttribute Rewrite(Expression key, Literal value)
    {
        return new ImportAttribute(key, value);
    }
}
