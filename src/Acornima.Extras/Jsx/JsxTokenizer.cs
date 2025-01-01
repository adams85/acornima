using System;
using System.Text;
using Acornima.Helpers;

namespace Acornima.Jsx;

using static Tokenizer;
using static SyntaxErrorMessages;
using static JsxSyntaxErrorMessages;

// https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js

public sealed class JsxTokenizer : ITokenizer, IExtension
{
    private readonly JsxTokenizerOptions _options;
    internal readonly Tokenizer _tokenizer;

    internal JsxTokenizer(JsxTokenizerOptions options)
    {
        _options = options;
        _tokenizer = new Tokenizer(options, extension: this);
    }

    public JsxTokenizer(string input)
        : this(input, JsxTokenizerOptions.Default) { }

    public JsxTokenizer(string input, JsxTokenizerOptions options)
        : this(input, SourceType.Script, sourceFile: null, options) { }

    public JsxTokenizer(string input, SourceType sourceType, string? sourceFile, JsxTokenizerOptions options)
        : this(input ?? throw new ArgumentNullException(nameof(input)), 0, input.Length, sourceType, sourceFile, options) { }

    public JsxTokenizer(string input, int start, int length, SourceType sourceType, string? sourceFile, JsxTokenizerOptions options)
        : this(options)
    {
        _tokenizer.ResetInternal(input, start, length, sourceType, sourceFile, trackRegExpContext: true);
    }

    bool IExtension.SupportsMinimalContextTracking => false;

    public string Input => _tokenizer.Input;
    public Range Range => _tokenizer.Range;
    public SourceType SourceType => _tokenizer.SourceType;
    public string? SourceFile => _tokenizer.SourceFile;

    public JsxTokenizerOptions Options => _options;
    TokenizerOptions ITokenizer.Options => _options;

    public Token Current => _tokenizer.Current;

    public Token GetToken(in TokenizerContext context = default) => _tokenizer.GetToken();

    public void Next(in TokenizerContext context = default) => _tokenizer.Next();

    public void Reset(string input, int start, int length, SourceType sourceType = SourceType.Script, string? sourceFile = null)
        => _tokenizer.Reset(input, start, length, sourceType, sourceFile);

    void IExtension.ReadToken(TokenContext currentContext)
    {
        if (currentContext != TokenContext.QuoteInTemplate
            ? !TryReadToken(_tokenizer.FullCharCodeAtPosition(), currentContext)
            : !_tokenizer.TryReadTemplateToken())
        {
            // Raise(start, $"Unexpected character '{UnicodeHelper.CodePointToString(cp)}'"); // original acornjs error reporting
            _tokenizer.Unexpected();
        }
    }

    private bool TryReadToken(int cp, TokenContext currentContext)
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `readToken(code) {`

        if (currentContext == JsxTokenContext.InExpression)
        {
            return ReadContent();
        }

        if (currentContext == JsxTokenContext.InOpeningTag || currentContext == JsxTokenContext.InClosingTag)
        {
            if (Tokenizer.IsIdentifierStart(cp))
            {
                return ReadWord();
            }

            if (cp == '>')
            {
                ++_tokenizer._position;
                return _tokenizer.FinishToken(JsxTokenType.TagEnd, ((char)cp).ToStringCached());
            }

            if ((cp == '"' || cp == '\'') && currentContext == JsxTokenContext.InOpeningTag)
            {
                return ReadString(quote: cp);
            }
        }

        if (cp == '<' && _tokenizer._expressionAllowed && _tokenizer.CharCodeAtPosition(1) != '!')
        {
            ++_tokenizer._position;
            return _tokenizer.FinishToken(JsxTokenType.TagStart, ((char)cp).ToStringCached());
        }

        return _tokenizer.TryReadToken(cp);
    }

    // Reads inline JSX contents token.

    private bool ReadContent()
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_readToken() {`

        _tokenizer.AcquireStringBuilder(out var sb);
        try
        {
            ref readonly var input = ref _tokenizer._input;
            ref var position = ref _tokenizer._position;
            var start = position;
            var chunkStart = start;

            for (int ch; (ch = _tokenizer.CharCodeAtPosition()) >= 0;)
            {
                switch (ch)
                {
                    case '<':
                    case '{':
                        if (position == start)
                        {
                            if (ch == '<' && _tokenizer._expressionAllowed)
                            {
                                ++position;
                                return _tokenizer.FinishToken(JsxTokenType.TagStart, ((char)ch).ToStringCached());
                            }
                            else
                            {
                                return _tokenizer.TryReadToken(ch);
                            }
                        }
                        var @out = chunkStart == start
                            ? input.SliceBetween(chunkStart, position)
                            : sb.Append(input, chunkStart, position - chunkStart).ToString().AsSpan();
                        return _tokenizer.FinishToken(JsxTokenType.Text, JsxToken.TextValue(Tokenizer.DeduplicateString(@out, ref _tokenizer._stringPool, Tokenizer.NonIdentifierDeduplicationThreshold)));

                    case '&':
                        sb.Append(input, chunkStart, position - chunkStart);
                        ReadEntity(sb, ch);
                        chunkStart = position;
                        break;

                    case '>':
                    case '}':
                        // _tokenizer.Raise(position, $"Unexpected token `{input[position]}`. Did you mean `{(ch == '>' ? "&gt;" : "&rbrace;")}` or `{{\"{input[position]}\"}}`?");  // original acornjs error reporting
                        _tokenizer.Unexpected(position);
                        break;

                    case '\n' or '\r' or '\u2028' or '\u2029':
                        sb.Append(input, chunkStart, position - chunkStart);
                        ReadNewLine(sb, ch, normalizeCRLF: true);
                        chunkStart = position;
                        break;

                    default:
                        ++position;
                        break;
                }
            }
        }
        finally { _tokenizer.ReleaseStringBuilder(ref sb); }

        // return _tokenizer.Raise<bool>(_tokenizer._start, 'Unterminated JSX contents'); // original acornjs error reporting
        return _tokenizer.Raise<bool>(_tokenizer._start, JsxUnterminatedContent);
    }

    private bool ReadString(int quote)
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_readString(quote) {`

        _tokenizer.AcquireStringBuilder(out var sb);
        try
        {
            ref readonly var input = ref _tokenizer._input;
            ref var position = ref _tokenizer._position;
            var start = ++position;
            var chunkStart = start;

            for (int ch; (ch = _tokenizer.CharCodeAtPosition()) >= 0;)
            {
                if (ch == quote)
                {
                    var value = chunkStart == start
                        ? input.SliceBetween(chunkStart, position)
                        : sb.Append(input, chunkStart, position - chunkStart).ToString().AsSpan();

                    ++position;
                    return _tokenizer.FinishToken(TokenType.String, Tokenizer.DeduplicateString(value, ref _tokenizer._stringPool, Tokenizer.NonIdentifierDeduplicationThreshold));
                }

                switch (ch)
                {
                    case '&':
                        sb.Append(input, chunkStart, position - chunkStart);
                        ReadEntity(sb, ch);
                        chunkStart = position;
                        break;

                    case '\n' or '\r' or '\u2028' or '\u2029':
                        sb.Append(input, chunkStart, position - chunkStart);
                        ReadNewLine(sb, ch, normalizeCRLF: false);
                        chunkStart = position;
                        break;

                    default:
                        ++position;
                        break;
                }
            }
        }
        finally { _tokenizer.ReleaseStringBuilder(ref sb); }

        // return Raise<bool>(_start, "Unterminated string constant"); // original acornjs error reporting
        return _tokenizer.Unexpected<bool>();
    }

    // Read a JSX identifier (valid tag or attribute name).
    //
    // Optimized version since JSX identifiers can't contain
    // escape characters and so can be read as single slice.
    // Also assumes that first character was already checked
    // by isIdentifierStart in readToken.

    private bool ReadWord()
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_readWord() {`

        ref var position = ref _tokenizer._position;
        var start = position++;
        for (int ch; Tokenizer.IsIdentifierChar(ch = _tokenizer.CharCodeAtPosition()) || ch == '-';)
        {
            ++position;
        }
        var word = _tokenizer._input.AsSpan(start, position - start);
        return _tokenizer.FinishToken(JsxTokenType.Name, JsxToken.IdentifierValue(Tokenizer.DeduplicateString(word, ref _tokenizer._stringPool)));
    }

    private void ReadEntity(StringBuilder sb, int ch)
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_readEntity() {`

        // The acornjs implementation has some issues around handling edge cases of code point parsing, so we adapt the approach of Babel:
        // https://github.com/babel/babel/blob/v7.24.4/packages/babel-parser/src/plugins/jsx/index.ts#L194

        ref readonly var input = ref _tokenizer._input;
        ref var position = ref _tokenizer._position;
        var startPos = ++position;
        if (_tokenizer.CharCodeAtPosition() == '#')
        {
            ++position;

            byte radix = 10;
            if (_tokenizer.CharCodeAtPosition() == 'x')
            {
                radix = 16;
                ++position;
            }

            if (_tokenizer.ReadInt(out var cp, out var overflow, out _, radix) > 0 && _tokenizer.CharCodeAtPosition() == ';')
            {
                if (overflow || cp > UnicodeHelper.LastCodePoint)
                {
                    _tokenizer.Raise(startPos - 1, UndefinedUnicodeCodePoint);
                }

                ++position;
                sb.AppendCodePoint((int)cp);
                return;
            }
        }
        else
        {
            for (var i = 0; i <= Xhtml.EntityMaxLength && (ch = _tokenizer.CharCodeAtPosition()) >= 0; i++)
            {
                ++position;
                if (ch == ';')
                {
                    if (Xhtml.Entities.TryGetValue(input.AsMemory(startPos, i), out var entity))
                    {
                        sb.Append(entity);
                        return;
                    }
                    break;
                }
            }
        }

        position = startPos;
        sb.Append('&');
    }

    private void ReadNewLine(StringBuilder sb, int ch, bool normalizeCRLF = false)
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_readNewLine(normalizeCRLF) {`

        ref var position = ref _tokenizer._position;
        ++position;

        if (ch == '\r')
        {
            if (_tokenizer.CharCodeAtPosition() == '\n')
            {
                if (!normalizeCRLF)
                {
                    sb.Append('\r');
                }
                ch = '\n';
                ++position;
            }
        }

        sb.Append((char)ch);

        ++_tokenizer._currentLine;
        _tokenizer._lineStart = position;
    }

    void IExtension.UpdateContext(TokenType previousType)
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `updateContext(prevType) {`

        if (_tokenizer._type == TokenType.BraceLeft)
        {
            var currentContext = _tokenizer.CurrentContext;
            if (currentContext == JsxTokenContext.InOpeningTag)
            {
                _tokenizer._contextStack.Push(TokenContext.BracketsInExpression);
                _tokenizer._expressionAllowed = true;
                return;
            }
            else if (currentContext == JsxTokenContext.InExpression)
            {
                _tokenizer._contextStack.Push(TokenContext.BracketsInTemplate);
                _tokenizer._expressionAllowed = true;
                return;
            }
        }
        else if (_tokenizer._type == TokenType.Slash && previousType == JsxTokenType.TagStart)
        {
            _tokenizer._contextStack.Pop();
            // do not consider JSX expr -> JSX open tag -> ... anymore
            // reconsider as closing tag context
            _tokenizer._contextStack.PeekRef() = JsxTokenContext.InClosingTag;
            _tokenizer._expressionAllowed = true;
            return;
        }

        _tokenizer.UpdateContext(previousType);
    }

    #region Token-specific context update code (TokenType.UpdateContext implementations)

    internal static void UpdateContext_TagStart(Tokenizer tokenizer, TokenType previousType)
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `tokTypes.jsxTagStart.updateContext = function`

        tokenizer._contextStack.Push(JsxTokenContext.InExpression); // treat as beginning of JSX expression
        tokenizer._contextStack.Push(JsxTokenContext.InOpeningTag); // start opening tag context
        tokenizer._expressionAllowed = false;
    }

    internal static void UpdateContext_TagEnd(Tokenizer tokenizer, TokenType previousType)
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `tokTypes.jsxTagEnd.updateContext = function`

        var @out = tokenizer._contextStack.Pop();
        if (@out == JsxTokenContext.InOpeningTag && previousType == TokenType.Slash || @out == JsxTokenContext.InClosingTag)
        {
            tokenizer._contextStack.Pop();
            tokenizer._expressionAllowed = tokenizer.CurrentContext == JsxTokenContext.InExpression;
        }
        else
        {
            tokenizer._expressionAllowed = true;
        }
    }

    #endregion
}
