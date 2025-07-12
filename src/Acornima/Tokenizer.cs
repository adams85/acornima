using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Acornima.Helpers;

namespace Acornima;

using static SyntaxErrorMessages;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js

public sealed partial class Tokenizer : ITokenizer
{
    internal const string UnknownError = nameof(UnknownError);

    private readonly TokenizerOptions _options;
    private readonly IExtension? _extension;

    internal Tokenizer(TokenizerOptions options, IExtension? extension)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _extension = extension;
        _input = null!;
        _type = null!;
    }

    public Tokenizer(string input)
        : this(input, TokenizerOptions.Default) { }

    public Tokenizer(string input, TokenizerOptions options)
        : this(input, SourceType.Script, sourceFile: null, options) { }

    public Tokenizer(string input, SourceType sourceType, string? sourceFile, TokenizerOptions options)
        : this(input ?? throw new ArgumentNullException(nameof(input)), 0, input.Length, sourceType, sourceFile, options) { }

    public Tokenizer(string input, int start, int length, SourceType sourceType, string? sourceFile, TokenizerOptions options)
        : this(options, extension: null)
    {
        ResetInternal(input, start, length, sourceType, sourceFile, trackRegExpContext: true);
    }

    public string Input => _input;
    public Range Range => new Range(_startPosition, _endPosition);
    public SourceType SourceType => _sourceType;
    public string? SourceFile => _sourceFile;

    public TokenizerOptions Options => _options;

    public Token Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new Token(_type.Kind, _value, new Range(_start, _end), new SourceLocation(_startLocation, _endLocation, _sourceFile));
    }

    public Token GetToken(in TokenizerContext context = default)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.getToken = function`

        Next(context);
        return Current;
    }

    // Move to the next token
    public void Next(in TokenizerContext context = default)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.next = function`

        if (!context.IgnoreEscapeSequenceInKeyword && _type.Keyword is not null && _containsEscape)
        {
            // RaiseRecoverable(_start, "Escape sequence in keyword " + _type.Label); // original acornjs error reporting
            RaiseRecoverable(_start, InvalidEscapedReservedWord);
        }

        _strict = _inModule || context.Strict;
        _requireValidEscapeSequenceInTemplate = context.RequireValidEscapeSequenceInTemplate;
        _lastTokenEnd = _end;
        _lastTokenStart = _start;
        _lastTokenEndLocation = _endLocation;
        _lastTokenStartLocation = _startLocation;

        NextToken();

        // NOTE: Originally, in acornjs this callback fires with an EOF before the first actual token.
        // This behavior doesn't seem useful, so we change it.
        _options._onToken?.Invoke(Current);
    }

    // Read a single token, updating the token-related properties.
    private void NextToken()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.nextToken = function`

        Debug.Assert(_contextStack.Count > 0);
        var currentContext = CurrentContext;

        if (!currentContext.PreserveSpace)
        {
            SkipSpace(_options._onComment);
        }

        _start = _position;
        _startLocation = CurrentPosition;

        if (_position >= _endPosition)
        {
            FinishToken(TokenType.EOF, TokenValue.EOF);
            ReleaseLargeBuffers();
            return;
        }

        if (_extension is null)
        {
            ReadToken(currentContext);
        }
        else
        {
            _extension.ReadToken(currentContext);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadToken(TokenContext currentContext)
    {
        if (currentContext != TokenContext.QuoteInTemplate
            ? !TryReadToken(FullCharCodeAtPosition())
            : !TryReadTemplateToken())
        {
            // Raise(start, $"Unexpected character '{UnicodeHelper.CodePointToString(cp)}'"); // original acornjs error reporting
            Unexpected();
        }
    }

    internal bool TryReadToken(int cp)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readToken = function`, `pp.getTokenFromCode = function`

        // NOTE: `getTokenFromCode` and `readToken` was merged into a single method (`TryReadToken`).
        // The merged method was also changed to return success instead of throwing on invalid token.

        // Identifier or keyword. '\uXXXX' sequences are allowed in
        // identifiers, so '\' also dispatches to that.
        if (IsIdentifierStart(cp, allowAstral: _options._ecmaVersion >= EcmaVersion.ES6) || cp == '\\')
        {
            return ReadWord();
        }

        switch (cp)
        {
            // The interpretation of a dot depends on whether it is followed
            // by a digit or another two do
            case '.':
                return ReadToken_Dot(cp);

            // Punctuation tokens.
            case '(':
                ++_position;
                return FinishToken(TokenType.ParenLeft, ((char)cp).ToStringCached());

            case ')':
                ++_position;
                return FinishToken(TokenType.ParenRight, ((char)cp).ToStringCached());

            case ';':
                ++_position;
                return FinishToken(TokenType.Semicolon, ((char)cp).ToStringCached());

            case ',':
                ++_position;
                return FinishToken(TokenType.Comma, ((char)cp).ToStringCached());

            case '[':
                ++_position;
                return FinishToken(TokenType.BracketLeft, ((char)cp).ToStringCached());

            case ']':
                ++_position;
                return FinishToken(TokenType.BracketRight, ((char)cp).ToStringCached());

            case '{':
                ++_position;
                return FinishToken(TokenType.BraceLeft, ((char)cp).ToStringCached());

            case '}':
                ++_position;
                return FinishToken(TokenType.BraceRight, ((char)cp).ToStringCached());

            case ':':
                ++_position;
                return FinishToken(TokenType.Colon, ((char)cp).ToStringCached());

            case '`':
                if (_options._ecmaVersion < EcmaVersion.ES6)
                {
                    break;
                }

                ++_position;
                return FinishToken(TokenType.BackQuote, ((char)cp).ToStringCached());

            case '0':
                var nextCh = CharCodeAtPosition(1);
                if (nextCh is 'x' or 'X')
                {
                    return ReadRadixNumber(16); // '0x', '0X' - hex number
                }

                if (_options._ecmaVersion >= EcmaVersion.ES6)
                {
                    if (nextCh is 'o' or 'O')
                    {
                        return ReadRadixNumber(8); // '0o', '0O' - octal number
                    }

                    if (nextCh is 'b' or 'B')
                    {
                        return ReadRadixNumber(2); // '0b', '0B' - binary number
                    }
                }

                return ReadNumber(startsWithZero: true, startsWithDot: false);

            // Anything else beginning with a digit is an integer, octal
            // number, or float.
            case >= '1' and <= '9':
                return ReadNumber(startsWithZero: false, startsWithDot: false);

            // Quotes produce strings.
            case '"' or '\'':
                return ReadString(quote: cp);

            // Operators are parsed inline in tiny state machines. '=' (61) is
            // often referred to. `finishOp` simply skips the amount of
            // characters it is given as second argument, and returns a token
            // of the type given by its first argument.

            case '/':
                return ReadToken_Slash(cp);

            case '%' or '*':
                return ReadToken_Mult_Modulo_Exp(cp);

            case '|' or '&':
                return ReadToken_PipeAmp(cp);

            case '^':
                return ReadToken_Caret(cp);

            case '+' or '-':
                return ReadToken_PlusMinus(cp);

            case '<' or '>':
                return ReadToken_Lt_Gt(cp);

            case '=' or '!':
                return ReadToken_Eq_Excl(cp);

            case '?':
                return ReadToken_Question(cp);

            case '~':
                return FinishOp(TokenType.PrefixOp, ((char)cp).ToStringCached());

            case '#':
                return ReadToken_NumberSign(cp);

            case '@' when _options.AllowDecorators():
                _position++;
                return FinishToken(TokenType.At, ((char)cp).ToStringCached());
        }

        return false;
    }

    private void SkipBlockComment(OnCommentHandler? onComment)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.skipBlockComment = function`

        var startLoc = CurrentPosition;
        var start = _position;
        var end = _input.IndexOf("*/", _position += 2, _endPosition - _position, StringComparison.Ordinal);
        if (end == -1)
        {
            // Raise(_position - 2, "Unterminated comment"); // original acornjs error reporting
            Unexpected(start);
        }

        _position = end + 2;
        for (int nextBreak, pos = start; (nextBreak = NextLineBreak(_input, pos, _position)) >= 0;)
        {
            ++_currentLine;
            pos = _lineStart = nextBreak;
        }

        onComment?.Invoke(new Comment(CommentKind.Block, new Range(start + 2, end), new Range(start, _position),
            new SourceLocation(startLoc, CurrentPosition, _sourceFile)));
    }

    private void SkipLineComment(int startSkip, CommentKind kind, OnCommentHandler? onComment)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.skipLineComment = function`

        var startLoc = CurrentPosition;
        var start = _position;
        _position += startSkip;

        for (int ch; (ch = CharCodeAtPosition()) >= 0 && !IsNewLine((char)ch);)
        {
            ++_position;
        }

        onComment?.Invoke(new Comment(kind, new Range(start + startSkip, _position), new Range(start, _position),
            new SourceLocation(startLoc, CurrentPosition, _sourceFile)));
    }

    // Called at the start of the parse and after every token. Skips
    // whitespace and comments.
    private void SkipSpace(OnCommentHandler? onComment)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.skipSpace = function`

        while (_position < _endPosition)
        {
            var ch = _input[_position];

            var charFlags = GetCharFlags(ch) & CharFlags.Skipped;
            if (charFlags == 0)
            {
                return;
            }

            if (charFlags == CharFlags.WhiteSpace)
            {
                ++_position;
            }
            else if (charFlags == CharFlags.LineTerminator)
            {
                ++_position;

                if (ch == '\r' && CharCodeAtPosition() == '\n')
                {
                    ++_position;
                }

                ++_currentLine;
                _lineStart = _position;
            }
            else
            {
                switch (ch)
                {
                    case '/':
                        ch = (char)CharCodeAtPosition(1);
                        if (ch == '*')
                        {
                            SkipBlockComment(onComment);
                            break;
                        }
                        else if (ch == '/')
                        {
                            SkipLineComment(startSkip: 2, CommentKind.Line, onComment);
                            break;
                        }
                        goto default;

                    case '#' when _position == 0
                        && _sourceType != SourceType.Unknown
                        && (_options._allowHashBang ?? _options._ecmaVersion >= EcmaVersion.ES14)
                        && 1 < _endPosition && _input[0] == '#' && _input[1] == '!':

                        SkipLineComment(startSkip: 2, CommentKind.HashBang, onComment);
                        break;

                    default:
                        return;
                }
            }
        }
    }

    internal int NextTokenPosition(out int line, out int lineStart)
    {
        // Replacement for the usage of the `skipWhiteSpace` regex in the original acornjs implementation.

        var originalPosition = _position;
        var originalLineNumber = _currentLine;
        var originalLineStart = _lineStart;

        SkipSpace(onComment: null);

        var position = _position;
        line = _currentLine;
        lineStart = _lineStart;

        _position = originalPosition;
        _currentLine = originalLineNumber;
        _lineStart = originalLineStart;

        return position;
    }

    internal int NextTokenPositionAt(int position, ref int line, ref int lineStart)
    {
        // Replacement for the usage of the `skipWhiteSpace` regex in the original acornjs implementation.

        var originalPosition = _position;
        var originalLineNumber = _currentLine;
        var originalLineStart = _lineStart;

        _position = position;
        _currentLine = line;
        _lineStart = lineStart;

        SkipSpace(onComment: null);

        position = _position;
        line = _currentLine;
        lineStart = _lineStart;

        _position = originalPosition;
        _currentLine = originalLineNumber;
        _lineStart = originalLineStart;

        return position;
    }

    // Called at the end of every token. Sets `end`, `val`, and
    // maintains `context` and `exprAllowed`, and skips the space after
    // the token, so that the next one's `start` will point at the
    // right position.
    internal bool FinishToken(TokenType type, TokenValue value)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.finishToken = function`

        _end = _position;
        _endLocation = CurrentPosition;

        var prevType = _type;
        _type = type;
        _value = value;

        if (_trackRegExpContext)
        {
            if (_extension is null)
            {
                UpdateContext(prevType);
            }
            else
            {
                _extension.UpdateContext(prevType);
            }
        }
        else if (_type.Kind == TokenKind.Punctuator)
        {
            Debug.Assert(_extension is null || _extension.SupportsMinimalContextTracking);

            UpdateContextMinimal(prevType);
        }

        return true;
    }

    #region Token reading

    // This is the function that is called to fetch the next token. It
    // is somewhat obscure, because it works in character codes rather
    // than characters, and because operator parsing has been inlined
    // into it.
    //
    // All in the name of speed.

    private bool ReadToken_Dot(int ch)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readToken_dot = function`

        var nextCh = CharCodeAtPosition(1);
        if (((char)nextCh).IsDecimalDigit())
        {
            return ReadNumber(startsWithZero: false, startsWithDot: true);
        }
        else if (_options._ecmaVersion >= EcmaVersion.ES6 && nextCh == '.' && CharCodeAtPosition(2) == '.')
        {
            return FinishOp(TokenType.Ellipsis, "...");
        }
        else
        {
            return FinishOp(TokenType.Dot, ((char)ch).ToStringCached());
        }
    }

    private bool ReadToken_Slash(int ch)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readToken_slash = function`

        var nextCh = CharCodeAtPosition(1);

        Debug.Assert(_trackRegExpContext || !_expressionAllowed);
        if (_expressionAllowed)
        {
            ++_position;
            return ReadRegExp();
        }
        else if (nextCh == '=')
        {
            return FinishOp(TokenType.Assign, "/=");
        }
        else
        {
            return FinishOp(TokenType.Slash, ((char)ch).ToStringCached());
        }
    }

    private bool ReadToken_Mult_Modulo_Exp(int ch)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readToken_mult_modulo_exp = function`

        var nextCh = CharCodeAtPosition(1);
        if (nextCh == '=')
        {
            return FinishOp(TokenType.Assign, ch == '*' ? "*=" : "%=");
        }
        else if (_options._ecmaVersion >= EcmaVersion.ES7 && nextCh == '*')
        {
            // exponentiation operator ** and **=
            return CharCodeAtPosition(2) == '='
                ? FinishOp(TokenType.Assign, "**=")
                : FinishOp(TokenType.StarStar, "**");
        }
        else
        {
            return FinishOp(ch == '*' ? TokenType.Star : TokenType.Modulo, ((char)ch).ToStringCached());
        }
    }

    private bool ReadToken_PipeAmp(int ch)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readToken_pipe_amp = function`

        var nextCh = CharCodeAtPosition(1);
        if (nextCh == ch)
        {
            if (_options._ecmaVersion >= EcmaVersion.ES12 && CharCodeAtPosition(2) == '=')
            {
                return FinishOp(TokenType.Assign, ch == '&' ? "&&=" : "||=");
            }
            else
            {
                return ch == '&'
                    ? FinishOp(TokenType.LogicalAnd, "&&")
                    : FinishOp(TokenType.LogicalOr, "||");
            }
        }
        else if (nextCh == '=')
        {
            return FinishOp(TokenType.Assign, ch == '&' ? "&=" : "|=");
        }
        else
        {
            return FinishOp(ch == '&' ? TokenType.BitwiseAnd : TokenType.BitwiseOr, ((char)ch).ToStringCached());
        }
    }

    private bool ReadToken_Caret(int ch)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readToken_caret = function`

        var nextCh = CharCodeAtPosition(1);
        if (nextCh == '=')
        {
            return FinishOp(TokenType.Assign, "^=");
        }
        else
        {
            return FinishOp(TokenType.BitwiseXor, ((char)ch).ToStringCached());
        }
    }

    private bool ReadToken_PlusMinus(int ch)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readToken_plus_min = function`

        var nextCh = CharCodeAtPosition(1);
        if (nextCh == ch)
        {
            if (_sourceType == SourceType.Script && nextCh == '-'
                && CharCodeAtPosition(2) == '>'
                && (_lastTokenEnd == 0 || _lastTokenEndLocation.Line != _currentLine))
            {
                // A `-->` line comment
                SkipLineComment(startSkip: 3, CommentKind.Line, _options._onComment);
                SkipSpace(_options._onComment);
                NextToken();
                return true;
            }
            else
            {
                return FinishOp(TokenType.IncDec, ch == '+' ? "++" : "--");
            }
        }
        else if (nextCh == '=')
        {
            return FinishOp(TokenType.Assign, ch == '+' ? "+=" : "-=");
        }
        else
        {
            return FinishOp(TokenType.PlusMinus, ((char)ch).ToStringCached());
        }
    }

    private bool ReadToken_Lt_Gt(int ch)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readToken_lt_gt = function`

        var nextCh = CharCodeAtPosition(1);
        if (nextCh == ch)
        {
            if (ch == '>' && CharCodeAtPosition(2) == '>')
            {
                return CharCodeAtPosition(3) == '='
                    ? FinishOp(TokenType.Assign, ">>>=")
                    : FinishOp(TokenType.BitShift, ">>>");
            }
            else if (CharCodeAtPosition(2) == '=')
            {
                return FinishOp(TokenType.Assign, ch == '<' ? "<<=" : ">>=");
            }
            else
            {
                return FinishOp(TokenType.BitShift, ch == '<' ? "<<" : ">>");
            }
        }
        else if (_sourceType == SourceType.Script && ch == '<' && nextCh == '!' && CharCodeAtPosition(2) == '-' && CharCodeAtPosition(3) == '-')
        {
            // `<!--`, an XML-style comment that should be interpreted as a line comment
            SkipLineComment(startSkip: 4, CommentKind.Line, _options._onComment);
            SkipSpace(_options._onComment);
            NextToken();
            return true;
        }
        else
        {
            return FinishOp(TokenType.Relational, nextCh == '='
                ? (ch == '<' ? "<=" : ">=")
                : ((char)ch).ToStringCached());
        }
    }

    private bool ReadToken_Eq_Excl(int ch)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readToken_eq_excl = function`

        var nextCh = CharCodeAtPosition(1);
        if (nextCh == '=')
        {
            return FinishOp(TokenType.Equality, CharCodeAtPosition(2) == '='
                ? (ch == '=' ? "===" : "!==")
                : (ch == '=' ? "==" : "!="));
        }
        else if (ch == '=' && nextCh == '>' && _options._ecmaVersion >= EcmaVersion.ES6)
        {
            return FinishOp(TokenType.Arrow, "=>");
        }
        else
        {
            return FinishOp(ch == '=' ? TokenType.Eq : TokenType.PrefixOp, ((char)ch).ToStringCached());
        }
    }

    private bool ReadToken_Question(int ch)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readToken_question = function`

        var ecmaVersion = _options._ecmaVersion;
        if (ecmaVersion >= EcmaVersion.ES11)
        {
            var nextCh = CharCodeAtPosition(1);
            if (nextCh == '.')
            {
                if (!((char)CharCodeAtPosition(2)).IsDecimalDigit())
                {
                    return FinishOp(TokenType.QuestionDot, "?.");
                }
            }

            if (nextCh == '?')
            {
                if (ecmaVersion >= EcmaVersion.ES12)
                {
                    if (CharCodeAtPosition(2) == '=')
                    {
                        return FinishOp(TokenType.Assign, "??=");
                    }
                }
                return FinishOp(TokenType.Coalesce, "??");
            }
        }

        return FinishOp(TokenType.Question, ((char)ch).ToStringCached());
    }

    private bool ReadToken_NumberSign(int ch)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readToken_numberSign = function`

        var ecmaVersion = _options._ecmaVersion;
        if (ecmaVersion >= EcmaVersion.ES13)
        {
            ++_position;
            ch = FullCharCodeAtPosition();
            if (IsIdentifierStart(ch, allowAstral: true) || ch == '\\')
            {
                return FinishToken(TokenType.PrivateId, DeduplicateString(ReadWord1(), ref _stringPool));
            }
        }

        // return Raise<bool>(_position, $"Unexpected character '{UnicodeHelper.CodePointToString(ch)}'"); // original acornjs error reporting
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool FinishOp(TokenType type, string value)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.finishOp = function`

        _position += value.Length;
        return FinishToken(type, value);
    }

    internal bool ReadRegExp()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readRegexp = function`

        var escaped = false;
        var inClass = false;
        var start = _position;
        for (int ch; (ch = CharCodeAtPosition()) >= 0; _position++)
        {
            if (IsNewLine((char)ch))
            {
                goto Unterminated;
            }

            if (!escaped)
            {
                switch (ch)
                {
                    case '[':
                        inClass = true;
                        break;
                    case ']' when inClass:
                        inClass = false;
                        break;
                    case '\\':
                        escaped = true;
                        break;
                    case '/' when !inClass:
                        var pattern = _input.SliceBetween(start, _position);
                        var flagsStart = ++_position;
                        var flags = ReadWord1();
                        if (_containsEscape)
                        {
                            // Unexpected(flagsStart); // original acornjs error reporting
                            RaiseRecoverable(flagsStart, InvalidRegExpFlags);
                        }

                        var patternCached = DeduplicateString(pattern, ref _stringPool, NonIdentifierDeduplicationThreshold);
                        var flagsCached = DeduplicateString(flags, ref _stringPool);

                        var parseResult = _options._regExpParseMode != RegExpParseMode.Skip
                            ? new RegExpParser(patternCached, start, flagsCached, flagsStart, this).Parse()
                            : default;

                        var regExpValue = new RegExpValue(patternCached, flagsCached);

                        return FinishToken(TokenType.RegExp, Tuple.Create(regExpValue, parseResult));
                }
            }
            else
            {
                escaped = false;
            }
        }

    Unterminated:
        // return Raise<bool>(start, "Unterminated regular expression"); // original acornjs error reporting
        return Raise<bool>(start - 1, UnterminatedRegExp);
    }

    // Read an integer in the given radix. Return the number of digits read,
    // excluding the potential separators.
    internal int ReadInt(out ulong value, out bool overflow, out bool hasSeparator, byte radix, bool allowSeparators = false, bool startsWithZero = false, int len = int.MaxValue)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readInt = function`

        value = 0;
        overflow = hasSeparator = false;
        char lastCh = default;
        var numDigits = 0;
        for (int i = 0, e = len; i < e; i++, _position++)
        {
            var ch = CharCodeAtPosition();

            if (allowSeparators && ch == '_')
            {
                hasSeparator = true;
                if (startsWithZero)
                {
                    // RaiseRecoverable(_position, "Numeric separator is not allowed in legacy octal numeric literals"); // original acornjs error reporting
                    if (i > 1)
                    {
                        RaiseRecoverable(_position, InvalidOrUnexpectedToken);
                    }
                    else
                    {
                        RaiseRecoverable(_position, ZeroDigitNumericSeparator);
                    }
                }
                else if (i == 0)
                {
                    // RaiseRecoverable(_position, "Numeric separator is not allowed at the first of digits"); // original acornjs error reporting
                    RaiseRecoverable(_start, InvalidOrUnexpectedToken);
                }

                if (lastCh == '_')
                {
                    // RaiseRecoverable(_position, "Numeric separator must be exactly one underscore"); // original acornjs error reporting
                    RaiseRecoverable(_position, ContinuousNumericSeparator);
                }

                lastCh = (char)ch;
                continue;
            }

            var digitValue = GetDigitValue(ch);
            if (digitValue >= radix)
            {
                break;
            }

            lastCh = (char)ch;
            if (!overflow)
            {
                try { value = checked(value * radix + digitValue); }
                catch (OverflowException) { overflow = true; }
            }
            else
            {
                value = value * radix + digitValue;
            }

            numDigits++;
        }

        if (allowSeparators && lastCh == '_')
        {
            // RaiseRecoverable(_position - 1, "Numeric separator is not allowed at the last of digits"); // original acornjs error reporting
            RaiseRecoverable(_position - 1, TrailingNumericSeparator);
        }

        return numDigits;
    }

    private bool ReadRadixNumber(byte radix)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readRadixNumber = function`

        _position += 2; // 0x

        var allowSeparators = _options._ecmaVersion >= EcmaVersion.ES12;
        if (!(ReadInt(out var intValue, out var overflow, out _, radix, allowSeparators) > 0))
        {
            // Raise(_start + 2, "Expected number in radix " + radix); // original acornjs error reporting
            Unexpected();
        }

        TokenType tokenType;
        TokenValue val;
        if (_options._ecmaVersion >= EcmaVersion.ES11 && CharCodeAtPosition() == 'n')
        {
            if (!overflow)
            {
                val = new BigInteger(intValue);
            }
            else
            {
                var slice = _input.SliceBetween(_start + 2, _position);
                val = ParseIntToBigInteger(slice, radix);
            }
            tokenType = TokenType.BigInt;
            ++_position;
        }
        else
        {
            if (IsIdentifierStart(FullCharCodeAtPosition()))
            {
                // Raise(_position, "Identifier directly after number"); // original acornjs error reporting
                Unexpected();
            }

            if (!overflow)
            {
                val = (double)intValue;
            }
            else
            {
                var slice = _input.SliceBetween(_start + 2, _position);
                val = ParseIntToDouble(slice, radix);
            }
            tokenType = TokenType.Number;
        }

        return FinishToken(tokenType, val);
    }

    // Read an integer, octal integer, or floating-point number.
    private bool ReadNumber(bool startsWithZero, bool startsWithDot)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readNumber = function`

        var start = _position;

        int numDigits, nextCh;
        ulong intValue;
        bool overflow, hasSeparator, allowSeparators = _options._ecmaVersion >= EcmaVersion.ES12;
        TokenValue val;
        ReadOnlySpan<char> slice;

        if (startsWithZero)
        {
            numDigits = ReadInt(out intValue, out overflow, out hasSeparator, radix: 8, allowSeparators, startsWithZero: true);
            Debug.Assert(numDigits > 0);

            nextCh = CharCodeAtPosition();
            if (nextCh is '8' or '9') // literals like 08 are valid in non-strict mode, so reparse them as a decimal number
            {
                _position = start;
                numDigits = ReadInt(out intValue, out overflow, out hasSeparator, radix: 10, allowSeparators, startsWithZero: true);
                Debug.Assert(numDigits > 0);

                nextCh = CharCodeAtPosition();
                if (nextCh == 'n')
                {
                    Unexpected<bool>(start);
                }

                goto ParseDecimal;
            }
            else if (numDigits > 1) // actual octal number
            {
                if (IsIdentifierStart(FullCharCodeAtPosition()))
                {
                    // Raise(_position, "Identifier directly after number"); // original acornjs error reporting
                    Unexpected(start);
                }

                if (_strict)
                {
                    // Raise(start, "Invalid number"); // original acornjs error reporting
                    RaiseRecoverable(start, StrictOctalLiteral);
                }

                if (!overflow)
                {
                    val = (double)intValue;
                }
                else
                {
                    slice = _input.SliceBetween(start, _position);
                    val = ParseIntToDouble(slice, radix: 10);
                }

                return FinishToken(TokenType.Number, val);
            }
        }
        else if (startsWithDot)
        {
            numDigits = 0;
            intValue = 0;
            overflow = hasSeparator = false;
            nextCh = CharCodeAtPosition();

            goto ParseDecimal;
        }
        else
        {
            numDigits = ReadInt(out intValue, out overflow, out hasSeparator, radix: 10, allowSeparators);
            Debug.Assert(numDigits > 0);
            nextCh = CharCodeAtPosition();
        }

        if (_options._ecmaVersion >= EcmaVersion.ES11 && nextCh == 'n')
        {
            if (!overflow)
            {
                val = new BigInteger(intValue);
            }
            else
            {
                slice = _input.SliceBetween(start, _position);
                val = ParseIntToBigInteger(slice, radix: 10);
            }
            ++_position;

            if (IsIdentifierStart(FullCharCodeAtPosition()))
            {
                // Raise(_position, "Identifier directly after number"); // original acornjs error reporting
                Unexpected(start);
            }

            return FinishToken(TokenType.BigInt, val);
        }

    ParseDecimal:
        bool hasSeparator2;
        var integerPartEnd = _position;

        if (nextCh == '.')
        {
            ++_position;
            ReadInt(out _, out _, out hasSeparator2, radix: 10, allowSeparators);
            hasSeparator = hasSeparator || hasSeparator2;
            nextCh = CharCodeAtPosition();
        }

        if (nextCh is 'e' or 'E')
        {
            ++_position;
            nextCh = CharCodeAtPosition();
            if (nextCh is '+' or '-')
            {
                ++_position;
            }
            if (!(ReadInt(out _, out _, out hasSeparator2, radix: 10, allowSeparators) > 0))
            {
                // Raise(start, "Invalid number"); // original acornjs error reporting
                Unexpected(start);
            }
            hasSeparator = hasSeparator || hasSeparator2;
        }

        if (startsWithZero && _strict && numDigits > 1)
        {
            RaiseRecoverable(start, StrictDecimalWithLeadingZero);
        }

        if (!overflow && _position - integerPartEnd <= 1) // no need to reparse literals like 10.
        {
            val = (double)intValue;
        }
        else
        {
            slice = _input.SliceBetween(start, _position);
            val = ParseFloatToDouble(slice, hasSeparator, this);
        }

        if (IsIdentifierStart(FullCharCodeAtPosition()))
        {
            // Raise(_position, "Identifier directly after number"); // original acornjs error reporting
            Unexpected(start);
        }

        return FinishToken(TokenType.Number, val);
    }

    // Read a string value, interpreting backslash-escapes.
    private bool ReadString(int quote)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readString = function`

        Unsafe.SkipInit(out bool normalizeRaw);
        _legacyOctalPosition = -1;
        AcquireStringBuilder(out var sb);
        try
        {
            var start = ++_position;
            var chunkStart = start;
            for (int ch; (ch = CharCodeAtPosition()) >= 0;)
            {
                if (ch == quote)
                {
                    var value = chunkStart == start
                        ? _input.SliceBetween(chunkStart, _position)
                        : sb.Append(_input, chunkStart, _position - chunkStart).ToString().AsSpan();

                    ++_position;
                    return FinishToken(TokenType.String, DeduplicateString(value, ref _stringPool, NonIdentifierDeduplicationThreshold));
                }

                switch (ch)
                {
                    case '\\':
                        sb.Append(_input, chunkStart, _position - chunkStart);
                        if (ReadEscapedChar(sb, inTemplate: false, ref normalizeRaw) is null)
                        {
                            return false;
                        }
                        chunkStart = _position;
                        break;

                    case '\u2028' or '\u2029':
                        if (_options._ecmaVersion < EcmaVersion.ES10)
                        {
                            goto Unterminated;
                        }

                        ++_position;
                        ++_currentLine;
                        _lineStart = _position;
                        break;

                    case '\n' or '\r':
                        goto Unterminated;

                    default:
                        ++_position;
                        break;
                }
            }
        }
        finally { ReleaseStringBuilder(ref sb); }

    Unterminated:
        // return Raise<bool>(_start, "Unterminated string constant"); // original acornjs error reporting
        return Unexpected<bool>();
    }

    // Read a string value, interpreting backslash-escapes.
    private int ReadCodePoint()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readCodePoint = function`

        var ch = CharCodeAtPosition();
        if (ch == '{')
        {
            var start = _position;
            if (_options._ecmaVersion < EcmaVersion.ES6)
            {
                // Unexpected(); // original acornjs error reporting
                Raise(start - 2, InvalidUnicodeEscapeSequence);
            }

            ++_position;
            var len = _input.IndexOf('}', _position, _endPosition - _position) - _position;
            var cp = ReadHexChar(len > 0 ? len : -1, start, isVariableLength: true);
            if ((uint)cp > UnicodeHelper.LastCodePoint)
            {
                if (cp > 0)
                {
                    // InvalidStringToken(errorPos, "Code point out of bounds"); // original acornjs error reporting
                    InvalidStringToken(start - 2, UndefinedUnicodeCodePoint);
                }

                // Propagate escape sequence error to caller when parsing a template literal.
                return -1;
            }

            ++_position;
            return cp;
        }
        else
        {
            return ReadHexChar(4, _position);
        }
    }

    // Reads template string tokens.

    internal bool TryReadTemplateToken()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.tryReadTemplateToken = function`

        _inTemplateElement = true;

        var normalizeRaw = false;
        var success = ReadTemplateToken(ref normalizeRaw, out var invalidTemplate)
            && (!invalidTemplate || ReadInvalidTemplateToken(ref normalizeRaw));

        _inTemplateElement = false;

        return success;
    }

    private void InvalidStringToken(int pos, string message, [CallerArgumentExpression(nameof(message))] string code = UnknownError)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.invalidStringToken = function`

        if (_inTemplateElement && _options._ecmaVersion >= EcmaVersion.ES9)
        {
            if (_requireValidEscapeSequenceInTemplate)
            {
                RaiseRecoverable(pos, message, code: code);
            }

            // NOTE: Original acornjs implementation uses a custom exception (INVALID_TEMPLATE_ESCAPE_ERROR) to
            // propagate escape sequence error in template literals up the call stack to ReadTemplateToken.
            // Exceptions are expensive in .NET and generally considered an antipattern to control flow,
            // so the implementation was changed to propagate the error using method returns.
            return;
        }

        Raise(pos, message, code: code);
    }

    private bool ReadTemplateToken(ref bool normalizeRaw, out bool invalidTemplate)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readTmplToken = function`

        invalidTemplate = false;
        _legacyOctalPosition = -1;
        AcquireStringBuilder(out var sb);
        try
        {
            var start = _position;
            var chunkStart = start;
            for (int ch; (ch = CharCodeAtPosition()) >= 0;)
            {
                switch (ch)
                {
                    case '`':
                    case '$' when CharCodeAtPosition(1) == '{':
                        if (_position == _start && _type.Kind == TokenKind.Template)
                        {
                            if (ch == '$')
                            {
                                _position += 2;
                                return FinishToken(TokenType.DollarBraceLeft, "${");
                            }
                            else
                            {
                                ++_position;
                                return FinishToken(TokenType.BackQuote, ((char)ch).ToStringCached());
                            }
                        }

                        var value = chunkStart == start
                            ? _input.SliceBetween(chunkStart, _position)
                            : sb.Append(_input, chunkStart, _position - chunkStart).ToString().AsSpan();

                        var templateCooked = DeduplicateString(value, ref _stringPool, NonIdentifierDeduplicationThreshold);

                        sb.Clear();
                        var templateRaw = DeduplicateString(ReadTemplateRaw(sb, normalizeRaw), ref _stringPool, NonIdentifierDeduplicationThreshold);

                        return FinishToken(TokenType.Template, new TemplateValue(templateCooked, templateRaw));

                    case '\\':
                        sb.Append(_input, chunkStart, _position - chunkStart);
                        if (ReadEscapedChar(sb, inTemplate: true, ref normalizeRaw) is null)
                        {
                            invalidTemplate = true;
                            return true;
                        }
                        chunkStart = _position;
                        break;

                    case '\r':
                        normalizeRaw = true;

                        ++_position;
                        sb.Append(_input, chunkStart, _position - chunkStart);
                        sb[sb.Length - 1] = '\n';

                        if (CharCodeAtPosition() == '\n')
                        {
                            ++_position;
                        }

                        goto FinishNewLine;

                    case '\n':
                    case '\u2028' or '\u2029':
                        ++_position;
                        sb.Append(_input, chunkStart, _position - chunkStart);

                    FinishNewLine:
                        ++_currentLine;
                        _lineStart = _position;

                        chunkStart = _position;
                        break;

                    default:
                        ++_position;
                        break;
                }
            }
        }
        finally { ReleaseStringBuilder(ref sb); }

        // return Raise<bool>(_start, "Unterminated template"); // original acornjs error reporting
        return Raise<bool>(_position, UnexpectedEOS);
    }

    // Reads a template token to search for the end, without validating any escape sequences
    private bool ReadInvalidTemplateToken(ref bool normalizeRaw)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readInvalidTemplateToken = function`

        for (int ch; (ch = CharCodeAtPosition()) >= 0;)
        {
            switch (ch)
            {
                case '\\':
                    ++_position;
                    break;

                case '$':
                    if (CharCodeAtPosition(1) != '{')
                    {
                        break;
                    }

                    // falls through
                    goto case '`';

                case '`':
                    AcquireStringBuilder(out var sb);
                    try
                    {
                        var templateRaw = DeduplicateString(ReadTemplateRaw(sb, normalizeRaw), ref _stringPool, NonIdentifierDeduplicationThreshold);

                        return FinishToken(TokenType.InvalidTemplate, new TemplateValue(null, templateRaw));
                    }
                    finally { ReleaseStringBuilder(ref sb); }

                case '\r':
                    normalizeRaw = true;

                    if (CharCodeAtPosition(1) == '\n')
                    {
                        ++_position;
                    }

                    goto case '\n';

                case '\n':
                case '\u2028' or '\u2029':
                    ++_position;
                    ++_currentLine;
                    _lineStart = _position;
                    continue;
            }

            _position++;
        }

        // return Raise<bool>(_start, "Unterminated template"); // original acornjs error reporting
        return Raise<bool>(_position, UnexpectedEOS);
    }

    private ReadOnlySpan<char> ReadTemplateRaw(StringBuilder sb, bool normalizeRaw)
    {
        var chunkStart = _start;
        if (normalizeRaw)
        {
            for (int index; (index = _input.IndexOf('\r', chunkStart, _position - chunkStart)) >= 0;)
            {
                sb.Append(_input, chunkStart, index - chunkStart).Append('\n');
                chunkStart = index + 1;
                if (_input.CharCodeAt(index + 1) == '\n')
                {
                    chunkStart++;
                }
            }

            if (chunkStart != _start)
            {
                return sb.Append(_input, chunkStart, _position - chunkStart).ToString().AsSpan();
            }
        }
        return _input.SliceBetween(chunkStart, _position);
    }

    // Used to read escaped characters
    private StringBuilder? ReadEscapedChar(StringBuilder sb, bool inTemplate, ref bool normalizeRaw)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readEscapedChar = function`

        ++_position;
        var ch = CharCodeAtPosition();
        ++_position;
        switch (ch)
        {
            case 'n': return sb.Append('\n');
            case 'r': return sb.Append('\r');
            case 'x': return (ch = ReadHexChar(2, _position)) >= 0 ? sb.Append((char)ch) : null;
            case 'u': return (ch = ReadCodePoint()) >= 0 ? sb.AppendCodePoint(ch) : null;
            case 't': return sb.Append('\t');
            case 'b': return sb.Append('\b');
            case 'v': return sb.Append('\v');
            case 'f': return sb.Append('\f');

            case '\r':
                normalizeRaw = true;

                if (CharCodeAtPosition() == '\n') // '\r\n'
                {
                    ++_position;
                }

                goto case '\n';

            case '\n':
            // Unicode new line characters after \ get removed from output in both
            // template literals and strings
            case '\u2028' or '\u2029':
                ++_currentLine;
                _lineStart = _position;

                return sb;

            case >= '0' and <= '7':
                var start = --_position;
                var end = Math.Min(start + 3, _endPosition);
                var octal = 0;
                do
                {
                    var val = (ushort)((octal << 3) + (ch - '0'));
                    if (val > 0xFF)
                    {
                        break;
                    }
                    octal = val;
                    ++_position;
                    ch = _input.CharCodeAt(_position, end);
                }
                while (((char)ch).IsOctalDigit());

                if (octal != 0 || _position - start != 1 || ch is '8' or '9')
                {
                    // Original acornjs error reporting
                    //if (_strict || inTemplate)
                    //{
                    //    InvalidStringToken(_position - 1, inTemplate
                    //        ? "Octal literal in template string"
                    //        : "Octal literal in strict mode");
                    //    return null;
                    //}

                    if (inTemplate)
                    {
                        InvalidStringToken(start - 1, TemplateOctalLiteral);
                        return null;
                    }
                    else if (_strict)
                    {
                        RaiseRecoverable(start - 1, StrictOctalEscape);
                    }

                    if (_legacyOctalPosition < 0)
                    {
                        _legacyOctalPosition = start - 1;
                    }
                }

                return sb.Append((char)octal);

            case '8' or '9':
                // Original acornjs error reporting
                //if (_strict || inTemplate)
                //{
                //    InvalidStringToken(_position - 1, inTemplate
                //        ? "Invalid escape sequence in template string"
                //        : "Invalid escape sequence");
                //}

                if (inTemplate)
                {
                    InvalidStringToken(_position - 2, Template8Or9Escape);
                    return null;
                }
                else if (_strict)
                {
                    RaiseRecoverable(_position - 2, Strict8Or9Escape);
                }

                if (_legacyOctalPosition < 0)
                {
                    _legacyOctalPosition = _position - 2;
                }

                goto default;

            default:
                return sb.Append((char)ch);
        }
    }

    // Used to read character escape sequences ('\x', '\u', '\u{').
    private int ReadHexChar(int len, int start, bool isVariableLength = false)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readHexChar = function`
        if (ReadInt(out var n, out var overflow, out _, radix: 16, len: len) != len)
        {
            // InvalidStringToken(errorPos, "Bad character escape sequence"); // original acornjs error reporting
            if (!isVariableLength && len == 2)
            {
                InvalidStringToken(start - 2, InvalidHexEscapeSequence);
            }
            else
            {
                InvalidStringToken(start - 2, InvalidUnicodeEscapeSequence);
            }
            return -1;
        }
        else if (overflow || n > int.MaxValue)
        {
            return int.MaxValue;
        }

        return (int)n;
    }

    // Read an identifier, and return it as a string. Sets `ContainsEscape`
    // to whether the word contained a '\u' escape.
    //
    // Incrementally adds only escaped chars, adding other chunks as-is
    // as a micro-optimization.
    private ReadOnlySpan<char> ReadWord1()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readWord1 = function`

        _containsEscape = false;
        AcquireStringBuilder(out var sb);
        try
        {
            var first = true;
            var chunkStart = _position;
            var astral = _options._ecmaVersion >= EcmaVersion.ES6;

            for (int cp; (cp = FullCharCodeAtPosition()) >= 0;)
            {
                if (IsIdentifierChar(cp, astral))
                {
                    _position += UnicodeHelper.GetCodePointLength((uint)cp);
                }
                else if (cp == '\\')
                {
                    _containsEscape = true;
                    sb.Append(_input, chunkStart, _position - chunkStart);
                    ++_position;
                    if (CharCodeAtPosition() != 'u')
                    {
                        // InvalidStringToken(_position, "Expecting Unicode escape sequence \\uXXXX"); // original acornjs error reporting
                        Unexpected();
                    }

                    ++_position;
                    var esc = ReadCodePoint();
                    if (first
                        ? !IsIdentifierStart(esc, astral)
                        : !IsIdentifierChar(esc, astral))
                    {
                        // InvalidStringToken(escStart, "Invalid Unicode escape"); // original acornjs error reporting
                        Unexpected();
                    }

                    sb.AppendCodePoint(esc);
                    chunkStart = _position;
                }
                else
                {
                    break;
                }

                first = false;
            }

            return !_containsEscape
                ? _input.SliceBetween(chunkStart, _position)
                : sb.Append(_input, chunkStart, _position - chunkStart).ToString().AsSpan();
        }
        finally { ReleaseStringBuilder(ref sb); }
    }

    // Read an identifier or keyword token. Will check for reserved
    // words when necessary.
    private bool ReadWord()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokenize.js > `pp.readWord = function`

        var word = ReadWord1();

        var tokenType = TokenType.GetKeywordBy(word);

        return tokenType is not null && tokenType.EcmaVersion <= _options._ecmaVersion
            ? FinishToken(tokenType, new TokenValue(tokenType.Value))
            : FinishToken(TokenType.Name, DeduplicateString(word, ref _stringPool));
    }

    #endregion

    private bool InGeneratorContext()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `pp.inGeneratorContext = function`

        for (var i = _contextStack.Count - 1; i >= 1; i--)
        {
            var context = _contextStack[i];
            if (context.Kind == TokenContextKind.Function)
            {
                return context.Generator;
            }
        }

        return false;
    }

    internal void UpdateContextMinimal(TokenType previousType)
    {
        // The minimal context tracking necessary for dealing with nested template literals.
        // Should be used when the tokenizer doesn't need to deal with regex disambiguation
        // (i.e. when the tokenizer is not used in standalone mode and the user doesn't care about tokens either).

        if (_type == TokenType.BraceLeft)
        {
            _contextStack.Push(TokenContext.BracketsInStatement);
        }
        else if (_type == TokenType.BraceRight)
        {
            if (_contextStack.Count > 0)
            {
                _contextStack.Pop();
            }
        }
        else if (_type == TokenType.BackQuote)
        {
            if (CurrentContext == TokenContext.QuoteInTemplate)
            {
                _contextStack.Pop();
            }
            else
            {
                _contextStack.Push(TokenContext.QuoteInTemplate);
            }
        }
        else if (_type == TokenType.DollarBraceLeft)
        {
            _contextStack.Push(TokenContext.BracketsInTemplate);
        }
    }

    internal void UpdateContext(TokenType previousType)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `pp.updateContext = function`

        var currentType = _type;
        if (currentType.Keyword is not null && previousType == TokenType.Dot)
        {
            _expressionAllowed = false;
        }
        else if (currentType.UpdateContext is { } update)
        {
            update(this, previousType);
        }
        else
        {
            _expressionAllowed = currentType.BeforeExpression;
        }
    }

    // Used to handle edge cases when token context could not be inferred correctly during tokenization phase
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void OverrideContext(TokenContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `pp.overrideContext = function`

        if (_trackRegExpContext)
        {
            _contextStack.PeekRef() = context;
        }
    }

    #region Token-specific context update code (TokenType.UpdateContext implementations)

    internal static void UpdateContext_ParenOrBraceRight(Tokenizer tokenizer, TokenType previousType)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `tt.parenR.updateContext = tt.braceR.updateContext = function`

        if (tokenizer._contextStack.Count == 1)
        {
            tokenizer._expressionAllowed = true;
            return;
        }

        var @out = tokenizer._contextStack.Pop();
        if (@out == TokenContext.BracketsInStatement && tokenizer.CurrentContext.Kind == TokenContextKind.Function)
        {
            @out = tokenizer._contextStack.Pop();
        }

        tokenizer._expressionAllowed = !@out.IsExpression;
    }

    internal static void UpdateContext_BraceLeft(Tokenizer tokenizer, TokenType previousType)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `tt.braceL.updateContext = function`

        tokenizer._contextStack.Push(BraceIsBlock(tokenizer, previousType) ? TokenContext.BracketsInStatement : TokenContext.BracketsInExpression);
        tokenizer._expressionAllowed = true;

        static bool BraceIsBlock(Tokenizer tokenizer, TokenType previousType)
        {
            //  https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `pp.braceIsBlock = function`

            var parent = tokenizer.CurrentContext;
            if (parent == TokenContext.FunctionInExpression || parent == TokenContext.FunctionInStatement)
            {
                return true;
            }

            if (previousType == TokenType.Colon && (parent == TokenContext.BracketsInStatement || parent == TokenContext.BracketsInExpression))
            {
                return !parent.IsExpression;
            }

            // The check for `TokenType.Name && ExpressionAllowed` detects whether we are
            // after a `yield` or `of` construct. See `UpdateContext_Name` for `TokenType.Name`.
            if (previousType == TokenType.Return || previousType == TokenType.Name && tokenizer._expressionAllowed)
            {
                return tokenizer._lastTokenEndLocation.Line != tokenizer._startLocation.Line;
            }

            if (previousType == TokenType.Else || previousType == TokenType.Semicolon || previousType == TokenType.EOF || previousType == TokenType.ParenRight || previousType == TokenType.Arrow)
            {
                return true;
            }

            if (previousType == TokenType.BraceLeft)
            {
                return parent == TokenContext.BracketsInStatement;
            }

            if (previousType == TokenType.Var || previousType == TokenType.Const || previousType == TokenType.Name)
            {
                return false;
            }

            return !tokenizer._expressionAllowed;
        }
    }

    internal static void UpdateContext_DollarBraceLeft(Tokenizer tokenizer, TokenType previousType)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `tt.dollarBraceL.updateContext = function`

        tokenizer._contextStack.Push(TokenContext.BracketsInTemplate);
        tokenizer._expressionAllowed = true;
    }

    internal static void UpdateContext_ParenLeft(Tokenizer tokenizer, TokenType previousType)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `tt.parenL.updateContext = function`

        var statementParens = previousType == TokenType.If || previousType == TokenType.For || previousType == TokenType.With || previousType == TokenType.While;
        tokenizer._contextStack.Push(statementParens ? TokenContext.ParensInStatement : TokenContext.ParensInExpression);
        tokenizer._expressionAllowed = true;
    }

    internal static void UpdateContext_IncDec(Tokenizer tokenizer, TokenType previousType)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `tt.incDec.updateContext = function`

        // tokExprAllowed stays unchanged
    }

    internal static void UpdateContext_FunctionOrClass(Tokenizer tokenizer, TokenType previousType)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `tt._function.updateContext = tt._class.updateContext = function`

        if (previousType.BeforeExpression && previousType != TokenType.Else
            && !(previousType == TokenType.Semicolon && tokenizer.CurrentContext != TokenContext.ParensInStatement)
            && !(previousType == TokenType.Return && tokenizer._lastTokenEndLocation.Line != tokenizer._startLocation.Line)
            && !((previousType == TokenType.Colon || previousType == TokenType.BraceLeft) && tokenizer.CurrentContext == TokenContext.BracketsInStatement))
        {
            tokenizer._contextStack.Push(TokenContext.FunctionInExpression);
        }
        else
        {
            tokenizer._contextStack.Push(TokenContext.FunctionInStatement);
        }

        tokenizer._expressionAllowed = false;
    }

    internal static void UpdateContext_Colon(Tokenizer tokenizer, TokenType previousType)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `tt.colon.updateContext = function`

        if (tokenizer.CurrentContext.Kind == TokenContextKind.Function)
        {
            tokenizer._contextStack.Pop();
        }

        tokenizer._expressionAllowed = true;
    }

    internal static void UpdateContext_BackQuote(Tokenizer tokenizer, TokenType previousType)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `tt.backQuote.updateContext = function`

        if (tokenizer.CurrentContext == TokenContext.QuoteInTemplate)
        {
            tokenizer._contextStack.Pop();
        }
        else
        {
            tokenizer._contextStack.Push(TokenContext.QuoteInTemplate);
        }

        tokenizer._expressionAllowed = false;
    }

    internal static void UpdateContext_Star(Tokenizer tokenizer, TokenType previousType)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `tt.star.updateContext = function`

        if (previousType == TokenType.Function)
        {
            var @out = tokenizer._contextStack.Pop();
            if (@out == TokenContext.FunctionInExpression)
            {
                tokenizer._contextStack.Push(TokenContext.GeneratorFunctionInExpression);
            }
            else
            {
                tokenizer._contextStack.Push(TokenContext.GeneratorFunctionInStatement);
            }
        }

        tokenizer._expressionAllowed = true;
    }

    internal static void UpdateContext_Name(Tokenizer tokenizer, TokenType previousType)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js > `tt.name.updateContext = function`

        var allowed = false;
        if (tokenizer._options._ecmaVersion >= EcmaVersion.ES6 && previousType != TokenType.Dot && tokenizer._value.Value is string value)
        {
            if (value == "of" && !tokenizer._expressionAllowed ||
                value == "yield" && tokenizer.InGeneratorContext())
            {
                allowed = true;
            }
        }

        tokenizer._expressionAllowed = allowed;
    }

    #endregion

    // Raise an unexpected token error.
    [DoesNotReturn]
    internal void Unexpected(int? pos = null)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.unexpected = function`

        Raise(pos ?? _start, InvalidOrUnexpectedToken);
    }

    [DoesNotReturn]
    internal T Unexpected<T>(int? pos = null)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.unexpected = function`

        Unexpected(pos);
        return default!;
    }

    // This function is used to raise exceptions on parse errors. It
    // takes an offset integer (into the current `input`) to indicate
    // the location of the error, attaches the position to the end
    // of the error message, and then raises a `SyntaxError` with that
    // message.
    [DoesNotReturn]
    internal void Raise(int pos, string message, ParseError.Factory? errorFactory = null,
        [CallerArgumentExpression(nameof(message))] string code = UnknownError)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/location.js > `pp.raise = function`

        var loc = GetLineInfo(_input, pos, out _);
        var error = errorFactory is null
            ? new SyntaxError(code, message, pos, loc, _sourceFile)
            : errorFactory(code, message, pos, loc, _sourceFile);
        _options._errorHandler.ThrowError(error);
    }

    [DoesNotReturn]
    internal T Raise<T>(int pos, string message, ParseError.Factory? errorFactory = null,
        [CallerArgumentExpression(nameof(message))] string code = UnknownError)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/location.js > `pp.raise = function`

        Raise(pos, message, errorFactory, code);
        return default!;
    }

    internal ParseError RaiseRecoverable(int pos, string message, ParseError.Factory? errorFactory = null,
        [CallerArgumentExpression(nameof(message))] string code = UnknownError)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/location.js > `pp.raiseRecoverable =`

        var loc = GetLineInfo(_input, pos, out _);
        var error = errorFactory is null
            ? new SyntaxError(code, message, pos, loc, _sourceFile)
            : errorFactory(code, message, pos, loc, _sourceFile);
        return _options._errorHandler.TolerateError(error, _options._tolerant);
    }

    /// <summary>
    /// Checks whether an ECMAScript regular expression is syntactically correct.
    /// </summary>
    /// <remarks>
    /// Unicode sets mode (flag v) is not supported currently, for such patterns the method returns <see langword="false"/>.
    /// </remarks>
    /// <returns><see langword="true"/> if the regular expression is syntactically correct, otherwise <see langword="false"/>.</returns>
    public static bool ValidateRegExp(string pattern, string flags, out ParseError? error,
        EcmaVersion ecmaVersion = EcmaVersion.Latest, ExperimentalESFeatures experimentalESFeatures = ExperimentalESFeatures.None)
    {
        if (pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern));
        }

        if (flags is null)
        {
            throw new ArgumentNullException(nameof(flags));
        }

        Debug.Assert(TokenizerOptions.Default is { RegExpParseMode: RegExpParseMode.Validate, Tolerant: false, EcmaVersion: EcmaVersion.Latest, ExperimentalESFeatures: ExperimentalESFeatures.None });

        var tokenizerOptions = ecmaVersion == EcmaVersion.Latest
            ? TokenizerOptions.Default
            : new TokenizerOptions { EcmaVersion = ecmaVersion, ExperimentalESFeatures = experimentalESFeatures };

        try
        {
            var parseResult = new RegExpParser(pattern, flags, tokenizerOptions).Parse();
            Debug.Assert(parseResult.Success);
        }
        catch (ParseErrorException ex)
        {
            error = ex.Error;
            return false;
        }

        error = default;
        return true;
    }

    /// <summary>
    /// Parses an ECMAScript regular expression and tries to construct a <see cref="Regex"/> instance with the equivalent behavior.
    /// </summary>
    /// <remarks>
    /// Please note that, because of some fundamental differences between the ECMAScript and .NET regular expression engines,
    /// not every ECMAScript regular expression can be converted to an equivalent <see cref="Regex"/> (or can be converted with compromises only).<br/>
    /// You can read more about the known issues and limitations of the conversion <see href="https://github.com/adams85/acornima#regular-expressions">here</see>.
    /// </remarks>
    /// <returns>
    /// An instance of <see cref="RegExpParseResult"/>, whose <see cref="RegExpParseResult.Regex"/> property contains the equivalent <see cref="Regex"/> if the conversion was possible,
    /// otherwise <see langword="null"/> (unless <paramref name="throwIfNotAdaptable"/> is <see langword="true"/>).
    /// </returns>
    /// <exception cref="SyntaxErrorException">
    /// <paramref name="pattern"/> is an invalid regular expression pattern (if <paramref name="throwIfNotAdaptable"/> is <see langword="true"/>).
    /// </exception>
    /// <exception cref="RegExpConversionErrorException">
    /// <paramref name="pattern"/> cannot be converted to an equivalent <see cref="Regex"/> (if <paramref name="throwIfNotAdaptable"/> is <see langword="true"/>).
    /// </exception>
    public static RegExpParseResult AdaptRegExp(string pattern, string flags, bool compiled = false, TimeSpan? matchTimeout = null, bool throwIfNotAdaptable = false,
        EcmaVersion ecmaVersion = EcmaVersion.Latest, ExperimentalESFeatures experimentalESFeatures = ExperimentalESFeatures.None)
    {
        if (pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern));
        }

        if (flags is null)
        {
            throw new ArgumentNullException(nameof(flags));
        }

        var tokenizerOptions = new TokenizerOptions
        {
            EcmaVersion = ecmaVersion,
            ExperimentalESFeatures = experimentalESFeatures,
            RegExpParseMode = !compiled ? RegExpParseMode.AdaptToInterpreted : RegExpParseMode.AdaptToCompiled,
            RegexTimeout = matchTimeout ?? TokenizerOptions.Default.RegexTimeout,
            Tolerant = !throwIfNotAdaptable,
        };

        return new RegExpParser(pattern, flags, tokenizerOptions).Parse();
    }

    internal interface IExtension
    {
        bool SupportsMinimalContextTracking { get; }

        void ReadToken(TokenContext currentContext);
        void UpdateContext(TokenType previousType);
    }
}
