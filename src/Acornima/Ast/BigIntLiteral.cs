using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public sealed class BigIntLiteral : Literal
{
    public BigIntLiteral(BigInteger value, string raw) : base(TokenKind.BigIntLiteral, raw)
    {
        Value = value;
    }

    public new BigInteger Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    // TODO: cache boxed value?
    protected override object? GetValue() => Value;

    public string BigInt => Value.ToString(CultureInfo.InvariantCulture);
}
