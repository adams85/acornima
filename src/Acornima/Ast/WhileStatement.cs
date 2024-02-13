using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Test), nameof(Body) })]
public sealed partial class WhileStatement : Statement
{
    public WhileStatement(Expression test, Statement body) : base(NodeType.WhileStatement)
    {
        Test = test;
        Body = body;
    }

    public Expression Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private WhileStatement Rewrite(Expression test, Statement body)
    {
        return new WhileStatement(test, body);
    }
}
