using System;
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
}
