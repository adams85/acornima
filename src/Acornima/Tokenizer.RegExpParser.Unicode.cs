using System;
using System.Diagnostics;
using Acornima.Helpers;

namespace Acornima;

using static SyntaxErrorMessages;

public partial class Tokenizer
{
    internal partial class RegExpParser
    {
        private sealed class UnicodeMode : IMode
        {
            public static readonly UnicodeMode Instance = new();

            private UnicodeMode() { }

            public void EatChar(char ch, RegExpParser parser)
            {
                if (ch.IsHighSurrogate())
                {
                    var pattern = parser._pattern;
                    ref var i = ref parser._index;

                    if (((char)pattern.CharCodeAt(i)).IsLowSurrogate())
                    {
                        i++;
                    }
                }
            }

            public void EatSetChar(char ch, RegExpParser parser, int startIndex)
            {
                if (ch.IsHighSurrogate())
                {
                    var pattern = parser._pattern;
                    ref var i = ref parser._index;
                    char ch2;

                    if ((ch2 = (char)pattern.CharCodeAt(i)).IsLowSurrogate())
                    {
                        i++;
                        ProcessSetCodePoint((int)UnicodeHelper.GetCodePoint(ch, ch2), parser, startIndex);
                        return;
                    }
                }

                ProcessSetCodePoint(ch, parser, startIndex);
            }

            private static void ProcessSetCodePoint(int cp, RegExpParser parser, int startIndex)
            {
                Debug.Assert(cp is >= 0 and <= UnicodeHelper.LastCodePoint, "Invalid end code point.");

                if (parser._setRangeStart >= 0)
                {
                    parser._setRangeStart = cp;
                }
                else
                {
                    parser._setRangeStart = ~parser._setRangeStart;

                    // Cases like /[z-a]/u or /[\d-a]/u are syntax error.
                    if (parser._setRangeStart > cp)
                    {
                        if (parser._setRangeStart <= UnicodeHelper.LastCodePoint)
                        {
                            parser.ReportSyntaxError(startIndex, RegExpRangeOutOfOrderCharacterClass);
                        }
                        else
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidCharacterClass);
                        }
                    }

                    parser._setRangeStart = SetRangeNotStarted;
                }
            }

            public bool EatEscapeSequence(RegExpParser parser, int startIndex)
            {
                return EatEscapeSequence(allowStringProperties: false, parser, startIndex);
            }

            internal static bool EatEscapeSequence(bool allowStringProperties, RegExpParser parser, int startIndex)
            {
                // https://tc39.es/ecma262/#prod-AtomEscape

                var pattern = parser._pattern;
                ref var i = ref parser._index;
                int endIndex;

                if ((uint)i >= (uint)pattern.Length)
                {
                    parser.ReportSyntaxError(startIndex, RegExpEscapeAtEndOfPattern);
                }

                var isQuantifiable = true;

                ushort charCode, charCode2;
                int cp;
                var ch = pattern[i];
                switch (ch)
                {
                    // CharacterEscape -> RegExpUnicodeEscapeSequence -> u{ CodePoint }
                    case 'u' when pattern.CharCodeAt(i + 1) == '{':
                        if (TryReadCodePoint(pattern, ref i, endIndex: pattern.Length, out cp))
                        {
                            if (parser.WithinSet)
                            {
                                ProcessSetCodePoint(cp, parser, startIndex);
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                        }
                        break;

                    // CharacterEscape -> RegExpUnicodeEscapeSequence
                    // CharacterEscape -> HexEscapeSequence
                    case 'u':
                    case 'x':
                        if (TryReadHexEscape(pattern, ref i, endIndex: pattern.Length, charCodeLength: ch == 'u' ? 4 : 2, out charCode))
                        {
                            if (ch == 'u' && ((char)charCode).IsHighSurrogate() && (uint)(i + 2) < (uint)pattern.Length && pattern[i + 1] == '\\' && pattern[i + 2] == 'u')
                            {
                                endIndex = i + 2;
                                if (TryReadHexEscape(pattern, ref endIndex, endIndex: pattern.Length, charCodeLength: 4, out charCode2) && ((char)charCode2).IsLowSurrogate())
                                {
                                    i = endIndex;

                                    if (parser.WithinSet)
                                    {
                                        cp = (int)UnicodeHelper.GetCodePoint((char)charCode, (char)charCode2);
                                        ProcessSetCodePoint(cp, parser, startIndex);
                                    }

                                    break;
                                }
                            }

                            if (parser.WithinSet)
                            {
                                ProcessSetCodePoint((char)charCode, parser, startIndex);
                            }
                        }
                        else
                        {
                            if (ch == 'u')
                            {
                                parser.ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                            }
                            else
                            {
                                parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                            }
                        }
                        break;

                    // CharacterEscape -> c ControlLetter
                    case 'c':
                        cp = pattern.CharCodeAt(i + 1);
                        if (((char)cp).IsBasicLatinLetter())
                        {
                            i++;
                            if (parser.WithinSet)
                            {
                                charCode = (ushort)(cp & 0x1F); // value is equal to the character code modulo 32
                                ProcessSetCodePoint(charCode, parser, startIndex);
                            }
                            break;
                        }

                        parser.ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                        break;

                    // CharacterEscape -> 0 [lookahead ∉ DecimalDigit]
                    case '0':
                        if (!((char)pattern.CharCodeAt(i + 1)).IsDecimalDigit())
                        {
                            if (parser.WithinSet)
                            {
                                ProcessSetCodePoint(0, parser, startIndex);
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidDecimalEscape);
                        }
                        break;

                    // DecimalEscape
                    case >= '1' and <= '9':
                        if (!parser.WithinSet)
                        {
                            // Outside character sets, numbers may be backreferences (in this case the number is interpreted as decimal).
                            if (parser.TryConsumeBackreference(startIndex))
                            {
                                break;
                            }
                        }

                        // When the number is not a backreference, it's a syntax error.
                        if (!parser.WithinSet || ch >= '8')
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        }
                        else
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidDecimalEscape);
                        }
                        break;

                    // 'k' GroupName
                    case 'k':
                        if (!parser.WithinSet && parser._tokenizer._options._ecmaVersion >= EcmaVersion.ES9)
                        {
                            parser.ConsumeNamedBackreference(startIndex);
                        }
                        else
                        {
                            // \k escape sequences before ES2018 or within character sets are not allowed.
                            parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        }
                        break;

                    // CharacterClassEscape
                    case 'd' or 'D' or 's' or 'S' or 'w' or 'W':
                        if (parser.WithinSet)
                        {
                            if (parser._setRangeStart < 0)
                            {
                                parser.ReportSyntaxError(startIndex, RegExpInvalidCharacterClass);
                            }

                            parser._setRangeStart = SetRangeStartedWithCharClass;
                        }
                        break;

                    // CharacterClassEscape -> p{ UnicodePropertyValueExpression }
                    // CharacterClassEscape -> P{ UnicodePropertyValueExpression }
                    case 'p' or 'P':
                        if (parser._tokenizer._options._ecmaVersion >= EcmaVersion.ES9)
                        {
                            var nameStartIndex = i + 1;
                            if (pattern.CharCodeAt(nameStartIndex) == '{')
                            {
                                ReadOnlyMemory<char> expression;

                                nameStartIndex++;
                                endIndex = pattern.IndexOf('}', nameStartIndex);
                                if (endIndex >= 0
                                    && (ValidateUnicodeProperty(expression = pattern.AsMemory(nameStartIndex, endIndex - nameStartIndex), parser)
                                        || allowStringProperties && ch != 'P' && UnicodeProperties.IsAllowedBinaryOfStringsValue(expression, parser._tokenizer._options._ecmaVersion)))
                                {
                                    if (parser.WithinSet)
                                    {
                                        if (parser._setRangeStart < 0)
                                        {
                                            parser.ReportSyntaxError(startIndex, RegExpInvalidCharacterClass);
                                        }

                                        parser._setRangeStart = SetRangeStartedWithCharClass;
                                    }

                                    i = endIndex;
                                    break;
                                }
                            }

                            if (!parser.WithinSet)
                            {
                                parser.ReportSyntaxError(nameStartIndex, RegExpInvalidPropertyName);
                            }
                            else
                            {
                                parser.ReportSyntaxError(nameStartIndex, RegExpInvalidClassPropertyName);
                            }
                        }
                        else
                        {
                            // \p and \P escape sequences before ES2018 are not allowed.
                            parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        }
                        break;

                    // Assertion -> \b | \B
                    case 'b' or 'B' when !parser.WithinSet:
                        isQuantifiable = false;
                        break;

                    default:
                        if (!TryGetSimpleEscapeCharCode(ch, parser.WithinSet, out charCode))
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        }

                        if (parser.WithinSet)
                        {
                            ProcessSetCodePoint(charCode, parser, startIndex);
                        }
                        break;
                }

                i++;
                return isQuantifiable;
            }

            internal static bool ValidateUnicodeProperty(ReadOnlyMemory<char> expression, RegExpParser parser)
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
                        "sc" or "Script" or "scx" or "Script_Extensions" => UnicodeProperties.IsAllowedScriptValue(expression, parser._tokenizer._options._ecmaVersion),
                        _ => false,
                    };
                }
                else
                {
                    return UnicodeProperties.IsAllowedGeneralCategoryValue(expression)
                        || UnicodeProperties.IsAllowedBinaryValue(expression, parser._tokenizer._options._ecmaVersion);
                }
            }

            public void ParseSet(RegExpParser parser, int startIndex)
            {
                parser.ParseSetDefault(this, startIndex);
            }

            public bool AllowsQuantifierAfterGroup(RegExpGroupType groupType)
            {
                // Assertion groups may not be followed by quantifiers.
                return groupType is not
                (
                    RegExpGroupType.LookaheadAssertion or
                    RegExpGroupType.NegativeLookaheadAssertion or
                    RegExpGroupType.LookbehindAssertion or
                    RegExpGroupType.NegativeLookbehindAssertion
                );
            }
        }
    }
}
