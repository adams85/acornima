using System;
using System.Linq;
using Acornima.Helpers;
using TUnit.Core;
using Xunit;

namespace Acornima.Tests;

public class ArrayListTests
{
    [Test]
    [Arguments(new int[] { 1, 2, 3 }, -1, null)]
    [Arguments(new int[] { 1, 2, 3 }, 0, new int[] { 0, 1, 2, 3 })]
    [Arguments(new int[] { 1, 2, 3 }, 1, new int[] { 1, 0, 2, 3 })]
    [Arguments(new int[] { 1, 2, 3 }, 2, new int[] { 1, 2, 0, 3 })]
    [Arguments(new int[] { 1, 2, 3 }, 3, new int[] { 1, 2, 3, 0 })]
    [Arguments(new int[] { 1, 2, 3 }, 4, null)]
    public void Insert(int[] items, int index, int[]? expectedItems)
    {
        var list = new ArrayList<int>(items);
        if (expectedItems is not null)
        {
            list.Insert(index, 0);
            Assert.Equal(expectedItems.AsEnumerable(), list.AsEnumerable());
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(index, 0));
        }
    }

    [Test]
    [Arguments(new int[] { 1, 2, 3 }, -1, null)]
    [Arguments(new int[] { 1, 2, 3 }, 0, new int[] { 2, 3 })]
    [Arguments(new int[] { 1, 2, 3 }, 1, new int[] { 1, 3 })]
    [Arguments(new int[] { 1, 2, 3 }, 2, new int[] { 1, 2 })]
    [Arguments(new int[] { 1, 2, 3 }, 3, null)]
    public void RemoveAt(int[] items, int index, int[]? expectedItems)
    {
        var list = new ArrayList<int>(items);
        if (expectedItems is not null)
        {
            list.RemoveAt(index);
            Assert.Equal(expectedItems.AsEnumerable(), list.AsEnumerable());
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(index));
        }
    }
}
