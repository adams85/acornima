using System.Runtime.CompilerServices;

namespace Acornima;

public static class TokenExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly Range RangeRef(this in Token token) => ref token._range;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly SourceLocation LocationRef(this in Token token) => ref token._location;
}
