using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
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

#if NET8_0_OR_GREATER
    /// <summary>
    /// Ensures that we don't regress in stack handling, only test in modern runtime for now
    /// </summary>
    [Fact]
    public void CanHandleDeepRecursion()
    {
        if (OperatingSystem.IsMacOS())
        {
            // stack limit differ quite a lot
            return;
        }

        var parser = new Parser();
#if DEBUG
        const int depth = 400;
#else
        const int depth = 895;
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
            OnComment = (in Comment comment) => comments.Add(comment),
            OnToken = (in Token token) => tokens.Add(token)
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

    [Fact]
    public void RecordsParentNodeInUserDataCorrectly()
    {
        var parser = new Parser(new ParserOptions().RecordParentNodeInUserData(true));
        var script = parser.ParseScript("function toObj(a, b) { return { a, b() { return b } }; }");

        new ParentNodeChecker().Check(script);
    }

    [Theory]
    [InlineData("", 0, 1, 0)]
    [InlineData("  ", 2, 1, 2)]
    [InlineData(" ", 1, 1, 1)]
    [InlineData(" \r\n ", 4, 2, 1)]
    public void ShouldParseWhitespace(string code, int expectedEofIndex, int expectedEofLineNumber, int expectedEofColumn)
    {
        var tokens = new List<Token>();
        var parser = new Parser(new ParserOptions { OnToken = (in Token token) => tokens.Add(token) });

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
            OnToken = (in Token token) => tokens.Add(token),
            RegExpParseMode = RegExpParseMode.AdaptToInterpreted,
            RegexTimeout = TimeSpan.FromSeconds(1)
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
        var parser = new Parser(new ParserOptions { OnComment = (in Comment comment) => comments.Add(comment) });
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
        var parser = new Parser(new ParserOptions { OnComment = (in Comment comment) => comments.Add(comment) });

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
        var parser = new Parser(new ParserOptions { OnComment = (in Comment comment) => comments.Add(comment) });

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
        var parser = new Parser(new ParserOptions { OnComment = (in Comment comment) => comments.Add(comment) });

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
                OnComment = (in Comment comment) => comments.Add(comment)
            }
            : new ParserOptions
            {
                EcmaVersion = ecmaVersion,
                OnComment = (in Comment comment) => comments.Add(comment)
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
        var parser = new Parser(new ParserOptions { OnComment = (in Comment comment) => comments.Add(comment) });

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
        var parser = new Parser(new ParserOptions { OnComment = (in Comment comment) => comments.Add(comment) });

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
        public void Check(Node node)
        {
            Assert.Null(node.UserData);

            base.Visit(node);
        }

        public override object? Visit(Node node)
        {
            var parent = (Node?)node.UserData;
            Assert.NotNull(parent);
            Assert.Contains(node, parent!.ChildNodes);

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

    [InlineData("'use strict';\r\nfunction f(arguments){}", false, EcmaVersion.ES3, null)]
    [InlineData("'use strict';\r\nfunction f(arguments){}", false, EcmaVersion.ES5, "Unexpected eval or arguments in strict mode")]
    [InlineData("'use strict';\r\n(arguments)=>{}", false, EcmaVersion.ES6, "Unexpected eval or arguments in strict mode")]
    [InlineData("'use strict'\r\nfunction f(eval){}", false, EcmaVersion.ES3, null)]
    [InlineData("'use strict'\r\nfunction f(eval){}", false, EcmaVersion.ES5, "Unexpected eval or arguments in strict mode")]
    [InlineData("'use strict'\r\n(eval)=>{}", false, EcmaVersion.ES6, "Unexpected token '=>'")] // due to implementation differences V8 reports 'Malformed arrow function parameter list'
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

    [InlineData("script", "async function f() { for await (await of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")] // V8 reports "Unexpected reserved word"
    [InlineData("module", "async function f() { for await (await of []) {} }", EcmaVersion.Latest, "Unexpected token ']'")] // V8 reports "Unexpected reserved word"
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
        Assert.Equal(5, ex.Index);
        Assert.Equal(1, ex.LineNumber);
        Assert.Equal(5, ex.Column);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("...")]
    public void ThrowsErrorForDot(string script)
    {
        var parser = new Parser();
        var ex = Assert.Throws<SyntaxErrorException>(() => parser.ParseScript(script));
        Assert.Equal(0, ex.Index);
        Assert.Equal(1, ex.LineNumber);
        Assert.Equal(0, ex.Column);
    }

    [Fact]
    public void ThrowsErrorForInvalidRegExFlags()
    {
        var parser = new Parser();
        var ex = Assert.Throws<SyntaxErrorException>(() => parser.ParseScript("/'/o//'///C//ÿ"));
        Assert.Equal(3, ex.Index);
        Assert.Equal(1, ex.LineNumber);
        Assert.Equal(3, ex.Column);
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
        Assert.Equal(20, ex.Index);
        Assert.Equal(1, ex.LineNumber);
        Assert.Equal(20, ex.Column);
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

        var objectExpression = Assert.Single(program.DescendantNodes().OfType<PropertyDefinition>().Where(pd => pd.Key is PrivateIdentifier));
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
        var parser = new Parser(new ParserOptions { EcmaVersion = EcmaVersion.Experimental });
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

    [Theory]
    [InlineData("that", EcmaVersion.ES3, false)]
    [InlineData("this", EcmaVersion.ES3, true)]
    [InlineData("super", EcmaVersion.ES3, false)]
    [InlineData("export", EcmaVersion.ES3, false)]
    [InlineData("import", EcmaVersion.ES3, false)]

    [InlineData("that", EcmaVersion.ES5, false)]
    [InlineData("this", EcmaVersion.ES5, true)]
    [InlineData("super", EcmaVersion.ES5, false)]
    [InlineData("export", EcmaVersion.ES5, false)]
    [InlineData("import", EcmaVersion.ES5, false)]

    [InlineData("that", EcmaVersion.ES6, false)]
    [InlineData("this", EcmaVersion.ES6, true)]
    [InlineData("super", EcmaVersion.ES6, true)]
    [InlineData("export", EcmaVersion.ES6, true)]
    [InlineData("import", EcmaVersion.ES6, true)]
    public void IsKeyword_Works(string word, EcmaVersion ecmaVersion, bool isKeyword)
    {
        Assert.Equal(isKeyword, Parser.IsKeyword(word.AsSpan(), ecmaVersion, out _));
    }

    [Theory]
    [InlineData("word", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("word", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("await", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("await", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("enum", EcmaVersion.ES3, false, false, false, true)]
    [InlineData("enum", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("class", EcmaVersion.ES3, false, false, false, true)]
    [InlineData("class", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES3, false, false, false, true)]
    [InlineData("abstract", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("let", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("let", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("eval", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("eval", EcmaVersion.ES3, true, false, false, false)]

    [InlineData("word", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("word", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("word", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("word", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("await", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("await", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("await", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("await", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("enum", EcmaVersion.ES5, false, false, false, true)]
    [InlineData("enum", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("enum", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("enum", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("class", EcmaVersion.ES5, false, false, false, true)]
    [InlineData("class", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("class", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("class", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("let", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("let", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("let", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("let", EcmaVersion.ES5, true, false, true, true)]
    [InlineData("arguments", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("arguments", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("eval", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("eval", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("eval", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("eval", EcmaVersion.ES5, true, false, true, false)]

    [InlineData("word", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("word", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("word", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("word", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("word", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("word", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("await", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("await", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("await", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("await", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("await", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("await", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("enum", EcmaVersion.ES6, false, false, false, true)]
    [InlineData("enum", EcmaVersion.ES6, false, false, true, true)]
    [InlineData("enum", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("enum", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("enum", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("enum", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("class", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("class", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("class", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("class", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("class", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("class", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("let", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("let", EcmaVersion.ES6, false, false, true, true)]
    [InlineData("let", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("let", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("let", EcmaVersion.ES6, true, false, true, true)]
    [InlineData("let", EcmaVersion.ES6, true, true, true, true)]
    [InlineData("arguments", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("arguments", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("arguments", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("arguments", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("eval", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("eval", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("eval", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("eval", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("eval", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("eval", EcmaVersion.ES6, true, true, true, false)]
    public void IsReservedWord_Works(string word, EcmaVersion ecmaVersion, bool allowReserved, bool isModule, bool isStrict, bool isReservedWord)
    {
        var parser = new Parser(new ParserOptions
        {
            EcmaVersion = ecmaVersion,
            AllowReserved = allowReserved ? AllowReservedOption.Yes : AllowReservedOption.No
        });
        parser.Reset("", 0, 0, isModule ? SourceType.Module : SourceType.Script, null, strict: false);

        Assert.Equal(isReservedWord, parser._isReservedWord(word.AsSpan(), isStrict));
    }

    [Theory]
    [InlineData("word", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("word", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("await", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("await", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("enum", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("enum", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("class", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("class", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("let", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("let", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES3, true, false, false, false)]
    [InlineData("eval", EcmaVersion.ES3, false, false, false, false)]
    [InlineData("eval", EcmaVersion.ES3, true, false, false, false)]

    [InlineData("word", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("word", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("word", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("word", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("await", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("await", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("await", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("await", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("enum", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("enum", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("enum", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("enum", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("class", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("class", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("class", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("class", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES5, false, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES5, true, false, true, false)]
    [InlineData("let", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("let", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("let", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("let", EcmaVersion.ES5, true, false, true, true)]
    [InlineData("arguments", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("arguments", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES5, true, false, true, true)]
    [InlineData("eval", EcmaVersion.ES5, false, false, false, false)]
    [InlineData("eval", EcmaVersion.ES5, false, false, true, true)]
    [InlineData("eval", EcmaVersion.ES5, true, false, false, false)]
    [InlineData("eval", EcmaVersion.ES5, true, false, true, true)]

    [InlineData("word", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("word", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("word", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("word", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("word", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("word", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("await", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("await", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("await", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("await", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("await", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("await", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("enum", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("enum", EcmaVersion.ES6, false, false, true, true)]
    [InlineData("enum", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("enum", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("enum", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("enum", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("class", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("class", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("class", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("class", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("class", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("class", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES6, false, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, false, true, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("abstract", EcmaVersion.ES6, true, false, true, false)]
    [InlineData("abstract", EcmaVersion.ES6, true, true, true, false)]
    [InlineData("let", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("let", EcmaVersion.ES6, false, false, true, true)]
    [InlineData("let", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("let", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("let", EcmaVersion.ES6, true, false, true, true)]
    [InlineData("let", EcmaVersion.ES6, true, true, true, true)]
    [InlineData("arguments", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES6, false, false, true, true)]
    [InlineData("arguments", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("arguments", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("arguments", EcmaVersion.ES6, true, false, true, true)]
    [InlineData("arguments", EcmaVersion.ES6, true, true, true, true)]
    [InlineData("eval", EcmaVersion.ES6, false, false, false, false)]
    [InlineData("eval", EcmaVersion.ES6, false, false, true, true)]
    [InlineData("eval", EcmaVersion.ES6, false, true, true, true)]
    [InlineData("eval", EcmaVersion.ES6, true, false, false, false)]
    [InlineData("eval", EcmaVersion.ES6, true, false, true, true)]
    [InlineData("eval", EcmaVersion.ES6, true, true, true, true)]
    public void IsReservedWordBind_Works(string word, EcmaVersion ecmaVersion, bool allowReserved, bool isModule, bool isStrict, bool isReservedWord)
    {
        var parser = new Parser(new ParserOptions
        {
            EcmaVersion = ecmaVersion,
            AllowReserved = allowReserved ? AllowReservedOption.Yes : AllowReservedOption.No
        });
        parser.Reset("", 0, 0, isModule ? SourceType.Module : SourceType.Script, null, strict: false);

        Assert.Equal(isReservedWord, parser._isReservedWordBind(word.AsSpan(), isStrict));
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
}
