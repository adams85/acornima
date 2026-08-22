namespace Acornima;

/// <summary>
/// Identifies the kind of a legacy octal construct, that is, a syntactic form which the specification allows
/// in non-strict mode only (see <see href="https://tc39.es/ecma262/#sec-additional-syntax-numeric-literals"/>
/// and <see href="https://tc39.es/ecma262/#sec-additional-syntax-string-literals"/>).
/// </summary>
internal enum LegacyOctalKind : byte
{
    None,

    /// <summary>
    /// LegacyOctalEscapeSequence, e.g. <c>"\077"</c>
    /// </summary>
    OctalEscape,

    /// <summary>
    /// NonOctalDecimalEscapeSequence, that is, <c>"\8"</c> or <c>"\9"</c>
    /// </summary>
    EightOrNineEscape,

    /// <summary>
    /// LegacyOctalIntegerLiteral, e.g. <c>077</c>
    /// </summary>
    OctalLiteral,

    /// <summary>
    /// NonOctalDecimalIntegerLiteral, e.g. <c>08</c>
    /// </summary>
    DecimalWithLeadingZero,
}
