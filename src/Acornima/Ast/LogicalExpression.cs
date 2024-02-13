using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public sealed partial class LogicalExpression : BinaryExpression
{
    public LogicalExpression(string op, Expression left, Expression right) : this(OperatorFromString(op), left, right)
    {
    }

    public LogicalExpression(Operator op, Expression left, Expression right) : base(NodeType.LogicalExpression, op, left, right)
    {
    }

    [StringMatcher(
        "??" /* => Operator.NullishCoalescing */,
        "||" /* => Operator.LogicalOr */,
        "&&" /* => Operator.LogicalAnd */
    )]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    public static partial Operator OperatorFromString(string s);

    public static string? OperatorToString(Operator op)
    {
        return op switch
        {
            Operator.NullishCoalescing => "??",
            Operator.LogicalOr => "||",
            Operator.LogicalAnd => "&&",
            _ => null
        };
    }

    protected override BinaryExpression Rewrite(Expression left, Expression right)
    {
        return new LogicalExpression(Operator, left, right);
    }
}
