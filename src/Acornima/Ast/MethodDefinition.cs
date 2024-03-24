using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Decorators), nameof(Key), nameof(Value) })]
public sealed partial class MethodDefinition : ClassProperty
{
    private readonly NodeList<Decorator> _decorators;

    public MethodDefinition(
        PropertyKind kind,
        Expression key,
        FunctionExpression value,
        bool computed,
        bool isStatic,
        in NodeList<Decorator> decorators)
        : base(NodeType.MethodDefinition, kind, key, computed, isStatic)
    {
        Value = value;
        _decorators = decorators;
    }

    public new FunctionExpression Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    protected override Expression? GetValue() => Value;

    public ref readonly NodeList<Decorator> Decorators { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _decorators; }

    private MethodDefinition Rewrite(in NodeList<Decorator> decorators, Expression key, FunctionExpression value)
    {
        return new MethodDefinition(Kind, key, value, Computed, Static, decorators);
    }
}
