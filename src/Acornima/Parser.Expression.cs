using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static Unsafe;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js

// A recursive descent parser operates by defining functions for all
// syntactic elements, and recursively calling those, each function
// advancing the input stream and returning an AST node. Precedence
// of constructs (for example, the fact that `!x[1]` means `!(x[1])`
// instead of `(!x)[1]` is handled by the fact that the parser
// function that parses unary prefix operators is called first, and
// in turn calls the function that parses `[]` subscripts — that
// way, it'll receive the node for `x[1]` already parsed, and wraps
// *that* in the unary operator node.
//
// Acorn uses an [operator precedence parser][opp] to handle binary
// operator precedence, because it is much more compact than using
// the technique outlined above, which uses different, nesting
// functions to specify precedence, for all of the ten binary
// precedence levels that JavaScript defines.
//
// [opp]: http://en.wikipedia.org/wiki/Operator-precedence_parser

public partial class Parser
{
    // Check if property name clashes with already added.
    // Object/class getters and setters are not allowed to clash —
    // either with each other or with an init property — and in
    // strict mode, init properties are also not allowed to be repeated.
    private void CheckPropertyClash(Node property, ref bool hasProto, ref Dictionary<string, int>? propHash, ref DestructuringErrors destructuringErrors)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.checkPropClash = function`

        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES9 && property.Type == NodeType.SpreadElement)
        {
            return;
        }

        var prop = (Property)property;
        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6 && (prop.Computed || prop.Method || prop.Shorthand))
        {
            return;
        }

        var key = prop.Key;
        string name;
        if (key is Identifier identifier)
        {
            name = identifier.Name;
        }
        else if (key is Literal literal)
        {
            Debug.Assert(literal.Kind is TokenKind.StringLiteral or TokenKind.NumericLiteral or TokenKind.BigIntLiteral);
            name = Convert.ToString(literal.Value, CultureInfo.InvariantCulture)!;
        }
        else
        {
            return;
        }

        var kind = prop.Kind;
        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6)
        {
            if (kind == PropertyKind.Init && name == "__proto__")
            {
                if (hasProto)
                {
                    if (IsNullRef(ref destructuringErrors))
                    {
                        RaiseRecoverable(key.Start, "Redefinition of __proto__ property");
                    }
                    else if (destructuringErrors.DoubleProto < 0)
                    {
                        destructuringErrors.DoubleProto = key.Start;
                    }
                }
                else
                {
                    hasProto = true;
                }
            }
        }
        else
        {
            propHash ??= new Dictionary<string, int>();
            if (propHash.TryGetValue(name, out var other))
            {
                const int initPropFlag = 1 << (int)PropertyKind.Init;
                const int getterFlag = 1 << (int)PropertyKind.Get;
                const int setterFlag = 1 << (int)PropertyKind.Set;

                var redefinition = kind == PropertyKind.Init
                    ? _strict && (other & initPropFlag) != 0 || (other & (getterFlag | setterFlag)) != 0
                    : (other & (initPropFlag | 1 << (int)kind)) != 0;

                if (redefinition)
                {
                    RaiseRecoverable(key.Start, "Redefinition of property");
                }
            }
            else
            {
                other = 0;
            }

            propHash[name] = other | (1 << (int)kind);
        }
    }

    // These nest, from the most general expression type at the top to
    // 'atomic', nondivisible expression types at the bottom. Most of
    // the functions will simply let the function(s) below them parse,
    // and, *if* the syntactic construct they handle is present, wrap
    // the AST node that the inner parser gave them in another node.

    // Parse a full expression. The optional arguments are used to
    // forbid the `in` operator (in for loops initalization expressions)
    // and provide reference for storing '=' operator inside shorthand
    // property assignment in contexts where both object expression
    // and object pattern might appear (so it's possible to raise
    // delayed syntax error at correct position).
    private Expression ParseExpression(ref DestructuringErrors destructuringErrors, ExpressionContext context = ExpressionContext.Default)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseExpression = function`

        var startMarker = StartNode();

        var expression = ParseMaybeAssign(ref destructuringErrors, context);

        if (_tokenizer._type == TokenType.Comma)
        {
            var expressions = new ArrayList<Expression>() { expression };

            while (Eat(TokenType.Comma))
            {
                expressions.Add(ParseMaybeAssign(ref destructuringErrors, context));
            }

            return FinishNode(startMarker, new SequenceExpression(NodeList.From(ref expressions)));
        }

        return expression;
    }

    // Parse an assignment expression. This includes applications of
    // operators like `+=`.
    private Expression ParseMaybeAssign(ref DestructuringErrors destructuringErrors, ExpressionContext context = ExpressionContext.Default)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseMaybeAssign = function`

        EnterRecursion();

        if (IsContextual("yield"))
        {
            if (InGenerator())
            {
                return ExitRecursion(ParseYield(context));
            }

            // The tokenizer will assume an expression is allowed after
            // `yield`, but this isn't that kind of yield
            _tokenizer._expressionAllowed = false;
        }

        var ownsDestructuringErrors = IsNullRef(ref destructuringErrors);
        var ownDestructuringErrors = new DestructuringErrors();
        ref var actualDestructuringErrors = ref (ownsDestructuringErrors ? ref ownDestructuringErrors : ref destructuringErrors);

        var oldParenAssign = actualDestructuringErrors.ParenthesizedAssign;
        var oldTrailingComma = actualDestructuringErrors.TrailingComma;
        var oldDoubleProto = actualDestructuringErrors.DoubleProto;
        actualDestructuringErrors.ParenthesizedAssign = actualDestructuringErrors.TrailingComma = -1;

        var startMarker = StartNode();

        if (_tokenizer._type == TokenType.ParenLeft || _tokenizer._type == TokenType.Name)
        {
            _potentialArrowAt = _tokenizer._start;
            _potentialArrowInForAwait = (context & ExpressionContext.AwaitForInit) == ExpressionContext.AwaitForInit;
        }

        var left = ParseMaybeConditional(ref actualDestructuringErrors, context);

        if (_tokenizer._type.IsAssignment)
        {
            var op = AssignmentExpression.OperatorFromString((string)_tokenizer._value.Value!);
            Debug.Assert(op != Operator.Unknown);

            var leftNode = _tokenizer._type == TokenType.Eq ? ToAssignable(left, isBinding: false, ref actualDestructuringErrors) : left;

            if (!ownsDestructuringErrors)
            {
                actualDestructuringErrors.ParenthesizedAssign = actualDestructuringErrors.TrailingComma = actualDestructuringErrors.DoubleProto = -1;
            }

            if (actualDestructuringErrors.ShorthandAssign >= leftNode.Start)
            {
                actualDestructuringErrors.ShorthandAssign = -1; // reset because shorthand default was used correctly
            }

            if (_tokenizer._type == TokenType.Eq)
            {
                CheckLValPattern(leftNode);
            }
            else
            {
                CheckLValSimple(leftNode);
            }

            Next();

            var right = ParseMaybeAssign(ref NullRef<DestructuringErrors>(), context);

            if (oldDoubleProto >= 0)
            {
                actualDestructuringErrors.DoubleProto = oldDoubleProto;
            }

            return ExitRecursion(FinishNode(startMarker, new AssignmentExpression(op, leftNode, right)));
        }

        if (ownsDestructuringErrors)
        {
            CheckExpressionErrors(ref actualDestructuringErrors, andThrow: true);
        }

        if (oldParenAssign >= 0)
        {
            actualDestructuringErrors.ParenthesizedAssign = oldParenAssign;
        }

        if (oldTrailingComma >= 0)
        {
            actualDestructuringErrors.TrailingComma = oldTrailingComma;
        }

        return ExitRecursion(left);
    }

    // Parse a ternary conditional (`?:`) operator.
    private Expression ParseMaybeConditional(ref DestructuringErrors destructuringErrors, ExpressionContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseMaybeConditional = function`

        var startMarker = StartNode();

        var expr = ParseMaybeBinary(ref destructuringErrors, context);
        if (CheckExpressionErrors(ref destructuringErrors))
        {
            return expr;
        }

        if (Eat(TokenType.Question))
        {
            var consequent = ParseMaybeAssign(ref NullRef<DestructuringErrors>());

            Expect(TokenType.Colon);
            var alternate = ParseMaybeAssign(ref NullRef<DestructuringErrors>(), context);

            return FinishNode(startMarker, new ConditionalExpression(test: expr, consequent, alternate));
        }

        return expr;
    }

    // Start the precedence parser.
    private Expression ParseMaybeBinary(ref DestructuringErrors destructuringErrors, ExpressionContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseExprOps = function`

        var startMarker = StartNode();

        var expr = ParseMaybeUnary(sawUnary: false, incDec: false, ref destructuringErrors, context);
        if (CheckExpressionErrors(ref destructuringErrors))
        {
            return expr;
        }

        return expr.Start == startMarker.Index && expr.Type == NodeType.ArrowFunctionExpression
            ? expr
            : ParseBinaryOp(startMarker, expr, minPrec: -1, context);
    }

    // Parse binary operators with the operator precedence parsing
    // algorithm. `left` is the left-hand side of the operator.
    // `minPrec` provides context that allows the function to stop and
    // defer further parser to one of its callers when it encounters an
    // operator that has a lower precedence than the set it is parsing.
    private Expression ParseBinaryOp(in Marker leftStartMarker, Expression left, int minPrec, ExpressionContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseExprOp = function`

        for (; ; )
        {
            var prec = _tokenizer._type.Precedence;
            // NOTE: TokenType.Precedence defaults to -1 for non-binary operator tokens.
            if (prec > minPrec && (context == ExpressionContext.Default || _tokenizer._type != TokenType.In))
            {
                Operator op;
                bool coalesce;
                var logical = _tokenizer._type == TokenType.LogicalOr || _tokenizer._type == TokenType.LogicalAnd;
                if (logical)
                {
                    coalesce = false;
                    op = _tokenizer._type == TokenType.LogicalOr ? Operator.LogicalOr : Operator.LogicalAnd;
                }
                else if (_tokenizer._type == TokenType.Coalesce)
                {
                    coalesce = true;
                    op = Operator.NullishCoalescing;

                    // Handle the precedence of `TokenType.Coalesce` as equal to the range of logical expressions.
                    // In other words, `BinaryExpession.Right` shouldn't contain logical expressions in order to check the mixed error.
                    prec = TokenType.LogicalAnd.Precedence;
                }
                else
                {
                    coalesce = false;
                    op = NonLogicalBinaryExpression.OperatorFromString((string)_tokenizer._value.Value!);
                    Debug.Assert(op != Operator.Unknown);
                }

                Next();

                var rightStartMarker = StartNode();
                // NOTE: We don't need stack overflow protection here because this recursion is known to be bounded (by the number of precedence levels).
                var right = ParseBinaryOp(rightStartMarker, ParseMaybeUnary(sawUnary: false, incDec: false, ref NullRef<DestructuringErrors>(), context), prec, context);
                var node = BuildBinary(leftStartMarker, left, right, op, logical || coalesce);

                if (logical && _tokenizer._type == TokenType.Coalesce
                    || coalesce && (_tokenizer._type == TokenType.LogicalOr || _tokenizer._type == TokenType.LogicalAnd))
                {
                    RaiseRecoverable(_tokenizer._start, "Logical expressions and coalesce expressions cannot be mixed. Wrap either by parentheses");
                }

                // NOTE: Original acornjs implementation does a recursive call here, but we can optimize that into a loop to keep the call stack shallow.
                left = node;
                continue;
            }

            return left;
        }
    }

    private BinaryExpression BuildBinary(in Marker startMarker, Expression left, Expression right, Operator op, bool logical)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.buildBinary = function`

        if (right.Type == NodeType.PrivateIdentifier)
        {
            Raise(right.Start, "Private identifier can only be left side of binary expression");
        }

        return FinishNode<BinaryExpression>(startMarker, logical
            ? new LogicalExpression(op, left, right)
            : new NonLogicalBinaryExpression(op, left, right));
    }

    // Parse unary operators, both prefix and postfix.
    private Expression ParseMaybeUnary(bool sawUnary, bool incDec, ref DestructuringErrors destructuringErrors, ExpressionContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseMaybeUnary = function`

        var startMarker = StartNode();

        Expression expr;
        Operator op;
        if (IsContextual("await") && CanAwait())
        {
            EnterRecursion();
            expr = ExitRecursion(ParseAwait(context));
            sawUnary = true;
        }
        else if (_tokenizer._type.Prefix)
        {
            var update = _tokenizer._type == TokenType.IncDec;
            op = update
                ? UpdateExpression.OperatorFromString((string)_tokenizer._value.Value!)
                : NonUpdateUnaryExpression.OperatorFromString((string)_tokenizer._value.Value!);
            Debug.Assert(op != Operator.Unknown);

            Next();

            EnterRecursion();
            var argument = ExitRecursion(ParseMaybeUnary(sawUnary: true, incDec: update, ref NullRef<DestructuringErrors>(), context));
            CheckExpressionErrors(ref destructuringErrors, andThrow: true);

            if (update)
            {
                CheckLValSimple(argument);
            }
            else
            {
                if (op == Operator.Delete)
                {
                    if (_strict && IsLocalVariableAccess(argument))
                    {
                        RaiseRecoverable(startMarker.Index, "Deleting local variable in strict mode");
                    }
                    else if (IsPrivateFieldAccess(argument))
                    {
                        RaiseRecoverable(startMarker.Index, "Private fields can not be deleted");
                    }
                }

                // Original acornjs implementation sets this flag in an else branch.
                // TODO: report bug(?)
                sawUnary = true;
            }

            expr = FinishNode<UnaryExpression>(startMarker, update
                ? new UpdateExpression(op, argument, prefix: true)
                : new NonUpdateUnaryExpression(op, argument));
        }
        else if (!sawUnary && _tokenizer._type == TokenType.PrivateId)
        {
            if (_options._checkPrivateFields && ((context & ExpressionContext.ForInit) != 0 || _privateNameStack.Count == 0))
            {
                Unexpected();
            }

            expr = ParsePrivateIdentifier();
            // only could be private fields in 'in', such as #x in obj
            if (_tokenizer._type != TokenType.In)
            {
                Unexpected();
            }
        }
        else
        {
            expr = ParseExprSubscripts(ref destructuringErrors, context);
            if (CheckExpressionErrors(ref destructuringErrors))
            {
                return expr;
            }

            while (_tokenizer._type.Postfix && !CanInsertSemicolon())
            {
                op = UpdateExpression.OperatorFromString((string)_tokenizer._value.Value!);
                CheckLValSimple(expr);
                Next();
                expr = FinishNode(startMarker, new UpdateExpression(op, expr, prefix: false));
            }
        }

        if (!incDec && Eat(TokenType.StarStar))
        {
            if (sawUnary)
            {
                Unexpected(_tokenizer._lastTokenStart);
            }
            else
            {
                EnterRecursion();
                var right = ExitRecursion(ParseMaybeUnary(sawUnary: false, incDec: false, ref NullRef<DestructuringErrors>(), context));
                return BuildBinary(startMarker, left: expr, right, Operator.Exponentiation, logical: false);
            }
        }

        return expr;
    }

    private static bool IsLocalVariableAccess(Expression expr)
    {
        for (; ; )
        {
            switch (expr)
            {
                case Identifier:
                    return true;

                case ParenthesizedExpression parenthesizedExpression:
                    expr = parenthesizedExpression.Expression;
                    continue;
            }

            return false;
        }
    }

    private static bool IsPrivateFieldAccess(Expression expr)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `function isPrivateFieldAccess`

        for (; ; )
        {
            switch (expr)
            {
                case MemberExpression { Property.Type: NodeType.PrivateIdentifier }:
                    return true;

                case ChainExpression chainExpression:
                    expr = chainExpression.Expression;
                    continue;

                case ParenthesizedExpression parenthesizedExpression:
                    expr = parenthesizedExpression.Expression;
                    continue;
            }

            return false;
        }
    }

    // Parse call, dot, and `[]`-subscript expressions.
    private Expression ParseExprSubscripts(ref DestructuringErrors destructuringErrors, ExpressionContext context = ExpressionContext.Default)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseExprSubscripts = function`

        var startMarker = StartNode();

        Expression expr;
        if ((context & ExpressionContext.Decorator) == 0)
        {
            expr = ParseExprAtom(ref destructuringErrors, context);
        }
        else
        {
            if (_tokenizer._type != TokenType.Name && _tokenizer._type != TokenType.ParenLeft)
            {
                Unexpected<Expression>();
            }

            expr = ParseExprAtom(ref destructuringErrors, context & ~ExpressionContext.Decorator);
        }

        if (expr.Type == NodeType.ArrowFunctionExpression
            && !_tokenizer._input.SliceBetween(_tokenizer._lastTokenStart, _tokenizer._lastTokenEnd).SequenceEqual(")".AsSpan()))
        {
            return expr;
        }

        var result = ParseSubscripts(startMarker, expr, noCalls: false, context);

        if (!IsNullRef(ref destructuringErrors) && result.Type == NodeType.MemberExpression)
        {
            if (destructuringErrors.ParenthesizedAssign >= result.Start)
            {
                destructuringErrors.ParenthesizedAssign = -1;
            }

            if (destructuringErrors.ParenthesizedBind >= result.Start)
            {
                destructuringErrors.ParenthesizedBind = -1;
            }

            if (destructuringErrors.TrailingComma >= result.Start)
            {
                destructuringErrors.TrailingComma = -1;
            }
        }

        return result;
    }

    private Expression ParseSubscripts(in Marker startMarker, Expression baseExpr, bool noCalls, ExpressionContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseSubscripts = function`

        const string asyncKeyword = "async";

        var maybeAsyncArrow = _tokenizerOptions._ecmaVersion >= EcmaVersion.ES8 && baseExpr is Identifier { Name: asyncKeyword }
            && _tokenizer._lastTokenEnd == baseExpr.End && !CanInsertSemicolon() && baseExpr.End - baseExpr.Start == asyncKeyword.Length
            && _potentialArrowAt == baseExpr.Start;

        var optionalChained = false;
        var hasCall = false;

        for (; ; )
        {
            var element = ParseSubscript(startMarker, baseExpr, noCalls, maybeAsyncArrow, ref optionalChained, ref hasCall, context);

            if (element == baseExpr || element.Type == NodeType.ArrowFunctionExpression)
            {
                if (optionalChained)
                {
                    element = FinishNode(startMarker, new ChainExpression(element));
                }

                return element;
            }

            baseExpr = element;
        }
    }

    private Expression ParseSubscript(in Marker startMarker, Expression baseExpr, bool noCalls, bool maybeAsyncArrow,
        ref bool optionalChained, ref bool hasCall, ExpressionContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseSubscript = function`

        var optionalSupported = _tokenizerOptions._ecmaVersion >= EcmaVersion.ES11;
        var optional = optionalSupported && Eat(TokenType.QuestionDot);
        if (optional)
        {
            if (noCalls)
            {
                Raise(_tokenizer._lastTokenStart, "Optional chaining cannot appear in the callee of new expressions");
            }

            if ((context & ExpressionContext.Decorator) != 0)
            {
                Raise(_tokenizer._lastTokenStart, "Invalid decorator member expression");
            }
        }

        var computed = Eat(TokenType.BracketLeft);
        if (computed
            || (optional && _tokenizer._type != TokenType.ParenLeft && _tokenizer._type != TokenType.BackQuote)
            || Eat(TokenType.Dot))
        {
            Expression property;
            if (computed)
            {
                if ((context & ExpressionContext.Decorator) != 0)
                {
                    Raise(_tokenizer._lastTokenStart, "Invalid decorator member expression");
                }

                property = ParseExpression(ref NullRef<DestructuringErrors>());
                Expect(TokenType.BracketRight);
            }
            else
            {
                if (hasCall && (context & ExpressionContext.Decorator) != 0)
                {
                    Raise(_tokenizer._lastTokenStart, "Invalid decorator member expression");
                }

                property = _tokenizer._type == TokenType.PrivateId && baseExpr.Type != NodeType.Super
                    ? ParsePrivateIdentifier()
                    : ParseIdentifier(liberal: _options._allowReserved != AllowReservedOption.Never);
            }

            baseExpr = FinishNode(startMarker, new MemberExpression(obj: baseExpr, property, computed, optional));
            optionalChained = optionalChained || optional;
        }
        else if (!noCalls && Eat(TokenType.ParenLeft))
        {
            var destructuringErrors = new DestructuringErrors();
            var oldYieldPos = _yieldPosition;
            var oldAwaitPos = _awaitPosition;
            var oldAwaitIdentPos = _awaitIdentifierPosition;
            _yieldPosition = _awaitPosition = _awaitIdentifierPosition = 0;

            if (hasCall && (context & ExpressionContext.Decorator) != 0)
            {
                Raise(_tokenizer._lastTokenStart, "Invalid decorator member expression");
            }

            NodeList<Expression> exprList = ParseExprList(close: TokenType.ParenRight,
                allowTrailingComma: _tokenizerOptions._ecmaVersion >= EcmaVersion.ES8, allowEmptyItem: false,
                ref destructuringErrors)!;

            if (maybeAsyncArrow && !optional && !CanInsertSemicolon() && Eat(TokenType.Arrow))
            {
                CheckPatternErrors(ref destructuringErrors, isAssign: false);
                CheckYieldAwaitInDefaultParams();
                if (_awaitIdentifierPosition > 0)
                {
                    Raise(_awaitIdentifierPosition, "Cannot use 'await' as identifier inside an async function");
                }

                _yieldPosition = oldYieldPos;
                _awaitPosition = oldAwaitPos;
                _awaitIdentifierPosition = oldAwaitIdentPos;
                return ParseArrowExpression(startMarker, exprList.AsNodes()!, isAsync: true, context);
            }

            CheckExpressionErrors(ref destructuringErrors, andThrow: true);

            if (oldYieldPos != 0)
            {
                _yieldPosition = oldYieldPos;
            }
            if (oldAwaitPos != 0)
            {
                _awaitPosition = oldAwaitPos;
            }
            if (oldAwaitIdentPos != 0)
            {
                _awaitIdentifierPosition = oldAwaitIdentPos;
            }
            baseExpr = FinishNode(startMarker, new CallExpression(callee: baseExpr, exprList, optional));
            optionalChained = optionalChained || optional;
            hasCall = true;
        }
        else if (_tokenizer._type == TokenType.BackQuote)
        {
            if (optional || optionalChained)
            {
                Raise(_tokenizer._start, "Optional chaining cannot appear in the tag of tagged template expressions");
            }

            if ((context & ExpressionContext.Decorator) != 0)
            {
                Raise(_tokenizer._start, "Invalid decorator member expression");
            }

            var quasi = ParseTemplate(isTagged: true);
            baseExpr = FinishNode(startMarker, new TaggedTemplateExpression(tag: baseExpr, quasi));
        }

        return baseExpr;
    }

    // Parse an atomic expression — either a single token that is an
    // expression, an expression started by a keyword like `function` or
    // `new`, or an expression wrapped in punctuation like `()`, `[]`,
    // or `{}`.
    private Expression ParseExprAtom(ref DestructuringErrors destructuringErrors, ExpressionContext context = ExpressionContext.Default)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseExprAtom = function`

        // WARNING: ExpressionContext.ForNew should not be propagated in most cases.

        var startMarker = StartNode();

        bool canBeArrow;

        switch (_tokenizer._type.Kind)
        {
            case TokenKind.Keyword:
                switch (_tokenizer._type.Keyword!.Value)
                {
                    case Keyword.Super:
                        if (!AllowSuper())
                        {
                            Raise(_tokenizer._start, "'super' keyword outside a method");
                        }

                        Next();
                        if (_tokenizer._type == TokenType.ParenLeft && !AllowDirectSuper())
                        {
                            Raise(startMarker.Index, "super() call outside constructor of a subclass");
                        }

                        // The `super` keyword can appear at below:
                        // SuperProperty:
                        //     super [ Expression ]
                        //     super . IdentifierName
                        // SuperCall:
                        //     super ( Arguments )
                        if (_tokenizer._type != TokenType.Dot && _tokenizer._type != TokenType.BracketLeft && _tokenizer._type != TokenType.ParenLeft)
                        {
                            Unexpected();
                        }

                        return FinishNode(startMarker, new Super());

                    case Keyword.This:
                        Next();
                        return FinishNode(startMarker, new ThisExpression());

                    case Keyword.Function:
                        Next();
                        return (FunctionExpression)ParseFunction(startMarker, FunctionOrClassFlags.None);

                    case Keyword.Class:
                        Next();
                        EnterRecursion();
                        return ExitRecursion((ClassExpression)ParseClass(startMarker, FunctionOrClassFlags.None));

                    case Keyword.New:
                        EnterRecursion();
                        return ExitRecursion(ParseNew());

                    case Keyword.Import when _tokenizerOptions._ecmaVersion >= EcmaVersion.ES11:
                        return ParseExprImport(context);
                }
                goto default;

            case TokenKind.Identifier when _tokenizer._type == TokenType.Name:
                canBeArrow = _potentialArrowAt == _tokenizer._start;
                var containsEsc = _tokenizer._containsEscape;
                var id = ParseIdentifier(liberal: _tokenizer._type != TokenType.Name);

                if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES8 && !containsEsc && id.Name == "async" && !CanInsertSemicolon() && Eat(TokenType.Function))
                {
                    _tokenizer.OverrideContext(TokenContext.FunctionInExpression);
                    return (Expression)ParseFunction(startMarker, FunctionOrClassFlags.None, isAsync: true, context & ~ExpressionContext.ForNew);
                }

                if (canBeArrow && !CanInsertSemicolon())
                {
                    if (Eat(TokenType.Arrow))
                    {
                        return ParseArrowExpression(startMarker, parameters: new NodeList<Node>(id), isAsync: false, context & ~ExpressionContext.ForNew);
                    }

                    if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES8 && id.Name == "async" && _tokenizer._type == TokenType.Name && !containsEsc
                        && (!_potentialArrowInForAwait || !"of".Equals(_tokenizer._value.Value) || _tokenizer._containsEscape))
                    {
                        id = ParseIdentifier();
                        if (CanInsertSemicolon() || !Eat(TokenType.Arrow))
                        {
                            Unexpected();
                        }

                        return ParseArrowExpression(startMarker, parameters: new NodeList<Node>(id), isAsync: true, context & ~ExpressionContext.ForNew);
                    }
                }

                return id;

            case TokenKind.NullLiteral:
            case TokenKind.BooleanLiteral:
                var raw = _tokenizer._containsEscape
                    ? Tokenizer.DeduplicateString(_tokenizer._input.SliceBetween(_tokenizer._start, _tokenizer._end), ref _tokenizer._stringPool)
                    : _tokenizer._type.Label;
                Literal literal = _tokenizer._type.Kind == TokenKind.NullLiteral
                    ? new NullLiteral(raw)
                    : new BooleanLiteral(_tokenizer._type.Value, raw);
                Next();
                return FinishNode(startMarker, literal);

            case TokenKind.StringLiteral:
            case TokenKind.NumericLiteral:
            case TokenKind.BigIntLiteral:
            case TokenKind.RegExpLiteral:
                raw = Tokenizer.DeduplicateString(_tokenizer._input.SliceBetween(_tokenizer._start, _tokenizer._end), ref _tokenizer._stringPool, Tokenizer.NonIdentifierDeduplicationThreshold);
                literal = _tokenizer._type.Kind switch
                {
                    TokenKind.StringLiteral => new StringLiteral((string)_tokenizer._value.Value!, raw),
                    TokenKind.NumericLiteral => new NumericLiteral(_tokenizer._value.NumericValue, raw),
                    TokenKind.BigIntLiteral => new BigIntLiteral(_tokenizer._value.BigIntValue, raw),
                    _ => CreateRegExpLiteral(_tokenizer._value, raw)
                };

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static RegExpLiteral CreateRegExpLiteral(in TokenValue value, string raw)
                {
                    var tuple = (Tuple<RegExpValue, RegExpParseResult>)value.Value!;
                    return new RegExpLiteral(tuple.Item1, tuple.Item2, raw);
                }

                Next();
                return FinishNode(startMarker, literal);

            case TokenKind.Punctuator when _tokenizer._type == TokenType.ParenLeft:
                canBeArrow = _potentialArrowAt == _tokenizer._start;
                var expr = ParseParenAndDistinguishExpression(canBeArrow, context & ~ExpressionContext.ForNew);
                if (!IsNullRef(ref destructuringErrors))
                {
                    if (destructuringErrors.ParenthesizedAssign < 0 && !IsSimpleAssignTarget(expr))
                    {
                        destructuringErrors.ParenthesizedAssign = startMarker.Index;
                    }

                    if (destructuringErrors.ParenthesizedBind < 0)
                    {
                        destructuringErrors.ParenthesizedBind = startMarker.Index;
                    }
                }

                return expr;

            case TokenKind.Punctuator when _tokenizer._type == TokenType.BracketLeft:
                Next();
                var elements = ParseExprList(close: TokenType.BracketRight, allowTrailingComma: true, allowEmptyItem: true, ref destructuringErrors);
                return FinishNode(startMarker, new ArrayExpression(elements));

            case TokenKind.Punctuator when _tokenizer._type == TokenType.BraceLeft:
                _tokenizer.OverrideContext(TokenContext.BracketsInExpression);
                EnterRecursion();
                return ExitRecursion((Expression)ParseObject(isPattern: false, ref destructuringErrors));

            case TokenKind.Punctuator when _tokenizer._type == TokenType.BackQuote:
                return ParseTemplate(isTagged: false);

            case TokenKind.Punctuator when _tokenizer._type == TokenType.Slash:
                // If a division operator appears in an expression position, the
                // tokenizer got confused, and we force it to read a regexp instead.
                _tokenizer.ReadRegExp();
                goto case TokenKind.RegExpLiteral;

            case TokenKind.Punctuator when _tokenizer._type == TokenType.Assign && "/=".Equals(_tokenizer._value.Value):
                _tokenizer._position -= 1;
                _tokenizer.ReadRegExp();
                goto case TokenKind.RegExpLiteral;

            case TokenKind.Punctuator when _tokenizer._type == TokenType.At && _tokenizerOptions.EcmaVersion == EcmaVersion.Experimental:
                EnterRecursion();
                return ExitRecursion(ParseDecoratedClassExpression(startMarker));

            default:
                return Unexpected<Expression>();
        }
    }

    private Expression ParseExprImport(ExpressionContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseExprImport = function`

        var startMarker = StartNode();

        // Consume `import` as an identifier for `import.meta`.
        // Because `ParseIdentifier(true)` doesn't check escape sequences, it needs the check of `ContainsEscape`.
        if (_tokenizer._containsEscape)
        {
            RaiseRecoverable(_tokenizer._start, "Escape sequence in keyword import");
        }

        Next();

        if (_tokenizer._type == TokenType.ParenLeft && (context & ExpressionContext.ForNew) == 0)
        {
            return ParseDynamicImport(startMarker);
        }
        else if (_tokenizer._type == TokenType.Dot)
        {
            var meta = FinishNode(startMarker, new Identifier(TokenType.Import.Label));

            return ParseImportMeta(startMarker, meta);
        }
        else
        {
            return Unexpected<Expression>();
        }
    }

    private ImportExpression ParseDynamicImport(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseDynamicImport = function`

        Next(); // skip `(`

        // Parse node.source.
        var source = ParseMaybeAssign(ref NullRef<DestructuringErrors>());

        Expression? attributes = null;
        if (_tokenizerOptions._ecmaVersion == EcmaVersion.Experimental
            && Eat(TokenType.Comma) && _tokenizer._type != TokenType.ParenRight)
        {
            attributes = ParseMaybeAssign(ref NullRef<DestructuringErrors>());
            if (Eat(TokenType.Comma))
            {
                AfterTrailingComma(TokenType.ParenRight, notNext: true);
            }
            Expect(TokenType.ParenRight);
        }
        // Verify ending.
        else if (!Eat(TokenType.ParenRight))
        {
            var errorPos = _tokenizer._start;

            if (Eat(TokenType.Comma) && Eat(TokenType.ParenRight))
            {
                RaiseRecoverable(errorPos, "Trailing comma is not allowed in import()");
            }
            else
            {
                Unexpected(errorPos);
            }
        }

        return FinishNode(startMarker, new ImportExpression(source, attributes));
    }

    private MetaProperty ParseImportMeta(in Marker startMarker, Identifier meta)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseImportMeta = function`

        Next(); // skip `.`

        var containsEsc = _tokenizer._containsEscape;
        var property = ParseIdentifier(liberal: true);

        if (property.Name != "meta")
        {
            RaiseRecoverable(property.Start, "The only valid meta property for import is 'import.meta'");
        }
        if (containsEsc)
        {
            RaiseRecoverable(property.Start, "'import.meta' must not contain escaped characters");
        }
        if (!_inModule && !_options._allowImportExportEverywhere)
        {
            RaiseRecoverable(property.Start, "Cannot use 'import.meta' outside a module");
        }

        return FinishNode(startMarker, new MetaProperty(meta, property));
    }

    private Expression ParseParenExpression()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseParenExpression = function`

        Expect(TokenType.ParenLeft);
        var val = ParseExpression(ref NullRef<DestructuringErrors>());
        Expect(TokenType.ParenRight);
        return val;
    }

    private Expression ParseParenAndDistinguishExpression(bool canBeArrow, ExpressionContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseParenAndDistinguishExpression = function`

        var startMarker = StartNode();
        Expression val;
        var allowTrailingComma = _tokenizerOptions._ecmaVersion >= EcmaVersion.ES8;

        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6)
        {
            Next();

            var innerStartMarker = StartNode();
            var parameters = new ArrayList<Node>();

            var destructuringErrors = new DestructuringErrors();
            var oldYieldPos = _yieldPosition;
            var oldAwaitPos = _awaitPosition;
            // Do not save awaitIdentPos to allow checking awaits nested in parameters
            _yieldPosition = _awaitPosition = 0;

            var spreadStart = -1;

            var first = true;
            var lastIsComma = false;
            while (_tokenizer._type != TokenType.ParenRight)
            {
                if (!first)
                {
                    Expect(TokenType.Comma);
                }
                else
                {
                    first = false;
                }

                if (allowTrailingComma && AfterTrailingComma(TokenType.ParenRight, notNext: true))
                {
                    lastIsComma = true;
                    break;
                }
                else if (_tokenizer._type == TokenType.Ellipsis)
                {
                    spreadStart = _tokenizer._start;
                    parameters.Add(ParseRestBinding());
                    if (_tokenizer._type == TokenType.Comma)
                    {
                        Raise(_tokenizer._start, "Comma is not permitted after the rest element");
                    }
                    break;
                }
                else
                {
                    parameters.Add(ParseMaybeAssign(ref destructuringErrors, ExpressionContext.Default));
                }
            }

            var innerEndMarker = new Marker(_tokenizer._lastTokenEnd, _tokenizer._lastTokenEndLocation);
            Expect(TokenType.ParenRight);

            if (canBeArrow && !CanInsertSemicolon() && Eat(TokenType.Arrow))
            {
                CheckPatternErrors(ref destructuringErrors, isAssign: false);
                CheckYieldAwaitInDefaultParams();
                _yieldPosition = oldYieldPos;
                _awaitPosition = oldAwaitPos;

                return ParseArrowExpression(startMarker, NodeList.From(ref parameters), isAsync: false, context);
            }

            if (parameters.Count == 0 || lastIsComma)
            {
                Unexpected(_tokenizer._lastTokenStart);
            }
            if (spreadStart >= 0)
            {
                Unexpected(spreadStart);
            }
            CheckExpressionErrors(ref destructuringErrors, andThrow: true);

            if (oldYieldPos != 0)
            {
                _yieldPosition = oldYieldPos;
            }
            if (oldAwaitPos != 0)
            {
                _awaitPosition = oldAwaitPos;
            }

            if (parameters.Count > 1)
            {
                var exprList = new ArrayList<Expression>(new Expression[parameters.Count]);
                for (var i = 0; i < parameters.Count; i++)
                {
                    exprList[i] = (Expression)parameters[i];
                }
                val = FinishNodeAt(innerStartMarker, innerEndMarker, new SequenceExpression(NodeList.From(ref exprList)));
            }
            else
            {
                val = (Expression)parameters[0];
            }
        }
        else
        {
            val = ParseParenExpression();
        }

        return _options._preserveParens
            ? FinishNode(startMarker, new ParenthesizedExpression(val))
            : val;
    }

    // New's precedence is slightly tricky. It must allow its argument to
    // be a `[]` or dot subscript expression, but not a call — at least,
    // not without wrapping it in parentheses. Thus, it uses the noCalls
    // argument to parseSubscripts to prevent it from consuming the
    // argument list.
    private Expression ParseNew()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseNew = function`

        if (_tokenizer._containsEscape)
        {
            RaiseRecoverable(_tokenizer._start, "Escape sequence in keyword new");
        }

        var startMarker = StartNode();
        Next();

        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6 && _tokenizer._type == TokenType.Dot)
        {
            var meta = FinishNode(startMarker, new Identifier(TokenType.New.Label));
            Next();

            var containsEsc = _tokenizer._containsEscape;
            var property = ParseIdentifier(liberal: true);
            if (property.Name != "target")
            {
                RaiseRecoverable(property.Start, "The only valid meta property for new is 'new.target'");
            }
            if (containsEsc)
            {
                RaiseRecoverable(startMarker.Index, "'new.target' must not contain escaped characters");
            }
            if (!AllowNewDotTarget())
            {
                RaiseRecoverable(startMarker.Index, "'new.target' can only be used in functions and class static block");
            }

            return FinishNode(startMarker, new MetaProperty(meta, property));
        }

        var calleeStartMarker = StartNode();
        var callee = ParseSubscripts(calleeStartMarker, ParseExprAtom(ref NullRef<DestructuringErrors>(), ExpressionContext.ForNew), noCalls: true, ExpressionContext.Default);

        var arguments = Eat(TokenType.ParenLeft)
            ? ParseExprList(close: TokenType.ParenRight, allowTrailingComma: _tokenizerOptions._ecmaVersion >= EcmaVersion.ES8, allowEmptyItem: false, ref NullRef<DestructuringErrors>())!
            : new NodeList<Expression>();

        return FinishNode(startMarker, new NewExpression(callee, arguments));
    }

    // Parse template expression.
    private TemplateElement ParseTemplateElement()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseTemplateElement = function`

        var startMarker = StartNode();

        var value = new TemplateValue(cooked: _tokenizer._value.TemplateCooked, raw: (string)_tokenizer._value.Value!);

        Next();
        var tail = _tokenizer._type == TokenType.BackQuote;
        return FinishNode(startMarker, new TemplateElement(value, tail));
    }

    private TemplateLiteral ParseTemplate(bool isTagged)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseTemplate = function`

        var startMarker = StartNode();
        Next(requireValidEscapeSequenceInTemplate: !isTagged);

        var currentElement = ParseTemplateElement();
        var quasis = new ArrayList<TemplateElement> { currentElement };
        var expressions = new ArrayList<Expression>();
        while (!currentElement.Tail)
        {
            if (_tokenizer._type == TokenType.EOF)
            {
                Raise(_tokenizer._position, "Unterminated template literal");
            }

            Expect(TokenType.DollarBraceLeft);
            expressions.Add(ParseExpression(ref NullRef<DestructuringErrors>()));
            if (_tokenizer._type == TokenType.BraceRight)
            {
                Next(requireValidEscapeSequenceInTemplate: !isTagged);
            }
            else
            {
                Unexpected();
            }
            quasis.Add(currentElement = ParseTemplateElement());
        }

        Next();
        return FinishNode(startMarker, new TemplateLiteral(NodeList.From(ref quasis), NodeList.From(ref expressions)));
    }

    private bool IsAsyncProperty(Expression key, bool computed)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.isAsyncProp = function`

        return !computed && key is Identifier { Name: "async" }
            && (_tokenizer._type == TokenType.Name
                || _tokenizer._type == TokenType.BracketLeft
                || _tokenizer._type.Kind is TokenKind.StringLiteral or TokenKind.NumericLiteral or TokenKind.BigIntLiteral
                || _tokenizer._type.Keyword is not null
                || _tokenizerOptions._ecmaVersion >= EcmaVersion.ES9 && _tokenizer._type == TokenType.Star)
            && !Tokenizer.ContainsLineBreak(_tokenizer._input.SliceBetween(_tokenizer._lastTokenEnd, _tokenizer._start));
    }

    // Parse an object literal or binding pattern.
    private Node ParseObject(bool isPattern, ref DestructuringErrors destructuringErrors)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseObj = function`

        var startMarker = StartNode();
        var properties = new ArrayList<Node>();

        Next();

        var first = true;
        var hasProto = false;
        Dictionary<string, int>? propHash = null;
        while (!Eat(TokenType.BraceRight))
        {
            if (!first)
            {
                Expect(TokenType.Comma);
                if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES5 && AfterTrailingComma(TokenType.BraceRight))
                {
                    break;
                }
            }
            else
            {
                first = false;
            }

            var property = ParseProperty(isPattern, ref destructuringErrors);
            if (!isPattern)
            {
                propHash ??= new Dictionary<string, int>();
                CheckPropertyClash(property, ref hasProto, ref propHash, ref destructuringErrors);
            }

            properties.Add(property);
        }

        return FinishNode<Node>(startMarker, isPattern
            ? new ObjectPattern(NodeList.From(ref properties))
            : new ObjectExpression(NodeList.From(ref properties)));
    }

    private Node ParseProperty(bool isPattern, ref DestructuringErrors destructuringErrors)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseProperty = function`

        var propertyStartMarker = StartNode();

        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES9 && Eat(TokenType.Ellipsis))
        {
            if (isPattern)
            {
                var argument = ParseIdentifier(liberal: false);
                if (_tokenizer._type == TokenType.Comma)
                {
                    RaiseRecoverable(_tokenizer._start, "Comma is not permitted after the rest element");
                }

                return FinishNode(propertyStartMarker, new RestElement(argument));
            }
            else
            {
                // Parse argument.
                var argument = ParseMaybeAssign(ref destructuringErrors, ExpressionContext.Default);

                // To disallow trailing comma via `ToAssignable()`.
                if (_tokenizer._type == TokenType.Comma && !IsNullRef(ref destructuringErrors) && destructuringErrors.TrailingComma < 0)
                {
                    destructuringErrors.TrailingComma = _tokenizer._start;
                }

                // Finish
                return FinishNode(propertyStartMarker, new SpreadElement(argument));
            }
        }

        bool isGenerator, isAsync;
        Marker startMarker;
        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6)
        {
            startMarker = isPattern || !IsNullRef(ref destructuringErrors)
                ? StartNode()
                : default;
            isGenerator = !isPattern && Eat(TokenType.Star);
        }
        else
        {
            startMarker = default;
            isGenerator = false;
        }

        var containsEsc = _tokenizer._containsEscape;
        var key = ParsePropertyName(out var computed);
        if (!isPattern && !containsEsc && !isGenerator && _tokenizerOptions._ecmaVersion >= EcmaVersion.ES8 && IsAsyncProperty(key, computed))
        {
            isAsync = true;
            isGenerator = _tokenizerOptions._ecmaVersion >= EcmaVersion.ES9 && Eat(TokenType.Star);
            key = ParsePropertyName(out computed);
        }
        else
        {
            isAsync = false;
        }

        var value = ParsePropertyValue(ref key, ref computed, out var kind, out var method, out var shorthand, isPattern, isGenerator, isAsync,
            containsEsc, startMarker, ref destructuringErrors);

        Debug.Assert(!isPattern || kind == PropertyKind.Init);
        return FinishNode<Property>(propertyStartMarker, isPattern
            ? new AssignmentProperty(key, value, computed, shorthand)
            : new ObjectProperty(kind, key, value, computed, method, shorthand));
    }

    private Expression ParseGetterSetter(ref Expression key, ref bool computed, PropertyKind kind)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseGetterSetter = function`

        key = ParsePropertyName(out computed);
        var value = ParseMethod(false);

        if (kind == PropertyKind.Get)
        {
            if (value.Params.Count != 0)
            {
                RaiseRecoverable(value.Start, "getter should have no params");
            }
        }
        else
        {
            if (value.Params.Count != 1)
            {
                RaiseRecoverable(value.Start, "setter should have exactly one param");
            }
            else if (value.Params[0] is RestElement)
            {
                RaiseRecoverable(value.Params[0].Start, "Setter cannot use rest params");
            }
        }

        return value;
    }

    private Node ParsePropertyValue(ref Expression key, ref bool computed, out PropertyKind kind, out bool method, out bool shorthand, bool isPattern, bool isGenerator, bool isAsync,
        bool containsEsc, in Marker startMarker, ref DestructuringErrors destructuringErrors)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parsePropertyValue = function`

        if ((isGenerator || isAsync) && _tokenizer._type == TokenType.Colon)
        {
            Unexpected();
        }

        Node value;

        if (Eat(TokenType.Colon))
        {
            value = isPattern ? ParseMaybeDefault(startMarker) : ParseMaybeAssign(ref destructuringErrors, ExpressionContext.Default);
            kind = PropertyKind.Init;
            method = shorthand = false;
        }
        else if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6 && _tokenizer._type == TokenType.ParenLeft)
        {
            if (isPattern)
            {
                Unexpected();
            }

            kind = PropertyKind.Init;
            method = true;
            shorthand = false;
            value = ParseMethod(isGenerator, isAsync);
        }
        else if (!isPattern && !containsEsc
            && !computed && _tokenizerOptions._ecmaVersion >= EcmaVersion.ES5 && key is Identifier { Name: "get" or "set" } identifier
            && _tokenizer._type != TokenType.Comma && _tokenizer._type != TokenType.BraceRight && _tokenizer._type != TokenType.Eq)
        {
            if (isGenerator || isAsync)
            {
                Unexpected();
            }

            kind = identifier.Name[0] == 'g' ? PropertyKind.Get : PropertyKind.Set;
            method = shorthand = false;
            value = ParseGetterSetter(ref key, ref computed, kind);
        }
        else if (!computed && _tokenizerOptions._ecmaVersion >= EcmaVersion.ES6 && key is Identifier keyIdentifier)
        {
            if (isGenerator || isAsync)
            {
                Unexpected();
            }

            CheckUnreserved(keyIdentifier);

            if (keyIdentifier.Name == "await" && _awaitIdentifierPosition == 0)
            {
                _awaitIdentifierPosition = startMarker.Index;
            }

            kind = PropertyKind.Init;
            if (isPattern)
            {
                value = ParseMaybeDefault(startMarker, key);
            }
            else if (_tokenizer._type == TokenType.Eq && !IsNullRef(ref destructuringErrors))
            {
                if (destructuringErrors.ShorthandAssign < 0)
                {
                    destructuringErrors.ShorthandAssign = _tokenizer._start;
                }

                value = ParseMaybeDefault(startMarker, key);
            }
            else
            {
                value = key;
            }
            method = false;
            shorthand = true;
        }
        else
        {
            Unexpected();
            value = default!;
            kind = default;
            method = shorthand = default;
        }

        return value;
    }

    private Expression ParsePropertyName(out bool computed)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parsePropertyName = function`

        Expression key;

        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6 && Eat(TokenType.BracketLeft))
        {
            computed = true;
            key = ParseMaybeAssign(ref NullRef<DestructuringErrors>());
            Expect(TokenType.BracketRight);
        }
        else
        {
            computed = false;
            key = _tokenizer._type.Kind is TokenKind.StringLiteral or TokenKind.NumericLiteral or TokenKind.BigIntLiteral
                ? ParseExprAtom(ref NullRef<DestructuringErrors>())
                : ParseIdentifier(liberal: _options._allowReserved != AllowReservedOption.Never);
        }

        return key;
    }

    // Parse object or class method.
    private FunctionExpression ParseMethod(bool isGenerator, bool isAsync = false, bool allowDirectSuper = false)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseMethod = function`

        Debug.Assert(!isGenerator || _tokenizerOptions._ecmaVersion >= EcmaVersion.ES6);
        Debug.Assert(!isAsync || _tokenizerOptions._ecmaVersion >= EcmaVersion.ES8);

        var startMarker = StartNode();

        var oldYieldPos = _yieldPosition;
        var oldAwaitPos = _awaitPosition;
        var oldAwaitIdentPos = _awaitIdentifierPosition;
        _yieldPosition = _awaitPosition = _awaitIdentifierPosition = 0;

        EnterScope(FunctionFlags(isAsync, isGenerator) | (allowDirectSuper ? ScopeFlags.Super | ScopeFlags.DirectSuper : ScopeFlags.Super));

        Expect(TokenType.ParenLeft);

        NodeList<Node> parameters = ParseBindingList(close: TokenType.ParenRight, allowEmptyElement: false, allowTrailingComma: _tokenizerOptions._ecmaVersion >= EcmaVersion.ES8)!;
        CheckYieldAwaitInDefaultParams();
        var body = ParseFunctionBody(id: null, parameters, isArrowFunction: false, isMethod: true, ExpressionContext.Default, out _);

        _yieldPosition = oldYieldPos;
        _awaitPosition = oldAwaitPos;
        _awaitIdentifierPosition = oldAwaitIdentPos;

        return FinishNode(startMarker, new FunctionExpression(id: null, parameters, (FunctionBody)body, isGenerator, isAsync));
    }

    // Parse arrow function expression with given parameters.

    private ArrowFunctionExpression ParseArrowExpression(in Marker startMarker, in NodeList<Node> parameters, bool isAsync, ExpressionContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseArrowExpression = function`

        Debug.Assert(!isAsync || _tokenizerOptions._ecmaVersion >= EcmaVersion.ES8);

        var oldYieldPos = _yieldPosition;
        var oldAwaitPos = _awaitPosition;
        var oldAwaitIdentPos = _awaitIdentifierPosition;
        _yieldPosition = _awaitPosition = _awaitIdentifierPosition = 0;

        EnterScope(FunctionFlags(isAsync, generator: false) | ScopeFlags.Arrow);

        NodeList<Node> paramList = ToAssignableList(parameters!, isBinding: true)!;
        var body = ParseFunctionBody(id: null, paramList, isArrowFunction: true, isMethod: false, context, out var expression);

        _yieldPosition = oldYieldPos;
        _awaitPosition = oldAwaitPos;
        _awaitIdentifierPosition = oldAwaitIdentPos;

        return FinishNode(startMarker, new ArrowFunctionExpression(paramList, body, expression, isAsync));
    }

    // Parse function body and check parameters.
    private StatementOrExpression ParseFunctionBody(Identifier? id, in NodeList<Node> parameters,
        bool isArrowFunction, bool isMethod, ExpressionContext context, out bool expression)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseFunctionBody = function`

        expression = isArrowFunction && _tokenizer._type != TokenType.BraceLeft;
        var strict = false;

        StatementOrExpression body;
        if (expression)
        {
            CheckParams(parameters, allowDuplicates: false);
            body = ParseMaybeAssign(ref NullRef<DestructuringErrors>(), context);
        }
        else
        {
            var startMarker = StartNode();

            var nonSimple = _tokenizerOptions._ecmaVersion >= EcmaVersion.ES7 && !IsSimpleParamList(parameters);

            var oldStrict = _strict;
            Expect(TokenType.BraceLeft);
            var statements = ParseDirectivePrologue(allowStrictDirective: !nonSimple);
            strict = _strict;

            // Add the params to varDeclaredNames to ensure that an error is thrown
            // if a let/const declaration in the function clashes with one of the params.
            CheckParams(parameters, allowDuplicates: !oldStrict && !strict && !isArrowFunction && !isMethod && IsSimpleParamList(parameters));

            // Ensure the function name isn't a forbidden identifier in strict mode, e.g. 'eval'
            if (strict && id is not null)
            {
                CheckLValSimple(id, BindingType.Outside);
            }

            // Start a new scope with regard to labels and the `inFunction`
            // flag (restore them to their old value afterwards).
            var oldLabels = _labels;
            _labels = new ArrayList<Label>();
            ParseBlock(ref statements, createNewLexicalScope: false, exitStrict: strict && !oldStrict);
            _labels = oldLabels;

            body = FinishNode(startMarker, new FunctionBody(NodeList.From(ref statements), strict));
        }

        ExitScope();

        return body;
    }

    private static bool IsSimpleParamList(in NodeList<Node> parameters)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.isSimpleParamList = function`

        for (var i = 0; i < parameters.Count; i++)
        {
            if (parameters[i].Type != NodeType.Identifier)
            {
                return false;
            }
        }

        return true;
    }

    // Checks function params for various disallowed patterns such as using "eval"
    // or "arguments" and duplicate parameters.
    private void CheckParams(in NodeList<Node> parameters, bool allowDuplicates)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.checkParams = function`

        var nameHash = allowDuplicates ? null : new HashSet<string>();
        for (var i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];
            Debug.Assert(param is not null);
            CheckLValInnerPattern(param!, BindingType.Var, checkClashes: nameHash);
        }
    }

    // Parses a comma-separated list of expressions, and returns them as
    // an array. `close` is the token type that ends the list, and
    // `allowEmpty` can be turned on to allow subsequent commas with
    // nothing in between them to be parsed as `null` (which is needed
    // for array literals).
    private NodeList<Expression?> ParseExprList(TokenType close, bool allowTrailingComma, bool allowEmptyItem, ref DestructuringErrors destructuringErrors)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseExprList = function`

        var elements = new ArrayList<Expression?>();
        var first = true;
        while (!Eat(close))
        {
            if (!first)
            {
                Expect(TokenType.Comma);
                if (allowTrailingComma && AfterTrailingComma(close))
                {
                    break;
                }
            }
            else
            {
                first = false;
            }

            Expression? element;
            if (allowEmptyItem && _tokenizer._type == TokenType.Comma)
            {
                element = null;
            }
            else if (_tokenizer._type == TokenType.Ellipsis)
            {
                element = ParseSpread(ref destructuringErrors);
                if (!IsNullRef(ref destructuringErrors) && _tokenizer._type == TokenType.Comma && destructuringErrors.TrailingComma < 0)
                {
                    destructuringErrors.TrailingComma = _tokenizer._start;
                }
            }
            else
            {
                element = ParseMaybeAssign(ref destructuringErrors, ExpressionContext.Default);
            }

            elements.Add(element);
        }

        return NodeList.From(ref elements);
    }

    private void CheckUnreserved(Identifier id)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.checkUnreserved = function`

        var name = id.Name;

        if (InGenerator() && name == "yield")
        {
            RaiseRecoverable(id.Start, "Can not use 'yield' as identifier inside a generator");
        }

        if (InAsync() && name == "await")
        {
            RaiseRecoverable(id.Start, "Can not use 'await' as identifier inside an async function");
        }

        if (InClassFieldInit() && name == "arguments")
        {
            RaiseRecoverable(id.Start, "Cannot use 'arguments' in class field initializer");
        }

        if (InClassStaticBlock() && (name is "arguments" or "await"))
        {
            Raise(id.Start, $"Cannot use {name} in class static initialization block");
        }

        ReadOnlySpan<char> nameSpan = name.AsSpan();
        if (IsKeyword(nameSpan, _tokenizerOptions._ecmaVersion))
        {
            Raise(id.Start, $"Unexpected keyword '{name}'");
        }

        if (_tokenizerOptions._ecmaVersion < EcmaVersion.ES6
            && _tokenizer._input.SliceBetween(id.Start, id.End).IndexOf('\\') >= 0)
        {
            return;
        }

        if (_isReservedWord(nameSpan, _strict))
        {
            if (!InAsync() && name == "await")
            {
                RaiseRecoverable(id.Start, "Can not use keyword 'await' outside an async function");
            }

            RaiseRecoverable(id.Start, $"The keyword '{name}' is reserved");
        }
    }

    // Parse the next token as an identifier. If `liberal` is true (used
    // when parsing properties), it will also convert keywords into
    // identifiers.
    private Identifier ParseIdentifier(bool liberal = false)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseIdent = function`, `pp.parseIdentNode = function`

        // NOTE: `parseIdentNode` was merged into this method.

        var startMarker = StartNode();

        string name;
        if (_tokenizer._type == TokenType.Name)
        {
            name = (string)_tokenizer._value.Value!;
        }
        else if (_tokenizer._type.Keyword is not null)
        {
            name = _tokenizer._type.Label;

            // To fix https://github.com/acornjs/acorn/issues/575
            // `class` and `function` keywords push new context into `_state.ContextStack`.
            // But there is no chance to pop the context if the keyword is consumed as an identifier such as a property name.
            // If the previous token is a dot, this does not apply because the context-managing code already ignored the keyword
            if (_tokenizer._trackRegExpContext
                && _tokenizer._type.Keyword.Value is Keyword.Class or Keyword.Function
                && (_tokenizer._lastTokenEnd != _tokenizer._lastTokenStart + 1 || _tokenizer._input.CharCodeAt(_tokenizer._lastTokenStart) != '.'))
            {
                _tokenizer._contextStack.Pop();
            }
            _tokenizer._type = TokenType.Name;
        }
        else
        {
            return Unexpected<Identifier>();
        }

        Next(ignoreEscapeSequenceInKeyword: liberal);

        var identifier = FinishNode(startMarker, new Identifier(name));

        if (!liberal)
        {
            CheckUnreserved(identifier);

            if (identifier.Name == "await" && _awaitIdentifierPosition == 0)
            {
                _awaitIdentifierPosition = startMarker.Index;
            }
        }

        return identifier;
    }

    private PrivateIdentifier ParsePrivateIdentifier()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parsePrivateIdent = function`

        var startMarker = StartNode();

        if (_tokenizer._type != TokenType.PrivateId)
        {
            Unexpected();
        }

        var name = (string)_tokenizer._value.Value!;

        Next();
        var privateIdentifier = FinishNode(startMarker, new PrivateIdentifier(name));

        // For validating existence
        if (_options._checkPrivateFields)
        {
            if (_privateNameStack.Count == 0)
            {
                Raise(privateIdentifier.Start, $"Private field '#{name}' must be declared in an enclosing class");
            }
            else
            {
                _privateNameStack.PeekRef().Used.Add(privateIdentifier);
            }
        }

        return privateIdentifier;
    }

    private YieldExpression ParseYield(ExpressionContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseYield = function`

        if (_yieldPosition == 0)
        {
            _yieldPosition = _tokenizer._start;
        }

        var startMarker = StartNode();
        Next();

        bool @delegate;
        Expression? argument;
        if (_tokenizer._type == TokenType.Semicolon || CanInsertSemicolon()
            || !_tokenizer._type.StartsExpression && _tokenizer._type != TokenType.Star
                && !(_tokenizer._type == TokenType.Slash || _tokenizer._type == TokenType.Assign && "/=".Equals(_tokenizer._value.Value)))
        {
            @delegate = false;
            argument = null;
        }
        else
        {
            @delegate = Eat(TokenType.Star);
            argument = ParseMaybeAssign(ref NullRef<DestructuringErrors>(), context);
        }

        return FinishNode(startMarker, new YieldExpression(argument, @delegate));
    }

    private AwaitExpression ParseAwait(ExpressionContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/expression.js > `pp.parseAwait = function`

        if (_awaitPosition == 0)
        {
            _awaitPosition = _tokenizer._start;
        }

        var startMarker = StartNode();
        Next();

        var argument = ParseMaybeUnary(sawUnary: true, incDec: false, ref NullRef<DestructuringErrors>(), context);
        return FinishNode(startMarker, new AwaitExpression(argument));
    }

    private Expression ParseDecoratedClassExpression(in Marker node)
    {
        var previousDecorators = _decorators;
        _decorators = ParseDecorators();
        if (_tokenizer._type != TokenType.Class)
        {
            Unexpected();
        }
        var expression = ParseExprAtom(ref NullRef<DestructuringErrors>());
        _decorators = previousDecorators;

        return FinishNode(node, expression);
    }

    private ArrayList<Decorator> ParseDecorators()
    {
        var decorators = new ArrayList<Decorator>();

        do
        {
            decorators.Add(ParseDecorator());
        }
        while (_tokenizer._type == TokenType.At);

        return decorators;
    }

    private Decorator ParseDecorator()
    {
        var startMarker = StartNode();

        Next();

        var expression = ParseExprSubscripts(ref NullRef<DestructuringErrors>(), ExpressionContext.Decorator);

        if (_tokenizer._type == TokenType.Semicolon)
        {
            Raise(_tokenizer._start, "Decorators must not be followed by a semicolon.");
        }

        return FinishNode(startMarker, new Decorator(expression));
    }
}
