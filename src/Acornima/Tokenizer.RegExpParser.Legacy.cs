using System;
using System.Runtime.CompilerServices;
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

            public void EatChar(char ch, RegExpParser parser) { }

            public void EatSetChar(char ch, RegExpParser parser, int startIndex)
            {
                ProcessSetChar(ch, parser, startIndex);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void ProcessSetChar(char ch, RegExpParser parser, int startIndex)
            {
                ProcessSetChar(ch, ch, parser, startIndex);
            }

            private static void ProcessSetChar(char ch, int charCode, RegExpParser parser, int startIndex)
            {
                if (parser._setRangeStart >= 0)
                {
                    parser._setRangeStart = charCode;
                }
                else
                {
                    parser._setRangeStart = ~parser._setRangeStart;

                    if (parser._setRangeStart > charCode && parser._setRangeStart <= UnicodeHelper.LastCodePoint)
                    {
                        // Cases like /[z-a]/ are syntax error. However, cases like /[\d-a]/ are ignored.
                        parser.ReportSyntaxError(startIndex, RegExpRangeOutOfOrderCharacterClass);
                    }

                    parser._setRangeStart = SetRangeNotStarted;
                }
            }

            public bool EatEscapeSequence(RegExpParser parser, int startIndex)
            {
                // https://tc39.es/ecma262/#prod-AtomEscape

                var pattern = parser._pattern;
                ref var i = ref parser._index;

                if ((uint)i >= (uint)pattern.Length)
                {
                    parser.ReportSyntaxError(startIndex, RegExpEscapeAtEndOfPattern);
                }

                var isQuantifiable = true;

                ushort charCode;
                var ch = pattern[i];
                switch (ch)
                {
                    // CharacterEscape -> RegExpUnicodeEscapeSequence
                    // CharacterEscape -> HexEscapeSequence
                    case 'u':
                    case 'x':
                        if (TryReadHexEscape(pattern, ref i, endIndex: pattern.Length, charCodeLength: ch == 'u' ? 4 : 2, out charCode))
                        {
                            if (parser.WithinSet)
                            {
                                ProcessSetChar((char)charCode, parser, startIndex);
                            }
                        }
                        else
                        {
                            // Ignore
                            // * unterminated \x escape sequences (e.g. /\x0/ --> @"x0"),
                            // * unterminated \u escape sequences (e.g. /\u012/ --> @"u012"),
                            // * invalid \x escape sequences (e.g. /\x0y/ --> @"x0y"),
                            // * invalid \u escape sequences (e.g. /\u012y/ --> @"u012y"), including
                            // * UTF32-like invalid escape sequences (e.g. /\u{0010FFFF}/ --> @"u{0010FFFF}").
                            if (parser.WithinSet)
                            {
                                ProcessSetChar(ch, parser, startIndex);
                            }
                        }
                        break;

                    // CharacterEscape -> c ControlLetter
                    case 'c':
                        charCode = (ushort)pattern.CharCodeAt(i + 1);
                        if (((char)charCode).IsBasicLatinLetter())
                        {
                            i++;
                            if (parser.WithinSet)
                            {
                                charCode = (ushort)(charCode & 0x1Fu); // value is equal to the character code modulo 32
                                ProcessSetChar((char)charCode, parser, startIndex);
                            }
                            break;
                        }

                        if (parser.WithinSet)
                        {
                            // Within character sets, '_' and decimal digits are also allowed.
                            if (charCode == '_' || ((char)charCode).IsDecimalDigit())
                            {
                                i++;
                                charCode = (ushort)(charCode & 0x1Fu); // value is equal to the character code modulo 32
                                ProcessSetChar((char)charCode, parser, startIndex);
                                break;
                            }
                        }

                        // Ignore
                        // * unterminated caret notation escapes (e.g. /\c/ --> @"\\c",
                        // * invalid caret notation escapes (e.g. /\ch/ --> @"\\ch").
                        // (See also https://stackoverflow.com/a/48718489/8656352)
                        if (parser.WithinSet)
                        {
                            // Unterminated/invalid cases like \c are interpreted as \\c even in character sets:
                            // /[\c]/ is equivalent to /[\\c]/ (not a typo, this does match both '\' and 'c')
                            ProcessSetChar('\\', parser, startIndex);
                            ProcessSetChar(ch, parser, startIndex);
                        }
                        break;

                    // CharacterEscape (octal)
                    case '0':
                        ReadOctalEscape(pattern, ref i, out charCode);
                        if (parser.WithinSet)
                        {
                            ProcessSetChar((char)charCode, parser, startIndex);
                        }
                        break;

                    // DecimalEscape / CharacterEscape (octal)
                    case >= '1' and <= '9':
                        if (!parser.WithinSet)
                        {
                            // Outside character sets, numbers may be backreferences (in this case the number is interpreted as decimal).
                            if (parser.TryConsumeBackreference(startIndex))
                            {
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
                            // \8 and \9 are interpreted as plain digit characters.
                            if (parser.WithinSet)
                            {
                                ProcessSetChar(ch, parser, startIndex);
                            }
                        }
                        break;

                    // 'k' GroupName
                    case 'k':
                        if (parser._capturingGroupNames is null)
                        {
                            if (!parser._hasScannedForCapturingGroups && parser._tokenizer._options._ecmaVersion >= EcmaVersion.ES9)
                            {
                                parser.ScanForCapturingGroups(startIndex);
                                if (parser._capturingGroupNames is not null)
                                {
                                    goto HasNamedCapturingGroups;
                                }
                            }

                            // When the pattern contains no named capturing group, \k escapes are ignored.
                            break;
                        }

                    HasNamedCapturingGroups:
                        if (!parser.WithinSet)
                        {
                            parser.ConsumeNamedBackreference(startIndex);
                        }
                        else
                        {
                            // \k escape sequences within character sets are not allowed
                            // (except when there are no named capturing groups; see above).
                            parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        }
                        break;

                    // CharacterClassEscape
                    case 'd' or 'D' or 's' or 'S' or 'w' or 'W':
                        if (parser.WithinSet)
                        {
                            parser._setRangeStart = parser._setRangeStart >= 0 ? SetRangeStartedWithCharClass : SetRangeNotStarted;
                        }
                        break;

                    // Assertion -> \b | \B
                    case 'b' or 'B' when !parser.WithinSet:
                        isQuantifiable = false;
                        break;

                    default:
                        if (parser.WithinSet)
                        {
                            if (!TryGetSimpleEscapeCharCode(ch, withinSet: true, out charCode))
                            {
                                charCode = ch;
                            }

                            ProcessSetChar(ch, charCode, parser, startIndex);
                        }
                        break;
                }

                i++;
                return isQuantifiable;
            }

            private static void ReadOctalEscape(string pattern, ref int i, out ushort charCode)
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

            public void ParseSet(RegExpParser parser, int startIndex)
            {
                parser.ParseSetDefault(this, startIndex);
            }

            public bool AllowsQuantifierAfterGroup(RegExpGroupType groupType)
            {
                // Lookbehind assertion groups may not be followed by quantifiers.
                // However, lookahead assertion groups may be.
                return groupType is not
                (
                    RegExpGroupType.LookbehindAssertion or
                    RegExpGroupType.NegativeLookbehindAssertion
                );
            }
        }
    }
}
