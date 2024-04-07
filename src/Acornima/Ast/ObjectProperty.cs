using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Key), nameof(Value) })]
public sealed partial class ObjectProperty : Property
{
    public ObjectProperty(PropertyKind kind, Expression key, Node value, bool computed, bool shorthand, bool method)
        : base(kind, key, value, computed, shorthand)
    {
        Method = method;
    }

    public new bool Method { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    private protected override bool GetMethod() => Method;

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextProperty(Key, Value, Shorthand);

    private ObjectProperty Rewrite(Expression key, Node value)
    {
        return new ObjectProperty(Kind, key, value, Computed, Shorthand, Method);
    }
}
