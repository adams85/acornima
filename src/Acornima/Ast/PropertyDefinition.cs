using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Decorators), nameof(Key), nameof(Value) })]
public sealed partial class PropertyDefinition : ClassProperty
{
    private readonly NodeList<Decorator> _decorators;

    public PropertyDefinition(
        Expression key,
        Expression? value,
        bool computed,
        bool isStatic,
        in NodeList<Decorator> decorators)
        : base(NodeType.PropertyDefinition, PropertyKind.Property, key, computed, isStatic)
    {
        Value = value;
        _decorators = decorators;
    }

    public new Expression? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    protected override Expression? GetValue() => Value;

    public ref readonly NodeList<Decorator> Decorators { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _decorators; }

    private PropertyDefinition Rewrite(in NodeList<Decorator> decorators, Expression key, Expression? value)
    {
        return new PropertyDefinition(key, value, Computed, Static, decorators);
    }
}
