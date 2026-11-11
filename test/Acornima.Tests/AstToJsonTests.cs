using System;
using System.Linq;
using Acornima.Jsx;
using TUnit.Core;
using Xunit;

namespace Acornima.Tests;

public class AstToJsonTests
{
    // The parser builds these left-deep trees iteratively, so they parse fine but are deep enough to exhaust the stack on conversion.
    // See https://github.com/adams85/acornima/issues/55
    [Test]
    [Arguments("a", "+a", LocationMembersPlacement.End)]
    [Arguments("a", "+a", LocationMembersPlacement.Start)]
    [Arguments("a", ".b", LocationMembersPlacement.End)]
    [Arguments("a", "()", LocationMembersPlacement.End)]
    public void ThrowsCatchableExceptionOnTooDeepRecursion(string head, string link, LocationMembersPlacement locationMembersPlacement)
    {
        const int depth = 100_000;
        var input = head + string.Concat(Enumerable.Repeat(link, depth));
        var program = new Parser().ParseScript(input);

        var options = new AstToJsonOptions { IncludeLineColumn = true, IncludeRange = true, LocationMembersPlacement = locationMembersPlacement };
        Assert.Throws<InsufficientExecutionStackException>(() => program.ToJson(options));
    }

    [Test]
    public void Jsx_ThrowsCatchableExceptionOnTooDeepRecursion()
    {
        const int depth = 100_000;
        var input = "a" + string.Concat(Enumerable.Repeat("+a", depth));
        var program = new JsxParser().ParseScript(input);

        Assert.Throws<InsufficientExecutionStackException>(() => program.ToJson(JsxAstToJsonOptions.Default));
    }
}
