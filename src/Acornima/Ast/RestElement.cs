using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed partial class RestElement : Node, IBindingPattern
{
    public RestElement(Node argument) : base(NodeType.RestElement)
    {
        Argument = argument;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="MemberExpression"/> (in assignment contexts only) | <see cref="ArrayPattern"/> | <see cref="ObjectPattern"/>
    /// </remarks>
    public Node Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private RestElement Rewrite(Node argument)
    {
        return new RestElement(argument);
    }
}
