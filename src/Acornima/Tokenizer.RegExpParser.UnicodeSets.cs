using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Acornima.Helpers;

namespace Acornima;

using static SyntaxErrorMessages;

#pragma warning disable CS0618 // Type or member is obsolete

public partial class Tokenizer
{
    internal sealed partial class RegExpParser
    {
        private sealed class UnicodeSetsMode : IMode
        {
            // Return values for class set parsing methods.
            // Indicates whether the parsed construct can match multi-code-point strings.
            private const int CharSetNone = 0; // Nothing parsed
            private const int CharSetOk = 1; // Parsed, cannot contain strings
            private const int CharSetString = 2; // Parsed, can contain strings

            public static readonly UnicodeSetsMode Instance = new();

            private UnicodeSetsMode() { }

            // Outside character classes, flag 'v' behaves like flag 'u'.

            public void ProcessChar(char ch, Action<StringBuilder, char>? appender, RegExpParser parser)
            {
                var pattern = parser._pattern;
                ref var i = ref parser._index;

                if (ch.IsHighSurrogate() && ((char)pattern.CharCodeAt(i + 1)).IsLowSurrogate())
                {
                    i++;
                }
            }

            public void ProcessSetSpecialChar(char ch, RegExpParser parser)
            {
                Debug.Fail($"{nameof(ProcessSetSpecialChar)} should not be called for {nameof(UnicodeSetsMode)} as {nameof(ParseSet)} handles all set logic.");
            }

            public void ProcessSetChar(char ch, Action<StringBuilder, char>? appender, RegExpParser parser, int startIndex)
            {
                Debug.Fail($"{nameof(ProcessSetChar)} should not be called for {nameof(UnicodeSetsMode)} as {nameof(ParseSet)} handles all set logic.");
            }

            public bool RewriteSet(RegExpParser parser)
            {
                Debug.Fail($"{nameof(RewriteSet)} should not be called for {nameof(UnicodeSetsMode)} as it is validation only.");
                return false;
            }

            public void RewriteDot(RegExpParser parser)
            {
                // No-op for this mode as it is validation only.
            }

            public bool AllowsQuantifierAfterGroup(RegExpGroupType groupType)
            {
                return groupType is RegExpGroupType.Capturing or RegExpGroupType.NamedCapturing or RegExpGroupType.NonCapturing;
            }

            public void HandleInvalidRangeQuantifier(RegExpParser parser, int startIndex)
            {
                parser.ReportSyntaxError(startIndex, RegExpIncompleteQuantifier);
            }

            public bool AdjustEscapeSequence(RegExpParser parser, out RegExpConversionError? conversionError)
            {
                Debug.Assert(!parser.WithinSet, $"{nameof(AdjustEscapeSequence)} should not be called for sets in {nameof(UnicodeSetsMode)} as those escape sequences are handled by {nameof(EatCharacterEscape)} and {nameof(EatCharacterClassEscape)}.");

                // Since only validation is currently supported for flag 'v', parser._stringBuilder is always null.
                return UnicodeMode.AdjustEscapeSequence(allowStringProperties: true, parser, out conversionError);
            }

            #region Character class parsing

            // Based on: https://github.com/acornjs/acorn/blob/8.16.0/acorn/src/regexp.js

            public bool ParseSet(RegExpParser parser, out RegExpConversionError? conversionError)
            {
                // Parse the entire [...] block recursively.

                var pattern = parser._pattern;
                ref var i = ref parser._index;
                var start = i;

                // We're positioned at '['. Advance past it.
                i++;

                var negate = Eat('^', pattern, ref i);

                var result = ClassSetExpression(parser);

                // i is now at ']', the main loop will advance past it.

                if (negate && result == CharSetString)
                {
                    parser.ReportSyntaxError(start, RegExpNegatedCharacterClassWithStrings);
                }

                parser.ClearFollowingQuantifierError();

                conversionError = null;
                return true;
            }

            // https://tc39.es/ecma262/#prod-ClassSetExpression
            // https://tc39.es/ecma262/#prod-ClassUnion
            // https://tc39.es/ecma262/#prod-ClassIntersection
            // https://tc39.es/ecma262/#prod-ClassSubtraction
            private static int ClassSetExpression(RegExpParser parser)
            {
                // NOTE: `regexp_classContents` and `regexp_classSetExpression` was merged into this method
                // to keep the call stack shallow. Common error checking was moved here from call sites.
                // `regexp_eatClassSetRange` and a call to `regexp_eatClassSetOperand` were also merged into this method
                // to match the error reporting behavior of V8 and to avoid reparsing of escape sequences.

                var pattern = parser._pattern;
                ref var i = ref parser._index;
                int start;
                int left, right;

                int result = CharSetOk, subResult;

                for (var isSetOperationAllowed = true; ; isSetOperationAllowed = false)
                {
                    // https://tc39.es/ecma262/#prod-ClassUnion
                    if (pattern.CharCodeAt(i) == ']')
                    {
                        return result;
                    }
                    else if (EatClassSetCharacter(parser, out left))
                    {
                        // https://tc39.es/ecma262/#prod-ClassSetRange
                        if (Eat('-', pattern, ref i))
                        {
                            start = i;
                            if (Eat('-', pattern, ref i))
                            {
                                goto ClassSubtraction;
                            }
                            else if (EatClassSetCharacter(parser, out right))
                            {
                                if (left > right)
                                {
                                    parser.ReportSyntaxError(start, RegExpRangeOutOfOrderCharacterClass);
                                }

                                isSetOperationAllowed = false;

                                if (Eat('-', pattern, ref i))
                                {
                                    if (Eat('-', pattern, ref i))
                                    {
                                        goto ClassSubtraction;
                                    }
                                    else
                                    {
                                        parser.ReportSyntaxError(i, RegExpInvalidCharacterClass);
                                    }
                                }
                            }
                            else if (EatNestedClassOrClassStringDisjunction(parser) != CharSetNone)
                            {
                                parser.ReportSyntaxError(start, RegExpInvalidCharacterClass);
                            }
                            else
                            {
                                goto UnexpectedCharacter;
                            }
                        }
                    }
                    else if ((subResult = EatNestedClassOrClassStringDisjunction(parser)) != CharSetNone)
                    {
                        if (subResult == CharSetString)
                        {
                            result = CharSetString;
                        }

                        if (Eat('-', pattern, ref i))
                        {
                            if (Eat('-', pattern, ref i))
                            {
                                goto ClassSubtraction;
                            }
                            else
                            {
                                parser.ReportSyntaxError(i, RegExpInvalidCharacterClass);
                            }
                        }
                    }
                    else
                    {
                        goto UnexpectedCharacter;
                    }

                    // https://tc39.es/ecma262/#prod-ClassIntersection
                    if (!EatChars('&', '&', pattern, ref i))
                    {
                        goto MaybeClassSubtraction;
                    }

                    if (!isSetOperationAllowed)
                    {
                        parser.ReportSyntaxError(i - 2, RegExpInvalidClassSetOperation);
                    }

                    do
                    {
                        if (pattern.CharCodeAt(i) != '&')
                        {
                            // https://tc39.es/ecma262/#prod-ClassSetOperand
                            if (EatClassSetCharacter(parser, out right))
                            {
                                continue;
                            }
                            else if ((subResult = EatNestedClassOrClassStringDisjunction(parser)) != CharSetNone)
                            {
                                if (subResult != CharSetString)
                                {
                                    result = CharSetOk;
                                }

                                continue;
                            }
                        }

                        goto UnexpectedCharacter;
                    }
                    while (EatChars('&', '&', pattern, ref i));

                    right = pattern.CharCodeAt(i);
                    if (right == ']')
                    {
                        return result;
                    }
                    else if (right >= 0)
                    {
                        parser.ReportSyntaxError(i, RegExpInvalidClassSetOperation);
                    }
                    else
                    {
                        goto Unterminated;
                    }

                MaybeClassSubtraction:
                    // https://tc39.es/ecma262/#prod-ClassSubtraction
                    if (!EatChars('-', '-', pattern, ref i))
                    {
                        continue;
                    }

                ClassSubtraction:
                    if (!isSetOperationAllowed)
                    {
                        parser.ReportSyntaxError(i - 2, RegExpInvalidClassSetOperation);
                    }

                    do
                    {
                        // https://tc39.es/ecma262/#prod-ClassSetOperand
                        if (EatClassSetCharacter(parser, out right)
                            || EatNestedClassOrClassStringDisjunction(parser) != CharSetNone)
                        {
                            continue;
                        }

                        goto UnexpectedCharacter;
                    }
                    while (EatChars('-', '-', pattern, ref i));

                    right = pattern.CharCodeAt(i);
                    if (right == ']')
                    {
                        return result;
                    }
                    else if (right >= 0)
                    {
                        parser.ReportSyntaxError(i, RegExpInvalidClassSetOperation);
                    }
                    else
                    {
                        goto Unterminated;
                    }
                }

            UnexpectedCharacter:
                if (pattern.CharCodeAt(i) == '\\')
                {
                    parser.ReportSyntaxError(i, RegExpInvalidEscape);
                }
                else if (pattern.CharCodeAt(i) >= 0)
                {
                    parser.ReportSyntaxError(i, RegExpInvalidCharacterInClass);
                }

            Unterminated:
                parser.ReportSyntaxError(i, RegExpUnterminatedCharacterClass);
                return CharSetNone; // unreachable, just to keep the compiler happy
            }

            // https://tc39.es/ecma262/#prod-NestedClass
            // https://tc39.es/ecma262/#prod-ClassStringDisjunction
            private static int EatNestedClassOrClassStringDisjunction(RegExpParser parser)
            {
                // NOTE: `regexp_eatNestedClass` and `regexp_eatClassStringDisjunction ` was merged into this single method.

                var pattern = parser._pattern;
                ref var i = ref parser._index;
                var start = i;

                if (pattern.CharCodeAt(i) == '\\')
                {
                    var result = EatCharacterClassEscape(parser);
                    if (result != CharSetNone)
                    {
                        i++;
                        return result;
                    }

                    // \q{...}
                    if (pattern.CharCodeAt(i + 1) == 'q')
                    {
                        i += 2;

                        if (!Eat('{', pattern, ref i))
                        {
                            parser.ReportSyntaxError(start, RegExpInvalidEscape);
                        }

                        result = ClassStringDisjunctionContents(parser);

                        if (!Eat('}', pattern, ref i))
                        {
                            if (pattern.CharCodeAt(i) == '\\')
                            {
                                parser.ReportSyntaxError(i, RegExpInvalidEscape);
                            }
                            else
                            {
                                parser.ReportSyntaxError(i, RegExpInvalidCharacterInClass);
                            }
                        }

                        return result;
                    }
                }

                if (Eat('[', pattern, ref i))
                {
                    var negate = Eat('^', pattern, ref i);

                    ref var recursionDepth = ref parser._recursionDepthProvider.CurrentDepth;
                    StackGuard.EnsureSufficientExecutionStack(++recursionDepth);

                    var result = ClassSetExpression(parser);

                    recursionDepth--;

                    if (negate && result == CharSetString)
                    {
                        parser.ReportSyntaxError(start, RegExpNegatedCharacterClassWithStrings);
                    }

                    i++;
                    return result;
                }

                return CharSetNone;
            }

            // https://tc39.es/ecma262/#prod-ClassStringDisjunctionContents
            private static int ClassStringDisjunctionContents(RegExpParser parser)
            {
                var pattern = parser._pattern;
                ref var i = ref parser._index;

                var result = ClassString(parser);
                while (Eat('|', pattern, ref i))
                {
                    if (ClassString(parser) == CharSetString)
                    {
                        result = CharSetString;
                    }
                }

                return result;
            }

            // https://tc39.es/ecma262/#prod-ClassString
            // https://tc39.es/ecma262/#prod-NonEmptyClassString
            private static int ClassString(RegExpParser parser)
            {
                var count = 0;
                while (EatClassSetCharacter(parser, out _))
                {
                    count++;
                }

                return count == 1 ? CharSetOk : CharSetString;
            }

            // https://tc39.es/ecma262/#prod-ClassSetCharacter
            private static bool EatClassSetCharacter(RegExpParser parser, out int cp)
            {
                var pattern = parser._pattern;
                ref var i = ref parser._index;

                if (pattern.CharCodeAt(i) == '\\')
                {
                    if (EatCharacterEscape(parser, out cp))
                    {
                        i++;
                        return true;
                    }

                    return false;
                }

                cp = pattern.CodePointAt(i, pattern.Length);
                if (cp <= char.MaxValue)
                {
                    if (cp < 0 || IsClassSetSyntaxCharacter((char)cp))
                    {
                        return false;
                    }

                    if (cp == pattern.CharCodeAt(i + 1) && IsClassSetReservedDoublePunctuatorCharacter((char)cp))
                    {
                        parser.ReportSyntaxError(i, RegExpInvalidClassSetOperation);
                    }
                }

                i += UnicodeHelper.GetCodePointLength((uint)cp);
                return true;
            }

            private static bool EatCharacterEscape(RegExpParser parser, out int cp)
            {
                var pattern = parser._pattern;
                ref var i = ref parser._index;
                ushort charCode, charCode2;
                var startIndex = i++;
                int endIndex;
                var ch = pattern[i];
                switch (ch)
                {
                    // CharacterEscape -> RegExpUnicodeEscapeSequence -> u{ CodePoint }
                    case 'u' when pattern.CharCodeAt(i + 1) == '{':
                        if (TryReadCodePoint(pattern, ref i, endIndex: pattern.Length, out cp))
                        {
                            return true;
                        }

                        parser.ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                        break;

                    // CharacterEscape -> RegExpUnicodeEscapeSequence
                    case 'u':
                        if (TryReadHexEscape(pattern, ref i, endIndex: pattern.Length, charCodeLength: 4, out charCode))
                        {
                            if (((char)charCode).IsHighSurrogate() && i + 2 < pattern.Length && pattern[i + 1] == '\\' && pattern[i + 2] == 'u')
                            {
                                endIndex = i + 2;
                                if (TryReadHexEscape(pattern, ref endIndex, endIndex: pattern.Length, charCodeLength: 4, out charCode2) && ((char)charCode2).IsLowSurrogate())
                                {
                                    i = endIndex;
                                    cp = (int)UnicodeHelper.GetCodePoint((char)charCode, (char)charCode2);
                                    return true;
                                }
                            }

                            cp = charCode;
                            return true;
                        }

                        parser.ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                        break;

                    // CharacterEscape -> HexEscapeSequence
                    case 'x':
                        if (TryReadHexEscape(pattern, ref i, endIndex: pattern.Length, charCodeLength: 2, out charCode))
                        {
                            cp = charCode;
                            return true;
                        }

                        parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        break;

                    // CharacterEscape -> c ControlLetter
                    case 'c':
                        cp = pattern.CharCodeAt(i + 1);
                        if (((char)cp).IsBasicLatinLetter())
                        {
                            i++;
                            cp = (ushort)(cp & 0x1F); // value is equal to the character code modulo 32
                            return true;
                        }

                        parser.ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                        break;

                    // CharacterEscape -> 0 [lookahead ∉ DecimalDigit]
                    case '0':
                        if (!((char)pattern.CharCodeAt(i + 1)).IsDecimalDigit())
                        {
                            cp = 0;
                            return true;
                        }

                        parser.ReportSyntaxError(startIndex, RegExpInvalidDecimalEscape);
                        break;

                    // DecimalEscape
                    case >= '1' and <= '9':
                        if (ch >= '8')
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        }
                        else
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidDecimalEscape);
                        }

                        break;

                    default:
                        if (TryGetSimpleEscapeCharCode(ch, withinSet: true, out charCode)
                            || IsClassSetReservedPunctuator(ch))
                        {
                            cp = charCode;
                            return true;
                        }

                        break;
                }

                i = startIndex;
                cp = default;
                return false;
            }

            private static int EatCharacterClassEscape(RegExpParser parser)
            {
                var pattern = parser._pattern;
                ref var i = ref parser._index;
                var startIndex = i++;
                int endIndex;
                var ch = pattern.CharCodeAt(i);
                switch (ch)
                {
                    case 'd' or 'D' or 's' or 'S' or 'w' or 'W':
                        return CharSetOk;

                    case 'p' or 'P':
                        if (pattern.CharCodeAt(i + 1) == '{')
                        {
                            endIndex = pattern.IndexOf('}', i + 2);
                            if (endIndex >= 0)
                            {
                                var expression = pattern.AsMemory(i + 2, endIndex - (i + 2));

                                // First check if it's a valid unicode property (non-string).
                                if (UnicodeMode.ValidateUnicodeProperty(expression, translateToRanges: false, parser, out _))
                                {
                                    i = endIndex;
                                    return CharSetOk;
                                }

                                // In flag 'v' mode, check binary properties of strings (e.g. Basic_Emoji).
                                if (UnicodeProperties.IsAllowedBinaryOfStringsValue(expression, parser._tokenizer._options._ecmaVersion))
                                {
                                    if (ch == 'P')
                                    {
                                        parser.ReportSyntaxError(startIndex, RegExpInvalidClassPropertyName);
                                    }

                                    i = endIndex;
                                    return CharSetString;
                                }
                            }
                        }

                        parser.ReportSyntaxError(startIndex, RegExpInvalidClassPropertyName);
                        break;
                }

                i = startIndex;
                return CharSetNone;
            }

            // https://tc39.es/ecma262/#prod-ClassSetReservedDoublePunctuator
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsClassSetReservedDoublePunctuatorCharacter(char ch)
            {
                return ch == '!'
                    || ch.IsInRange('#', '&') // # $ % &
                    || ch.IsInRange('*', ',') // * + ,
                    || ch == '.'
                    || ch.IsInRange(':', '@') // : ; < = > ? @
                    || ch == '^'
                    || ch == '`'
                    || ch == '~';
            }

            // https://tc39.es/ecma262/#prod-ClassSetSyntaxCharacter
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsClassSetSyntaxCharacter(char ch)
            {
                return ch == '(' || ch == ')'
                    || ch == '-' || ch == '/'
                    || ch.IsInRange('[', ']') // [ \ ]
                    || ch.IsInRange('{', '}'); // { | }
            }

            // https://tc39.es/ecma262/#prod-ClassSetReservedPunctuator
            // ! # % & , - : ; < = > @ ` ~
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsClassSetReservedPunctuator(char ch)
            {
                return ch == '!'
                    || ch == '#'
                    || ch == '%'
                    || ch == '&'
                    || ch == ','
                    || ch == '-'
                    || ch.IsInRange(':', '>') // : ; < = >
                    || ch == '@'
                    || ch == '`'
                    || ch == '~';
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool Eat(char ch, string pattern, ref int i)
            {
                if (pattern.CharCodeAt(i) == ch)
                {
                    i++;
                    return true;
                }

                return false;
            }

            // Try to eat a two-character sequence (e.g. '&&' or '--').
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool EatChars(char ch1, char ch2, string pattern, ref int i)
            {
                if (pattern.CharCodeAt(i) == ch1 && pattern.CharCodeAt(i + 1) == ch2)
                {
                    i += 2;
                    return true;
                }

                return false;
            }

            #endregion
        }
    }
}
