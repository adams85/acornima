using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/index.js

public sealed partial class Parser : IParser
{
    private readonly ParserOptions _options;
    private readonly TokenizerOptions _tokenizerOptions;
    internal Tokenizer _tokenizer;
    private readonly IExtension? _extension;

    public Parser() : this(ParserOptions.Default) { }

    public Parser(ParserOptions options) : this(options, extension: null) { }

    internal Parser(ParserOptions options, IExtension? extension)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _extension = extension;
        _tokenizer = extension is null
            ? new Tokenizer(options.GetTokenizerOptions(), extension: null)
            : extension.CreateTokenizer(options.GetTokenizerOptions());
        _tokenizerOptions = _tokenizer.Options;
        _isReservedWord = _isReservedWordBind = null!;
    }

    public ParserOptions Options => _options;

    public Script ParseScript(string input, string? sourceFile = null, bool strict = false)
        => ParseScript(input ?? ThrowArgumentNullException<string>(nameof(input)), 0, input.Length, sourceFile, strict);

    public Script ParseScript(string input, int start, int length, string? sourceFile = null, bool strict = false)
    {
        if (strict && _tokenizerOptions._ecmaVersion < EcmaVersion.ES5)
        {
            throw new InvalidOperationException(string.Format(null, ExceptionMessages.InvalidEcmaVersionForStrictMode, EcmaVersion.ES5));
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

    public Module ParseModule(string input, string? sourceFile = null)
        => ParseModule(input ?? ThrowArgumentNullException<string>(nameof(input)), 0, input.Length, sourceFile);

    public Module ParseModule(string input, int start, int length, string? sourceFile = null)
    {
        if (_tokenizerOptions._ecmaVersion < EcmaVersion.ES6)
        {
            throw new InvalidOperationException(string.Format(null, ExceptionMessages.InvalidEcmaVersionForModule, EcmaVersion.ES6));
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

    public Expression ParseExpression(string input, string? sourceFile = null, bool strict = false)
        => ParseExpression(input ?? ThrowArgumentNullException<string>(nameof(input)), 0, input.Length, sourceFile, strict);

    public Expression ParseExpression(string input, int start, int length, string? sourceFile = null, bool strict = false)
    {
        if (strict && _tokenizerOptions._ecmaVersion < EcmaVersion.ES5)
        {
            throw new InvalidOperationException(string.Format(null, ExceptionMessages.InvalidEcmaVersionForStrictMode, EcmaVersion.ES5));
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

    internal interface IExtension
    {
        Tokenizer CreateTokenizer(TokenizerOptions tokenizerOptions);

        Expression ParseExprAtom();
    }
}
