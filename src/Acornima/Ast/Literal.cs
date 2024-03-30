using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Acornima.Ast;

[VisitableNode(SealOverrideMethods = true)]
public abstract partial class Literal : Expression
{
    private protected Literal(TokenKind kind, string raw)
        : base(NodeType.Literal)
    {
        Kind = kind;
        Raw = raw;
    }

    public TokenKind Kind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    /// <remarks>
    /// <see langword="null"/> | <see cref="string"/> | <see cref="bool"/> | <see cref="double"/> | <see cref="BigInteger"/> | <see cref="Regex"/>
    /// </remarks>
    public object? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetValue(); }

    protected abstract object? GetValue();

    public string Raw { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
