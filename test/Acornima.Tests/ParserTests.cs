using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using Acornima.Ast;
using Acornima.Helpers;
using Xunit;

namespace Acornima.Tests;

public partial class ParserTests
{
    [Theory]
    [InlineData("script", null, 0, 0, typeof(ArgumentNullException))]
    [InlineData("script", "", 0, 0, null)]
    [InlineData("script", "", 0, 1, typeof(ArgumentOutOfRangeException))]
    [InlineData("script", "", -1, 0, typeof(ArgumentOutOfRangeException))]
    [InlineData("script", "", -1, 1, typeof(ArgumentOutOfRangeException))]
    [InlineData("script", " ", 0, 0, null)]
    [InlineData("script", " ", 0, 1, null)]
    [InlineData("script", " ", 1, 0, null)]
    [InlineData("script", " ", 1, 1, typeof(ArgumentOutOfRangeException))]
    [InlineData("script", " ", -1, 0, typeof(ArgumentOutOfRangeException))]
    [InlineData("script", " ", -1, 1, typeof(ArgumentOutOfRangeException))]
    [InlineData("module", null, 0, 0, typeof(ArgumentNullException))]
    [InlineData("module", "", 0, 0, null)]
    [InlineData("module", "", 0, 1, typeof(ArgumentOutOfRangeException))]
    [InlineData("module", "", -1, 0, typeof(ArgumentOutOfRangeException))]
    [InlineData("module", "", -1, 1, typeof(ArgumentOutOfRangeException))]
    [InlineData("module", " ", 0, 0, null)]
    [InlineData("module", " ", 0, 1, null)]
    [InlineData("module", " ", 1, 0, null)]
    [InlineData("module", " ", 1, 1, typeof(ArgumentOutOfRangeException))]
    [InlineData("module", " ", -1, 0, typeof(ArgumentOutOfRangeException))]
    [InlineData("module", " ", -1, 1, typeof(ArgumentOutOfRangeException))]
    [InlineData("expression", null, 0, 0, typeof(ArgumentNullException))]
    [InlineData("expression", "", 0, 0, typeof(SyntaxErrorException))]
    [InlineData("expression", "", 0, 1, typeof(ArgumentOutOfRangeException))]
    [InlineData("expression", "", -1, 0, typeof(ArgumentOutOfRangeException))]
    [InlineData("expression", "", -1, 1, typeof(ArgumentOutOfRangeException))]
    [InlineData("expression", " ", 0, 0, typeof(SyntaxErrorException))]
    [InlineData("expression", " ", 0, 1, typeof(SyntaxErrorException))]
    [InlineData("expression", " ", 1, 0, typeof(SyntaxErrorException))]
    [InlineData("expression", " ", 1, 1, typeof(ArgumentOutOfRangeException))]
    [InlineData("expression", " ", -1, 0, typeof(ArgumentOutOfRangeException))]
    [InlineData("expression", " ", -1, 1, typeof(ArgumentOutOfRangeException))]
    [InlineData("expression", " x", 0, 1, typeof(SyntaxErrorException))]
    [InlineData("expression", " x ", 2, 1, typeof(SyntaxErrorException))]
    [InlineData("expression", " x ", 1, 1, null)]
    [InlineData("expression", " x ", 0, 3, null)]
    public void ShouldValidateParseArgs(string sourceType, string? input, int start, int length, Type? expectedExceptionType)
    {
        var parser = new Parser();
        var parseAction = GetSliceParseActionFor(sourceType);

        if (expectedExceptionType is null)
        {
            var root = parseAction(parser, input!, start, length);
            if (sourceType != "expression")
            {
                Assert.IsAssignableFrom<Program>(root);
                Assert.Empty(root.As<Program>().Body);
            }
            else
            {
                Assert.IsAssignableFrom<Expression>(root);
            }
        }
        else
        {
            Assert.Throws(expectedExceptionType, () => parseAction(parser, input!, start, length));
        }
    }

#if NET10_0_OR_GREATER
    /// <summary>
    /// Ensures that we don't regress in stack handling, only test in modern runtime for now
    /// </summary>
    [Fact]
    public void CanHandleDeepRecursion()
    {
        if (OperatingSystem.IsMacOS())
        {
            // stack limit differs quite a lot
            return;
        }

        var parser = new Parser();
#if DEBUG
        const int depth = 360;
#else
        const int depth = 845;
#endif
        var input = $"if ({new string('(', depth)}true{new string(')', depth)}) {{ }}";
        parser.ParseScript(input);
    }
#endif

    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion_MaybeAssign()
    {
        var parser = new Parser();
        const int depth = 100_000;
        var input = $"if ({new string('(', depth)}true{new string(')', depth)}) {{ }}";
        Assert.Throws<InsufficientExecutionStackException>(() => parser.ParseScript(input));
    }

    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion_MaybeAssign_Yield()
    {
        var parser = new Parser();
        const int depth = 100_000;
        var input = "function* f() { " + string.Join(" ", Enumerable.Range(0, depth).Select(_ => "yield")) + " 0 }";
        Assert.Throws<InsufficientExecutionStackException>(() => parser.ParseScript(input));
    }

    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion_MaybeUnary_Prefix()
    {
        var parser = new Parser();
        const int depth = 100_000;
        var input = string.Join("", Enumerable.Range(0, depth).Select(_ => "+-")) + "x";
        Assert.Throws<InsufficientExecutionStackException>(() => parser.ParseScript(input));
    }

    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion_MaybeUnary_Exponentiation()
    {
        var parser = new Parser();
        const int depth = 100_000;
        var input = string.Join("**", Enumerable.Range(0, depth).Select(n => n.ToString(CultureInfo.InvariantCulture)));
        Assert.Throws<InsufficientExecutionStackException>(() => parser.ParseScript(input));
    }

    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion_MaybeUnary_Await()
    {
        var parser = new Parser();
        const int depth = 100_000;
        var input = string.Join(" ", Enumerable.Range(0, depth).Select(_ => "await")) + " m()";
        Assert.Throws<InsufficientExecutionStackException>(() => parser.ParseModule(input));
    }

    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion_ExprAtom()
    {
        var parser = new Parser();
        const int depth = 100_000;
        var input = string.Join(" ", Enumerable.Range(0, depth).Select(_ => "new")) + "X";
        Assert.Throws<InsufficientExecutionStackException>(() => parser.ParseScript(input));
    }

    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion_Binding()
    {
        var parser = new Parser();
        const int depth = 100_000;
        var input = "try{}catch(" + string.Join("", Enumerable.Range(0, depth).Select(_ => "[...")) + "x" + new string(']', depth) + "){}";
        Assert.Throws<InsufficientExecutionStackException>(() => parser.ParseScript(input));
    }

    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion_Binding_Reinterpreted()
    {
        var parser = new Parser();
        const int depth = 100_000;
        var input = string.Join("", Enumerable.Range(0, depth).Select(_ => "[...")) + "x" + new string(']', depth) + "=[]";
        Assert.Throws<InsufficientExecutionStackException>(() => parser.ParseScript(input));
    }

    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion_Statement()
    {
        var parser = new Parser();
        const int depth = 100_000;
        var input = string.Join("", Enumerable.Range(0, depth).Select(_ => "function f(){")) + "x" + new string('}', depth);
        Assert.Throws<InsufficientExecutionStackException>(() => parser.ParseScript(input));
    }

    [Fact]
    public void CanReuseParser()
    {
        var comments = new List<Comment>();
        var tokens = new List<Token>();

        var parser = new Parser(new ParserOptions
        {
            OnComment = (in comment) => comments.Add(comment),
            OnToken = (in token) => tokens.Add(token)
        });

        var code = "var /* c1 */ foo=/a|b/; // c2";
        var script = parser.ParseScript(code);

        Assert.Equal(new string[] { "var", "foo", "=", "/a|b/", ";", "" }, tokens.Select(t => t.GetRawValue(code).ToString()).ToArray());
        Assert.Equal(0, tokens[0].Range.Start);

        Assert.Equal(new string[] { "/* c1 */", "// c2" }, comments.Select(c => c.GetRawValue(code).ToString()).ToArray());
        Assert.Equal(4, comments[0].Range.Start);

        comments.Clear();
        tokens.Clear();

        code = "/*c1*/ foo=1; //c2 ";
        script = parser.ParseScript(code);

        Assert.Equal(new string[] { "foo", "=", "1", ";", "" }, tokens.Select(t => t.GetRawValue(code).ToString()).ToArray());
        Assert.Equal(7, tokens[0].Range.Start);

        Assert.Equal(new string[] { "/*c1*/", "//c2 " }, comments.Select(c => c.GetRawValue(code).ToString()).ToArray());
        Assert.Equal(0, comments[0].Range.Start);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RecordsParentNodeInUserDataCorrectly(bool registerUserHandler)
    {
        var userHandlerCalled = false;

        var options = (registerUserHandler ? new ParserOptions { OnNode = delegate { userHandlerCalled = true; } } : new ParserOptions())
            .RecordParentNodeInUserData();

        var parser = new Parser(options);
        var script = parser.ParseScript("function toObj(a, b) { return { a, b() { return b } }; }");

        Func<Node, Node?> parentGetter = node => (Node?)node.UserData;

        new ParentNodeChecker(parentGetter).Check(script);

        Assert.Equal(registerUserHandler, userHandlerCalled);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ShouldPreserveUserOnNodeHandler(bool registerUserHandler)
    {
        const string code = "function toObj(a, b) { return { a, b: x => { let y = 2; return x * y } }; }";

        var userHandlerCalled = false;
        OnNodeHandler? userHandler = registerUserHandler ? delegate { userHandlerCalled = true; } : null;

        var options = new ParserOptions { OnNode = userHandler };
        Assert.Same(userHandler, options.OnNode);

        options = options.RecordParentNodeInUserData();
        Assert.Same(userHandler, options.OnNode);

        options = options.RecordParentNodeInUserData(enable: false);
        Assert.Same(userHandler, options.OnNode);

        var parser = new Parser(options);
        var script = parser.ParseScript(code);

        Assert.DoesNotContain(script.DescendantNodesAndSelf(), node => node.UserData is not null);
        Assert.Equal(registerUserHandler, userHandlerCalled);
    }

    [Theory]
    [InlineData("", 0, 1, 0)]
    [InlineData("  ", 2, 1, 2)]
    [InlineData(" ", 1, 1, 1)]
    [InlineData(" \r\n ", 4, 2, 1)]
    public void ShouldParseWhitespace(string code, int expectedEofIndex, int expectedEofLineNumber, int expectedEofColumn)
    {
        var tokens = new List<Token>();
        var parser = new Parser(new ParserOptions { OnToken = (in token) => tokens.Add(token) });

        var script = parser.ParseScript(code);

        var token = Assert.Single(tokens);

        Assert.Equal(TokenKind.EOF, token.Kind);
        Assert.Equal("", token.Value);
        Assert.Equal("", token.GetRawValue(code).ToString());
        Assert.Equal(Range.From(expectedEofIndex, expectedEofIndex), token.Range);
        var expectedEofPosition = Position.From(expectedEofLineNumber, expectedEofColumn);
        Assert.Equal(SourceLocation.From(expectedEofPosition, expectedEofPosition), token.Location);
    }

    [Fact]
    public void ShouldParseTokens()
    {
        var tokens = new List<Token>();
        var parser = new Parser(new ParserOptions
        {
            OnToken = (in token) => tokens.Add(token),
#pragma warning disable CS0618 // Type or member is obsolete
            RegExpParseMode = RegExpParseMode.AdaptToInterpreted,
            RegexTimeout = TimeSpan.FromSeconds(1)
#pragma warning restore CS0618 // Type or member is obsolete
        });

        var code =
            """
            var /* a */ $x = // b
            [null,true
             , '\u0066alse',	.1,2n,/a/u, `t
             \r\n`
             ]
            
            """.Replace("\r\n", "\n");

        var script = parser.ParseScript(code);

        Assert.Equal(21, tokens.Count);

        var token = tokens[0];
        Assert.Equal(TokenKind.Keyword, token.Kind);
        Assert.Equal("var", token.Value);
        Assert.Equal("var", token.GetRawValue(code).ToString());
        Assert.Equal(Range.From(0, 3), token.Range);
        Assert.Equal(SourceLocation.From(Position.From(1, 0), Position.From(1, 3)), token.Location);

        token = tokens[1];
        Assert.Equal(TokenKind.Identifier, token.Kind);
        Assert.Equal("$x", token.Value);
        Assert.Equal("$x", token.GetRawValue(code).ToString());
        Assert.Equal(Range.From(12, 14), token.Range);
        Assert.Equal(SourceLocation.From(Position.From(1, 12), Position.From(1, 14)), token.Location);

        token = tokens[2];
        Assert.Equal(TokenKind.Punctuator, token.Kind);
        Assert.Equal("=", token.Value);
        Assert.Equal("=", token.GetRawValue(code).ToString());
        Assert.Equal(Range.From(15, 16), token.Range);
        Assert.Equal(SourceLocation.From(Position.From(1, 15), Position.From(1, 16)), token.Location);

        token = tokens[4];
        Assert.Equal(TokenKind.NullLiteral, token.Kind);
        Assert.Null(token.Value);
        Assert.Equal("null", token.GetRawValue(code).ToString());
        Assert.Equal(Range.From(23, 27), token.Range);
        Assert.Equal(SourceLocation.From(Position.From(2, 1), Position.From(2, 5)), token.Location);

        token = tokens[6];
        Assert.Equal(TokenKind.BooleanLiteral, token.Kind);
        Assert.Same(CachedValues.True, token.Value);
        Assert.Equal("true", token.GetRawValue(code).ToString());
        Assert.Equal(Range.From(28, 32), token.Range);
        Assert.Equal(SourceLocation.From(Position.From(2, 6), Position.From(2, 10)), token.Location);

        token = tokens[8];
        Assert.Equal(TokenKind.StringLiteral, token.Kind);
        Assert.Equal("false", token.Value);
        Assert.Equal(@"'\u0066alse'", token.GetRawValue(code).ToString());
        Assert.Equal(Range.From(36, 48), token.Range);
        Assert.Equal(SourceLocation.From(Position.From(3, 3), Position.From(3, 15)), token.Location);

        token = tokens[10];
        Assert.Equal(TokenKind.NumericLiteral, token.Kind);
        Assert.Equal(0.1, token.Value);
        Assert.Equal(".1", token.GetRawValue(code).ToString());
        Assert.Equal(Range.From(50, 52), token.Range);
        Assert.Equal(SourceLocation.From(Position.From(3, 17), Position.From(3, 19)), token.Location);

        token = tokens[12];
        Assert.Equal(TokenKind.BigIntLiteral, token.Kind);
        Assert.Equal(new BigInteger(2), token.Value);
        Assert.Equal("2n", token.GetRawValue(code).ToString());
        Assert.Equal(Range.From(53, 55), token.Range);
        Assert.Equal(SourceLocation.From(Position.From(3, 20), Position.From(3, 22)), token.Location);

        token = tokens[14];
        Assert.Equal(TokenKind.RegExpLiteral, token.Kind);
        var regExpValue = Assert.IsType<RegExpValue>(token.Value);
        Assert.Equal("a", regExpValue.Pattern);
        Assert.Equal("u", regExpValue.Flags);
        Assert.True(token.RegExpParseResult?.Success);
        Assert.NotNull(token.RegExpParseResult?.Regex);
        Assert.Equal(Range.From(56, 60), token.Range);
        Assert.Equal(SourceLocation.From(Position.From(3, 23), Position.From(3, 27)), token.Location);

        token = tokens[16];
        Assert.Equal("`", token.Value);
        token = tokens[17];
        Assert.Equal(TokenKind.Template, token.Kind);
        var templateValue = Assert.IsType<TemplateValue>(token.Value);
        Assert.Equal("t\n \r\n", templateValue.Cooked);
        Assert.Equal($"t\n \\r\\n", templateValue.Raw);
        Assert.Equal(Range.From(63, 70), token.Range);
        Assert.Equal(SourceLocation.From(Position.From(3, 30), Position.From(4, 5)), token.Location);
        token = tokens[18];
        Assert.Equal("`", token.Value);

        token = tokens[20];
        Assert.Equal(TokenKind.EOF, token.Kind);
        Assert.Equal("", token.Value);
        Assert.Equal("", token.GetRawValue(code).ToString());
        Assert.Equal(Range.From(75, 75), token.Range);
        Assert.Equal(SourceLocation.From(Position.From(6, 0), Position.From(6, 0)), token.Location);
    }

    [Theory]
    [InlineData("#!/usr/bin/env node", CommentKind.HashBang, "/usr/bin/env node")]
    [InlineData("//this is a comment", CommentKind.Line, "this is a comment")]
    [InlineData("<!--this is a comment", CommentKind.Line, "this is a comment")]
    [InlineData("-->this is a comment", CommentKind.Line, "this is a comment")]
    [InlineData("/*this is a comment*/", CommentKind.Block, "this is a comment")]
    public void ShouldParseLoneComments(string code, CommentKind expectedCommentKind, string expectedContent)
    {
        var comments = new List<Comment>();
        var parser = new Parser(new ParserOptions { OnComment = (in comment) => comments.Add(comment) });
        var program = parser.ParseScript(code);

        Assert.NotNull(program);
        var comment = Assert.Single(comments);
        Assert.Equal(expectedCommentKind, comment.Kind);
        Assert.Equal(expectedContent, comment.GetContent(code).ToString());
        Assert.Equal(code, comment.GetRawValue(code).ToString());
    }

    [Fact]
    public void ShouldParseLineComment()
    {
        var comments = new List<Comment>();
        var parser = new Parser(new ParserOptions { OnComment = (in comment) => comments.Add(comment) });

        var code =
            """

            var x = 1; // this is a line comment 
            x += 2;
            """.Replace("\r\n", "\n");

        const string sourceFile = "line-comment.js";
        var script = parser.ParseScript(code, sourceFile);

        var comment = Assert.Single(comments);

        Assert.Equal(CommentKind.Line, comment.Kind);
        Assert.Equal(" this is a line comment ", comment.GetContent(code).ToString());
        Assert.Equal("// this is a line comment ", comment.GetRawValue(code).ToString());
        Assert.Equal(Range.From(12, 38), comment.Range);
        Assert.Equal(SourceLocation.From(Position.From(2, 11), Position.From(2, 37), sourceFile), comment.Location);
    }

    [Fact]
    public void ShouldParseBlockComment()
    {
        var comments = new List<Comment>();
        var parser = new Parser(new ParserOptions { OnComment = (in comment) => comments.Add(comment) });

        var code =
            """

            var x = 1; /* this is a
            block comment */
            x += 2;
            """.Replace("\r\n", "\n");

        const string sourceFile = "line-comment.js";
        var script = parser.ParseScript(code, sourceFile);

        var comment = Assert.Single(comments);

        Assert.Equal(CommentKind.Block, comment.Kind);
        Assert.Equal(
            """
             this is a
            block comment 
            """.Replace("\r\n", "\n"), comment.GetContent(code).ToString());
        Assert.Equal(
            """
            /* this is a
            block comment */
            """.Replace("\r\n", "\n"), comment.GetRawValue(code).ToString());
        Assert.Equal(Range.From(12, 41), comment.Range);
        Assert.Equal(SourceLocation.From(Position.From(2, 11), Position.From(3, 16), sourceFile), comment.Location);
    }

    [Theory]
    [InlineData("script", false)]
    [InlineData("module", true)]
    public void ShouldParseHtmlLikeLineComment(string sourceType, bool expectSyntaxError)
    {
        var comments = new List<Comment>();
        var parser = new Parser(new ParserOptions { OnComment = (in comment) => comments.Add(comment) });

        var code =
            """

            var x = 1; <!-- this is a 
            x += 2;
            --> block comment 
            x -= 1;
            """.Replace("\r\n", "\n");

        if (!expectSyntaxError)
        {
            Program script = GetParseActionFor(sourceType)(parser, code).As<Program>();

            Assert.Equal(2, comments.Count);

            var comment = comments[0];
            Assert.Equal(CommentKind.Line, comment.Kind);
            Assert.Equal(" this is a ", comment.GetContent(code).ToString());
            Assert.Equal("<!-- this is a ", comment.GetRawValue(code).ToString());
            Assert.Equal(Range.From(12, 27), comment.Range);
            Assert.Equal(SourceLocation.From(Position.From(2, 11), Position.From(2, 26)), comment.Location);

            comment = comments[1];
            Assert.Equal(CommentKind.Line, comment.Kind);
            Assert.Equal(" block comment ", comment.GetContent(code).ToString());
            Assert.Equal("--> block comment ", comment.GetRawValue(code).ToString());
            Assert.Equal(Range.From(36, 54), comment.Range);
            Assert.Equal(SourceLocation.From(Position.From(4, 0), Position.From(4, 18)), comment.Location);
        }
        else
        {
            Assert.Throws<SyntaxErrorException>(() => GetParseActionFor(sourceType)(parser, code));
        }
    }

    [Theory]
    [InlineData("script", EcmaVersion.ES2023, null, false)]
    [InlineData("script", EcmaVersion.ES2023, false, true)]
    [InlineData("script", EcmaVersion.ES2022, null, true)]
    [InlineData("script", EcmaVersion.ES2022, true, false)]
    [InlineData("module", EcmaVersion.ES2023, null, false)]
    [InlineData("module", EcmaVersion.ES2023, false, true)]
    [InlineData("module", EcmaVersion.ES2022, null, true)]
    [InlineData("module", EcmaVersion.ES2022, true, false)]
    [InlineData("expression", EcmaVersion.ES2023, null, true)]
    [InlineData("expression", EcmaVersion.ES2023, false, true)]
    [InlineData("expression", EcmaVersion.ES2023, true, true)]
    [InlineData("expression", EcmaVersion.ES2022, true, true)]
    public void ShouldParseHashBangComment(string sourceType, EcmaVersion ecmaVersion, bool? allowHashBang, bool expectSyntaxError)
    {
        var comments = new List<Comment>();
        var parserOptions = allowHashBang is not null
            ? new ParserOptions
            {
                AllowHashBang = allowHashBang.Value,
                EcmaVersion = ecmaVersion,
                OnComment = (in comment) => comments.Add(comment)
            }
            : new ParserOptions
            {
                EcmaVersion = ecmaVersion,
                OnComment = (in comment) => comments.Add(comment)
            };

        var parser = new Parser(parserOptions);
        var parseAction = GetParseActionFor(sourceType);

        var code =
            """
            #!/usr/bin/env node

            console.log("Hello world");
            """.Replace("\r\n", "\n");

        if (!expectSyntaxError)
        {
            var script = parseAction(parser, code);

            var comment = Assert.Single(comments);

            Assert.Equal(CommentKind.HashBang, comment.Kind);
            Assert.Equal("/usr/bin/env node", comment.GetContent(code).ToString());
            Assert.Equal("#!/usr/bin/env node", comment.GetRawValue(code).ToString());
            Assert.Equal(Range.From(0, 19), comment.Range);
            Assert.Equal(SourceLocation.From(Position.From(1, 0), Position.From(1, 19)), comment.Location);
        }
        else
        {
            Assert.Throws<SyntaxErrorException>(() => parseAction(parser, code));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void ThrowsErrorForInvalidHashBangComment(int startIndex)
    {
        var comments = new List<Comment>();
        var parser = new Parser(new ParserOptions { OnComment = (in comment) => comments.Add(comment) });

        var code =
            """
             #!/usr/bin/env node

            console.log("Hello world");
            """.Replace("\r\n", "\n");

        var ex = Assert.Throws<SyntaxErrorException>(() => parser.ParseScript(code, startIndex, code.Length - startIndex));
    }

    [Fact]
    public void ShouldParseCommentsWithinSliceOnly()
    {
        var comments = new List<Comment>();
        var parser = new Parser(new ParserOptions { OnComment = (in comment) => comments.Add(comment) });

        var code =
            """
            #!/usr/bin/env node
            // comment
            aaa 
             "use strict"; /*
            comment2 */
            """.Replace("\r\n", "\n");

        var program = parser.ParseScript(code, 34, 49 - 34, strict: true);

        Assert.Empty(comments);

        Assert.True(program.Strict);
        Assert.Equal(Range.From(34, 49), program.Range);
        Assert.Equal(SourceLocation.From(Position.From(3, 3), Position.From(4, 13)), program.Location);

        var statement = Assert.Single(program.Body);
        var directive = Assert.IsType<Directive>(statement);
        Assert.Equal(Range.From(37, 49), directive.Range);
    }

    private sealed class ParentNodeChecker : AstVisitor
    {
        private readonly Func<Node, Node?> _parentGetter;

        public ParentNodeChecker(Func<Node, Node?> parentGetter)
        {
            _parentGetter = parentGetter;
        }

        public void Check(Node node)
        {
            Assert.Null(_parentGetter(node));

            base.Visit(node);
        }

        public override object? Visit(Node node)
        {
            var parent = _parentGetter(node);
            Assert.NotNull(parent);
            Assert.Contains(node, parent.ChildNodes);

            return base.Visit(node);
        }
    }

    [Fact]
    public void ShouldParseLocation()
    {
        var parser = new Parser();
        var program = parser.ParseScript("// End on second line\r\n");

        Assert.Equal(Position.From(1, 0), program.Location.Start);
        Assert.Equal(Position.From(2, 0), program.Location.End);
    }

    [Fact]
    public void ProgramShouldBeStrict()
    {
        var parser = new Parser();
        var program = parser.ParseScript("'use strict'; function p() {}");

        Assert.True(program.Strict);
    }

    [Fact]
    public void ProgramShouldNotBeStrict()
    {
        var parser = new Parser();
        var program = parser.ParseScript("function p() {}");

        Assert.False(program.Strict);
    }

    [Fact]
    public void FunctionShouldNotBeStrict()
    {
        var parser = new Parser();
        var program = parser.ParseScript("function p() {}");
        var function = program.Body.First().As<FunctionDeclaration>();

        Assert.False(function.Body.Strict);
    }

    [Fact]
    public void FunctionWithUseStrictShouldBeStrict()
    {
        var parser = new Parser();
        var program = parser.ParseScript("function p() { 'use strict'; }");
        var function = program.Body.First().As<FunctionDeclaration>();

        Assert.True(function.Body.Strict);
    }

    [Fact]
    public void FunctionShouldBeStrictInProgramStrict()
    {
        var parser = new Parser();
        var program = parser.ParseScript("'use strict'; function p() {}");
        var function = program.Body.Skip(1).First().As<FunctionDeclaration>();

        Assert.True(function.Body.Strict);
    }

    [Fact]
    public void FunctionShouldBeStrict()
    {
        var parser = new Parser();
        var program = parser.ParseScript("function p() {'use strict'; return false;}");
        var function = program.Body.First().As<FunctionDeclaration>();

        Assert.True(function.Body.Strict);
    }

    [Fact]
    public void FunctionShouldBeStrictInStrictFunction()
    {
        var parser = new Parser();
        var program = parser.ParseScript("function p() {'use strict'; function q() { return; } return; }");
        var p = program.Body.First().As<FunctionDeclaration>();
        var q = p.Body.As<BlockStatement>().Body.Skip(1).First().As<FunctionDeclaration>();

        Assert.Equal("p", p.Id?.Name);
        Assert.Equal("q", q.Id?.Name);

        Assert.True(p.Body.Strict);
        Assert.True(q.Body.Strict);
    }

    [Fact]
    public void CodeFollowingStrictFunctionShouldNotBeStrict()
    {
        // The legacy octal literal is valid as it's not part of the strict function's code,
        // not even when it immediately follows the closing brace of the function body.
        // See https://github.com/adams85/acornima/issues/50

        var parser = new Parser();
        var program = parser.ParseScript("function p() {'use strict'; } 0755");

        var p = program.Body.First().As<FunctionDeclaration>();
        Assert.True(p.Body.Strict);

        var literal = program.Body.Skip(1).First().As<ExpressionStatement>().Expression.As<NumericLiteral>();
        Assert.Equal(493d, literal.Value); // 0755

        Assert.False(program.Strict);
    }

    [Theory]
    // In tolerant mode no error may be recorded for the token which follows the closing brace of a strict function body
    // when the enclosing code is not strict, and exactly one must be recorded when it is.
    // See https://github.com/adams85/acornima/issues/50
    [InlineData("function f() { 'use strict' } 0755", false, 0)]
    [InlineData("function f() { 'use strict'; } '\\222'", false, 0)]
    [InlineData("function f() { 'use strict' } 0755\nfunction g() { 'use strict' } 0644", false, 0)]
    [InlineData("function f() { 'use strict'; 0755 }", false, 1)]
    [InlineData("'use strict'; function f() {} 0755", false, 1)]
    [InlineData("function f() { 'use strict'; function g() {} 0755 }", false, 1)]
    [InlineData("function f() { 'use strict' } 0755", true, 1)]
    public void ShouldReportLegacyOctalFollowingStrictFunctionBodyOnlyInStrictCode(string input, bool strict, int expectedErrorCount)
    {
        var errorCollector = new ParseErrorCollector();
        var parser = new Parser(new ParserOptions { Tolerant = true, ErrorHandler = errorCollector });
        parser.ParseScript(input, strict: strict);

        Assert.Equal(expectedErrorCount, errorCollector.Errors.Count);
    }

    [Theory]
    [InlineData("'use strict'; 0", false, EcmaVersion.ES3, null)]
    [InlineData("'use strict'; 0", false, EcmaVersion.ES5, null)]
    [InlineData("'use strict'; 0", true, EcmaVersion.ES6, null)]
    [InlineData("'use strict'; 00", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("'use strict'; 00", true, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("'use strict'\nin {}; 00", false, EcmaVersion.ES6, "<no directive>")]
    [InlineData("'\\00'; 'use strict'; 00", false, EcmaVersion.ES3, null)]
    [InlineData("'\\00'; 'use strict'; 00", false, EcmaVersion.ES5, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("'\\00'; 'use strict'; 00", true, EcmaVersion.ES6, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("'use strict'; '\\00'; 00", false, EcmaVersion.ES3, null)]
    [InlineData("'use strict'; '\\00'; 00", false, EcmaVersion.ES5, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("'use strict'; '\\00'; 00", true, EcmaVersion.ES6, "Octal escape sequences are not allowed in strict mode")]

    [InlineData("'x';'use strict'; 0", false, EcmaVersion.ES5, null)]
    [InlineData("'x';'use strict'; 00", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("'x' 'use strict'; 0", false, EcmaVersion.ES5, "Unexpected string")]
    [InlineData("'x' 'use strict'; 00", false, EcmaVersion.ES5, "Unexpected string")]
    [InlineData("'x'\n'use strict'; 0", false, EcmaVersion.ES5, null)]
    [InlineData("'x'\n'use strict'; 00", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]

    [InlineData("function f() {'use strict'; 0 }", false, EcmaVersion.ES5, null)]
    [InlineData("() => {'use strict'; 0 }", true, EcmaVersion.ES6, null)]
    [InlineData("function f() {'use strict'; 00 }", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("() => {'use strict'; 00 }", true, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("function f() {'\\00'; 'use strict'; 00", false, EcmaVersion.ES5, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("() => {'\\00'; 'use strict'; 00", true, EcmaVersion.ES6, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("function f() {'use strict'; '\\00'; 00", false, EcmaVersion.ES5, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("function f() {'use strict'; '\\8'; 00", false, EcmaVersion.ES5, "\\8 and \\9 are not allowed in strict mode")]
    [InlineData("function f() {'use strict'; '\\9'; 00", false, EcmaVersion.ES5, "\\8 and \\9 are not allowed in strict mode")]
    [InlineData("() => {'use strict'; '\\00'; 00", true, EcmaVersion.ES6, "Octal escape sequences are not allowed in strict mode")]

    [InlineData("(x = 0) => 00", false, EcmaVersion.ES6, "<no directive>")]
    [InlineData("(x = 0) => 00", true, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("(x = 0) => { 00 }", false, EcmaVersion.ES6, "<no directive>")]
    [InlineData("(x = 0) => { 00 }", true, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("(x = 0) => {'use strict'; 0 }", false, EcmaVersion.ES6, null)]
    [InlineData("(x = 0) => {'use strict'; 0 }", false, EcmaVersion.ES7, "Illegal 'use strict' directive in function with non-simple parameter list")]
    [InlineData("'use strict'; (x = 0) => {'use strict'; 0 }", false, EcmaVersion.ES6, null)]
    [InlineData("(x = 0) => {'use strict'; 0 }", true, EcmaVersion.ES6, null)]
    [InlineData("(x = 0) => {'use strict'; 00 }", false, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("(x = 0) => {'use strict'; 00 }", false, EcmaVersion.ES7, "Illegal 'use strict' directive in function with non-simple parameter list")]
    [InlineData("'use strict'; (x = 0) => {'use strict'; 00 }", false, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("(x = 0) => {'use strict'; 00 }", true, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("(x = 0) => {'\\00'; 'use strict'; 00", false, EcmaVersion.ES6, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("(x = 0) => {'\\8'; 'use strict'; 00", false, EcmaVersion.ES6, "\\8 and \\9 are not allowed in strict mode")]
    [InlineData("(x = 0) => {'\\9'; 'use strict'; 00", false, EcmaVersion.ES6, "\\8 and \\9 are not allowed in strict mode")]
    [InlineData("(x = 0) => {'\\00'; 'use strict'; 00", false, EcmaVersion.ES7, "Illegal 'use strict' directive in function with non-simple parameter list")]
    [InlineData("'use strict'; (x = 0) => {'\\00'; 'use strict'; 00", false, EcmaVersion.ES6, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("(x = 0) => {'\\00'; 'use strict'; 00", true, EcmaVersion.ES6, "Octal escape sequences are not allowed in strict mode")]

    [InlineData("(x = 0) => {'use strict'; 0 }; 00", false, EcmaVersion.ES6, null)]
    [InlineData("'use strict'; (x = 0) => {'use strict'; 0 }; 00", false, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("(x = 0) => {'use strict'; 0 }; 00", true, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]

    // Strict mode must be turned off before the token which follows the closing brace of a strict function body is read,
    // as that token already belongs to the enclosing code. See https://github.com/adams85/acornima/issues/50
    [InlineData("function f() { 'use strict' } 0755", false, EcmaVersion.ES5, null)]
    [InlineData("function f() { 'use strict'; } 0755", false, EcmaVersion.ES5, null)]
    [InlineData("function f() { 'use strict' } 00", false, EcmaVersion.ES5, null)]
    [InlineData("function f() { 'use strict'; } 08", false, EcmaVersion.ES5, null)]
    [InlineData("function f() { 'use strict'; } 09", false, EcmaVersion.ES5, null)]
    [InlineData("function f() { 'use strict'; } '\\222'", false, EcmaVersion.ES5, null)]
    [InlineData("function f() { 'use strict'; } '\\8'", false, EcmaVersion.ES5, null)]
    [InlineData("function f() { 'use strict'; } '\\9'", false, EcmaVersion.ES5, null)]
    [InlineData("function f() { 'use strict' } 0755", false, EcmaVersion.ES3, null)]
    [InlineData("x = function() { 'use strict' }\n0755", false, EcmaVersion.ES5, null)]
    [InlineData("async function f() { 'use strict' } 0755", false, EcmaVersion.ES8, null)]
    [InlineData("function* g() { 'use strict' } 0755", false, EcmaVersion.ES6, null)]
    [InlineData("async function* g() { 'use strict' } 0755", false, EcmaVersion.ES9, null)]
    [InlineData("x = () => { 'use strict' }\n0755", false, EcmaVersion.ES6, null)]
    [InlineData("x = () => { 'use strict' }\n'\\222'", false, EcmaVersion.ES6, null)]
    [InlineData("x = async () => { 'use strict' }\n0755", false, EcmaVersion.ES8, null)]
    [InlineData("({ m() { 'use strict' }, n: 0755 })", false, EcmaVersion.ES6, null)]
    [InlineData("({ get m() { 'use strict' }, n: 0755 })", false, EcmaVersion.ES6, null)]
    [InlineData("({ set m(v) { 'use strict' }, n: 0755 })", false, EcmaVersion.ES6, null)]
    [InlineData("class C { m() { 'use strict' } } 0755", false, EcmaVersion.ES6, null)]

    // ...but only when the enclosing code isn't strict for a reason of its own, in which case strict mode must be kept.
    [InlineData("'use strict'; function f() {} 0755", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("'use strict'; function f() { 'use strict' } 0755", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("function f() { 'use strict' } 0755", true, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("function f() { 'use strict'; function g() {} 0755 }", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("function f() { 'use strict'; function g() { 'use strict' } 0755 }", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("function f() { 'use strict'; function g() { 'use strict'; } '\\222' }", false, EcmaVersion.ES5, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("'use strict'; { } 0755", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("function f() { 'use strict'; { } 0755 }", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("function f() { 'use strict'; if (x) { } 0755 }", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("function f() { 'use strict'; try { } finally { } 0755 }", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("function f() { 'use strict'; with (x) {} } 0755", false, EcmaVersion.ES5, "Strict mode code may not include a with statement")]
    [InlineData("class C { static { 0755 } }", false, EcmaVersion.ES13, "Octal literals are not allowed in strict mode")]

    [InlineData("'use strict';\r\nfunction f(arguments){}", false, EcmaVersion.ES3, null)]
    [InlineData("'use strict';\r\nfunction f(arguments){}", false, EcmaVersion.ES5, "Unexpected eval or arguments in strict mode")]
    [InlineData("'use strict';\r\n(arguments)=>{}", false, EcmaVersion.ES6, "Unexpected eval or arguments in strict mode")]
    [InlineData("'use strict'\r\nfunction f(eval){}", false, EcmaVersion.ES3, null)]
    [InlineData("'use strict'\r\nfunction f(eval){}", false, EcmaVersion.ES5, "Unexpected eval or arguments in strict mode")]
    [InlineData("'use strict'\r\n(eval)=>{}", false, EcmaVersion.ES6, "Unexpected token '=>'")] // V8 reports "Malformed arrow function parameter list"

    // A "use strict" directive which is terminated by automatic semicolon insertion must put the parser into strict mode
    // before the token following the directive is checked, even though that token is necessarily scanned earlier
    // (it is the very token which the ASI decision is based on). See https://github.com/adams85/acornima/issues/47
    [InlineData("'use strict'\n0755", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("'use strict'\n0755", true, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("'use strict'\r\n0755", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("'use strict'\n00", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("'use strict'\n08", false, EcmaVersion.ES5, "Decimals with leading zeros are not allowed in strict mode")]
    [InlineData("'use strict'\n09", false, EcmaVersion.ES5, "Decimals with leading zeros are not allowed in strict mode")]
    [InlineData("'use strict'\n08.5", false, EcmaVersion.ES5, "Decimals with leading zeros are not allowed in strict mode")]
    [InlineData("'use strict'\n'\\222'", false, EcmaVersion.ES5, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("'use strict'\n'\\8'", false, EcmaVersion.ES5, "\\8 and \\9 are not allowed in strict mode")]
    [InlineData("'use strict'\n'\\9'", false, EcmaVersion.ES5, "\\8 and \\9 are not allowed in strict mode")]
    [InlineData("'use strict'\n/*c*/ 0755", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("'use strict'\n//c\n0755", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("'use strict'\n0755 + 0644", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("'use strict'\n'\\222' + '\\222'", false, EcmaVersion.ES5, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("'use strict'\n0755\n0644", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("'use strict'\nvar z;\n0755", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("'x'\n'use strict'\n0755", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("'\\222'\n'use strict'\n0755", false, EcmaVersion.ES5, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("'use strict'\n'use strict'\n0755", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("function f() { 'use strict'\n0755 }", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("function f() { 'use strict'\n'\\222' }", false, EcmaVersion.ES5, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("function f() { 'use strict'\n'\\8' }", false, EcmaVersion.ES5, "\\8 and \\9 are not allowed in strict mode")]
    [InlineData("function f() { 'use strict'\n0755 }", true, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("(function() { 'use strict'\n0755 })", false, EcmaVersion.ES5, "Octal literals are not allowed in strict mode")]
    [InlineData("() => { 'use strict'\n0755 }", false, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("({ m() { 'use strict'\n0755 } })", false, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("class C { m() { 'use strict'\n0755 } }", false, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("async function f() { 'use strict'\n0755 }", false, EcmaVersion.ES8, "Octal literals are not allowed in strict mode")]
    [InlineData("function* g() { 'use strict'\n0755 }", false, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("({ get m() { 'use strict'\n0755 } })", false, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("({ set m(v) { 'use strict'\n0755 } })", false, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]
    [InlineData("0755", true, EcmaVersion.ES6, "Octal literals are not allowed in strict mode")]

    // ...but only when the string literal is actually a directive, that is, when automatic semicolon insertion does apply,
    // and only when the directive is a "use strict" directive.
    [InlineData("'use strict'\n0", false, EcmaVersion.ES5, null)]
    [InlineData("'use strict'\n0.5", false, EcmaVersion.ES5, null)]
    [InlineData("'use strict'\n0x1F", false, EcmaVersion.ES5, null)]
    [InlineData("'use strict'\n0e755", false, EcmaVersion.ES5, null)]
    [InlineData("'use strict'\n+0755", false, EcmaVersion.ES5, "<no directive>")]
    [InlineData("'use strict'\n.length; 0755", false, EcmaVersion.ES5, "<no directive>")]
    [InlineData("'use strict'\ninstanceof String; 0755", false, EcmaVersion.ES5, "<no directive>")]
    [InlineData("'use strict'\nin {}; 0755", false, EcmaVersion.ES6, "<no directive>")]
    [InlineData("'use strict'\n`\\222`", false, EcmaVersion.ES9, "<no directive>")] // a tagged template, so no ASI and hence no directive at all (invalid escapes in tagged templates are allowed since ES2018)
    [InlineData("'x'\n0755", false, EcmaVersion.ES5, null)]
    [InlineData("'\\222'\n0755", false, EcmaVersion.ES5, null)]
    [InlineData("'\\8'\n0755", false, EcmaVersion.ES5, null)]
    [InlineData("0755\n'use strict'", false, EcmaVersion.ES5, "<no directive>")]
    [InlineData("0755", false, EcmaVersion.ES5, "<no directive>")]
    [InlineData("function f() { 0755 }", false, EcmaVersion.ES5, "<no directive>")]
    [InlineData("function f() { 0755; 'use strict'; }", false, EcmaVersion.ES5, "<no directive>")]
    [InlineData("function f() { 'use strict'\nvar x }", false, EcmaVersion.ES5, null)]
    [InlineData("'use strict'\n0755", false, EcmaVersion.ES3, "<no directive>")]

    // The retroactive check of the directive prologue's own string literals must keep working.
    [InlineData("function f() { '\\222'; 'use strict'; }", false, EcmaVersion.ES5, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("function f() { '\\8'; 'use strict'; }", false, EcmaVersion.ES5, "\\8 and \\9 are not allowed in strict mode")]
    [InlineData("function f() { '\\222'\n'use strict'\n}", false, EcmaVersion.ES5, "Octal escape sequences are not allowed in strict mode")]

    [InlineData("function f() { '\\077'; 'use strict'; await x }", false, EcmaVersion.ES8, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("function f() { '\\077'; 'use strict' await x }", false, EcmaVersion.ES8, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("function f() { '\\077'; 'use strict' \n await x }", false, EcmaVersion.ES8, "Octal escape sequences are not allowed in strict mode")]
    [InlineData("function f() { 'use strict'; await '\\077' }", false, EcmaVersion.ES8, "Octal escape sequences are not allowed in strict mode")] // V8 reports "await is only valid in async functions and the top level bodies of modules"
    [InlineData("function f() { 'use strict' await '\\077' }", false, EcmaVersion.ES8, "Unexpected identifier 'await'")]  // V8 reports "Unexpected reserved word"
    [InlineData("function f() { 'use strict' \n await '\\077' }", false, EcmaVersion.ES8, "Octal escape sequences are not allowed in strict mode")] // V8 reports "await is only valid in async functions and the top level bodies of modules"
    public void ShouldHandleStrictModeDetectionEdgeCases(string input, bool isModule, EcmaVersion ecmaVersion, string? expectedError)
    {
        var parser = new Parser(new ParserOptions { EcmaVersion = ecmaVersion });

        var expectDirective = true;
        if (expectedError is null || !(expectDirective = expectedError != "<no directive>"))
        {
            Program root = isModule ? parser.ParseModule(input) : parser.ParseScript(input);
            Assert.NotNull(root);

            if (expectDirective)
            {
                if (ecmaVersion >= EcmaVersion.ES5)
                {
                    Assert.Contains(root.DescendantNodes(), stmt => stmt.GetType() == typeof(Directive));
                }
                else
                {
                    Assert.DoesNotContain(root.DescendantNodes(), stmt => stmt.GetType() == typeof(Directive));
                }
            }
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => isModule ? parser.ParseModule(input) : parser.ParseScript(input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Theory]
    // The error must be reported for the offending construct itself, not for the directive,
    // and the source order of errors must be preserved. See https://github.com/adams85/acornima/issues/47
    [InlineData("'use strict'\n0755", "StrictOctalLiteral", 13, 2, 0)]
    [InlineData("'use strict'\r\n0755", "StrictOctalLiteral", 14, 2, 0)]
    [InlineData("'use strict'\n00", "StrictOctalLiteral", 13, 2, 0)]
    [InlineData("'use strict'\n08", "StrictDecimalWithLeadingZero", 13, 2, 0)]
    [InlineData("'use strict'\n09", "StrictDecimalWithLeadingZero", 13, 2, 0)]
    [InlineData("'use strict'\n'\\222'", "StrictOctalEscape", 14, 2, 1)]
    [InlineData("'use strict'\n'\\8'", "Strict8Or9Escape", 14, 2, 1)]
    [InlineData("'use strict'\n'\\9'", "Strict8Or9Escape", 14, 2, 1)]
    [InlineData("'use strict'\n/*c*/ 0755", "StrictOctalLiteral", 19, 2, 6)]
    [InlineData("'use strict'\n0755 + 0644", "StrictOctalLiteral", 13, 2, 0)]
    [InlineData("'use strict'\n'\\222' + '\\222'", "StrictOctalEscape", 14, 2, 1)]
    [InlineData("function f() { 'use strict'\n0755 }", "StrictOctalLiteral", 28, 2, 0)]
    [InlineData("'\\222'\n'use strict'\n0755", "StrictOctalEscape", 1, 1, 1)]
    public void ShouldReportPositionOfLegacyOctalFollowingAsiTerminatedUseStrictDirective(
        string input, string expectedErrorCode, int expectedIndex, int expectedLineNumber, int expectedColumn)
    {
        var parser = new Parser();

        var ex = Assert.Throws<SyntaxErrorException>(() => parser.ParseScript(input));
        Assert.Equal(expectedErrorCode, ex.Error.Code);
        Assert.Equal(expectedIndex, ex.Error.Index);
        Assert.Equal(expectedLineNumber, ex.LineNumber);
        Assert.Equal(expectedColumn, ex.Column);
    }

    [Theory]
    // In tolerant mode the retroactive check must report each offending construct exactly once,
    // that is, it must not report the same construct which the tokenizer has already reported
    // (and vice versa). See https://github.com/adams85/acornima/issues/47
    [InlineData("'use strict'\n0755", 1)]
    [InlineData("'use strict'\n'\\222'", 1)]
    [InlineData("'use strict'\n0755\n0644", 2)]
    [InlineData("'\\222'\n'use strict'\n0755", 2)]
    [InlineData("'use strict'\n0", 0)]
    [InlineData("'x'\n0755", 0)]
    public void ShouldReportLegacyOctalFollowingAsiTerminatedUseStrictDirectiveExactlyOnce(string input, int expectedErrorCount)
    {
        var errorCollector = new ParseErrorCollector();
        var parser = new Parser(new ParserOptions { Tolerant = true, ErrorHandler = errorCollector });
        parser.ParseScript(input);

        Assert.Equal(expectedErrorCount, errorCollector.Errors.Count);
    }

    [Theory]
    // When strict mode is turned on by the parser option, it already applies to the very first token,
    // so nothing needs to be (and nothing may be) reported retroactively.
    [InlineData("0755", "StrictOctalLiteral")]
    [InlineData("08", "StrictDecimalWithLeadingZero")]
    [InlineData("'\\222'", "StrictOctalEscape")]
    [InlineData("'\\8'", "Strict8Or9Escape")]
    [InlineData("'use strict'\n0755", "StrictOctalLiteral")]
    [InlineData("'use strict'\n'\\222'", "StrictOctalEscape")]
    public void ShouldReportLegacyOctalWhenStrictModeIsTurnedOnByOption(string input, string expectedErrorCode)
    {
        var parser = new Parser();

        var ex = Assert.Throws<SyntaxErrorException>(() => parser.ParseScript(input, strict: true));
        Assert.Equal(expectedErrorCode, ex.Error.Code);
    }

    [Theory]
    [InlineData("script", "(class { x = () => arguments })", EcmaVersion.Latest, "'arguments' is not allowed in class field initializer or static initialization block")]
    [InlineData("script", "() => { (class { x = () => arguments }) }", EcmaVersion.Latest, "'arguments' is not allowed in class field initializer or static initialization block")]
    [InlineData("script", "() => class { x = () => { arguments } }", EcmaVersion.Latest, "'arguments' is not allowed in class field initializer or static initialization block")]
    [InlineData("script", "() => class { x = function() { arguments } }", EcmaVersion.Latest, null)]
    public void ShouldHandleArgumentsEdgeCases(string sourceType, string input, EcmaVersion ecmaVersion, string? expectedError)
    {
        var parser = new Parser(new ParserOptions { EcmaVersion = ecmaVersion });
        var parseAction = GetParseActionFor(sourceType);

        if (expectedError is null)
        {
            Assert.NotNull(parseAction(parser, input));
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parseAction(parser, input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Theory]
    [InlineData("script", "class C { x = () => new.target }", EcmaVersion.Latest, null)]
    [InlineData("script", "(class { x = () => new.target })", EcmaVersion.Latest, null)]
    [InlineData("script", "() => { (class { x = () => new.target }) }", EcmaVersion.Latest, null)]
    [InlineData("script", "() => class { x = () => { new.target } }", EcmaVersion.Latest, null)]
    [InlineData("script", "() => class { x = function() { new.target } }", EcmaVersion.Latest, null)]

    [InlineData("script", "class C { [new.target]() { } }", EcmaVersion.Latest, "new.target expression is not allowed here")]
    [InlineData("script", "() => class C { [new.target]() { } }", EcmaVersion.Latest, "new.target expression is not allowed here")]
    [InlineData("script", "() => { return class C { [new.target]() { } } }", EcmaVersion.Latest, "new.target expression is not allowed here")]
    [InlineData("script", "function f() { return class C { [new.target]() { } } }", EcmaVersion.Latest, null)]
    [InlineData("script", "(function() { return class C { [new.target]() { } } })", EcmaVersion.Latest, null)]

    [InlineData("script", "class C { m(a = new.target) { } }", EcmaVersion.Latest, null)]
    [InlineData("script", "class C { m({ [new.target]: a }) { } }", EcmaVersion.Latest, null)]
    public void ShouldHandleNewTargetEdgeCases(string sourceType, string input, EcmaVersion ecmaVersion, string? expectedError)
    {
        var parser = new Parser(new ParserOptions { EcmaVersion = ecmaVersion });
        var parseAction = GetParseActionFor(sourceType);

        if (expectedError is null)
        {
            Assert.NotNull(parseAction(parser, input));
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parseAction(parser, input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Theory]
    [InlineData("script", "(class { x = () => super.y })", EcmaVersion.Latest, null)]
    [InlineData("script", "() => { (class { x = () => super.y }) }", EcmaVersion.Latest, null)]
    [InlineData("script", "() => class { x = () => { super.y } }", EcmaVersion.Latest, null)]
    [InlineData("script", "() => class { x = function() { super.y } }", EcmaVersion.Latest, "'super' keyword unexpected here")]
    [InlineData("script", "class C { x = class extends super.constructor { [super.constructor.name] = super.constructor } }", EcmaVersion.Latest, null)]
    [InlineData("script", "() => class { x = class extends super.constructor { [super.constructor.name] = super.constructor } }", EcmaVersion.Latest, null)]

    [InlineData("script", "class C extends Object { constructor() { class X { p = super() } } }", EcmaVersion.Latest, "'super' keyword unexpected here")]
    [InlineData("script", "class C extends Object { constructor() { class X { [super()]() {} } } }", EcmaVersion.Latest, null)]
    [InlineData("script", "class C extends Object { constructor() { class X { m(a = super()) {} } } }", EcmaVersion.Latest, "'super' keyword unexpected here")]
    [InlineData("script", "class C extends Object { constructor() { class X { m({[super()]: a }) {} } } }", EcmaVersion.Latest, "'super' keyword unexpected here")]

    [InlineData("script", "class C { m() { return class X { p = super.toString() } } }", EcmaVersion.Latest, null)]
    [InlineData("script", "class C { m() { return class X { [super.toString()]() {} } } }", EcmaVersion.Latest, null)]
    [InlineData("script", "class C { m() { return class X { m(a = super.toString()) {} } } }", EcmaVersion.Latest, null)]
    [InlineData("script", "class C { m() { return class X { m({[super.toString()]: a }) {} } } }", EcmaVersion.Latest, null)]

    [InlineData("script", "class C { m = () => class X { p = super.toString() } }", EcmaVersion.Latest, null)]
    [InlineData("script", "class C { m = () => class X { [super.toString()]() {} } }", EcmaVersion.Latest, null)]
    [InlineData("script", "class C { m = () => class X { m(a = super.toString()) {} } }", EcmaVersion.Latest, null)]
    [InlineData("script", "class C { m = () => class X { m({[super.toString()]: a }) {} } }", EcmaVersion.Latest, null)]
    public void ShouldHandleSuperKeywordEdgeCases(string sourceType, string input, EcmaVersion ecmaVersion, string? expectedError)
    {
        var parser = new Parser(new ParserOptions { EcmaVersion = ecmaVersion });
        var parseAction = GetParseActionFor(sourceType);

        if (expectedError is null)
        {
            Assert.NotNull(parseAction(parser, input));
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parseAction(parser, input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Theory]
    // Direct super calls are not allowed at the top level unless AllowSuperCallOutsideConstructor is enabled.
    // (AllowSuperOutsideMethod alone doesn't enable them.)
    [InlineData("script", "super()", false, false, "'super' keyword unexpected here")]
    [InlineData("script", "super()", true, false, "'super' keyword unexpected here")]
    [InlineData("script", "super()", false, true, null)]
    [InlineData("script", "super()", true, true, null)]
    [InlineData("module", "super()", false, false, "'super' keyword unexpected here")]
    [InlineData("module", "super()", false, true, null)]
    [InlineData("expression", "super()", false, false, "'super' keyword unexpected here")]
    [InlineData("expression", "super()", false, true, null)]

    // Arrow functions inherit the this binding of the top level, so direct super calls are allowed in them as well.
    [InlineData("script", "(() => super())()", false, false, "'super' keyword unexpected here")]
    [InlineData("script", "(() => super())()", false, true, null)]
    [InlineData("script", "() => () => super()", false, true, null)]
    [InlineData("script", "async () => super()", false, true, null)]
    // (The argument of the nested eval call is just a string literal to the parser. When the host parses that string,
    // the top level cases above apply to it.)
    [InlineData("script", "(() => eval('super()'))()", false, false, null)]
    [InlineData("script", "(() => eval('super()'))()", false, true, null)]

    // Ordinary functions introduce a this binding of their own, so direct super calls remain disallowed in them.
    [InlineData("script", "function f() { super() }", false, true, "'super' keyword unexpected here")]
    [InlineData("script", "function f() { super() }", true, true, "'super' keyword unexpected here")]
    [InlineData("script", "(function () { super() })", false, true, "'super' keyword unexpected here")]
    [InlineData("script", "() => function () { super() }", false, true, "'super' keyword unexpected here")]
    [InlineData("script", "({ m() { super() } })", false, true, "'super' keyword unexpected here")]

    // Super property accesses are allowed wherever direct super calls are, and AllowSuperOutsideMethod allows
    // exactly those, without allowing direct super calls.
    [InlineData("script", "super.x", false, false, "'super' keyword unexpected here")]
    [InlineData("script", "super.x", true, false, null)]
    [InlineData("script", "super.x", false, true, null)]
    [InlineData("script", "(() => super.x)()", false, true, null)]
    [InlineData("script", "(() => super.x)()", true, false, null)]
    // (Both options follow the top level's this binding, so neither of them reaches into an ordinary function.)
    [InlineData("script", "function f() { super.x }", false, true, "'super' keyword unexpected here")]
    [InlineData("script", "function f() { super.x }", true, false, "'super' keyword unexpected here")]
    [InlineData("script", "function f() { super.x }", true, true, "'super' keyword unexpected here")]
    // (Methods bring a home object of their own, so they are unaffected by either option.)
    [InlineData("script", "({ m() { super.x } })", false, false, null)]
    [InlineData("script", "({ m() { super.x } })", true, false, null)]

    // Classes are unaffected: the constructor of a derived class remains the only place where direct super calls are allowed.
    [InlineData("script", "class A extends B { constructor() { super() } }", false, false, null)]
    [InlineData("script", "class A extends B { constructor() { super() } }", false, true, null)]
    [InlineData("script", "class A { constructor() { super() } }", false, false, "'super' keyword unexpected here")]
    [InlineData("script", "class A { constructor() { super() } }", false, true, "'super' keyword unexpected here")]
    [InlineData("script", "class A extends B { m() { super() } }", false, true, "'super' keyword unexpected here")]
    [InlineData("script", "class A extends B { constructor() { function f() { super() } } }", false, true, "'super' keyword unexpected here")]
    [InlineData("script", "class C { x = super.y }", false, false, null)]
    [InlineData("script", "class C { x = super.y }", false, true, null)]
    // (Class field initializers don't introduce a this binding of their own, but they are never a place for a direct
    // super call either: https://tc39.es/ecma262/#sec-class-definitions-static-semantics-early-errors makes it a
    // Syntax Error if the Initializer Contains SuperCall. So neither the constructor of a derived class nor the
    // option enables one there.)
    [InlineData("script", "class A extends B { constructor() { class C { x = super() } } }", false, false, "'super' keyword unexpected here")]
    [InlineData("script", "class C { x = super() }", false, false, "'super' keyword unexpected here")]
    [InlineData("script", "class C { x = super() }", false, true, "'super' keyword unexpected here")]
    // (A computed class element name, on the other hand, is evaluated in the enclosing scope, so it inherits
    // whatever that scope allows.)
    [InlineData("script", "class C { [super()]() { } }", false, false, "'super' keyword unexpected here")]
    [InlineData("script", "class C { [super()]() { } }", false, true, null)]
    public void ShouldHandleSuperCallOutsideConstructor(string sourceType, string input, bool allowSuperOutsideMethod, bool allowSuperCallOutsideConstructor, string? expectedError)
    {
        var parser = new Parser(new ParserOptions
        {
            AllowSuperOutsideMethod = allowSuperOutsideMethod,
            AllowSuperCallOutsideConstructor = allowSuperCallOutsideConstructor,
        });
        var parseAction = GetParseActionFor(sourceType);

        if (expectedError is null)
        {
            Assert.NotNull(parseAction(parser, input));
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parseAction(parser, input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Fact]
    public void AllowSuperCallOutsideConstructorShouldDefaultToFalse()
    {
        Assert.False(new ParserOptions().AllowSuperCallOutsideConstructor);
        Assert.False(ParserOptions.Default.AllowSuperCallOutsideConstructor);

        var options = ParserOptions.Default with { AllowSuperCallOutsideConstructor = true };
        Assert.True(options.AllowSuperCallOutsideConstructor);
        Assert.False(ParserOptions.Default.AllowSuperCallOutsideConstructor);

        // The option must survive further copies of the options object.
        Assert.True((options with { EcmaVersion = EcmaVersion.ES2022 }).AllowSuperCallOutsideConstructor);
    }

    [Theory]
    [InlineData("script", "(class { x = await })", EcmaVersion.Latest, null)]
    [InlineData("module", "(class { x = await })", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "(class { x = await 1 })", EcmaVersion.Latest, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("module", "(class { x = await 1 })", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "(class { x = () => await })", EcmaVersion.Latest, null)]
    [InlineData("module", "(class { x = () => await })", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "(class { x = () => await 1 })", EcmaVersion.Latest, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("module", "(class { x = () => await 1 })", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "(class { x = async () => await })", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "(class { x = async () => await })", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "(class { x = async () => await 1 })", EcmaVersion.Latest, null)]
    [InlineData("module", "(class { x = async () => await 1 })", EcmaVersion.Latest, null)]

    [InlineData("script", "() => class { x = await }", EcmaVersion.Latest, null)]
    [InlineData("module", "() => class { x = await }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "() => class { x = await 1 }", EcmaVersion.Latest, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("module", "() => class { x = await 1 }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "() => class { x = () => await }", EcmaVersion.Latest, null)]
    [InlineData("module", "() => class { x = () => await }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "() => class { x = () => await 1 }", EcmaVersion.Latest, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("module", "() => class { x = () => await 1 }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "() => class { x = async () => await }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "() => class { x = async () => await }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "() => class { x = async () => await 1 }", EcmaVersion.Latest, null)]
    [InlineData("module", "() => class { x = async () => await 1 }", EcmaVersion.Latest, null)]

    [InlineData("script", "async () => class { x = await }", EcmaVersion.Latest, null)]
    [InlineData("module", "async () => class { x = await }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async () => class { x = await 1 }", EcmaVersion.Latest, "Unexpected number")]
    [InlineData("module", "async () => class { x = await 1 }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async () => class { x = () => await }", EcmaVersion.Latest, null)]
    [InlineData("module", "async () => class { x = () => await }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async () => class { x = () => await 1 }", EcmaVersion.Latest, "Unexpected number")]
    [InlineData("module", "async () => class { x = () => await 1 }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async () => class { x = async () => await }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async () => class { x = async () => await }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async () => class { x = async () => await 1 }", EcmaVersion.Latest, null)]
    [InlineData("module", "async () => class { x = async () => await 1 }", EcmaVersion.Latest, null)]

    [InlineData("script", "async () => class { x = (a = await) => a }", EcmaVersion.Latest, null)]
    [InlineData("module", "async () => class { x = (a = await) => a }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async () => class { x = (a = await 1) => a }", EcmaVersion.Latest, "Unexpected number")]
    [InlineData("module", "async () => class { x = (a = await 1) => a }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async () => class { x = class await { y = await } }", EcmaVersion.Latest, null)]
    [InlineData("module", "async () => class { x = class await { y = await } }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async () => class { x = class await { y = await 1 } }", EcmaVersion.Latest, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("module", "async () => class { x = class await { y = await 1 } }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async () => class { x = () => { { try {} catch (await) { } } } }", EcmaVersion.Latest, null)]
    [InlineData("module", "async () => class { x = () => { { try {} catch (await) { } } } }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async () => class { x = () => { { try {} catch { var await = 1 } } } }", EcmaVersion.Latest, null)]
    [InlineData("module", "async () => class { x = () => { { try {} catch { var await = 1 } } } }", EcmaVersion.Latest, "Unexpected reserved word")]
    public void ShouldHandleAwaitInClassFieldInitializer(string sourceType, string input, EcmaVersion ecmaVersion, string? expectedError)
    {
        // See also: https://github.com/acornjs/acorn/issues/1334, https://github.com/acornjs/acorn/issues/1338

        var parser = new Parser(new ParserOptions { EcmaVersion = ecmaVersion });
        var parseAction = GetParseActionFor(sourceType);

        if (expectedError is null)
        {
            Assert.NotNull(parseAction(parser, input));
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parseAction(parser, input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Theory]
    [InlineData("script", "await", EcmaVersion.Latest, null)]
    [InlineData("script", "await", EcmaVersion.ES13, null)]
    [InlineData("script", "await", EcmaVersion.ES8, null)]
    [InlineData("script", "await", EcmaVersion.ES7, null)]
    [InlineData("module", "await", EcmaVersion.Latest, "Unexpected end of input")]
    [InlineData("module", "await", EcmaVersion.ES13, "Unexpected end of input")]
    [InlineData("module", "await", EcmaVersion.ES12, "Unexpected reserved word")]
    [InlineData("module", "await", EcmaVersion.ES6, "Unexpected reserved word")]
    [InlineData("script", "await 0", EcmaVersion.Latest, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("script", "await 0", EcmaVersion.ES13, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("script", "await 0", EcmaVersion.ES8, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("script", "await 0", EcmaVersion.ES7, "Unexpected number")]
    [InlineData("module", "await 0", EcmaVersion.Latest, null)]
    [InlineData("module", "await 0", EcmaVersion.ES13, null)]
    [InlineData("module", "await 0", EcmaVersion.ES12, "Unexpected reserved word")]
    [InlineData("module", "await 0", EcmaVersion.ES6, "Unexpected reserved word")]
    [InlineData("script", "{ await 0 }", EcmaVersion.Latest, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("script", "{ await 0 }", EcmaVersion.ES13, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("script", "{ await 0 }", EcmaVersion.ES8, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("script", "{ await 0 }", EcmaVersion.ES7, "Unexpected number")]
    [InlineData("module", "{ await 0 }", EcmaVersion.Latest, null)]
    [InlineData("module", "{ await 0 }", EcmaVersion.ES13, null)]
    [InlineData("module", "{ await 0 }", EcmaVersion.ES12, "Unexpected reserved word")]
    [InlineData("module", "{ await 0 }", EcmaVersion.ES6, "Unexpected reserved word")]
    [InlineData("script", "for await (x of a) {}", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "for await (x of a) {}", EcmaVersion.ES13, "Unexpected reserved word")]
    [InlineData("script", "for await (x of a) {}", EcmaVersion.ES12, "Unexpected reserved word")]
    [InlineData("script", "for await (x of a) {}", EcmaVersion.ES9, "Unexpected reserved word")]
    [InlineData("script", "for await (x of a) {}", EcmaVersion.ES8, "Unexpected identifier 'await'")]
    [InlineData("module", "for await (x of a) {}", EcmaVersion.Latest, null)]
    [InlineData("module", "for await (x of a) {}", EcmaVersion.ES13, null)]
    [InlineData("module", "for await (x of a) {}", EcmaVersion.ES12, "Unexpected reserved word")]
    [InlineData("module", "for await (x of a) {}", EcmaVersion.ES9, "Unexpected reserved word")]
    [InlineData("module", "for await (x of a) {}", EcmaVersion.ES8, "Unexpected identifier 'await'")]
    public void ShouldHandleAwaitOutsideFunction(string sourceType, string input, EcmaVersion ecmaVersion, string? expectedError)
    {
        var parser = new Parser(new ParserOptions { EcmaVersion = ecmaVersion });
        var parseAction = GetParseActionFor(sourceType);

        if (expectedError is null)
        {
            Assert.NotNull(parseAction(parser, input));
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parseAction(parser, input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Theory]
    [InlineData("script", "async function f() { var await = 0 }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { var await = 0 }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { var [await] = [] }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token ']'"
    [InlineData("module", "async function f() { var [await] = [] }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token ']'"
    [InlineData("script", "async function f() { var [x = await] = [] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { var [x = await] = [] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { var [...await] = [] }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token ']'"
    [InlineData("module", "async function f() { var [...await] = [] }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token ']'"
    [InlineData("script", "async function f() { var {await} = {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { var {await} = {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { var {x: await} = {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token '}'"
    [InlineData("module", "async function f() { var {x: await} = {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token '}'"
    [InlineData("script", "async function f() { var {x = await} = {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { var {x = await} = {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { var {...await} = {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token '}'"
    [InlineData("module", "async function f() { var {...await} = {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token '}'"
    [InlineData("script", "async function f() { var [{await}] = [] }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { var [{await}] = [] }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async function f() { fn = await => 1 }", EcmaVersion.Latest, "Unexpected token '=>'")]
    [InlineData("module", "async function f() { fn = await => 1 }", EcmaVersion.Latest, "Unexpected token '=>'")]
    [InlineData("script", "async function f() { (await) => {} }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("module", "async function f() { (await) => {} }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("script", "async function f() { (...await) => {} }", EcmaVersion.Latest, "Unexpected token ')'")] // V8 reports "Unexpected reserved word"
    [InlineData("module", "async function f() { (...await) => {} }", EcmaVersion.Latest, "Unexpected token ')'")] // V8 reports "Unexpected reserved word"
    [InlineData("script", "async function f() { ([await]) => {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { ([await]) => {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { ([x = await]) => {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { ([x = await]) => {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { ([...await]) => {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { ([...await]) => {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { ({await}) => {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { ({await}) => {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { ({x: await}) => {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({x: await}) => {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { ({x = await}) => {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({x = await}) => {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { ({...await}) => {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({...await}) => {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { ([{await}]) => {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { ([{await}]) => {} }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async function f() { fn = async await => 1 }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { fn = async await => 1 }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { async (await) => {} }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("module", "async function f() { async (await) => {} }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("script", "async function f() { async (...await) => {} }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("module", "async function f() { async (...await) => {} }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("script", "async function f() { async ([await]) => {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { async ([await]) => {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { async ([x = await]) => {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { async ([x = await]) => {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { async ([...await]) => {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { async ([...await]) => {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { async ({await}) => {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { async ({await}) => {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { async ({x: await}) => {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { async ({x: await}) => {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { async ({x = await}) => {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { async ({x = await}) => {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { async ({...await}) => {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { async ({...await}) => {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { async ([{await}]) => {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { async ([{await}]) => {} }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "fn = async await => 1", EcmaVersion.Latest, "'await' is not a valid identifier name in an async function")]
    [InlineData("module", "fn = async await => 1", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async (await) => {}", EcmaVersion.Latest, "'await' is not a valid identifier name in an async function")]
    [InlineData("module", "async (await) => {}", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("script", "async (...await) => {}", EcmaVersion.Latest, "'await' is not a valid identifier name in an async function")]
    [InlineData("module", "async (...await) => {}", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("script", "async ([await]) => {}", EcmaVersion.Latest, "'await' is not a valid identifier name in an async function")]
    [InlineData("module", "async ([await]) => {}", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async ([x = await]) => {}", EcmaVersion.Latest, "'await' is not a valid identifier name in an async function")]
    [InlineData("module", "async ([x = await]) => {}", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async ([...await]) => {}", EcmaVersion.Latest, "'await' is not a valid identifier name in an async function")]
    [InlineData("module", "async ([...await]) => {}", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async ({await}) => {}", EcmaVersion.Latest, "'await' is not a valid identifier name in an async function")]
    [InlineData("module", "async ({await}) => {}", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async ({x: await}) => {}", EcmaVersion.Latest, "'await' is not a valid identifier name in an async function")]
    [InlineData("module", "async ({x: await}) => {}", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async ({x = await}) => {}", EcmaVersion.Latest, "'await' is not a valid identifier name in an async function")]
    [InlineData("module", "async ({x = await}) => {}", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async ({...await}) => {}", EcmaVersion.Latest, "'await' is not a valid identifier name in an async function")]
    [InlineData("module", "async ({...await}) => {}", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async ([{await}]) => {}", EcmaVersion.Latest, "'await' is not a valid identifier name in an async function")]
    [InlineData("module", "async ([{await}]) => {}", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async function f() { function await() {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { function await() {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (function await() {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (function await() {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (function (await) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (function (await) {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (function (...await) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (function (...await) {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (function ([await]) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (function ([await]) {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (function ([x = await]) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (function ([x = await]) {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (function ([...await]) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (function ([...await]) {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (function ({await}) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (function ({await}) {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (function ({x: await}) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (function ({x: await}) {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (function ({x = await}) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (function ({x = await}) {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (function ({...await}) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (function ({...await}) {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (function ([{await}]) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (function ([{await}]) {}) }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async function f() { async function await() {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { async function await() {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { fn = async function await() {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { fn = async function await() {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { fn = async function (await) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { fn = async function (await) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { fn = async function (...await) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { fn = async function (...await) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { fn = async function ([await]) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Illegal await-expression in formal parameters of async function"
    [InlineData("module", "async function f() { fn = async function ([await]) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Illegal await-expression in formal parameters of async function"
    [InlineData("script", "async function f() { fn = async function ([x = await]) {} }", EcmaVersion.Latest, "Unexpected token ']'")] // V8 reports "Illegal await-expression in formal parameters of async function"
    [InlineData("module", "async function f() { fn = async function ([x = await]) {} }", EcmaVersion.Latest, "Unexpected token ']'")] // V8 reports "Illegal await-expression in formal parameters of async function"
    [InlineData("script", "async function f() { fn = async function ([...await]) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Illegal await-expression in formal parameters of async function"
    [InlineData("module", "async function f() { fn = async function ([...await]) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Illegal await-expression in formal parameters of async function"
    [InlineData("script", "async function f() { fn = async function ({await}) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { fn = async function ({await}) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { fn = async function ({x: await}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Illegal await-expression in formal parameters of async function"
    [InlineData("module", "async function f() { fn = async function ({x: await}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Illegal await-expression in formal parameters of async function"
    [InlineData("script", "async function f() { fn = async function ({x = await}) {} }", EcmaVersion.Latest, "Unexpected token '}'")] // V8 reports "Illegal await-expression in formal parameters of async function"
    [InlineData("module", "async function f() { fn = async function ({x = await}) {} }", EcmaVersion.Latest, "Unexpected token '}'")] // V8 reports "Illegal await-expression in formal parameters of async function"
    [InlineData("script", "async function f() { fn = async function ({...await}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Illegal await-expression in formal parameters of async function"
    [InlineData("module", "async function f() { fn = async function ({...await}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Illegal await-expression in formal parameters of async function"
    [InlineData("script", "async function f() { fn = async function ([{await}]) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { fn = async function ([{await}]) {} }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async function f() { class await {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { class await {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (class await {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { (class await {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (class { await = 0 }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (class { await = 0 }) }", EcmaVersion.Latest, null)]
    [InlineData("script", "async function f() { (class { x = await }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (class { x = await }) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (class { await() {} }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (class { await() {} }) }", EcmaVersion.Latest, null)]
    [InlineData("script", "async function f() { (class { m(await) {} }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (class { m(await) {} }) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (class { m(...await) {} }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { (class { m(...await) {} }) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { (class { m({m({x: [await]}) {} }) }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "async function f() { (class { m({m({x: [await]}) {} }) }", EcmaVersion.Latest, "Invalid destructuring assignment target")]

    [InlineData("script", "async function f() { ({await: 0}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { ({await: 0}) }", EcmaVersion.Latest, null)]
    [InlineData("script", "async function f() { ({x: await}) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({x: await}) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "function f() { ({x: await}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function f() { ({x: await}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { ({await() {} }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { ({await() {} }) }", EcmaVersion.Latest, null)]
    [InlineData("script", "async function f() { ({m(await) {} }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { ({m(await) {} }) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { ({m(...await) {} }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { ({m(...await) {} }) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { ({m({x: [await]}) {} }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "async function f() { ({m({x: [await]}) {} }) }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async function f() { try {} catch (await) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { try {} catch (await) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { try {} catch (...await) {} }", EcmaVersion.Latest, "Unexpected token '...'")]
    [InlineData("module", "async function f() { try {} catch (...await) {} }", EcmaVersion.Latest, "Unexpected token '...'")]
    [InlineData("script", "async function f() { try {} catch ([await]) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token ']'"
    [InlineData("module", "async function f() { try {} catch ([await]) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token ']'"
    [InlineData("script", "async function f() { try {} catch ([x = await]) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { try {} catch ([x = await]) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { try {} catch ([...await]) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token ']'"
    [InlineData("module", "async function f() { try {} catch ([...await]) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token ']'"
    [InlineData("script", "async function f() { try {} catch ({await}) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { try {} catch ({await}) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { try {} catch ({x: await}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token '}'"
    [InlineData("module", "async function f() { try {} catch ({x: await}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token '}'"
    [InlineData("script", "async function f() { try {} catch ({x = await}) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { try {} catch ({x = await}) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { try {} catch ({...await}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token '}'"
    [InlineData("module", "async function f() { try {} catch ({...await}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")] // V8 reports "Unexpected token '}'"
    [InlineData("script", "async function f() { try {} catch ([{await}]) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { try {} catch ([{await}]) {} }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async function f() { await: { break await } }", EcmaVersion.Latest, "Unexpected token ':'")]
    [InlineData("module", "async function f() { await: { break await } }", EcmaVersion.Latest, "Unexpected token ':'")]
    [InlineData("script", "async function f() { { break await } }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { { break await } }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "function* g() { var yield = 0 }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { var yield = 0 }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { var [yield] = [] }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { var [yield] = [] }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { var [x = yield] = [] }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { var [x = yield] = [] }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { var [...yield] = [] }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { var [...yield] = [] }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { var {yield} = {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { var {yield} = {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { var {x: yield} = {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { var {x: yield} = {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { var {x = yield} = {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { var {x = yield} = {} }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { var {...yield} = {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { var {...yield} = {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { var [{yield}] = [] }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { var [{yield}] = [] }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]

    [InlineData("script", "function* g() { fn = yield => 1 }", EcmaVersion.Latest, "Unexpected token '=>'")]
    [InlineData("module", "function* g() { fn = yield => 1 }", EcmaVersion.Latest, "Unexpected token '=>'")]
    [InlineData("script", "function* g() { (yield) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("module", "function* g() { (yield) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("script", "function* g() { (...yield) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")] // V8 reports "Unexpected identifier 'yield'"
    [InlineData("module", "function* g() { (...yield) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")] // V8 reports "Unexpected strict mode reserved word"
    [InlineData("script", "function* g() { ([yield]) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("module", "function* g() { ([yield]) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("script", "function* g() { ([x = yield]) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("module", "function* g() { ([x = yield]) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("script", "function* g() { ([...yield]) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("module", "function* g() { ([...yield]) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("script", "function* g() { ({yield}) => {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { ({yield}) => {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { ({x: yield}) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("module", "function* g() { ({x: yield}) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("script", "function* g() { ({x = yield}) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("module", "function* g() { ({x = yield}) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("script", "function* g() { ({...yield}) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")] // V8 reports "`...` must be followed by an identifier in declaration contexts"
    [InlineData("module", "function* g() { ({...yield}) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")] // V8 reports "`...` must be followed by an identifier in declaration contexts"
    [InlineData("script", "function* g() { ([{yield}]) => {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { ([{yield}]) => {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]

    [InlineData("script", "function* g() { fn = async yield => 1 }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { fn = async yield => 1 }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { async (yield) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("module", "function* g() { async (yield) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("script", "function* g() { async (...yield) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("module", "function* g() { async (...yield) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("script", "function* g() { async ([yield]) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("module", "function* g() { async ([yield]) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("script", "function* g() { async ([x = yield]) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("module", "function* g() { async ([x = yield]) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("script", "function* g() { async ([...yield]) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("module", "function* g() { async ([...yield]) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("script", "function* g() { async ({yield}) => {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { async ({yield}) => {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { async ({x: yield}) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("module", "function* g() { async ({x: yield}) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("script", "function* g() { async ({x = yield}) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("module", "function* g() { async ({x = yield}) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")]
    [InlineData("script", "function* g() { async ({...yield}) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")] // V8 reports "`...` must be followed by an identifier in declaration contexts"
    [InlineData("module", "function* g() { async ({...yield}) => {} }", EcmaVersion.Latest, "Yield expression not allowed in formal parameter")] // V8 reports "`...` must be followed by an identifier in declaration contexts"
    [InlineData("script", "function* g() { async ([{yield}]) => {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { async ([{yield}]) => {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]

    [InlineData("script", "function* g() { function yield() {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { function yield() {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (function yield() {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (function yield() {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (function (yield) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (function (yield) {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (function (...yield) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (function (...yield) {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (function ([yield]) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (function ([yield]) {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (function ([x = yield]) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (function ([x = yield]) {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (function ([...yield]) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (function ([...yield]) {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (function ({yield}) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (function ({yield}) {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (function ({x: yield}) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (function ({x: yield}) {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (function ({x = yield}) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (function ({x = yield}) {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (function ({...yield}) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (function ({...yield}) {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (function ([{yield}]) {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (function ([{yield}]) {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]

    [InlineData("script", "function* g() { async function yield() {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { async function yield() {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { fn = async function yield() {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { fn = async function yield() {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { fn = async function (yield) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { fn = async function (yield) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { fn = async function (...yield) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { fn = async function (...yield) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { fn = async function ([yield]) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { fn = async function ([yield]) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { fn = async function ([x = yield]) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { fn = async function ([x = yield]) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { fn = async function ([...yield]) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { fn = async function ([...yield]) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { fn = async function ({yield}) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { fn = async function ({yield}) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { fn = async function ({x: yield}) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { fn = async function ({x: yield}) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { fn = async function ({x = yield}) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { fn = async function ({x = yield}) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { fn = async function ({...yield}) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { fn = async function ({...yield}) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { fn = async function ([{yield}]) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { fn = async function ([{yield}]) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]

    [InlineData("script", "function* g() { class yield {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")] // V8 reports "Unexpected identifier 'yield'" (even though class id should be parsed in strict mode and yield is a strict mode identifier)
    [InlineData("module", "function* g() { class yield {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (class yield {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")] // V8 reports "Unexpected identifier 'yield'" (even though class id should be parsed in strict mode and yield is a strict mode identifier)
    [InlineData("module", "function* g() { (class yield {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (class { yield = 0 }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (class { yield = 0 }) }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { (class { x = yield }) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("module", "function* g() { (class { x = yield }) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (class { yield() {} }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { (class { yield() {} }) }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { (class { m(yield) {} }) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("module", "function* g() { (class { m(yield) {} }) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (class { m(...yield) {} }) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("module", "function* g() { (class { m(...yield) {} }) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { (class { m({m({x: [yield]}) {} }) }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { (class { m({m({x: [yield]}) {} }) }", EcmaVersion.Latest, "Invalid destructuring assignment target")]

    [InlineData("script", "function* g() { ({yield: 0}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { ({yield: 0}) }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { ({x: yield}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { ({x: yield}) }", EcmaVersion.Latest, null)]
    [InlineData("script", "function g() { ({x: yield}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function g() { ({x: yield}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { ({yield() {} }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { ({yield() {} }) }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { ({m(yield) {} }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { ({m(yield) {} }) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { ({m(...yield) {} }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { ({m(...yield) {} }) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { ({m({x: [yield]}) {} }) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { ({m({x: [yield]}) {} }) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]

    [InlineData("script", "function* g() { try {} catch (yield) {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { try {} catch (yield) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { try {} catch (...yield) {} }", EcmaVersion.Latest, "Unexpected token '...'")]
    [InlineData("module", "function* g() { try {} catch (...yield) {} }", EcmaVersion.Latest, "Unexpected token '...'")]
    [InlineData("script", "function* g() { try {} catch ([yield]) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { try {} catch ([yield]) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { try {} catch ([x = yield]) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { try {} catch ([x = yield]) {} }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { try {} catch ([...yield]) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { try {} catch ([...yield]) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { try {} catch ({yield}) {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { try {} catch ({yield}) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { try {} catch ({x: yield}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { try {} catch ({x: yield}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { try {} catch ({x = yield}) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { try {} catch ({x = yield}) {} }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { try {} catch ({...yield}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { try {} catch ({...yield}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { try {} catch ([{yield}]) {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { try {} catch ([{yield}]) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]

    [InlineData("script", "function* g() { yield: { break yield } }", EcmaVersion.Latest, "Unexpected token ':'")]
    [InlineData("module", "function* g() { yield: { break yield } }", EcmaVersion.Latest, "Unexpected token ':'")]
    [InlineData("script", "function* g() { { break yield } }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { { break yield } }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]

    [InlineData("script", "(...x,)=>a", EcmaVersion.Latest, "Rest parameter must be last formal parameter")]
    [InlineData("script", "([...x,])=>a", EcmaVersion.Latest, "Rest element must be last element")]
    [InlineData("script", "({...x,})=>a", EcmaVersion.Latest, "Rest element must be last element")]
    [InlineData("script", "async(...x,)=>a", EcmaVersion.Latest, "Rest parameter must be last formal parameter")]
    [InlineData("script", "async([...x,])=>a", EcmaVersion.Latest, "Rest element must be last element")]
    [InlineData("script", "async({...x,})=>a", EcmaVersion.Latest, "Rest element must be last element")]
    [InlineData("script", "function f(...x,){}", EcmaVersion.Latest, "Rest parameter must be last formal parameter")]
    [InlineData("script", "function f([...x,]){}", EcmaVersion.Latest, "Rest element must be last element")]
    [InlineData("script", "function f({...x,}){}", EcmaVersion.Latest, "Rest element must be last element")]
    [InlineData("script", "var[...x,]=[]", EcmaVersion.Latest, "Rest element must be last element")]
    [InlineData("script", "var{...x,}={}", EcmaVersion.Latest, "Rest element must be last element")]
    [InlineData("script", "try{}catch([...x,]){}", EcmaVersion.Latest, "Rest element must be last element")]
    [InlineData("script", "try{}catch({...x,}){}", EcmaVersion.Latest, "Rest element must be last element")]
    public void ShouldHandleVariableBindingEdgeCases(string sourceType, string input, EcmaVersion ecmaVersion, string? expectedError)
    {
        var parser = new Parser(new ParserOptions { EcmaVersion = ecmaVersion });
        var parseAction = GetParseActionFor(sourceType);

        if (expectedError is null)
        {
            Assert.NotNull(parseAction(parser, input));
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parseAction(parser, input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Theory]
    [InlineData("script", "async function f() { await = 0 }", EcmaVersion.Latest, "Unexpected token '='")]
    [InlineData("module", "async function f() { await = 0 }", EcmaVersion.Latest, "Unexpected token '='")]
    [InlineData("script", "async function f() { (await) = 0 }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("module", "async function f() { (await) = 0 }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("script", "async function f() { [await] = [] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { [await] = [] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { [x = await] = [] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { [x = await] = [] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { [...await] = [] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { [...await] = [] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { ({await} = {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { ({await} = {}) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { ({x: await} = {}) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({x: await} = {}) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { ({x = await} = {}) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({x = await} = {}) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { ({...await} = {}) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({...await} = {}) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { [{await}] = [] }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { [{await}] = [] }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async function f() { for (await in {}) {} }", EcmaVersion.Latest, "Unexpected token 'in'")]
    [InlineData("module", "async function f() { for (await in {}) {} }", EcmaVersion.Latest, "Unexpected token 'in'")]
    [InlineData("script", "async function f() { for ((await) in {}) {} }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("module", "async function f() { for ((await) in {}) {} }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("script", "async function f() { for ([await] in {}) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { for ([await] in {}) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { for ([x = await] in {}) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { for ([x = await] in {}) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { for ([...await] in {}) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { for ([...await] in {}) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { for ({await} in {})) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { for ({await} in {})) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { for ({x: await} in {}) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { for ({x: await} in {}) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { for ({x = await} in {}) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { for ({x = await} in {}) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { for ({...await} in {}) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { for ({...await} in {}) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { for ([{await}] in {}) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { for ([{await}] in {}) {} }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async function f() { for (await of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { for (await of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { for ((await) of []) {} }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("module", "async function f() { for ((await) of []) {} }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("script", "async function f() { for ([await] of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { for ([await] of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { for ([x = await] of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { for ([x = await] of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { for ([...await] of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { for ([...await] of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { for ({await} of [])) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { for ({await} of [])) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { for ({x: await} of []) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { for ({x: await} of []) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { for ({x = await} of []) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { for ({x = await} of []) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { for ({...await} of []) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { for ({...await} of []) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { for ([{await}] of []) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { for ([{await}] of []) {} }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async function f() { for await (await of []) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { for await (await of []) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { for await ((await) of []) {} }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("module", "async function f() { for await ((await) of []) {} }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("script", "async function f() { for await ([await] of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { for await ([await] of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { for await ([x = await] of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { for await ([x = await] of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { for await ([...await] of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { for await ([...await] of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { for await ({await} of [])) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { for await ({await} of [])) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { for await ({x: await} of []) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { for await ({x: await} of []) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { for await ({x = await} of []) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { for await ({x = await} of []) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { for await ({...await} of []) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { for await ({...await} of []) {} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { for await ([{await}] of []) {} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { for await ([{await}] of []) {} }", EcmaVersion.Latest, "Unexpected reserved word")]

    [InlineData("script", "async function f() { await += 1 }", EcmaVersion.Latest, "Unexpected token '+='")]
    [InlineData("module", "async function f() { await += 1 }", EcmaVersion.Latest, "Unexpected token '+='")]
    [InlineData("script", "async function f() { (await) += 1 }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("module", "async function f() { (await) += 1 }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("script", "async function f() { [await] += 1 }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { [await] += 1 }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { [x = await] += 1 }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { [x = await] += 1 }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { [...await] += 1 }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { [...await] += 1 }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { ({await} += 1) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { ({await} += 1) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { ({x: await} += 1) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({x: await} += 1) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { ({x = await} += 1) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({x = await} += 1) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { ({...await} += 1) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({...await} += 1) }", EcmaVersion.Latest, "Unexpected token '}'")]

    [InlineData("script", "async function f() { ++await }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ++await }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { ++(await) }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("module", "async function f() { ++(await) }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("script", "async function f() { ++[await] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { ++[await] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { ++[x = await] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { ++[x = await] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { ++[...await] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { ++[...await] }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { ++{await} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { ++{await} }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { ++{x: await} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ++{x: await} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { ++{x = await} }", EcmaVersion.Latest, "Unexpected token '='")] // V8 reports "Unexpected token '}'"
    [InlineData("module", "async function f() { ++{x = await} }", EcmaVersion.Latest, "Unexpected token '='")] // V8 reports "Unexpected token '}'"
    [InlineData("script", "async function f() { ++{...await} }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ++{...await} }", EcmaVersion.Latest, "Unexpected token '}'")]

    [InlineData("script", "async function f() { await++ }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { await++ }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { (await)++ }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("module", "async function f() { (await)++ }", EcmaVersion.Latest, "Unexpected token ')'")]
    [InlineData("script", "async function f() { [await]++ }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { [await]++ }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { [x = await]++ }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { [x = await]++ }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { [...await]++ }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "async function f() { [...await]++ }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "async function f() { ({await}++) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "async function f() { ({await}++) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("script", "async function f() { ({x: await}++) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({x: await}++) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { ({x = await}++) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({x = await}++) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "async function f() { ({...await}++) }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "async function f() { ({...await}++) }", EcmaVersion.Latest, "Unexpected token '}'")]

    [InlineData("script", "function* g() { yield = 0 }", EcmaVersion.Latest, "Unexpected token '='")]
    [InlineData("module", "function* g() { yield = 0 }", EcmaVersion.Latest, "Unexpected token '='")]
    [InlineData("script", "function* g() { (yield) = 0 }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("module", "function* g() { (yield) = 0 }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("script", "function* g() { [yield] = [] }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { [yield] = [] }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { [x = yield] = [] }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { [x = yield] = [] }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { [...yield] = [] }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { [...yield] = [] }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { ({yield} = {}) }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { ({yield} = {}) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { ({x: yield} = {}) }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { ({x: yield} = {}) }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { ({x = yield} = {}) }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { ({x = yield} = {}) }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { ({...yield} = {}) }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { ({...yield} = {}) }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { [{yield}] = [] }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { [{yield}] = [] }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]

    [InlineData("script", "function* g() { for (yield in {}) {} }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "Invalid left-hand side in assignment"
    [InlineData("module", "function* g() { for (yield in {}) {} }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "Invalid left-hand side in assignment"
    [InlineData("script", "function* g() { for ((yield) in {}) {} }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "Invalid left-hand side in assignment"
    [InlineData("module", "function* g() { for ((yield) in {}) {} }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "Invalid left-hand side in assignment"
    [InlineData("script", "function* g() { for ([yield] in {}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { for ([yield] in {}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { for ([x = yield] in {}) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { for ([x = yield] in {}) {} }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { for ([...yield] in {}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { for ([...yield] in {}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { for ({yield} in {})) {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { for ({yield} in {})) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { for ({x: yield} in {}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { for ({x: yield} in {}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { for ({x = yield} in {}) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { for ({x = yield} in {}) {} }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { for ({...yield} in {}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { for ({...yield} in {}) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { for ([{yield}] in {}) {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { for ([{yield}] in {}) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]

    [InlineData("script", "function* g() { for (yield of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("module", "function* g() { for (yield of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")]
    [InlineData("script", "function* g() { for ((yield) of []) {} }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "Invalid left-hand side of assignment"
    [InlineData("module", "function* g() { for ((yield) of []) {} }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "Invalid left-hand side of assignment"
    [InlineData("script", "function* g() { for ([yield] of []) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { for ([yield] of []) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { for ([x = yield] of []) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { for ([x = yield] of []) {} }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { for ([...yield] of []) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { for ([...yield] of []) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { for ({yield} of [])) {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { for ({yield} of [])) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { for ({x: yield} of []) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { for ({x: yield} of []) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { for ({x = yield} of []) {} }", EcmaVersion.Latest, null)]
    [InlineData("module", "function* g() { for ({x = yield} of []) {} }", EcmaVersion.Latest, null)]
    [InlineData("script", "function* g() { for ({...yield} of []) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("module", "function* g() { for ({...yield} of []) {} }", EcmaVersion.Latest, "Invalid destructuring assignment target")]
    [InlineData("script", "function* g() { for ([{yield}] of []) {} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { for ([{yield}] of []) {} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]

    [InlineData("script", "function* g() { yield += 1 }", EcmaVersion.Latest, "Unexpected token '+='")]
    [InlineData("module", "function* g() { yield += 1 }", EcmaVersion.Latest, "Unexpected token '+='")]
    [InlineData("script", "function* g() { (yield) += 1 }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("module", "function* g() { (yield) += 1 }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("script", "function* g() { [yield] += 1 }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("module", "function* g() { [yield] += 1 }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("script", "function* g() { [x = yield] += 1 }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("module", "function* g() { [x = yield] += 1 }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("script", "function* g() { [...yield] += 1 }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("module", "function* g() { [...yield] += 1 }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("script", "function* g() { ({yield} += 1) }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { ({yield} += 1) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { ({x: yield} += 1) }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("module", "function* g() { ({x: yield} += 1) }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("script", "function* g() { ({x = yield} += 1) }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("module", "function* g() { ({x = yield} += 1) }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("script", "function* g() { ({...yield} += 1) }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]
    [InlineData("module", "function* g() { ({...yield} += 1) }", EcmaVersion.Latest, "Invalid left-hand side in assignment")]

    [InlineData("script", "function* g() { ++yield }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { ++yield }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { ++(yield) }", EcmaVersion.Latest, "Invalid left-hand side expression in prefix operation")]
    [InlineData("module", "function* g() { ++(yield) }", EcmaVersion.Latest, "Invalid left-hand side expression in prefix operation")]
    [InlineData("script", "function* g() { ++[yield] }", EcmaVersion.Latest, "Invalid left-hand side expression in prefix operation")]
    [InlineData("module", "function* g() { ++[yield] }", EcmaVersion.Latest, "Invalid left-hand side expression in prefix operation")]
    [InlineData("script", "function* g() { ++[x = yield] }", EcmaVersion.Latest, "Invalid left-hand side expression in prefix operation")]
    [InlineData("module", "function* g() { ++[x = yield] }", EcmaVersion.Latest, "Invalid left-hand side expression in prefix operation")]
    [InlineData("script", "function* g() { ++[...yield] }", EcmaVersion.Latest, "Invalid left-hand side expression in prefix operation")]
    [InlineData("module", "function* g() { ++[...yield] }", EcmaVersion.Latest, "Invalid left-hand side expression in prefix operation")]
    [InlineData("script", "function* g() { ++{yield} }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { ++{yield} }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { ++{x: yield} }", EcmaVersion.Latest, "Invalid left-hand side expression in prefix operation")]
    [InlineData("module", "function* g() { ++{x: yield} }", EcmaVersion.Latest, "Invalid left-hand side expression in prefix operation")]
    [InlineData("script", "function* g() { ++{x = yield} }", EcmaVersion.Latest, "Unexpected token '='")] // V8 reports "Invalid left-hand side expression in prefix operation"
    [InlineData("module", "function* g() { ++{x = yield} }", EcmaVersion.Latest, "Unexpected token '='")] // V8 reports "Invalid left-hand side expression in prefix operation"
    [InlineData("script", "function* g() { ++{...yield} }", EcmaVersion.Latest, "Invalid left-hand side expression in prefix operation")]
    [InlineData("module", "function* g() { ++{...yield} }", EcmaVersion.Latest, "Invalid left-hand side expression in prefix operation")]

    [InlineData("script", "function* g() { yield++ }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("module", "function* g() { yield++ }", EcmaVersion.Latest, "Unexpected token '}'")]
    [InlineData("script", "function* g() { (yield)++ }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("module", "function* g() { (yield)++ }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("script", "function* g() { [yield]++ }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("module", "function* g() { [yield]++ }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("script", "function* g() { [x = yield]++ }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("module", "function* g() { [x = yield]++ }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("script", "function* g() { [...yield]++ }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("module", "function* g() { [...yield]++ }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("script", "function* g() { ({yield}++) }", EcmaVersion.Latest, "Unexpected identifier 'yield'")]
    [InlineData("module", "function* g() { ({yield}++) }", EcmaVersion.Latest, "Unexpected strict mode reserved word")]
    [InlineData("script", "function* g() { ({x: yield}++) }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("module", "function* g() { ({x: yield}++) }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("script", "function* g() { ({x = yield}++) }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("module", "function* g() { ({x = yield}++) }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("script", "function* g() { ({x = yield}\n++) }", EcmaVersion.Latest, "Invalid shorthand property initializer")]
    [InlineData("module", "function* g() { ({x = yield}\n++) }", EcmaVersion.Latest, "Invalid shorthand property initializer")]
    [InlineData("script", "function* g() { ({...yield}++) }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("module", "function* g() { ({...yield}++) }", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]

    [InlineData("script", "(...x,)=a", EcmaVersion.Latest, "Unexpected token '...'")] // V8 reports "Rest parameter must be last formal parameter"
    [InlineData("script", "[...x,]=a", EcmaVersion.Latest, "Rest element must be last element")]
    [InlineData("script", "{...x,}=a", EcmaVersion.Latest, "Unexpected token '...'")] // V8 reports "Rest parameter must be last formal parameter"
    [InlineData("script", "({...x,}=a)", EcmaVersion.Latest, "Rest element must be last element")]

    [InlineData("script", "({__proto__: x, __proto__: y}++)", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("module", "({__proto__: x, __proto__: y}++)", EcmaVersion.Latest, "Invalid left-hand side expression in postfix operation")]
    [InlineData("script", "({__proto__: x, __proto__: y}\n++)", EcmaVersion.Latest, "Duplicate __proto__ fields are not allowed in object literals")]
    [InlineData("module", "({__proto__: x, __proto__: y}\n++)", EcmaVersion.Latest, "Duplicate __proto__ fields are not allowed in object literals")]
    public void ShouldHandleVariableAssignmentEdgeCases(string sourceType, string input, EcmaVersion ecmaVersion, string? expectedError)
    {
        var parser = new Parser(new ParserOptions { EcmaVersion = ecmaVersion });
        var parseAction = GetParseActionFor(sourceType);

        if (expectedError is null)
        {
            Assert.NotNull(parseAction(parser, input));
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parseAction(parser, input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Theory]
    [InlineData("script", "for (async of [1]) { console.log(async) }", EcmaVersion.ES7, null)]
    [InlineData("module", "for (async of [1]) { console.log(async) }", EcmaVersion.ES7, null)]
    [InlineData("script", "for (async of [1]) { console.log(async) }", EcmaVersion.ES8, "The left-hand side of a for-of loop may not be 'async'")]
    [InlineData("module", "for (async of [1]) { console.log(async) }", EcmaVersion.ES8, "The left-hand side of a for-of loop may not be 'async'")]
    [InlineData("script", "for (async of [1]) { console.log(async) }", EcmaVersion.Latest, "The left-hand side of a for-of loop may not be 'async'")]
    [InlineData("module", "for (async of [1]) { console.log(async) }", EcmaVersion.Latest, "The left-hand side of a for-of loop may not be 'async'")]
    [InlineData("script", "for (async\nof [1]) { console.log(async) }", EcmaVersion.ES7, null)]
    [InlineData("module", "for (async\nof [1]) { console.log(async) }", EcmaVersion.ES7, null)]
    [InlineData("script", "for (async\nof [1]) { console.log(async) }", EcmaVersion.ES8, "The left-hand side of a for-of loop may not be 'async'")]
    [InlineData("module", "for (async\nof [1]) { console.log(async) }", EcmaVersion.ES8, "The left-hand side of a for-of loop may not be 'async'")]
    [InlineData("script", "for (async\nof [1]) { console.log(async) }", EcmaVersion.Latest, "The left-hand side of a for-of loop may not be 'async'")]
    [InlineData("module", "for (async\nof [1]) { console.log(async) }", EcmaVersion.Latest, "The left-hand side of a for-of loop may not be 'async'")]
    [InlineData("script", "for (async of\n[1]) { console.log(async) }", EcmaVersion.ES7, null)]
    [InlineData("module", "for (async of\n[1]) { console.log(async) }", EcmaVersion.ES7, null)]
    [InlineData("script", "for (async of\n[1]) { console.log(async) }", EcmaVersion.ES8, "The left-hand side of a for-of loop may not be 'async'")]
    [InlineData("module", "for (async of\n[1]) { console.log(async) }", EcmaVersion.ES8, "The left-hand side of a for-of loop may not be 'async'")]
    [InlineData("script", "for (async of\n[1]) { console.log(async) }", EcmaVersion.Latest, "The left-hand side of a for-of loop may not be 'async'")]
    [InlineData("module", "for (async of\n[1]) { console.log(async) }", EcmaVersion.Latest, "The left-hand side of a for-of loop may not be 'async'")]

    [InlineData("script", "for await (async of [1]) { console.log(async) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "for await (async of [1]) { console.log(async) }", EcmaVersion.Latest, null)]
    [InlineData("script", "async () => { for await (async of [1]) { console.log(async) } }", EcmaVersion.Latest, null)]
    [InlineData("script", "for await (async\nof [1]) { console.log(async) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "for await (async\nof [1]) { console.log(async) }", EcmaVersion.Latest, null)]
    [InlineData("script", "async () => { for await (async\nof [1]) { console.log(async) } }", EcmaVersion.Latest, null)]
    [InlineData("script", "for await (async of\n[1]) { console.log(async) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "for await (async of\n[1]) { console.log(async) }", EcmaVersion.Latest, null)]
    [InlineData("script", "async () => { for await (async of\n[1]) { console.log(async) } }", EcmaVersion.Latest, null)]

    [InlineData("script", "for (x = async of [1]) { console.log(async) }", EcmaVersion.ES7, "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (x = async of [1]) { console.log(async) }", EcmaVersion.ES7, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (x = async of [1]) { console.log(async) }", EcmaVersion.ES8, "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (x = async of [1]) { console.log(async) }", EcmaVersion.ES8, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (x = async of [1]) { console.log(async) }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "The left-hand side of a for-of loop may not be 'async'."
    [InlineData("module", "for (x = async of [1]) { console.log(async) }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "The left-hand side of a for-of loop may not be 'async'."
    [InlineData("script", "for (x = async\nof [1]) { console.log(async) }", EcmaVersion.ES7, "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (x = async\nof [1]) { console.log(async) }", EcmaVersion.ES7, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (x = async\nof [1]) { console.log(async) }", EcmaVersion.ES8, "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (x = async\nof [1]) { console.log(async) }", EcmaVersion.ES8, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (x = async\nof [1]) { console.log(async) }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "The left-hand side of a for-of loop may not be 'async'."
    [InlineData("module", "for (x = async\nof [1]) { console.log(async) }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "The left-hand side of a for-of loop may not be 'async'."
    [InlineData("script", "for (x = async of\n[1]) { console.log(async) }", EcmaVersion.ES7, "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (x = async of\n[1]) { console.log(async) }", EcmaVersion.ES7, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (x = async of\n[1]) { console.log(async) }", EcmaVersion.ES8, "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (x = async of\n[1]) { console.log(async) }", EcmaVersion.ES8, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (x = async of\n[1]) { console.log(async) }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "The left-hand side of a for-of loop may not be 'async'."
    [InlineData("module", "for (x = async of\n[1]) { console.log(async) }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "The left-hand side of a for-of loop may not be 'async'."

    [InlineData("script", "for await (x = async of [1]) { console.log(async) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "for await (x = async of [1]) { console.log(async) }", EcmaVersion.Latest, "Unexpected token '='")]
    [InlineData("script", "async () => { for await (x = async of [1]) { console.log(async) } }", EcmaVersion.Latest, "Unexpected token '='")]
    [InlineData("script", "for await (x = async\nof [1]) { console.log(async) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "for await (x = async\nof [1]) { console.log(async) }", EcmaVersion.Latest, "Unexpected token '='")]
    [InlineData("script", "async () => { for await (x = async\nof [1]) { console.log(async) } }", EcmaVersion.Latest, "Unexpected token '='")]
    [InlineData("script", "for await (x = async of\n[1]) { console.log(async) }", EcmaVersion.Latest, "Unexpected reserved word")]
    [InlineData("module", "for await (x = async of\n[1]) { console.log(async) }", EcmaVersion.Latest, "Unexpected token '='")]
    [InlineData("script", "async () => { for await (x = async of\n[1]) { console.log(async) } }", EcmaVersion.Latest, "Unexpected token '='")]

    [InlineData("script", "for (x, async of [1]) { console.log(async) }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "The left-hand side of a for-of loop may not be 'async'."
    [InlineData("module", "for (x, async of [1]) { console.log(async) }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "The left-hand side of a for-of loop may not be 'async'."

    [InlineData("script", "for (x, y = async of [1]) { console.log(async) }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "The left-hand side of a for-of loop may not be 'async'."
    [InlineData("module", "for (x, y = async of [1]) { console.log(async) }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")] // V8 reports "The left-hand side of a for-of loop may not be 'async'."

    [InlineData("script", "async () => { for await (x, async of [1]) { console.log(async) } }", EcmaVersion.Latest, "Unexpected token ','")]
    [InlineData("module", "async () => { for await (x, async of [1]) { console.log(async) } }", EcmaVersion.Latest, "Unexpected token ','")]

    [InlineData("script", "async () => { for await (x, y = async of [1]) { console.log(async) } }", EcmaVersion.Latest, "Unexpected token ','")]
    [InlineData("module", "async () => { for await (x, y = async of [1]) { console.log(async) } }", EcmaVersion.Latest, "Unexpected token ','")]

    [InlineData("script", "for (x ? async of => {} : y of [1]) { console.log(async) }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (x ? async of => {} : y of [1]) { console.log(async) }", EcmaVersion.Latest, "Invalid left-hand side in for-loop")]

    [InlineData("script", "async () => { for await (x ? async of => {} : y of [1]) { console.log(async) } }", EcmaVersion.Latest, "Unexpected token '?'")]
    [InlineData("module", "async () => { for await (x ? async of => {} : y of [1]) { console.log(async) } }", EcmaVersion.Latest, "Unexpected token '?'")]

    [InlineData("script", "async () => { for ((async) of [1]) { console.log(async) } }", EcmaVersion.Latest, null)]
    [InlineData("module", "async () => { for ((async) of [1]) { console.log(async) } }", EcmaVersion.Latest, null)]
    [InlineData("script", "async () => { for ((async)\nof [1]) { console.log(async) } }", EcmaVersion.Latest, null)]
    [InlineData("module", "async () => { for ((async)\nof [1]) { console.log(async) } }", EcmaVersion.Latest, null)]
    [InlineData("script", "async () => { for ((async) of\n[1]) { console.log(async) } }", EcmaVersion.Latest, null)]
    [InlineData("module", "async () => { for ((async) of\n[1]) { console.log(async) } }", EcmaVersion.Latest, null)]
    public void ShouldHandleAsyncOfAmbiguityInForLoop(string sourceType, string input, EcmaVersion ecmaVersion, string? expectedError)
    {
        // See also: https://github.com/tc39/ecma262/issues/2034

        var parser = new Parser(new ParserOptions { EcmaVersion = ecmaVersion });
        var parseAction = GetParseActionFor(sourceType);

        if (expectedError is null)
        {
            Assert.NotNull(parseAction(parser, input));
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parseAction(parser, input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Theory]
    [InlineData("script", "using x = resource", false, false, "Unexpected identifier 'x'")]
    [InlineData("module", "using x = resource", false, false, null)]
    [InlineData("script", "using x = resource", true, false, null)]
    [InlineData("module", "using x = resource", true, false, null)]
    [InlineData("script", "await using x = resource", false, false, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("module", "await using x = resource", false, false, null)]
    [InlineData("script", "await using x = resource", false, true, "Unexpected identifier 'x'")]
    [InlineData("script", "await using x = resource", true, false, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("module", "await using x = resource", true, false, null)]
    [InlineData("script", "await using x = resource", true, true, null)]

    [InlineData("script", "switch (0) { case 0: using x = resource }", false, false, "Unexpected identifier 'x'")]
    [InlineData("module", "switch (0) { case 0: using x = resource }", false, false, "Unexpected identifier 'x'")]
    [InlineData("script", "switch (0) { case 0: using x = resource }", true, false, "Unexpected identifier 'x'")]
    [InlineData("module", "switch (0) { case 0: using x = resource }", true, false, "Unexpected identifier 'x'")]
    [InlineData("script", "switch (0) { case 0: await using x = resource }", false, false, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("module", "switch (0) { case 0: await using x = resource }", false, false, "Unexpected identifier 'x'")]
    [InlineData("script", "switch (0) { case 0: await using x = resource }", false, true, "Unexpected identifier 'x'")]
    [InlineData("script", "switch (0) { case 0: await using x = resource }", true, false, "await is only valid in async functions and the top level bodies of modules")]
    [InlineData("module", "switch (0) { case 0: await using x = resource }", true, false, "Unexpected identifier 'x'")]
    [InlineData("script", "switch (0) { case 0: await using x = resource }", true, true, "Unexpected identifier 'x'")]

    [InlineData("module", "for (using", false, false, "Unexpected end of input")]
    [InlineData("script", "for (using", false, false, "Unexpected end of input")]
    [InlineData("module", "for (using of =) {}", false, false, "Unexpected token ')'")]
    [InlineData("script", "for (using of =) {}", false, false, "Unexpected token ')'")]
    [InlineData("module", "for (using of = x;;) {}", false, false, null)]
    [InlineData("script", "for (using of = x;;) {}", false, false, null)]

    [InlineData("module", "for (await using", false, false, "Unexpected end of input")]
    [InlineData("script", "for (await using", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("module", "for (await using of =) {}", false, false, "Unexpected token ')'")]
    [InlineData("script", "for (await using of =) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("module", "for (await using of = x;;) {}", false, false, null)]
    [InlineData("script", "for (await using of = x;;) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"

    [InlineData("module", "for (using of = of of of) {}", false, false, "for-of loop variable declaration may not have an initializer")]
    [InlineData("script", "for (using of = of of of) {}", false, false, "for-of loop variable declaration may not have an initializer")]
    [InlineData("module", "for (using of =/**/of of of) {}", false, false, "for-of loop variable declaration may not have an initializer")]
    [InlineData("script", "for (using of =/**/of of of) {}", false, false, "for-of loop variable declaration may not have an initializer")]
    [InlineData("module", "for (using of == of of of) {}", false, false, "Unexpected token '=='")]
    [InlineData("script", "for (using of == of of of) {}", false, false, "Unexpected token '=='")]
    [InlineData("module", "for (using of => of of of) {}", false, false, "Unexpected token '=>'")]
    [InlineData("script", "for (using of => of of of) {}", false, false, "Unexpected token '=>'")]

    [InlineData("module", "for (using in =) {}", false, false, "Unexpected token '='")]
    [InlineData("script", "for (using in =) {}", false, false, "Unexpected token '='")]
    [InlineData("module", "for (using in = x;;) {}", false, false, "Unexpected token '='")]
    [InlineData("script", "for (using in = x;;) {}", false, false, "Unexpected token '='")]

    [InlineData("module", "for (await using in =) {}", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (await using in =) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("module", "for (await using in = x;;) {}", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (await using in = x;;) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"

    [InlineData("module", "for (using of = of in of) {}", false, false, "for-in loop variable declaration may not have an initializer")]
    [InlineData("script", "for (using of = of in of) {}", false, false, "for-in loop variable declaration may not have an initializer")]
    [InlineData("module", "for (using of =/**/of in of) {}", false, false, "for-in loop variable declaration may not have an initializer")]
    [InlineData("script", "for (using of =/**/of in of) {}", false, false, "for-in loop variable declaration may not have an initializer")]
    [InlineData("module", "for (using of == of in of) {}", false, false, "Unexpected token '=='")]
    [InlineData("script", "for (using of == of in of) {}", false, false, "Unexpected token '=='")]
    [InlineData("module", "for (using of => of in of) {}", false, false, "Unexpected token '=>'")]
    [InlineData("script", "for (using of => of in of) {}", false, false, "Unexpected token '=>'")]
    [InlineData("module", "for (await using of = of in of) {}", false, false, "for-in loop variable declaration may not have an initializer")]
    [InlineData("script", "for (await using of = of in of) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"

    [InlineData("module", "for (using of of []) {}", false, false, "Unexpected token ']'")]
    [InlineData("script", "for (using of of []) {}", false, false, "Unexpected token ']'")]
    [InlineData("module", "for (await using of of []) {}", false, false, null)]
    [InlineData("script", "for (await using of of []) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("script", "async () => { for (await using of of []) {} }", false, false, null)]

    [InlineData("module", "for (using of x) {}", false, false, null)]
    [InlineData("script", "for (using of x) {}", false, false, null)]
    [InlineData("module", "for (using\nof x) {}", false, false, null)]
    [InlineData("script", "for (using\nof x) {}", false, false, null)]

    [InlineData("module", "for (using in x) {}", false, false, null)]
    [InlineData("script", "for (using in x) {}", false, false, null)]
    [InlineData("module", "for (using\nin x) {}", false, false, null)]
    [InlineData("script", "for (using\nin x) {}", false, false, null)]

    [InlineData("module", "for (using instanceof x) {}", false, false, "Unexpected token ')'")]
    [InlineData("script", "for (using instanceof x) {}", false, false, "Unexpected token ')'")]
    [InlineData("module", "for (using\ninstanceof x) {}", false, false, "Unexpected token ')'")]
    [InlineData("script", "for (using\ninstanceof x) {}", false, false, "Unexpected token ')'")]

    [InlineData("module", "for (await using of x) {}", false, false, "Missing initializer in await using declaration")]
    [InlineData("script", "for (await using of x) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("script", "async () => { for (await using of x) {} }", false, false, "Missing initializer in await using declaration")]
    [InlineData("module", "for (await\nusing of x) {}", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (await\nusing of x) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("script", "async () => { for (await\nusing of x) {} }", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (await using\nof x) {}", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (await using\nof x) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("script", "async () => { for (await using\nof x) {} }", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (await\nusing\nof x) {}", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (await\nusing\nof x) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("script", "async () => { for (await\nusing\nof x) {} }", false, false, "Invalid left-hand side in for-loop")]

    [InlineData("module", "for (await using in x) {}", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (await using in x) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("script", "async () => { for (await using in x) {} }", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (await\nusing in x) {}", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (await\nusing in x) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("script", "async () => { for (await\nusing in x) {} }", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (await using\nin x) {}", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (await using\nin x) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("script", "async () => { for (await using\nin x) {} }", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (await\nusing\nin x) {}", false, false, "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (await\nusing\nin x) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("script", "async () => { for (await\nusing\nin x) {} }", false, false, "Invalid left-hand side in for-loop")]

    [InlineData("module", "for (await using instanceof x) {}", false, false, "Unexpected token ')'")]
    [InlineData("script", "for (await using instanceof x) {}", false, false, "Unexpected identifier 'using'")] // V8 reports "Unexpected token 'using'"
    [InlineData("script", "async () => { for (await using instanceof x) {} }", false, false, "Unexpected token ')'")]
    public void ShouldHandleUsingEdgeCases(string sourceType, string input, bool allowTopLevelUsing, bool allowAwaitOutsideFunction, string? expectedError)
    {
        var parser = new Parser(new ParserOptions
        {
            AllowAwaitOutsideFunction = allowAwaitOutsideFunction,
            AllowTopLevelUsing = allowTopLevelUsing,
        });
        var parseAction = GetParseActionFor(sourceType);

        if (expectedError is null)
        {
            Assert.NotNull(parseAction(parser, input));
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parseAction(parser, input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Fact]
    public void LabelSetShouldPointToStatement()
    {
        var parser = new Parser();
        var program = parser.ParseScript("here: Hello();");
        var labeledStatement = program.Body.First().As<LabeledStatement>();
        var body = labeledStatement.Body;

        Assert.Equal(labeledStatement.Label, body.LabelSet);
    }

    [Theory]
    [InlineData(1.189008226412092e+38, "0x5973772948c653ac1971f1576e03c4d4")]
    [InlineData(18446744073709552000d, "0xffffffffffffffff")]
    public void ShouldParseNumericLiterals(object expected, string source)
    {
        var parser = new Parser();
        var expression = parser.ParseExpression(source);

        var literal = expression as NumericLiteral;

        Assert.NotNull(literal);
        Assert.Equal(expected, literal.Value);
    }

    [Theory]
    [InlineData("export { Mercury as \"☿\" } from \"./export-expname_FIXTURE.js\";", NodeType.ExportNamedDeclaration, false, "Mercury", true, "☿")]
    [InlineData("export * as \"All\" from \"./export-expname_FIXTURE.js\";", NodeType.ExportAllDeclaration, false, null, true, "All")]
    [InlineData("export { \"☿\" as Ami } from \"./export-expname_FIXTURE.js\"", NodeType.ExportNamedDeclaration, true, "☿", false, "Ami")]
    [InlineData("import { \"☿\" as Ami } from \"./export-expname_FIXTURE.js\";", NodeType.ImportDeclaration, false, "Ami", true, "☿")]
    public void ShouldParseModuleImportExportWithStringLiterals(string source, NodeType nodeType,
        bool localIsLiteral, string? expectedLocalName, bool exportedIsLiteral, string? expectedExportedName)
    {
        var program = new Parser().ParseModule(source);

        string? actualLocalName, actualExportedName;
        switch (nodeType)
        {
            case NodeType.ExportNamedDeclaration:
                var namedDeclaratiopn = Assert.Single(program.DescendantNodes().OfType<ExportNamedDeclaration>());
                var exportSpecifier = Assert.Single(namedDeclaratiopn.Specifiers);
                actualLocalName = GetExportOrImportName(exportSpecifier.Local, localIsLiteral);
                actualExportedName = GetExportOrImportName(exportSpecifier.Exported, exportedIsLiteral);
                break;

            case NodeType.ExportAllDeclaration:
                var exportAllDeclaration = Assert.Single(program.DescendantNodes().OfType<ExportAllDeclaration>());
                actualLocalName = null;
                actualExportedName = exportAllDeclaration.Exported is not null ? GetExportOrImportName(exportAllDeclaration.Exported, exportedIsLiteral) : null;
                break;

            case NodeType.ImportDeclaration:
                var importDeclaration = Assert.Single(program.DescendantNodes().OfType<ImportDeclaration>());
                var importDeclarationSpecifier = Assert.Single(importDeclaration.Specifiers);
                (actualLocalName, actualExportedName) = importDeclarationSpecifier switch
                {
                    ImportSpecifier importSpecifier => (GetExportOrImportName(importSpecifier.Local, localIsLiteral), GetExportOrImportName(importSpecifier.Imported, exportedIsLiteral)),
                    _ => throw new InvalidOperationException(),
                };
                break;

            default:
                throw new InvalidOperationException();
        }

        Assert.Equal(expectedLocalName, actualLocalName);
        Assert.Equal(expectedExportedName, actualExportedName);

        static string GetExportOrImportName(Expression expression, bool isLiteral)
        {
            return isLiteral ? Assert.IsType<StringLiteral>(expression).Value : Assert.IsType<Identifier>(expression).Name;
        }
    }

    [Fact]
    public void ShouldParseClassInheritance()
    {
        var parser = new Parser();
        var program = parser.ParseScript("class Rectangle extends aggregation(Shape, Colored, ZCoord) { }");

        var classDeclaration = Assert.Single(program.DescendantNodes().OfType<ClassDeclaration>());
        Assert.IsType<CallExpression>(classDeclaration.SuperClass);
    }

    [Fact]
    public void ShouldParseClassStaticBlocks()
    {
        const string code =
            """
            class aa {
                static qq() {
                }
                static staticProperty1 = 'Property 1';
                static staticProperty2;
                static {
                    this.staticProperty2 = 'Property 2';
                }
                static staticProperty3;
                static {
                    this.staticProperty3 = 'Property 3';
                }
            }
            """;

        var program = new Parser().ParseScript(code);

        var classDeclaration = Assert.Single(program.DescendantNodes().OfType<ClassDeclaration>());
        var staticBlocks = program.DescendantNodes().OfType<StaticBlock>().ToArray();
        Assert.Equal(2, staticBlocks.Length);
        Assert.Distinct(staticBlocks);

        var staticBlocks2 = classDeclaration.DescendantNodes().OfType<StaticBlock>().ToArray();
        Assert.True(staticBlocks.SequenceEqualUnordered(staticBlocks2));
    }

    [Fact]
    public void ShouldSymbolPropertyKey()
    {
        var parser = new Parser();
        var program = parser.ParseScript("var a = { [Symbol.iterator]: undefined }");

        var property = Assert.Single(program.DescendantNodes().OfType<Property>());
        var objectProperty = Assert.Single(program.DescendantNodes().OfType<ObjectProperty>());
        Assert.Same(property, objectProperty);

        Assert.True(objectProperty.Computed);
        var memberExpression = Assert.IsType<MemberExpression>(objectProperty.Key);
        var identifier = Assert.IsType<Identifier>(memberExpression.Object);
        Assert.Equal("Symbol", identifier.Name);
        identifier = Assert.IsType<Identifier>(memberExpression.Property);
        Assert.Equal("iterator", identifier.Name);
    }

    [Fact]
    public void ShouldParseArrayPattern()
    {
        var parser = new Parser();

        var program = parser.ParseScript(
            """
            var values = [1, 2, 3];

            var callCount = 0;
            var f;
            f = ([...[...x]]) => {
                callCount = callCount + 1;
            };

            f(values);
            """);

        var arrowFunctionExpression = Assert.Single(program.DescendantNodes().OfType<ArrowFunctionExpression>());
        var param = Assert.Single(arrowFunctionExpression.Params);
        var arrayPattern = Assert.IsType<ArrayPattern>(param);
        var element = Assert.Single(arrayPattern.Elements);
        var restElement = Assert.IsType<RestElement>(element);
        arrayPattern = Assert.IsType<ArrayPattern>(restElement.Argument);
        element = Assert.Single(arrayPattern.Elements);
        restElement = Assert.IsType<RestElement>(element);
        var identifier = Assert.IsType<Identifier>(restElement.Argument);
        Assert.Equal("x", identifier.Name);
    }

    [Fact]
    public void ThrowsErrorForInvalidCurly()
    {
        var parser = new Parser();
        var ex = Assert.Throws<SyntaxErrorException>(() => parser.ParseScript("if (1}=1) eval('1');"));
        Assert.Equal(5, ex.Error.Index);
        Assert.Equal(1, ex.LineNumber);
        Assert.Equal(5, ex.Column);
        Assert.Equal("UnexpectedToken", ex.Error.Code);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("...")]
    public void ThrowsErrorForDot(string script)
    {
        var parser = new Parser();
        var ex = Assert.Throws<SyntaxErrorException>(() => parser.ParseScript(script));
        Assert.Equal(0, ex.Error.Index);
        Assert.Equal(1, ex.LineNumber);
        Assert.Equal(0, ex.Column);
        Assert.Equal("UnexpectedToken", ex.Error.Code);
    }

    [Fact]
    public void ThrowsErrorForInvalidRegExpFlags()
    {
        var parser = new Parser();
        var ex = Assert.Throws<SyntaxErrorException>(() => parser.ParseScript("/'/o//'///C//ÿ"));
        Assert.Equal(3, ex.Error.Index);
        Assert.Equal(1, ex.LineNumber);
        Assert.Equal(3, ex.Column);
        Assert.Equal("InvalidRegExpFlags", ex.Error.Code);
    }

    [Fact]
    public void AllowsSingleProto()
    {
        var parser = new Parser(new ParserOptions { Tolerant = false });
        var program = parser.ParseScript("if({ __proto__: [] } instanceof Array) {}");

        var objectExpression = Assert.Single(program.DescendantNodes().OfType<ObjectExpression>());
        var property = Assert.Single(objectExpression.Properties);
        var objectProperty = Assert.IsType<ObjectProperty>(property);
        var identifier = Assert.IsType<Identifier>(objectProperty.Key);
        Assert.Equal("__proto__", identifier.Name);
    }

    [Fact]
    public void ThrowsErrorForDuplicateProto()
    {
        var parser = new Parser(new ParserOptions { Tolerant = false });
        var ex = Assert.Throws<SyntaxErrorException>(() => parser.ParseScript("if({ __proto__: [], __proto__: [] } instanceof Array) {}"));
        Assert.Equal(20, ex.Error.Index);
        Assert.Equal(1, ex.LineNumber);
        Assert.Equal(20, ex.Column);
        Assert.Equal("DuplicateProto", ex.Error.Code);
    }

    [Theory]
    [InlineData("(async () => { for await (var x of []) { } })()")]
    [InlineData("(async () => { for await (let x of []) { } })()")]
    [InlineData("(async () => { for await (const x of []) { } })()")]
    [InlineData("(async () => { for await (x of []) { } })()")]
    public void ParsesValidForAwaitLoops(string code)
    {
        var errorCollector = new ParseErrorCollector();
        var parser = new Parser(new ParserOptions { Tolerant = true, ErrorHandler = errorCollector });
        parser.ParseScript(code);

        Assert.Empty(errorCollector.Errors);
    }

    [Theory]
    [InlineData("(async () => { for await (;;) { } })()")]
    [InlineData("(async () => { for await (var i = 0, j = 1;;) { } })()")]
    [InlineData("(async () => { for await (let i = 0, j = 1;;) { } })()")]
    [InlineData("(async () => { for await (const i = 0, j = 1;;) { } })()")]
    [InlineData("(async () => { for await (i = 0, j = 1;;) { } })()")]
    [InlineData("(async () => { for await (var x = (0 in []) in {}) { } })()")]
    [InlineData("(async () => { for await (let x in {}) { } })()")]
    [InlineData("(async () => { for await (const x in {}) { } })()")]
    [InlineData("(async () => { for await (let in {}) { } })()")]
    [InlineData("(async () => { for await (const in {}) { } })()")]
    [InlineData("(async () => { for await (x in {}) { } })()")]
    public void ReportsInvalidForAwaitLoops(string code)
    {
        var parser = new Parser(new ParserOptions { Tolerant = false });
        Assert.Throws<SyntaxErrorException>(() => parser.ParseScript(code));
    }

    [Fact]
    public void CanParsePrivateIdentifierInOperator()
    {
        const string code =
            """
            class aa {
                #bb;
                cc(ee) {
                    var d =  #bb in ee;
                }
            }
            """;

        var program = new Parser().ParseScript(code);

        var objectExpression = Assert.Single(program.DescendantNodes().OfType<PropertyDefinition>(), pd => pd.Key is PrivateIdentifier);
        Assert.Equal("bb", objectExpression.Key.As<PrivateIdentifier>().Name);

        var binaryExpression = Assert.Single(program.DescendantNodes().OfType<BinaryExpression>());
        Assert.Equal(Operator.In, binaryExpression.Operator);
        var privateIdentifier = Assert.IsType<PrivateIdentifier>(binaryExpression.Left);
        Assert.Equal("bb", privateIdentifier.Name);
    }

    [Theory]
    [InlineData("`a`", "a")]
    [InlineData("`a${b}`", "a", "b")]
    [InlineData("`a${b}c`", "a", "b", "c")]
    public void TemplateLiteralChildNodesShouldCorrectOrder(string source, params string[] correctOrder)
    {
        var parser = new Parser();
        var script = parser.ParseScript(source);
        var templateLiteral = script.DescendantNodes().OfType<TemplateLiteral>().First();

        var childNodes = templateLiteral.ChildNodes.ToArray();
        for (var index = 0; index < correctOrder.Length; index++)
        {
            var raw = correctOrder[index];
            var rawFromNode = GetRawItem(childNodes[index]);
            Assert.Equal(raw, rawFromNode);
        }

        static string? GetRawItem(Node? item)
        {
            if (item is TemplateElement element)
            {
                return element.Value.Raw;
            }

            if (item is Identifier identifier)
            {
                return identifier.Name;
            }

            return string.Empty;
        }
    }

    [Fact]
    public void CanParseClassElementsWithNewLinesInsteadOfSemicolon()
    {
        // field-definition-accessor-no-line-terminator.js
        var parser = new Parser(new ParserOptions { ExperimentalESFeatures = ExperimentalESFeatures.Decorators });
        var program = parser.ParseScript("""
         var C = class {
           accessor
           $;
           static accessor
           $;
         }
         """);

        var declaration = (VariableDeclaration)Assert.Single(program.Body);
        var variableDeclarator = Assert.Single(declaration.Declarations);
        var classExpression = Assert.IsType<ClassExpression>(variableDeclarator.Init);

        var classElements = classExpression.Body.Body;
        Assert.Equal(4, classElements.Count);

        var first = Assert.IsType<PropertyDefinition>(classElements[0]);
        Assert.Equal("accessor", ((Identifier)first.Key).Name);
        Assert.Null(first.Value);

        var second = Assert.IsType<PropertyDefinition>(classElements[1]);
        Assert.Equal("$", ((Identifier)second.Key).Name);
        Assert.Null(second.Value);

        var third = Assert.IsType<PropertyDefinition>(classElements[2]);
        Assert.Equal("accessor", ((Identifier)third.Key).Name);
        Assert.True(third.Static);
        Assert.Null(third.Value);

        var fourth = Assert.IsType<PropertyDefinition>(classElements[3]);
        Assert.Equal("$", ((Identifier)fourth.Key).Name);
        Assert.Null(fourth.Value);
    }

    [Theory]
    [InlineData("script", true)]
    [InlineData("module", false)]
    [InlineData("expression", false)]
    public void ShouldParseTopLevelAwait(string sourceType, bool shouldThrow)
    {
        const string code = "await import('x')";

        var parser = new Parser();
        var parseAction = GetParseActionFor(sourceType);

        if (!shouldThrow)
        {
            var node = parseAction(parser, code);
            var awaitExpression = node.DescendantNodesAndSelf().OfType<AwaitExpression>().FirstOrDefault();
            Assert.NotNull(awaitExpression);
            Assert.IsType<ImportExpression>(awaitExpression.Argument);
        }
        else
        {
            Assert.Throws<SyntaxErrorException>(() => parseAction(parser, code));
        }
    }

    [Theory]
    [InlineData("script", false)]
    [InlineData("module", true)]
    [InlineData("expression", false)]
    public void ShouldAllowLetKeywordInYieldExpression(string sourceType, bool shouldThrow)
    {
        // See also: https://github.com/sebastienros/esprima-dotnet/issues/403

        const string code = "function* f(x) { yield let }";

        var parser = new Parser();
        var parseAction = GetParseActionFor(sourceType);

        if (!shouldThrow)
        {
            var node = parseAction(parser, code);
            var yieldExpression = node.DescendantNodesAndSelf().OfType<YieldExpression>().FirstOrDefault();
            Assert.NotNull(yieldExpression);
            Assert.IsType<Identifier>(yieldExpression.Argument);
            Assert.Equal("let", yieldExpression.Argument.As<Identifier>().Name);
        }
        else
        {
            Assert.Throws<SyntaxErrorException>(() => parseAction(parser, code));
        }
    }

    [Theory]
    [InlineData("script")]
    [InlineData("module")]
    [InlineData("expression")]
    public void ShouldAllowImportExpressionInYieldExpression(string sourceType)
    {
        // See also: https://github.com/sebastienros/esprima-dotnet/issues/403

        const string code = "function* f(x) { yield import(x) }";

        var parser = new Parser();
        var parseAction = GetParseActionFor(sourceType);

        var node = parseAction(parser, code);
        var yieldExpression = node.DescendantNodesAndSelf().OfType<YieldExpression>().FirstOrDefault();
        Assert.NotNull(yieldExpression);
        Assert.IsType<ImportExpression>(yieldExpression.Argument);
    }

    [Theory]
    [InlineData("script")]
    [InlineData("module")]
    [InlineData("expression")]
    public void ShouldDisallowImportKeywordInYieldExpression(string sourceType)
    {
        // See also: https://github.com/sebastienros/esprima-dotnet/issues/403

        const string code = "function* f(x) { yield import }";

        var parser = new Parser();
        var parseAction = GetParseActionFor(sourceType);

        Assert.Throws<SyntaxErrorException>(() => parseAction(parser, code));
    }

    [Fact]
    public void ShouldDisallowReturnInClassStaticBlock()
    {
        var parser = new Parser(new ParserOptions
        {
            AllowReturnOutsideFunction = true,
        });

        var ex = Assert.Throws<SyntaxErrorException>(() => parser.ParseScript("class X { static { return; } }"));
        Assert.Equal("Illegal return statement", ex.Description);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ShouldDisallowTestOfConditionalExpressionToBeAnUnparenthesizedArrowFunction(bool preserveParens, bool isAsync)
    {
        var asyncToken = isAsync ? "async " : "";

        var parser = new Parser(new ParserOptions { PreserveParens = preserveParens });
        var ex = Assert.Throws<SyntaxErrorException>(() => parser.ParseScript(asyncToken + "() => {} ? 1 : 0"));
        Assert.Equal(asyncToken.Length + 9, ex.Error.Index);
        Assert.Equal(1, ex.LineNumber);
        Assert.Equal(asyncToken.Length + 9, ex.Column);
        Assert.Equal(nameof(SyntaxErrorMessages.UnexpectedToken), ex.Error.Code);

        Assert.Equal(asyncToken.TrimEnd() + "()=>({})?1:0", parser.ParseScript(asyncToken + "() => ({}) ? 1 : 0").ToJavaScript());
        Assert.Equal(
            preserveParens ? asyncToken.TrimEnd() + "()=>({}?1:0)" : asyncToken.TrimEnd() + "()=>({})?1:0",
            parser.ParseScript(asyncToken + "() => ({} ? 1 : 0)").ToJavaScript());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ShouldAllowTestOfConditionalExpressionToBeAParenthesizedArrowFunction(bool preserveParens, bool isAsync)
    {
        var asyncToken = isAsync ? "async " : "";

        var parser = new Parser(new ParserOptions { PreserveParens = preserveParens });
        var ast = parser.ParseScript("(" + asyncToken + "() => {}) ? 1 : 0");
        var conditionalExpression = ast.DescendantNodesAndSelf().OfType<ConditionalExpression>().FirstOrDefault();
        Assert.NotNull(conditionalExpression);
        var test = conditionalExpression.Test;
        if (preserveParens)
        {
            test = Assert.IsType<ParenthesizedExpression>(test).Expression;
        }
        Assert.IsType<ArrowFunctionExpression>(test);

        Assert.Equal("(" + asyncToken.TrimEnd() + "()=>{})?1:0", ast.ToJavaScript());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ShouldDisallowNewSuper(bool parenthesize, bool preserveParens)
    {
        var parser = new Parser(new ParserOptions { PreserveParens = preserveParens });
        var ex = Assert.Throws<SyntaxErrorException>(() => parser.ParseScript($"class A extends B {{ constructor() {{ new {(parenthesize ? "(super)" : "super")}() }} }}"));
        Assert.Equal(40 + (parenthesize ? 1 : 0), ex.Error.Index);
        Assert.Equal(1, ex.LineNumber);
        Assert.Equal(40 + (parenthesize ? 1 : 0), ex.Column);
        Assert.Equal(nameof(SyntaxErrorMessages.UnexpectedSuper), ex.Error.Code);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ShouldPreserveParensInReinterpretedPattern(bool preserveParens)
    {
        var parser = new Parser(new ParserOptions { PreserveParens = preserveParens });
        var ast = parser.ParseScript("((x)) = 0");
        Assert.Equal(preserveParens ? 2 : 0, ast.DescendantNodes().OfType<ParenthesizedExpression>().Count());
    }

    [Theory]
    [InlineData("script", "fn() = 0", null)]
    [InlineData("script", "'use strict'; fn() = 0", "Invalid left-hand side in assignment")]
    [InlineData("module", "fn() = 0", "Invalid left-hand side in assignment")]
    [InlineData("script", "((fn())) = 0", null)]
    [InlineData("module", "((fn())) = 0", "Invalid left-hand side in assignment")]
    [InlineData("script", "fn() = (fn()) = 0", null)]
    [InlineData("module", "fn() = (fn()) = 0", "Invalid left-hand side in assignment")]
    [InlineData("script", "fn() += 0", null)]
    [InlineData("module", "fn() += 0", "Invalid left-hand side in assignment")]
    [InlineData("script", "((fn())) += 0", null)]
    [InlineData("module", "((fn())) += 0", "Invalid left-hand side in assignment")]
    [InlineData("script", "fn() ??= 0", "Invalid left-hand side in assignment")]
    [InlineData("module", "fn() ??= 0", "Invalid left-hand side in assignment")]
    [InlineData("script", "fn() ||= 0", "Invalid left-hand side in assignment")]
    [InlineData("module", "fn() ||= 0", "Invalid left-hand side in assignment")]
    [InlineData("script", "fn() &&= 0", "Invalid left-hand side in assignment")]
    [InlineData("module", "fn() &&= 0", "Invalid left-hand side in assignment")]
    [InlineData("script", "++fn()", null)]
    [InlineData("module", "++fn()", "Invalid left-hand side expression in prefix operation")]
    [InlineData("script", "((++fn()))", null)]
    [InlineData("module", "((++fn()))", "Invalid left-hand side expression in prefix operation")]
    [InlineData("script", "fn()++", null)]
    [InlineData("module", "fn()++", "Invalid left-hand side expression in postfix operation")]
    [InlineData("script", "((fn()))++", null)]
    [InlineData("module", "((fn()))++", "Invalid left-hand side expression in postfix operation")]

    [InlineData("script", "[fn()] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; [fn()] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "[fn() = 0] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; [fn() = 0] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "[x = fn() = 0] = []", null)]
    [InlineData("script", "'use strict'; [x = fn() = 0] = []", "Invalid left-hand side in assignment")]
    [InlineData("script", "[fn() += 0] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; [fn() += 0] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "[++fn()] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; [++fn()] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "[fn()++] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; [fn()++] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "[...fn()] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; [...fn()] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "[...fn() = 0] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; [...fn() = 0] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "[...(fn() = 0)] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; [...(fn() = 0)] = []", "Invalid left-hand side in assignment")]
    [InlineData("script", "([...(fn() = 0)]) = []", "Invalid left-hand side in assignment")]
    [InlineData("script", "'use strict'; ([...(fn() = 0)]) = []", "Invalid left-hand side in assignment")]
    [InlineData("script", "({ x: fn() } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; ({ x: fn() } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "({ x: fn() = 0 } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; ({ x: fn() = 0 } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "({ x: y = fn() = 0 } = {})", null)]
    [InlineData("script", "'use strict'; ({ x: y = fn() = 0 } = {})", "Invalid left-hand side in assignment")]
    [InlineData("script", "({ x: fn() += 0 } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; ({ x: fn() += 0 } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "({ x: ++fn() } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; ({ x: ++fn() } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "({ x: fn()++ } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; ({ x: fn()++ } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "({ fn(): 0 } = {})", "Unexpected token ':'")]
    [InlineData("script", "'use strict'; ({ fn(): 0 } = {})", "Unexpected token ':'")]
    [InlineData("script", "({ ...fn() } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; ({ ...fn() } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "({ ...fn() = 0 } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; ({ ...fn() = 0 } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "({ ...(fn() = 0) } = {})", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; ({ ...(fn() = 0) } = {})", "Invalid left-hand side in assignment")]
    [InlineData("script", "({ ...(fn() = 0) }) = {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "'use strict'; ({ ...(fn() = 0) }) = {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "[{a: fn()} = {}] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; [{a: fn()} = {}] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "[{a: fn() = 0} = {}] = []", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; [{a: fn() = 0} = {}] = []", "Invalid destructuring assignment target")]

    [InlineData("script", "for (fn() = 0;;) {}", null)]
    [InlineData("script", "'use strict'; for (fn() = 0;;) {}", "Invalid left-hand side in assignment")]
    [InlineData("module", "for (fn() = 0;;) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for ((fn() = 0);;) {}", null)]
    [InlineData("module", "for ((fn() = 0);;) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for (((fn())) = 0;;) {}", null)]
    [InlineData("module", "for (((fn())) = 0;;) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for (fn() += 0;;) {}", null)]
    [InlineData("module", "for (fn() += 0;;) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for (fn() ??= 0;;) {}", "Invalid left-hand side in assignment")]
    [InlineData("module", "for (fn() ??= 0;;) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for (++fn();;) {}", null)]
    [InlineData("module", "for (++fn();;) {}", "Invalid left-hand side expression in prefix operation")]
    [InlineData("script", "for (fn()++;;) {}", null)]
    [InlineData("module", "for (fn()++;;) {}", "Invalid left-hand side expression in postfix operation")]

    [InlineData("script", "for (fn() in {}) {}", null)]
    [InlineData("script", "'use strict'; for (fn() in {}) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (fn() in {}) {}", "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (((fn())) in {}) {}", null)]
    [InlineData("module", "for (((fn())) in {}) {}", "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (fn() = 0 in {}) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (fn() = 0 in {}) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for ((fn() = 0) in {}) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for ((fn() = 0) in {}) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for (((fn())) = 0 in {}) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (((fn())) = 0 in {}) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for (fn() += 0 in {}) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (fn() += 0 in {}) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for (fn() ??= 0 in {}) {}", "Invalid left-hand side in assignment")]
    [InlineData("module", "for (fn() ??= 0 in {}) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for (++fn() in {}) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (++fn() in {}) {}", "Invalid left-hand side expression in prefix operation")]
    [InlineData("script", "for (fn()++ in {}) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (fn()++ in {}) {}", "Invalid left-hand side expression in postfix operation")]
    [InlineData("script", "for ([fn()] in {}) {}", "Invalid destructuring assignment target")]
    [InlineData("module", "for ([fn()] in {}) {}", "Invalid destructuring assignment target")]
    [InlineData("script", "for (([fn()]) in {}) {}", "Invalid left-hand side in for-loop")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("module", "for (([fn()]) in {}) {}", "Invalid left-hand side in for-loop")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("script", "for ([...fn()] in {}) {}", "Invalid destructuring assignment target")]
    [InlineData("module", "for ([...fn()] in {}) {}", "Invalid destructuring assignment target")]
    [InlineData("script", "for ({x: fn()} in {}) {}", "Invalid destructuring assignment target")]
    [InlineData("module", "for ({x: fn()} in {}) {}", "Invalid destructuring assignment target")]
    [InlineData("script", "for (({x: fn()}) in {}) {}", "Invalid left-hand side in for-loop")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("module", "for (({x: fn()}) in {}) {}", "Invalid left-hand side in for-loop")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("script", "for ({...fn()} in {}) {}", "Invalid destructuring assignment target")]
    [InlineData("module", "for ({...fn()} in {}) {}", "Invalid destructuring assignment target")]

    [InlineData("script", "for (fn() of []) {}", null)]
    [InlineData("script", "'use strict'; for (fn() of []) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (fn() of []) {}", "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (((fn())) of []) {}", null)]
    [InlineData("module", "for (((fn())) of []) {}", "Invalid left-hand side in for-loop")]
    [InlineData("script", "for (fn() = 0 of []) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (fn() = 0 of []) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for ((fn() = 0) of []) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for ((fn() = 0) of []) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for (((fn())) = 0 of []) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (((fn())) = 0 of []) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for (fn() += 0 of []) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (fn() += 0 of []) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for (fn() ??= 0 of []) {}", "Invalid left-hand side in assignment")]
    [InlineData("module", "for (fn() ??= 0 of []) {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "for (++fn() of []) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (++fn() of []) {}", "Invalid left-hand side expression in prefix operation")]
    [InlineData("script", "for (fn()++ of []) {}", "Invalid left-hand side in for-loop")]
    [InlineData("module", "for (fn()++ of []) {}", "Invalid left-hand side expression in postfix operation")]
    [InlineData("script", "for ([fn()] of []) {}", "Invalid destructuring assignment target")]
    [InlineData("module", "for ([fn()] of []) {}", "Invalid destructuring assignment target")]
    [InlineData("script", "for (([fn()]) of []) {}", "Invalid left-hand side in for-loop")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("module", "for (([fn()]) of []) {}", "Invalid left-hand side in for-loop")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("script", "for ([...fn()] of []) {}", "Invalid destructuring assignment target")]
    [InlineData("module", "for ([...fn()] of []) {}", "Invalid destructuring assignment target")]
    [InlineData("script", "for ({x: fn()} of []) {}", "Invalid destructuring assignment target")]
    [InlineData("module", "for ({x: fn()} of []) {}", "Invalid destructuring assignment target")]
    [InlineData("script", "for (({x: fn()}) of []) {}", "Invalid left-hand side in for-loop")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("module", "for (({x: fn()}) of []) {}", "Invalid left-hand side in for-loop")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("script", "for ({...fn()} of []) {}", "Invalid destructuring assignment target")]
    [InlineData("module", "for ({...fn()} of []) {}", "Invalid destructuring assignment target")]

    [InlineData("script", "(fn()) => {}", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; (fn()) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "(fn()) => {}", "Invalid destructuring assignment target")]
    [InlineData("script", "(((fn()))) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "(((fn()))) => {}", "Invalid destructuring assignment target")]
    [InlineData("script", "(fn() = 0) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "(fn() = 0) => {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "((fn() = 0) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "((fn() = 0) => {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "(((fn())) = 0) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "(((fn())) = 0) => {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "(fn() += 0) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "(fn() += 0) => {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "(fn() ??= 0) => {}", "Invalid left-hand side in assignment")]
    [InlineData("module", "(fn() ??= 0) => {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "(++fn()) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "(++fn()) => {}", "Invalid left-hand side expression in prefix operation")]
    [InlineData("script", "(fn()++) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "(fn()++) => {}", "Invalid left-hand side expression in postfix operation")]
    [InlineData("script", "(x, ...fn()) => {}", "Invalid destructuring assignment target")] // V8 reports "Unexpected token '...'"
    [InlineData("module", "(x, ...fn()) => {}", "Invalid destructuring assignment target")] // V8 reports "Unexpected token '...'"
    [InlineData("script", "(x, ...fn() = 0) => {}", "Invalid destructuring assignment target")] // V8 reports "Unexpected token '...'"
    [InlineData("module", "(x, ...fn() = 0) => {}", "Invalid left-hand side in assignment")] // V8 reports "Unexpected token '...'"
    [InlineData("script", "(x, ...(fn() = 0)) => {}", "Invalid destructuring assignment target")] // V8 reports "Unexpected token '('"
    [InlineData("module", "(x, ...(fn() = 0)) => {}", "Invalid left-hand side in assignment")] // V8 reports "Unexpected token '('"
    [InlineData("script", "([fn()]) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "([fn()]) => {}", "Invalid destructuring assignment target")]
    [InlineData("script", "([...fn()]) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "([...fn()]) => {}", "Invalid destructuring assignment target")]
    [InlineData("script", "({x: fn()}) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "({x: fn()}) => {}", "Invalid destructuring assignment target")]
    [InlineData("script", "({...fn()}) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "({...fn()}) => {}", "Invalid destructuring assignment target")]

    [InlineData("script", "async (fn()) => {}", "Invalid destructuring assignment target")]
    [InlineData("script", "'use strict'; async (fn()) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "async (fn()) => {}", "Invalid destructuring assignment target")]
    [InlineData("script", "async (await()) => {}", "'await' is not a valid identifier name in an async function")] // V8 reports "Unexpected token ')'"
    [InlineData("module", "async (await()) => {}", "Unexpected token ')'")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("script", "async (((fn()))) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "async (((fn()))) => {}", "Invalid destructuring assignment target")]
    [InlineData("script", "async ((await())) => {}", "Invalid destructuring assignment target")] // V8 reports "Unexpected token ')'"
    [InlineData("module", "async ((await())) => {}", "Unexpected token ')'")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("script", "async (fn() = 0) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "async (fn() = 0) => {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "async (await() = 0) => {}", "'await' is not a valid identifier name in an async function")] // V8 reports "Unexpected token ')'"
    [InlineData("module", "async (await() = 0) => {}", "Unexpected token ')'")] // V8 reports "Unexpected token ')'"
    [InlineData("script", "async ((fn() = 0) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "async ((fn() = 0) => {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "async ((await() = 0)) => {}", "Invalid destructuring assignment target")] // V8 reports "Unexpected token ')'"
    [InlineData("module", "async ((await() = 0)) => {}", "Unexpected token ')'")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("script", "async (((fn())) = 0) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "async (((fn())) = 0) => {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "async ((await()) = 0) => {}", "Invalid destructuring assignment target")] // V8 reports "'await' is not a valid identifier name in an async function"
    [InlineData("module", "async ((await()) = 0) => {}", "Unexpected token ')'")] // V8 reports "Invalid destructuring assignment target"
    [InlineData("script", "async (fn() += 0) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "async (fn() += 0) => {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "async (fn() ??= 0) => {}", "Invalid left-hand side in assignment")]
    [InlineData("module", "async (fn() ??= 0) => {}", "Invalid left-hand side in assignment")]
    [InlineData("script", "async (++fn()) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "async (++fn()) => {}", "Invalid left-hand side expression in prefix operation")]
    [InlineData("script", "async (fn()++) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "async (fn()++) => {}", "Invalid left-hand side expression in postfix operation")]
    [InlineData("script", "async (x, ...fn()) => {}", "Invalid destructuring assignment target")] // V8 reports "Unexpected token '...'"
    [InlineData("module", "async (x, ...fn()) => {}", "Invalid destructuring assignment target")] // V8 reports "Unexpected token '...'"
    [InlineData("script", "async (x, ...fn() = 0) => {}", "Invalid destructuring assignment target")] // V8 reports "Unexpected token '...'"
    [InlineData("module", "async (x, ...fn() = 0) => {}", "Invalid left-hand side in assignment")] // V8 reports "Unexpected token '...'"
    [InlineData("script", "async (x, ...(fn() = 0)) => {}", "Invalid destructuring assignment target")] // V8 reports "Unexpected token '('"
    [InlineData("module", "async (x, ...(fn() = 0)) => {}", "Invalid left-hand side in assignment")] // V8 reports "Unexpected token '('"
    [InlineData("script", "async ([fn()]) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "async ([fn()]) => {}", "Invalid destructuring assignment target")]
    [InlineData("script", "async ([...fn()]) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "async ([...fn()]) => {}", "Invalid destructuring assignment target")]
    [InlineData("script", "async ({x: fn()}) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "async ({x: fn()}) => {}", "Invalid destructuring assignment target")]
    [InlineData("script", "async ({...fn()}) => {}", "Invalid destructuring assignment target")]
    [InlineData("module", "async ({...fn()}) => {}", "Invalid destructuring assignment target")]
    public void ShouldAllowFunctionCallAssignmentTargets(string sourceType, string input, string? expectedError)
    {
        var parser = new Parser();
        var parseAction = GetParseActionFor(sourceType);

        if (expectedError is null)
        {
            Assert.NotNull(parseAction(parser, input));
        }
        else
        {
            var ex = Assert.Throws<SyntaxErrorException>(() => parseAction(parser, input));
            Assert.Equal(expectedError, ex.Description);
        }
    }

    [Theory]
    [InlineData("as")]
    [InlineData("do")]
    [InlineData("if")]
    [InlineData("in")]
    [InlineData("of")]
    [InlineData("for")]
    [InlineData("get")]
    [InlineData("let")]
    [InlineData("new")]
    [InlineData("set")]
    [InlineData("try")]
    [InlineData("var")]
    [InlineData("case")]
    [InlineData("else")]
    [InlineData("enum")]
    [InlineData("from")]
    [InlineData("null")]
    [InlineData("this")]
    [InlineData("true")]
    [InlineData("void")]
    [InlineData("with")]
    [InlineData("async")]
    [InlineData("await")]
    [InlineData("break")]
    [InlineData("catch")]
    [InlineData("class")]
    [InlineData("const")]
    [InlineData("false")]
    [InlineData("super")]
    [InlineData("throw")]
    [InlineData("while")]
    [InlineData("yield")]
    [InlineData("delete")]
    [InlineData("export")]
    [InlineData("import")]
    [InlineData("return")]
    [InlineData("static")]
    [InlineData("switch")]
    [InlineData("typeof")]
    [InlineData("finally")]
    [InlineData("continue")]
    [InlineData("debugger")]
    [InlineData("function")]
    [InlineData("arguments")]
    [InlineData("instanceof")]
    [InlineData("constructor")]
    public void UsesInternedInstancesForWellKnownTokens(string token)
    {
        var stringPool = new StringPool();

        var nonInternedToken = new string(token.ToCharArray());
        var slicedToken = Tokenizer.DeduplicateString(nonInternedToken.AsSpan(), ref stringPool);
        Assert.Equal(token, slicedToken);

        Assert.NotNull(string.IsInterned(slicedToken));
        Assert.Equal(0, stringPool.Count);
    }

    [Fact]
    public void UsesPooledInstancesForNotWellKnownTokens()
    {
        var stringPool = new StringPool();

        var token = "pow2";
        var slicedToken1 = Tokenizer.DeduplicateString("pow2".AsSpan(), ref stringPool);
        Assert.Equal(token, slicedToken1);

        var source = "async function pow2(x) { return x ** 2; }";
        var slicedToken2 = Tokenizer.DeduplicateString(source.AsSpan(15, token.Length), ref stringPool);
        Assert.Equal(token, slicedToken2);

        Assert.Same(slicedToken1, slicedToken2);
        Assert.Equal(1, stringPool.Count);
    }

    private static Func<Parser, string, Node> GetParseActionFor(string sourceType)
    {
        return sourceType switch
        {
            "script" => (parser, code) => parser.ParseScript(code),
            "module" => (parser, code) => parser.ParseModule(code),
            "expression" => (parser, code) => parser.ParseExpression(code),
            _ => throw new InvalidOperationException()
        };
    }

    private static Func<Parser, string, int, int, Node> GetSliceParseActionFor(string sourceType)
    {
        return sourceType switch
        {
            "script" => (parser, code, start, length) => parser.ParseScript(code, start, length),
            "module" => (parser, code, start, length) => parser.ParseModule(code, start, length),
            "expression" => (parser, code, start, length) => parser.ParseExpression(code, start, length),
            _ => throw new InvalidOperationException()
        };
    }

    #region Import Phases

    private static Parser CreateImportPhasesParser()
    {
        return new Parser(new ParserOptions { ExperimentalESFeatures = ExperimentalESFeatures.SourcePhaseImports | ExperimentalESFeatures.DeferImportEvaluation });
    }

    [Theory]
    [InlineData("import source x from 'mod';")]
    [InlineData("import source source from 'mod';")]
    [InlineData("import source from from 'mod';")]
    public void SourcePhaseImport_ValidStaticForms(string code)
    {
        var parser = CreateImportPhasesParser();
        var module = parser.ParseModule(code);
        var decl = Assert.IsType<ImportDeclaration>(Assert.Single(module.Body));
        Assert.Equal(ImportPhase.Source, decl.Phase);
        Assert.Single(decl.Specifiers);
        Assert.IsType<ImportDefaultSpecifier>(decl.Specifiers[0]);
    }

    [Theory]
    [InlineData("import source from 'mod';", 1)]
    [InlineData("import source, { x } from 'mod';", 2)]
    [InlineData("import source, * as ns from 'mod';", 2)]
    public void SourcePhaseImport_RegularImportWithSourceAsBinding(string code, int expectedSpecifierCount)
    {
        var parser = CreateImportPhasesParser();
        var module = parser.ParseModule(code);
        var decl = Assert.IsType<ImportDeclaration>(Assert.Single(module.Body));
        Assert.Equal(ImportPhase.None, decl.Phase);
        Assert.Equal(expectedSpecifierCount, decl.Specifiers.Count);
        var spec = Assert.IsType<ImportDefaultSpecifier>(decl.Specifiers[0]);
        Assert.Equal("source", spec.Local.Name);
    }

    [Theory]
    [InlineData("import source { x } from 'mod';")]
    [InlineData("import source * as ns from 'mod';")]
    [InlineData("import source 'mod';")]
    [InlineData("import source x, y from 'mod';")]
    public void SourcePhaseImport_InvalidStaticForms(string code)
    {
        var parser = CreateImportPhasesParser();
        Assert.Throws<SyntaxErrorException>(() => parser.ParseModule(code));
    }

    [Theory]
    [InlineData("import defer * as ns from 'mod';")]
    [InlineData("import defer * as ns from 'mod' with { };")]
    public void ImportDefer_ValidStaticForms(string code)
    {
        var parser = CreateImportPhasesParser();
        var module = parser.ParseModule(code);
        var decl = Assert.IsType<ImportDeclaration>(Assert.Single(module.Body));
        Assert.Equal(ImportPhase.Defer, decl.Phase);
        Assert.Single(decl.Specifiers);
        Assert.IsType<ImportNamespaceSpecifier>(decl.Specifiers[0]);
    }

    [Theory]
    [InlineData("import defer from 'mod';", 1)]
    [InlineData("import defer, { x } from 'mod';", 2)]
    [InlineData("import defer, * as ns from 'mod';", 2)]
    public void ImportDefer_RegularImportWithDeferAsBinding(string code, int expectedSpecifierCount)
    {
        var parser = CreateImportPhasesParser();
        var module = parser.ParseModule(code);
        var decl = Assert.IsType<ImportDeclaration>(Assert.Single(module.Body));
        Assert.Equal(ImportPhase.None, decl.Phase);
        Assert.Equal(expectedSpecifierCount, decl.Specifiers.Count);
        var spec = Assert.IsType<ImportDefaultSpecifier>(decl.Specifiers[0]);
        Assert.Equal("defer", spec.Local.Name);
    }

    [Theory]
    [InlineData("import defer x from 'mod';")]
    [InlineData("import defer { x } from 'mod';")]
    [InlineData("import defer x, * as ns from 'mod';")]
    [InlineData("export defer * as ns from 'mod';")]
    public void ImportDefer_InvalidStaticForms(string code)
    {
        var parser = CreateImportPhasesParser();
        Assert.Throws<SyntaxErrorException>(() => parser.ParseModule(code));
    }

    [Theory]
    [InlineData("import.source('mod')")]
    [InlineData("import.defer('mod')")]
    [InlineData("import.defer('mod', { with: { type: 'json' } })")]
    public void DynamicImportPhase_ValidForms(string code)
    {
        var parser = CreateImportPhasesParser();
        var program = parser.ParseScript(code);
        var stmt = (ExpressionStatement)Assert.Single(program.Body);
        var expr = Assert.IsType<ImportExpression>(stmt.Expression);
        Assert.NotEqual(ImportPhase.None, expr.Phase);
    }

    [Theory]
    [InlineData("import.source()")]
    [InlineData("import.defer()")]
    [InlineData("import.source('mod', { with: { type: 'json' } })")]
    [InlineData("new import.source('mod')")]
    [InlineData("new import.defer('mod')")]
    [InlineData("import.source(...['mod'])")]
    [InlineData("import.defer(...['mod'])")]
    [InlineData("import.UNKNOWN('mod')")]
    public void DynamicImportPhase_InvalidForms(string code)
    {
        var parser = CreateImportPhasesParser();
        Assert.Throws<SyntaxErrorException>(() => parser.ParseScript(code));
    }

    [Fact]
    public void SourcePhaseImport_NotEnabledWithoutFlag()
    {
        var parser = new Parser();
        // Without the flag, `source` is just a binding name
        var module = parser.ParseModule("import source from 'mod';");
        var decl = Assert.IsType<ImportDeclaration>(Assert.Single(module.Body));
        Assert.Equal(ImportPhase.None, decl.Phase);

        // import.source is rejected without the flag
        Assert.Throws<SyntaxErrorException>(() => parser.ParseScript("import.source('mod')"));
    }

    #endregion

    [Theory]
    [InlineData("import('x')", -1)]
    [InlineData("import('x',)", 10)]
    [InlineData("import('x', {})", -1)]
    [InlineData("import('x', {},)", 14)]
    [InlineData("import.source('x')", -1)]
    [InlineData("import.source('x',)", 17)]
    [InlineData("import.defer('x')", -1)]
    [InlineData("import.defer('x' ,)", 17)]
    [InlineData("import.defer('x', {})", -1)]
    [InlineData("import.defer('x', {},)", 20)]
    public void DynamicImport_AfterTrailingCommaShouldWork(string input, int expectedTrailingCommaPosition)
    {
        var actualTrailingCommaPosition = -1;

        var parser = new Parser(new ParserOptions
        {
            OnTrailingComma = (pos, _) => actualTrailingCommaPosition = actualTrailingCommaPosition < 0 ? pos : int.MaxValue,
            ExperimentalESFeatures = ExperimentalESFeatures.SourcePhaseImports | ExperimentalESFeatures.DeferImportEvaluation
        });

        parser.ParseScript(input);

        Assert.Equal(expectedTrailingCommaPosition, actualTrailingCommaPosition);
    }

    [Theory]
    [InlineData("", new TokenKind[0])]
    [InlineData(
        " /x/ ",
        new[] { TokenKind.RegExpLiteral },
        1, 1, 4)]
    [InlineData(
        "(/x/)",
        new[] { TokenKind.Punctuator, TokenKind.RegExpLiteral, TokenKind.Punctuator },
        1, 1, 4)]
    [InlineData(
        """
        let a
        /x/
        """,
        new[] { TokenKind.Identifier, TokenKind.Identifier, TokenKind.RegExpLiteral },
        2, 0, 3)]
    [InlineData(
        """
        let a<!--
        --> /x/
        /x/
        """,
        new[] { TokenKind.Identifier, TokenKind.Identifier, TokenKind.RegExpLiteral },
        3, 0, 3)]
    [InlineData(
        """
        let a<!-- /x/
        /x/
        -->
        """,
        new[] { TokenKind.Identifier, TokenKind.Identifier, TokenKind.RegExpLiteral },
        2, 0, 3)]
    [InlineData(
        "({ *m() { yield /x/ } })",
        new[]
        {
            TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Identifier, TokenKind.Punctuator, TokenKind.Punctuator,
            TokenKind.Punctuator, TokenKind.Identifier, TokenKind.RegExpLiteral, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator,
        })]
    [InlineData(
        "async function f() { await /x/ }",
        new[]
        {
            TokenKind.Identifier, TokenKind.Keyword, TokenKind.Identifier, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator,
            TokenKind.Identifier, TokenKind.RegExpLiteral, TokenKind.Punctuator,
        })]
    [InlineData(
        "yield /x/ 2",
        new[]
        {
            TokenKind.Identifier, TokenKind.Punctuator, TokenKind.Identifier, TokenKind.Punctuator, TokenKind.NumericLiteral,
        })]
    [InlineData(
        "await /x/ 2",
        new[]
        {
            TokenKind.Identifier, TokenKind.Punctuator, TokenKind.Identifier, TokenKind.Punctuator, TokenKind.NumericLiteral,
        })]
    public void ShouldDeferOnTokenToCorrectlyEmitRegExpLiteralTokens(string input, TokenKind[] expectedTokens,
        int expectedRegExpLine = 0, int expectedRegExpStartColumn = 0, int expectedRegExpEndColumn = 0)
    {
        var actualTokens = new List<Token>();
        OnTokenHandler onToken = (in token) => actualTokens.Add(token);

        var parser = new Parser(new ParserOptions { OnToken = onToken });
        var ast = parser.ParseScript(input);

        Assert.Equal(expectedTokens.Concat(new[] { TokenKind.EOF }), actualTokens.Select(token => token.Kind));

        var eof = actualTokens[actualTokens.Count - 1];
        Assert.Equal(input.Length, eof.Start);
        Assert.Equal(input.Length, eof.End);

        if (expectedRegExpLine > 0)
        {
            var token = actualTokens.First(token => token.Kind == TokenKind.RegExpLiteral);

            Assert.Equal(expectedRegExpLine, token.Location.Start.Line);
            Assert.Equal(expectedRegExpStartColumn, token.Location.Start.Column);
            Assert.Equal(expectedRegExpLine, token.Location.End.Line);
            Assert.Equal(expectedRegExpEndColumn, token.Location.End.Column);
        }
    }
}
