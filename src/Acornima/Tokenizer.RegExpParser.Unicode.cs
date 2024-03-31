using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Acornima.Helpers;
using Acornima.Properties;

namespace Acornima;

using static SyntaxErrorMessages;
using static RegExpConversionErrorMessages;

public partial class Tokenizer
{
    internal partial struct RegExpParser
    {
        private const string MatchSurrogatePairRegex = "[\uD800-\uDBFF][\uDC00-\uDFFF]";
        private const string MatchLoneSurrogateRegex = "[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]";
        private const string MatchAnyButSurrogateRegex = "[^\uD800-\uDFFF]";
        private const string MatchAnyButNewLineAndSurrogateRegex = "[^\n\r\u2028\u2029\uD800-\uDFFF]";

        private sealed class UnicodeMode : IMode
        {
            public static readonly UnicodeMode Instance = new();

            private UnicodeMode() { }

            public void ProcessChar(ref ParsePatternContext context, char ch, Action<StringBuilder, char>? appender, ref RegExpParser parser)
            {
                ref readonly var sb = ref context.StringBuilder;
                ref readonly var pattern = ref parser._pattern;
                ref var i = ref context.Index;

                if (!ch.IsSurrogate())
                {
                    appender?.Invoke(sb!, ch);
                }
                else if (ch.IsHighSurrogate() && ((char)pattern.CharCodeAt(i + 1)).IsLowSurrogate())
                {
                    // Surrogate pairs should be surrounded by a non-capturing group to act as one character (because of cases like /💩+/u).
                    sb?.Append("(?:").Append(ch).Append(pattern[i + 1]).Append(')');
                    i++;
                }
                else
                {
                    if (sb is not null)
                    {
                        AppendLoneSurrogate(sb, ch);
                    }
                }
            }

            private static void AppendCodePointSafe(StringBuilder sb, int cp)
            {
                if (cp > char.MaxValue)
                {
                    // Surrogate pairs should be surrounded by a non-capturing group to act as one character (because of cases like /\ud83d\udca9+/u).
                    sb.Append("(?:").AppendCodePoint(cp).Append(')');
                }
                else
                {
                    Debug.Assert(cp >= 0, "Invalid code point.");
                    AppendUnicodeCharSafe(sb, (char)cp);
                }
            }

            private static void AppendUnicodeCharSafe(StringBuilder sb, char ch)
            {
                if (!ch.IsSurrogate())
                {
                    AppendCharSafe(sb, ch);
                }
                else
                {
                    AppendLoneSurrogate(sb, ch);
                }
            }

            private static void AppendLoneSurrogate(StringBuilder sb, char ch)
            {
                if (sb is not null)
                {
                    // Lone surrogates must not match parts of surrogate pairs
                    // (see https://exploringjs.com/es6/ch_regexp.html#_consequence-lone-surrogates-in-the-regular-expression-only-match-lone-surrogates).
                    // We can simulate this using negative lookbehind/lookahead.

                    sb.Append("(?:");
                    _ = ch.IsHighSurrogate()
                        ? sb.Append(ch).Append("(?![\uDC00-\uDFFF])")
                        : sb.Append("(?<![\uD800-\uDBFF])").Append(ch);
                    sb.Append(')');
                }
            }

            public void ProcessSetSpecialChar(ref ParsePatternContext context, char ch) { }

            public void ProcessSetChar(ref ParsePatternContext context, char ch, Action<StringBuilder, char>? appender, ref RegExpParser parser, int startIndex)
            {
                ref readonly var pattern = ref parser._pattern;
                ref var i = ref context.Index;

                if (ch.IsHighSurrogate() && ((char)pattern.CharCodeAt(i + 1)).IsLowSurrogate())
                {
                    // Surrogate pairs should be surrounded by a non-capturing group to act as one character.
                    AddSetCodePoint(ref context, (int)UnicodeHelper.GetCodePoint(ch, pattern[i + 1]), ref parser, startIndex);
                    i++;
                }
                else
                {
                    AddSetCodePoint(ref context, ch, ref parser, startIndex);
                }
            }

            private static void AddSetCodePoint(ref ParsePatternContext context, int cp, ref RegExpParser parser, int startIndex)
            {
                Debug.Assert(cp is >= 0 and <= UnicodeHelper.LastCodePoint, "Invalid end code point.");

                var sb = context.StringBuilder;

                if (context.SetRangeStart >= 0)
                {
                    if (sb is not null)
                    {
                        context.UnicodeSet.Add(new CodePointRange(cp));
                    }

                    context.SetRangeStart = cp;
                }
                else
                {
                    context.SetRangeStart = ~context.SetRangeStart;

                    // Cases like /[z-a]/u or /[\d-a]/u are syntax error.
                    if (context.SetRangeStart > cp)
                    {
                        if (context.SetRangeStart <= UnicodeHelper.LastCodePoint)
                        {
                            parser.ReportSyntaxError(startIndex, RegExpRangeOutOfOrderCharacterClass);
                        }
                        else
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidCharacterClass);
                        }
                    }

                    if (sb is not null)
                    {
                        context.UnicodeSet.LastItemRef() = new CodePointRange(context.SetRangeStart, cp);
                    }

                    context.SetRangeStart = SetRangeNotStarted;
                }
            }

            public bool RewriteSet(ref ParsePatternContext context, ref RegExpParser parser)
            {
                ref readonly var sb = ref context.StringBuilder;
                ref readonly var pattern = ref parser._pattern;

                if (sb is not null)
                {
                    if (context.SetRangeStart < 0)
                    {
                        context.UnicodeSet.Add(new CodePointRange('-'));
                    }

                    CodePointRange.NormalizeRanges(ref context.UnicodeSet);

                    AppendSet(sb, context.UnicodeSet.AsReadOnlySpan(), isInverted: pattern.CharCodeAt(context.SetStartIndex + 1) == '^');

                    context.UnicodeSet = default;
                }

                return true;
            }

            private static void AppendSet(StringBuilder sb, ReadOnlySpan<CodePointRange> normalizedSet, bool isInverted)
            {
                // 0. Handle edge cases

                switch (
                    normalizedSet.Length == 0 ? isInverted :
                    normalizedSet.Length == 1 && normalizedSet[0].Start == 0 && normalizedSet[0].End == UnicodeHelper.LastCodePoint ? !isInverted :
                    (bool?)null)
                {
                    case false:
                        sb.Append(MatchNoneRegex);
                        return;
                    case true:
                        sb.Append("(?:").Append(MatchSurrogatePairRegex)
                            .Append('|').Append(MatchLoneSurrogateRegex)
                            .Append('|').Append(MatchAnyButSurrogateRegex)
                            .Append(')');
                        return;
                }

                // 1. Split set into BMP and astral parts

                int i;
                for (i = 0; i < normalizedSet.Length; i++)
                {
                    ref readonly var range = ref normalizedSet[i];
                    if (range.Start > char.MaxValue)
                    {
                        break;
                    }
                    else if (range.End > char.MaxValue)
                    {
                        i++;
                        break;
                    }
                }

                Span<CodePointRange> rangeSpan;
                ArrayList<CodePointRange> bmpRanges;
                if (i != 0)
                {
                    bmpRanges = new ArrayList<CodePointRange>(new CodePointRange[i]);
                    rangeSpan = bmpRanges.AsSpan();
                    normalizedSet.Slice(0, rangeSpan.Length).CopyTo(rangeSpan);

                    ref var range = ref rangeSpan.Last();
                    if (range.End > char.MaxValue)
                    {
                        range = new CodePointRange(range.Start, char.MaxValue);
                        i--;
                    }
                }
                else
                {
                    bmpRanges = default;
                }

                ArrayList<CodePointRange> astralRanges;
                if (i != normalizedSet.Length)
                {
                    astralRanges = new ArrayList<CodePointRange>(new CodePointRange[normalizedSet.Length - i]);
                    rangeSpan = astralRanges.AsSpan();
                    normalizedSet.Slice(i, rangeSpan.Length).CopyTo(rangeSpan);

                    ref var range = ref rangeSpan[0];
                    if (range.Start <= char.MaxValue)
                    {
                        range = new CodePointRange(char.MaxValue + 1, range.End);
                    }
                }
                else
                {
                    astralRanges = default;
                }

                if (isInverted)
                {
                    astralRanges = CodePointRange.InvertRanges(astralRanges.AsReadOnlySpan(), start: char.MaxValue + 1);
                }

                // 3. Lone surrogates need special care: we need to handle ranges which contains surrogates separately

                ArrayList<CodePointRange> loneHighSurrogateRanges = default;
                ArrayList<CodePointRange> loneLowSurrogateRanges = default;
                for (i = 0; i < bmpRanges.Count; i++)
                {
                    var range = bmpRanges[i];

                    if (range.End < 0xD800)
                    {
                        continue;
                    }

                    if (range.Start > 0xDFFF)
                    {
                        break;
                    }

                    if (range.End > 0xDFFF)
                    {
                        if (range.Start < 0xD800)
                        {
                            bmpRanges[i] = new CodePointRange(range.Start, 0xD800 - 1);
                            bmpRanges.Insert(++i, new CodePointRange(0xDFFF + 1, range.End));
                            range = new CodePointRange(0xD800, 0xDFFF);
                        }
                        else
                        {
                            bmpRanges[i] = new CodePointRange(0xDFFF + 1, range.End);
                            range = new CodePointRange(range.Start, 0xDFFF);
                        }
                    }
                    else
                    {
                        if (range.Start < 0xD800)
                        {
                            bmpRanges[i] = new CodePointRange(range.Start, 0xD800 - 1);
                            range = new CodePointRange(0xD800, range.End);
                        }
                        else
                        {
                            bmpRanges.RemoveAt(i--);
                        }
                    }

                    if (range.End >= 0xDC00 && range.Start < 0xDC00)
                    {
                        loneHighSurrogateRanges.Add(new CodePointRange(range.Start, 0xDC00 - 1));
                        loneLowSurrogateRanges.Add(new CodePointRange(0xDC00, range.End));
                    }
                    else if (range.Start < 0xDC00)
                    {
                        loneHighSurrogateRanges.Add(range);
                    }
                    else
                    {
                        loneLowSurrogateRanges.Add(range);
                    }
                }

                if (isInverted)
                {
                    bmpRanges.Add(new CodePointRange(0xD800, 0xDFFF));
                    loneHighSurrogateRanges = CodePointRange.InvertRanges(loneHighSurrogateRanges.AsReadOnlySpan(), start: 0xD800, end: 0xDBFF);
                    loneLowSurrogateRanges = CodePointRange.InvertRanges(loneLowSurrogateRanges.AsReadOnlySpan(), start: 0xDC00, end: 0xDFFF);
                }

                // 4. Append ranges

                sb.Append("(?:");

                string? separator = null;

                if (astralRanges.Count > 0)
                {
                    rangeSpan = astralRanges.AsSpan();
                    Span<char> start = stackalloc char[2];
                    Span<char> end = stackalloc char[2];

                    for (i = 0; i < rangeSpan.Length; i++)
                    {
                        sb.Append(separator);
                        separator = "|";

                        ref readonly var range = ref rangeSpan[i];

                        if (range.Start == range.End)
                        {
                            sb.AppendCodePoint(range.Start);
                        }
                        else
                        {
                            Debug.Assert(range.Start <= range.End);

                            UnicodeHelper.GetSurrogatePair((uint)range.Start, out start[0], out start[1]);
                            UnicodeHelper.GetSurrogatePair((uint)range.End, out end[0], out end[1]);

                            if (start[0] == end[0])
                            {
                                sb.Append(start[0]);
                                AppendAstralRange(sb, new CodePointRange(start[1], end[1]));
                            }
                            else
                            {
                                var s1 = start[1] > 0xDC00 ? 1 : 0;
                                var s2 = end[1] < 0xDFFF ? 1 : 0;
                                string? innerSeparator = null;

                                if (s1 != 0)
                                {
                                    sb.Append(start[0]);
                                    AppendAstralRange(sb, new CodePointRange(start[1], 0xDFFF));
                                    innerSeparator = "|";
                                }
                                if (end[0] - start[0] >= s1 + s2)
                                {
                                    sb.Append(innerSeparator);
                                    AppendAstralRange(sb, new CodePointRange(start[0] + s1, end[0] - s2));
                                    AppendAstralRange(sb, new CodePointRange(0xDC00, 0xDFFF));
                                    innerSeparator = "|";
                                }
                                if (s2 != 0)
                                {
                                    sb.Append(innerSeparator);
                                    sb.Append(end[0]);
                                    AppendAstralRange(sb, new CodePointRange(0xDC00, end[1]));
                                }
                            }
                        }
                    }
                }

                if (loneHighSurrogateRanges.Count > 0)
                {
                    sb.Append(separator);
                    separator = "|";

                    sb.Append('[');

                    rangeSpan = loneHighSurrogateRanges.AsSpan();
                    for (i = 0; i < rangeSpan.Length; i++)
                    {
                        AppendBmpRange(sb, rangeSpan[i], s_appendChar);
                    }

                    sb.Append(']').Append("(?![\uDC00-\uDFFF])");
                }

                if (loneLowSurrogateRanges.Count > 0)
                {
                    sb.Append(separator);
                    separator = "|";

                    sb.Append("(?<![\uD800-\uDBFF])").Append('[');

                    rangeSpan = loneLowSurrogateRanges.AsSpan();
                    for (i = 0; i < rangeSpan.Length; i++)
                    {
                        AppendBmpRange(sb, rangeSpan[i], s_appendChar);
                    }

                    sb.Append(']');
                }

                if (bmpRanges.Count > 0)
                {
                    sb.Append(separator);

                    sb.Append('[');

                    if (isInverted)
                    {
                        sb.Append('^');
                    }

                    rangeSpan = bmpRanges.AsSpan();
                    for (i = 0; i < rangeSpan.Length; i++)
                    {
                        AppendBmpRange(sb, rangeSpan[i], s_appendCharSafe);
                    }

                    sb.Append(']');
                }

                sb.Append(')');

                static void AppendRange(StringBuilder sb, CodePointRange range, Action<StringBuilder, char> appender, Action<StringBuilder> onRangeStart, Action<StringBuilder> onRangeEnd)
                {
                    if (range.Start == range.End)
                    {
                        appender(sb, (char)range.Start);
                    }
                    else
                    {
                        onRangeStart(sb);
                        appender(sb, (char)range.Start);
                        if (range.End > range.Start + 1)
                        {
                            sb.Append('-');
                        }
                        appender(sb, (char)range.End);
                        onRangeEnd(sb);
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static void AppendBmpRange(StringBuilder sb, CodePointRange range, Action<StringBuilder, char> appender) => AppendRange(sb, range, appender, static _ => { }, static _ => { });

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static void AppendAstralRange(StringBuilder sb, CodePointRange range) => AppendRange(sb, range, s_appendChar, static sb => sb.Append('['), static sb => sb.Append(']'));
            }

            private static void AddStandardCharClass(ref ArrayList<CodePointRange> set, char ch)
            {
                // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Regular_expressions/Character_classes#types

                ReadOnlySpan<CodePointRange> ranges = ch switch
                {
                    // [0-9]
                    'd' => stackalloc CodePointRange[]
                    {
                        new CodePointRange(0x0030, 0x0039)
                    },
                    // [^0-9]
                    'D' => stackalloc CodePointRange[]
                    {
                        new CodePointRange(0x0000, 0x002F),
                        new CodePointRange(0x003A, UnicodeHelper.LastCodePoint),
                    },
                    // [\f\n\r\t\v\u0020\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]
                    's' => stackalloc CodePointRange[]
                    {
                        new CodePointRange(0x0009, 0x000D),
                        new CodePointRange(0x0020),
                        new CodePointRange(0x00A0),
                        new CodePointRange(0x1680),
                        new CodePointRange(0x2000, 0x200A),
                        new CodePointRange(0x2028, 0x2029),
                        new CodePointRange(0x202F),
                        new CodePointRange(0x205F),
                        new CodePointRange(0x3000),
                        new CodePointRange(0xFEFF),
                    },
                    // [^\f\n\r\t\v\u0020\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]
                    'S' => stackalloc CodePointRange[]
                    {
                        new CodePointRange(0x0000, 0x0008),
                        new CodePointRange(0x000E, 0x001F),
                        new CodePointRange(0x0021, 0x009F),
                        new CodePointRange(0x00A1, 0x167F),
                        new CodePointRange(0x1681, 0x1FFF),
                        new CodePointRange(0x200B, 0x2027),
                        new CodePointRange(0x202A, 0x202E),
                        new CodePointRange(0x2030, 0x205E),
                        new CodePointRange(0x2060, 0x2FFF),
                        new CodePointRange(0x3001, 0xFEFE),
                        new CodePointRange(0xFF00, UnicodeHelper.LastCodePoint),
                    },
                    // [A-Za-z0-9_]
                    'w' => stackalloc CodePointRange[]
                    {
                        new CodePointRange(0x0030, 0x0039),
                        new CodePointRange(0x0041, 0x005A),
                        new CodePointRange(0x005F),
                        new CodePointRange(0x0061, 0x007A),
                    },
                    // [^A-Za-z0-9_]
                    'W' => stackalloc CodePointRange[]
                    {
                        new CodePointRange(0x0000, 0x002F),
                        new CodePointRange(0x003A, 0x0040),
                        new CodePointRange(0x005B, 0x005E),
                        new CodePointRange(0x0060),
                        new CodePointRange(0x007B, UnicodeHelper.LastCodePoint),
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(ch), ch, null)
                };

                set.AddRange(ranges);
            }

            private static bool ValidateUnicodeProperty(ReadOnlyMemory<char> expression, bool translateToRanges, ref RegExpParser parser, out CodePointRange[]? codePointRanges)
            {
                var index = expression.Span.IndexOf('=');
                if (index >= 0)
                {
                    // https://tc39.es/ecma262/#table-nonbinary-unicode-properties

                    var propertyName = expression.Span.Slice(0, index);
                    expression = expression.Slice(index + 1);
                    switch (propertyName)
                    {
                        case "gc" or "General_Category":
                            if (translateToRanges)
                            {
                                codePointRanges = UnicodeProperties.GetGeneralCategoryRange(expression, parser.GetCodePointRangeCache());
                                return codePointRanges is not null;
                            }
                            else
                            {
                                codePointRanges = default;
                                return UnicodeProperties.IsAllowedGeneralCategoryValue(expression);
                            }

                        case "sc" or "Script" or "scx" or "Script_Extensions":
                            // Translating unicode properties other than General Categories is not implemented currently.
                            codePointRanges = default;
                            return UnicodeProperties.IsAllowedScriptValue(expression, parser._tokenizer._options._ecmaVersion);

                        default:
                            codePointRanges = default;
                            return false;
                    }
                }
                else
                {
                    if (translateToRanges)
                    {
                        codePointRanges = UnicodeProperties.GetGeneralCategoryRange(expression, parser.GetCodePointRangeCache());
                        if (codePointRanges is not null)
                        {
                            return true;
                        }
                    }
                    else if (UnicodeProperties.IsAllowedGeneralCategoryValue(expression))
                    {
                        codePointRanges = default;
                        return true;
                    }

                    // Translating unicode properties other than General Categories is not implemented currently.
                    codePointRanges = default;
                    return UnicodeProperties.IsAllowedBinaryValue(expression, parser._tokenizer._options._ecmaVersion);
                }
            }

            public void RewriteDot(ref ParsePatternContext context, bool dotAll)
            {
                ref readonly var sb = ref context.StringBuilder;
                if (sb is not null)
                {
                    // '.' has to be adjusted to also match all surrogate pairs.

                    sb.Append("(?:").Append(MatchSurrogatePairRegex)
                        .Append('|').Append(MatchLoneSurrogateRegex)
                        .Append('|');
                    (dotAll
                        ? sb.Append(MatchAnyButSurrogateRegex)
                        : sb.Append(MatchAnyButNewLineAndSurrogateRegex))
                        .Append(')');
                }
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

            public void HandleInvalidRangeQuantifier(ref ParsePatternContext context, ref RegExpParser parser, int startIndex)
            {
                parser.ReportSyntaxError(startIndex, RegExpIncompleteQuantifier);
            }

            public bool AdjustEscapeSequence(ref ParsePatternContext context, ref RegExpParser parser, out RegExpConversionError? conversionError)
            {
                // https://tc39.es/ecma262/#prod-AtomEscape

                ref readonly var sb = ref context.StringBuilder;
                ref readonly var pattern = ref parser._pattern;
                ref var i = ref context.Index;

                ushort charCode, charCode2;
                int cp;
                var startIndex = i++;
                int endIndex;
                var ch = pattern[i];
                switch (ch)
                {
                    // CharacterEscape -> RegExpUnicodeEscapeSequence -> u{ CodePoint }
                    case 'u' when pattern.CharCodeAt(i + 1) == '{':
                        // Rewrite \u{...} escape sequences as follows:
                        // * /\u{3F}/u --> @"\x3F"
                        // * /\u{FFFF}/u --> "\uFFFF" (+ negative lookahead/lookbehind in the case of lone surrogates)
                        // * /\u{1F4A9}/u --> "\uD83D\uDCA9"

                        if (parser._tokenizer._options._ecmaVersion < EcmaVersion.ES6
                            || !TryReadCodePoint(pattern, ref i, endIndex: pattern.Length, out cp))
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                            cp = default; // keeps the compiler happy
                        }

                        if (!context.WithinSet)
                        {
                            if (sb is not null)
                            {
                                AppendCodePointSafe(sb, cp);
                            }

                            context.ClearFollowingQuantifierError();
                        }
                        else
                        {
                            AddSetCodePoint(ref context, cp, ref parser, startIndex);
                        }
                        break;

                    // CharacterEscape -> RegExpUnicodeEscapeSequence
                    // CharacterEscape -> HexEscapeSequence
                    case 'u':
                    case 'x':
                        if (TryReadHexEscape(pattern, ref i, endIndex: pattern.Length, charCodeLength: ch == 'u' ? 4 : 2, out charCode))
                        {
                            if (ch == 'u' && ((char)charCode).IsHighSurrogate() && i + 2 < pattern.Length && pattern[i + 1] == '\\' && pattern[i + 2] == 'u')
                            {
                                endIndex = i + 2;
                                if (TryReadHexEscape(pattern, ref endIndex, endIndex: pattern.Length, charCodeLength: 4, out charCode2) && ((char)charCode2).IsLowSurrogate())
                                {
                                    cp = (int)UnicodeHelper.GetCodePoint((char)charCode, (char)charCode2);
                                    if (!context.WithinSet)
                                    {
                                        if (sb is not null)
                                        {
                                            AppendCodePointSafe(sb, cp);
                                        }

                                        context.ClearFollowingQuantifierError();
                                    }
                                    else
                                    {
                                        AddSetCodePoint(ref context, cp, ref parser, startIndex);
                                    }

                                    i = endIndex;
                                    break;
                                }
                            }

                            if (!context.WithinSet)
                            {
                                if (sb is not null)
                                {
                                    AppendUnicodeCharSafe(sb, (char)charCode);
                                }

                                context.ClearFollowingQuantifierError();
                            }
                            else
                            {
                                AddSetCodePoint(ref context, (char)charCode, ref parser, startIndex);
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
                        if (i + 1 < pattern.Length)
                        {
                            if (pattern[i + 1].IsBasicLatinLetter())
                            {
                                charCode = (ushort)(char.ToUpperInvariant(pattern[++i]) - '@');

                                if (!context.WithinSet)
                                {
                                    context.AppendCharSafe?.Invoke(sb!, (char)charCode);
                                    context.ClearFollowingQuantifierError();
                                }
                                else
                                {
                                    AddSetCodePoint(ref context, charCode, ref parser, startIndex);
                                }
                                break;
                            }
                        }

                        parser.ReportSyntaxError(startIndex, RegExpInvalidUnicodeEscape);
                        break;

                    // CharacterEscape -> 0 [lookahead ∉ DecimalDigit]
                    case '0':
                        if (!((char)pattern.CharCodeAt(i + 1)).IsDecimalDigit())
                        {
                            if (!context.WithinSet)
                            {
                                context.AppendCharSafe?.Invoke(sb!, '\0');
                                context.ClearFollowingQuantifierError();
                            }
                            else
                            {
                                AddSetCodePoint(ref context, 0, ref parser, startIndex);
                            }
                        }
                        else
                        {
                            if (!context.WithinSet)
                            {
                                parser.ReportSyntaxError(startIndex, RegExpInvalidDecimalEscape);
                            }
                            else
                            {
                                parser.ReportSyntaxError(startIndex, RegExpInvalidClassEscape);
                            }
                        }
                        break;

                    // DecimalEscape
                    case >= '1' and <= '9':
                        if (!context.WithinSet)
                        {
                            // Outside character sets, numbers may be backreferences (in this case the number is interpreted as decimal).
                            if (parser.TryAdjustBackreference(ref context, startIndex, out conversionError))
                            {
                                if (conversionError is not null)
                                {
                                    return false;
                                }

                                context.ClearFollowingQuantifierError();
                                break;
                            }
                        }

                        // When the number is not a backreference, it's a syntax error.
                        if (!context.WithinSet || ch >= '8')
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        }
                        else
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidClassEscape);
                        }
                        break;

                    // 'k' GroupName
                    case 'k':
                        if (!context.WithinSet && parser._tokenizer._options._ecmaVersion >= EcmaVersion.ES9)
                        {
                            parser.AdjustNamedBackreference(ref context, startIndex, out conversionError);
                            if (conversionError is not null)
                            {
                                return false;
                            }

                            context.ClearFollowingQuantifierError();
                        }
                        else
                        {
                            // \k escape sequence before ES2018 or within character sets is not allowed.
                            parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        }
                        break;

                    // CharacterClassEscape
                    case 'd' or 'D' or 's' or 'S' or 'w' or 'W':
                        if (!context.WithinSet)
                        {
                            // RegexOptions.ECMAScript incorrectly interprets \s as [\f\n\r\t\v\u0020]. This doesn't align with the JS specification,
                            // which defines \s as [\f\n\r\t\v\u0020\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]. We need to adjust both \s and \S.
                            // \D and \W also have to be adjusted outside character sets.

                            if (sb is not null)
                            {
                                if (ch is 'D' or 'S' or 'W')
                                {
                                    const string invertedWhiteSpacePattern = "\0-\u0008\u000E-\u001F\\x21-\u009F\u00A1-\u167F\u1681-\u1FFF\u200B-\u2027\u202A-\u202E\u2030-\u205E\u2060-\u2FFF\u3001-\uD7FF\uE000-\uFEFE\uFF00-\uFFFF";
                                    const string invertedDigitPattern = "\0-\\x2F\\x3A-\uD7FF\uE000-\uFFFF";
                                    const string invertedWordCharPattern = "\0-\\x2F\\x3A-\\x40\\x5B-\\x5E\\x60\\x7B-\uD7FF\uE000-\uFFFF";

                                    sb.Append("(?:").Append(MatchSurrogatePairRegex)
                                        .Append('|').Append(MatchLoneSurrogateRegex)
                                        .Append('|').Append('[').Append(ch switch { 'D' => invertedDigitPattern, 'S' => invertedWhiteSpacePattern, _ => invertedWordCharPattern }).Append(']')
                                        .Append(')');
                                }
                                else
                                {
                                    _ = ch == 's'
                                        ? sb.Append('[').Append('\\').Append(ch).Append(AdditionalWhiteSpacePattern).Append(']')
                                        : sb.Append(pattern, startIndex, 2);
                                }
                            }

                            context.ClearFollowingQuantifierError();
                        }
                        else
                        {
                            if (context.SetRangeStart < 0)
                            {
                                parser.ReportSyntaxError(startIndex, RegExpInvalidCharacterClass);
                            }

                            if (sb is not null)
                            {
                                AddStandardCharClass(ref context.UnicodeSet, ch);
                            }

                            context.SetRangeStart = SetRangeStartedWithCharClass;
                        }
                        break;

                    // CharacterClassEscape -> p{ UnicodePropertyValueExpression }
                    // CharacterClassEscape -> P{ UnicodePropertyValueExpression }
                    case 'p' or 'P':
                        if (parser._tokenizer._options._ecmaVersion >= EcmaVersion.ES9)
                        {
                            if (pattern.CharCodeAt(i + 1) == '{')
                            {
                                CodePointRange[]? codePointRanges = null;

                                endIndex = pattern.IndexOf('}', i + 2);
                                if (endIndex < 0
                                    || !ValidateUnicodeProperty(pattern.AsMemory(i + 2, endIndex - (i + 2)), translateToRanges: sb is not null, ref parser, out codePointRanges))
                                {
                                    if (!context.WithinSet)
                                    {
                                        parser.ReportSyntaxError(startIndex, RegExpInvalidPropertyName);
                                    }
                                    else
                                    {
                                        parser.ReportSyntaxError(startIndex, RegExpInvalidClassPropertyName);
                                    }
                                }

                                ReadOnlySpan<CodePointRange> codePointRangeSpan;
                                if (sb is not null)
                                {
                                    // Unicode property escape support are pretty limited in .NET and we can't even use that little bit
                                    // because it only matches characters, not code points (e.g. "\uD80C\uDC00".match(/\p{L}/u) succeeds,
                                    // while Regex.Matches("\uD80C\uDC00", @"\p{L}", RegexOptions.ECMAScript) doesn't!)
                                    // Until .NET catches up, the only thing we can do is to manually translate property expressions to
                                    // code point ranges. However, there are two problems with this:
                                    // 1. The resulting set can be huge (there are > 1M code points...)
                                    // 2. The Unicode data needed for doing this is too big to include in this project.
                                    // So, the best effort we can make ATM is to cook from what .NET provides out of the box,
                                    // which is practically General Categories. There's no easy way to support other expressions for now.

                                    if (codePointRanges is null)
                                    {
                                        conversionError = parser.ReportConversionFailure(startIndex, RegExpInconvertibleUnicodePropertyEscape);
                                        return false;
                                    }

                                    codePointRangeSpan = ch == 'P'
                                        ? CodePointRange.InvertRanges(codePointRanges).AsReadOnlySpan()
                                        : codePointRanges;
                                }
                                else
                                {
                                    codePointRangeSpan = default;
                                }

                                if (!context.WithinSet)
                                {
                                    if (sb is not null)
                                    {
                                        AppendSet(sb, codePointRangeSpan, isInverted: false);
                                    }

                                    context.ClearFollowingQuantifierError();
                                }
                                else
                                {
                                    if (context.SetRangeStart < 0)
                                    {
                                        parser.ReportSyntaxError(startIndex, RegExpInvalidCharacterClass);
                                    }

                                    if (sb is not null)
                                    {
                                        context.UnicodeSet.AddRange(codePointRangeSpan);
                                    }

                                    context.SetRangeStart = SetRangeStartedWithCharClass;
                                }

                                i = endIndex;
                            }
                            else
                            {
                                if (!context.WithinSet)
                                {
                                    parser.ReportSyntaxError(startIndex, RegExpInvalidPropertyName);
                                }
                                else
                                {
                                    parser.ReportSyntaxError(startIndex, RegExpInvalidClassPropertyName);
                                }
                            }
                        }
                        else
                        {
                            // \p and \P escape sequences before ES2018 are not allowed.
                            parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        }
                        break;

                    default:
                        if (!TryGetSimpleEscapeCharCode(ch, context.WithinSet, out charCode))
                        {
                            parser.ReportSyntaxError(startIndex, RegExpInvalidEscape);
                        }

                        if (!context.WithinSet)
                        {
                            sb?.Append(pattern, startIndex, 2);
                            if (ch is 'b' or 'B')
                            {
                                context.SetFollowingQuantifierError(RegExpNothingToRepeat);
                            }
                            else
                            {
                                context.ClearFollowingQuantifierError();
                            }
                        }
                        else
                        {
                            AddSetCodePoint(ref context, charCode, ref parser, startIndex);
                        }
                        break;
                }

                conversionError = null;
                return true;
            }
        }
    }
}
