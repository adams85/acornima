using System.Runtime.CompilerServices;

namespace Acornima;

public readonly struct TemplateValue
{
    public TemplateValue(string? cooked, string raw)
    {
        Cooked = cooked;
        Raw = raw;
    }

    public string? Cooked { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public string Raw { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public override string ToString() => Raw ?? string.Empty;
}
