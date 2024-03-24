using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima;

// Object type used to represent tokens. Note that normally, tokens
// simply exist as properties on the parser object. This is only
// used for the onToken callback and the external tokenizer.
public readonly struct Token
{
    public static Token Punctuator(string value, Range range, SourceLocation location) => new Token(TokenKind.Punctuator, value, range, location);
    public static Token Keyword(string value, Range range, SourceLocation location) => new Token(TokenKind.Keyword, value, range, location);
    public static Token Identifier(string value, Range range, SourceLocation location) => new Token(TokenKind.Identifier, value, range, location);
    public static Token NullLiteral(Range range, SourceLocation location) => new Token(TokenKind.NullLiteral, new TokenValue(null), range, location);
    public static Token BooleanLiteral(bool value, Range range, SourceLocation location) => new Token(TokenKind.BooleanLiteral, new TokenValue(value.AsCachedObject()), range, location);
    public static Token StringLiteral(string value, Range range, SourceLocation location) => new Token(TokenKind.StringLiteral, value, range, location);
    public static Token NumericLiteral(double value, Range range, SourceLocation location) => new Token(TokenKind.NumericLiteral, value, range, location);
    public static Token BigIntLiteral(BigInteger value, Range range, SourceLocation location) => new Token(TokenKind.BigIntLiteral, value, range, location);
    public static Token RegExpLiteral(RegExpValue value, RegExpParseResult parseResult, Range range, SourceLocation location) => new Token(TokenKind.RegExpLiteral, Tuple.Create(value, parseResult), range, location);
    public static Token Template(TemplateValue value, Range range, SourceLocation location) => new Token(TokenKind.Template, value, range, location);
    public static Token EOF(Range range, SourceLocation location) => new Token(TokenKind.EOF, TokenValue.EOF, range, location);

    private readonly TokenValue _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Token(TokenKind kind, TokenValue value, Range range, SourceLocation location)
    {
        Kind = kind;
        _value = value;
        _range = range;
        Location = location;
    }

    public TokenKind Kind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    /// <remarks>
    /// Return value type depends on <see cref="Kind"/> as follows:<br/>
    /// * <see cref="TokenKind.Punctuator"/> | <see cref="TokenKind.Keyword"/> | <see cref="TokenKind.Identifier"/> | <see cref="TokenKind.StringLiteral"/> | <see cref="TokenKind.EOF"/> => <see cref="string"/><br/>
    /// * <see cref="TokenKind.NullLiteral"/> => <see langword="null"/><br/>
    /// * <see cref="TokenKind.BooleanLiteral"/> => <see cref="bool"/><br/>
    /// * <see cref="TokenKind.NumericLiteral"/> => <see cref="double"/><br/>
    /// * <see cref="TokenKind.BigIntLiteral"/> => <see cref="BigInteger"/><br/>
    /// * <see cref="TokenKind.RegExpLiteral"/> => <see cref="Acornima.RegExpValue"/><br/>
    /// * <see cref="TokenKind.Template"/> => <see cref="Acornima.TemplateValue"/><br/>
    /// Please be aware that this operation may involve boxing when the return value is a value type. Thus, it is preferable to use the typed value getters if you know <see cref="Kind"/> beforehand.
    /// </remarks>
    public object? Value => Kind switch
    {
        TokenKind.Punctuator or
        TokenKind.Keyword or
        TokenKind.Identifier or
        TokenKind.NullLiteral or
        TokenKind.BooleanLiteral or
        TokenKind.StringLiteral or
        TokenKind.EOF => _value.Value,
        TokenKind.NumericLiteral => _value.NumericValue,
        TokenKind.BigIntLiteral => _value.BigIntValue,
        TokenKind.RegExpLiteral => ((Tuple<RegExpValue, RegExpParseResult>)_value.Value!).Item1,
        TokenKind.Template => new TemplateValue(_value.TemplateCooked, (string)_value.Value!),
        _ => null
    };

    public bool? BooleanValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value.Value as bool?; }

    public string? StringValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value.Value as string; }

    public double? NumericValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Kind == TokenKind.NumericLiteral ? _value.NumericValue : null; }

    public BigInteger? BigIntValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Kind == TokenKind.BigIntLiteral ? _value.BigIntValue : null; }

    public RegExpValue? RegExpValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (_value.Value as Tuple<RegExpValue, RegExpParseResult>)?.Item1; }
    public RegExpParseResult? RegExpParseResult { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (_value.Value as Tuple<RegExpValue, RegExpParseResult>)?.Item2; }

    public TemplateValue? TemplateValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Kind == TokenKind.Template ? new TemplateValue(_value.TemplateCooked, (string)_value.Value!) : null; }

    public int Start { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _range.Start; }
    public int End { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _range.End; }

    private readonly Range _range;
    public Range Range { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _range; }

    public SourceLocation Location { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> GetRawValue(string input)
    {
        return input.SliceBetween(Start, End);
    }

    public override string ToString()
    {
        return Kind switch
        {
            TokenKind.Punctuator or
            TokenKind.Keyword or
            TokenKind.Identifier or
            TokenKind.BooleanLiteral or
            TokenKind.StringLiteral => $"{Kind} ({_value.Value})",
            TokenKind.NumericLiteral => $"{Kind} ({_value.NumericValue.ToString(CultureInfo.InvariantCulture)})",
            TokenKind.BigIntLiteral => $"{Kind} ({_value.BigIntValue.ToString(CultureInfo.InvariantCulture)})",
            TokenKind.RegExpLiteral => $"{Kind} ({((Tuple<RegExpValue, RegExpParseResult>)_value.Value!).Item1})",
            TokenKind.Template => $"{Kind} ({new TemplateValue(_value.TemplateCooked, (string)_value.Value!)})",
            _ => Kind.ToString()
        };
    }
}
