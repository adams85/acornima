using System;
using System.Runtime.CompilerServices;
using System.Text;
using Acornima.Helpers;

namespace Acornima;

using static SyntaxErrorMessages;

public partial class Tokenizer
{
    internal partial class RegExpParser
    {
        private sealed class LegacyMode : IMode
        {
            public static readonly LegacyMode Instance = new();

            private LegacyMode() { }

            public void ProcessChar(char ch, Action<StringBuilder, char>? appender, RegExpParser parser)
            {
                ref readonly var sb = ref parser._stringBuilder;
                appender?.Invoke(sb!, ch);
            }

            public void ProcessSetSpecialChar(char ch, RegExpParser parser)
            {
                ref readonly var sb = ref parser._stringBuilder;
                sb?.Append(ch);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ProcessSetChar(char ch, Action<StringBuilder, char>? appender, RegExpParser parser, int startIndex)
            {
                ProcessSetChar(ch, ch, appender, parser, startIndex);
            }

            private static void ProcessSetChar(char ch, int charCode, Action<StringBuilder, char>? appender, RegExpParser parser, int startIndex)
            {
                ref readonly var sb = ref parser._stringBuilder;

                if (parser._setRangeStart >= 0)
                {
                    appender?.Invoke(sb!, ch);
                    parser._setRangeStart = charCode;
                }
                else
                {
                    parser._setRangeStart = ~parser._setRangeStart;

                    if (parser._setRangeStart > charCode)
                    {
                        if (parser._setRangeStart <= UnicodeHelper.LastCodePoint)
                        {
                            // Cases like /[z-a]/ are syntax error.
                            parser.ReportSyntaxError(startIndex, RegExpRangeOutOfOrderCharacterClass);
                        }
                        else
                        {
                            // Cases like /[\d-a]/ are valid but they're problematic in .NET, so they need to be escaped like @"[\d\x2Da]".
                            if (sb is not null)
                            {
                                sb.Remove(sb.Length - 1, 1);
                                AppendCharSafe(sb, '-');
                            }
                        }
                    }

                    appender?.Invoke(sb!, ch);
                    parser._setRangeStart = SetRangeNotStarted;
                }
            }

            public bool RewriteSet(RegExpParser parser)
            {
                ref readonly var sb = ref parser._stringBuilder;

                if (sb is not null)
                {
                    ref readonly var pattern = ref parser._pattern;
                    ref readonly var i = ref parser._index;

                    // [] should not match any characters.
                    if (parser._setStartIndex == i - 1)
                    {
                        sb.Remove(sb.Length - 1, 1).Append(MatchNoneRegex);
                        return true;
                    }

                    // [^] should match any character including newline.
                    if (parser._setStartIndex == i - 2 && pattern[i - 1] == '^')
                    {
                        sb.Remove(sb.Length - 2, 2).Append(MatchAnyRegex);
                        return true;
                    }

                    return false;
                }

                return true;
            }

            public void RewriteDot(RegExpParser parser)
            {
                ref readonly var sb = ref parser._stringBuilder;
                if (sb is not null)
                {
                    _ = (parser._effectiveFlags & RegExpFlags.DotAll) != 0
                        ? sb.Append(MatchAnyRegex)
                        : sb.Append(MatchAnyButNewLineRegex);
                }
            }

            private static void ParseOctalEscape(string pattern, ref int i, out ushort charCode)
            {
                charCode = 0;
                var endIndex = Math.Min(i + 3, pattern.Length);
                do
                {
                    var newCharCode = (ushort)((charCode << 3) + (pattern[i] - '0'));
                    if (newCharCode > 0xFF)
                    {
                        break;
                    }
                    charCode = newCharCode;
                }
                while (++i < endIndex && pattern[i].IsOctalDigit());

                i--;
            }

            public bool AllowsQuantifierAfterGroup(RegExpGroupType groupType)
            {
                // Lookbehind assertion groups may not be followed by quantifiers.
                // However, lookahead assertion groups may be. RegexOptions.ECMAScript seems to handle such cases in the same way as JS.
                return groupType is not
                (
                    RegExpGroupType.LookbehindAssertion or
                    RegExpGroupType.NegativeLookbehindAssertion
                );
            }

            public void HandleInvalidRangeQuantifier(RegExpParser parser, int startIndex)
            {
                // Invalid {} quantifiers like /.{/, /.{}/, /.{-1}/, etc. are ignored. RegexOptions.ECMAScript behaves in the same way,
                // so we don't need to do anything about such cases.

                ref readonly var sb = ref parser._stringBuilder;
                ref readonly var pattern = ref parser._pattern;

                sb?.Append(pattern[startIndex]);

                parser.ClearFollowingQuantifierError();
            }

            public bool AdjustEscapeSequence(RegExpParser parser, out RegExpConversionError? conversionError)
            {
                // https://tc39.es/ecma262/#prod-AtomEscape

                ref readonly var sb = ref parser._stringBuilder;
                ref readonly var pattern = ref parser._pattern;
                ref var i = ref parser._index;

                ushort charCode;
                var startIndex = i++;
                var ch = pattern[i];
                switch (ch)
                {
                    // CharacterEscape -> RegExpUnicodeEscapeSequence
                    // CharacterEscape -> HexEscapeSequence
                    case 'u':
                    case 'x':
                        if (TryReadHexEscape(pattern, ref i, endIndex: pattern.Length, charCodeLength: ch == 'u' ? 4 : 2, out charCode))
                        {
                            if (!parser.WithinSet)
                            {
                                parser._appendCharSafe?.Invoke(sb!, (char)charCode);
                                parser.ClearFollowingQuantifierError();
                            }
                            else
                            {
                                ProcessSetChar((char)charCode, parser._appendCharSafe, parser, startIndex);
                            }
                        }
                        else
                        {
                            // Rewrite
                            // * unterminated \x escape sequences (e.g. /\x0/ --> @"x0"),
                            // * unterminated \u escape sequences (e.g. /\u012/ --> @"u012"),
                            // * invalid \x escape sequences (e.g. /\x0y/ --> @"x0y"),
                            // * invalid \u escape sequences (e.g. /\u012y/ --> @"u012y"), including
                            // * UTF32-like invalid escape sequences (e.g. /\u{0010FFFF}/ --> @"u{0010FFFF}").
                            if (!parser.WithinSet)
                            {
                                sb?.Append(ch);
                                parser.ClearFollowingQuantifierError();
                            }
                            else
                            {
                                ProcessSetChar(ch, parser._appendChar, parser, startIndex);
                            }
                        }
                        break;

                    // CharacterEscape -> c ControlLetter
                    case 'c':
                        if (i + 1 < pattern.Length)
                        {
                            if (((char)(charCode = pattern[i + 1])).IsBasicLatinLetter())
                            {
                                charCode = (ushort)(charCode & 0x1Fu); // value is equal to the character code modulo 32

                                if (!parser.WithinSet)
                                {
                                    parser._appendCharSafe?.Invoke(sb!, (char)charCode);
                                    parser.ClearFollowingQuantifierError();
                                }
                                else
                                {
                                    ProcessSetChar((char)charCode, parser._appendCharSafe, parser, startIndex);
                                }
                                i++;
                                break;
                            }

                            if (parser.WithinSet)
                            {
                                // Within character sets, '_' and decimal digits are also allowed.
                                if ((charCode = pattern[i + 1]) == '_' || ((char)charCode).IsDecimalDigit())
                                {
                                    charCode = (ushort)(charCode & 0x1Fu); // value is equal to the character code modulo 32

                                    ProcessSetChar((char)charCode, parser._appendCharSafe, parser, startIndex);
                                    i++;
                                    break;
                                }
                            }
                        }

                        // Rewrite
                        // * unterminated caret notation escapes (e.g. /\c/ --> @"\\c",
                        // * invalid caret notation escapes (e.g. /\ch/ --> @"\\ch").
                        // (See also https://stackoverflow.com/a/48718489/8656352)
                        if (!parser.WithinSet)
                        {
                            sb?.Append('\\').Append('\\').Append(ch);
                            parser.ClearFollowingQuantifierError();
                        }
                        else
                        {
                            // Unterminated/invalid cases like \c is interpreted as \\c even in character sets:
                            // /[\c]/ is equivalent to @"[\\c]" (not a typo, this does match both '\' and 'c')
                            sb?.Append('\\');
                            ProcessSetChar('\\', parser._appendChar, parser, startIndex);
                            ProcessSetChar(ch, parser._appendChar, parser, startIndex);
                        }
                        break;

                    // CharacterEscape (octal)
                    case '0':
                        ParseOctalEscape(pattern, ref i, out charCode);
                        if (!parser.WithinSet)
                        {
                            parser._appendCharSafe?.Invoke(sb!, (char)charCode);
                            parser.ClearFollowingQuantifierError();
                        }
                        else
                        {
                            ProcessSetChar((char)charCode, parser._appendCharSafe, parser, startIndex);
                        }
                        break;

                    // DecimalEscape / CharacterEscape (octal)
                    case >= '1' and <= '9':
                        if (!parser.WithinSet)
                        {
                            // Outside character sets, numbers may be backreferences (in this case the number is interpreted as decimal).
                            if (parser.TryAdjustBackreference(startIndex, out conversionError))
                            {
                                if (conversionError is not null)
                                {
                                    return false;
                                }

                                parser.ClearFollowingQuantifierError();
                                break;
                            }
                        }

                        // When the number is not a backreference, it's an octal character code.
                        if (ch <= '7')
                        {
                            goto case '0';
                        }
                        else
                        {
                            // \8 and \9 are interpreted as plain digit characters. However, we can't simply unescape them because
                            // that might cause problems in patterns like /()\1\8/
                            if (!parser.WithinSet)
                            {
                                parser._appendCharSafe?.Invoke(sb!, ch);
                                parser.ClearFollowingQuantifierError();
                            }
                            else
                            {
                                ProcessSetChar(ch, parser._appendCharSafe, parser, startIndex);
                            }
                        }
                        break;

                    // 'k' GroupName
                    case 'k':
                        if (parser._capturingGroupNames is not { Count: > 0 })
                        {
                            // When the pattern contains no named capturing group,
                            // \k escapes are ignored - but not by the .NET regex engine,
                            // so they need to be rewritten (e.g. /\k<a>/ --> @"k<a>", /[\k<a>]/ --> @"[k<a>]").
                            sb?.Append(ch);
                            parser.ClearFollowingQuantifierError();
                            break;
                        }

                        if (!parser.WithinSet)
                        {
                            parser.AdjustNamedBackreference(startIndex, out conversionError);
                            if (conversionError is not null)
                            {
                                return false;
                            }

                            parser.ClearFollowingQuantifierError();
                        }
                        else
                        {
                            // \k escape sequence within character sets is not allowed
                            // (except when there are no named capturing groups; see above).
                            parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        }
                        break;

                    // CharacterClassEscape
                    case 'd' or 'D' or 's' or 'S' or 'w' or 'W':
                        // RegexOptions.ECMAScript incorrectly interprets \s as [\f\n\r\t\v\u0020]. This doesn't align with the JS specification,
                        // which defines \s as [\f\n\r\t\v\u0020\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]. We need to adjust both \s and \S.

                        const string invertedWhiteSpacePattern = "\0-\u0008\u000E-\u001F\\x21-\u009F\u00A1-\u167F\u1681-\u1FFF\u200B-\u2027\u202A-\u202E\u2030-\u205E\u2060-\u2FFF\u3001-\uFEFE\uFF00-\uFFFF";

                        if (!parser.WithinSet)
                        {
                            if (sb is not null)
                            {
                                if (ch == 's')
                                {
                                    sb.Append('[').Append('\\').Append(ch).Append(AdditionalWhiteSpacePattern).Append(']');
                                }
                                else if (ch == 'S')
                                {
                                    sb.Append('[').Append(invertedWhiteSpacePattern).Append(']');
                                }
                                else
                                {
                                    sb.Append(pattern, startIndex, 2);
                                }
                            }

                            parser.ClearFollowingQuantifierError();
                        }
                        else
                        {
                            if (sb is not null)
                            {
                                if (parser._setRangeStart < 0)
                                {
                                    // Cases like /[a-\d]/ are valid in JS but they're problematic in .NET, so they need to be escaped like @"[a\x2D\d]".
                                    sb.Remove(sb.Length - 1, 1);
                                    AppendCharSafe(sb, '-');
                                }

                                if (ch == 's')
                                {
                                    sb.Append('\\').Append(ch).Append(AdditionalWhiteSpacePattern);
                                }
                                else if (ch == 'S')
                                {
                                    sb.Append(invertedWhiteSpacePattern);
                                }
                                else
                                {
                                    sb.Append(pattern, startIndex, 2);
                                }
                            }

                            parser._setRangeStart = parser._setRangeStart >= 0 ? SetRangeStartedWithCharClass : SetRangeNotStarted;
                        }
                        break;

                    // \p and \P escapes are ignored - but not by the .NET regex engine,
                    // so they need to be rewritten (e.g. /\p{Sc}/ --> @"p{Sc}").
                    case 'p' or 'P':
                    // Several .NET-only escape sequences must be unescaped as RegexOptions.ECMAScript doesn't handle them correctly.
                    case 'a' or 'e':
                        if (!parser.WithinSet)
                        {
                            sb?.Append(ch);
                            parser.ClearFollowingQuantifierError();
                        }
                        else
                        {
                            ProcessSetChar(ch, parser._appendChar, parser, startIndex);
                        }
                        break;

                    default:
                        if (!parser.WithinSet)
                        {
                            if (ch is '<' or 'A' or 'Z' or 'z' or 'G')
                            {
                                // These .NET-only escape sequences must be unescaped outside character sets as RegexOptions.ECMAScript doesn't handle them correctly.
                                sb?.Append(ch);
                                parser.ClearFollowingQuantifierError();
                            }
                            else
                            {
                                sb?.Append(pattern, startIndex, 2);
                                if (ch is 'b' or 'B')
                                {
                                    parser.SetFollowingQuantifierError(RegExpNothingToRepeat);
                                }
                                else
                                {
                                    parser.ClearFollowingQuantifierError();
                                }
                            }
                        }
                        else
                        {
                            if (ch != '-')
                            {
                                if (!TryGetSimpleEscapeCharCode(ch, parser.WithinSet, out charCode))
                                {
                                    charCode = ch;
                                }

                                sb?.Append('\\');
                                ProcessSetChar(ch, charCode, parser._appendChar, parser, startIndex);
                            }
                            else
                            {
                                // Within character sets, when range starts with a \- escape sequence, RegexOptions.ECMAScript behaves weird,
                                // so we need to rewrite such cases (e.g. /[\--0]/ --> /[\x2D-0]/).
                                ProcessSetChar(ch, parser._appendCharSafe, parser, startIndex);
                            }
                        }
                        break;
                }

                conversionError = null;
                return true;
            }
        }
    }
}
