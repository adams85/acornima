using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Decorators), nameof(Id), nameof(SuperClass), nameof(Body) })]
public sealed partial class ClassDeclaration : Declaration, IClass
{
    private readonly NodeList<Decorator> _decorators;

    public ClassDeclaration(
        Identifier? id,
        Expression? superClass,
        ClassBody body,
        in NodeList<Decorator> decorators)
        : base(NodeType.ClassDeclaration)
    {
        Id = id;
        SuperClass = superClass;
        Body = body;
        _decorators = decorators;
    }

    /// <remarks>
    /// Diverging from the ESTree specification, <see langword="null"/> is used to indicate an anonymous default exported class (instead of introducing <see langword="AnonymousDefaultExportedClassDeclaration"/>).
    /// </remarks>
    public Identifier? Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? SuperClass { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ClassBody Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<Decorator> Decorators { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _decorators; }

    private ClassDeclaration Rewrite(in NodeList<Decorator> decorators, Identifier? id, Expression? superClass, ClassBody body)
    {
        return new ClassDeclaration(id, superClass, body, decorators);
    }
}
