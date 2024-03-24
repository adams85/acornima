namespace Acornima.Ast;

public sealed class ObjectProperty : Property
{
    public ObjectProperty(PropertyKind kind, Expression key, Node value, bool computed, bool method, bool shorthand)
        : base(kind, key, value, computed, method, shorthand) { }

    protected override Property Rewrite(Expression key, Node value)
    {
        return new ObjectProperty(Kind, key, value, Computed, Method, Shorthand);
    }
}
