using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Elements) })]
public sealed partial class ArrayPattern : Node, IBindingPattern
{
    private readonly NodeList<Node?> _elements;

    public ArrayPattern(in NodeList<Node?> elements) : base(NodeType.ArrayPattern)
    {
        _elements = elements;
    }

    /// <remarks>
    /// { <see cref="Identifier"/> | <see cref="MemberExpression"/> (in assignment contexts only) | <see cref="ArrayPattern"/> | <see cref="ObjectPattern"/> | <see cref="AssignmentPattern"/> | <see cref="RestElement"/> | <see langword="null"/> (omitted element) }
    /// </remarks>
    public ref readonly NodeList<Node?> Elements { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _elements; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ArrayPattern Rewrite(in NodeList<Node?> elements)
    {
        return new ArrayPattern(elements);
    }
}
