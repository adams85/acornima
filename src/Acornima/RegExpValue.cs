using System;
using System.Runtime.CompilerServices;

namespace Acornima;

public readonly struct RegExpValue
{
    public RegExpValue(string pattern, string flags)
    {
        Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        Flags = flags ?? throw new ArgumentNullException(nameof(pattern));
    }

    public string Pattern { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public string Flags { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public override string ToString()
    {
        return $"/{(Pattern is { Length: > 0 } ? Pattern : "(?:)")}/{Flags}";
    }
}
