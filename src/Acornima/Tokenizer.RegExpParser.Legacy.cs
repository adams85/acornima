using System;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima;

using static SyntaxErrorMessages;

public partial class Tokenizer
{
    internal partial class RegExpParser
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EatSetChar(char ch, int startIndex)
        {
            ProcessSetChar(ch, startIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessSetChar(char ch, int startIndex)
        {
            ProcessSetChar(ch, ch, startIndex);
        }

        private void ProcessSetChar(char ch, int charCode, int startIndex)
        {
            if (_setRangeStart >= 0)
            {
                _setRangeStart = charCode;
            }
            else
            {
                _setRangeStart = ~_setRangeStart;

                if (_setRangeStart > charCode && _setRangeStart <= UnicodeHelper.LastCodePoint)
                {
                    // Cases like /[z-a]/ are syntax error. However, cases like /[\d-a]/ are ignored.
                    ReportSyntaxError(startIndex, RegExpRangeOutOfOrderCharacterClass);
                }

                _setRangeStart = SetRangeNotStarted;
            }
        }

        private bool EatEscapeSequence(int startIndex)
        {
            // https://tc39.es/ecma262/#prod-AtomEscape

            ref var i = ref _index;

            if ((uint)i >= (uint)_pattern.Length)
            {
                ReportSyntaxError(startIndex, RegExpEscapeAtEndOfPattern);
            }

            var isQuantifiable = true;

            ushort charCode;
            var ch = _pattern[i];
            switch (ch)
            {
                // CharacterEscape -> RegExpUnicodeEscapeSequence
                // CharacterEscape -> HexEscapeSequence
                case 'u':
                case 'x':
                    if (TryReadHexEscape(_pattern, ref i, endIndex: _pattern.Length, charCodeLength: ch == 'u' ? 4 : 2, out charCode))
                    {
                        if (WithinSet)
                        {
                            ProcessSetChar((char)charCode, startIndex);
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
                        if (WithinSet)
                        {
                            ProcessSetChar(ch, startIndex);
                        }
                    }
                    break;

                // CharacterEscape -> c ControlLetter
                case 'c':
                    charCode = (ushort)_pattern.CharCodeAt(i + 1);
                    if (((char)charCode).IsBasicLatinLetter())
                    {
                        i++;
                        if (WithinSet)
                        {
                            charCode = (ushort)(charCode & 0x1Fu); // value is equal to the character code modulo 32
                            ProcessSetChar((char)charCode, startIndex);
                        }
                        break;
                    }

                    if (WithinSet)
                    {
                        // Within character sets, '_' and decimal digits are also allowed.
                        if (charCode == '_' || ((char)charCode).IsDecimalDigit())
                        {
                            i++;
                            charCode = (ushort)(charCode & 0x1Fu); // value is equal to the character code modulo 32
                            ProcessSetChar((char)charCode, startIndex);
                            break;
                        }
                    }

                    // Ignore
                    // * unterminated caret notation escapes (e.g. /\c/ --> @"\\c",
                    // * invalid caret notation escapes (e.g. /\ch/ --> @"\\ch").
                    // (See also https://stackoverflow.com/a/48718489/8656352)
                    if (WithinSet)
                    {
                        // Unterminated/invalid cases like \c are interpreted as \\c even in character sets:
                        // /[\c]/ is equivalent to /[\\c]/ (not a typo, this does match both '\' and 'c')
                        ProcessSetChar('\\', startIndex);
                        ProcessSetChar(ch, startIndex);
                    }
                    break;

                // CharacterEscape (octal)
                case '0':
                    ReadOctalEscape(ref i, out charCode);
                    if (WithinSet)
                    {
                        ProcessSetChar((char)charCode, startIndex);
                    }
                    break;

                // DecimalEscape / CharacterEscape (octal)
                case >= '1' and <= '9':
                    if (!WithinSet)
                    {
                        // Outside character sets, numbers may be backreferences (in this case the number is interpreted as decimal).
                        if (TryConsumeBackreference(startIndex))
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
                        if (WithinSet)
                        {
                            ProcessSetChar(ch, startIndex);
                        }
                    }
                    break;

                // 'k' GroupName
                case 'k':
                    if (_capturingGroupNames is null)
                    {
                        if (!_hasScannedForCapturingGroups && _tokenizer._options._ecmaVersion >= EcmaVersion.ES9)
                        {
                            ScanForCapturingGroups(startIndex);
                            if (_capturingGroupNames is not null)
                            {
                                goto HasNamedCapturingGroups;
                            }
                        }

                        // When the pattern contains no named capturing group, \k escapes are ignored.
                        break;
                    }

                HasNamedCapturingGroups:
                    if (!WithinSet)
                    {
                        ConsumeNamedBackreference(startIndex);
                    }
                    else
                    {
                        // \k escape sequences within character sets are not allowed
                        // (except when there are no named capturing groups; see above).
                        ReportSyntaxError(startIndex, RegExpInvalidEscape);
                    }
                    break;

                // CharacterClassEscape
                case 'd' or 'D' or 's' or 'S' or 'w' or 'W':
                    if (WithinSet)
                    {
                        _setRangeStart = _setRangeStart >= 0 ? SetRangeStartedWithCharClass : SetRangeNotStarted;
                    }
                    break;

                // Assertion -> \b | \B
                case 'b' or 'B' when !WithinSet:
                    isQuantifiable = false;
                    break;

                default:
                    if (WithinSet)
                    {
                        if (!TryGetSimpleEscapeCharCode(ch, withinSet: true, out charCode))
                        {
                            charCode = ch;
                        }

                        ProcessSetChar(ch, charCode, startIndex);
                    }
                    break;
            }

            i++;
            return isQuantifiable;
        }

        private void ReadOctalEscape(ref int i, out ushort charCode)
        {
            charCode = 0;
            var endIndex = Math.Min(i + 3, _pattern.Length);
            do
            {
                var newCharCode = (ushort)((charCode << 3) + (_pattern[i] - '0'));
                if (newCharCode > 0xFF)
                {
                    break;
                }
                charCode = newCharCode;
            }
            while (++i < endIndex && _pattern[i].IsOctalDigit());

            i--;
        }
    }
}
