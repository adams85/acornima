using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Quasis), nameof(Expressions) })]
public sealed partial class TemplateLiteral : Expression
{
    private readonly NodeList<TemplateElement> _quasis;
    private readonly NodeList<Expression> _expressions;

    public TemplateLiteral(
        in NodeList<TemplateElement> quasis,
        in NodeList<Expression> expressions)
        : base(NodeType.TemplateLiteral)
    {
        _quasis = quasis;
        _expressions = expressions;
    }

    public ref readonly NodeList<TemplateElement> Quasis { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _quasis; }
    public ref readonly NodeList<Expression> Expressions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _expressions; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextTemplateLiteral(Quasis, Expressions);

    private TemplateLiteral Rewrite(in NodeList<TemplateElement> quasis, in NodeList<Expression> expressions)
    {
        return new TemplateLiteral(quasis, expressions);
    }
}
