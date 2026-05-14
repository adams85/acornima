using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Acornima.Helpers;

namespace Acornima;

using static SyntaxErrorMessages;

public partial class Tokenizer
{
    [Flags]
    internal enum RegExpFlags : byte
    {
        None = 0,
        Global = 1 << 0,
        IgnoreCase = 1 << 1,
        Multiline = 1 << 2,
        Unicode = 1 << 3,
        Sticky = 1 << 4,
        DotAll = 1 << 5,
        Indices = 1 << 6,
        UnicodeSets = 1 << 7
    }

    internal sealed partial class RegExpParser : StackGuard.IRecursionDepthProvider
    {
        // Method naming convention:
        // - Parse, Eat: parses the construct and moves index past its last character.
        // - Read, Consume: parses the construct and moves index to its last character.

        private const int SetRangeNotStarted = int.MaxValue;
        private const int SetRangeStartedWithCharClass = int.MaxValue - 1;

        private static RegExpFlags ParseFlags(string value, int startIndex, Tokenizer tokenizer)
        {
            var flags = RegExpFlags.None;

            var ecmaVersion = tokenizer._options._ecmaVersion;

            for (var i = 0; i < value.Length; i++)
            {
                var flag = value[i] switch
                {
                    'g' => RegExpFlags.Global,
                    'i' => RegExpFlags.IgnoreCase,
                    'm' => RegExpFlags.Multiline,
                    'u' when ecmaVersion >= EcmaVersion.ES6 => RegExpFlags.Unicode,
                    'y' when ecmaVersion >= EcmaVersion.ES6 => RegExpFlags.Sticky,
                    's' when ecmaVersion >= EcmaVersion.ES9 => RegExpFlags.DotAll,
                    'd' when ecmaVersion >= EcmaVersion.ES13 => RegExpFlags.Indices,
                    'v' when ecmaVersion >= EcmaVersion.ES15 => RegExpFlags.UnicodeSets,
                    _ => RegExpFlags.None
                };

                if (flag == RegExpFlags.None || (flags & flag) != 0)
                {
                    // unknown or already set
                    tokenizer.Raise(startIndex, InvalidRegExpFlags);
                }
                flags |= flag;
            }

            if ((flags & RegExpFlags.UnicodeSets) != 0 && (flags & RegExpFlags.Unicode) != 0)
            {
                // cannot have them both
                tokenizer.Raise(startIndex, InvalidRegExpFlags);
            }

            return flags;
        }

        private readonly Tokenizer _tokenizer;
        private readonly StackGuard.IRecursionDepthProvider _recursionDepthProvider;

        internal string _pattern;
        internal int _patternStartIndex;
        internal string _flagsOriginal;
        internal int _flagsStartIndex;
        private RegExpFlags _flags;

        internal RegExpParser(Tokenizer tokenizer)
        {
            _tokenizer = tokenizer;
            _recursionDepthProvider = tokenizer._recursionDepthProvider ?? this;

            _pattern = null!;
            _flagsOriginal = null!;
        }

        internal void Reset(string pattern, int patternStartIndex, string flags, int flagsStartIndex)
        {
            // _flags is reset by Validate()

            _pattern = pattern;
            _patternStartIndex = patternStartIndex;
            _flagsOriginal = flags;
            _flagsStartIndex = flagsStartIndex;
        }

        internal ParseError ReportRecoverableError(int index, string message, ParseError.Factory errorFactory,
            [CallerArgumentExpression(nameof(message))] string code = UnknownError)
        {
            return _tokenizer.RaiseRecoverable(_patternStartIndex + index, message, errorFactory, code);
        }

        [DoesNotReturn]
        internal void ReportSyntaxError(int index, string messageFormat,
            [CallerArgumentExpression(nameof(messageFormat))] string code = UnknownError)
        {
            _tokenizer.Raise(_patternStartIndex + index, string.Format(null, messageFormat, _pattern, _flagsOriginal), code: code);
        }

        public void Validate()
        {
            _flags = ParseFlags(_flagsOriginal, _flagsStartIndex, _tokenizer);

            _tokenizer.AcquireStringBuilder(out _auxiliaryStringBuilder);
            try
            {
                ResetParseContext();

                if ((_flags & RegExpFlags.UnicodeSets) != 0)
                {
                    ParsePattern(UnicodeSetsMode.Instance);
                }
                else if ((_flags & RegExpFlags.Unicode) != 0)
                {
                    ParsePattern(UnicodeMode.Instance);
                }
                else
                {
                    ParsePattern(LegacyMode.Instance);
                }
            }
            finally { _tokenizer.ReleaseStringBuilder(ref _auxiliaryStringBuilder); }
        }

        private void ParsePattern<TMode>(TMode mode)
            where TMode : IMode
        {
            ref var i = ref _index;
            int min, max;

            var isQuantifiable = true;

            Debug.Assert(i == 0);
            while ((uint)i < (uint)_pattern.Length)
            {
                var startIndex = i;
                var ch = _pattern[i++];
                switch (ch)
                {
                    case '[':
                        mode.ParseSet(this, startIndex);
                        break;

                    case '(':
                        var groupType = DetermineGroupType();
                        switch (groupType)
                        {
                            case RegExpGroupType.Capturing:
                                if (!_hasScannedForCapturingGroups)
                                {
                                    _capturingGroupCount++;
                                }
                                goto default;

                            case RegExpGroupType.NamedCapturing:
                                var endIndex = i + 1;
                                var groupName = ReadNormalizedCapturingGroupName(ref endIndex)!;
                                i = endIndex + 1;

                                if (!_hasScannedForCapturingGroups)
                                {
                                    _capturingGroupCount++;
                                }
                                _capturingGroupNames ??= new Dictionary<string, int>();
#if NET6_0_OR_GREATER
                                ref var groupId = ref CollectionsMarshal.GetValueRefOrAddDefault(_capturingGroupNames, groupName, out var entryExists);
                                if (!entryExists || groupId < 0)
                                {
                                    groupId = startIndex; // use the start index of the group as ID
#else
                                if (!_capturingGroupNames.TryGetValue(groupName, out var groupId) || groupId < 0)
                                {
                                    _capturingGroupNames[groupName] = groupId = startIndex; // use the start index of the group as ID
#endif
                                    if (_tokenizer._options.AllowRegExpDuplicateNamedCapturingGroups())
                                    {
#if DEBUG
                                        Debug.Assert(TryAddNamedGroupId(groupId));
#else
                                        TryAddNamedGroupId(groupId);
#endif
                                    }
                                }
                                else if (!_tokenizer._options.AllowRegExpDuplicateNamedCapturingGroups() || !TryAddNamedGroupId(groupId))
                                {
                                    ReportSyntaxError(startIndex + 3, RegExpDuplicateCaptureGroupName);
                                }
                                break;

                            case RegExpGroupType.Modifier:
                                ParseModifierPattern(startIndex);
                                break;

                            case RegExpGroupType.Unknown:
                                ReportSyntaxError(startIndex, RegExpInvalidGroup);
                                break;

                            default:
                                i += (int)groupType >> 2;
                                break;
                        }

                        _groupStack.PushRef().Reset(groupType);
                        continue;

                    case '|':
                        if (_tokenizer._options.AllowRegExpDuplicateNamedCapturingGroups())
                        {
                            BeginAlternateInNamedGroupIds();
                        }
                        continue;

                    case ')':
                        if (_groupStack.Count == 0)
                        {
                            ReportSyntaxError(startIndex, RegExpUnmatchedParen);
                        }

                        if (_tokenizer._options.AllowRegExpDuplicateNamedCapturingGroups())
                        {
                            HoistNamedGroupIdsToParent();
                        }

                        groupType = _groupStack.PopRef().Type;

                        isQuantifiable = mode.AllowsQuantifierAfterGroup(groupType);
                        break;

                    case '^':
                    case '$':
                        continue;

                    case '.':
                        break;

                    case '*' or '+' or '?':
                        ReportSyntaxError(startIndex, RegExpNothingToRepeat);
                        break;

                    case '{':
                        if (TryConsumeRangeQuantifier(startIndex, out min, out max))
                        {
                            ReportSyntaxError(startIndex, RegExpNothingToRepeat);
                        }
                        goto case '}';

                    case '}':
                    case ']':
                        if (mode is not LegacyMode)
                        {
                            ReportSyntaxError(startIndex, RegExpLoneQuantifierBrackets);
                        }
                        goto default;

                    case '\\':
                        if (mode.EatEscapeSequence(this, startIndex))
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }

                    default:
                        mode.EatChar(ch, this);
                        break;
                }

                if ((uint)i >= (uint)_pattern.Length)
                {
                    break;
                }

                startIndex = i;
                ch = _pattern[i];
                switch (ch)
                {
                    case '*' or '+' or '?':
                        i++;
                        break;

                    case '{':
                        i++;
                        if (TryConsumeRangeQuantifier(startIndex, out min, out max))
                        {
                            if (max >= 0 && min > max)
                            {
                                ReportSyntaxError(startIndex, RegExpRangeOutOfOrder);
                            }

                            i++;
                        }
                        else if (mode is LegacyMode)
                        {
                            // In legacy mode, invalid {} quantifiers like /.{/, /.{}/, /.{-1}/, etc. are ignored.
                            i = startIndex;
                            goto default;
                        }
                        else
                        {
                            ReportSyntaxError(startIndex, RegExpIncompleteQuantifier);
                        }
                        break;

                    default:
                        isQuantifiable = true;
                        continue;
                }

                if (!isQuantifiable)
                {
                    ReportSyntaxError(startIndex, RegExpInvalidQuantifier);
                }

                if ((uint)i >= (uint)_pattern.Length)
                {
                    break;
                }

                if (_pattern[i] == '?')
                {
                    i++;
                }
            }

            if (_groupStack.Count > 0)
            {
                ReportSyntaxError(i, RegExpUnterminatedGroup);
            }
        }

        private void ParseSetDefault<TMode>(TMode mode, int startIndex)
            where TMode : IMode
        {
            ref var i = ref _index;

            _setStartIndex = startIndex;
            _setRangeStart = SetRangeNotStarted;

            if (_pattern.CharCodeAt(i) == '^')
            {
                i++;
            }

            while ((uint)i < (uint)_pattern.Length)
            {
                startIndex = i;
                var ch = _pattern[i++];
                switch (ch)
                {
                    case ']':
                        _setStartIndex = -1;
                        return;

                    case '-':
                        if (_setRangeStart is >= 0 and not SetRangeNotStarted)
                        {
                            // We use bitwise complement to indicate that '-' was encountered after a character (or character class like \d or \p{...}).
                            _setRangeStart = ~_setRangeStart;
                        }
                        else
                        {
                            // We encountered a case like /[-]/, /[0-9-]/, /[0-/d-]/, /[/d-0-]/ or /[\0--]/
                            mode.EatSetChar(ch, this, startIndex);
                        }
                        break;

                    case '\\':
                        mode.EatEscapeSequence(this, startIndex);
                        break;

                    default:
                        mode.EatSetChar(ch, this, startIndex);
                        break;
                }
            }

            ReportSyntaxError(i, RegExpUnterminatedCharacterClass);
        }

        // In order to know whether an escape is a backreference or not, we have to scan
        // the entire regexp and find the number of capturing parentheses. However we
        // don't want to scan the regexp twice unless it is necessary. This mini-parser
        // is called when needed. It can see the difference between capturing and
        // non-capturing parentheses and can skip character sets and backslash-escaped
        // characters.
        //
        // Important: The scanner has to be in a consistent state when calling
        // ScanForCapturingGroups, e.g. not in the middle of an escape sequence '\[' or while
        // parsing a nested character set.
        private void ScanForCapturingGroups(int i)
        {
            // Based on: https://github.com/v8/v8/blob/14.8.3/src/regexp/regexp-parser.cc#L1437

            Debug.Assert(!_hasScannedForCapturingGroups);
            _hasScannedForCapturingGroups = true;

            // Potential problematic constructs:
            // * Escaped opening/closing brackets (\(, \), \[, \], \{, \}, \<, \>) --> These are handled (see below).
            // * ?<Name> and \k<Name> --> Shouldn't be an actual problem as opening/closing brackets are not allowed to occur in capturing group names.
            // * \p{...} --> Might be problematic as, in theory, property values can contain special chars (see https://unicode.org/reports/tr18/#property_syntax),
            //   however it seems that currently no such value is defined (see https://unicode.org/Public/UCD/latest/ucd/PropertyValueAliases.txt),
            //   so we can ignore this for now.

            // When starting within a character set, skip everything within the set.
            if (WithinSet)
            {
                // \k is always invalid within a character set in unicode mode, thus ScanForCapturingGroups
                // should never be called within a set.
                Debug.Assert((_flags & (RegExpFlags.Unicode | RegExpFlags.UnicodeSets)) == 0);

                while ((uint)i < (uint)_pattern.Length)
                {
                    switch (_pattern[i++])
                    {
                        case '\\':
                            i++;
                            break;
                        case ']':
                            goto EndOfLoop;
                    }
                }

            EndOfLoop:;
            }

            var isUnicodeSets = (_flags & RegExpFlags.UnicodeSets) != 0;

            // Add count of captures after this position.
            while ((uint)i < (uint)_pattern.Length)
            {
                switch (_pattern[i++])
                {
                    case '\\':
                        i++;
                        break;

                    case '[':
                        var setDepth = 0;
                        while ((uint)i < (uint)_pattern.Length)
                        {
                            switch (_pattern[i++])
                            {
                                case '\\':
                                    i++;
                                    break;
                                case '[':
                                    // For flag 'v', '[' inside a class is treated as a nested class.
                                    // Otherwise, '[' is a normal character.
                                    if (isUnicodeSets)
                                    {
                                        setDepth++;
                                    }
                                    break;
                                case ']':
                                    if (setDepth > 0)
                                    {
                                        setDepth--;
                                        break;
                                    }
                                    goto EndOfLoop;
                            }
                        }

                    EndOfLoop:
                        break;

                    case '(':
                        if (_pattern.CharCodeAt(i) != '?')
                        {
                            _capturingGroupCount++;
                        }
                        else
                        {
                            i++;
                            if (_tokenizer._options._ecmaVersion >= EcmaVersion.ES9
                                && _pattern.CharCodeAt(i) == '<' && _pattern.CharCodeAt(i + 1) is not ('=' or '!'))
                            {
                                var endIndex = i;
                                var groupName = ReadNormalizedCapturingGroupName(ref endIndex, throwOnError: false);
                                if (groupName is null)
                                {
                                    i++;
                                    break;
                                }
                                i = endIndex + 1;

                                _capturingGroupCount++;
                                _capturingGroupNames ??= new Dictionary<string, int>();
#if NET6_0_OR_GREATER
                                ref var groupId = ref CollectionsMarshal.GetValueRefOrAddDefault(_capturingGroupNames, groupName, out var entryExists);
                                if (!entryExists)
                                {
                                    groupId = -1;
#else
                                if (!_capturingGroupNames.TryGetValue(groupName, out var groupId))
                                {
                                    _capturingGroupNames.Add(groupName, -1);
#endif
                                }
                            }
                        }
                        break;
                }
            }
        }

        private static bool TryReadHexEscape(string pattern, ref int i, int endIndex, int charCodeLength, out ushort charCode)
        {
            if (i + charCodeLength < endIndex)
            {
                if (ushort.TryParse(pattern.AsSpan(i + 1, charCodeLength).ToParsable(), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out charCode))
                {
                    i += charCodeLength;
                    return true;
                }
            }

            charCode = default;
            return false;
        }

        private static bool TryReadCodePoint(string pattern, ref int i, int endIndex, out int cp)
        {
            var valueStartIndex = i + 2;

            var escapeEndIndex = pattern.IndexOf('}', valueStartIndex, endIndex - valueStartIndex);
            if (escapeEndIndex < 0)
            {
                cp = default;
                return false;
            }

            var slice = pattern.AsSpan(valueStartIndex, escapeEndIndex - valueStartIndex);
            if (!int.TryParse(slice.ToParsable(), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out cp)
                // NOTE: int.TryParse with NumberStyles.AllowHexSpecifier may return a negative number (e.g. '80000000' -> -2147483648)!
                || (uint)cp > UnicodeHelper.LastCodePoint)
            {
                cp = default;
                return false;
            }

            i = escapeEndIndex;
            return true;
        }

        private static bool TryGetSimpleEscapeCharCode(char ch, bool withinSet, out ushort charCode)
        {
            switch (ch)
            {
                // ClassSetCharacter -> \b
                case 'b':
                    Debug.Assert(withinSet);
                    charCode = '\b';
                    return true;

                // CharacterEscape -> ControlEscape
                case 'f': charCode = '\f'; return true;
                case 'n': charCode = '\n'; return true;
                case 'r': charCode = '\r'; return true;
                case 't': charCode = '\t'; return true;
                case 'v': charCode = '\v'; return true;

                // CharacterEscape -> IdentityEscape -> '/'
                case '/':

                // CharacterEscape -> IdentityEscape -> SyntaxCharacter
                case '^' or '$' or '\\' or '.' or '*' or '+' or '?' or '(' or ')' or '[' or ']' or '{' or '}' or '|':
                    charCode = ch;
                    return true;

                // ClassAtom -> -
                case '-':
                    charCode = ch;
                    // '-' is not a SyntaxCharacter by definition but may need to be escaped in character sets.
                    // However, outside sets, it is not allowed to be escaped in unicode mode.
                    return withinSet;
            }

            charCode = default;
            return false;
        }

        private bool TryConsumeRangeQuantifier(int startIndex, out int min, out int max)
        {
            ref var i = ref _index;

            var endIndex = _pattern.IndexOf('}', i);
            if (endIndex < 0 || endIndex == i)
            {
                min = max = default;
                return false;
            }

            var index = _pattern.IndexOf(',', i, endIndex - i);
            if (index < 0)
            {
                index = endIndex;
            }

            var slice = _pattern.AsSpan(i, index - i);
            if (!int.TryParse(slice.ToParsable(), NumberStyles.None, CultureInfo.InvariantCulture, out min))
            {
                if (slice.Length == 0 || slice.FindIndex(ch => !ch.IsDecimalDigit()) >= 0)
                {
                    min = max = default;
                    return false;
                }
                min = -1;
            }

            if (index == endIndex)
            {
                max = min;
            }
            else if (index == endIndex - 1)
            {
                max = int.MaxValue;
            }
            else
            {
                index++;
                slice = _pattern.AsSpan(index, endIndex - index);
                if (!int.TryParse(slice.ToParsable(), NumberStyles.None, CultureInfo.InvariantCulture, out max))
                {
                    if (slice.FindIndex(ch => !ch.IsDecimalDigit()) >= 0)
                    {
                        min = max = default;
                        return false;
                    }
                    max = -1;
                }
            }

            i = endIndex;
            return true;
        }

        private RegExpGroupType DetermineGroupType()
        {
            var i = _index;

            if (_pattern.CharCodeAt(i) != '?')
            {
                return RegExpGroupType.Capturing;
            }

            return _pattern.CharCodeAt(++i) switch
            {
                ':' => RegExpGroupType.NonCapturing,
                '=' => RegExpGroupType.LookaheadAssertion,
                '!' => RegExpGroupType.NegativeLookaheadAssertion,
                '<' when _tokenizer._options._ecmaVersion >= EcmaVersion.ES9 => _pattern.CharCodeAt(++i) switch
                {
                    '=' => RegExpGroupType.LookbehindAssertion,
                    '!' => RegExpGroupType.NegativeLookbehindAssertion,
                    _ => RegExpGroupType.NamedCapturing,
                },
                'i' or 'm' or 's' or '-' when _tokenizer._options.AllowRegExpModifiers() => RegExpGroupType.Modifier,
                _ => RegExpGroupType.Unknown
            };
        }

        private void ParseModifierPattern(int startIndex)
        {
            RegExpFlags flag, flagsToAdd = RegExpFlags.None, flagsToRemove = RegExpFlags.None;
            ref var i = ref _index;

            for (i++; ;)
            {
                switch (_pattern.CharCodeAt(i++))
                {
                    case 'i': flag = RegExpFlags.IgnoreCase; break;
                    case 'm': flag = RegExpFlags.Multiline; break;
                    case 's': flag = RegExpFlags.DotAll; break;
                    case '-': goto ParseFlagsToRemove;
                    case ':':
                        Debug.Assert(flagsToAdd != 0);
                        return;
                    default:
                        ReportSyntaxError(startIndex, RegExpInvalidGroup);
                        return;
                }

                if ((flagsToAdd & flag) != 0) // duplicate
                {
                    ReportSyntaxError(i - 1, RegExpRepeatedFlag);
                }

                flagsToAdd |= flag;
            }

        ParseFlagsToRemove:
            for (; ; )
            {
                switch (_pattern.CharCodeAt(i++))
                {
                    case 'i': flag = RegExpFlags.IgnoreCase; break;
                    case 'm': flag = RegExpFlags.Multiline; break;
                    case 's': flag = RegExpFlags.DotAll; break;
                    case '-':
                        ReportSyntaxError(i - 1, RegExpMultipleFlagDashes);
                        return;
                    case ':':
                        if ((flagsToAdd | flagsToRemove) == 0) // edge case of /(?-:)/
                        {
                            ReportSyntaxError(i - 2, RegExpInvalidFlagGroup);
                        }
                        return;
                    default:
                        ReportSyntaxError(startIndex, RegExpInvalidGroup);
                        return;
                }

                if ((flagsToRemove & flag) != 0  // duplicate
                    || (flagsToAdd & flag) != 0) // same in add and remove group
                {
                    ReportSyntaxError(i - 1, RegExpRepeatedFlag);
                }

                flagsToRemove |= flag;
            }
        }

        private string? ReadNormalizedCapturingGroupName(ref int i, bool throwOnError = true)
        {
            var nameStartIndex = i + 1;
            var endIndex = _pattern.IndexOf('>', nameStartIndex);
            if (endIndex >= 0)
            {
                var cp = _pattern.CodePointAt(i = nameStartIndex, endIndex);
                var allowAstral = _tokenizer._options.EcmaVersion >= EcmaVersion.ES11 || (_flags & (RegExpFlags.Unicode | RegExpFlags.UnicodeSets)) != 0;
                if (IsIdentifierStart(cp, allowAstral) || cp == '\\')
                {
                    var groupName = ReadIdentifier(ref i, endIndex, allowAstral, throwOnError);
                    if (i == endIndex)
                    {
                        return DeduplicateString(groupName, ref _tokenizer._stringPool);
                    }
                }
            }

            if (throwOnError)
            {
                ReportSyntaxError(nameStartIndex, RegExpInvalidCaptureGroupName);
            }

            return null;
        }

        private ReadOnlySpan<char> ReadIdentifier(ref int i, int endIndex, bool allowAstral, bool throwOnError = true)
        {
            var containsEscape = false;
            var sb = _auxiliaryStringBuilder!.Clear();
            var first = true;
            var chunkStart = i;

            for (int cp; (cp = _pattern.CodePointAt(i, endIndex)) >= 0;)
            {
                if (IsIdentifierChar(cp, allowAstral))
                {
                    i += UnicodeHelper.GetCodePointLength((uint)cp);
                }
                else if (cp == '\\')
                {
                    containsEscape = true;
                    sb.Append(_pattern, chunkStart, i - chunkStart);
                    var escStart = i++;
                    if (_pattern.CharCodeAt(i, endIndex) != 'u')
                    {
                        i = -1;
                        return default;
                    }

                    if (_pattern.CharCodeAt(i + 1, endIndex) == '{')
                    {
                        if (!allowAstral)
                        {
                            i = -1;
                            return default;
                        }
                        else if (!TryReadCodePoint(_pattern, ref i, endIndex, out cp))
                        {
                            if (throwOnError)
                            {
                                ReportSyntaxError(escStart, RegExpInvalidUnicodeEscape);
                            }

                            i = -1;
                            return default;
                        }

                        i++;
                    }
                    else
                    {
                        if (!TryReadHexEscape(_pattern, ref i, endIndex, charCodeLength: 4, out var ch))
                        {
                            if (throwOnError)
                            {
                                ReportSyntaxError(escStart, RegExpInvalidUnicodeEscape);
                            }

                            i = -1;
                            return default;
                        }

                        i++;
                        cp = ch;

                        if (allowAstral && ((char)ch).IsHighSurrogate() && i + 1 < endIndex && _pattern[i] == '\\' && _pattern[i + 1] == 'u')
                        {
                            escStart = i++;
                            if (TryReadHexEscape(_pattern, ref i, endIndex, charCodeLength: 4, out var ch2) && ((char)ch2).IsLowSurrogate())
                            {
                                i++;
                                cp = (int)UnicodeHelper.GetCodePoint((char)ch, (char)ch2);
                            }
                            else
                            {
                                i = escStart;
                            }
                        }
                    }

                    if (first
                        ? !IsIdentifierStart(cp, allowAstral)
                        : !IsIdentifierChar(cp, allowAstral))
                    {
                        i = -1;
                        return default;
                    }

                    sb.AppendCodePoint(cp);
                    chunkStart = i;
                }
                else
                {
                    break;
                }

                first = false;
            }

            return !containsEscape
                ? _pattern.SliceBetween(chunkStart, i)
                : sb.Append(_pattern, chunkStart, i - chunkStart).ToString().AsSpan();
        }

        private bool TryConsumeBackreference(int startIndex)
        {
            ref var i = ref _index;

            var endIndex = _pattern.AsSpan().FindIndex(ch => !ch.IsDecimalDigit(), startIndex: i + 1);
            if (endIndex < 0)
            {
                endIndex = _pattern.Length;
            }

            var slice = _pattern.AsSpan(i, endIndex - i);
            var number = int.Parse(slice.ToParsable(), NumberStyles.None, CultureInfo.InvariantCulture);

            if (number <= _capturingGroupCount)
            {
                goto Success;
            }

            if (!_hasScannedForCapturingGroups)
            {
                ScanForCapturingGroups(startIndex);
                if (number <= _capturingGroupCount)
                {
                    goto Success;
                }
            }

            return false;

        Success:
            i = endIndex - 1;
            return true;
        }

        private void ConsumeNamedBackreference(int startIndex)
        {
            ref var i = ref _index;
            var endIndex = i + 1;

            // 'k' GroupName
            if ((uint)endIndex < (uint)_pattern.Length && _pattern[endIndex] == '<')
            {
                var groupName = ReadNormalizedCapturingGroupName(ref endIndex)!;

                if (_capturingGroupNames is not null && _capturingGroupNames.ContainsKey(groupName))
                {
                    goto Success;
                }

                if (!_hasScannedForCapturingGroups)
                {
                    ScanForCapturingGroups(startIndex);
                    if (_capturingGroupNames is not null && _capturingGroupNames.ContainsKey(groupName))
                    {
                        goto Success;
                    }
                }

                ReportSyntaxError(startIndex + 3, RegExpInvalidNamedCaptureReference);
            }
            else
            {
                ReportSyntaxError(startIndex, RegExpInvalidNamedReference);
            }

        Success:
            i = endIndex;
        }

        #region Context for ParsePattern

        private int _index;

        private StringBuilder? _auxiliaryStringBuilder;

        private bool _hasScannedForCapturingGroups;

        // If _hasScannedForCapturingGroups is false, the number of capturing groups encountered so far
        // (will be increased when the opening bracket of a capturing group is found).
        // Otherwise, the total number of capturing groups in the pattern.
        private int _capturingGroupCount;

        // If _hasScannedForCapturingGroups is false, the names and IDs of named capturing groups encountered so far
        // (will be updated when the opening bracket of a named capturing group is found).
        // Otherwise, the names of all named capturing groups in the pattern (negative IDs indicate lookahead).
        private Dictionary<string, int>? _capturingGroupNames;

        // Group type needs to be stored until ')' is found so we can determine whether or not the group is quantifiable.
        // Also, since ES2025, we need to do some extra bookkeeping as group names don't need to be unique in the pattern
        // but still have to be unique in an alternate part of a group.
        private ArrayList<RegExpGroup> _groupStack;

        private ArrayList<int> _rootNamedGroupIds;

        // The start index of a character set (e.g. /[a-z]/). Negative values indicate that the parser is not within a character set currently.
        private int _setStartIndex;

        private bool WithinSet { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _setStartIndex >= 0; }

        // A variable which keeps track of ranges in character sets and encodes multiple pieces of information related to this:
        // Basically, it stores the starting code point of a potential range. However, it can also have the following special values:
        // * SetRangeNotStarted - Indicates that a range hasn't started yet (i.e. we're at the start of the set or right after a range).
        // * SetRangeStartedWithCharClass - Indicates that a potentially invalid range has started with a character class like \d or \p{...} (e.g. /[\d-A]/).
        // May store the bitwise complement of the possible values listed above, which indicates that the range indicator '-' has been encountered.
        private int _setRangeStart;

        private int _recursionDepth;

        ref int StackGuard.IRecursionDepthProvider.CurrentDepth => ref _recursionDepth;

        private void ResetParseContext()
        {
            // _auxiliaryStringBuilder and _setRangeStart are reset externally.

            _index = 0;

            _hasScannedForCapturingGroups = false;
            _capturingGroupCount = 0;
            _capturingGroupNames = null;

            _groupStack.Clear();

            _rootNamedGroupIds.Clear();

            _setStartIndex = -1;

            _recursionDepth = 0;
        }

        internal void ReleaseReferencesAndLargeBuffers()
        {
            _pattern = null!;
            _flagsOriginal = null!;

            _capturingGroupNames = null;

            _groupStack.Yield(out var groupStackItems, out _);
            if (groupStackItems is not null)
            {
                if (groupStackItems.Length > 64)
                {
                    new ArrayList<RegExpGroup>(groupStackItems, count: 0) { Capacity = 64 }
                        .Yield(out groupStackItems, out _);
                }

                for (var i = 0; i < groupStackItems!.Length; i++)
                {
                    ref var namedGroupIds = ref groupStackItems[i].NamedGroupIds;
                    namedGroupIds.Clear();
                    if (namedGroupIds.Capacity > 16)
                    {
                        namedGroupIds.Capacity = 16;
                    }
                }

                _groupStack = new ArrayList<RegExpGroup>(groupStackItems, count: 0);
            }

            _rootNamedGroupIds.Clear();
            if (_rootNamedGroupIds.Capacity > 64)
            {
                _rootNamedGroupIds.Capacity = 64;
            }
        }

        private bool TryAddNamedGroupId(int id)
        {
            ref var currentNamedGroupIds = ref _rootNamedGroupIds;
            var currentNamedGroupOffset = 0;

            var index = currentNamedGroupIds.AsReadOnlySpan().BinarySearch(id);
            if (index >= 0)
            {
                return false;
            }

            for (var i = 0; i < _groupStack.Count; i++)
            {
                ref var group = ref _groupStack.GetItemRef(i);
                currentNamedGroupIds = ref group.NamedGroupIds;
                currentNamedGroupOffset = group.NamedGroupOffset;

                index = currentNamedGroupIds.AsReadOnlySpan(currentNamedGroupOffset).BinarySearch(id);
                if (index >= 0)
                {
                    return false;
                }
            }

            currentNamedGroupIds.Insert(currentNamedGroupOffset + ~index, id);
            return true;
        }

        private void BeginAlternateInNamedGroupIds()
        {
            if (_groupStack.Count == 0)
            {
                _rootNamedGroupIds.Clear();
            }
            else
            {
                ref var group = ref _groupStack.PeekRef();
                group.NamedGroupOffset = group.NamedGroupIds.Count;
            }
        }

        private void HoistNamedGroupIdsToParent()
        {
            ref readonly var currentNamedGroupIds = ref _groupStack.PeekRef().NamedGroupIds;
            if (currentNamedGroupIds.Count > 0)
            {
                ref var parentNamedGroupIds = ref Unsafe.NullRef<ArrayList<int>>();
                int parentNamedGroupOffset;
                if (_groupStack.Count == 1)
                {
                    parentNamedGroupIds = ref _rootNamedGroupIds;
                    parentNamedGroupOffset = 0;
                }
                else
                {
                    ref var group = ref _groupStack.GetItemRef(_groupStack.Count - 2);
                    parentNamedGroupIds = ref group.NamedGroupIds;
                    parentNamedGroupOffset = group.NamedGroupOffset;
                }

                var parentNamedGroupIdsInCurrentAlternate = parentNamedGroupIds.AsReadOnlySpan(parentNamedGroupOffset);
                for (var i = 0; i < currentNamedGroupIds.Count; i++)
                {
                    var id = currentNamedGroupIds[i];
                    var index = parentNamedGroupIdsInCurrentAlternate.BinarySearch(id);
                    if (index < 0)
                    {
                        parentNamedGroupIds.Insert(parentNamedGroupOffset + ~index, id);
                        parentNamedGroupIdsInCurrentAlternate = parentNamedGroupIds.AsReadOnlySpan(parentNamedGroupOffset);
                    }
                }
            }
        }

        #endregion

        private interface IMode
        {
            void EatChar(char ch, RegExpParser parser);

            void EatSetChar(char ch, RegExpParser parser, int startIndex);

            bool EatEscapeSequence(RegExpParser parser, int startIndex);

            void ParseSet(RegExpParser parser, int startIndex);

            bool AllowsQuantifierAfterGroup(RegExpGroupType groupType);
        }

        // Enum values encodes the length of the group prefix so that (value / 4) is equal to prefix length.
        private enum RegExpGroupType : byte
        {
            Unknown,
            Capturing = 0 * 4 + 1, // (x)
            NamedCapturing = 2 * 4 + 0, // (?<Name>x)
            NonCapturing = 2 * 4 + 1, // (?:x)
            LookaheadAssertion = 2 * 4 + 2, // x(?=y)
            NegativeLookaheadAssertion = 2 * 4 + 3, // x(?!y)
            LookbehindAssertion = 3 * 4 + 0, // (?<=y)x
            NegativeLookbehindAssertion = 3 * 4 + 1, // (?<!y)x
            Modifier = byte.MaxValue, // (?ims-ims:x) — prefix length varies, handled specially
        }

        private struct RegExpGroup
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset(RegExpGroupType type)
            {
                Type = type;
                NamedGroupOffset = 0;
                NamedGroupIds.Clear();
            }

            public RegExpGroupType Type;

            // An index into NamedGroupIds pointing to the the first named group within the current alternate.
            public int NamedGroupOffset;

            // IDs of named groups within the current group, in the order of alternates and in ascending order per alternate.
            public ArrayList<int> NamedGroupIds;
        }
    }
}
