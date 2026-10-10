using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Acornima.Ast;
using Acornima.Tests.Helpers;
using Esprima.Tests.Helpers;
using TUnit.Core;
using Xunit;

namespace Acornima.Tests;

public class NodeListTests
{
    public static IEnumerable<(int, int, Lazy<NodeList<NumericLiteral>>)> CreateTestData(int start, int count)
    {
        var array = Enumerable
            .Range(start, count)
            .Select(x => new NumericLiteral(x, x.ToString(CultureInfo.InvariantCulture)))
            .ToArray();

        return
        [
            (start, count, Lazy.Create("Sequence", () => NodeList.From(array.Select(x => x)))),
            (start, count, Lazy.Create("Collection", () => NodeList.From(new BreakingCollection<NumericLiteral>(array)))),
            (start, count, Lazy.Create("ReadOnlyList", () => NodeList.From(new BreakingReadOnlyList<NumericLiteral>(array)))),
        ];
    }

    [Test]
    [MethodDataSource(nameof(CreateTestData), Arguments = [1, 0])]
    [MethodDataSource(nameof(CreateTestData), Arguments = [1, 3])]
    [MethodDataSource(nameof(CreateTestData), Arguments = [1, 4])]
    [MethodDataSource(nameof(CreateTestData), Arguments = [1, 7])]
    [MethodDataSource(nameof(CreateTestData), Arguments = [1, 10])]
    [MethodDataSource(nameof(CreateTestData), Arguments = [1, 22])]
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
