using System.Globalization;
using System.Unicode;

namespace Acornima.Tests.Helpers;

public static class JavaScriptCharacter
{
    public static bool IsLineTerminator(int cp)
    {
        // https://tc39.es/ecma262/#sec-line-terminators

        return cp is '\n' // <LF>
            or '\r' // <CR>
            or '\u2028' // <LS>
            or '\u2029'; // <PS>
    }

    public static bool IsWhiteSpace(int cp)
    {
        // https://tc39.es/ecma262/#prod-WhiteSpace

        return cp is
            '\t' // <TAB>
            or '\v' // <VT>
            or '\f' // <FF>
            or '\uFEFF' // <ZWNBSP>
            || UnicodeInfo.GetCharInfo(cp).Category == UnicodeCategory.SpaceSeparator; // <USP>
    }

    public static bool IsIdentifierStart(int cp)
    {
        // https://tc39.es/ecma262/#prod-IdentifierStartChar

        return cp is '$' or '_'
            || UnicodeInfo.GetCharInfo(cp).CoreProperties.HasFlag(CoreProperties.IdentifierStart); /* UnicodeIDStart */
    }

    public static bool IsIdentifierPart(int cp)
    {
        // https://tc39.es/ecma262/#prod-IdentifierPartChar

        return cp is '$'
            or '\u200C' // <ZWNJ>
            or '\u200D' // <ZWJ>
            || UnicodeInfo.GetCharInfo(cp).CoreProperties.HasFlag(CoreProperties.IdentifierContinue); /* UnicodeIDContinue */
    }

    public static bool IsCommentStart(int cp) => cp is '/' or '#';
}
