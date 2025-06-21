using System;

namespace Acornima.TypeScript;

using static Tokenizer;

/// <summary>
/// A TypeScript tokenizer that extends the standard JavaScript tokenizer to skip TypeScript-specific syntax.
/// This allows parsing TypeScript code as JavaScript by ignoring type annotations, generics, interfaces, etc.
/// </summary>
public sealed class TypeScriptTokenizer : ITokenizer, IExtension
{
    private readonly TypeScriptTokenizerOptions _options;
    internal readonly Tokenizer _tokenizer;

    internal TypeScriptTokenizer(TypeScriptTokenizerOptions options)
    {
        _options = options;
        _tokenizer = new Tokenizer(options, extension: this);
    }

    public TypeScriptTokenizer(string input)
        : this(input, TypeScriptTokenizerOptions.Default)
    {
    }

    public TypeScriptTokenizer(string input, TypeScriptTokenizerOptions options)
        : this(input, SourceType.Script, sourceFile: null, options)
    {
    }

    public TypeScriptTokenizer(string input, SourceType sourceType, string? sourceFile, TypeScriptTokenizerOptions options)
        : this(input ?? throw new ArgumentNullException(nameof(input)), 0, input.Length, sourceType, sourceFile, options)
    {
    }

    public TypeScriptTokenizer(string input, int start, int length, SourceType sourceType, string? sourceFile, TypeScriptTokenizerOptions options)
        : this(options)
    {
        _tokenizer.ResetInternal(input, start, length, sourceType, sourceFile, trackRegExpContext: true);
    }

    bool IExtension.SupportsMinimalContextTracking => false;

    public string Input => _tokenizer.Input;
    public Range Range => _tokenizer.Range;
    public SourceType SourceType => _tokenizer.SourceType;
    public string? SourceFile => _tokenizer.SourceFile;

    public TypeScriptTokenizerOptions Options => _options;
    TokenizerOptions ITokenizer.Options => _options;

    public Token Current => _tokenizer.Current;

    public Token GetToken(in TokenizerContext context = default) => _tokenizer.GetToken();

    public void Next(in TokenizerContext context = default) => _tokenizer.Next();

    public void Reset(string input, int start, int length, SourceType sourceType = SourceType.Script, string? sourceFile = null)
        => _tokenizer.Reset(input, start, length, sourceType, sourceFile);

    void IExtension.ReadToken(TokenContext currentContext)
    {
        if (IsInTemplateLiteralContext(currentContext))
        {
            if (currentContext == TokenContext.QuoteInTemplate
                    ? !_tokenizer.TryReadTemplateToken()
                    : !_tokenizer.TryReadToken(_tokenizer.FullCharCodeAtPosition()))
            {
                _tokenizer.Unexpected();
            }

            return;
        }

        var cp = _tokenizer.FullCharCodeAtPosition();

        bool handled = false;

        if (cp == ':' && IsTypeAnnotation(currentContext))
        {
            _tokenizer._position++;
            SkipTypeAnnotation();
            handled = true;
        }
        else if (cp == '!' && IsNonNullAssertion(currentContext))
        {
            _tokenizer._position++;
            handled = true;
        }
        else if (IsAtKeyword("as") && IsTypeAssertion(currentContext))
        {
            SkipTypeAssertion();
            handled = true;
        }
        else if (cp == '<' && IsGenericPosition())
        {
            SkipGenericParameters();
            handled = true;
        }

        if (handled)
        {
            SkipWhitespace();
            if (_tokenizer._position < _tokenizer._input.Length)
            {
                ((IExtension)this).ReadToken(currentContext);
            }
        }
        else
        {
            if (!_tokenizer.TryReadToken(cp))
            {
                _tokenizer.Unexpected();
            }
        }
    }

    private void SkipWhitespace()
    {
        while (_tokenizer._position < _tokenizer._input.Length)
        {
            var ch = _tokenizer._input[_tokenizer._position];
            if (char.IsWhiteSpace(ch))
            {
                _tokenizer._position++;
            }
            else
            {
                break;
            }
        }
    }

    void IExtension.UpdateContext(TokenType previousType)
    {
        _tokenizer.UpdateContext(previousType);
    }

    private bool IsAtKeyword(string keyword)
    {
        var pos = _tokenizer._position;
        var input = _tokenizer._input;

        if (pos + keyword.Length > input.Length)
            return false;

        for (int i = 0; i < keyword.Length; i++)
        {
            if (input[pos + i] != keyword[i])
                return false;
        }

        if (pos + keyword.Length < input.Length)
        {
            var nextChar = input[pos + keyword.Length];
            if (char.IsLetterOrDigit(nextChar) || nextChar == '_')
                return false;
        }

        if (pos > 0)
        {
            var prevChar = input[pos - 1];
            if (char.IsLetterOrDigit(prevChar) || prevChar == '_')
                return false;
        }

        return true;
    }

    private static bool IsInTemplateLiteralContext(TokenContext currentContext)
    {
        return currentContext == TokenContext.QuoteInTemplate ||
               currentContext == TokenContext.BracketsInTemplate ||
               currentContext.Kind == TokenContextKind.BackQuote ||
               currentContext.Kind == TokenContextKind.DollarBraceLeft;
    }

    private bool IsGenericPosition()
    {
        var position = _tokenizer._position - 1;
        while (position >= 0 && char.IsWhiteSpace(_tokenizer._input[position]))
            position--;

        if (position < 0)
            return false;

        var ch = _tokenizer._input[position];

        if (!char.IsLetterOrDigit(ch) && ch != '_')
            return false;

        while (position >= 0 && (char.IsLetterOrDigit(_tokenizer._input[position]) || _tokenizer._input[position] == '_'))
            position--;

        var prevPos = position;
        while (prevPos >= 0 && char.IsWhiteSpace(_tokenizer._input[prevPos]))
            prevPos--;
        if (prevPos >= 7)
        {
            var beforeIdentifier = _tokenizer._input.Substring(Math.Max(0, prevPos - 20), Math.Min(20, prevPos + 1));
            if (beforeIdentifier.Contains("function") || beforeIdentifier.Contains("class") ||
                beforeIdentifier.Contains("interface") || beforeIdentifier.Contains("type"))
            {
                return true;
            }
        }

        if (prevPos >= 0 && _tokenizer._input[prevPos] == '=')
            return true;

        return false;
    }

    private bool IsTypeAnnotation(TokenContext currentContext)
    {
        if (currentContext == TokenContext.BracketsInExpression ||
            currentContext == TokenContext.QuoteInTemplate ||
            currentContext == TokenContext.BracketsInTemplate)
        {
            return false;
        }

        var pos = _tokenizer._position - 1;
        while (pos >= 0 && char.IsWhiteSpace(_tokenizer._input[pos]))
            pos--;

        if (pos < 0)
            return false;

        var ch = _tokenizer._input[pos];

        // Type annotation after parameter names, variable names, or function signatures
        if (!char.IsLetterOrDigit(ch) && ch != ')' && ch != ']')
            return false;

        // Check if we're in an object literal context
        if (IsInObjectLiteralContext(pos))
            return false;

        var nextPos = _tokenizer._position + 1;
        while (nextPos < _tokenizer._input.Length && char.IsWhiteSpace(_tokenizer._input[nextPos]))
            nextPos++;

        if (nextPos < _tokenizer._input.Length)
        {
            var nextChar = _tokenizer._input[nextPos];
            if (char.IsLetter(nextChar) || nextChar == '{' || nextChar == '[' || nextChar == '(' || nextChar == '\'' || nextChar == '"')
                return true;
        }

        return true;
    }

    private bool IsTypeAssertion(TokenContext currentContext)
    {
        if (!IsAtKeyword("as"))
            return false;

        if (IsInTemplateLiteralContext(currentContext))
            return false;

        var pos = _tokenizer._position - 1;
        while (pos >= 0 && char.IsWhiteSpace(_tokenizer._input[pos]))
            pos--;

        if (pos < 0)
            return false;

        var ch = _tokenizer._input[pos];
        if (!char.IsLetterOrDigit(ch) && ch != ')' && ch != ']' && ch != '}')
            return false;

        var nextPos = _tokenizer._position + 2; // Skip "as"
        while (nextPos < _tokenizer._input.Length && char.IsWhiteSpace(_tokenizer._input[nextPos]))
            nextPos++;

        if (nextPos >= _tokenizer._input.Length)
            return false;

        var nextChar = _tokenizer._input[nextPos];
        return char.IsLetter(nextChar) || nextChar == '{' || nextChar == '[';
    }

    private bool IsNonNullAssertion(TokenContext currentContext)
    {
        if (IsInTemplateLiteralContext(currentContext))
            return false;

        var pos = _tokenizer._position - 1;
        while (pos >= 0 && char.IsWhiteSpace(_tokenizer._input[pos]))
            pos--;

        if (pos < 0)
            return false;

        var ch = _tokenizer._input[pos];
        if (!char.IsLetterOrDigit(ch) && ch != ')' && ch != ']' && ch != '}')
            return false;

        var nextPos = _tokenizer._position + 1;
        if (nextPos >= _tokenizer._input.Length)
            return true;

        var nextChar = _tokenizer._input[nextPos];
        return nextChar != '=';
    }

    private bool IsInObjectLiteralContext(int fromPos)
    {
        var braceCount = 0;
        var parenCount = 0;

        for (var i = fromPos; i >= 0; i--)
        {
            var ch = _tokenizer._input[i];

            switch (ch)
            {
                case '}':
                    braceCount++;
                    break;
                case '{':
                    braceCount--;
                    if (braceCount < 0)
                    {
                        return true;
                    }

                    break;
                case ')':
                    parenCount++;
                    break;
                case '(':
                    parenCount--;
                    break;
                case ';' when braceCount == 0 && parenCount == 0:
                    return false;
            }
        }

        return false;
    }

    private void SkipTypeAnnotation()
    {
        int depth = 0;
        bool inString = false;
        char stringChar = '\0';
        bool inTemplate = false;
        int templateBraceDepth = 0;

        while (_tokenizer._position < _tokenizer._input.Length)
        {
            var ch = _tokenizer._input[_tokenizer._position];

            if (inTemplate)
            {
                if (ch == '`')
                {
                    inTemplate = false;
                    templateBraceDepth = 0;
                }
                else if (ch == '$' && _tokenizer._position + 1 < _tokenizer._input.Length && _tokenizer._input[_tokenizer._position + 1] == '{')
                {
                    _tokenizer._position++; // Skip to {
                    templateBraceDepth++;
                }
                else if (ch == '}' && templateBraceDepth > 0)
                {
                    templateBraceDepth--;
                }
                else if (ch == '\\' && _tokenizer._position + 1 < _tokenizer._input.Length)
                {
                    _tokenizer._position++;
                }

                _tokenizer._position++;
                continue;
            }

            if (inString)
            {
                if (ch == stringChar && (_tokenizer._position == 0 || _tokenizer._input[_tokenizer._position - 1] != '\\'))
                {
                    inString = false;
                }
                else if (ch == '\\' && _tokenizer._position + 1 < _tokenizer._input.Length)
                {
                    _tokenizer._position++;
                }

                _tokenizer._position++;
                continue;
            }

            switch (ch)
            {
                case '"' or '\'':
                    inString = true;
                    stringChar = ch;
                    break;
                case '`':
                    inTemplate = true;
                    templateBraceDepth = 0;
                    break;
                case '<' or '(' or '[':
                    depth++;
                    break;
                case '>' or ')' or ']':
                    depth--;
                    if (depth < 0)
                        return;
                    break;
                case '{':
                    if (depth == 0)
                    {
                        if (IsLikelyFunctionBody())
                            return;
                    }

                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth < 0)
                        return;
                    break;
                case '&' or '|':
                    if (depth == 0)
                    {
                        var nextPos = _tokenizer._position + 1;
                        while (nextPos < _tokenizer._input.Length && char.IsWhiteSpace(_tokenizer._input[nextPos]))
                            nextPos++;

                        if (nextPos < _tokenizer._input.Length)
                        {
                            var nextChar = _tokenizer._input[nextPos];
                            if (char.IsLetter(nextChar) || nextChar == '{' || nextChar == '[')
                            {
                                _tokenizer._position++;
                                continue;
                            }
                        }

                        return;
                    }

                    break;
                case '=':
                    if (depth == 0)
                    {
                        if (_tokenizer._position + 1 < _tokenizer._input.Length && _tokenizer._input[_tokenizer._position + 1] == '>')
                        {
                            _tokenizer._position++;
                            break;
                        }

                        return;
                    }

                    break;
                case ';' or ',' or '\n' or '\r':
                    if (depth == 0)
                        return;
                    break;
            }

            _tokenizer._position++;
        }
    }

    private void SkipGenericParameters()
    {
        int depth = 1;
        _tokenizer._position++;

        while (_tokenizer._position < _tokenizer._input.Length && depth > 0)
        {
            var ch = _tokenizer._input[_tokenizer._position];
            if (ch == '<')
                depth++;
            else if (ch == '>')
                depth--;

            _tokenizer._position++;
        }
    }

    private void SkipTypeAssertion()
    {
        _tokenizer._position += 2;

        while (_tokenizer._position < _tokenizer._input.Length && char.IsWhiteSpace(_tokenizer._input[_tokenizer._position]))
            _tokenizer._position++;

        SkipTypeAnnotation();
    }

    private bool IsLikelyFunctionBody()
    {
        var pos = _tokenizer._position + 1;

        while (pos < _tokenizer._input.Length && char.IsWhiteSpace(_tokenizer._input[pos]))
            pos++;

        if (pos >= _tokenizer._input.Length)
            return false;

        var remainingInput = _tokenizer._input.Substring(pos);
        return remainingInput.StartsWith("return ", StringComparison.Ordinal) ||
               remainingInput.StartsWith("if ", StringComparison.Ordinal) ||
               remainingInput.StartsWith("for ", StringComparison.Ordinal) ||
               remainingInput.StartsWith("while ", StringComparison.Ordinal) ||
               remainingInput.StartsWith("const ", StringComparison.Ordinal) ||
               remainingInput.StartsWith("let ", StringComparison.Ordinal) ||
               remainingInput.StartsWith("var ", StringComparison.Ordinal) ||
               remainingInput.StartsWith("throw ", StringComparison.Ordinal) ||
               remainingInput.StartsWith("console.", StringComparison.Ordinal) ||
               remainingInput.StartsWith("}");
    }
}
