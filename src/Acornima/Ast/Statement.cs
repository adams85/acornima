using System.Runtime.CompilerServices;

namespace Acornima.Ast;

public abstract class Statement : StatementOrExpression
{
    internal Identifier? _labelSet;

    protected Statement(NodeType type) : base(type)
    {
    }

    public Identifier? LabelSet { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _labelSet; }
}
