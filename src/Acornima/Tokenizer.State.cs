using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Acornima.Helpers;

namespace Acornima;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js

public partial class Tokenizer
{
    internal string _input;
    internal int _startPosition, _endPosition;
    private SourceType _sourceType;
    internal string? _sourceFile;

    // Used to signal to callers of `ReadWord1` whether the word
    // contained any escape sequences. This is needed because words with
    // escape sequences must not be interpreted as keywords.
    internal bool _containsEscape;

    // Used to signal to callers of `ReadString` whether the string literal
    // contained any legacy octal escape sequences and, if so, at what position.
    // This information is needed for detecting invalid directives in strict mode.
    internal int _legacyOctalPosition;

    // The current position of the tokenizer in the input.
    internal int _position;
    private int _lineStart;
    private int _currentLine;

    // Properties of the current token:
    // Its type
    internal TokenType _type;
    // For tokens that include more information than their type, the value
    internal TokenValue _value;
    // Its start and end offset
    internal int _start, _end;
    // And, if locations are used, the {line, column} object
    // corresponding to those offsets
    internal Position _startLocation, _endLocation;

    // Position information for the previous token
    internal Position _lastTokenStartLocation, _lastTokenEndLocation;
    internal int _lastTokenStart, _lastTokenEnd;

    // The context stack is used to superficially track syntactic
    // context to predict whether a regular expression is allowed in a
    // given position.
    internal ArrayList<TokenContext> _contextStack;
    internal bool _expressionAllowed;

    internal bool _inModule;
    internal bool _strict;

    private bool _requireValidEscapeSequenceInTemplate;
    private bool _inTemplateElement;

    private StringBuilder? _sb;

    internal StringPool _stringPool;

    public void Reset(string input, SourceType sourceType = SourceType.Script, string? sourceFile = null)
    {
        Reset(input, start: 0, sourceType, sourceFile);
    }

    public void Reset(string input, int start, SourceType sourceType = SourceType.Script, string? sourceFile = null)
    {
        Reset(input ?? throw new ArgumentNullException(nameof(input)), start, input.Length - start, sourceType, sourceFile);
    }

    public void Reset(string input, int start, int length, SourceType sourceType = SourceType.Script, string? sourceFile = null)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _startPosition = 0 <= start && start <= input.Length
            ? start
            : throw new ArgumentOutOfRangeException(nameof(start), start, null);
        _endPosition = 0 <= length && length <= input.Length - start
            ? _startPosition + length
            : throw new ArgumentOutOfRangeException(nameof(length), length, null);
        _sourceType = sourceType;
        _sourceFile = sourceFile;

        _containsEscape = false;
        _legacyOctalPosition = -1;

        // Set up token state

        if (start == 0)
        {
            _position = _lineStart = 0;
            _currentLine = 1;
        }
        else
        {
            _position = start;
            _currentLine = GetLineInfo(_input, start, out _lineStart).Line;
        }

        _type = TokenType.EOF;
        _value = TokenValue.EOF;
        _start = _end = _position;
        _startLocation = _endLocation = CurrentPosition;

        _lastTokenEndLocation = _lastTokenStartLocation = default;
        _lastTokenStart = _lastTokenEnd = _position;

        _contextStack.Clear();
        _contextStack.Push(TokenContext.BracketsInStatement);

        _expressionAllowed = true;

        _inModule = _strict = sourceType == SourceType.Module;

        _sb = _sb is not null ? _sb.Clear() : new StringBuilder();
        _stringPool = default;

        _options._errorHandler.Reset();
    }

    internal void ReleaseLargeBuffers()
    {
        (_sb ?? throw new InvalidOperationException()).Clear();
        if (_sb.Capacity > 1024)
        {
            _sb.Capacity = 1024;
        }

        _stringPool = default;

        _contextStack.Clear();
        _contextStack.Push(TokenContext.BracketsInStatement);
        if (_contextStack.Capacity > 64)
        {
            _contextStack.Capacity = 64;
        }

        _codePointRangeCache = null;
    }

    private CodePointRange.Cache? _codePointRangeCache;

    private Position CurrentPosition { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => new Position(_currentLine, _position - _lineStart); }

    private TokenContext CurrentContext { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _contextStack.Peek(); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AcquireStringBuilder([NotNull] out StringBuilder? sb)
    {
        Debug.Assert(_sb is not null, $"String builder is already in use.");
        sb = _sb!.Clear();
        _sb = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReleaseStringBuilder(ref StringBuilder? sb)
    {
        Debug.Assert(_sb is null, $"String builder is not in use currently.");
        _sb = sb;
        sb = null;
    }
}
