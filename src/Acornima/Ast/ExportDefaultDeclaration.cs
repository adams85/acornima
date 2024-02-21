using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Declaration) })]
public sealed partial class ExportDefaultDeclaration : ExportDeclaration
{
    public ExportDefaultDeclaration(StatementOrExpression declaration) : base(NodeType.ExportDefaultDeclaration)
    {
        Declaration = declaration;
    }

    /// <remarks>
    /// <see cref="Expression"/> | <see cref="ClassDeclaration"/> | <see cref="FunctionDeclaration"/>
    /// </remarks>
    public StatementOrExpression Declaration { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private ExportDefaultDeclaration Rewrite(StatementOrExpression declaration)
    {
        return new ExportDefaultDeclaration(declaration);
    }
}
