using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Object), nameof(Property) })]
public sealed partial class MemberExpression : Expression, IDestructuringPattern, IChainElement
{
    public MemberExpression(Expression obj, Expression property, bool computed, bool optional)
        : base(NodeType.MemberExpression)
    {
        Object = obj;
        Property = property;
        Computed = computed;
        Optional = optional;
    }

    public Expression Object { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Property { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Computed { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Optional { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private MemberExpression Rewrite(Expression @object, Expression property)
    {
        return new MemberExpression(@object, property, Computed, Optional);
    }
}
