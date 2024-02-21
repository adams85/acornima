using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Properties) })]
public sealed partial class ObjectExpression : Expression
{
    private readonly NodeList<Node> _properties;

    public ObjectExpression(in NodeList<Node> properties) : base(NodeType.ObjectExpression)
    {
        _properties = properties;
    }

    /// <remarks>
    /// { <see cref="Property"/> | <see cref="SpreadElement"/> }
    /// </remarks>
    public ref readonly NodeList<Node> Properties { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _properties; }

    private ObjectExpression Rewrite(in NodeList<Node> properties)
    {
        return new ObjectExpression(properties);
    }
}
