using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima.Ast;

public sealed class BooleanLiteral : Literal
{
    private readonly object _value;

    internal BooleanLiteral(object? value, string raw)
        : base(TokenKind.BooleanLiteral, raw)
    {
        Debug.Assert(value is bool);
        _value = value!;
    }

    public BooleanLiteral(bool value, string raw)
        : this(value.AsCachedObject(), raw) { }

    public new bool Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ReferenceEquals(_value, CachedValues.True); }

    protected override object? GetValue() => Value;
}
