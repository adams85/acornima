using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Decorators), nameof(Key), nameof(Value) })]
public sealed partial class AccessorProperty : ClassProperty
{
    private readonly NodeList<Decorator> _decorators;

    public AccessorProperty(
        Expression key,
        Expression? value,
        bool computed,
        bool isStatic,
        in NodeList<Decorator> decorators)
        : base(NodeType.AccessorProperty, PropertyKind.Property, key, computed, isStatic)
    {
        Value = value;
        _decorators = decorators;
    }

    public new Expression? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    protected override Expression? GetValue() => Value;

    public ref readonly NodeList<Decorator> Decorators { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _decorators; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AccessorProperty Rewrite(in NodeList<Decorator> decorators, Expression key, Expression? value)
    {
        return new AccessorProperty(key, value, Computed, Static, decorators);
    }
}
