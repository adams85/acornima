using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Acornima.Helpers;

#if DEBUG
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(), nq}}")]
#endif
internal readonly struct CodePointRange : IComparable<CodePointRange>
{
    public CodePointRange(int codePoint) : this(codePoint, codePoint) { }

    public CodePointRange(int start, int end)
    {
        Debug.Assert(start >= 0 && end <= UnicodeHelper.LastCodePoint && start <= end);

        Start = start;
        End = end;
    }

    public readonly int Start;

    public readonly int End; /* Inclusive */

    public int Length => End - Start + 1;

    public bool Contains(int codePoint) => Start <= codePoint && codePoint <= End;

    public int CompareTo(CodePointRange other) => Start - other.Start;

    internal static bool RangesContain(int codePoint, ReadOnlySpan<int> ranges, ReadOnlySpan<int> rangeLengthLookup)
    {
        Debug.Assert(codePoint is >= 0 and <= UnicodeHelper.LastCodePoint);

        var codePointShifted = codePoint << 8;

        var index = ranges.BinarySearch(codePointShifted);
        if (index >= 0
            || (index = ~index) < ranges.Length && DecodeRange(ranges[index], rangeLengthLookup).Contains(codePoint)
            || index > 0 && DecodeRange(ranges[index - 1], rangeLengthLookup).Contains(codePoint))
        {
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static CodePointRange DecodeRange(int data, ReadOnlySpan<int> rangeLengths)
    {
        var start = data >> 8;
        return new CodePointRange(start, start + rangeLengths[data & 0xFF]);
    }

#if DEBUG
    private string GetDebuggerDisplay()
    {
        return Start != End
            ? string.Format(CultureInfo.InvariantCulture, "[U+{0:X4}..U+{1:X4}]", Start, End)
            : string.Format(CultureInfo.InvariantCulture, "[U+{0:X4}]", Start);
    }
#endif
}
