namespace Acornima.Ast;

public abstract class ImportOrExportDeclaration : Declaration
{
    private protected ImportOrExportDeclaration(NodeType type) : base(type)
    {
    }
}
