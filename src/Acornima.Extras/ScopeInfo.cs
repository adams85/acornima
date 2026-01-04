using System;
using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima;

public sealed class ScopeInfo
{
    public static ScopeInfo From(Node associatedNode,
        ScopeInfo? parent, ScopeInfo? varScope, ScopeInfo? thisScope,
        ReadOnlySpan<Identifier> varVariables, ReadOnlySpan<Identifier> lexicalVariables, ReadOnlySpan<Identifier> functions)
    {
        var scope = new ScopeInfo();
        return scope.Initialize(associatedNode ?? throw new ArgumentNullException(nameof(associatedNode)),
            parent, varScope ?? scope, thisScope ?? scope,
            varVariables, lexicalVariables, functions);
    }

    internal ScopeInfo()
    {
        AssociatedNode = null!;
        VarScope = ThisScope = this;
    }

    internal ScopeInfo Initialize(Node associatedNode, ScopeInfo? parent, ScopeInfo varScope, ScopeInfo thisScope,
        ReadOnlySpan<Identifier> varVariables, ReadOnlySpan<Identifier> lexicalVariables, ReadOnlySpan<Identifier> functions,
        Identifier? additionalVarVariable = null, Identifier? additionalLexicalVariable = null)
    {
        AssociatedNode = associatedNode;
        Parent = parent;
        VarScope = varScope;
        ThisScope = thisScope;
        VarVariables = new VariableCollection(varVariables, additionalVarVariable);
        LexicalVariables = new VariableCollection(lexicalVariables, additionalLexicalVariable);
        Functions = new VariableCollection(functions, additionalItem: null);

        return this;
    }

    public Node AssociatedNode { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }

    public ScopeInfo? Parent { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }
    public ScopeInfo VarScope { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }
    public ScopeInfo ThisScope { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }

    /// <summary>
    /// A list of distinct var-declared names sorted in ascending order in the current lexical scope.
    /// </summary>
    public VariableCollection VarVariables { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }

    /// <summary>
    ///  A list of distinct lexically-declared names sorted in ascending order in the current lexical scope.
    /// </summary>
    public VariableCollection LexicalVariables { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }

    /// <summary>
    /// A list of distinct lexically-declared <see cref="FunctionDeclaration"/> names sorted in ascending order in the current lexical scope.
    /// </summary>
    public VariableCollection Functions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }

    /// <summary>
    /// Gets or sets the arbitrary, user-defined data object associated with the current <see cref="ScopeInfo"/>.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    public object? UserData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    }
}
