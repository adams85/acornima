using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public sealed partial class NonLogicalBinaryExpression : BinaryExpression
{
    public NonLogicalBinaryExpression(string op, Expression left, Expression right)
        : this(OperatorFromString(op), left, right) { }

    public NonLogicalBinaryExpression(Operator op, Expression left, Expression right)
        : base(NodeType.BinaryExpression, op, left, right) { }

    [StringMatcher(
        "|" /* => Operator.BitwiseOr */,
        "^" /* => Operator.BitwiseXor */,
        "&" /* => Operator.BitwiseAnd */,
        "==" /* => Operator.Equality */,
        "!=" /* => Operator.Inequality */,
        "===" /* => Operator.StrictEquality */,
        "!==" /* => Operator.StrictInequality */,
        "<" /* => Operator.LessThan */,
        "<=" /* => Operator.LessThanOrEqual */,
        ">" /* => Operator.GreaterThan */,
        ">=" /* => Operator.GreaterThanOrEqual */,
        "in" /* => Operator.In */,
        "instanceof" /* => Operator.InstanceOf */,
        "<<" /* => Operator.LeftShift */,
        ">>" /* => Operator.RightShift */,
        ">>>" /* => Operator.UnsignedRightShift */,
        "+" /* => Operator.Addition */,
        "-" /* => Operator.Subtraction */,
        "*" /* => Operator.Multiplication */,
        "/" /* => Operator.Division */,
        "%" /* => Operator.Remainder */,
        "**" /* => Operator.Exponentiation */
    )]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    public static partial Operator OperatorFromString(string s);

    public static string? OperatorToString(Operator op)
    {
        return op switch
        {
            Operator.BitwiseOr => "|",
            Operator.BitwiseXor => "^",
            Operator.BitwiseAnd => "&",
            Operator.Equality => "==",
            Operator.Inequality => "!=",
            Operator.StrictEquality => "===",
            Operator.StrictInequality => "!==",
            Operator.LessThan => "<",
            Operator.LessThanOrEqual => "<=",
            Operator.GreaterThan => ">",
            Operator.GreaterThanOrEqual => ">=",
            Operator.In => "in",
            Operator.InstanceOf => "instanceof",
            Operator.LeftShift => "<<",
            Operator.RightShift => ">>",
            Operator.UnsignedRightShift => ">>>",
            Operator.Addition => "+",
            Operator.Subtraction => "-",
            Operator.Multiplication => "*",
            Operator.Division => "/",
            Operator.Remainder => "%",
            Operator.Exponentiation => "**",
            _ => null
        };
    }

    protected override BinaryExpression Rewrite(Expression left, Expression right)
    {
        return new NonLogicalBinaryExpression(Operator, left, right);
    }
}
