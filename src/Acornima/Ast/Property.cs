using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public abstract class Property : Node, IProperty
{
    private protected Property(PropertyKind kind, Expression key, Node value, bool computed, bool shorthand)
        : base(NodeType.Property)
    {
        Kind = kind;
        Key = key;
        Value = value;
        Computed = computed;
        Shorthand = shorthand;
    }

    public PropertyKind Kind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="StringLiteral"/> | <see cref="NumericLiteral"/> | <see cref="BigIntLiteral"/> | '[' <see cref="Expression"/> ']'
    /// </remarks>
    public Expression Key { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    /// <remarks>
    /// When property of an object literal: <see cref="Expression"/> (incl. <see cref="SpreadElement"/> and <see cref="FunctionExpression"/> for getters/setters/methods)<br />
    /// When property of an object pattern: <see cref="Identifier"/> | <see cref="MemberExpression"/> (in assignment contexts only) | <see cref="ArrayPattern"/> | <see cref="ObjectPattern"/> | <see cref="AssignmentPattern"/> | <see cref="RestElement"/>
    /// </remarks>
    public Node Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Computed { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Shorthand { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Method { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetMethod(); }
    private protected abstract bool GetMethod();
}
