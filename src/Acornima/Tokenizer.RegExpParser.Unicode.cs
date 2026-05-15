using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima;

using static SyntaxErrorMessages;

public partial class Tokenizer
{
    internal partial class RegExpParser
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EatCharU(char ch)
        {
            if (ch.IsHighSurrogate())
            {
                ref var i = ref _index;

                if (((char)_pattern.CharCodeAt(i)).IsLowSurrogate())
                {
                    i++;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EatSetCharU(char ch, int startIndex)
        {
            if (ch.IsHighSurrogate())
            {
                ref var i = ref _index;
                char ch2;

                if ((ch2 = (char)_pattern.CharCodeAt(i)).IsLowSurrogate())
                {
                    i++;
                    ProcessSetCodePoint((int)UnicodeHelper.GetCodePoint(ch, ch2), startIndex);
                    return;
                }
            }

            ProcessSetCodePoint(ch, startIndex);
        }

        private void ProcessSetCodePoint(int cp, int startIndex)
        {
            Debug.Assert(cp is >= 0 and <= UnicodeHelper.LastCodePoint, "Invalid end code point.");

            if (_setRangeStart >= 0)
            {
                _setRangeStart = cp;
            }
            else
            {
                _setRangeStart = ~_setRangeStart;

                // Cases like /[z-a]/u or /[\d-a]/u are syntax error.
                if (_setRangeStart > cp)
                {
                    if (_setRangeStart <= UnicodeHelper.LastCodePoint)
                    {
                        ReportSyntaxError(startIndex, RegExpRangeOutOfOrderCharacterClass);
                    }
                    else
                    {
                        ReportSyntaxError(startIndex, RegExpInvalidCharacterClass);
                    }
                }

                _setRangeStart = SetRangeNotStarted;
            }
        }

        private bool EatEscapeSequenceU(int startIndex)
        {
            // https://tc39.es/ecma262/#prod-AtomEscape

            ref var i = ref _index;
            int endIndex;

            if ((uint)i >= (uint)_pattern.Length)
            {
                ReportSyntaxError(startIndex, RegExpEscapeAtEndOfPattern);
            }

            var isQuantifiable = true;

            ushort charCode, charCode2;
            int cp;
            var ch = _pattern[i];
            switch (ch)
            {
                // CharacterEscape -> RegExpUnicodeEscapeSequence -> u{ CodePoint }
                case 'u' when _pattern.CharCodeAt(i + 1) == '{':
                    if (TryReadCodePoint(_pattern, ref i, endIndex: _pattern.Length, out cp))
                    {
                        if (WithinSet)
                        {
                            ProcessSetCodePoint(cp, startIndex);
                        }
                    }
                    else
                    {
                        ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                    }
                    break;

                // CharacterEscape -> RegExpUnicodeEscapeSequence
                // CharacterEscape -> HexEscapeSequence
                case 'u':
                case 'x':
                    if (TryReadHexEscape(_pattern, ref i, endIndex: _pattern.Length, charCodeLength: ch == 'u' ? 4 : 2, out charCode))
                    {
                        if (ch == 'u' && ((char)charCode).IsHighSurrogate() && (uint)(i + 2) < (uint)_pattern.Length && _pattern[i + 1] == '\\' && _pattern[i + 2] == 'u')
                        {
                            endIndex = i + 2;
                            if (TryReadHexEscape(_pattern, ref endIndex, endIndex: _pattern.Length, charCodeLength: 4, out charCode2) && ((char)charCode2).IsLowSurrogate())
                            {
                                i = endIndex;

                                if (WithinSet)
                                {
                                    cp = (int)UnicodeHelper.GetCodePoint((char)charCode, (char)charCode2);
                                    ProcessSetCodePoint(cp, startIndex);
                                }

                                break;
                            }
                        }

                        if (WithinSet)
                        {
                            ProcessSetCodePoint((char)charCode, startIndex);
                        }
                    }
                    else
                    {
                        if (ch == 'u')
                        {
                            ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                        }
                        else
                        {
                            ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        }
                    }
                    break;

                // CharacterEscape -> c ControlLetter
                case 'c':
                    cp = _pattern.CharCodeAt(i + 1);
                    if (((char)cp).IsBasicLatinLetter())
                    {
                        i++;
                        if (WithinSet)
                        {
                            charCode = (ushort)(cp & 0x1F); // value is equal to the character code modulo 32
                            ProcessSetCodePoint(charCode, startIndex);
                        }
                        break;
                    }

                    ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                    break;

                // CharacterEscape -> 0 [lookahead ∉ DecimalDigit]
                case '0':
                    if (!((char)_pattern.CharCodeAt(i + 1)).IsDecimalDigit())
                    {
                        if (WithinSet)
                        {
                            ProcessSetCodePoint(0, startIndex);
                        }
                    }
                    else
                    {
                        ReportSyntaxError(startIndex, RegExpInvalidDecimalEscape);
                    }
                    break;

                // DecimalEscape
                case >= '1' and <= '9':
                    if (!WithinSet)
                    {
                        // Outside character sets, numbers may be backreferences (in this case the number is interpreted as decimal).
                        if (TryConsumeBackreference(startIndex))
                        {
                            break;
                        }
                    }

                    // When the number is not a backreference, it's a syntax error.
                    if (!WithinSet || ch >= '8')
                    {
                        ReportSyntaxError(startIndex, RegExpInvalidEscape);
                    }
                    else
                    {
                        ReportSyntaxError(startIndex, RegExpInvalidDecimalEscape);
                    }
                    break;

                // 'k' GroupName
                case 'k':
                    if (!WithinSet && _tokenizer._options._ecmaVersion >= EcmaVersion.ES9)
                    {
                        ConsumeNamedBackreference(startIndex);
                    }
                    else
                    {
                        // \k escape sequences before ES2018 or within character sets are not allowed.
                        ReportSyntaxError(startIndex, RegExpInvalidEscape);
                    }
                    break;

                // CharacterClassEscape
                case 'd' or 'D' or 's' or 'S' or 'w' or 'W':
                    if (WithinSet)
                    {
                        if (_setRangeStart < 0)
                        {
                            ReportSyntaxError(startIndex, RegExpInvalidCharacterClass);
                        }

                        _setRangeStart = SetRangeStartedWithCharClass;
                    }
                    break;

                // CharacterClassEscape -> p{ UnicodePropertyValueExpression }
                // CharacterClassEscape -> P{ UnicodePropertyValueExpression }
                case 'p' or 'P':
                    if (_tokenizer._options._ecmaVersion >= EcmaVersion.ES9)
                    {
                        var nameStartIndex = i + 1;
                        if (_pattern.CharCodeAt(nameStartIndex) == '{')
                        {
                            ReadOnlyMemory<char> expression;

                            nameStartIndex++;
                            endIndex = _pattern.IndexOf('}', nameStartIndex);
                            if (endIndex >= 0
                            && (ValidateUnicodeProperty(expression = _pattern.AsMemory(nameStartIndex, endIndex - nameStartIndex))
                                || _isUnicodeSets && ch != 'P' && UnicodeProperties.IsAllowedBinaryOfStringsValue(expression, _tokenizer._options._ecmaVersion)))
                            {
                                if (WithinSet)
                                {
                                    if (_setRangeStart < 0)
                                    {
                                        ReportSyntaxError(startIndex, RegExpInvalidCharacterClass);
                                    }

                                    _setRangeStart = SetRangeStartedWithCharClass;
                                }

                                i = endIndex;
                                break;
                            }
                        }

                        if (!WithinSet)
                        {
                            ReportSyntaxError(nameStartIndex, RegExpInvalidPropertyName);
                        }
                        else
                        {
                            ReportSyntaxError(nameStartIndex, RegExpInvalidClassPropertyName);
                        }
                    }
                    else
                    {
                        // \p and \P escape sequences before ES2018 are not allowed.
                        ReportSyntaxError(startIndex, RegExpInvalidEscape);
                    }
                    break;

                // Assertion -> \b | \B
                case 'b' or 'B' when !WithinSet:
                    isQuantifiable = false;
                    break;

                default:
                    if (!TryGetSimpleEscapeCharCode(ch, WithinSet, out charCode))
                    {
                        ReportSyntaxError(startIndex, RegExpInvalidEscape);
                    }

                    if (WithinSet)
                    {
                        ProcessSetCodePoint(charCode, startIndex);
                    }
                    break;
            }

            i++;
            return isQuantifiable;
        }

        private bool ValidateUnicodeProperty(ReadOnlyMemory<char> expression)
        {
            var index = expression.Span.IndexOf('=');
            if (index >= 0)
            {
                // https://tc39.es/ecma262/#table-nonbinary-unicode-properties

                var propertyName = expression.Span.Slice(0, index);
                expression = expression.Slice(index + 1);
                return propertyName switch
                {
                    "gc" or "General_Category" => UnicodeProperties.IsAllowedGeneralCategoryValue(expression),
                    "sc" or "Script" or "scx" or "Script_Extensions" => UnicodeProperties.IsAllowedScriptValue(expression, _tokenizer._options._ecmaVersion),
                    _ => false,
                };
            }
            else
            {
                return UnicodeProperties.IsAllowedGeneralCategoryValue(expression)
                    || UnicodeProperties.IsAllowedBinaryValue(expression, _tokenizer._options._ecmaVersion);
            }
        }
    }
}
