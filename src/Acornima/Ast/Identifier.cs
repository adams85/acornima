using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode]
public sealed partial class Identifier : Expression, IDestructuringPatternElement
{
    public Identifier(string name)
        : base(NodeType.Identifier)
    {
        Name = name;
    }

    public string Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
