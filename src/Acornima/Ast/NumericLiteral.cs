using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public sealed class NumericLiteral : Literal
{
    public NumericLiteral(double value, string raw)
        : base(TokenKind.NumericLiteral, raw)
    {
        Value = value;
    }

    public new double Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    // TODO: cache boxed value?
    protected override object? GetValue() => Value;
}
