using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public sealed class ObjectProperty : Property
{
    public ObjectProperty(PropertyKind kind, Expression key, Node value, bool computed, bool shorthand, bool method)
        : base(kind, key, value, computed, shorthand)
    {
        Method = method;
    }

    protected override Property Rewrite(Expression key, Node value)
    {
        return new ObjectProperty(Kind, key, value, Computed, Shorthand, Method);
    }

    public new bool Method { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    private protected override bool GetMethod() => Method;
}
