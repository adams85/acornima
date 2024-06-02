using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Object), nameof(Body) })]
public sealed partial class WithStatement : Statement
{
    public WithStatement(Expression obj, Statement body)
        : base(NodeType.WithStatement)
    {
        Object = obj;
        Body = body;
    }

    public Expression Object { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private WithStatement Rewrite(Expression @object, Statement body)
    {
        return new WithStatement(@object, body);
    }
}
