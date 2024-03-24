using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Body) })]
public sealed partial class ClassBody : Node
{
    private readonly NodeList<Node> _body;

    public ClassBody(in NodeList<Node> body)
        : base(NodeType.ClassBody)
    {
        _body = body;
    }

    /// <remarks>
    /// { <see cref="MethodDefinition"/> | <see cref="PropertyDefinition"/> | <see cref="StaticBlock"/> }
    /// </remarks>
    public ref readonly NodeList<Node> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

    private ClassBody Rewrite(in NodeList<Node> body)
    {
        return new ClassBody(body);
    }
}
