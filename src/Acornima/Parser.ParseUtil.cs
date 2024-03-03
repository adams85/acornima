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

    [DoesNotReturn]
    private void Unexpected(int? pos = null)
    {
        string message;
        switch (_tokenizer._type.Kind)
        {
            case TokenKind.Punctuator when _tokenizer._type == TokenType.BackQuote:
                message = SyntaxErrorMessages.UnexpectedTemplateString;
                break;

            case TokenKind.Punctuator:
                message = string.Format(SyntaxErrorMessages.UnexpectedToken, (string)_tokenizer._value.Value!);
                break;

            case TokenKind.Keyword:
            case TokenKind.NullLiteral:
            case TokenKind.BooleanLiteral:
                if (_tokenizer._containsEscape)
                {
                    RaiseRecoverable(_tokenizer._start, SyntaxErrorMessages.InvalidEscapedReservedWord);
                }

                message = string.Format(SyntaxErrorMessages.UnexpectedToken, _tokenizer._type.Label);
                break;

            case TokenKind.Identifier:
                // TODO: reserved words

                message = string.Format(SyntaxErrorMessages.UnexpectedTokenIdentifier, _tokenizer._type == TokenType.PrivateId
                    ? "#" + (string)_tokenizer._value.Value!
                    : (string)_tokenizer._value.Value!);
                break;

            case TokenKind.NumericLiteral:
            case TokenKind.BigIntLiteral:
                message = SyntaxErrorMessages.UnexpectedTokenNumber;
                break;

            case TokenKind.StringLiteral:
                message = SyntaxErrorMessages.UnexpectedTokenString;
                break;

            case TokenKind.EOF:
                message = SyntaxErrorMessages.UnexpectedEOS;
                break;

            default:
                message = SyntaxErrorMessages.InvalidOrUnexpectedToken;
                break;
        }

        Raise(pos ?? _tokenizer._start, message);
    }

    [DoesNotReturn]
    private T Unexpected<T>(int? pos = null)
    {
        Unexpected(pos);
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
            RaiseRecoverable(destructuringErrors.TrailingComma, "Comma is not permitted after the rest element");
        }

        var parens = isAssign ? destructuringErrors.ParenthesizedAssign : destructuringErrors.ParenthesizedBind;
        if (parens >= 0)
        {
            RaiseRecoverable(parens, isAssign ? "Assigning to rvalue" : "Parenthesized pattern");
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
            Raise(destructuringErrors.ShorthandAssign, "Shorthand property assignments are valid only in destructuring patterns");
        }

        if (destructuringErrors.DoubleProto >= 0)
        {
            RaiseRecoverable(destructuringErrors.DoubleProto, "Redefinition of __proto__ property");
        }

        return false;
    }

    private void CheckYieldAwaitInDefaultParams()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/parseutil.js > `pp.checkYieldAwaitInDefaultParams = function`

        if (_yieldPosition != 0 && (_awaitPosition == 0 || _yieldPosition < _awaitPosition))
        {
            Raise(_yieldPosition, "Yield expression cannot be a default value");
        }

        if (_awaitPosition != 0)
        {
            Raise(_awaitPosition, "Await expression cannot be a default value");
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
}
