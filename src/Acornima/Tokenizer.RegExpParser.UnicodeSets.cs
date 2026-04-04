using System;
using System.Runtime.CompilerServices;
using System.Text;
using Acornima.Helpers;

namespace Acornima;

using static SyntaxErrorMessages;

public partial class Tokenizer
{
    internal sealed partial class RegExpParser
    {
        // Return values for class set parsing methods.
        // Indicates whether the parsed construct can match multi-code-point strings.
        private const int CharSetNone = 0;   // Nothing parsed
        private const int CharSetOk = 1;     // Parsed, cannot contain strings
        private const int CharSetString = 2; // Parsed, can contain strings

        private const int MaxClassSetNestingDepth = 64;

        /// <summary>
        /// IMode implementation for RegExp unicode sets mode (v flag).
        /// Validation-only: no .NET Regex conversion is performed.
        /// The v flag implies unicode semantics (switchU=true in acorn) plus new class set expressions.
        /// </summary>
        private sealed class UnicodeSetsMode : IMode
        {
            public static readonly UnicodeSetsMode Instance = new();

            private UnicodeSetsMode() { }

            // Outside character classes, v-flag behaves like u-flag.
            // Since we're validation-only (sb is always null), we just need to validate syntax.

            public void ProcessChar(char ch, Action<StringBuilder, char>? appender, RegExpParser parser)
            {
                // Even in validation-only mode, we need unicode-aware consumption
                // (surrogate pairs) so the main pattern parser advances correctly.
                UnicodeMode.Instance.ProcessChar(ch, appender, parser);
            }

            public void ProcessSetSpecialChar(char ch, RegExpParser parser)
            {
                // Not called for UnicodeSetsMode — TryParseCharacterClass handles all set logic.
            }

            public void ProcessSetChar(char ch, Action<StringBuilder, char>? appender, RegExpParser parser, int startIndex)
            {
                // Not called for UnicodeSetsMode — TryParseCharacterClass handles all set logic.
            }

            public bool RewriteSet(RegExpParser parser)
            {
                // Not called for UnicodeSetsMode.
                return false;
            }

            public void RewriteDot(RegExpParser parser)
            {
                // Validation-only: no rewriting needed.
            }

            public bool AllowsQuantifierAfterGroup(RegExpGroupType groupType)
            {
                // Same as UnicodeMode.
                return groupType is RegExpGroupType.Capturing or RegExpGroupType.NamedCapturing or RegExpGroupType.NonCapturing;
            }

            public void HandleInvalidRangeQuantifier(RegExpParser parser, int startIndex)
            {
                // In unicode modes, invalid quantifiers are syntax errors (same as UnicodeMode).
                parser.ReportSyntaxError(startIndex, RegExpIncompleteQuantifier);
            }

            public bool AdjustEscapeSequence(RegExpParser parser, out RegExpConversionError? conversionError)
            {
                // Handle \p{...} and \P{...} specially — v-flag allows binary properties of strings (e.g. RGI_Emoji).
                ref var i = ref parser._index;
                var pattern = parser._pattern;
                var ch = pattern.CharCodeAt(i + 1);

                if ((ch == 'p' || ch == 'P') && parser._tokenizer._options._ecmaVersion >= EcmaVersion.ES9)
                {
                    if (pattern.CharCodeAt(i + 2) == '{')
                    {
                        var endIndex = pattern.IndexOf('}', i + 3);
                        if (endIndex >= 0)
                        {
                            var expression = pattern.AsMemory(i + 3, endIndex - (i + 3));
                            var isStringProperty = UnicodeProperties.IsAllowedBinaryOfStringsValue(expression, parser._tokenizer._options._ecmaVersion);
                            if (ValidateUnicodeProperty(expression, translateToRanges: false, parser, out _)
                                || isStringProperty)
                            {
                                // \P (negated) is not allowed with string properties.
                                if (ch == 'P' && isStringProperty)
                                {
                                    parser.ReportSyntaxError(i, RegExpInvalidPropertyName);
                                }

                                i = endIndex;
                                parser.ClearFollowingQuantifierError();
                                conversionError = null;
                                return true;
                            }
                        }
                        parser.ReportSyntaxError(i, RegExpInvalidPropertyName);
                    }
                }

                // For all other escapes, delegate to UnicodeMode.
                return UnicodeMode.Instance.AdjustEscapeSequence(parser, out conversionError);
            }

            public bool TryParseCharacterClass(RegExpParser parser)
            {
                // Parse the entire [...] block recursively.
                ref var i = ref parser._index;
                var pattern = parser._pattern;
                var classStart = i;

                if (++parser._classSetNestingDepth > MaxClassSetNestingDepth)
                {
                    parser.ReportSyntaxError(i, RegExpInvalidCharacterInClass);
                }

                // We're positioned at '['. Advance past it.
                var negate = pattern.CharCodeAt(i + 1) == '^';
                i += negate ? 2 : 1;

                var result = ClassContents(parser);

                if (pattern.CharCodeAt(i) != ']')
                {
                    parser.ReportSyntaxError(i, RegExpUnterminatedCharacterClass);
                }

                if (negate && result == CharSetString)
                {
                    parser.ReportSyntaxError(classStart, RegExpNegatedCharacterClassWithStrings);
                }

                parser._classSetNestingDepth--;

                // i is now at ']'; the main loop will advance past it.
                return true;
            }

            // https://tc39.es/ecma262/#prod-ClassContents
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int ClassContents(RegExpParser parser)
            {
                if (parser._pattern.CharCodeAt(parser._index) == ']')
                {
                    return CharSetOk; // empty class
                }

                return ClassSetExpression(parser);
            }

            // https://tc39.es/ecma262/#prod-ClassSetExpression
            // https://tc39.es/ecma262/#prod-ClassUnion
            // https://tc39.es/ecma262/#prod-ClassIntersection
            // https://tc39.es/ecma262/#prod-ClassSubtraction
            private static int ClassSetExpression(RegExpParser parser)
            {
                ref var i = ref parser._index;
                var pattern = parser._pattern;
                int result = CharSetOk, subResult;

                if (EatClassSetRange(parser))
                {
                    // Continue with ClassUnion processing below.
                }
                else if ((subResult = EatClassSetOperand(parser)) != CharSetNone)
                {
                    if (subResult == CharSetString)
                    {
                        result = CharSetString;
                    }

                    // https://tc39.es/ecma262/#prod-ClassIntersection
                    var start = i;
                    while (EatChars(pattern, ref i, '&', '&'))
                    {
                        if (pattern.CharCodeAt(i) != '&'
                            && (subResult = EatClassSetOperand(parser)) != CharSetNone)
                        {
                            if (subResult != CharSetString)
                            {
                                result = CharSetOk;
                            }
                            continue;
                        }
                        parser.ReportSyntaxError(i, RegExpInvalidCharacterInClass);
                    }
                    if (start != i)
                    {
                        return result;
                    }

                    // https://tc39.es/ecma262/#prod-ClassSubtraction
                    while (EatChars(pattern, ref i, '-', '-'))
                    {
                        if (EatClassSetOperand(parser) != CharSetNone)
                        {
                            continue;
                        }
                        parser.ReportSyntaxError(i, RegExpInvalidCharacterInClass);
                    }
                    if (start != i)
                    {
                        return result;
                    }
                }
                else
                {
                    parser.ReportSyntaxError(i, RegExpInvalidCharacterInClass);
                }

                // https://tc39.es/ecma262/#prod-ClassUnion
                for (; ; )
                {
                    if (EatClassSetRange(parser))
                    {
                        continue;
                    }

                    subResult = EatClassSetOperand(parser);
                    if (subResult == CharSetNone)
                    {
                        return result;
                    }
                    if (subResult == CharSetString)
                    {
                        result = CharSetString;
                    }
                }
            }

            // https://tc39.es/ecma262/#prod-ClassSetRange
            private static bool EatClassSetRange(RegExpParser parser)
            {
                var saved = parser._index;
                if (EatClassSetCharacter(parser, out var left))
                {
                    if (parser._pattern.CharCodeAt(parser._index) == '-')
                    {
                        parser._index++;
                        if (EatClassSetCharacter(parser, out var right))
                        {
                            if (left != -1 && right != -1 && left > right)
                            {
                                parser.ReportSyntaxError(saved, RegExpRangeOutOfOrderCharacterClass);
                            }
                            return true;
                        }
                    }
                    parser._index = saved;
                }
                return false;
            }

            // https://tc39.es/ecma262/#prod-ClassSetOperand
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int EatClassSetOperand(RegExpParser parser)
            {
                if (EatClassSetCharacter(parser, out _))
                {
                    return CharSetOk;
                }

                int result;
                if ((result = EatClassStringDisjunction(parser)) != CharSetNone)
                {
                    return result;
                }

                return EatNestedClass(parser);
            }

            // https://tc39.es/ecma262/#prod-NestedClass
            private static int EatNestedClass(RegExpParser parser)
            {
                ref var i = ref parser._index;
                var pattern = parser._pattern;
                var saved = i;

                if (pattern.CharCodeAt(i) == '[')
                {
                    if (++parser._classSetNestingDepth > MaxClassSetNestingDepth)
                    {
                        parser.ReportSyntaxError(i, RegExpInvalidCharacterInClass);
                    }

                    i++;
                    var negate = pattern.CharCodeAt(i) == '^';
                    if (negate)
                    {
                        i++;
                    }

                    var result = ClassContents(parser);

                    if (pattern.CharCodeAt(i) == ']')
                    {
                        i++;
                        parser._classSetNestingDepth--;
                        if (negate && result == CharSetString)
                        {
                            parser.ReportSyntaxError(saved, RegExpNegatedCharacterClassWithStrings);
                        }
                        return result;
                    }

                    parser._classSetNestingDepth--;
                    i = saved;
                }

                if (pattern.CharCodeAt(i) == '\\')
                {
                    i++; // advance past '\'
                    var result = EatCharacterClassEscape(parser);
                    if (result != CharSetNone)
                    {
                        return result;
                    }
                    i = saved;
                }

                return CharSetNone;
            }

            // https://tc39.es/ecma262/#prod-ClassStringDisjunction
            private static int EatClassStringDisjunction(RegExpParser parser)
            {
                ref var i = ref parser._index;
                var pattern = parser._pattern;
                var saved = i;

                // \q{...}
                if (EatChars(pattern, ref i, '\\', 'q'))
                {
                    if (pattern.CharCodeAt(i) == '{')
                    {
                        i++;
                        var result = ClassStringDisjunctionContents(parser);
                        if (pattern.CharCodeAt(i) == '}')
                        {
                            i++;
                            return result;
                        }
                    }
                    else
                    {
                        // Make the same message as V8.
                        parser.ReportSyntaxError(saved, RegExpInvalidEscape);
                    }
                    i = saved;
                }

                return CharSetNone;
            }

            // https://tc39.es/ecma262/#prod-ClassStringDisjunctionContents
            private static int ClassStringDisjunctionContents(RegExpParser parser)
            {
                var result = ClassString(parser);
                while (parser._pattern.CharCodeAt(parser._index) == '|')
                {
                    parser._index++;
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
                // Exactly 1 character → CharSetOk (single code point, not a "string").
                // 0 or 2+ characters → CharSetString (empty string or multi-code-point string).
                return count == 1 ? CharSetOk : CharSetString;
            }

            // https://tc39.es/ecma262/#prod-ClassSetCharacter
            private static bool EatClassSetCharacter(RegExpParser parser, out int lastIntValue)
            {
                ref var i = ref parser._index;
                var pattern = parser._pattern;
                var saved = i;

                if (pattern.CharCodeAt(i) == '\\')
                {
                    i++;
                    // CharacterEscape
                    if (EatCharacterEscape(parser, out lastIntValue))
                    {
                        return true;
                    }
                    // ClassSetReservedPunctuator
                    if (EatClassSetReservedPunctuator(pattern, ref i, out lastIntValue))
                    {
                        return true;
                    }
                    // \b (backspace in character class)
                    if (pattern.CharCodeAt(i) == 'b')
                    {
                        i++;
                        lastIntValue = 0x08; // BS
                        return true;
                    }
                    i = saved;
                    lastIntValue = -1;
                    return false;
                }

                var ch = pattern.CharCodeAt(i);
                if (ch < 0)
                {
                    lastIntValue = -1;
                    return false;
                }

                // Check for ClassSetReservedDoublePunctuator: if current char equals next char and is a reserved double punctuator character
                var next = pattern.CharCodeAt(i + 1);
                if (ch == next && IsClassSetReservedDoublePunctuatorCharacter(ch))
                {
                    lastIntValue = -1;
                    return false;
                }

                if (IsClassSetSyntaxCharacter(ch))
                {
                    lastIntValue = -1;
                    return false;
                }

                // Read as code point (handles surrogate pairs)
                lastIntValue = pattern.CodePointAt(i, pattern.Length);
                i += lastIntValue > 0xFFFF ? 2 : 1;
                return true;
            }

            // Parse character escape sequences valid in unicode mode.
            // Returns true if a valid escape was consumed, with the code point in lastIntValue.
            private static bool EatCharacterEscape(RegExpParser parser, out int lastIntValue)
            {
                ref var i = ref parser._index;
                var pattern = parser._pattern;
                var ch = pattern.CharCodeAt(i);

                switch (ch)
                {
                    // IdentityEscape for syntax characters: ^ $ \ . * + ? ( ) [ ] { } |
                    case '^' or '$' or '\\' or '.' or '*' or '+' or '?' or '(' or ')' or '[' or ']' or '{' or '}' or '|' or '/':
                        lastIntValue = ch;
                        i++;
                        return true;

                    // \f \n \r \t \v
                    case 'f':
                        lastIntValue = '\f';
                        i++;
                        return true;
                    case 'n':
                        lastIntValue = '\n';
                        i++;
                        return true;
                    case 'r':
                        lastIntValue = '\r';
                        i++;
                        return true;
                    case 't':
                        lastIntValue = '\t';
                        i++;
                        return true;
                    case 'v':
                        lastIntValue = '\v';
                        i++;
                        return true;

                    // \cA-\cZ, \ca-\cz
                    case 'c':
                        if (i + 1 < pattern.Length && ((char)pattern.CharCodeAt(i + 1)).IsBasicLatinLetter())
                        {
                            i++;
                            lastIntValue = char.ToUpperInvariant(pattern[i]) - '@';
                            i++;
                            return true;
                        }
                        lastIntValue = -1;
                        return false;

                    // \0 (NUL, only when not followed by a digit)
                    case '0':
                        if (!((char)pattern.CharCodeAt(i + 1)).IsDecimalDigit())
                        {
                            lastIntValue = 0;
                            i++;
                            return true;
                        }
                        lastIntValue = -1;
                        return false;

                    // \xHH
                    case 'x':
                    {
                        var escapeStart = i;
                        if (TryReadHexEscape(pattern, ref i, pattern.Length, 2, out var hexValue))
                        {
                            // TryReadHexEscape leaves i at the last hex digit; advance past it.
                            i++;
                            lastIntValue = hexValue;
                            return true;
                        }
                        parser.ReportSyntaxError(escapeStart - 1, RegExpInvalidEscape);
                        lastIntValue = -1;
                        return false;
                    }

                    // \uHHHH or \u{HHHH}
                    case 'u':
                    {
                        var escapeStart = i;
                        if (TryReadUnicodeEscape(pattern, ref i, out var cp))
                        {
                            lastIntValue = cp;
                            return true;
                        }
                        parser.ReportSyntaxError(escapeStart - 1, RegExpInvalidUnicodeEscape);
                        lastIntValue = -1;
                        return false;
                    }

                    default:
                        lastIntValue = -1;
                        return false;
                }
            }

            // Try to read \uHHHH or \u{HHHH...} escape. On entry, i points to 'u'. On success, i is past the escape.
            private static bool TryReadUnicodeEscape(string pattern, ref int i, out int codePoint)
            {
                var saved = i;
                // i points to 'u'

                if (pattern.CharCodeAt(i + 1) == '{')
                {
                    // \u{HHHH...}
                    i += 2; // past 'u{'
                    var start = i;
                    uint value = 0;
                    while (true)
                    {
                        var ch = pattern.CharCodeAt(i);
                        if (ch == '}')
                        {
                            if (i > start && value <= 0x10FFFF)
                            {
                                i++;
                                codePoint = (int)value;
                                return true;
                            }
                            break;
                        }

                        var digit = HexValue(ch);
                        if (digit < 0)
                        {
                            break;
                        }
                        value = (value << 4) | (uint)digit;
                        if (value > 0x10FFFF)
                        {
                            break; // early exit on overflow
                        }
                        i++;
                    }
                    i = saved;
                    codePoint = -1;
                    return false;
                }
                else
                {
                    // \uHHHH — TryReadHexEscape expects i at 'u', reads from i+1.
                    // TryReadHexEscape leaves i at the last hex digit; we advance past it.
                    if (TryReadHexEscape(pattern, ref i, pattern.Length, 4, out var high))
                    {
                        i++; // advance past last hex digit
                        if (((char)high).IsHighSurrogate()
                            && pattern.CharCodeAt(i) == '\\'
                            && pattern.CharCodeAt(i + 1) == 'u')
                        {
                            var pairPos = i + 1; // at 'u' of second escape
                            if (TryReadHexEscape(pattern, ref pairPos, pattern.Length, 4, out var low)
                                && ((char)low).IsLowSurrogate())
                            {
                                i = pairPos + 1; // advance past last hex digit of second escape
                                codePoint = (int)UnicodeHelper.GetCodePoint((char)high, (char)low);
                                return true;
                            }
                        }
                        codePoint = high;
                        return true;
                    }
                    i = saved;
                    codePoint = -1;
                    return false;
                }
            }

            // Shared hex escape reader: reads exactly `charCodeLength` hex digits.
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool TryReadHexEscape(string pattern, ref int i, int endIndex, int charCodeLength, out ushort value)
            {
                return RegExpParser.TryReadHexEscape(pattern, ref i, endIndex, charCodeLength, out value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int HexValue(int ch)
            {
                if ((uint)(ch - '0') <= 9) return ch - '0';
                if ((uint)(ch - 'a') <= 5) return ch - 'a' + 10;
                if ((uint)(ch - 'A') <= 5) return ch - 'A' + 10;
                return -1;
            }

            // \p{...} and \P{...} character class escapes, plus \d \D \s \S \w \W.
            // On entry, i points past '\' (at the escape letter: d, D, s, S, w, W, p, P).
            // Returns CharSetOk, CharSetString, or CharSetNone if nothing was consumed.
            private static int EatCharacterClassEscape(RegExpParser parser)
            {
                ref var i = ref parser._index;
                var pattern = parser._pattern;
                var ch = pattern.CharCodeAt(i);

                // \d \D \s \S \w \W
                if (ch is 'd' or 'D' or 's' or 'S' or 'w' or 'W')
                {
                    i++;
                    return CharSetOk;
                }

                // \p{...} or \P{...}
                bool negate;
                if ((negate = ch == 'P') || ch == 'p')
                {
                    var escapeStart = i - 1; // points at '\'
                    i++;
                    if (pattern.CharCodeAt(i) == '{')
                    {
                        i++;
                        var propStart = i;
                        while (true)
                        {
                            var c = pattern.CharCodeAt(i);
                            if (c == '}')
                            {
                                break;
                            }
                            if (c < 0)
                            {
                                parser.ReportSyntaxError(escapeStart, RegExpInvalidClassPropertyName);
                                break; // unreachable
                            }
                            i++;
                        }

                        var expression = pattern.AsMemory(propStart, i - propStart);
                        i++; // past '}'

                        // First check if it's a valid unicode property (non-string).
                        if (ValidateUnicodeProperty(expression, translateToRanges: false, parser, out _))
                        {
                            return CharSetOk;
                        }

                        // In v-flag mode, check binary properties of strings (e.g. Basic_Emoji).
                        if (UnicodeProperties.IsAllowedBinaryOfStringsValue(expression, parser._tokenizer._options._ecmaVersion))
                        {
                            if (negate)
                            {
                                parser.ReportSyntaxError(escapeStart, RegExpInvalidClassPropertyName);
                            }
                            return CharSetString;
                        }

                        parser.ReportSyntaxError(escapeStart, RegExpInvalidClassPropertyName);
                    }
                    // Backtrack if no '{' follows
                    i = escapeStart + 1; // restore to where we were (at 'p'/'P')
                }

                return CharSetNone;
            }

            // https://tc39.es/ecma262/#prod-ClassSetReservedPunctuator
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool EatClassSetReservedPunctuator(string pattern, ref int i, out int lastIntValue)
            {
                var ch = pattern.CharCodeAt(i);
                if (IsClassSetReservedPunctuator(ch))
                {
                    lastIntValue = ch;
                    i++;
                    return true;
                }
                lastIntValue = -1;
                return false;
            }

            // https://tc39.es/ecma262/#prod-ClassSetReservedPunctuator
            // ! # % & , - : ; < = > @ ` ~
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsClassSetReservedPunctuator(int ch)
            {
                return ch == '!'
                    || ch == '#'
                    || ch == '%'
                    || ch == '&'
                    || ch == ','
                    || ch == '-'
                    || (uint)(ch - ':') <= (uint)('>'-':') // : ; < = >
                    || ch == '@'
                    || ch == '`'
                    || ch == '~';
            }

            // https://tc39.es/ecma262/#prod-ClassSetReservedDoublePunctuator
            // Characters that are forbidden when they appear consecutively (doubled).
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsClassSetReservedDoublePunctuatorCharacter(int ch)
            {
                return ch == '!'
                    || (uint)(ch - '#') <= (uint)('&'-'#') // # $ % &
                    || (uint)(ch - '*') <= (uint)(','-'*') // * + ,
                    || ch == '.'
                    || (uint)(ch - ':') <= (uint)('@'-':') // : ; < = > ? @
                    || ch == '^'
                    || ch == '`'
                    || ch == '~';
            }

            // https://tc39.es/ecma262/#prod-ClassSetSyntaxCharacter
            // ( ) - / [ \ ] { | }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsClassSetSyntaxCharacter(int ch)
            {
                return ch == '(' || ch == ')'
                    || ch == '-' || ch == '/'
                    || (uint)(ch - '[') <= (uint)(']'-'[') // [ \ ]
                    || (uint)(ch - '{') <= (uint)('}'-'{'); // { | }
            }

            // Try to eat a two-character sequence (e.g. '&&' or '--').
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool EatChars(string pattern, ref int i, char c1, char c2)
            {
                if (pattern.CharCodeAt(i) == c1 && pattern.CharCodeAt(i + 1) == c2)
                {
                    i += 2;
                    return true;
                }
                return false;
            }
        }
    }
}
