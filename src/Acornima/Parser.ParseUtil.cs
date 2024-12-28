using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima;

using static SyntaxErrorMessages;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js

public partial class Parser
{
    // Predicate that tests whether the next token is of the given
    // type, and if yes, consumes it as a side effect.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool Eat(TokenType type)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.eat = function`

        if (_tokenizer._type == type)
        {
            Next();
            return true;
        }

        return false;
    }

    // Tests whether parsed token is a contextual keyword.
    private bool IsContextual(string name)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.isContextual = function`

        return _tokenizer._type == TokenType.Name
            && name.Equals(_tokenizer._value.Value)
            && !_tokenizer._containsEscape;
    }

    // Consumes contextual keyword if possible.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool EatContextual(string name)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.eatContextual = function`

        if (IsContextual(name))
        {
            Next();
            return true;
        }

        return false;
    }

    // Asserts that following token is given contextual keyword.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExpectContextual(string name)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.expectContextual = function`

        if (!EatContextual(name))
        {
            Unexpected();
        }
    }

    // Test whether a semicolon can be inserted at the current position.
    private bool CanInsertSemicolon()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.canInsertSemicolon = function`

        return _tokenizer._type == TokenType.EOF
            || _tokenizer._type == TokenType.BraceRight
            || Tokenizer.ContainsLineBreak(_tokenizer._input.SliceBetween(_tokenizer._lastTokenEnd, _tokenizer._start));
    }

    private bool InsertSemicolon()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.insertSemicolon = function`

        if (CanInsertSemicolon())
        {
            _options._onInsertedSemicolon?.Invoke(_tokenizer._lastTokenEnd, _tokenizer._lastTokenEndLocation);
            return true;
        }

        return false;
    }

    // Consume a semicolon, or, failing that, see if we are allowed to
    // pretend that there is a semicolon at this position.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Semicolon()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.semicolon = function`

        if (!Eat(TokenType.Semicolon) && !InsertSemicolon())
        {
            // We deviate a bit from the original acornjs implementation here to match the error reporting behavior of V8.
            if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES8
                && _tokenizer._input.SliceBetween(_tokenizer._lastTokenStart, _tokenizer._lastTokenEnd).SequenceEqual("await".AsSpan()) && !CanAwait)
            {
                Raise(_tokenizer._lastTokenStart, AwaitNotInAsyncContext);
            }

            Unexpected();
        }
    }

    private bool AfterTrailingComma(TokenType type, bool isAllowed = true, bool notNext = false)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.afterTrailingComma = function`

        if (_tokenizer._type == type)
        {
            if (!isAllowed)
            {
                RaiseRecoverable(_tokenizer._start, _tokenizer._type, _tokenizer._value.Value);
            }

            _options._onTrailingComma?.Invoke(_tokenizer._lastTokenStart, _tokenizer._lastTokenStartLocation);

            if (!notNext)
            {
                Next();
            }

            return true;
        }

        return false;
    }

    // Expect a token of a given type. If found, consume it, otherwise,
    // raise an unexpected token error.
    internal void Expect(TokenType type)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.expect = function`

        if (!Eat(type))
        {
            Unexpected();
        }
    }

    internal static string GetUnexpectedTokenMessage(TokenType tokenType, object? tokenValue, out string code)
    {
        switch (tokenType.Kind)
        {
            case TokenKind.Punctuator when tokenType == TokenType.BackQuote:
                code = nameof(UnexpectedTemplateString);
                return UnexpectedTemplateString;

            case TokenKind.Punctuator:
                code = nameof(UnexpectedToken);
                return string.Format(null, UnexpectedToken, (string)tokenValue!);

            case TokenKind.Keyword:
            case TokenKind.NullLiteral:
            case TokenKind.BooleanLiteral:
                code = nameof(UnexpectedToken);
                return string.Format(null, UnexpectedToken, tokenType.Label);

            case TokenKind.Identifier:
                code = nameof(UnexpectedTokenIdentifier);
                return string.Format(null, UnexpectedTokenIdentifier, tokenType == TokenType.PrivateId
                    ? '#'.ToStringCached() + (string)tokenValue!
                    : (string)tokenValue!);

            case TokenKind.NumericLiteral:
            case TokenKind.BigIntLiteral:
                code = nameof(UnexpectedTokenNumber);
                return UnexpectedTokenNumber;

            case TokenKind.StringLiteral:
                code = nameof(UnexpectedTokenString);
                return UnexpectedTokenString;

            case TokenKind.RegExpLiteral:
                code = nameof(UnexpectedTokenRegExp);
                return UnexpectedTokenRegExp;

            case TokenKind.EOF:
                code = nameof(UnexpectedEOS);
                return UnexpectedEOS;
        }

        code = nameof(InvalidOrUnexpectedToken);
        return InvalidOrUnexpectedToken;
    }

    [DoesNotReturn]
    internal void Unexpected()
    {
        Unexpected(new TokenState(_tokenizer));
    }

    [DoesNotReturn]
    internal void Unexpected(int position, TokenType tokenType, object? tokenValue)
    {
        Unexpected(new TokenState(position, tokenType, tokenValue));
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal void Unexpected(TokenState tokenState)
    {
        Raise(tokenState.Position, GetUnexpectedTokenMessage(tokenState.TokenType, tokenState.TokenValue, out var code), code: code);
    }

    [DoesNotReturn]
    internal T Unexpected<T>()
    {
        Unexpected();
        return default!;
    }

    [DoesNotReturn]
    internal void Raise(int pos, string message, [CallerArgumentExpression(nameof(message))] string code = Tokenizer.UnknownError)
    {
        _tokenizer.Raise(pos, message, code: code);
    }

    [DoesNotReturn]
    internal void Raise(int pos, string messageFormat, object?[] args, [CallerArgumentExpression(nameof(messageFormat))] string code = Tokenizer.UnknownError)
    {
        Raise(pos, string.Format(null, messageFormat, args), code);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ParseError RaiseRecoverable(int pos, string message, [CallerArgumentExpression(nameof(message))] string code = Tokenizer.UnknownError)
    {
        return _tokenizer.RaiseRecoverable(pos, message, code: code);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ParseError RaiseRecoverable(int pos, string messageFormat, object?[] args, [CallerArgumentExpression(nameof(messageFormat))] string code = Tokenizer.UnknownError)
    {
        return RaiseRecoverable(pos, string.Format(null, messageFormat, args), code);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal ParseError RaiseRecoverable(int pos, TokenType tokenType, object? tokenValue)
    {
        return RaiseRecoverable(pos, GetUnexpectedTokenMessage(tokenType, tokenValue, out var code), code);
    }

    private void CheckPatternErrors(ref DestructuringErrors destructuringErrors, bool isAssign)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.checkPatternErrors = function`

        // TODO: move to callsite?
        if (Unsafe.IsNullRef(ref destructuringErrors))
        {
            return;
        }

        var position = destructuringErrors.GetTrailingComma();
        if (position >= 0)
        {
            // RaiseRecoverable(destructuringErrors.TrailingComma, "Comma is not permitted after the rest element"); // original acornjs error reporting
            if (destructuringErrors.TrailingComma < 0)
            {
                Raise(position, ParamAfterRest);
            }
            else
            {
                Raise(position, ElementAfterRest);
            }
        }

        position = isAssign ? destructuringErrors.ParenthesizedAssign : destructuringErrors.ParenthesizedBind;
        if (position >= 0)
        {
            // RaiseRecoverable(parens, isAssign ? "Assigning to rvalue" : "Parenthesized pattern"); // original acornjs error reporting
            if (isAssign)
            {
                Raise(position, InvalidLhsInAssignment);
            }
            else
            {
                Raise(position, InvalidDestructuringTarget);
            }
        }
    }

    private bool CheckExpressionErrors(ref DestructuringErrors destructuringErrors, bool andThrow = false)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.checkExpressionErrors = function`

        // TODO: move to callsite?
        if (Unsafe.IsNullRef(ref destructuringErrors))
        {
            return false;
        }

        if (!andThrow)
        {
            return destructuringErrors.ShorthandAssign >= 0 || destructuringErrors.DoubleProto >= 0;
        }

        if (destructuringErrors.ShorthandAssign >= 0)
        {
            // Raise(destructuringErrors.ShorthandAssign, "Shorthand property assignments are valid only in destructuring patterns"); // original acornjs error reporting
            Raise(destructuringErrors.ShorthandAssign, InvalidCoverInitializedName);
        }

        if (destructuringErrors.DoubleProto >= 0)
        {
            // RaiseRecoverable(destructuringErrors.DoubleProto, "Redefinition of __proto__ property"); // original acornjs error reporting
            Raise(destructuringErrors.DoubleProto, DuplicateProto);
        }

        return false;
    }

    private void CheckYieldAwaitInDefaultParams()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.checkYieldAwaitInDefaultParams = function`

        if (_yieldPosition != 0 && (_awaitPosition == 0 || _yieldPosition < _awaitPosition))
        {
            // Raise(_yieldPosition, "Yield expression cannot be a default value"); // original acornjs error reporting
            Raise(_yieldPosition, YieldInParameter);
        }

        if (_awaitPosition != 0)
        {
            // Raise(_awaitPosition, "Await expression cannot be a default value"); // original acornjs error reporting
            Raise(_awaitPosition, AwaitExpressionFormalParameter);
        }
    }

    private static bool IsSimpleAssignTarget(Expression expr)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.isSimpleAssignTarget = function`

        for (; ; )
        {
            if (expr is ParenthesizedExpression parenthesizedExpression)
            {
                // NOTE: Original acornjs implementation does a recursive call here, but we can optimize that into a loop to keep the call stack shallow.
                expr = parenthesizedExpression.Expression;
                continue;
            }

            return expr.Type is NodeType.Identifier or NodeType.MemberExpression;
        }
    }

    internal readonly struct TokenState
    {
        public TokenState(int position, TokenType tokenType, object? tokenValue)
        {
            Position = position;
            TokenType = tokenType;
            TokenValue = tokenValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TokenState(Tokenizer tokenizer)
            : this(tokenizer._start, tokenizer._type, tokenizer._value.Value) { }

        public readonly int Position;
        public readonly TokenType TokenType;
        public readonly object? TokenValue;
    }
}
