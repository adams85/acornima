using System.Runtime.CompilerServices;

namespace Acornima;

public static class RegExpParsingContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly Range RangeRef(this in RegExpParsingContext context) => ref context._range;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly SourceLocation LocationRef(this in RegExpParsingContext context) => ref context._location;
}
