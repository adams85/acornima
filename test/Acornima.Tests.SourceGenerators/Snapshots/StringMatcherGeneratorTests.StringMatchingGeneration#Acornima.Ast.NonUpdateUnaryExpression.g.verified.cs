//HintName: Acornima.Ast.NonUpdateUnaryExpression.g.cs
#nullable enable

using System;

namespace Acornima.Ast;

partial class NonUpdateUnaryExpression
{
    public static partial Operator OperatorFromString(string s)
    {
        switch (s.Length)
        {
            case 1:
            {
                return s[0] switch
                {
                    '-' => Operator.UnaryNegation,
                    '!' => Operator.LogicalNot,
                    '+' => Operator.UnaryPlus,
                    '~' => Operator.BitwiseNot,
                    _ => default
                };
            }
            case 4:
            {
                return s[0] == 'v' && s[1] == 'o' && s[2] == 'i' && s[3] == 'd' ? Operator.Void : default;
            }
            case 6:
            {
                return s[0] switch
                {
                    'd' => s[1] == 'e' && s[2] == 'l' && s[3] == 'e' && s[4] == 't' && s[5] == 'e' ? Operator.Delete : default,
                    't' => s[1] == 'y' && s[2] == 'p' && s[3] == 'e' && s[4] == 'o' && s[5] == 'f' ? Operator.TypeOf : default,
                    _ => default
                };
            }
            default:
                return default;
        }
    }
}
