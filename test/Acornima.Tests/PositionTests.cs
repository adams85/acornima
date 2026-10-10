using System;
using TUnit.Core;
using Xunit;

namespace Acornima.Tests;

public class PositionTests
{
    [Test]
    [Arguments(0, 0, "0,0")]
    [Arguments(1, 2, "1,2")]
    public void ToStringTest(int line, int column, string expected)
    {
        var position = Position.From(line, column);
        Assert.Equal(expected, position.ToString());
    }

    [Test]
    [Arguments(0, 0)]
    [Arguments(1, 2)]
    public void ParseTest(int line, int column)
    {
        var position = Position.From(line, column);
        Assert.Equal(position, Position.Parse(position.ToString()));
    }

    [Test]
    [Arguments("1,x", typeof(FormatException))]
    [Arguments("0,1", typeof(ArgumentOutOfRangeException))]
    public void ParseTest_InvalidInput(string s, Type exceptionType)
    {
        Assert.Throws(exceptionType, () => Position.Parse(s));
    }
}
