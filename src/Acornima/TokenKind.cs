namespace Acornima;

public enum TokenKind
{
    Unknown,
    Punctuator,
    Keyword,
    Identifier,
    NullLiteral,
    BooleanLiteral,
    StringLiteral,
    NumericLiteral,
    BigIntLiteral,
    RegExpLiteral,
    Template,
    EOF,
}
