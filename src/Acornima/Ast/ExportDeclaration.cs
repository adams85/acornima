namespace Acornima.Ast;

public abstract class ExportDeclaration : ImportOrExportDeclaration
{
    private protected ExportDeclaration(NodeType type) : base(type)
    {
    }
}
