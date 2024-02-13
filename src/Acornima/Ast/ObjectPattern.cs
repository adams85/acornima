using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Properties) })]
public sealed partial class ObjectPattern : Node, IBindingPattern
{
    private readonly NodeList<Node> _properties;

    public ObjectPattern(in NodeList<Node> properties) : base(NodeType.ObjectPattern)
    {
        _properties = properties;
    }

    /// <remarks>
    /// { <see cref="AssignmentProperty"/> | <see cref="RestElement"/> }
    /// </remarks>
    public ref readonly NodeList<Node> Properties { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _properties; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObjectPattern Rewrite(in NodeList<Node> properties)
    {
        return new ObjectPattern(properties);
    }
}
