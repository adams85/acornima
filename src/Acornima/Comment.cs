using System.Runtime.CompilerServices;
using System;

namespace Acornima;

public readonly struct Comment
{
    public Comment(CommentKind kind, Range contentRange, Range range, SourceLocation location)
    {
        Kind = kind;
        ContentRange = contentRange;
        _range = range;
        Location = location;
    }

    public CommentKind Kind { get; }

    public Range ContentRange { get; }

    public int Start { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _range.Start; }
    public int End { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _range.End; }

    private readonly Range _range;
    public Range Range { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _range; }

    public SourceLocation Location { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> GetContent(string input)
    {
        return input.SliceBetween(ContentRange.Start, ContentRange.End);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> GetRawValue(string input)
    {
        return input.SliceBetween(Start, End);
    }
}
