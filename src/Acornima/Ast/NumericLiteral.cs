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

    protected override object? GetValue()
    {
        const int index = 1;
        return _additionalDataSlot[index] ?? _additionalDataSlot.SetItem(index, Value, capacity: index + 1);
    }
}
