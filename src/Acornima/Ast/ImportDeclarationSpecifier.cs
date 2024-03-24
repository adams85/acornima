using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public abstract class ImportDeclarationSpecifier : ModuleSpecifier
{
    private protected ImportDeclarationSpecifier(Identifier local, NodeType type)
        : base(type)
    {
        Local = local;
    }

    public new Identifier Local { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    protected override Expression GetLocal() => Local;
}
