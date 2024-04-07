using System.Runtime.CompilerServices;

namespace Acornima;

public static class CommentExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly Range RangeRef(this in Comment comment) => ref comment._range;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly SourceLocation LocationRef(this in Comment comment) => ref comment._location;
}
