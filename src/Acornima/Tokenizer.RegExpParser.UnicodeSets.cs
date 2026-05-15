using System;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima;

using static SyntaxErrorMessages;

public partial class Tokenizer
{
    internal sealed partial class RegExpParser
    {
        // Return values for class set parsing methods.
        // Indicates whether the parsed construct can match multi-code-point strings.
        private const int CharSetNone = 0; // Nothing parsed
        private const int CharSetOk = 1; // Parsed, cannot contain strings
        private const int CharSetString = 2; // Parsed, can contain strings

        // Outside character sets, flag 'v' behaves like flag 'u'.

        #region Character set parsing

        // Based on: https://github.com/acornjs/acorn/blob/8.16.0/acorn/src/regexp.js

        private void ParseSetV(int startIndex)
        {
            // Parse the entire [...] block recursively.

            // We're positioned right after '['.
            ref var i = ref _index;

            var negate = Eat('^', ref i);

            var result = ClassSetExpression();

            if (negate && result == CharSetString)
            {
                ReportSyntaxError(startIndex, RegExpNegatedCharacterClassWithStrings);
            }

            // i is now at ']', advance past it.
            i++;
        }

        // https://tc39.es/ecma262/#prod-ClassSetExpression
        // https://tc39.es/ecma262/#prod-ClassUnion
        // https://tc39.es/ecma262/#prod-ClassIntersection
        // https://tc39.es/ecma262/#prod-ClassSubtraction
        private int ClassSetExpression()
        {
            // NOTE: `regexp_classContents` and `regexp_classSetExpression` was merged into this method
            // to keep the call stack shallow. Common error checking was moved here from call sites.
            // `regexp_eatClassSetRange` and a call to `regexp_eatClassSetOperand` were also merged into this method
            // to match the error reporting behavior of V8 and to avoid reparsing of escape sequences.

            ref var i = ref _index;
            int start;
            int left, right;

            int result = CharSetOk, subResult;

            for (var isSetOperationAllowed = true; ; isSetOperationAllowed = false)
            {
                // https://tc39.es/ecma262/#prod-ClassUnion
                if (_pattern.CharCodeAt(i) == ']')
                {
                    return result;
                }
                else if (EatClassSetCharacter(out left))
                {
                    // https://tc39.es/ecma262/#prod-ClassSetRange
                    if (Eat('-', ref i))
                    {
                        start = i;
                        if (Eat('-', ref i))
                        {
                            goto ClassSubtraction;
                        }
                        else if (EatClassSetCharacter(out right))
                        {
                            if (left > right)
                            {
                                ReportSyntaxError(start, RegExpRangeOutOfOrderCharacterClass);
                            }

                            isSetOperationAllowed = false;

                            if (Eat('-', ref i))
                            {
                                if (Eat('-', ref i))
                                {
                                    goto ClassSubtraction;
                                }
                                else
                                {
                                    ReportSyntaxError(i, RegExpInvalidCharacterClass);
                                }
                            }
                        }
                        else if (EatNestedClassOrClassStringDisjunction() != CharSetNone)
                        {
                            ReportSyntaxError(start, RegExpInvalidCharacterClass);
                        }
                        else
                        {
                            goto UnexpectedCharacter;
                        }
                    }
                }
                else if ((subResult = EatNestedClassOrClassStringDisjunction()) != CharSetNone)
                {
                    if (subResult == CharSetString)
                    {
                        result = CharSetString;
                    }

                    if (Eat('-', ref i))
                    {
                        if (Eat('-', ref i))
                        {
                            goto ClassSubtraction;
                        }
                        else
                        {
                            ReportSyntaxError(i, RegExpInvalidCharacterClass);
                        }
                    }
                }
                else
                {
                    goto UnexpectedCharacter;
                }

                // https://tc39.es/ecma262/#prod-ClassIntersection
                if (!EatChars('&', '&', ref i))
                {
                    goto MaybeClassSubtraction;
                }

                if (!isSetOperationAllowed)
                {
                    ReportSyntaxError(i - 2, RegExpInvalidClassSetOperation);
                }

                do
                {
                    if (_pattern.CharCodeAt(i) != '&')
                    {
                        // https://tc39.es/ecma262/#prod-ClassSetOperand
                        if (EatClassSetCharacter(out right))
                        {
                            continue;
                        }
                        else if ((subResult = EatNestedClassOrClassStringDisjunction()) != CharSetNone)
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
                while (EatChars('&', '&', ref i));

                right = _pattern.CharCodeAt(i);
                if (right == ']')
                {
                    return result;
                }
                else if (right >= 0)
                {
                    ReportSyntaxError(i, RegExpInvalidClassSetOperation);
                }
                else
                {
                    goto Unterminated;
                }

            MaybeClassSubtraction:
                // https://tc39.es/ecma262/#prod-ClassSubtraction
                if (!EatChars('-', '-', ref i))
                {
                    continue;
                }

            ClassSubtraction:
                if (!isSetOperationAllowed)
                {
                    ReportSyntaxError(i - 2, RegExpInvalidClassSetOperation);
                }

                do
                {
                    // https://tc39.es/ecma262/#prod-ClassSetOperand
                    if (EatClassSetCharacter(out right)
                        || EatNestedClassOrClassStringDisjunction() != CharSetNone)
                    {
                        continue;
                    }

                    goto UnexpectedCharacter;
                }
                while (EatChars('-', '-', ref i));

                right = _pattern.CharCodeAt(i);
                if (right == ']')
                {
                    return result;
                }
                else if (right >= 0)
                {
                    ReportSyntaxError(i, RegExpInvalidClassSetOperation);
                }
                else
                {
                    goto Unterminated;
                }
            }

        UnexpectedCharacter:
            if (_pattern.CharCodeAt(i) == '\\')
            {
                ReportSyntaxError(i, RegExpInvalidEscape);
            }
            else if (_pattern.CharCodeAt(i) >= 0)
            {
                ReportSyntaxError(i, RegExpInvalidCharacterInClass);
            }

        Unterminated:
            ReportSyntaxError(i, RegExpUnterminatedCharacterClass);
            return CharSetNone; // unreachable, just to keep the compiler happy
        }

        // https://tc39.es/ecma262/#prod-NestedClass
        // https://tc39.es/ecma262/#prod-ClassStringDisjunction
        private int EatNestedClassOrClassStringDisjunction()
        {
            // NOTE: `regexp_eatNestedClass` and `regexp_eatClassStringDisjunction ` was merged into this single method.

            ref var i = ref _index;
            var start = i;

            if (_pattern.CharCodeAt(i) == '\\')
            {
                var result = ConsumeCharacterClassEscapeV();
                if (result != CharSetNone)
                {
                    i++;
                    return result;
                }

                // \q{...}
                if (_pattern.CharCodeAt(i + 1) == 'q')
                {
                    i += 2;

                    if (!Eat('{', ref i))
                    {
                        ReportSyntaxError(start, RegExpInvalidEscape);
                    }

                    result = ClassStringDisjunctionContents();

                    if (!Eat('}', ref i))
                    {
                        if (_pattern.CharCodeAt(i) == '\\')
                        {
                            ReportSyntaxError(i, RegExpInvalidEscape);
                        }
                        else
                        {
                            ReportSyntaxError(i, RegExpInvalidCharacterInClass);
                        }
                    }

                    return result;
                }
            }

            if (Eat('[', ref i))
            {
                var negate = Eat('^', ref i);

                ref var recursionDepth = ref _recursionDepthProvider.CurrentDepth;
                StackGuard.EnsureSufficientExecutionStack(++recursionDepth);

                var result = ClassSetExpression();

                recursionDepth--;

                if (negate && result == CharSetString)
                {
                    ReportSyntaxError(start, RegExpNegatedCharacterClassWithStrings);
                }

                i++;
                return result;
            }

            return CharSetNone;
        }

        // https://tc39.es/ecma262/#prod-ClassStringDisjunctionContents
        private int ClassStringDisjunctionContents()
        {
            ref var i = ref _index;

            var result = ClassString();
            while (Eat('|', ref i))
            {
                if (ClassString() == CharSetString)
                {
                    result = CharSetString;
                }
            }

            return result;
        }

        // https://tc39.es/ecma262/#prod-ClassString
        // https://tc39.es/ecma262/#prod-NonEmptyClassString
        private int ClassString()
        {
            var count = 0;
            while (EatClassSetCharacter(out _))
            {
                count++;
            }

            return count == 1 ? CharSetOk : CharSetString;
        }

        // https://tc39.es/ecma262/#prod-ClassSetCharacter
        private bool EatClassSetCharacter(out int cp)
        {
            ref var i = ref _index;

            if (_pattern.CharCodeAt(i) == '\\')
            {
                if (ConsumeCharacterEscapeV(out cp))
                {
                    i++;
                    return true;
                }

                return false;
            }

            cp = _pattern.CodePointAt(i, _pattern.Length);
            if (cp <= char.MaxValue)
            {
                if (cp < 0 || IsClassSetSyntaxCharacter((char)cp))
                {
                    return false;
                }

                if (cp == _pattern.CharCodeAt(i + 1) && IsClassSetReservedDoublePunctuatorCharacter((char)cp))
                {
                    ReportSyntaxError(i, RegExpInvalidClassSetOperation);
                }
            }

            i += UnicodeHelper.GetCodePointLength((uint)cp);
            return true;
        }

        private bool ConsumeCharacterEscapeV(out int cp)
        {
            ref var i = ref _index;
            var startIndex = i++;
            int endIndex;

            if ((uint)i >= (uint)_pattern.Length)
            {
                ReportSyntaxError(startIndex, RegExpEscapeAtEndOfPattern);
            }

            ushort charCode, charCode2;
            var ch = _pattern[i];
            switch (ch)
            {
                // CharacterEscape -> RegExpUnicodeEscapeSequence -> u{ CodePoint }
                case 'u' when _pattern.CharCodeAt(i + 1) == '{':
                    if (TryReadCodePoint(_pattern, ref i, endIndex: _pattern.Length, out cp))
                    {
                        return true;
                    }

                    ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                    break;

                // CharacterEscape -> RegExpUnicodeEscapeSequence
                case 'u':
                    if (TryReadHexEscape(_pattern, ref i, endIndex: _pattern.Length, charCodeLength: 4, out charCode))
                    {
                        if (((char)charCode).IsHighSurrogate() && (uint)(i + 2) < (uint)_pattern.Length && _pattern[i + 1] == '\\' && _pattern[i + 2] == 'u')
                        {
                            endIndex = i + 2;
                            if (TryReadHexEscape(_pattern, ref endIndex, endIndex: _pattern.Length, charCodeLength: 4, out charCode2) && ((char)charCode2).IsLowSurrogate())
                            {
                                i = endIndex;
                                cp = (int)UnicodeHelper.GetCodePoint((char)charCode, (char)charCode2);
                                return true;
                            }
                        }

                        cp = charCode;
                        return true;
                    }

                    ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                    break;

                // CharacterEscape -> HexEscapeSequence
                case 'x':
                    if (TryReadHexEscape(_pattern, ref i, endIndex: _pattern.Length, charCodeLength: 2, out charCode))
                    {
                        cp = charCode;
                        return true;
                    }

                    ReportSyntaxError(startIndex, RegExpInvalidEscape);
                    break;

                // CharacterEscape -> c ControlLetter
                case 'c':
                    cp = _pattern.CharCodeAt(i + 1);
                    if (((char)cp).IsBasicLatinLetter())
                    {
                        i++;
                        cp = (ushort)(cp & 0x1F); // value is equal to the character code modulo 32
                        return true;
                    }

                    ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                    break;

                // CharacterEscape -> 0 [lookahead ∉ DecimalDigit]
                case '0':
                    if (!((char)_pattern.CharCodeAt(i + 1)).IsDecimalDigit())
                    {
                        cp = 0;
                        return true;
                    }

                    ReportSyntaxError(startIndex, RegExpInvalidDecimalEscape);
                    break;

                // DecimalEscape
                case >= '1' and <= '9':
                    if (ch >= '8')
                    {
                        ReportSyntaxError(startIndex, RegExpInvalidEscape);
                    }
                    else
                    {
                        ReportSyntaxError(startIndex, RegExpInvalidDecimalEscape);
                    }

                    break;

                default:
                    if (TryGetSimpleEscapeCharCode(ch, withinSet: true, out charCode))
                    {
                        cp = charCode;
                        return true;
                    }
                    else if (IsClassSetReservedPunctuator(ch))
                    {
                        cp = ch;
                        return true;
                    }

                    break;
            }

            i = startIndex;
            cp = default;
            return false;
        }

        private int ConsumeCharacterClassEscapeV()
        {
            ref var i = ref _index;
            var startIndex = i++;
            int endIndex;

            if ((uint)i >= (uint)_pattern.Length)
            {
                ReportSyntaxError(startIndex, RegExpEscapeAtEndOfPattern);
            }

            var ch = _pattern[i];
            switch (ch)
            {
                case 'd' or 'D' or 's' or 'S' or 'w' or 'W':
                    return CharSetOk;

                case 'p' or 'P':
                    var nameStartIndex = i + 1;
                    if (_pattern.CharCodeAt(nameStartIndex) == '{')
                    {
                        nameStartIndex++;
                        endIndex = _pattern.IndexOf('}', nameStartIndex);
                        if (endIndex >= 0)
                        {
                            var expression = _pattern.AsMemory(nameStartIndex, endIndex - nameStartIndex);

                            // First check if it's a valid unicode property (non-string).
                            if (ValidateUnicodeProperty(expression))
                            {
                                i = endIndex;
                                return CharSetOk;
                            }

                            // In flag 'v' mode, check binary properties of strings (e.g. Basic_Emoji).
                            if (UnicodeProperties.IsAllowedBinaryOfStringsValue(expression, _tokenizer._options._ecmaVersion))
                            {
                                if (ch == 'P')
                                {
                                    ReportSyntaxError(nameStartIndex, RegExpInvalidClassPropertyName);
                                }

                                i = endIndex;
                                return CharSetString;
                            }
                        }
                    }

                    ReportSyntaxError(nameStartIndex, RegExpInvalidClassPropertyName);
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
        private bool Eat(char ch, ref int i)
        {
            if (_pattern.CharCodeAt(i) == ch)
            {
                i++;
                return true;
            }

            return false;
        }

        // Try to eat a two-character sequence (e.g. '&&' or '--').
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EatChars(char ch1, char ch2, ref int i)
        {
            if (_pattern.CharCodeAt(i) == ch1 && _pattern.CharCodeAt(i + 1) == ch2)
            {
                i += 2;
                return true;
            }

            return false;
        }

        #endregion
    }
}
