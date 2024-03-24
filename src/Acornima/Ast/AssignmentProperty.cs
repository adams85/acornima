namespace Acornima.Ast;

public sealed class AssignmentProperty : Property
{
    public AssignmentProperty(Expression key, Node value, bool computed, bool shorthand)
        : base(PropertyKind.Init, key, value, computed, method: false, shorthand: shorthand) { }

    protected override Property Rewrite(Expression key, Node value)
    {
        return new AssignmentProperty(key, value, Computed, Shorthand);
    }
}
