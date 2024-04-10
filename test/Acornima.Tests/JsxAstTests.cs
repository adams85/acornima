using System.Collections.Generic;
using System.Linq;
using Acornima.Ast;
using Acornima.Jsx;
using Acornima.Jsx.Ast;
using Xunit;

namespace Acornima.Tests;

public partial class JsxAstTests
{
    private static JsxName CreateJsxName(JsxParser parser, string tagName)
        => parser.ParseExpression($"<{tagName}></{tagName}>").As<JsxElement>().OpeningElement.Name;

    [Theory]
    [InlineData("a")]
    [InlineData("b.a")]
    [InlineData("ns:a")]
    [InlineData("ns:b.a")]
    [InlineData("d.c.b.a")]
    [InlineData("ns:d.c.b.a")]
    public void JsxName_GetQualifiedName_Works(string tagName)
    {
        var parser = new JsxParser(new JsxParserOptions { JsxAllowNamespacedObjects = true });
        var jsxName = CreateJsxName(parser, tagName);
        Assert.Equal(tagName, jsxName.GetQualifiedName());
    }

    [Fact]
    public void JsxName_ValueEqualityComparer_Works()
    {
        var parser = new JsxParser(new JsxParserOptions { JsxAllowNamespacedObjects = true });

        var jsxNameSet = new HashSet<JsxName>(new[] { "a", "b.a", "ns:a", "ns:b.a", "d.c.b.a", "ns:d.c.b.a" }
            .Select(tagName => CreateJsxName(parser, tagName)),
            JsxName.ValueEqualityComparer.Default);

        Assert.Contains(CreateJsxName(parser, "ns:d.c.b.a"), jsxNameSet);
        Assert.Contains(CreateJsxName(parser, "d.c.b.a"), jsxNameSet);
        Assert.Contains(CreateJsxName(parser, "a"), jsxNameSet);
        Assert.DoesNotContain(CreateJsxName(parser, "ns2:d.c.b.a"), jsxNameSet);
        Assert.DoesNotContain(CreateJsxName(parser, "c.b.a"), jsxNameSet);
        Assert.DoesNotContain(CreateJsxName(parser, "b"), jsxNameSet);
    }
}
