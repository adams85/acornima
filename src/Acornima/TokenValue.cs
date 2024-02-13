using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Acornima;

internal readonly struct TokenValue
{
    public static readonly string EOF = string.Empty;

    // Punctuator | Keyword | Identifier | NullLiteral | BoolLiteral | StringLiteral | RegExpLiteral (Tuple<RegExpValue, RegExpParseResult>) | Template (raw)
    public readonly object? Value;
    // Template (cooked)
    public readonly string? TemplateCooked;
    // NumericLiteral
    public readonly double NumericValue;
    // BigIntLiteral
    public readonly BigInteger BigIntValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TokenValue(object? value, string? templateCooked = null, double numericValue = default, BigInteger bigIntValue = default)
    {
        Value = value;
        TemplateCooked = templateCooked;
        NumericValue = numericValue;
        BigIntValue = bigIntValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator TokenValue(string value) => new TokenValue(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator TokenValue(double value) => new TokenValue(value: null, numericValue: value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator TokenValue(BigInteger value) => new TokenValue(value: null, bigIntValue: value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator TokenValue(Tuple<RegExpValue, RegExpParseResult> value) => new TokenValue(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator TokenValue(TemplateValue value) => new TokenValue(value.Raw, value.Cooked);
}
