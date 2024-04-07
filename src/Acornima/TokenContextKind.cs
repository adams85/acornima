namespace Acornima;

internal enum TokenContextKind
{
    // Notes for maintainers:
    // Don't use the negative value range as it's reserved for extensions (see JsxTokenContextKind).

    Unknown,
    BraceLeft,
    DollarBraceLeft,
    ParenLeft,
    BackQuote,
    Function,
}
