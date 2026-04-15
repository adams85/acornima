using System.Runtime.CompilerServices;

namespace Acornima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Source), nameof(Options) })]
public sealed partial class ImportExpression : Expression
{
    public ImportExpression(Expression source)
        : this(source, null, ImportPhase.None) { }

    public ImportExpression(Expression source, Expression? options)
        : this(source, options, ImportPhase.None) { }

    public ImportExpression(Expression source, Expression? options, ImportPhase phase)
        : base(NodeType.ImportExpression)
    {
        Source = source;
        Options = options;
        Phase = phase;
    }

    public Expression Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? Options { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ImportPhase Phase { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    private ImportExpression Rewrite(Expression source, Expression? options)
    {
        return new ImportExpression(source, options, Phase);
    }
}
