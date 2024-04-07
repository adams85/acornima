using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

public readonly struct TemplateValue
{
    public static TemplateValue From(string? cooked, string raw)
    {
        return new TemplateValue(cooked, raw ?? ThrowArgumentNullException<string>(nameof(raw)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TemplateValue(string? cooked, string raw)
    {
        Cooked = cooked;
        _raw = raw;
    }

    public string? Cooked { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private readonly string _raw;
    public string Raw { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _raw ?? string.Empty; }

    public override string ToString() => Cooked ?? Raw;
}
