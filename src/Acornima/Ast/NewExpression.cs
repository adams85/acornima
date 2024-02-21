using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Callee), nameof(Arguments) })]
public sealed partial class NewExpression : Expression
{
    private readonly NodeList<Expression> _arguments;

    public NewExpression(
        Expression callee,
        in NodeList<Expression> args)
        : base(NodeType.NewExpression)
    {
        Callee = callee;
        _arguments = args;
    }

    public Expression Callee { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<Expression> Arguments { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _arguments; }

    private NewExpression Rewrite(Expression callee, in NodeList<Expression> arguments)
    {
        return new NewExpression(callee, arguments);
    }
}
