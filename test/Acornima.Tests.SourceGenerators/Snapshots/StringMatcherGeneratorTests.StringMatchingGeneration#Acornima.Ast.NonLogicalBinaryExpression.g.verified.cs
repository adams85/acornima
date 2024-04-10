//HintName: Acornima.Ast.NonLogicalBinaryExpression.g.cs
#nullable enable

using System;

namespace Acornima.Ast;

partial class NonLogicalBinaryExpression
{
    public static partial Operator OperatorFromString(string s)
    {
        switch (s.Length)
        {
            case 1:
            {
                return s[0] switch
                {
                    '-' => Operator.Subtraction,
                    '*' => Operator.Multiplication,
                    '/' => Operator.Division,
                    '&' => Operator.BitwiseAnd,
                    '%' => Operator.Remainder,
                    '^' => Operator.BitwiseXor,
                    '+' => Operator.Addition,
                    '<' => Operator.LessThan,
                    '>' => Operator.GreaterThan,
                    '|' => Operator.BitwiseOr,
                    _ => default
                };
            }
            case 2:
            {
                return s[0] switch
                {
                    '!' => s[1] == '=' ? Operator.Inequality : default,
                    '*' => s[1] == '*' ? Operator.Exponentiation : default,
                    '<' => s[1] switch
                    {
                        '<' => Operator.LeftShift,
                        '=' => Operator.LessThanOrEqual,
                        _ => default
                    },
                    '=' => s[1] == '=' ? Operator.Equality : default,
                    '>' => s[1] switch
                    {
                        '=' => Operator.GreaterThanOrEqual,
                        '>' => Operator.RightShift,
                        _ => default
                    },
                    'i' => s[1] == 'n' ? Operator.In : default,
                    _ => default
                };
            }
            case 3:
            {
                return s[0] switch
                {
                    '!' => s[1] == '=' && s[2] == '=' ? Operator.StrictInequality : default,
                    '=' => s[1] == '=' && s[2] == '=' ? Operator.StrictEquality : default,
                    '>' => s[1] == '>' && s[2] == '>' ? Operator.UnsignedRightShift : default,
                    _ => default
                };
            }
            case 10:
            {
                return s == "instanceof" ? Operator.InstanceOf : default;
            }
            default:
                return default;
        }
    }
}
