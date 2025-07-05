using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static Unsafe;
using static SyntaxErrorMessages;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js

public partial class Parser
{
    // Parse a program. Initializes the parser, reads any number of
    // statements, and wraps them in a Program node.  Optionally takes a
    // `program` argument. If present, the statements will be appended
    // to its body instead of creating a new node.

    private NodeList<Statement> ParseTopLevel()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseTopLevel = function`

        Next();

        var body = ParseDirectivePrologue(allowStrictDirective: true);
        while (_tokenizer._type != TokenType.EOF)
        {
            var statement = ParseStatement(StatementContext.Default, topLevel: true);
            body.Add(statement);
        }

        if (_inModule && _undefinedExports is { Count: > 0 } undefinedExports)
        {
            foreach (var kvp in undefinedExports)
            {
                // RaiseRecoverable(kvp.Value, $"Export '{kvp.Key}' is not defined"); // original acornjs error reporting
                Raise(kvp.Value, ModuleExportUndefined, new object[] { kvp.Key });
            }
        }

        return NodeList.From(ref body);
    }

    private bool IsLet(StatementContext context = StatementContext.Default)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.isLet = function`

        if (_tokenizerOptions._ecmaVersion < EcmaVersion.ES6 || !IsContextual("let"))
        {
            return false;
        }

        var next = _tokenizer.NextTokenPosition();
        var nextCh = _tokenizer.FullCharCodeAt(next);

        // For ambiguous cases, determine if a LexicalDeclaration (or only a
        // Statement) is allowed here. If context is not empty then only a Statement
        // is allowed. However, `let [` is an explicit negative lookahead for
        // ExpressionStatement, so special-case it first.

        if (nextCh is '[' or '\\')
        {
            return true;
        }

        if (context != StatementContext.Default)
        {
            return false;
        }

        if (nextCh is '{' || ((char)nextCh).IsHighSurrogate())
        {
            return true;
        }

        if (Tokenizer.IsIdentifierStart(nextCh, allowAstral: true))
        {
            var pos = next + 1;
            while (Tokenizer.IsIdentifierChar(nextCh = _tokenizer.FullCharCodeAt(pos), allowAstral: true))
            {
                ++pos;
            }

            if (nextCh is '\\' || ((char)nextCh).IsHighSurrogate())
            {
                return true;
            }

            if (!IsKeywordRelationalOperator(_tokenizer._input.SliceBetween(next, pos)))
            {
                return true;
            }
        }

        return false;
    }

    // check 'async [no LineTerminator here] function'
    // - 'async /*foo*/ function' is OK.
    // - 'async /*\n*/ function' is invalid.
    private bool IsAsyncFunction()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.isAsyncFunction = function`

        if (_tokenizerOptions._ecmaVersion < EcmaVersion.ES8 || !IsContextual("async"))
        {
            return false;
        }

        var next = _tokenizer.NextTokenPosition();
        int after;

        if (!Tokenizer.ContainsLineBreak(_tokenizer._input.SliceBetween(_tokenizer._position, next)))
        {
            var keyword = TokenType.Function.Label;
            var endIndex = next + keyword.Length;

            if (endIndex <= _tokenizer._endPosition
                && _tokenizer._input.AsSpan(next, keyword.Length).SequenceEqual(keyword.AsSpan()))
            {
                return endIndex == _tokenizer._endPosition
                    || !(Tokenizer.IsIdentifierChar(after = _tokenizer._input.CharCodeAt(endIndex)) || ((char)after).IsHighSurrogate());
            }
        }

        return false;
    }

    private bool IsUsingKeyword(bool isAwaitUsing, bool isFor)
    {
        // https://github.com/acornjs/acorn/blob/8bcc8974b518bb5c543e81cacf57bf86b5739e4d/acorn/src/statement.js > `pp.isUsingKeyword = function`

        if (!_tokenizerOptions.AllowExplicitResourceManagement() || !IsContextual(isAwaitUsing ? "await" : "using"))
        {
            return false;
        }

        var next = _tokenizer.NextTokenPosition();

        if (Tokenizer.ContainsLineBreak(_tokenizer._input.SliceBetween(_tokenizer._position, next)))
        {
            return false;
        }

        if (isAwaitUsing)
        {
            var awaitEndPos = next + 5 /* await */;
            int after;
            if (_tokenizer._input.SliceBetween(next, awaitEndPos) is not "using"
                || awaitEndPos == _tokenizer._input.Length
                || Tokenizer.IsIdentifierChar(after = _tokenizer._input.CharCodeAt(awaitEndPos))
                || after is > 0xd7ff and < 0xdc00)
            {
                return false;
            }

            next = _tokenizer.NextTokenPosition();
            if (Tokenizer.ContainsLineBreak(_tokenizer._input.SliceBetween(_tokenizer._position, next)))
            {
                return false;
            }
        }

        var ch = _tokenizer.FullCharCodeAt(next);
        if (!Tokenizer.IsIdentifierStart(ch, true) && ch != '\\')
        {
            return false;
        }

        var idStart = next;
        do
        {
            next += ch <= 0xffff ? 1 : 2;
        } while (Tokenizer.IsIdentifierChar(ch = _tokenizer.FullCharCodeAt(next)));

        if (ch == '\\')
        {
            return true;
        }

        var id = _tokenizer._input.SliceBetween(idStart, next);
        if (IsKeywordRelationalOperator(id) || isFor && id is "of")
        {
            return false;
        }

        return true;
    }

    private bool IsAwaitUsing(bool isFor)
    {
        return IsUsingKeyword(isAwaitUsing: true, isFor);
    }

    private bool IsUsing(bool isFor)
    {
        return IsUsingKeyword(isAwaitUsing: false, isFor);
    }

    // Parse a single statement.
    //
    // If expecting a statement and finding a slash operator, parse a
    // regular expression literal. This is to handle cases like
    // `if (foo) /blah/.exec(foo)`, where looking at the previous token
    // does not help.
    private Statement ParseStatement(StatementContext context, bool topLevel = false)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseStatement = function`

        EnterRecursion();

        var startMarker = StartNode();

        bool hasContext;
        TokenType startType;
        VariableDeclarationKind kind;

        if (IsLet(context))
        {
            startType = TokenType.Var;
            kind = VariableDeclarationKind.Let;
        }
        else
        {
            startType = _tokenizer._type;
            kind = VariableDeclarationKind.Unknown;
        }

        // Most types of statements are recognized by the keyword they
        // start with. Many are trivial to parse, some require a bit of
        // complexity.

        if (startType.Keyword is not null)
        {
            switch (startType.Keyword.Value)
            {
                case Keyword.Break or Keyword.Continue:
                    return ExitRecursion(ParseBreakContinueStatement(startMarker, startType));

                case Keyword.Debugger:
                    return ExitRecursion(ParseDebuggerStatement(startMarker));

                case Keyword.Do:
                    return ExitRecursion(ParseDoStatement(startMarker));

                case Keyword.For:
                    return ExitRecursion(ParseForStatement(startMarker));

                case Keyword.Function:
                    // Function as sole body of either an if statement or a labeled statement
                    // works, but not when it is part of a labeled statement that is the sole
                    // body of an if statement.

                    hasContext = context != StatementContext.Default;
                    if (hasContext && _tokenizerOptions._ecmaVersion >= EcmaVersion.ES6)
                    {
                        if (_strict)
                        {
                            // Unexpected(); // original acornjs error reporting
                            Raise(_tokenizer._start, StrictFunction);
                        }
                        else if (context is not (StatementContext.If or StatementContext.Label))
                        {
                            // Unexpected(); // original acornjs error reporting
                            Raise(_tokenizer._start, SloppyFunction);

                        }
                    }

                    return ExitRecursion(ParseFunctionStatement(startMarker, isAsync: false, declarationPosition: !hasContext));

                case Keyword.Class:
                    if (context != StatementContext.Default)
                    {
                        Unexpected();
                    }

                    return ExitRecursion(ParseClassStatement(startMarker));

                case Keyword.If:
                    return ExitRecursion(ParseIfStatement(startMarker));

                case Keyword.Return:
                    return ExitRecursion(ParseReturnStatement(startMarker));

                case Keyword.Switch:
                    return ExitRecursion(ParseSwitchStatement(startMarker));

                case Keyword.Throw:
                    return ExitRecursion(ParseThrowStatement(startMarker));

                case Keyword.Try:
                    return ExitRecursion(ParseTryStatement(startMarker));

                case Keyword.Const or Keyword.Var:
                    if (kind == VariableDeclarationKind.Unknown)
                    {
                        kind = startType.Keyword.Value == Keyword.Var ? VariableDeclarationKind.Var : VariableDeclarationKind.Const;
                    }

                    if (context != StatementContext.Default && kind != VariableDeclarationKind.Var)
                    {
                        Unexpected();
                    }

                    return ExitRecursion(ParseVarStatement(startMarker, kind));

                case Keyword.While:
                    return ExitRecursion(ParseWhileStatement(startMarker));

                case Keyword.With:
                    return ExitRecursion(ParseWithStatement(startMarker));

                case Keyword.Export or Keyword.Import:
                    if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES10 && startType.Keyword.Value == Keyword.Import)
                    {
                        var next = _tokenizer.NextTokenPosition();
                        var nextCh = _tokenizer._input.CharCodeAt(next, _tokenizer._endPosition);
                        if (nextCh is '(' or '.')
                        {
                            return ExitRecursion(ParseExpressionStatement(startMarker, ParseExpression(ref NullRef<DestructuringErrors>())));
                        }
                    }

                    if (!_options._allowImportExportEverywhere)
                    {
                        if (!topLevel)
                        {
                            // Raise(_tokenizer._start, "'import' and 'export' may only appear at the top level"); // original acornjs error reporting
                            Unexpected();
                        }

                        if (!_inModule)
                        {
                            // Raise(_tokenizer._start, "'import' and 'export' may appear only with 'sourceType: module'"); // original acornjs error reporting
                            if (startType.Keyword.Value == Keyword.Import)
                            {
                                Raise(_tokenizer._start, ImportOutsideModule);
                            }
                            else
                            {
                                Unexpected();
                            }
                        }
                    }

                    if (startType == TokenType.Import)
                    {
                        return ExitRecursion(ParseImportDeclaration(startMarker));
                    }
                    else
                    {
                        return ExitRecursion(ParseExport(startMarker, topLevel ? _exports : null));
                    }
            }
        }
        else if (startType == TokenType.BraceLeft)
        {
            return ExitRecursion(ParseBlockStatement(startMarker));
        }
        else if (startType == TokenType.Semicolon)
        {
            return ExitRecursion(ParseEmptyStatement(startMarker));
        }
        else if (IsAsyncFunction())
        {
            hasContext = context != StatementContext.Default;
            if (hasContext)
            {
                // Unexpected(); // original acornjs error reporting
                Raise(_tokenizer._start, AsyncFunctionInSingleStatementContext);
            }

            Next();
            return ExitRecursion(ParseFunctionStatement(startMarker, isAsync: true, declarationPosition: !hasContext));
        }
        else if (startType == TokenType.At)
        {
            return ExitRecursion(ParseDecoratedClassStatement(startMarker));
        }

        VariableDeclarationKind usingKind = IsAwaitUsing(isFor: false)
            ? VariableDeclarationKind.AwaitUsing
            : IsUsing(isFor: false) ? VariableDeclarationKind.Using : VariableDeclarationKind.Unknown;

        if (usingKind != VariableDeclarationKind.Unknown)
        {
            if (!AllowUsing)
            {
                Raise(_tokenizer._start, UsingInTopLevel);
            }
            if (usingKind == VariableDeclarationKind.AwaitUsing)
            {
                if (!CanAwait)
                {
                    Raise(_tokenizer._start, AwaitNotInAsyncContext);
                }

                Next();
            }

            Next();
            var variableDeclaration = ParseVar(usingKind, isFor: false);
            Semicolon();
            return ExitRecursion(FinishNode(startMarker, variableDeclaration));
        }

        var maybeName = _tokenizer._value.Value;
        var expr = ParseExpression(ref NullRef<DestructuringErrors>());
        if (startType == TokenType.Name && expr is Identifier identifier && Eat(TokenType.Colon))
        {
            return ExitRecursion(ParseLabeledStatement(startMarker, (string)maybeName!, identifier, context));
        }
        else
        {
            return ExitRecursion(ParseExpressionStatement(startMarker, expr));
        }
    }

    private Statement ParseBreakContinueStatement(in Marker startMarker, TokenType keyword)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseBreakContinueStatement = function`

        Next();

        var isBreak = keyword == TokenType.Break;
        Identifier? label;
        if (Eat(TokenType.Semicolon) || InsertSemicolon())
        {
            label = null;

            // Verify that there is an actual destination to break or
            // continue to.
            for (var i = 0; i < _labels.Count; i++)
            {
                ref readonly var lab = ref _labels.GetItemRef(i);
                if (isBreak ? lab.Kind != LabelKind.None : lab.Kind == LabelKind.Loop)
                {
                    goto Success;
                }
            }

            // Raise(startMarker.Index, "Unsyntactic " + keyword.Label); // original acornjs error reporting
            if (isBreak)
            {
                Raise(startMarker.Index, IllegalBreak);
            }
            else
            {
                Raise(startMarker.Index, NoIterationStatement);
            }
        }
        else if (_tokenizer._type == TokenType.Name)
        {
            label = ParseIdentifier();
            Semicolon();

            // Verify that there is an actual destination to break or
            // continue to.
            for (var i = 0; i < _labels.Count; i++)
            {
                ref readonly var lab = ref _labels.GetItemRef(i);
                if (_labels.GetItemRef(i).Name == label.Name)
                {
                    if (!isBreak && lab.Kind != LabelKind.Loop)
                    {
                        // Raise(startMarker.Index, "Unsyntactic " + keyword.Label); // original acornjs error reporting
                        Raise(startMarker.Index, IllegalContinue, new object[] { label.Name });
                    }

                    goto Success;
                }
            }

            // Raise(startMarker.Index, "Unsyntactic " + keyword.Label); // original acornjs error reporting
            Raise(label.Start, UnknownLabel, new object[] { label.Name });
        }
        else
        {
            return Unexpected<Statement>();
        }

    Success:
        return FinishNode<Statement>(startMarker, isBreak ? new BreakStatement(label) : new ContinueStatement(label));
    }

    private DebuggerStatement ParseDebuggerStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseDebuggerStatement = function`

        Next();
        Semicolon();

        return FinishNode(startMarker, new DebuggerStatement());
    }

    private DoWhileStatement ParseDoStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseDoStatement = function`

        Next();

        _labels.PushRef().Reset(LabelKind.Loop);
        var body = ParseStatement(StatementContext.Do);
        _labels.PopRef();

        Expect(TokenType.While);

        var test = ParseParenExpression();
        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6)
        {
            Eat(TokenType.Semicolon);
        }
        else
        {
            Semicolon();
        }

        return FinishNode(startMarker, new DoWhileStatement(body, test));
    }

    // Disambiguating between a `for` and a `for`/`in` or `for`/`of`
    // loop is non-trivial. Basically, we have to parse the init `var`
    // statement or expression, disallowing the `in` operator (see
    // the second parameter to `parseExpression`), and then check
    // whether the next token is `in` or `of`. When there is no init
    // part (semicolon immediately after the opening parenthesis), it
    // is a regular `for` loop.
    private Statement ParseForStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseForStatement = function`

        Next();

        int awaitAt;
        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES9 && IsContextual("await"))
        {
            if (!CanAwait)
            {
                Raise(_tokenizer._start, UnexpectedReserved);
            }
            awaitAt = _tokenizer._start;
            Next();
        }
        else
        {
            awaitAt = -1;
        }

        _labels.PushRef().Reset(LabelKind.Loop);
        EnterScope(ScopeFlags.None);

        Expect(TokenType.ParenLeft);

        if (_tokenizer._type == TokenType.Semicolon)
        {
            if (awaitAt >= 0)
            {
                Unexpected(awaitAt, TokenType.Name, "await");
            }

            return ParseFor(startMarker, init: null);
        }

        var isLet = false;
        StatementOrExpression? init;
        if (_tokenizer._type == TokenType.Var || _tokenizer._type == TokenType.Const || (isLet = IsLet()))
        {
            var initStartMarker = StartNode();
            VariableDeclarationKind kind;
            if (isLet)
                kind = VariableDeclarationKind.Let;
            else
                kind = _tokenizer._type.Keyword!.Value == Keyword.Var ? VariableDeclarationKind.Var : VariableDeclarationKind.Const;

            Next();

            var initDeclaration = FinishNode(initStartMarker, ParseVar(kind, isFor: true));

            if (_tokenizer._type == TokenType.In)
            {
                if (initDeclaration.Declarations.Count != 1)
                {
                    // This error is not reported by the original acornjs implementation.
                    Raise(initDeclaration.Start, ForInOfLoopMultiBindings, new object[] { "for-in" });
                }

                if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES9 && awaitAt >= 0)
                {
                    Unexpected(awaitAt, TokenType.Name, "await");
                }

                return ParseForInOf(startMarker, isForIn: true, await: false, initDeclaration);
            }
            else if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6 && IsContextual("of"))
            {
                if (initDeclaration.Declarations.Count != 1)
                {
                    // This error is not reported by the original acornjs implementation.
                    Raise(initDeclaration.Start, ForInOfLoopMultiBindings, new object[] { "for-of" });
                }

                return ParseForInOf(startMarker, isForIn: false, await: awaitAt >= 0, initDeclaration);
            }

            init = initDeclaration;
        }
        else
        {
            var startsWithLet = IsContextual("let");

            VariableDeclarationKind usingKind = IsUsing(isFor: true)
                ? VariableDeclarationKind.Using
                : IsAwaitUsing(true) ? VariableDeclarationKind.AwaitUsing : VariableDeclarationKind.Unknown;

            if (usingKind != VariableDeclarationKind.Unknown)
            {
                var marker = StartNode();
                Next();
                if (usingKind == VariableDeclarationKind.AwaitUsing)
                {
                    if (!CanAwait)
                    {
                        Raise(_tokenizer._start, AwaitNotInAsyncContext);
                    }

                    Next();
                }

                VariableDeclaration variableDeclaration = ParseVar(usingKind, isFor: true);
                FinishNode(marker, variableDeclaration);
                return ParseForAfterInit(marker, variableDeclaration, awaitAt);
            }


            var containsEscape = _tokenizer._containsEscape;
            var destructuringErrors = new DestructuringErrors();

            var oldForInitPosition = _forInitPosition;
            _forInitPosition = _tokenizer._start;

            init = awaitAt < 0
                ? ParseExpression(ref destructuringErrors, ExpressionContext.ForInit)
                : ParseExprSubscripts(ref destructuringErrors, ExpressionContext.AwaitForInit);

            // Swap variables using XOR
            _forInitPosition ^= oldForInitPosition;
            oldForInitPosition ^= _forInitPosition;
            _forInitPosition ^= oldForInitPosition;

            if (_tokenizer._type == TokenType.In)
            {
                if (awaitAt >= 0) // this implies _ecmaVersion >= EcmaVersion.ES9
                {
                    Unexpected(awaitAt, TokenType.Name, "await");
                }

                var initPattern = ToAssignable(init, ref destructuringErrors, isBinding: false, lhsKind: LeftHandSideKind.ForInOf);
                CheckLValPattern(initPattern, lhsKind: LeftHandSideKind.ForInOf);

                return ParseForInOf(startMarker, isForIn: true, await: false, initPattern);
            }
            else if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6 && IsContextual("of"))
            {
                if (awaitAt < 0 && _tokenizerOptions._ecmaVersion >= EcmaVersion.ES8
                    && oldForInitPosition == init.Start && !containsEscape && init is Identifier { Name: "async" })
                {
                    Raise(init.Start, ForOfAsync);
                }

                if (startsWithLet)
                {
                    // Raise(init.Start, "The left-hand side of a for-of loop may not start with 'let'"); // original acornjs error reporting
                    Raise(init.Start, ForOfLet);
                }

                var initPattern = ToAssignable(init, ref destructuringErrors, isBinding: false, lhsKind: LeftHandSideKind.ForInOf);
                CheckLValPattern(initPattern, lhsKind: LeftHandSideKind.ForInOf);

                return ParseForInOf(startMarker, isForIn: false, await: awaitAt >= 0, initPattern);
            }
            else if (awaitAt >= 0)
            {
                Unexpected();
            }

            CheckExpressionErrors(ref destructuringErrors, andThrow: true);
        }

        if (awaitAt >= 0)
        {
            Unexpected(awaitAt, TokenType.Name, "await");
        }

        return ParseFor(startMarker, init);
    }

    // Helper method to parse for loop after variable initialization
    private Statement ParseForAfterInit(in Marker node, VariableDeclaration init, int awaitAt)
    {
        // https://github.com/acornjs/acorn/blob/8.15.0/acorn/src/statement.js#L325 > `pp.parseForAfterInit = function`
        if ((_tokenizer._type == TokenType.In || (_options.EcmaVersion >= EcmaVersion.ES6 && IsContextual("of"))) && init.Declarations.Count == 1)
        {
            var await = false;
            if (_options.EcmaVersion >= EcmaVersion.ES9)
            {
                if (_tokenizer._type == TokenType.In)
                {
                    if (awaitAt > -1)
                    {
                        Unexpected(awaitAt, TokenType.Name, "await");
                    }
                }
                else
                {
                    await = awaitAt > -1;
                }
            }

            return ParseForInOf(node, isForIn: true, await, init);
        }

        if (awaitAt > -1)
        {
            Unexpected(awaitAt, TokenType.Name, "await");
        }
        return ParseFor(node, init);
    }

    private FunctionDeclaration ParseFunctionStatement(in Marker startMarker, bool isAsync, bool declarationPosition)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseFunctionStatement = function`

        Next();
        var flags = declarationPosition
            ? FunctionOrClassFlags.Statement
            : FunctionOrClassFlags.Statement | FunctionOrClassFlags.HangingStatement;
        return (FunctionDeclaration)ParseFunction(startMarker, flags, isAsync);
    }

    private ClassDeclaration ParseClassStatement(in Marker startMarker)
    {
        // NOTE: This method doesn't exist in acornjs, was added for consistency.

        Next();
        return (ClassDeclaration)ParseClass(startMarker, FunctionOrClassFlags.Statement);
    }

    private IfStatement ParseIfStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseIfStatement = function`

        Next();
        var test = ParseParenExpression();
        // allow function declarations in branches, but only in non-strict mode
        var consequent = ParseStatement(StatementContext.If);
        var alternate = Eat(TokenType.Else) ? ParseStatement(StatementContext.If) : null;

        return FinishNode(startMarker, new IfStatement(test, consequent, alternate));
    }

    private ReturnStatement ParseReturnStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseReturnStatement = function`

        if (!_options._allowReturnOutsideFunction && !InFunction)
        {
            // Raise(_tokenizer._start, "'return' outside of function"); // original acornjs error reporting
            Raise(_tokenizer._start, IllegalReturn);
        }

        Next();

        // In `return` (and `break`/`continue`), the keywords with
        // optional arguments, we eagerly look for a semicolon or the
        // possibility to insert one.

        Expression? argument;
        if (Eat(TokenType.Semicolon) || InsertSemicolon())
        {
            argument = null;
        }
        else
        {
            argument = ParseExpression(ref NullRef<DestructuringErrors>());
            Semicolon();
        }

        return FinishNode(startMarker, new ReturnStatement(argument));
    }

    private SwitchStatement ParseSwitchStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseSwitchStatement = function`

        Next();
        var discriminant = ParseParenExpression();

        var cases = new ArrayList<SwitchCase>();
        Expect(TokenType.BraceLeft);

        _labels.PushRef().Reset(LabelKind.Switch);
        EnterScope(ScopeFlags.Switch);

        var sawDefault = false;
        while (!Eat(TokenType.BraceRight))
        {
            var caseStartMarker = StartNode();

            Expression? test;
            if (_tokenizer._type == TokenType.Case)
            {
                Next();
                test = ParseExpression(ref NullRef<DestructuringErrors>());
            }
            else if (_tokenizer._type == TokenType.Default)
            {
                if (sawDefault)
                {
                    // RaiseRecoverable(_tokenizer._start, "Multiple default clauses"); // original acornjs error reporting
                    Raise(_tokenizer._start, MultipleDefaultsInSwitch);
                }
                else
                {
                    sawDefault = true;
                }

                Next();
                test = null;
            }
            else
            {
                return Unexpected<SwitchStatement>();
            }

            Expect(TokenType.Colon);

            var consequent = new ArrayList<Statement>();
            while (_tokenizer._type != TokenType.BraceRight && _tokenizer._type != TokenType.Case && _tokenizer._type != TokenType.Default)
            {
                consequent.Add(ParseStatement(StatementContext.Default));
            }

            var current = FinishNode(caseStartMarker, new SwitchCase(test, NodeList.From(ref consequent)));

            cases.Add(current);
        }

        var scope = ExitScope();
        _labels.PopRef();

        return FinishNode(startMarker, new SwitchStatement(discriminant, NodeList.From(ref cases)), scope);
    }

    private ThrowStatement ParseThrowStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseThrowStatement = function`

        Next();

        if (Tokenizer.ContainsLineBreak(_tokenizer._input.SliceBetween(_tokenizer._lastTokenEnd, _tokenizer._start)))
        {
            // Raise(_tokenizer._lastTokenEnd, "Illegal newline after throw"); // original acornjs error reporting
            RaiseRecoverable(_tokenizer._lastTokenEnd, NewlineAfterThrow);
        }

        var argument = ParseExpression(ref NullRef<DestructuringErrors>());
        Semicolon();

        return FinishNode(startMarker, new ThrowStatement(argument));
    }

    private Node ParseCatchClauseParam()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseCatchClauseParam = function`

        var param = ParseBindingAtom();

        ScopeFlags scopeFlags;
        BindingType bindingType;
        if (param.Type == NodeType.Identifier)
        {
            scopeFlags = ScopeFlags.SimpleCatch;
            bindingType = BindingType.SimpleCatch;
        }
        else
        {
            scopeFlags = ScopeFlags.None;
            bindingType = BindingType.Lexical;
        }

        EnterScope(scopeFlags);

        CheckLValPattern(param, bindingType);
        ref var lexicalList = ref CurrentScope._lexical;
        lexicalList.ParamCount = lexicalList.Count;

        Expect(TokenType.ParenRight);

        return param;
    }

    private TryStatement ParseTryStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseTryStatement = function`

        Next();
        var blockStartMarker = StartNode();
        var block = ParseBlockStatement(blockStartMarker);

        CatchClause? handler = null;
        if (_tokenizer._type == TokenType.Catch)
        {
            var clauseStartMarker = StartNode();

            Next();

            Node? param;
            if (Eat(TokenType.ParenLeft))
            {
                param = ParseCatchClauseParam();
            }
            else
            {
                if (_tokenizerOptions._ecmaVersion < EcmaVersion.ES10)
                {
                    Unexpected();
                }

                param = null;
                EnterScope(ScopeFlags.None);
            }

            blockStartMarker = StartNode();
            var body = ParseBlockStatement(blockStartMarker, createNewLexicalScope: false);
            var scope = ExitScope();

            handler = FinishNode(clauseStartMarker, new CatchClause(param, body), scope);
        }

        NestedBlockStatement? finalizer;
        if (Eat(TokenType.Finally))
        {
            blockStartMarker = StartNode();
            finalizer = ParseBlockStatement(blockStartMarker);
        }
        else
        {
            finalizer = null;
        }

        if (handler is null && finalizer is null)
        {
            // Raise(startMarker.Index, "Missing catch or finally clause"); // original acornjs error reporting
            Raise(_tokenizer._lastTokenEnd, NoCatchOrFinally);
        }

        return FinishNode(startMarker, new TryStatement(block, handler, finalizer));
    }

    private VariableDeclaration ParseVarStatement(in Marker startMarker, VariableDeclarationKind kind)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseVarStatement = function`

        Next();
        var declaration = ParseVar(kind, isFor: false);
        Semicolon();

        return FinishNode(startMarker, declaration);
    }

    private WhileStatement ParseWhileStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseWhileStatement = function`

        Next();
        var test = ParseParenExpression();

        _labels.PushRef().Reset(LabelKind.Loop);
        var body = ParseStatement(StatementContext.While);
        _labels.PopRef();

        return FinishNode(startMarker, new WhileStatement(test, body));
    }

    private WithStatement ParseWithStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseWithStatement = function`

        if (_strict)
        {
            // Raise(_tokenizer._start, "'with' in strict mode"); // original acornjs error reporting
            RaiseRecoverable(_tokenizer._start, StrictWith);
        }

        Next();
        var obj = ParseParenExpression();
        var body = ParseStatement(StatementContext.With);

        return FinishNode(startMarker, new WithStatement(obj, body));
    }

    private EmptyStatement ParseEmptyStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseEmptyStatement = function`

        Next();

        return FinishNode(startMarker, new EmptyStatement());
    }

    private LabeledStatement ParseLabeledStatement(in Marker startMarker, string maybeName, Identifier expr, StatementContext context)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseLabeledStatement = function`

        for (var i = 0; i < _labels.Count; i++)
        {
            ref readonly var label = ref _labels.GetItemRef(i);
            if (label.Name == maybeName)
            {
                // Raise(expr.Start, $"Label '{maybeName}' is already declared"); // original acornjs error reporting
                Raise(expr.Start, LabelRedeclaration, new object[] { maybeName });
            }
        }

        var kind = _tokenizer._type.IsLoop
            ? LabelKind.Loop
            : (_tokenizer._type == TokenType.Switch ? LabelKind.Switch : LabelKind.None);

        for (var i = _labels.Count - 1; i >= 0; i--)
        {
            ref var label = ref _labels.GetItemRef(i);
            if (label.StatementStart == startMarker.Index)
            {
                // Update information about previous labels on this node
                label.StatementStart = startMarker.Index;
                label.Kind = kind;
            }
            else
            {
                break;
            }
        }

        _labels.PushRef().Reset(kind, maybeName, _tokenizer._start);
        var body = ParseStatement(context | StatementContext.Label);
        _labels.PopRef();

        return FinishNode(startMarker, new LabeledStatement(expr, body));
    }

    private NonSpecialExpressionStatement ParseExpressionStatement(in Marker startMarker, Expression expression)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseExpressionStatement = function`

        Semicolon();

        return FinishNode(startMarker, new NonSpecialExpressionStatement(expression));
    }

    // Parse a semicolon-enclosed block of statements, handling `"use
    // strict"` declarations when `allowStrict` is true (used for
    // function bodies).
    private NestedBlockStatement ParseBlockStatement(in Marker startMarker, bool createNewLexicalScope = true)
    {
        // NOTE: This method doesn't exist in acornjs, was added for consistency.

        Expect(TokenType.BraceLeft);

        var statements = new ArrayList<Statement>();
        var scope = ParseBlock(ref statements, createNewLexicalScope);

        return FinishNode(startMarker, new NestedBlockStatement(NodeList.From(ref statements)), scope);
    }

    private ReadOnlyRef<Scope> ParseBlock(ref ArrayList<Statement> body, bool createNewLexicalScope = true, bool exitStrict = false)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseBlock = function`

        if (createNewLexicalScope)
        {
            EnterScope(ScopeFlags.None);
        }

        while (!Eat(TokenType.BraceRight))
        {
            var statement = ParseStatement(StatementContext.Default);
            body.Add(statement);
        }

        _strict = _strict && !exitStrict;

        return createNewLexicalScope ? ExitScope() : default;
    }

    // Parse a regular `for` loop. The disambiguation code in
    // `ParseForStatement` will already have parsed the init statement or
    // expression.
    private ForStatement ParseFor(in Marker startMarker, StatementOrExpression? init)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseFor = function`

        Expect(TokenType.Semicolon);

        var test = _tokenizer._type == TokenType.Semicolon ? null : ParseExpression(ref NullRef<DestructuringErrors>());
        Expect(TokenType.Semicolon);

        var update = _tokenizer._type == TokenType.ParenRight ? null : ParseExpression(ref NullRef<DestructuringErrors>());
        Expect(TokenType.ParenRight);

        var body = ParseStatement(StatementContext.For);

        var scope = ExitScope();
        _labels.PopRef();

        return FinishNode(startMarker, new ForStatement(init, test, update, body), scope);
    }

    // Parse a `for`/`in` and `for`/`of` loop, which are almost
    // same from parser's perspective.
    private Statement ParseForInOf(in Marker startMarker, bool isForIn, bool await, Node init)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseForIn = function`

        Next();

        VariableDeclarator? variableDeclarator;
        if (init is VariableDeclaration variableDeclaration
            && (variableDeclarator = variableDeclaration.Declarations[0]).Init is not null)
        {
            if (!isForIn
                || _tokenizerOptions._ecmaVersion < EcmaVersion.ES8
                || _strict
                || variableDeclaration.Kind != VariableDeclarationKind.Var
                || variableDeclarator.Id.Type != NodeType.Identifier)
            {
                // Raise(init.Start, $"{(isForIn ? "for-in" : "for-of")} loop variable declaration may not have an initializer"); // original acornjs error reporting
                Raise(variableDeclarator.Id.Start, ForInOfLoopInitializer, new object[] { isForIn ? "for-in" : "for-of" });
            }
        }

        var left = init;
        var right = isForIn
            ? ParseExpression(ref NullRef<DestructuringErrors>())
            : ParseMaybeAssign(ref NullRef<DestructuringErrors>());
        Expect(TokenType.ParenRight);

        var body = ParseStatement(StatementContext.For);

        var scope = ExitScope();
        _labels.PopRef();

        return FinishNode<Statement>(startMarker, isForIn
            ? new ForInStatement(left, right, body)
            : new ForOfStatement(left, right, body, await),
            scope);
    }

    // Parse a list of variable declarations.
    private VariableDeclaration ParseVar(VariableDeclarationKind kind, bool isFor)
    {
        // https://github.com/acornjs/acorn/blob/8.15.0/acorn/src/statement.js > `pp.parseVar = function`

        var declarations = new ArrayList<VariableDeclarator>();
        do
        {
            var declStartMarker = StartNode();

            var id = ParseVarId(kind);

            Expression? init;
            if (Eat(TokenType.Eq))
            {
                init = ParseMaybeAssign(ref NullRef<DestructuringErrors>(), isFor ? ExpressionContext.ForInit : ExpressionContext.Default);
            }
            else
            {
                if (kind == VariableDeclarationKind.Const && !(_tokenizer._type == TokenType.In || _tokenizerOptions._ecmaVersion >= EcmaVersion.ES6 && IsContextual("of")))
                {
                    // Unexpected(); // original acornjs error reporting
                    Raise(_tokenizer._lastTokenEnd, DeclarationMissingInitializer_Const);
                }
                else if (kind is VariableDeclarationKind.Using or VariableDeclarationKind.AwaitUsing && _tokenizerOptions.AllowExplicitResourceManagement() && _tokenizer._type != TokenType.In && !IsContextual("of"))
                {
                    // Raise(_tokenizer._lastTokenEnd, `Missing initializer in ${kind} declaration`); // original acornjs error reporting
                    // TODO
                    Raise(_tokenizer._lastTokenEnd, DeclarationMissingInitializer_Destructuring);
                }
                else if (id.Type != NodeType.Identifier && !(isFor && (_tokenizer._type == TokenType.In || IsContextual("of"))))
                {
                    // Raise(_tokenizer._lastTokenEnd, "Complex binding patterns require an initialization value"); // original acornjs error reporting
                    Raise(_tokenizer._lastTokenEnd, DeclarationMissingInitializer_Destructuring);
                }
                init = null;
            }

            declarations.Add(FinishNode(declStartMarker, new VariableDeclarator(id, init)));
        }
        while (Eat(TokenType.Comma));

        return new VariableDeclaration(kind, NodeList.From(ref declarations));
    }

    private Node ParseVarId(VariableDeclarationKind kind)
    {
        // https://github.com/acornjs/acorn/blob/8.15.0/acorn/src/statement.js > `pp.parseVarId = function`

        var id = kind is VariableDeclarationKind.Using or VariableDeclarationKind.AwaitUsing
            ? ParseIdentifier()
            : ParseBindingAtom();

        CheckLValPattern(id, kind == VariableDeclarationKind.Var ? BindingType.Var : BindingType.Lexical);
        return id;
    }

    // Parse a function declaration or expression (depending on the
    // `statement` parameter).
    private StatementOrExpression ParseFunction(in Marker startMarker, FunctionOrClassFlags flags, bool isAsync = false,
        ExpressionContext context = ExpressionContext.Default)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseFunction = function`

        bool generator;
        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES9 || !isAsync && _tokenizerOptions._ecmaVersion >= EcmaVersion.ES6)
        {
            if (_tokenizer._type == TokenType.Star && (flags & FunctionOrClassFlags.HangingStatement) != 0)
            {
                // Unexpected(); // original acornjs error reporting
                Raise(startMarker.Index, GeneratorInSingleStatementContext);
            }

            generator = Eat(TokenType.Star);
        }
        else
        {
            generator = false;
        }

        Debug.Assert(!isAsync || _tokenizerOptions._ecmaVersion >= EcmaVersion.ES8);

        Identifier? id;
        var isStatement = (flags & FunctionOrClassFlags.Statement) != 0;
        if (isStatement && ((flags & FunctionOrClassFlags.NullableId) == 0 || _tokenizer._type == TokenType.Name))
        {
            id = ParseIdentifier();
            if ((flags & FunctionOrClassFlags.HangingStatement) == 0)
            {
                // If it is a regular function declaration in sloppy mode, then it is
                // subject to Annex B semantics (`BindingType.Function`). Otherwise, the binding
                // mode depends on properties of the current scope (see `TreatFunctionsAsVar`).
                CheckLValSimple(id, _strict || generator || isAsync
                    ? (TreatFunctionsAsVar ? BindingType.Var : BindingType.Lexical)
                    : BindingType.Function);
            }
        }
        else
        {
            id = null;
        }

        var oldYieldPos = _yieldPosition;
        var oldAwaitPos = _awaitPosition;
        var oldAwaitIdentPos = _awaitIdentifierPosition;
        _yieldPosition = _awaitPosition = _awaitIdentifierPosition = 0;

        EnterScope(FunctionFlags(isAsync, generator));

        if (!isStatement && _tokenizer._type == TokenType.Name)
        {
            id = ParseIdentifier();
        }

        var parameters = ParseFunctionParams();
        var scope = ParseFunctionBody(id, parameters, isArrowFunction: false, isMethod: false, context, out _, out var body);

        _yieldPosition = oldYieldPos;
        _awaitPosition = oldAwaitPos;
        _awaitIdentifierPosition = oldAwaitIdentPos;

        return FinishNode<StatementOrExpression>(startMarker, isStatement
            ? new FunctionDeclaration(id, parameters, (FunctionBody)body, generator, isAsync)
            : new FunctionExpression(id, parameters, (FunctionBody)body, generator, isAsync),
            scope);
    }

    private NodeList<Node> ParseFunctionParams()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseFunctionParams = function`

        Expect(TokenType.ParenLeft);
        var parameters = ParseBindingList(close: TokenType.ParenRight, allowEmptyElement: false, allowTrailingComma: _tokenizerOptions._ecmaVersion >= EcmaVersion.ES8);
        CheckYieldAwaitInDefaultParams();
        return parameters!;
    }

    // Parse a class declaration or literal (depending on the
    // `FunctionOrClassFlags.Statement` flag).
    private StatementOrExpression ParseClass(in Marker startMarker, FunctionOrClassFlags flags)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseClass = function`

        // ecma-262 14.6 Class Definitions
        // A class definition is always strict mode code.
        var oldStrict = _strict;
        _strict = true;

        var id = ParseClassId(flags);

        // Original acornjs implementation doesn't create a scope for classes, however, as Acornima exposes scope information,
        // it's necessary to create one so consumers can store and look up the class name, which is visible in the scope of the class.
        EnterScope(ScopeFlags.None);

        var superClass = ParseClassSuper();

        var privateNameStatusIndex = EnterClassBody();
        var classBodyStartMarker = StartNode();

        var hasSuperClass = superClass is not null;
        var hadConstructor = false;
        var body = new ArrayList<Node>();

        Expect(TokenType.BraceLeft);

        while (_tokenizer._type != TokenType.BraceRight)
        {
            if (Eat(TokenType.Semicolon))
            {
                continue;
            }

            var element = ParseClassElement(hasSuperClass);

            body.Add(element);
            if (element is MethodDefinition { Kind: PropertyKind.Constructor })
            {
                if (hadConstructor)
                {
                    // RaiseRecoverable(element.Start, "Duplicate constructor in the same class"); // original acornjs error reporting
                    Raise(element.Start, DuplicateConstructor);
                }
                else
                {
                    hadConstructor = true;
                }
            }
            else if (element is ClassProperty { Key.Type: NodeType.PrivateIdentifier } prop
                && IsPrivateNameConflicted(ref _privateNameStack.GetItemRef(privateNameStatusIndex), prop))
            {
                // RaiseRecoverable(prop.Key.Start, $"Identifier '#{prop.Key.As<PrivateIdentifier>().Name}' has already been declared"); // original acornjs error reporting
                Raise(prop.Key.Start, VarRedeclaration, new object[] { '#'.ToStringCached() + prop.Key.As<PrivateIdentifier>().Name });
            }
        }

        _strict = oldStrict;
        Next();

        var classBody = FinishNode(classBodyStartMarker, new ClassBody(NodeList.From(ref body)));
        ExitClassBody();

        var scope = ExitScope();

        var isStatement = (flags & FunctionOrClassFlags.Statement) != 0;

        return FinishNode<StatementOrExpression>(startMarker, isStatement
            ? new ClassDeclaration(id, superClass, classBody, NodeList.From(ref _decorators))
            : new ClassExpression(id, superClass, classBody, NodeList.From(ref _decorators)),
            scope);
    }

    private Node ParseClassElement(bool constructorAllowsSuper)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseClassElement = function`

        var startMarker = StartNode();

        var hasDecorators = _tokenizer._type == TokenType.At;
        var decorators = hasDecorators ? ParseDecorators() : default;

        string? keyName = null;
        string keyword;

        var isStatic = EatContextual(keyword = "static");
        if (isStatic)
        {
            // Parse static init block
            if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES13 && Eat(TokenType.BraceLeft))
            {
                if (hasDecorators)
                {
                    Raise(startMarker.Index, DecoratorAppliedToStaticBlock);
                }

                return ParseClassStaticBlock(startMarker);
            }

            if (!(IsClassElementNameStart() || _tokenizer._type == TokenType.Star))
            {
                isStatic = false;
                keyName = keyword;
            }
        }

        var isAsync = keyName is null && _tokenizerOptions._ecmaVersion >= EcmaVersion.ES8 && EatContextual(keyword = "async");
        if (isAsync)
        {
            if (!((IsClassElementNameStart() || _tokenizer._type == TokenType.Star) && !CanInsertSemicolon()))
            {
                isAsync = false;
                keyName = keyword;
            }
        }

        var isGenerator = keyName is null
            // NOTE: We only need to check the ECMA version in the case of async generators
            // since non-async generators were introduced in ES6, together with classes.
            && (!isAsync || _tokenizerOptions._ecmaVersion >= EcmaVersion.ES9)
            && Eat(TokenType.Star);

        var isAccessor = false;
        var kind = PropertyKind.Unknown;

        if (keyName is null && !isAsync && !isGenerator)
        {
            if (EatContextual(keyword = "get"))
            {
                if (IsClassElementNameStart())
                {
                    kind = PropertyKind.Get;
                }
                else
                {
                    keyName = keyword;
                }
            }
            else if (EatContextual(keyword = "set"))
            {
                if (IsClassElementNameStart())
                {
                    kind = PropertyKind.Set;
                }
                else
                {
                    keyName = keyword;
                }
            }
            else if (_tokenizerOptions.AllowDecorators() && EatContextual(keyword = "accessor"))
            {
                if (!CanInsertSemicolon() && IsClassElementNameStart())
                {
                    isAccessor = true;
                }
                else
                {
                    keyName = keyword;
                }
            }
        }

        // Parse element name
        Expression key;
        bool computed;
        if (keyName is null)
        {
            key = ParseClassElementName(out computed);
        }
        else
        {
            // 'async', 'get', 'set', 'accessor' or 'static' were not a keyword contextually.
            // The last token is any of those. Make it the element name.
            computed = false;
            var keyStartMarker = new Marker(_tokenizer._lastTokenStart, _tokenizer._lastTokenStartLocation);
            key = FinishNode(keyStartMarker, new Identifier(keyName));
        }

        // Parse element value
        if (_tokenizerOptions._ecmaVersion < EcmaVersion.ES13 || _tokenizer._type == TokenType.ParenLeft || kind != PropertyKind.Unknown || isGenerator || isAsync)
        {
            if (kind == PropertyKind.Unknown)
            {
                kind = PropertyKind.Method;
            }
            return ParseClassMethod(startMarker, kind, key, computed, isStatic, isAsync, isGenerator, constructorAllowsSuper, ref decorators);
        }
        else
        {
            return ParseClassField(startMarker, key, computed, isStatic, isAccessor, ref decorators);
        }
    }

    private bool IsClassElementNameStart()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.isClassElementNameStart = function`

        return _tokenizer._type == TokenType.Name
            || _tokenizer._type == TokenType.PrivateId
            || _tokenizer._type == TokenType.BracketLeft
            || _tokenizer._type.Kind is TokenKind.StringLiteral or TokenKind.NumericLiteral or TokenKind.BigIntLiteral
            || _tokenizer._type.Keyword is not null;
    }

    private Expression ParseClassElementName(out bool computed)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseClassElementName = function`

        if (_tokenizer._type == TokenType.PrivateId)
        {
            if ("constructor".Equals(_tokenizer._value.Value))
            {
                // Raise(_tokenizer._start, "Classes can't have an element named '#constructor'"); // original acornjs error reporting
                Raise(_tokenizer._start, ConstructorIsPrivate);
            }
            computed = false;
            return ParsePrivateIdentifier();
        }
        else
        {
            return ParsePropertyName(out computed);
        }
    }

    private MethodDefinition ParseClassMethod(in Marker startMarker, PropertyKind kind, Expression key,
        bool computed, bool isStatic, bool isAsync, bool isGenerator, bool constructorAllowsSuper, ref ArrayList<Decorator> decorators)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseClassMethod = function`

        var isConstructor = !isStatic && CheckKeyName(key, computed, "constructor");
        var allowsDirectSuper = isConstructor && constructorAllowsSuper;

        // Check key and flags
        if (isConstructor)
        {
            if (kind != PropertyKind.Method)
            {
                // Raise(key.Start, "Constructor can't have get/set modifier"); // original acornjs error reporting
                Raise(key.Start, ConstructorIsAccessor);
            }
            if (isGenerator)
            {
                // Raise(key.Start, "Constructor can't be a generator"); // original acornjs error reporting
                Raise(key.Start, ConstructorIsGenerator);
            }
            if (isAsync)
            {
                // Raise(key.Start, "Constructor can't be an async method"); // original acornjs error reporting
                Raise(key.Start, ConstructorIsAsync);
            }

            kind = PropertyKind.Constructor;
        }
        else if (isStatic && CheckKeyName(key, computed, "prototype"))
        {
            // Raise(key.Start, "Classes may not have a static property named prototype"); // original acornjs error reporting
            Raise(key.Start, StaticPrototype);
        }

        // Parse value
        var value = ParseMethod(isGenerator, isAsync, allowsDirectSuper);

        // Check value
        if (kind == PropertyKind.Get)
        {
            if (value.Params.Count != 0)
            {
                // RaiseRecoverable(value.Start, "getter should have no params"); // original acornjs error reporting
                Raise(value.Start, BadGetterArity);
            }
        }
        else if (kind == PropertyKind.Set)
        {
            if (value.Params.Count != 1)
            {
                // RaiseRecoverable(value.Start, "setter should have exactly one param"); // original acornjs error reporting
                Raise(value.Start, BadSetterArity);
            }
            else if (value.Params[0] is RestElement)
            {
                // RaiseRecoverable(value.Params[0].Start, "Setter cannot use rest params"); // original acornjs error reporting
                Raise(value.Start, BadSetterRestParameter);
            }
        }

        return FinishNode(startMarker, new MethodDefinition(kind, key, value, computed, isStatic, NodeList.From(ref decorators)));
    }

    private ClassProperty ParseClassField(in Marker startMarker, Expression key, bool computed, bool isStatic, bool isAccessor, ref ArrayList<Decorator> decorators)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseClassField = function`

        if (CheckKeyName(key, computed, "constructor"))
        {
            // Raise(key.Start, "Classes can't have a field named 'constructor'"); // original acornjs error reporting
            Raise(key.Start, ConstructorClassField);
        }
        else if (isStatic && CheckKeyName(key, computed, "prototype"))
        {
            // Raise(key.Start, "Classes can't have a static field named 'prototype'"); // original acornjs error reporting
            Raise(key.Start, StaticPrototype);
        }

        Expression? value;
        if (Eat(TokenType.Eq))
        {
            // To raise SyntaxError if 'arguments' exists in the initializer.
            ref var currentScope = ref CurrentScope;
            var thisScopeIndex = currentScope._currentThisScopeIndex;
            var varScopeIndex = currentScope._currentVarScopeIndex;

            ref var thisScopeFlags = ref _scopeStack.GetItemRef(thisScopeIndex)._flags;
            ref var varScopeFlags = ref _scopeStack.GetItemRef(varScopeIndex)._flags;

            var oldThisScopeFlags = thisScopeFlags;
            var oldVarScopeFlags = varScopeFlags;
            thisScopeFlags |= ScopeFlags.InClassFieldInit;
            varScopeFlags |= ScopeFlags.InClassFieldInit;

            value = ParseMaybeAssign(ref NullRef<DestructuringErrors>());

            _scopeStack.GetItemRef(thisScopeIndex)._flags = oldThisScopeFlags;
            _scopeStack.GetItemRef(varScopeIndex)._flags = oldVarScopeFlags;
        }
        else
        {
            value = null;
        }
        Semicolon();

        return FinishNode<ClassProperty>(startMarker, isAccessor
            ? new AccessorProperty(key, value, computed, isStatic, NodeList.From(ref decorators))
            : new PropertyDefinition(key, value, computed, isStatic, NodeList.From(ref decorators)));
    }

    private StaticBlock ParseClassStaticBlock(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseClassStaticBlock = function`

        var body = new ArrayList<Statement>();

        var oldLabels = _labels;
        _labels = new ArrayList<Label>();
        EnterScope(ScopeFlags.ClassStaticBlock | ScopeFlags.Super);

        while (!Eat(TokenType.BraceRight))
        {
            var statement = ParseStatement(StatementContext.Default);
            body.Add(statement);
        }

        var scope = ExitScope();
        _labels = oldLabels;

        return FinishNode(startMarker, new StaticBlock(NodeList.From(ref body)), scope);
    }

    private Identifier? ParseClassId(FunctionOrClassFlags flags)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseClassId = function`

        Identifier? id;
        if (_tokenizer._type == TokenType.Name)
        {
            id = ParseIdentifier();
            if ((flags & FunctionOrClassFlags.Statement) != 0)
            {
                CheckLValSimple(id, BindingType.Lexical);
            }
        }
        else
        {
            if ((flags & (FunctionOrClassFlags.Statement | FunctionOrClassFlags.NullableId)) == FunctionOrClassFlags.Statement)
            {
                Unexpected();
            }
            id = null;
        }

        return id;
    }

    private Expression? ParseClassSuper()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseClassSuper = function`

        return Eat(TokenType.Extends) ? ParseExprSubscripts(ref NullRef<DestructuringErrors>()) : null;
    }

    private int EnterClassBody()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.enterClassBody = function`

        _privateNameStack.PushRef().Reset();
        return _privateNameStack.Count - 1;
    }

    private void ExitClassBody()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.exitClassBody = function`

        if (!_options._checkPrivateFields)
        {
            return;
        }

        ref readonly var current = ref _privateNameStack.PopRef();

        ref var parent = ref (_privateNameStack.Count == 0
            ? ref NullRef<PrivateNameStatus>()
            : ref _privateNameStack.PeekRef());

        for (var i = 0; i < current.Used.Count; i++)
        {
            var id = current.Used[i];
            if (current.Declared is null || !current.Declared.ContainsKey(id.Name))
            {
                if (!IsNullRef(ref parent))
                {
                    parent.Used.Add(id);
                }
                else
                {
                    // RaiseRecoverable(id.Start, $"Private field '#{id.Name}' must be declared in an enclosing class"); // original acornjs error reporting
                    Raise(id.Start, InvalidPrivateFieldResolution, new object[] { '#'.ToStringCached() + id.Name });
                }
            }
        }
    }

    private static bool IsPrivateNameConflicted(ref PrivateNameStatus privateNameStatus, ClassProperty element)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `function isPrivateNameConflicted`

        const int staticFlag = 1 << (int)PropertyKind.Unknown;
        const int getterFlag = 1 << (int)PropertyKind.Get;
        const int setterFlag = 1 << (int)PropertyKind.Set;
        const int reservedMask = getterFlag | setterFlag;

        var privateNameMap = privateNameStatus.Declared ??= new Dictionary<string, int>();

        var name = element.Key.As<PrivateIdentifier>().Name;
        if (!privateNameMap.TryGetValue(name, out var curr))
        {
            curr = 0;
        }
        else if ((curr & reservedMask) == reservedMask)
        {
            return true;
        }

        int next;
        if (element.Type == NodeType.MethodDefinition && element.Kind is PropertyKind.Get or PropertyKind.Set)
        {
            next = 1 << (int)element.Kind;

            if ((curr & next & reservedMask) != 0
                // `class { get #a(){}; static set #a(_){} }` is also conflict.
                || curr != 0 && ((curr & staticFlag) != 0) != element.Static)
            {
                return true;
            }

            next |= curr;
        }
        else
        {
            if (curr != 0)
            {
                return true;
            }

            next = reservedMask;
        }

        next |= element.Static ? staticFlag : 0;
        privateNameMap[name] = next;
        return false;
    }

    private static bool CheckKeyName(Expression key, bool computed, string name)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `function checkKeyName`

        return !computed
            && (key is Identifier identifier && identifier.Name == name
                || key is StringLiteral literal && name.Equals(literal.Value));
    }

    // Parses module export declaration.

    private ExportDeclaration ParseExport(in Marker startMarker, HashSet<string>? exports)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseExport = function`

        Next();

        if (Eat(TokenType.Star))
        {
            // export * from '...'
            return ParseExportAllDeclaration(startMarker, exports);
        }

        if (Eat(TokenType.Default))
        {
            // export default ...
            return ParseExportDefaultDeclaration(startMarker, exports);
        }

        return ParseExportDeclaration(startMarker, exports);
    }

    private ExportNamedDeclaration ParseExportDeclaration(in Marker startMarker, HashSet<string>? exports)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseExportDeclaration = function`

        Declaration? declaration;
        NodeList<ExportSpecifier> specifiers;
        StringLiteral? source;
        ArrayList<ImportAttribute> attributes;

        if (ShouldParseExportStatement())
        {
            // export var|const|let|function|class ...

            declaration = (Declaration)ParseStatement(StatementContext.Default);
            if (declaration is FunctionDeclaration functionDeclaration)
            {
                CheckExport(exports, functionDeclaration.Id!, functionDeclaration.Id!.Start);
            }
            else if (declaration is ClassDeclaration classDeclaration)
            {
                CheckExport(exports, classDeclaration.Id!, classDeclaration.Id!.Start);
            }
            else
            {
                CheckVariableExport(exports, declaration.As<VariableDeclaration>().Declarations);
            }

            specifiers = default;
            source = null;
            attributes = default;
        }
        else
        {
            // export { x, y as z } [from '...']

            specifiers = ParseExportSpecifiers(exports);
            if (EatContextual("from"))
            {
                if (_tokenizer._type != TokenType.String)
                {
                    Unexpected();
                }

                source = ParseExprAtom(ref NullRef<DestructuringErrors>()).As<StringLiteral>();
                attributes = _tokenizerOptions.AllowImportAttributes()
                    ? ParseImportAttributes()
                    : default;
            }
            else
            {
                for (var i = 0; i < specifiers.Count; i++)
                {
                    var spec = specifiers[i];
                    if (spec.Local.Type != NodeType.Literal)
                    {
                        var identifier = spec.Local.As<Identifier>();

                        // check for keywords used as local names
                        CheckUnreserved(identifier);

                        // check if export is defined
                        CheckLocalExport(identifier);
                    }
                    else
                    {
                        // Raise(spec.Local.Start, "A string literal cannot be used as an exported binding without `from`."); // original acornjs error reporting
                        Raise(spec.Local.Start, ModuleExportNameWithoutFromClause);
                    }
                }

                source = null;
                attributes = default;
            }

            Semicolon();

            declaration = null;
        }

        return FinishNode(startMarker, new ExportNamedDeclaration(declaration, specifiers, source, NodeList.From(ref attributes)));
    }

    private ExportDefaultDeclaration ParseExportDefaultDeclaration(in Marker startMarker, HashSet<string>? exports)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseExportDefaultDeclaration = function`

        CheckExport(exports, "default", _tokenizer._lastTokenStart);

        StatementOrExpression declaration;
        Marker declarationStartMarker;
        var isAsync = false;

        if (_tokenizer._type == TokenType.Function || (isAsync = IsAsyncFunction()))
        {
            declarationStartMarker = StartNode();
            Next();
            if (isAsync)
            {
                Next();
            }
            declaration = ParseFunction(declarationStartMarker, FunctionOrClassFlags.Statement | FunctionOrClassFlags.NullableId, isAsync);
        }
        else if (_tokenizer._type == TokenType.Class)
        {
            declarationStartMarker = StartNode();
            Next();
            declaration = ParseClass(declarationStartMarker, FunctionOrClassFlags.Statement | FunctionOrClassFlags.NullableId);
        }
        else if (_tokenizer._type == TokenType.At)
        {
            declarationStartMarker = StartNode();
            declaration = ParseDecoratedClassStatement(declarationStartMarker, FunctionOrClassFlags.Statement | FunctionOrClassFlags.NullableId);
        }
        else
        {
            declaration = ParseMaybeAssign(ref NullRef<DestructuringErrors>());
            Semicolon();
        }

        return FinishNode(startMarker, new ExportDefaultDeclaration(declaration));
    }

    private ExportAllDeclaration ParseExportAllDeclaration(in Marker startMarker, HashSet<string>? exports)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseExportAllDeclaration = function`

        Expression? exported;
        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES11 && EatContextual("as"))
        {
            exported = ParseModuleExportName();
            CheckExport(exports, exported, _tokenizer._lastTokenStart);
        }
        else
        {
            exported = null;
        }

        ExpectContextual("from");

        if (_tokenizer._type != TokenType.String)
        {
            Unexpected();
        }

        var source = ParseExprAtom(ref NullRef<DestructuringErrors>()).As<StringLiteral>();
        var attributes = _tokenizerOptions.AllowImportAttributes()
            ? ParseImportAttributes()
            : default;

        Semicolon();

        return FinishNode(startMarker, new ExportAllDeclaration(exported, source, NodeList.From(ref attributes)));
    }

    private void CheckExport(HashSet<string>? exports, object name, int pos)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.checkExport = function`

        if (exports is null)
        {
            return;
        }

        var exportName = name switch
        {
            Identifier identifier => identifier.Name,
            StringLiteral literal => literal.Value,
            _ => (string)name
        };

        if (!exports.Add(exportName))
        {
            // RaiseRecoverable(pos, $"Duplicate export '{exportName}'"); // original acornjs error reporting
            Raise(pos, DuplicateExport, new[] { exportName });
        }
    }

    private void CheckPatternExport(HashSet<string>? exports, Node pattern)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.checkPatternExport = function`

        int i;
        switch (pattern)
        {
            case Identifier identifier:
                CheckExport(exports, identifier, identifier.Start);
                break;

            case ObjectPattern objectPattern:
                for (i = 0; i < objectPattern.Properties.Count; i++)
                {
                    CheckPatternExport(exports, objectPattern.Properties[i]);
                }
                break;

            case ArrayPattern arrayPattern:
                for (i = 0; i < arrayPattern.Elements.Count; i++)
                {
                    if (arrayPattern.Elements[i] is { } element)
                    {
                        CheckPatternExport(exports, element);
                    }
                }
                break;

            case Property property:
                CheckPatternExport(exports, property.Value);
                break;

            case AssignmentPattern assignmentPattern:
                CheckPatternExport(exports, assignmentPattern.Left);
                break;

            case RestElement restElement:
                CheckPatternExport(exports, restElement.Argument);
                break;
        }
    }

    private void CheckVariableExport(HashSet<string>? exports, in NodeList<VariableDeclarator> decls)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.checkVariableExport = function`

        if (exports is null)
        {
            return;
        }

        for (var i = 0; i < decls.Count; i++)
        {
            CheckPatternExport(exports, decls[i].Id);
        }
    }

    private bool ShouldParseExportStatement()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.shouldParseExportStatement = function`

        return _tokenizer._type.Keyword is Keyword.Var or Keyword.Const or Keyword.Class or Keyword.Function
            || IsLet()
            || IsAsyncFunction();
    }

    // Parses a comma-separated list of module exports.

    private ExportSpecifier ParseExportSpecifier(HashSet<string>? exports)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseExportSpecifier = function`

        var startMarker = StartNode();
        var local = ParseModuleExportName();

        var exported = EatContextual("as") ? ParseModuleExportName() : local;
        CheckExport(exports, exported, exported.Start);

        return FinishNode(startMarker, new ExportSpecifier(local, exported));
    }

    private NodeList<ExportSpecifier> ParseExportSpecifiers(HashSet<string>? exports)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseExportSpecifiers = function`

        var nodes = new ArrayList<ExportSpecifier>();

        // export { x, y as z } [from '...']
        Expect(TokenType.BraceLeft);

        var first = true;
        while (!Eat(TokenType.BraceRight))
        {
            if (!first)
            {
                Expect(TokenType.Comma);
                if (AfterTrailingComma(TokenType.BraceRight))
                {
                    break;
                }
            }
            else
            {
                first = false;
            }

            nodes.Add(ParseExportSpecifier(exports));
        }

        return NodeList.From(ref nodes);
    }

    // Parses import declaration.
    private ImportDeclaration ParseImportDeclaration(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseImport = function`

        Next();

        NodeList<ImportDeclarationSpecifier> specifiers;

        if (_tokenizer._type == TokenType.String)
        {
            // import '...'
            specifiers = default;
        }
        else
        {
            specifiers = ParseImportSpecifiers();
            ExpectContextual("from");
            if (_tokenizer._type != TokenType.String)
            {
                Unexpected();
            }
        }

        var source = ParseExprAtom(ref NullRef<DestructuringErrors>()).As<StringLiteral>();
        var attributes = _tokenizerOptions.AllowImportAttributes()
            ? ParseImportAttributes()
            : default;

        Semicolon();

        return FinishNode(startMarker, new ImportDeclaration(specifiers, source, NodeList.From(ref attributes)));
    }

    private ImportSpecifier ParseImportSpecifier()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseImportSpecifier = function`

        var startMarker = StartNode();
        var imported = ParseModuleExportName();

        Identifier? local;
        if (EatContextual("as"))
        {
            local = ParseIdentifier();
            CheckLValSimple(local, BindingType.Lexical);
        }
        else
        {
            if (imported is Identifier identifier)
            {
                CheckUnreserved(identifier);
                local = identifier;
            }
            else
            {
                local = null;
            }
            CheckLValSimple(imported, BindingType.Lexical);
            Debug.Assert(local is not null);
        }

        return FinishNode(startMarker, new ImportSpecifier(imported, local!));
    }

    private ImportDefaultSpecifier ParseImportDefaultSpecifier()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseImportDefaultSpecifier = function`

        // import defaultObj, { x, y as z } from '...'

        var startMarker = StartNode();
        var local = ParseIdentifier();
        CheckLValSimple(local, BindingType.Lexical);
        return FinishNode(startMarker, new ImportDefaultSpecifier(local));
    }

    private ImportNamespaceSpecifier ParseImportNamespaceSpecifier()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseImportNamespaceSpecifier = function`

        var startMarker = StartNode();
        Next();
        ExpectContextual("as");
        var local = ParseIdentifier();
        CheckLValSimple(local, BindingType.Lexical);
        return FinishNode(startMarker, new ImportNamespaceSpecifier(local));
    }

    // Parses a comma-separated list of module imports.
    private NodeList<ImportDeclarationSpecifier> ParseImportSpecifiers()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseImportSpecifiers = function`

        var nodes = new ArrayList<ImportDeclarationSpecifier>();

        if (_tokenizer._type == TokenType.Name)
        {
            nodes.Add(ParseImportDefaultSpecifier());
            if (!Eat(TokenType.Comma))
            {
                return NodeList.From(ref nodes);
            }
        }

        if (_tokenizer._type == TokenType.Star)
        {
            nodes.Add(ParseImportNamespaceSpecifier());
            return NodeList.From(ref nodes);
        }

        Expect(TokenType.BraceLeft);

        var first = true;
        while (!Eat(TokenType.BraceRight))
        {
            if (!first)
            {
                Expect(TokenType.Comma);
                if (AfterTrailingComma(TokenType.BraceRight))
                {
                    break;
                }
            }
            else
            {
                first = false;
            }

            nodes.Add(ParseImportSpecifier());
        }

        return NodeList.From(ref nodes);
    }

    private Expression ParseModuleExportName()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseModuleExportName = function`

        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES13 && _tokenizer._type == TokenType.String)
        {
            var stringLiteral = ParseExprAtom(ref NullRef<DestructuringErrors>()).As<StringLiteral>();
            if (stringLiteral.Value.AsSpan().ContainsLoneSurrogate())
            {
                // Raise(stringLiteral.Start, "An export name cannot include a lone surrogate."); // original acornjs error reporting
                Raise(stringLiteral.Start, InvalidModuleExportName);
            }
            return stringLiteral;
        }

        return ParseIdentifier(liberal: true);
    }

    private ArrayList<Statement> ParseDirectivePrologue(bool allowStrictDirective)
    {
        if (_tokenizerOptions._ecmaVersion < EcmaVersion.ES5)
        {
            return new ArrayList<Statement>();
        }

        var firstRestrictedPos = -1;

        var body = new ArrayList<Statement>();
        var sawStrict = false;
        while (_tokenizer._type == TokenType.String)
        {
            if (firstRestrictedPos < 0 && _tokenizer._legacyOctalPosition >= 0)
            {
                firstRestrictedPos = _tokenizer._legacyOctalPosition;
            }

            var startMarker = StartNode();

            var expr = ParseExpression(ref NullRef<DestructuringErrors>());
            if (expr is StringLiteral literal)
            {
                var directive = Tokenizer.DeduplicateString(literal.Raw.SliceBetween(1, literal.Raw.Length - 1), ref _tokenizer._stringPool);
                if (!sawStrict && directive == "use strict")
                {
                    if (!allowStrictDirective)
                    {
                        RaiseRecoverable(startMarker.Index, IllegalLanguageModeDirective, new object[] { directive });
                    }

                    if (!_strict)
                    {
                        if (firstRestrictedPos >= 0)
                        {
                            if (_tokenizer._input[firstRestrictedPos + 1] is '8' or '9')
                            {
                                RaiseRecoverable(firstRestrictedPos, Strict8Or9Escape);
                            }
                            else
                            {
                                RaiseRecoverable(firstRestrictedPos, StrictOctalEscape);
                            }
                        }

                        _strict = true;
                    }

                    sawStrict = true;
                }

                Semicolon();
                body.Add(FinishNode(startMarker, new Directive(expr, directive)));
            }
            else
            {
                Semicolon();
                body.Add(FinishNode(startMarker, new NonSpecialExpressionStatement(expr)));
                break;
            }
        }

        return body;
    }

    private ClassDeclaration ParseDecoratedClassStatement(in Marker startMarker, FunctionOrClassFlags flags = FunctionOrClassFlags.Statement)
    {
        var oldDecorators = _decorators;
        _decorators = ParseDecorators();
        Expect(TokenType.Class);
        var declaration = (ClassDeclaration)ParseClass(startMarker, flags);
        _decorators = oldDecorators;

        return FinishNode(startMarker, declaration);
    }

    private ArrayList<ImportAttribute> ParseImportAttributes()
    {
        if (!Eat(TokenType.With))
        {
            return default;
        }

        Expect(TokenType.BraceLeft);

        var attributes = new ArrayList<ImportAttribute>();
        var first = true;
        var parameterSet = new HashSet<string>();

        while (!Eat(TokenType.BraceRight))
        {
            if (!first)
            {
                Expect(TokenType.Comma);
                if (AfterTrailingComma(TokenType.BraceRight))
                {
                    break;
                }
            }
            else
            {
                first = false;
            }

            var importAttribute = ParseImportAttribute();

            var key = importAttribute.Key is Identifier identifier
                ? identifier.Name
                : importAttribute.Key.As<StringLiteral>().Value;

            if (!parameterSet.Add(key))
            {
                Raise(importAttribute.Start, DuplicateImportAttribute, new object[] { key });
            }

            attributes.Add(importAttribute);
        }

        return attributes;
    }

    private ImportAttribute ParseImportAttribute()
    {
        var startMarker = StartNode();
        var errorState = new TokenState(_tokenizer);

        var key = ParsePropertyName(out var computed);
        if (computed || key is not (Identifier or StringLiteral))
        {
            Unexpected(errorState);
        }

        if (!Eat(TokenType.Colon) || _tokenizer._type != TokenType.String)
        {
            Unexpected();
        }

        var valueStartMarker = StartNode();
        var raw = Tokenizer.DeduplicateString(_tokenizer._input.SliceBetween(_tokenizer._start, _tokenizer._end), ref _tokenizer._stringPool, Tokenizer.NonIdentifierDeduplicationThreshold);
        var value = new StringLiteral((string)_tokenizer._value.Value!, raw);
        Next();
        value = FinishNode(valueStartMarker, value);

        return FinishNode(startMarker, new ImportAttribute(key, value));
    }
}
