using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima.Ast;

using static ExceptionHelper;

[VisitableNode(ChildProperties = new[] { nameof(Declarations) })]
public sealed partial class VariableDeclaration : Declaration
{
    public static string GetVariableDeclarationKindToken(VariableDeclarationKind kind)
    {
        return kind switch
        {
            VariableDeclarationKind.Var => "var",
            VariableDeclarationKind.Let => "let",
            VariableDeclarationKind.Const => "const",
            _ => ThrowArgumentOutOfRangeException(nameof(kind), kind.ToString(), null)
        };
    }

    private readonly NodeList<VariableDeclarator> _declarations;

    public VariableDeclaration(
        VariableDeclarationKind kind,
        in NodeList<VariableDeclarator> declarations)
        : base(NodeType.VariableDeclaration)
    {
        Kind = kind;
        _declarations = declarations;
    }

    public VariableDeclarationKind Kind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<VariableDeclarator> Declarations { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _declarations; }

    private VariableDeclaration Rewrite(in NodeList<VariableDeclarator> declarations)
    {
        return new VariableDeclaration(Kind, declarations);
    }
}
