using System;
using System.Linq;
using Acornima.Jsx;
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
}
