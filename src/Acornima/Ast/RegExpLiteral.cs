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
    /// Can be <see langword="null"/> when the parser was not configured to convert regular expressions to <see cref="Regex"/> objects
    /// (see also <see cref="ParserOptions.RegExpParseMode"/>) or tolerant parsing was enabled and it was not possible to construct an equivalent <see cref="Regex"/> object.
    /// </remarks>
    public new Regex? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ParseResult.Regex; }

    protected override object? GetValue() => Value;

    public RegExpValue RegExp { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public RegExpParseResult ParseResult { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
