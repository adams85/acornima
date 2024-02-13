using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public sealed partial class UpdateExpression : UnaryExpression
{
    public UpdateExpression(string op, Expression arg, bool prefix) : this(OperatorFromString(op), arg, prefix)
    {
    }

    public UpdateExpression(Operator op, Expression arg, bool prefix) : base(NodeType.UpdateExpression, op, arg, prefix)
    {
    }

    [StringMatcher(
        "++" /* => Operator.Increment */,
        "--" /* => Operator.Decrement */
    )]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    public static partial Operator OperatorFromString(string s);

    public static string? OperatorToString(Operator op)
    {
        return op switch
        {
            Operator.Increment => "++",
            Operator.Decrement => "--",
            _ => null
        };
    }

    protected override UnaryExpression Rewrite(Expression argument)
    {
        return new UpdateExpression(Operator, argument, Prefix);
    }
}
