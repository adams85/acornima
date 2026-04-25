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
    /// Will be <see langword="null"/> unless the parser is configured to convert regular expressions to <see cref="Regex"/> objects
    /// via <see cref="ParserOptions.OnRegExp"/>. (However, the library offers no built-in conversion. For this purpose,
    /// you can use a third-party library such as <see href="https://github.com/adams85/regexp2regex">regexp2regex</see>.)
    /// </remarks>
    public new Regex? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ParseResult.Regex; }

    protected override object? GetValue() => Value;

    public RegExpValue RegExp { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public RegExpParseResult ParseResult { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
