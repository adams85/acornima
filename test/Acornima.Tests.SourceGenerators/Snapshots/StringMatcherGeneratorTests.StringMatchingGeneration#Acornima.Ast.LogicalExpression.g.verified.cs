//HintName: Acornima.Ast.LogicalExpression.g.cs
#nullable enable

using System;

namespace Acornima.Ast;

partial class LogicalExpression
{
    public static partial Operator OperatorFromString(string s)
    {
        return s[0] switch
        {
            '?' => s[1] == '?' ? Operator.NullishCoalescing : default,
            '&' => s[1] == '&' ? Operator.LogicalAnd : default,
            '|' => s[1] == '|' ? Operator.LogicalOr : default,
            _ => default
        };
    }
}
