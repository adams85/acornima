using System;
using System.Collections.Generic;
using System.Linq;
using Acornima.Ast;
using Acornima.Jsx;
using Acornima.Jsx.Ast;
using Xunit;

namespace Acornima.Tests;

public partial class JsxParserTests
{
    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion_ParseElement()
    {
        var parser = new JsxParser();
        const int depth = 100_000;
        var input = $"({string.Join("", Enumerable.Range(0, depth).Select(_ => "<>"))}{string.Join("", Enumerable.Range(0, depth).Select(_ => "</>"))})";
        Assert.Throws<InsufficientExecutionStackException>(() => parser.ParseScript(input));
    }

    [Fact]
    public void ThrowsCatchableExceptionOnTooDeepRecursion_ParseAttribute()
    {
        var parser = new JsxParser();
        const int depth = 100_000;
        var input = $"({string.Join("", Enumerable.Range(0, depth).Select(_ => "<t a="))}";
        Assert.Throws<InsufficientExecutionStackException>(() => parser.ParseScript(input));
    }

    [Fact]
    public void UpstreamIssue92()
    {
        // https://github.com/acornjs/acorn-jsx/issues/92

        var parser = new JsxParser();
        var input =
            """
            let a
            <jsx />
            """;
        var ast = parser.ParseScript(input);

        Assert.Equal(2, ast.Body.Count);
        Assert.IsType<VariableDeclaration>(ast.Body[0]);
        var expressionStatement = Assert.IsType<NonSpecialExpressionStatement>(ast.Body[1]);
        Assert.IsType<JsxElement>(expressionStatement.Expression);
    }

    [Fact]
    public void UpstreamIssue127()
    {
        // https://github.com/acornjs/acorn-jsx/issues/127

        var parser = new JsxParser();
        var input =
            """
            (<div>
                <Header />
            </div>)
            """;
        var ast = parser.ParseModule(input);

        var statement = Assert.Single(ast.Body);
        var expressionStatement = Assert.IsType<NonSpecialExpressionStatement>(statement);
        var jsxElement = Assert.IsType<JsxElement>(expressionStatement.Expression);
        Assert.Equal(3, jsxElement.Children.Count);
        Assert.IsType<JsxText>(jsxElement.Children[0]);
        Assert.IsType<JsxElement>(jsxElement.Children[1]);
        Assert.IsType<JsxText>(jsxElement.Children[2]);
    }

    [Theory]
    [InlineData("", new TokenKind[0])]
    [InlineData(
        " <></> ",
        new[] { TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator },
        1, 1, 6)]
    [InlineData(
        "(<></>)",
        new[] { TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator },
        1, 1, 6)]
    [InlineData(
        """
        let a
        <></>
        """,
        new[] { TokenKind.Identifier, TokenKind.Identifier, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator },
        2, 0, 5)]
    [InlineData(
        """
        let a<!--
        --> <></>
        <></>
        """,
        new[] { TokenKind.Identifier, TokenKind.Identifier, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator },
        3, 0, 5)]
    [InlineData(
        """
        let a<!-- <></>
        <></>
        -->
        """,
        new[] { TokenKind.Identifier, TokenKind.Identifier, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator },
        2, 0, 5)]
    [InlineData(
        "({ *m() { yield <></> } })",
        new[]
        {
            TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Identifier, TokenKind.Punctuator, TokenKind.Punctuator,
            TokenKind.Punctuator, TokenKind.Identifier, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator,
        })]
    [InlineData(
        "async function f() { await <></> }",
        new[]
        {
            TokenKind.Identifier, TokenKind.Keyword, TokenKind.Identifier, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator,
            TokenKind.Identifier, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator, TokenKind.Punctuator,
        })]
    public void ShouldDeferOnTokenToCorrectlyEmitJsxTokens(string input, TokenKind[] expectedTokens,
        int expectedElementStartLine = 0, int expectedElementStartColumn = 0, int expectedElementEndColumn = 0)
    {
        var actualTokens = new List<Token>();
        OnTokenHandler onToken = (in token) => actualTokens.Add(token);

        var parser = new JsxParser(new JsxParserOptions { OnToken = onToken });
        var ast = parser.ParseScript(input);

        Assert.Equal(expectedTokens.Concat(new[] { TokenKind.EOF }), actualTokens.Select(token => token.Kind));

        var eof = actualTokens[actualTokens.Count - 1];
        Assert.Equal(input.Length, eof.Start);
        Assert.Equal(input.Length, eof.End);

        if (expectedElementStartLine > 0)
        {
            var startTagToken = actualTokens.First(token => token.Kind == TokenKind.Punctuator && token.StringValue == "<");

            var endTagToken = actualTokens
                .Where(token => token.Kind == TokenKind.Punctuator && token.StringValue == ">")
                .Skip(1)
                .First();

            Assert.Equal(expectedElementStartLine, startTagToken.Location.Start.Line);
            Assert.Equal(expectedElementStartColumn, startTagToken.Location.Start.Column);
            Assert.Equal(expectedElementStartLine, endTagToken.Location.End.Line);
            Assert.Equal(expectedElementEndColumn, endTagToken.Location.End.Column);
        }
    }
}
