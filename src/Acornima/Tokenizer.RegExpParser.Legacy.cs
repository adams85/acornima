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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void EatSetChar(char ch, RegExpParser parser, int startIndex)
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

            public void EatEscapeSequence(RegExpParser parser)
            {
                // https://tc39.es/ecma262/#prod-AtomEscape

                var pattern = parser._pattern;
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
                                parser.ClearFollowingQuantifierError();
                            }
                            else
                            {
                                EatSetChar((char)charCode, parser, startIndex);
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
                            if (!parser.WithinSet)
                            {
                                parser.ClearFollowingQuantifierError();
                            }
                            else
                            {
                                EatSetChar(ch, parser, startIndex);
                            }
                        }
                        break;

                    // CharacterEscape -> c ControlLetter
                    case 'c':
                        charCode = (ushort)pattern.CharCodeAt(i + 1);
                        if (((char)charCode).IsBasicLatinLetter())
                        {
                            i++;
                            if (!parser.WithinSet)
                            {
                                parser.ClearFollowingQuantifierError();
                            }
                            else
                            {
                                charCode = (ushort)(charCode & 0x1Fu); // value is equal to the character code modulo 32
                                EatSetChar((char)charCode, parser, startIndex);
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
                                EatSetChar((char)charCode, parser, startIndex);
                                break;
                            }
                        }

                        // Ignore
                        // * unterminated caret notation escapes (e.g. /\c/ --> @"\\c",
                        // * invalid caret notation escapes (e.g. /\ch/ --> @"\\ch").
                        // (See also https://stackoverflow.com/a/48718489/8656352)
                        if (!parser.WithinSet)
                        {
                            parser.ClearFollowingQuantifierError();
                        }
                        else
                        {
                            // Unterminated/invalid cases like \c are interpreted as \\c even in character sets:
                            // /[\c]/ is equivalent to /[\\c]/ (not a typo, this does match both '\' and 'c')
                            EatSetChar('\\', parser, startIndex);
                            EatSetChar(ch, parser, startIndex);
                        }
                        break;

                    // CharacterEscape (octal)
                    case '0':
                        ReadOctalEscape(pattern, ref i, out charCode);
                        if (!parser.WithinSet)
                        {
                            parser.ClearFollowingQuantifierError();
                        }
                        else
                        {
                            EatSetChar((char)charCode, parser, startIndex);
                        }
                        break;

                    // DecimalEscape / CharacterEscape (octal)
                    case >= '1' and <= '9':
                        if (!parser.WithinSet)
                        {
                            // Outside character sets, numbers may be backreferences (in this case the number is interpreted as decimal).
                            if (parser.TryEatBackreference(startIndex))
                            {
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
                            // \8 and \9 are interpreted as plain digit characters.
                            if (!parser.WithinSet)
                            {
                                parser.ClearFollowingQuantifierError();
                            }
                            else
                            {
                                EatSetChar(ch, parser, startIndex);
                            }
                        }
                        break;

                    // 'k' GroupName
                    case 'k':
                        if (parser._capturingGroupNames is not { Count: > 0 })
                        {
                            // TODO: reparse?

                            // When the pattern contains no named capturing group, \k escapes are ignored.
                            if (!parser.WithinSet)
                            {
                                parser.ClearFollowingQuantifierError();
                            }
                            break;
                        }

                        if (!parser.WithinSet)
                        {
                            parser.EatNamedBackreference(startIndex);
                            parser.ClearFollowingQuantifierError();
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
                        if (!parser.WithinSet)
                        {
                            parser.ClearFollowingQuantifierError();
                        }
                        else
                        {
                            parser._setRangeStart = parser._setRangeStart >= 0 ? SetRangeStartedWithCharClass : SetRangeNotStarted;
                        }
                        break;

                    default:
                        if (!parser.WithinSet)
                        {
                            if (ch is 'b' or 'B')
                            {
                                parser.SetFollowingQuantifierError(RegExpNothingToRepeat);
                            }
                            else
                            {
                                parser.ClearFollowingQuantifierError();
                            }
                        }
                        else
                        {
                            if (!TryGetSimpleEscapeCharCode(ch, withinSet: true, out charCode))
                            {
                                charCode = ch;
                            }

                            ProcessSetChar(ch, charCode, parser, startIndex);
                        }
                        break;
                }
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

            public void ParseSet(RegExpParser parser)
            {
                parser.ParseSetDefault(this);
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

            public void HandleInvalidRangeQuantifier(RegExpParser parser, int startIndex)
            {
                // Invalid {} quantifiers like /.{/, /.{}/, /.{-1}/, etc. are ignored.

                parser.ClearFollowingQuantifierError();
            }
        }
    }
}
