using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Elements) })]
public sealed partial class ArrayExpression : Expression
{
    private readonly NodeList<Expression?> _elements;

    public ArrayExpression(in NodeList<Expression?> elements) : base(NodeType.ArrayExpression)
    {
        _elements = elements;
    }

    /// <summary>
    /// { <see cref="Expression"/> (incl. <see cref="SpreadElement"/>) | <see langword="null"/> (omitted element) }
    /// </summary>
    public ref readonly NodeList<Expression?> Elements { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _elements; }

    private ArrayExpression Rewrite(in NodeList<Expression?> elements)
    {
        return new ArrayExpression(elements);
    }
}
