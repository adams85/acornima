namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Key), nameof(Value) })]
public sealed partial class AssignmentProperty : Property
{
    public AssignmentProperty(Expression key, Node value, bool computed, bool shorthand)
        : base(PropertyKind.Init, key, value, computed, shorthand) { }

    private protected override bool GetMethod() => false;

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextProperty(Key, Value, Shorthand);

    private AssignmentProperty Rewrite(Expression key, Node value)
    {
        return new AssignmentProperty(key, value, Computed, Shorthand);
    }
}
