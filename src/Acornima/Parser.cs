using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Properties;

namespace Acornima;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/index.js

public sealed partial class Parser
{
    private readonly ParserOptions _options;
    private readonly TokenizerOptions _tokenizerOptions;
    private Tokenizer _tokenizer;

    public Parser() : this(ParserOptions.Default) { }

    public Parser(ParserOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _tokenizerOptions = options.GetTokenizerOptions();
        _tokenizer = new Tokenizer(_tokenizerOptions);
        _isReservedWord = _isReservedWordBind = null!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Script ParseScript(string input, string? sourceFile = null, bool strict = false)
    {
        return ParseScript(input ?? throw new ArgumentNullException(nameof(input)), 0, input.Length, sourceFile, strict);
    }

    public Script ParseScript(string input, int start, int length, string? sourceFile = null, bool strict = false)
    {
        if (strict && _tokenizerOptions._ecmaVersion < EcmaVersion.ES5)
        {
            throw new InvalidOperationException(string.Format(ExceptionMessages.InvalidEcmaVersionForStrictMode, EcmaVersion.ES5));
        }

        Reset(input, start, length, SourceType.Script, sourceFile, strict);

        var startMarker = StartNode();

        try
        {
            var body = ParseTopLevel();

            Debug.Assert(_tokenizer._type == TokenType.EOF);
            return FinishNodeAt(startMarker, new Marker(_tokenizer._start, _tokenizer._startLocation), new Script(body, _strict));
        }
        finally
        {
            ReleaseLargeBuffers();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Module ParseModule(string input, string? sourceFile = null)
    {
        return ParseModule(input ?? throw new ArgumentNullException(nameof(input)), 0, input.Length, sourceFile);
    }

    public Module ParseModule(string input, int start, int length, string? sourceFile = null)
    {
        if (_tokenizerOptions._ecmaVersion < EcmaVersion.ES6)
        {
            throw new InvalidOperationException(string.Format(ExceptionMessages.InvalidEcmaVersionForModule, EcmaVersion.ES6));
        }

        Reset(input, start, length, SourceType.Module, sourceFile, strict: true);

        var startMarker = StartNode();

        try
        {
            var body = ParseTopLevel();

            Debug.Assert(_tokenizer._type == TokenType.EOF);
            return FinishNodeAt(startMarker, new Marker(_tokenizer._start, _tokenizer._startLocation), new Module(body));
        }
        finally
        {
            ReleaseLargeBuffers();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Expression ParseExpression(string input, string? sourceFile = null, bool strict = false)
    {
        return ParseExpression(input ?? throw new ArgumentNullException(nameof(input)), 0, input.Length, sourceFile, strict);
    }

    public Expression ParseExpression(string input, int start, int length, string? sourceFile = null, bool strict = false)
    {
        if (strict && _tokenizerOptions._ecmaVersion < EcmaVersion.ES5)
        {
            throw new InvalidOperationException(string.Format(ExceptionMessages.InvalidEcmaVersionForStrictMode, EcmaVersion.ES5));
        }

        Reset(input, start, length, SourceType.Unknown, sourceFile, strict);

        try
        {
            Next();
            var expression = ParseExpression(ref Unsafe.NullRef<DestructuringErrors>());
            if (_tokenizer._type != TokenType.EOF)
            {
                Unexpected();
            }
            return expression;
        }
        finally
        {
            ReleaseLargeBuffers();
        }
    }
}
