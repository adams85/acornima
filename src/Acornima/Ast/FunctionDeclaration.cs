using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Id), nameof(Params), nameof(Body) })]
public sealed partial class FunctionDeclaration : Declaration, IFunction
{
    private readonly NodeList<Node> _params;

    public FunctionDeclaration(
        Identifier? id,
        in NodeList<Node> parameters,
        FunctionBody body,
        bool generator,
        bool async)
        : base(NodeType.FunctionDeclaration)
    {
        Id = id;
        _params = parameters;
        Body = body;
        Generator = generator;
        Async = async;
    }

    /// <remarks>
    /// Diverging from the ESTree specification, <see langword="null"/> is used to indicate an anonymous default exported function (instead of introducing <see langword="AnonymousDefaultExportedFunctionDeclaration"/>).
    /// </remarks>
    public Identifier? Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    /// <remarks>
    /// { <see cref="Identifier"/> | <see cref="ArrayPattern"/> | <see cref="ObjectPattern"/> | <see cref="AssignmentPattern"/> | <see cref="RestElement"/> }
    /// </remarks>
    public ref readonly NodeList<Node> Params { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _params; }

    public FunctionBody Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    StatementOrExpression IFunction.Body => Body;

    public bool Generator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    bool IFunction.Expression => false;
    public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private FunctionDeclaration Rewrite(Identifier? id, in NodeList<Node> @params, FunctionBody body)
    {
        return new FunctionDeclaration(id, @params, body, Generator, Async);
    }
}
