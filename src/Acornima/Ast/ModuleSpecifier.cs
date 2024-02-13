using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public abstract class ModuleSpecifier : Node
{
    private protected ModuleSpecifier(NodeType type) : base(type)
    {
    }

    public Expression Local { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetLocal(); }
    protected abstract Expression GetLocal();
}
