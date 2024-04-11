using System;
using System.Globalization;
using System.Linq;
using Acornima.Ast;
using Acornima.Tests.Helpers;
using Esprima.Tests.Helpers;
using Xunit;

namespace Acornima.Tests;

public class NodeListTests
{
    public static TheoryData<int, int, Lazy<NodeList<NumericLiteral>>> CreateTestData(int start, int count)
    {
        var array = Enumerable
            .Range(start, count)
            .Select(x => new NumericLiteral(x, x.ToString(CultureInfo.InvariantCulture)))
            .ToArray();

        return new TheoryData<int, int, Lazy<NodeList<NumericLiteral>>>
        {
            { start, count, Lazy.Create("Sequence", () => NodeList.From(array.Select(x => x))) },
            { start, count, Lazy.Create("Collection", () => NodeList.From(new BreakingCollection<NumericLiteral>(array))) },
            { start, count, Lazy.Create("ReadOnlyList", () => NodeList.From(new BreakingReadOnlyList<NumericLiteral>(array))) }
        };
    }

    [Theory]
    [MemberData(nameof(CreateTestData), 1, 0)]
    [MemberData(nameof(CreateTestData), 1, 3)]
    [MemberData(nameof(CreateTestData), 1, 4)]
    [MemberData(nameof(CreateTestData), 1, 7)]
    [MemberData(nameof(CreateTestData), 1, 10)]
    [MemberData(nameof(CreateTestData), 1, 22)]
    public void Create(int start, int count, Lazy<NodeList<NumericLiteral>> xs)
    {
        var list = xs.Value;

        Assert.Equal(count, list.Count);

        for (var i = 0; i < count; i++)
        {
            Assert.Equal(start + i, list[i].Value);
        }

        using (var e = list.GetEnumerator())
        {
            for (var i = 0; i < count; i++)
            {
                Assert.True(e.MoveNext());
                Assert.Equal(start + i, e.Current.Value);
            }

            Assert.False(e.MoveNext());
        }
    }
}
