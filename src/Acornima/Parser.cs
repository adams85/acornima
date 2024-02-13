using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

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
    private void Next(bool ignoreEscapeSequenceInKeyword = false, bool requireValidEscapeSequenceInTemplate = true)
    {
        _tokenizer.Next(new TokenizerContext(_strict, ignoreEscapeSequenceInKeyword, requireValidEscapeSequenceInTemplate));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Marker StartNode()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/node.js > `pp.startNode = function`

        return new Marker(_tokenizer._start, _tokenizer._startLocation);
    }

    private T FinishNodeAt<T>(in Marker startMarker, in Marker endMarker, T node) where T : Node
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/node.js > `function finishNodeAt`, `pp.finishNodeAt = function`

        node._range = new Range(startMarker.Index, endMarker.Index);
        node._location = new SourceLocation(startMarker.Position, endMarker.Position, _tokenizer._sourceFile);
        _options.OnNode?.Invoke(node);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T FinishNode<T>(in Marker startMarker, T node) where T : Node
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/node.js > `pp.finishNode = function`

        return FinishNodeAt(startMarker, new Marker(_tokenizer._lastTokenEnd, _tokenizer._lastTokenEndLocation), node);
    }

    private T ReinterpretNode<T>(Node originalNode, T node) where T : Node
    {
        node._range = originalNode._range;
        node._location = originalNode._location;
        _options.OnNode?.Invoke(node);
        return node;
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
            throw new InvalidOperationException($"To parse input in strict mode, you need to configure the parser to use {EcmaVersion.ES5} or newer.");
        }

        Reset(input, start, length, SourceType.Script, sourceFile, strict);

        var startMarker = StartNode();

        try
        {
            var body = ParseTopLevel();

            Debug.Assert(_tokenizer._type == TokenType.EOF);
            return FinishNodeAt(startMarker, new Marker(_tokenizer._start, _tokenizer._startLocation), new Script(body, strict));
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
            throw new InvalidOperationException($"To parse input as module code, you need to configure the parser to use {EcmaVersion.ES6} or newer.");
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
    public Expression ParseExpression(string input, string? sourceFile = null, SourceType sourceType = SourceType.Script, bool strict = false)
    {
        return ParseExpression(input ?? throw new ArgumentNullException(nameof(input)), 0, input.Length, sourceFile);
    }

    public Expression ParseExpression(string input, int start, int length, string? sourceFile = null, SourceType sourceType = SourceType.Script, bool strict = false)
    {
        if (sourceType == SourceType.Module)
        {
            if (_tokenizerOptions._ecmaVersion < EcmaVersion.ES6)
            {
                throw new InvalidOperationException($"To parse input as module code, you need to configure the parser to use {EcmaVersion.ES6} or newer.");
            }
        }

        Reset(input, start, length, sourceType, sourceFile, strict);

        try
        {
            Next();
            return ParseExpression(ref Unsafe.NullRef<DestructuringErrors>());
        }
        finally
        {
            ReleaseLargeBuffers();
        }
    }

    private void DeclareName(string name, BindingType bindingType, int pos)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.declareName = function`

        var redeclared = false;
        ref var scope = ref Unsafe.NullRef<Scope>();
        switch (bindingType)
        {
            case BindingType.Lexical:
                scope = ref CurrentScope;
                redeclared = scope.Lexical.IndexOf(name) >= 0 || scope.Functions.IndexOf(name) >= 0 || scope.Var.IndexOf(name) >= 0;
                scope.Lexical.Add(name);
                if (_inModule && (scope.Flags & ScopeFlags.Top) != 0)
                {
                    _undefinedExports!.Remove(name);
                }
                break;

            case BindingType.SimpleCatch:
                scope = ref CurrentScope;
                scope.Lexical.Add(name);
                break;

            case BindingType.Function:
                scope = ref CurrentScope;
                redeclared = (scope.Flags & _functionsAsVarInScopeFlags) != 0
                    ? scope.Lexical.IndexOf(name) >= 0
                    : scope.Lexical.IndexOf(name) >= 0 || scope.Var.IndexOf(name) >= 0;
                scope.Functions.Add(name);
                break;

            default:
                for (var i = _scopeStack.Count - 1; i >= 0; --i)
                {
                    scope = ref _scopeStack.GetItemRef(i);
                    if (scope.Lexical.IndexOf(name) >= 0 && !((scope.Flags & ScopeFlags.SimpleCatch) != 0 && scope.Lexical[0] == name)
                        || (scope.Flags & _functionsAsVarInScopeFlags) == 0 && scope.Functions.IndexOf(name) >= 0)
                    {
                        redeclared = true;
                        break;
                    }

                    scope.Var.Add(name);
                    if (_inModule && (scope.Flags & ScopeFlags.Top) != 0)
                    {
                        _undefinedExports!.Remove(name);
                    }
                    if ((scope.Flags & ScopeFlags.Var) != 0)
                    {
                        break;
                    }
                }
                break;
        }

        if (redeclared)
        {
            RaiseRecoverable(pos, $"Identifier '{name}' has already been declared");
        }
    }

    private void CheckLocalExport(Identifier id)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.checkLocalExport = function`

        ref readonly var rootScope = ref _scopeStack.GetItemRef(0);
        // scope.functions must be empty as Module code is always strict.
        if (rootScope.Lexical.IndexOf(id.Name) < 0
            && rootScope.Var.IndexOf(id.Name) < 0)
        {
            _undefinedExports![id.Name] = id.Start;
        }
    }
}
