using System;
using TUnit.Core;
using Xunit;

namespace Acornima.Tests;

public class RangeTests
{
    [Test]
    [Arguments(1, 1, "[1)")]
    [Arguments(0, 5, "[0..5)")]
    public void ToStringTest(int start, int end, string expected)
    {
        var range = Range.From(start, end);
        Assert.Equal(expected, range.ToString());
    }

    [Test]
    [Arguments(1, 1)]
    [Arguments(0, 5)]
    public void ParseTest(int start, int end)
    {
        var range = Range.From(start, end);
        Assert.Equal(range, Range.Parse(range.ToString()));
    }

    [Test]
    [Arguments("[1,2 x)", typeof(FormatException))]
    [Arguments("[1..0)", typeof(ArgumentOutOfRangeException))]
    public void ParseTest_InvalidInput(string s, Type exceptionType)
    {
        Assert.Throws(exceptionType, () => Range.Parse(s));
    }
}
