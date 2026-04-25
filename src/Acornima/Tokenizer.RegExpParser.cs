using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
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
            ParseCore(out _);
        }

        private void ParseCore(out ArrayList<RegExpCapturingGroup> capturingGroups)
        {
            // TODO: return capturingGroups?

            _flags = ParseFlags(_flagsOriginal, _flagsStartIndex, _tokenizer);

            _tokenizer.AcquireStringBuilder(out var sb);
            try
            {
                _auxiliaryStringBuilder = sb;

                CheckBracesBalance();

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

                capturingGroups = _capturingGroups;
            }
            finally
            {
                _tokenizer.ReleaseStringBuilder(ref sb);
                _auxiliaryStringBuilder = null;
            }
        }

        private void CheckBracesBalance()
        {
            _capturingGroups = default;
            _capturingGroupNames?.Clear();

            var isUnicodeSets = (_flags & RegExpFlags.UnicodeSets) != 0;
            var isUnicode = isUnicodeSets || (_flags & RegExpFlags.Unicode) != 0;
            var inGroup = 0;
            var inQuantifier = false;
            var setDepth = 0;

            // Potential problematic constructs:
            // * Escaped opening/closing brackets (\(, \), \[, \], \{, \}, \<, \>) --> These are handled (see below).
            // * ?<Name> and \k<Name> --> Shouldn't be an actual problem as opening/closing brackets are not allowed to occur in capturing group names.
            // * \p{...} --> Might be problematic as, in theory, property values can contain special chars (see https://unicode.org/reports/tr18/#property_syntax),
            //   however it seems that currently no such value is defined (see https://unicode.org/Public/UCD/latest/ucd/PropertyValueAliases.txt),
            //   so we can ignore this for now.

            for (var i = 0; i < _pattern.Length; i++)
            {
                var ch = _pattern[i];

                if (ch == '\\')
                {
                    if (i + 1 >= _pattern.Length)
                    {
                        ReportSyntaxError(i, RegExpEscapeAtEndOfPattern);
                    }

                    // Skip escape
                    i++;
                    continue;
                }

                switch (ch)
                {
                    case '(':
                        if (setDepth > 0)
                        {
                            break;
                        }

                        inGroup++;

                        var groupType = DetermineGroupType(i);
                        switch (groupType)
                        {
                            case RegExpGroupType.Capturing:
                                _capturingGroups.Add(new RegExpCapturingGroup(i, name: null));
                                break;

                            case RegExpGroupType.NamedCapturing:
                                var groupStartIndex = i++;
                                var groupName = ReadNormalizedCapturingGroupName(ref i)!;
                                var isDuplicate = !(_capturingGroupNames ??= new HashSet<string>()).Add(groupName);
                                if (isDuplicate && !_tokenizer._options.AllowRegExpDuplicateNamedCapturingGroups())
                                {
                                    ReportSyntaxError(groupStartIndex + 3, RegExpDuplicateCaptureGroupName);
                                }
                                _capturingGroups.Add(new RegExpCapturingGroup(i, groupName));
                                break;

                            case RegExpGroupType.Modifier:
                                ParseModifierPattern(ref i);
                                break;

                            case RegExpGroupType.Unknown:
                                ReportSyntaxError(i, RegExpInvalidGroup);
                                break;
                        }

                        break;

                    case ')':
                        if (setDepth > 0)
                        {
                            break;
                        }

                        if (inGroup == 0)
                        {
                            ReportSyntaxError(i, RegExpUnmatchedParen);
                        }

                        inGroup--;
                        break;

                    case '{':
                        if (setDepth > 0)
                        {
                            break;
                        }

                        if (!inQuantifier)
                        {
                            inQuantifier = true;
                        }
                        else if (isUnicode)
                        {
                            ReportSyntaxError(i, RegExpIncompleteQuantifier);
                        }

                        break;

                    case '}':
                        if (setDepth > 0)
                        {
                            break;
                        }

                        if (inQuantifier)
                        {
                            inQuantifier = false;
                        }
                        else if (isUnicode)
                        {
                            ReportSyntaxError(i, RegExpLoneQuantifierBrackets);
                        }

                        break;

                    case '[':
                        if (setDepth > 0 && !isUnicodeSets)
                        {
                            break;
                        }

                        setDepth++;
                        break;

                    case ']':
                        if (setDepth > 0)
                        {
                            setDepth--;
                        }
                        else if (isUnicode)
                        {
                            ReportSyntaxError(i, RegExpLoneQuantifierBrackets);
                        }

                        break;

                    default:
                        break;
                }
            }

            if (inGroup > 0)
            {
                ReportSyntaxError(_pattern.Length, RegExpUnterminatedGroup);
            }

            if (setDepth > 0)
            {
                ReportSyntaxError(_pattern.Length, RegExpUnterminatedCharacterClass);
            }

            if (isUnicode)
            {
                if (inQuantifier)
                {
                    ReportSyntaxError(_pattern.Length, RegExpLoneQuantifierBrackets);
                }
            }
        }

        private void ParsePattern<TMode>(TMode mode)
            where TMode : IMode
        {
            ref var i = ref _index;
            for (i = 0; i < _pattern.Length; i++)
            {
                var ch = _pattern[i];
                switch (ch)
                {
                    case '[':
                        mode.ParseSet(this);
                        break;

                    case ']':
                        Debug.Assert(mode is LegacyMode, RegExpLoneQuantifierBrackets); // CheckBracesBalance should ensure this.
                        goto default;

                    case '(':
                        var currentGroupAlternate = _capturingGroupNames is { Count: > 0 }
                            ? _groupStack.PeekRef().LastAlternate
                            : null;

                        var groupType = DetermineGroupType(i);
                        switch (groupType)
                        {
                            case RegExpGroupType.Capturing:
                                _capturingGroupCounter++;
                                goto default;

                            case RegExpGroupType.NamedCapturing:
                                var groupName = _capturingGroups[_capturingGroupCounter++].Name;
                                Debug.Assert(groupName is not null);

                                if (!currentGroupAlternate!.TryAddGroupName(groupName!))
                                {
                                    ReportSyntaxError(i + 3, RegExpDuplicateCaptureGroupName);
                                }

                                i = _pattern.IndexOf('>', i + 3);
                                Debug.Assert(i >= 0);

                                _groupStack.PushRef().Reset(groupType, parent: currentGroupAlternate);
                                goto FinishGroupStart;

                            case RegExpGroupType.Modifier:
                                ParseModifierPattern(ref i);
                                break;

                            default:
                                i += (int)groupType >> 2;
                                break;
                        }

                        if (currentGroupAlternate is not null)
                        {
                            _groupStack.PushRef().Reset(groupType, parent: currentGroupAlternate);
                        }
                        else
                        {
                            _groupStack.PushRef().Reset(groupType);
                        }

                    FinishGroupStart:
                        SetFollowingQuantifierError(RegExpNothingToRepeat);
                        break;

                    case '|':
                        if (_capturingGroupNames is { Count: > 0 })
                        {
                            _groupStack.PeekRef().AddAlternate();
                        }
                        SetFollowingQuantifierError(RegExpNothingToRepeat);
                        break;

                    case ')':
                        Debug.Assert(_groupStack.Count > (_capturingGroupNames is { Count: > 0 } ? 1 : 0), RegExpUnmatchedParen); // CheckBracesBalance should ensure this.

                        if (_capturingGroupNames is { Count: > 0 })
                        {
                            _groupStack.PeekRef().HoistGroupNamesToParent();
                        }

                        groupType = _groupStack.Pop().Type;

                        if (mode.AllowsQuantifierAfterGroup(groupType))
                        {
                            ClearFollowingQuantifierError();
                        }
                        else
                        {
                            SetFollowingQuantifierError(RegExpInvalidQuantifier);
                        }
                        break;

                    case '^':
                        SetFollowingQuantifierError(RegExpNothingToRepeat);
                        break;

                    case '$':
                        SetFollowingQuantifierError(RegExpNothingToRepeat);
                        break;

                    case '.':
                        ClearFollowingQuantifierError();
                        break;

                    case '*' or '+' or '?':
                        if (_followingQuantifierErrorCode is not null)
                        {
                            Debug.Assert(_followingQuantifierErrorMessage is not null);
                            ReportSyntaxError(i, _followingQuantifierErrorMessage!, _followingQuantifierErrorCode);
                        }

                        if ((char)_pattern.CharCodeAt(i + 1) == '?')
                        {
                            i++;
                        }

                        SetFollowingQuantifierError(RegExpNothingToRepeat);
                        break;

                    case '{':
                        if (!TryEatRangeQuantifier())
                        {
                            mode.HandleInvalidRangeQuantifier(this, i);
                            break;
                        }

                        if ((char)_pattern.CharCodeAt(i + 1) == '?')
                        {
                            i++;
                        }

                        SetFollowingQuantifierError(RegExpNothingToRepeat);
                        break;

                    case '\\':
                        // TODO: report RegExpEscapeAtEndOfPattern
                        Debug.Assert(i + 1 < _pattern.Length, "Unexpected end of escape sequence in regular expression.");
                        mode.EatEscapeSequence(this);
                        break;

                    default:
                        mode.EatChar(ch, this);
                        ClearFollowingQuantifierError();
                        break;
                }
            }
        }

        private void ParseSetDefault<TMode>(TMode mode)
            where TMode : IMode
        {
            ref var i = ref _index;

            _setStartIndex = i;
            _setRangeStart = SetRangeNotStarted;

            i++;

            if ((char)_pattern.CharCodeAt(i) == '^')
            {
                i++;
            }

            for (; i < _pattern.Length; i++)
            {
                var ch = _pattern[i];

                switch (ch)
                {
                    case ']':
                        _setStartIndex = -1;
                        ClearFollowingQuantifierError();
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
                            mode.EatSetChar(ch, this, startIndex: i);
                        }
                        break;

                    case '\\':
                        // TODO: report RegExpEscapeAtEndOfPattern
                        Debug.Assert(i + 1 < _pattern.Length, "Unexpected end of escape sequence in regular expression.");
                        mode.EatEscapeSequence(this);
                        break;

                    default:
                        mode.EatSetChar(ch, this, startIndex: i);
                        break;
                }
            }

            // TODO: needed?
            ReportSyntaxError(i, RegExpUnterminatedCharacterClass); // unreachable if CheckBracesBalance works correctly
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
                // Assertion (word boundary) / Backspace
                case 'b':
                    // NOTE: For the sake of simplicity, we also use this logic for validation in unicode mode,
                    // so we return an unused dummy value for word boundary escapes outside character sets.
                    charCode = withinSet ? '\b' : char.MaxValue;
                    return true;
                case 'B':
                    charCode = char.MaxValue;
                    return !withinSet;

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

                // '-' is not a SyntaxCharacter by definition but must be escaped in character sets.
                // (However, outside the class it is not allowed to be escaped in unicode mode!)
                case '-':
                    charCode = ch;
                    return withinSet;
            }

            charCode = default;
            return false;
        }

        private bool TryEatRangeQuantifier()
        {
            ref var i = ref _index;

            var startIndex = i + 1;
            var endIndex = _pattern.IndexOf('}', startIndex);
            if (endIndex < 0 || endIndex == startIndex)
            {
                return false;
            }

            var index = _pattern.IndexOf(',', startIndex, endIndex - startIndex);
            if (index < 0)
            {
                index = endIndex;
            }

            int min, max;
            var slice = _pattern.AsSpan(startIndex, index - startIndex);
            if (!int.TryParse(slice.ToParsable(), NumberStyles.None, CultureInfo.InvariantCulture, out min))
            {
                if (slice.Length == 0 || slice.FindIndex(ch => !ch.IsDecimalDigit()) >= 0)
                {
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
                        return false;
                    }
                    max = -1;
                }
            }

            if (min >= 0 && max >= 0)
            {
                if (min > max)
                {
                    ReportSyntaxError(i, RegExpRangeOutOfOrder);
                }

                if (_followingQuantifierErrorCode is not null)
                {
                    Debug.Assert(_followingQuantifierErrorMessage is not null);
                    ReportSyntaxError(i, _followingQuantifierErrorMessage!, _followingQuantifierErrorCode);
                }
            }

            i = endIndex;
            return true;
        }

        private RegExpGroupType DetermineGroupType(int i)
        {
            if ((uint)++i >= (uint)_pattern.Length || _pattern[i] != '?')
            {
                return RegExpGroupType.Capturing;
            }

            if ((uint)++i >= (uint)_pattern.Length)
            {
                return RegExpGroupType.Unknown;
            }

            return _pattern[i] switch
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

        private void ParseModifierPattern(ref int i)
        {
            RegExpFlags flag, flagsToAdd = RegExpFlags.None, flagsToRemove = RegExpFlags.None;
            var startIndex = i;

            for (i += 2; ; i++)
            {
                switch (_pattern.CharCodeAt(i))
                {
                    case 'i': flag = RegExpFlags.IgnoreCase; break;
                    case 'm': flag = RegExpFlags.Multiline; break;
                    case 's': flag = RegExpFlags.DotAll; break;
                    case '-': i++; goto ParseFlagsToRemove;
                    case ':':
                        Debug.Assert(flagsToAdd != 0);
                        return;
                    default:
                        ReportSyntaxError(startIndex, RegExpInvalidGroup);
                        return;
                }

                if ((flagsToAdd & flag) != 0) // duplicate
                {
                    ReportSyntaxError(i, RegExpRepeatedFlag);
                }

                flagsToAdd |= flag;
            }

        ParseFlagsToRemove:
            for (; ; i++)
            {
                switch (_pattern.CharCodeAt(i))
                {
                    case 'i': flag = RegExpFlags.IgnoreCase; break;
                    case 'm': flag = RegExpFlags.Multiline; break;
                    case 's': flag = RegExpFlags.DotAll; break;
                    case '-':
                        ReportSyntaxError(i, RegExpMultipleFlagDashes);
                        return;
                    case ':':
                        if ((flagsToAdd | flagsToRemove) == 0) // edge case of /(?-:)/
                        {
                            ReportSyntaxError(i - 1, RegExpInvalidFlagGroup);
                        }
                        return;
                    default:
                        ReportSyntaxError(startIndex, RegExpInvalidGroup);
                        return;
                }

                if ((flagsToRemove & flag) != 0 // duplicate
                    || (flagsToAdd & flag) != 0) // same in add and remove group
                {
                    ReportSyntaxError(i, RegExpRepeatedFlag);
                }

                flagsToRemove |= flag;
            }
        }

        private string? ReadNormalizedCapturingGroupName(ref int i)
        {
            if (_pattern.CharCodeAt(i + 1) == '<')
            {
                var startIndex = i + 2;
                var endIndex = _pattern.IndexOf('>', startIndex);
                if (endIndex >= 0)
                {
                    var cp = _pattern.CodePointAt(i = startIndex, endIndex);
                    var allowAstral = (_flags & RegExpFlags.Unicode) != 0 || _tokenizer._options.EcmaVersion >= EcmaVersion.ES11;
                    if (IsIdentifierStart(cp, allowAstral) || cp == '\\')
                    {
                        var groupName = ReadIdentifier(ref i, endIndex, allowAstral);
                        if (i == endIndex)
                        {
                            return DeduplicateString(groupName, ref _tokenizer._stringPool);
                        }
                    }
                }

                ReportSyntaxError(startIndex, RegExpInvalidCaptureGroupName);
            }

            return null;
        }

        private ReadOnlySpan<char> ReadIdentifier(ref int i, int endIndex, bool allowAstral)
        {
            var containsEscape = false;
            var sb = _auxiliaryStringBuilder is not null ? _auxiliaryStringBuilder.Clear() : _auxiliaryStringBuilder = new StringBuilder();
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
                            ReportSyntaxError(escStart, RegExpInvalidUnicodeEscape);
                        }

                        ++i;
                    }
                    else
                    {
                        if (!TryReadHexEscape(_pattern, ref i, endIndex, charCodeLength: 4, out var ch))
                        {
                            ReportSyntaxError(escStart, RegExpInvalidUnicodeEscape);
                        }
                        cp = ch;
                        ++i;

                        if (allowAstral && ((char)ch).IsHighSurrogate() && i + 1 < endIndex && _pattern[i] == '\\' && _pattern[i + 1] == 'u')
                        {
                            escStart = i++;
                            if (TryReadHexEscape(_pattern, ref i, endIndex, charCodeLength: 4, out var ch2) && ((char)ch2).IsLowSurrogate())
                            {
                                ++i;
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

        private bool TryEatBackreference(int startIndex)
        {
            ref var i = ref _index;

            var endIndex = _pattern.AsSpan().FindIndex(ch => !ch.IsDecimalDigit(), startIndex: i + 1);
            if (endIndex < 0)
            {
                endIndex = _pattern.Length;
            }

            var slice = _pattern.AsSpan(i, endIndex - i);
            var number = int.Parse(slice.ToParsable(), NumberStyles.None, CultureInfo.InvariantCulture);
            if (number > _capturingGroups.Count)
            {
                return false;
            }

            i = endIndex - 1;
            return true;
        }

        private void EatNamedBackreference(int startIndex)
        {
            ref var i = ref _index;

            // 'k' GroupName
            if (ReadNormalizedCapturingGroupName(ref i) is { } groupName)
            {
                if (_capturingGroupNames is null || !_capturingGroupNames.Contains(groupName))
                {
                    ReportSyntaxError(startIndex + 3, RegExpInvalidNamedCaptureReference);
                }
            }
            else
            {
                ReportSyntaxError(startIndex, RegExpInvalidNamedReference);
            }
        }

        // TODO: remove?
        //private static bool IsDefinedCapturingGroupName(string value, int startIndex, ReadOnlySpan<RegExpCapturingGroup> capturingGroups)
        //{
        //    for (var i = 0; i < capturingGroups.Length; i++)
        //    {
        //        var group = capturingGroups[i];
        //        if (group.StartIndex < startIndex && group.Name == value)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        #region Context for ParsePattern

        private int _index;

        // TODO: needed?
        private StringBuilder? _auxiliaryStringBuilder;

        private ArrayList<RegExpCapturingGroup> _capturingGroups;

        private HashSet<string>? _capturingGroupNames;

        // The number of capturing groups encountered so far. Will be increased when the opening bracket of a capturing group is found.
        private int _capturingGroupCounter;

        // Originally, group names are unique in JS regexes but there's a proposal which may change this soon
        // (see https://github.com/tc39/proposal-duplicate-named-capturing-groups).
        // The .NET regex engine handles duplicate group names fine, so nothing prevents us from implementing this,
        // however it makes things a bit more complicated: group names have still to be unique in an alternate part of a group,
        // so we need to do some extra bookkeeping to handle this.
        private ArrayList<RegExpGroup> _groupStack;

        // The start index of a character set (e.g. /[a-z]/). Negative values indicate that the parser is not within a character set currently.
        private int _setStartIndex;

        private bool WithinSet { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _setStartIndex >= 0; }

        // A variable which keeps track of ranges in character sets and encodes multiple pieces of information related to this:
        // Basically, it stores the starting code point of a potential range. However, it can also have the following special values:
        // * SetRangeNotStarted - Indicates that a range hasn't started yet (i.e. we're at the start of the set or right after a range).
        // * SetRangeStartedWithCharClass - Indicates that a potentially invalid range has started with a character class like \d or \p{...} (e.g. /[\d-A]/).
        // May store the bitwise complement of the possible values listed above, which indicates that the range indicator '-' has been encountered.
        private int _setRangeStart;

        // A variable which keeps track whether the current construct can be followed by a quantifier. A null value indicates that a quantifier can follow,
        // otherwise it stores the error message for cases where a quantifier follows.
        private string? _followingQuantifierErrorMessage;
        private string? _followingQuantifierErrorCode;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearFollowingQuantifierError()
        {
            _followingQuantifierErrorMessage = _followingQuantifierErrorCode = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetFollowingQuantifierError(string message, [CallerArgumentExpression(nameof(message))] string? code = "UnknownError")
        {
            _followingQuantifierErrorMessage = message;
            _followingQuantifierErrorCode = code;
        }

        private int _recursionDepth;

        ref int StackGuard.IRecursionDepthProvider.CurrentDepth => ref _recursionDepth;

        private void ResetParseContext()
        {
            // TODO: _capturingGroups?

            // _auxiliaryStringBuilder, _index, _capturingGroupNames, _setRangeStart are reset externally.
            // _capturingGroups is not reused.

            _capturingGroupCounter = 0;

            _groupStack.Clear();
            if (_capturingGroupNames is { Count: > 0 })
            {
                _groupStack.PushRef() = new RegExpGroup() { FirstAlternate = new RegExpGroupAlternate(null) };
            }

            _setStartIndex = -1;

            SetFollowingQuantifierError(RegExpNothingToRepeat);

            _recursionDepth = 0;
        }

        internal void ReleaseReferencesAndLargeBuffers()
        {
            _pattern = null!;
            _flagsOriginal = null!;

            _capturingGroups = default;
            _capturingGroupNames = null;

            _groupStack.Clear();
            if (_groupStack.Capacity > 64)
            {
                _groupStack.Capacity = 64;
            }
        }

        #endregion

        private interface IMode
        {
            void EatChar(char ch, RegExpParser parser);

            void EatSetChar(char ch, RegExpParser parser, int startIndex);

            void EatEscapeSequence(RegExpParser parser);

            void ParseSet(RegExpParser parser);

            bool AllowsQuantifierAfterGroup(RegExpGroupType groupType);

            void HandleInvalidRangeQuantifier(RegExpParser parser, int startIndex);
        }
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
        // Alternates are tracked only if the RegExp has named capturing groups.
        // Otherwise, the corresponding fields are unused and must remain at their default value.

        // NOTE: We optimize for the case of no alternates.
        private ArrayList<RegExpGroupAlternate> _additionalAlternates;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset(RegExpGroupType type)
        {
            Type = type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset(RegExpGroupType type, RegExpGroupAlternate? parent = null)
        {
            Reset(type);
            FirstAlternate = new RegExpGroupAlternate(parent);
            _additionalAlternates.Clear();
        }

        public RegExpGroupType Type;

        public RegExpGroupAlternate? FirstAlternate;

        public readonly RegExpGroupAlternate? LastAlternate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _additionalAlternates.Count == 0 ? FirstAlternate : _additionalAlternates.LastItemRef();
        }

        public void AddAlternate()
        {
            Debug.Assert(FirstAlternate is not null);
            _additionalAlternates.Add(new RegExpGroupAlternate(FirstAlternate!.Parent));
        }

        public readonly void HoistGroupNamesToParent()
        {
            Debug.Assert(FirstAlternate is not null);
            var parent = FirstAlternate!.Parent;
            Debug.Assert(parent is not null);

            FirstAlternate!.HoistGroupNamesTo(parent!);
            for (var i = 0; i < _additionalAlternates.Count; i++)
            {
                _additionalAlternates[i].HoistGroupNamesTo(parent!);
            }
        }
    }

    private sealed class RegExpGroupAlternate
    {
        private ArrayList<string> _groupNames;

        public RegExpGroupAlternate(RegExpGroupAlternate? parent)
        {
            Parent = parent;
        }

        public readonly RegExpGroupAlternate? Parent;

        public bool IsDefinedGroupName(string value) => _groupNames.AsReadOnlySpan().BinarySearch(value) >= 0;

        public bool TryAddGroupName(string value)
        {
            var index = _groupNames.AsReadOnlySpan().BinarySearch(value);

            var isDefined = index >= 0;
            var scope = this;
            for (; ; )
            {
                if (isDefined)
                {
                    return false;
                }

                if ((scope = scope!.Parent) is null)
                {
                    break;
                }

                isDefined = scope.IsDefinedGroupName(value);
            }

            _groupNames.Insert(~index, value);
            return true;
        }

        public void HoistGroupNamesTo(RegExpGroupAlternate other)
        {
            if (_groupNames.Count > 0)
            {
                if (other._groupNames.Count == 0)
                {
                    other._groupNames = _groupNames;
                }
                else
                {
                    foreach (var groupName in _groupNames)
                    {
                        var index = other._groupNames.AsReadOnlySpan().BinarySearch(groupName);
                        if (index < 0)
                        {
                            other._groupNames.Insert(~index, groupName);
                        }
                    }
                }
                _groupNames = default;
            }
        }
    }

    internal readonly struct RegExpCapturingGroup
    {
        public RegExpCapturingGroup(int startIndex, string? name)
        {
            StartIndex = startIndex;
            Name = name;
        }

        public int StartIndex { get; }
        public string? Name { get; }
    }
}
