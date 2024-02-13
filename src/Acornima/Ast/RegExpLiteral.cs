using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Acornima.Ast;

public sealed class RegExpLiteral : Literal
{
    public RegExpLiteral(string pattern, string flags, RegExpParseResult parseResult, string raw)
        : this(new RegExpValue(pattern, flags), parseResult, raw) { }

    public RegExpLiteral(RegExpValue regExp, RegExpParseResult parseResult, string raw)
        : base(TokenKind.RegExpLiteral, raw)
    {
        RegExp = regExp;
        ParseResult = parseResult;
    }

    /// <remarks>
    /// May return <see langword="null"/> if a <see cref="Regex"/> object couldn't be created out of the pattern or flags.
    /// </remarks>
    public new Regex? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ParseResult.Regex; }

    protected override object? GetValue() => Value;

    public RegExpValue RegExp { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public RegExpParseResult ParseResult { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
