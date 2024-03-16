using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public abstract class ClassProperty : Node, IProperty, IClassElement
{
    private protected ClassProperty(NodeType type, PropertyKind kind, Expression key, bool computed, bool isStatic) : base(type)
    {
        Kind = kind;
        Key = key;
        Computed = computed;
        Static = isStatic;
    }

    public PropertyKind Kind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="StringLiteral"/> | <see cref="NumericLiteral"/> | <see cref="BigIntLiteral"/> | '[' <see cref="Expression"/> ']' | <see cref="PrivateIdentifier"/>
    /// </remarks>
    public Expression Key { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Computed { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Static { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public Expression? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetValue(); }
    Node? IProperty.Value => GetValue();
    protected abstract Expression? GetValue();
}
