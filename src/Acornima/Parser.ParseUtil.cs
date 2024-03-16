using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Properties;

namespace Acornima;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js

public partial class Parser
{
    // Predicate that tests whether the next token is of the given
    // type, and if yes, consumes it as a side effect.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Eat(TokenType type)
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

    public bool InsertSemicolon()
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
    public void Semicolon()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.semicolon = function`

        if (!Eat(TokenType.Semicolon) && !InsertSemicolon())
        {
            // We deviate a bit from the original acornjs implementation here to match the error reporting behavior of V8.
            if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES8
                && _tokenizer._input.SliceBetween(_tokenizer._lastTokenStart, _tokenizer._lastTokenEnd).SequenceEqual("await".AsSpan()) && !CanAwait())
            {
                RaiseRecoverable(_tokenizer._lastTokenStart, SyntaxErrorMessages.AwaitNotInAsyncContext);
            }

            Unexpected();
        }
    }

    private bool AfterTrailingComma(TokenType type, bool notNext = false)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.afterTrailingComma = function`

        if (_tokenizer._type == type)
        {
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
    private void Expect(TokenType type)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.expect = function`

        if (!Eat(type))
        {
            Unexpected();
        }
    }

    private static string GetUnexpectedTokenMessage(TokenState state)
    {
        switch (state.TokenType.Kind)
        {
            case TokenKind.Punctuator when state.TokenType == TokenType.BackQuote:
                return SyntaxErrorMessages.UnexpectedTemplateString;

            case TokenKind.Punctuator:
                return string.Format(SyntaxErrorMessages.UnexpectedToken, (string)state.TokenValue!);

            case TokenKind.Keyword:
            case TokenKind.NullLiteral:
            case TokenKind.BooleanLiteral:
                return string.Format(SyntaxErrorMessages.UnexpectedToken, state.TokenType.Label);

            case TokenKind.Identifier:
                return string.Format(SyntaxErrorMessages.UnexpectedTokenIdentifier, state.TokenType == TokenType.PrivateId
                    ? '#'.ToStringCached() + (string)state.TokenValue!
                    : (string)state.TokenValue!);

            case TokenKind.NumericLiteral:
            case TokenKind.BigIntLiteral:
                return SyntaxErrorMessages.UnexpectedTokenNumber;

            case TokenKind.StringLiteral:
                return SyntaxErrorMessages.UnexpectedTokenString;

            case TokenKind.EOF:
                return SyntaxErrorMessages.UnexpectedEOS;
        }

        return SyntaxErrorMessages.InvalidOrUnexpectedToken;
    }

    [DoesNotReturn]
    private void Unexpected()
    {
        Unexpected(new TokenState(_tokenizer));
    }

    [DoesNotReturn]
    private void Unexpected(TokenState tokenState)
    {
        Raise(tokenState.Position, GetUnexpectedTokenMessage(tokenState));
    }

    [DoesNotReturn]
    private T Unexpected<T>()
    {
        Unexpected();
        return default!;
    }

    [DoesNotReturn]
    private void Raise(int pos, string message, ParseError.Factory? errorFactory = null) => _tokenizer.Raise(pos, message, errorFactory);

    [DoesNotReturn]
    private T Raise<T>(int pos, string message, ParseError.Factory? errorFactory = null) => _tokenizer.Raise<T>(pos, message, errorFactory);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ParseError RaiseRecoverable(int pos, string message, ParseError.Factory? errorFactory = null) => _tokenizer.RaiseRecoverable(pos, message, errorFactory);

    private void CheckPatternErrors(ref DestructuringErrors destructuringErrors, bool isAssign)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.checkPatternErrors = function`

        // TODO: move to callsite?
        if (Unsafe.IsNullRef(ref destructuringErrors))
        {
            return;
        }

        if (destructuringErrors.TrailingComma >= 0)
        {
            // RaiseRecoverable(destructuringErrors.TrailingComma, "Comma is not permitted after the rest element"); // original acornjs error reporting
            RaiseRecoverable(destructuringErrors.TrailingComma, SyntaxErrorMessages.ParamAfterRest);
        }

        var parens = isAssign ? destructuringErrors.ParenthesizedAssign : destructuringErrors.ParenthesizedBind;
        if (parens >= 0)
        {
            // RaiseRecoverable(parens, isAssign ? "Assigning to rvalue" : "Parenthesized pattern"); // original acornjs error reporting
            RaiseRecoverable(parens, isAssign ? SyntaxErrorMessages.InvalidLhsInAssignment : SyntaxErrorMessages.InvalidDestructuringTarget);
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
            Raise(destructuringErrors.ShorthandAssign, SyntaxErrorMessages.InvalidCoverInitializedName);
        }

        if (destructuringErrors.DoubleProto >= 0)
        {
            // RaiseRecoverable(destructuringErrors.DoubleProto, "Redefinition of __proto__ property"); // original acornjs error reporting
            RaiseRecoverable(destructuringErrors.DoubleProto, SyntaxErrorMessages.DuplicateProto);
        }

        return false;
    }

    private void CheckYieldAwaitInDefaultParams()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.checkYieldAwaitInDefaultParams = function`

        if (_yieldPosition != 0 && (_awaitPosition == 0 || _yieldPosition < _awaitPosition))
        {
            // Raise(_yieldPosition, "Yield expression cannot be a default value"); // original acornjs error reporting
            Raise(_yieldPosition, SyntaxErrorMessages.YieldInParameter);
        }

        if (_awaitPosition != 0)
        {
            // Raise(_awaitPosition, "Await expression cannot be a default value"); // original acornjs error reporting
            Raise(_awaitPosition, SyntaxErrorMessages.AwaitExpressionFormalParameter);
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

    private readonly struct TokenState
    {
        public TokenState(int position, TokenType tokenType, object? tokenValue)
        {
            Position = position;
            TokenType = tokenType;
            TokenValue = tokenValue;
        }

        public TokenState(Tokenizer tokenizer)
            : this(tokenizer._start, tokenizer._type, tokenizer._value.Value) { }

        public readonly int Position;
        public readonly TokenType TokenType;
        public readonly object? TokenValue;
    }
}
