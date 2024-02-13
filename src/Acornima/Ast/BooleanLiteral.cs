using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima.Ast;

public sealed class BooleanLiteral : Literal
{
    private readonly object _value;

    public BooleanLiteral(bool value, string raw) : base(TokenKind.BooleanLiteral, raw)
    {
        _value = value.AsCachedObject();
    }

    public new bool Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ReferenceEquals(_value, CachedValues.True); }

    protected override object? GetValue() => Value;
}
