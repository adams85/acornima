//HintName: Acornima.Ast.AssignmentExpression.g.cs
#nullable enable

using System;

namespace Acornima.Ast;

partial class AssignmentExpression
{
    public static partial Operator OperatorFromString(string s)
    {
        switch (s.Length)
        {
            case 1:
            {
                return s[0] == '=' ? Operator.Assignment : default;
            }
            case 2:
            {
                return s[0] switch
                {
                    '-' => s[1] == '=' ? Operator.SubtractionAssignment : default,
                    '*' => s[1] == '=' ? Operator.MultiplicationAssignment : default,
                    '/' => s[1] == '=' ? Operator.DivisionAssignment : default,
                    '&' => s[1] == '=' ? Operator.BitwiseAndAssignment : default,
                    '%' => s[1] == '=' ? Operator.RemainderAssignment : default,
                    '^' => s[1] == '=' ? Operator.BitwiseXorAssignment : default,
                    '+' => s[1] == '=' ? Operator.AdditionAssignment : default,
                    '|' => s[1] == '=' ? Operator.BitwiseOrAssignment : default,
                    _ => default
                };
            }
            case 3:
            {
                return s[0] switch
                {
                    '?' => s[1] == '?' && s[2] == '=' ? Operator.NullishCoalescingAssignment : default,
                    '*' => s[1] == '*' && s[2] == '=' ? Operator.ExponentiationAssignment : default,
                    '&' => s[1] == '&' && s[2] == '=' ? Operator.LogicalAndAssignment : default,
                    '<' => s[1] == '<' && s[2] == '=' ? Operator.LeftShiftAssignment : default,
                    '>' => s[1] == '>' && s[2] == '=' ? Operator.RightShiftAssignment : default,
                    '|' => s[1] == '|' && s[2] == '=' ? Operator.LogicalOrAssignment : default,
                    _ => default
                };
            }
            case 4:
            {
                return s[0] == '>' && s[1] == '>' && s[2] == '>' && s[3] == '=' ? Operator.UnsignedRightShiftAssignment : default;
            }
            default:
                return default;
        }
    }
}
