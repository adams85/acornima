using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public sealed partial class NonUpdateUnaryExpression : UnaryExpression
{
    public NonUpdateUnaryExpression(string op, Expression arg) : this(OperatorFromString(op), arg)
    {
    }

    public NonUpdateUnaryExpression(Operator op, Expression arg) : base(NodeType.UnaryExpression, op, arg, prefix: true)
    {
    }

    [StringMatcher(
        "!" /* => Operator.LogicalNot  */,
        "~" /* => Operator.BitwiseNot */,
        "+" /* => Operator.UnaryPlus */,
        "-" /* => Operator.UnaryNegation */,
        "typeof" /* => Operator.TypeOf  */,
        "void" /* => Operator.Void */,
        "delete" /* => Operator.Delete */
    )]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    public static partial Operator OperatorFromString(string s);

    public static string? OperatorToString(Operator op)
    {
        return op switch
        {
            Operator.LogicalNot => "!",
            Operator.BitwiseNot => "~",
            Operator.UnaryPlus => "+",
            Operator.UnaryNegation => "-",
            Operator.TypeOf => "typeof",
            Operator.Void => "void",
            Operator.Delete => "delete",
            _ => null
        };
    }

    protected override UnaryExpression Rewrite(Expression argument)
    {
        return new NonUpdateUnaryExpression(Operator, argument);
    }
}
