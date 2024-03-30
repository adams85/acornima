using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public sealed class BigIntLiteral : Literal
{
    public BigIntLiteral(BigInteger value, string raw)
        : base(TokenKind.BigIntLiteral, raw)
    {
        Value = value;
    }

    public new BigInteger Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected override object? GetValue()
    {
        const int index = 1;
        return _additionalDataSlot[index] ?? _additionalDataSlot.SetItem(index, Value, capacity: index + 1);
    }

    public string BigInt => Value.ToString(CultureInfo.InvariantCulture);
}
