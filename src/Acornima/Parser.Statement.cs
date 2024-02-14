using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static Unsafe;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js

public partial class Parser
{
    // Parse a program. Initializes the parser, reads any number of
    // statements, and wraps them in a Program node.  Optionally takes a
    // `program` argument.  If present, the statements will be appended
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
                RaiseRecoverable(kvp.Value, $"Export '{kvp.Key}' is not defined");
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
        var nextCh = _tokenizer._input.CharCodeAt(next);

        // For ambiguous cases, determine if a LexicalDeclaration (or only a
        // Statement) is allowed here. If context is not empty then only a Statement
        // is allowed. However, `let [` is an explicit negative lookahead for
        // ExpressionStatement, so special-case it first.

        if (nextCh is '[' or '\\') // TODO: Acorn comment says '/' - report bug
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
            while (Tokenizer.IsIdentifierChar(nextCh = _tokenizer._input.CharCodeAt(pos), allowAstral: true))
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

            if (next + keyword.Length <= _tokenizer._endPosition
                && _tokenizer._input.AsSpan(next, keyword.Length).SequenceEqual(keyword.AsSpan()))
            {
                return next + keyword.Length == _tokenizer._endPosition
                    || !(Tokenizer.IsIdentifierChar(after = _tokenizer._input.CharCodeAt(next + keyword.Length)) || ((char)after).IsHighSurrogate());
            }
        }

        return false;
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
                    return ParseBreakContinueStatement(startMarker, startType);

                case Keyword.Debugger:
                    return ParseDebuggerStatement(startMarker);

                case Keyword.Do:
                    return ParseDoStatement(startMarker);

                case Keyword.For:
                    return ParseForStatement(startMarker);

                case Keyword.Function:
                    // Function as sole body of either an if statement or a labeled statement
                    // works, but not when it is part of a labeled statement that is the sole
                    // body of an if statement.

                    hasContext = context != StatementContext.Default;
                    if (hasContext
                        && _tokenizerOptions._ecmaVersion >= EcmaVersion.ES6
                        && (_strict || context is not (StatementContext.If or StatementContext.Label)))
                    {
                        Unexpected();
                    }

                    return ParseFunctionStatement(startMarker, isAsync: false, declarationPosition: !hasContext);

                case Keyword.Class:
                    if (context != StatementContext.Default)
                    {
                        Unexpected();
                    }

                    return ParseClassStatement(startMarker);

                case Keyword.If:
                    return ParseIfStatement(startMarker);

                case Keyword.Return:
                    return ParseReturnStatement(startMarker);

                case Keyword.Switch:
                    return ParseSwitchStatement(startMarker);

                case Keyword.Throw:
                    return ParseThrowStatement(startMarker);

                case Keyword.Try:
                    return ParseTryStatement(startMarker);

                case Keyword.Const or Keyword.Var:
                    if (kind == VariableDeclarationKind.Unknown)
                    {
                        kind = startType.Keyword.Value == Keyword.Var ? VariableDeclarationKind.Var : VariableDeclarationKind.Const;
                    }

                    if (context != StatementContext.Default && kind != VariableDeclarationKind.Var)
                    {
                        Unexpected();
                    }

                    return ParseVarStatement(startMarker, kind);

                case Keyword.While:
                    return ParseWhileStatement(startMarker);

                case Keyword.With:
                    return ParseWithStatement(startMarker);

                case Keyword.Export or Keyword.Import:
                    if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES10 && startType.Keyword.Value == Keyword.Import)
                    {
                        var next = _tokenizer.NextTokenPosition();
                        var nextCh = _tokenizer._input.CharCodeAt(next);
                        if (nextCh is '(' or '.')
                        {
                            return ParseExpressionStatement(startMarker, ParseExpression(ref NullRef<DestructuringErrors>()));
                        }
                    }

                    if (!_options._allowImportExportEverywhere)
                    {
                        if (!topLevel)
                        {
                            Raise(_tokenizer._start, "'import' and 'export' may only appear at the top level");
                        }

                        if (!_inModule)
                        {
                            Raise(_tokenizer._start, "'import' and 'export' may appear only with 'sourceType: module'");
                        }
                    }

                    if (startType == TokenType.Import)
                    {
                        return ParseImport(startMarker);
                    }
                    else
                    {
                        return ParseExport(startMarker, topLevel ? _exports : null);
                    }
            }
        }
        else if (startType == TokenType.BraceLeft)
        {
            return ParseBlockStatement(startMarker);
        }
        else if (startType == TokenType.Semicolon)
        {
            return ParseEmptyStatement(startMarker);
        }
        else if (IsAsyncFunction())
        {
            hasContext = context != StatementContext.Default;
            if (hasContext)
            {
                Unexpected();
            }

            Next();
            return ParseFunctionStatement(startMarker, isAsync: true, declarationPosition: !hasContext);
        }

        var maybeName = _tokenizer._value.Value;
        var expr = ParseExpression(ref NullRef<DestructuringErrors>());
        if (startType == TokenType.Name && expr is Identifier identifier && Eat(TokenType.Colon))
        {
            return ParseLabeledStatement(startMarker, (string)maybeName!, identifier, context);
        }
        else
        {
            return ParseExpressionStatement(startMarker, expr);
        }
    }

    private Statement ParseBreakContinueStatement(in Marker startMarker, TokenType keyword)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseBreakContinueStatement = function`

        var isBreak = keyword == TokenType.Break;
        Next();

        Identifier? label;
        if (Eat(TokenType.Semicolon) || InsertSemicolon())
        {
            label = null;
        }
        else if (_tokenizer._type != TokenType.Name)
        {
            return Unexpected<Statement>();
        }
        else
        {
            label = ParseIdentifier();
            Semicolon();
        }

        // Verify that there is an actual destination to break or
        // continue to.
        var i = 0;
        for (; i < _labels.Count; i++)
        {
            ref var lab = ref _labels.GetItemRef(i);

            if (label is null)
            {
                if (isBreak ? lab.Kind != LabelKind.None : lab.Kind == LabelKind.Loop)
                {
                    break;
                }
            }
            else if (lab.Name == label.Name)
            {
                if (isBreak || lab.Kind == LabelKind.Loop)
                {
                    break;
                }
            }
        }

        if (i == _labels.Count)
        {
            Raise(startMarker.Index, "Unsyntactic " + keyword.Label);
        }

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

        _labels.Push(new Label(LabelKind.Loop));
        var body = ParseStatement(StatementContext.Do);
        _labels.Pop();

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

        var awaitAt = _tokenizerOptions._ecmaVersion >= EcmaVersion.ES9 && CanAwait() && EatContextual("await") ? _tokenizer._lastTokenStart : -1;

        _labels.Push(new Label(LabelKind.Loop));
        EnterScope(ScopeFlags.None);

        Expect(TokenType.ParenLeft);

        if (_tokenizer._type == TokenType.Semicolon)
        {
            if (awaitAt >= 0)
            {
                Unexpected(awaitAt);
            }

            return ParseFor(startMarker, null);
        }

        var isLet = false;
        StatementOrExpression? init;
        if (_tokenizer._type == TokenType.Var || _tokenizer._type == TokenType.Const || (isLet = IsLet()))
        {
            var initStartMarker = StartNode();
            var kind = isLet
                ? VariableDeclarationKind.Let
                : (_tokenizer._type.Keyword!.Value == Keyword.Var ? VariableDeclarationKind.Var : VariableDeclarationKind.Const);

            Next();

            var initDeclaration = FinishNode(initStartMarker, ParseVar(kind, isFor: true));

            if ((_tokenizer._type == TokenType.In || _tokenizerOptions._ecmaVersion >= EcmaVersion.ES6 && IsContextual("of"))
                && initDeclaration.Declarations.Count == 1)
            {
                if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES9 && _tokenizer._type == TokenType.In && awaitAt >= 0)
                {
                    Unexpected(awaitAt);
                }

                return ParseForIn(startMarker, await: awaitAt >= 0, initDeclaration);
            }

            init = initDeclaration;
        }
        else
        {
            var startsWithLet = IsContextual("let");
            var destructuringErrors = new DestructuringErrors();

            init = ParseExpression(ref destructuringErrors, awaitAt >= 0 ? ExpressionContext.AwaitForInit : ExpressionContext.ForInit);

            var isForOf = false;
            if (_tokenizer._type == TokenType.In || (isForOf = _tokenizerOptions._ecmaVersion >= EcmaVersion.ES6 && IsContextual("of")))
            {
                if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES9 && _tokenizer._type == TokenType.In && awaitAt >= 0)
                {
                    Unexpected(awaitAt);
                }

                if (startsWithLet && isForOf)
                {
                    Raise(init.Start, "The left-hand side of a for-of loop may not start with 'let'");
                }

                var initPattern = ToAssignable(init, isBinding: false, ref destructuringErrors);
                CheckLValPattern(initPattern);

                return ParseForIn(startMarker, await: awaitAt >= 0, initPattern);
            }

            CheckExpressionErrors(ref destructuringErrors, andThrow: true);
        }

        if (awaitAt >= 0)
        {
            Unexpected(awaitAt);
        }

        return ParseFor(startMarker, init);
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

        if (!_options._allowReturnOutsideFunction && !InFunction())
        {
            Raise(_tokenizer._start, "'return' outside of function");
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

        _labels.Push(new Label(LabelKind.Switch));
        EnterScope(ScopeFlags.None);

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
                    RaiseRecoverable(_tokenizer._start, "Multiple default clauses");
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
                consequent.Push(ParseStatement(StatementContext.Default));
            }

            var current = FinishNode(caseStartMarker, new SwitchCase(test, NodeList.From(ref consequent)));

            cases.Add(current);
        }

        ExitScope();
        _labels.Pop();

        return FinishNode(startMarker, new SwitchStatement(discriminant, NodeList.From(ref cases)));
    }

    private ThrowStatement ParseThrowStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseThrowStatement = function`

        Next();

        if (Tokenizer.ContainsLineBreak(_tokenizer._input.SliceBetween(_tokenizer._lastTokenEnd, _tokenizer._start)))
        {
            Raise(_tokenizer._lastTokenEnd, "Illegal newline after throw");
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
            ExitScope();

            handler = FinishNode(clauseStartMarker, new CatchClause(param, body));
        }

        BlockStatement? finalizer;
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
            Raise(startMarker.Index, "Missing catch or finally clause");
        }

        return FinishNode(startMarker, new TryStatement(block, handler, finalizer));
    }

    private VariableDeclaration ParseVarStatement(in Marker startMarker, VariableDeclarationKind kind, bool allowMissingInitializer = false)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseVarStatement = function`

        Next();
        var declaration = ParseVar(kind, isFor: false, allowMissingInitializer);
        Semicolon();

        return FinishNode(startMarker, declaration);
    }

    private WhileStatement ParseWhileStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseWhileStatement = function`

        Next();
        var test = ParseParenExpression();

        _labels.Push(new Label(LabelKind.Loop));
        var body = ParseStatement(StatementContext.While);
        _labels.Pop();

        return FinishNode(startMarker, new WhileStatement(test, body));
    }

    private WithStatement ParseWithStatement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseWithStatement = function`

        if (_strict)
        {
            Raise(_tokenizer._start, "'with' in strict mode");
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
                Raise(expr.Start, $"Label '{maybeName}' is already declared");
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

        _labels.Push(new Label(kind, maybeName, _tokenizer._start));
        var body = ParseStatement(context | StatementContext.Label);
        _labels.Pop();

        return FinishNode(startMarker, new LabeledStatement(expr, body));
    }

    private ExpressionStatement ParseExpressionStatement(in Marker startMarker, Expression expression)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseExpressionStatement = function`

        Semicolon();

        return FinishNode(startMarker, new NonSpecialExpressionStatement(expression));
    }

    // Parse a semicolon-enclosed block of statements, handling `"use
    // strict"` declarations when `allowStrict` is true (used for
    // function bodies).
    private BlockStatement ParseBlockStatement(in Marker startMarker, bool createNewLexicalScope = true)
    {
        // NOTE: This method doesn't exist in acornjs, was added for consistency.

        Expect(TokenType.BraceLeft);

        var statements = new ArrayList<Statement>();
        ParseBlock(ref statements, createNewLexicalScope);

        return FinishNode(startMarker, new NestedBlockStatement(NodeList.From(ref statements)));
    }

    private void ParseBlock(ref ArrayList<Statement> body, bool createNewLexicalScope = true, bool exitStrict = false)
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

        if (exitStrict)
        {
            _strict = false;
        }

        if (createNewLexicalScope)
        {
            ExitScope();
        }
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

        ExitScope();
        _labels.Pop();

        return FinishNode(startMarker, new ForStatement(init, test, update, body));
    }

    // Parse a `for`/`in` and `for`/`of` loop, which are almost
    // same from parser's perspective.
    private Statement ParseForIn(in Marker startMarker, bool await, Node init)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseForIn = function`

        var isForIn = _tokenizer._type == TokenType.In;
        Next();

        VariableDeclarator? variableDeclarator;
        if (init is VariableDeclaration variableDeclaration
            && (variableDeclarator = variableDeclaration.Declarations[0]).Init is { } variableInitializer)
        {
            if (!isForIn
                || _tokenizerOptions._ecmaVersion < EcmaVersion.ES8
                || _strict
                || variableDeclaration.Kind != VariableDeclarationKind.Var
                || variableDeclarator.Id.Type != NodeType.Identifier)
            {
                Raise(init.Start, $"{(isForIn ? "for-in" : "for-of")} loop variable declaration may not have an initializer");
            }
        }

        var left = init;
        var right = isForIn
            ? ParseExpression(ref NullRef<DestructuringErrors>())
            : ParseMaybeAssign(ref NullRef<DestructuringErrors>());
        Expect(TokenType.ParenRight);

        var body = ParseStatement(StatementContext.For);

        ExitScope();
        _labels.Pop();

        return FinishNode<Statement>(startMarker, isForIn
            ? new ForInStatement(left, right, body)
            : new ForOfStatement(left, right, body, await));
    }

    // Parse a list of variable declarations.
    private VariableDeclaration ParseVar(VariableDeclarationKind kind, bool isFor, bool allowMissingInitializer = false)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseVar = function`

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
            else if (!allowMissingInitializer)
            {
                if (kind == VariableDeclarationKind.Const && !(_tokenizer._type == TokenType.In || _tokenizerOptions._ecmaVersion >= EcmaVersion.ES6 && IsContextual("of")))
                {
                    return Unexpected<VariableDeclaration>();
                }
                else if (id.Type != NodeType.Identifier && !(isFor && (_tokenizer._type == TokenType.In || IsContextual("of"))))
                {
                    return Raise<VariableDeclaration>(_tokenizer._lastTokenEnd, "Complex binding patterns require an initialization value");
                }
                else
                {
                    init = null;
                }
            }
            else
            {
                init = null;
            }

            declarations.Add(FinishNode(declStartMarker, new VariableDeclarator(id, init)));
        }
        while (Eat(TokenType.Comma));

        return new VariableDeclaration(kind, NodeList.From(ref declarations));
    }

    private Node ParseVarId(VariableDeclarationKind kind)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseVarId = function`

        var id = ParseBindingAtom();
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
                Unexpected();
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
                    ? (TreatFunctionsAsVar() ? BindingType.Var : BindingType.Lexical)
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
        var body = (FunctionBody)ParseFunctionBody(id, parameters, isArrowFunction: false, isMethod: false, context, out _, out var strict);

        _yieldPosition = oldYieldPos;
        _awaitPosition = oldAwaitPos;
        _awaitIdentifierPosition = oldAwaitIdentPos;

        return FinishNode<StatementOrExpression>(startMarker, isStatement
            ? new FunctionDeclaration(id, parameters, body, generator, strict, isAsync)
            : new FunctionExpression(id, parameters, body, generator, strict, isAsync));
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
                    RaiseRecoverable(element.Start, "Duplicate constructor in the same class");
                }
                else
                {
                    hadConstructor = true;
                }
            }
            else if (element is ClassProperty { Key.Type: NodeType.PrivateIdentifier } prop
                && IsPrivateNameConflicted(ref _privateNameStack.GetItemRef(privateNameStatusIndex), prop))
            {
                RaiseRecoverable(prop.Key.Start, $"Identifier '#{prop.Key.As<PrivateIdentifier>().Name}' has already been declared");
            }
        }

        _strict = oldStrict;
        Next();

        var classBody = FinishNode(classBodyStartMarker, new ClassBody(NodeList.From(ref body)));
        ExitClassBody();

        var isStatement = (flags & FunctionOrClassFlags.Statement) != 0;
        // TODO: decorators
        return FinishNode<StatementOrExpression>(startMarker, isStatement
            ? new ClassDeclaration(id, superClass, classBody, new NodeList<Decorator>())
            : new ClassExpression(id, superClass, classBody, new NodeList<Decorator>()));
    }

    private Node ParseClassElement(bool constructorAllowsSuper)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseClassElement = function`

        var startMarker = StartNode();
        string? keyName = null;
        string keyword;

        var isStatic = EatContextual(keyword = "static");
        if (isStatic)
        {
            // Parse static init block
            if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES13 && Eat(TokenType.BraceLeft))
            {
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

        // Original acornjs implementation doesn't require ES6 for generators.
        // TODO: report bug
        var isGenerator = keyName is null
            && (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES9 || (!isAsync && _tokenizerOptions._ecmaVersion >= EcmaVersion.ES6))
            && Eat(TokenType.Star);

        var kind = keyName is null && !isAsync && !isGenerator && (EatContextual(keyword = "get") || EatContextual(keyword = "set"))
            ? keyword[0] == 'g' ? PropertyKind.Get : PropertyKind.Set
            : PropertyKind.Unknown;
        if (kind != PropertyKind.Unknown)
        {
            if (!IsClassElementNameStart())
            {
                kind = PropertyKind.Unknown;
                keyName = keyword;
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
            // 'async', 'get', 'set', or 'static' were not a keyword contextually.
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
            return ParseClassMethod(startMarker, kind, key, computed, isStatic, isAsync, isGenerator, constructorAllowsSuper);
        }
        else
        {
            return ParseClassField(startMarker, key, computed, isStatic);
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
                Raise(_tokenizer._start, "Classes can't have an element named '#constructor'");
            }
            computed = false;
            return ParsePrivateIdentifier();
        }
        else
        {
            return ParsePropertyName(out computed);
        }
    }

    private MethodDefinition ParseClassMethod(in Marker startMarker, PropertyKind kind, Expression key, bool computed, bool isStatic, bool isAsync, bool isGenerator, bool constructorAllowsSuper)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseClassMethod = function`

        var isConstructor = !isStatic && CheckKeyName(key, computed, "constructor");
        var allowsDirectSuper = isConstructor && constructorAllowsSuper;

        // Check key and flags
        if (isConstructor)
        {
            if (kind != PropertyKind.Method)
            {
                Raise(key.Start, "Constructor can't have get/set modifier");
            }
            if (isGenerator)
            {
                Raise(key.Start, "Constructor can't be a generator");
            }
            if (isAsync)
            {
                Raise(key.Start, "Constructor can't be an async method");
            }

            kind = PropertyKind.Constructor;
        }
        else if (isStatic && CheckKeyName(key, computed, "prototype"))
        {
            Raise(key.Start, "Classes may not have a static property named prototype");
        }

        // Parse value
        var value = ParseMethod(isGenerator, isAsync, allowsDirectSuper);

        // Check value
        if (kind == PropertyKind.Get)
        {
            if (value.Params.Count != 0)
            {
                RaiseRecoverable(value.Start, "getter should have no params");
            }
        }
        else if (kind == PropertyKind.Set)
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

        // TODO: decorators
        return FinishNode(startMarker, new MethodDefinition(kind, key, value, computed, isStatic, new NodeList<Decorator>()));
    }

    private PropertyDefinition ParseClassField(in Marker startMarker, Expression key, bool computed, bool isStatic)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseClassField = function`

        if (CheckKeyName(key, computed, "constructor"))
        {
            Raise(key.Start, "Classes can't have a field named 'constructor'");
        }
        else if (isStatic && CheckKeyName(key, computed, "prototype"))
        {
            Raise(key.Start, "Classes can't have a static field named 'prototype'");
        }

        Expression? value;
        if (Eat(TokenType.Eq))
        {
            // To raise SyntaxError if 'arguments' exists in the initializer.
            ref var scope = ref CurrentThisScope(out var thisScopeIndex);
            var oldScopeFlags = scope.Flags;
            scope.Flags |= ScopeFlags.InClassFieldInit;

            value = ParseMaybeAssign(ref NullRef<DestructuringErrors>());

            scope = _scopeStack.GetItemRef(thisScopeIndex);
            scope.Flags = oldScopeFlags;
        }
        else
        {
            value = null;
        }
        Semicolon();

        // TODO: decorators
        return FinishNode(startMarker, new PropertyDefinition(PropertyKind.Property, key, value, computed, isStatic, new NodeList<Decorator>()));
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

        ExitScope();
        _labels = oldLabels;

        return FinishNode(startMarker, new StaticBlock(NodeList.From(ref body)));
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

        var element = new PrivateNameStatus();
        _privateNameStack.Push(element);
        return _privateNameStack.Count - 1;
    }

    private void ExitClassBody()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.exitClassBody = function`

        if (!_options._checkPrivateFields)
        {
            return;
        }

        var current = _privateNameStack.Pop();

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
                    RaiseRecoverable(id.Start, $"Private field '#{id.Name}' must be declared in an enclosing class");
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
                || key is Literal literal && name.Equals(literal.Value));
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

    private ExportDeclaration ParseExportDeclaration(in Marker startMarker, HashSet<string>? exports)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/statement.js > `pp.parseExportDeclaration = function`

        Declaration? declaration;
        NodeList<ExportSpecifier> specifiers;
        Literal? source;

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

                source = (Literal)ParseExprAtom(ref NullRef<DestructuringErrors>());
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
                        Raise(spec.Local.Start, "A string literal cannot be used as an exported binding without `from`.");
                    }
                }

                source = null;
            }

            Semicolon();

            declaration = null;
        }

        // TODO: import attributes
        return FinishNode(startMarker, new ExportNamedDeclaration(declaration, specifiers, source, new NodeList<ImportAttribute>()));
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

        var source = (Literal)ParseExprAtom(ref NullRef<DestructuringErrors>());
        Semicolon();

        // TODO: import attributes
        return FinishNode(startMarker, new ExportAllDeclaration(source, exported, new NodeList<ImportAttribute>()));
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
            Literal literal => (string)literal.Value!,
            _ => (string)name
        };

        if (exports.Contains(exportName))
        {
            RaiseRecoverable(pos, $"Duplicate export '{exportName}'");
        }
        else
        {
            exports.Add(exportName);
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
    private ImportDeclaration ParseImport(in Marker startMarker)
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
        var source = (Literal)ParseExprAtom(ref NullRef<DestructuringErrors>());

        Semicolon();

        // TODO: import attributes
        return FinishNode(startMarker, new ImportDeclaration(specifiers, source, new NodeList<ImportAttribute>()));
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

        return FinishNode(startMarker, new ImportSpecifier(local!, imported));
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
            var stringLiteral = (Literal)ParseExprAtom(ref NullRef<DestructuringErrors>());
            if (((string)stringLiteral.Value!).AsSpan().ContainsLoneSurrogate())
            {
                Raise(stringLiteral.Start, "An export name cannot include a lone surrogate.");
            }
            return stringLiteral;
        }

        return ParseIdentifier(liberal: true);
    }

    private ArrayList<Statement> ParseDirectivePrologue(bool allowStrictDirective)
    {
        // NOTE: Original acornjs implementation of strict mode detection is fragile and buggy at the moment
        // (e.g.: `() => { 'a'[0]; 'use strict'; 00 }` is rejected while valid).
        // TODO: report bug

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
                        RaiseRecoverable(startMarker.Index, "Illegal 'use strict' directive in function with non-simple parameter list");
                    }

                    if (!_strict)
                    {
                        // NOTE: V8 reports this error after checking the rest of statements (e.g.
                        // `((a) => { "\00"; "use strict"; return 00 })()` reports for the octal number).
                        // TODO: fix error reporting order?
                        if (firstRestrictedPos >= 0)
                        {
                            // TODO: error message may differ for \08 and \8
                            RaiseRecoverable(firstRestrictedPos, "Octal literal in strict mode");
                        }

                        _strict = true;
                    }

                    sawStrict = true;
                }

                Semicolon();
                body.Push(FinishNode(startMarker, new Directive(expr, directive)));
            }
            else
            {
                Semicolon();
                body.Push(FinishNode(startMarker, new NonSpecialExpressionStatement(expr)));
                break;
            }
        }

        return body;
    }
}
