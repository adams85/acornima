using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Left), nameof(Right) })]
public sealed partial class AssignmentExpression : Expression
{
    public AssignmentExpression(string op, Node left, Expression right)
        : this(OperatorFromString(op), left, right)
    {
    }

    public AssignmentExpression(Operator op, Node left, Expression right)
        : base(NodeType.AssignmentExpression)
    {
        Operator = op;
        Left = left;
        Right = right;
    }

    [StringMatcher(
        "=" /* => Operator.Assignment */,
        "??=" /* => Operator.NullishCoalescingAssignment */,
        "||=" /* => Operator.LogicalOrAssignment */,
        "&&=" /* => Operator.LogicalAndAssignment */,
        "|=" /* => Operator.BitwiseOrAssignment */,
        "^=" /* => Operator.BitwiseXorAssignment */,
        "&=" /* => Operator.BitwiseAndAssignment */,
        "<<=" /* => Operator.LeftShiftAssignment */,
        ">>=" /* => Operator.RightShiftAssignment */,
        ">>>=" /* => Operator.UnsignedRightShiftAssignment */,
        "+=" /* => Operator.AdditionAssignment */,
        "-=" /* => Operator.SubtractionAssignment */,
        "*=" /* => Operator.MultiplicationAssignment */,
        "/=" /* => Operator.DivisionAssignment */,
        "%=" /* => Operator.RemainderAssignment */,
        "**=" /* => Operator.ExponentiationAssignment */
    )]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    public static partial Operator OperatorFromString(string s);

    public static string? OperatorToString(Operator op)
    {
        return op switch
        {
            Operator.Assignment => "=",
            Operator.NullishCoalescingAssignment => "??=",
            Operator.LogicalOrAssignment => "||=",
            Operator.LogicalAndAssignment => "&&=",
            Operator.BitwiseOrAssignment => "|=",
            Operator.BitwiseXorAssignment => "^=",
            Operator.BitwiseAndAssignment => "&=",
            Operator.LeftShiftAssignment => "<<=",
            Operator.RightShiftAssignment => ">>=",
            Operator.UnsignedRightShiftAssignment => ">>>=",
            Operator.AdditionAssignment => "+=",
            Operator.SubtractionAssignment => "-=",
            Operator.MultiplicationAssignment => "*=",
            Operator.DivisionAssignment => "/=",
            Operator.RemainderAssignment => "%=",
            Operator.ExponentiationAssignment => "**=",
            _ => null
        };
    }

    public Operator Operator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="MemberExpression"/> | <see cref="ArrayPattern"/> | <see cref="ObjectPattern"/> | <see cref="ParenthesizedExpression"/> (only if <see cref="ParserOptions.PreserveParens"/> is enabled)
    /// </remarks>
    public Node Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AssignmentExpression Rewrite(Node left, Expression right)
    {
        return new AssignmentExpression(Operator, left, right);
    }
}
