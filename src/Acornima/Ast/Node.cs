using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Acornima.Helpers;
using Acornima.Properties;

namespace Acornima.Ast;

// AST hierarchy is based on:
// * https://github.com/estree/estree
// * https://github.com/DefinitelyTyped/DefinitelyTyped/tree/master/types/estree

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(), nq}}")]
public abstract class Node : INode
{
    protected Node(NodeType type)
    {
        Type = type;
    }

    public NodeType Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public ChildNodes ChildNodes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => new ChildNodes(this); }

    /// <remarks>
    /// Inheritors who extend the AST with custom node types should override this method and provide an actual implementation.
    /// </remarks>
    protected internal virtual IEnumerator<Node>? GetChildNodes() => null;

    internal virtual Node? NextChildNode(ref ChildNodes.Enumerator enumerator) =>
        throw new NotImplementedException(string.Format(ExceptionMessages.OverrideGetChildNodes, nameof(GetChildNodes)));

    public int Start { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _range.Start; }

    public int End { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _range.End; }

    internal Range _range;
    public Range Range { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _range; init => _range = value; }

    internal SourceLocation _location;
    public SourceLocation Location { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _location; init => _location = value; }

    /// <summary>
    /// Gets or sets the arbitrary, user-defined data object associated with the current <see cref="Node"/>.
    /// </summary>
    /// <remarks>
    /// The operation is not guaranteed to be thread-safe. In case concurrent access or update is possible, the necessary synchronization is caller's responsibility.
    /// </remarks>
    public object? UserData { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] set; }

    protected internal abstract object? Accept(AstVisitor visitor);

    /// <summary>
    /// Dispatches the visitation of the current node to <see cref="AstVisitor.VisitExtension(Node)"/>.
    /// </summary>
    /// <remarks>
    /// When defining custom node types, inheritors can use this method to implement the abstract <see cref="Accept(AstVisitor)"/> method.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected object? AcceptAsExtension(AstVisitor visitor)
    {
        return visitor.VisitExtension(this);
    }

    private static Func<Node, string>? s_getDebugDisplayText;

    private protected virtual string GetDebuggerDisplay()
    {
        var getDebugDisplayText = LazyInitializer.EnsureInitialized(ref s_getDebugDisplayText, () =>
        {
            var astToJavaScriptType = System.Type.GetType("Acornima.AstToJavaScript, Acornima.Extras", throwOnError: false, ignoreCase: false);
            if (astToJavaScriptType is not null
                && astToJavaScriptType.GetMethod("ToDebugDisplayText", BindingFlags.Static | BindingFlags.NonPublic, binder: null, new[] { typeof(Node) }, modifiers: null) is { } toDebugDisplayTextMethod)
            {
                try { return (Func<Node, string>)Delegate.CreateDelegate(typeof(Func<Node, string>), toDebugDisplayTextMethod); }
                catch { /* intentional no-op */ }
            }

            return node => node.ToString()!;
        });

        return $"/*{Type}*/  {getDebugDisplayText!(this)}";
    }
}
