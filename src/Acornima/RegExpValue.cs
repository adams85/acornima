using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

public readonly struct RegExpValue
{
    public static RegExpValue From(string pattern, string flags)
    {
        return new RegExpValue(
            pattern ?? ThrowArgumentNullException<string>(nameof(pattern)),
            flags ?? ThrowArgumentNullException<string>(nameof(flags)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal RegExpValue(string pattern, string flags)
    {
        _pattern = pattern;
        _flags = flags;
    }

    private readonly string _pattern;
    public string Pattern { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _pattern ?? string.Empty; }

    private readonly string _flags;
    public string Flags { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _flags ?? string.Empty; }

    public override string ToString()
    {
        return $"/{(_pattern is { Length: > 0 } ? _pattern : "(?:)")}/{_flags}";
    }
}
