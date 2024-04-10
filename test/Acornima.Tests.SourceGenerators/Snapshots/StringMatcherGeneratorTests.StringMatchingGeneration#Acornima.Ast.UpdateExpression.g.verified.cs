//HintName: Acornima.Ast.UpdateExpression.g.cs
#nullable enable

using System;

namespace Acornima.Ast;

partial class UpdateExpression
{
    public static partial Operator OperatorFromString(string s)
    {
        return s[0] switch
        {
            '-' => s[1] == '-' ? Operator.Decrement : default,
            '+' => s[1] == '+' ? Operator.Increment : default,
            _ => default
        };
    }
}
