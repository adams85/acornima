namespace Acornima;

internal enum LegacyOctalKind : byte
{
    // See also:
    // - https://tc39.es/ecma262/#sec-additional-syntax-numeric-literals
    // - https://tc39.es/ecma262/#sec-additional-syntax-string-literals

    None,
    OctalEscape, // LegacyOctalEscapeSequence (e.g., `\077`)
    EightOrNineEscape, // NonOctalDecimalEscapeSequence (e.g., `\8 or \9`)
    OctalLiteral, // LegacyOctalIntegerLiteral (e.g., `077`)
    DecimalWithLeadingZero, // NonOctalDecimalIntegerLiteral (e.g., `08`)
}
