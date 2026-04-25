using System;
using Acornima.Helpers;

namespace Acornima.Tests.Helpers;

public static class CodePointRangeHelper
{
    internal static void AddRanges(ref ArrayList<CodePointRange> ranges, Predicate<int> includesCodePoint, int start = 0, int end = UnicodeHelper.LastCodePoint)
    {
        for (; start <= end; start++)
        {
            var cp = start;

            if (!includesCodePoint(cp))
            {
                continue;
            }

            if (ranges.Count > 0)
            {
                ref var range = ref ranges.LastItemRef();
                if (range.End == cp - 1)
                {
                    range = new CodePointRange(range.Start, cp);
                    continue;
                }
            }

            ranges.Add(new CodePointRange(cp));
        }
    }
}
