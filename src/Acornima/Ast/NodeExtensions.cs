using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima.Ast;

public static class NodeExtensions
{
#if DEBUG
    [DebuggerStepThrough]
#endif
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T As<T>(this INode node) where T : INode
    {
        return (T)node;
    }

    public static IEnumerable<Node> AncestorNodesAndSelf(this Node node, Node rootNode)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (rootNode is null)
        {
            throw new ArgumentNullException(nameof(rootNode));
        }

        if (node == rootNode)
        {
            return new[] { node };
        }

        return Impl(node, rootNode);

        static IEnumerable<Node> Impl(Node node, Node rootNode)
        {
            using (var ancestor = node.AncestorNodes(rootNode).GetEnumerator())
            {
                if (ancestor.MoveNext())
                {
                    yield return node;

                    do
                    {
                        yield return ancestor.Current;
                    }
                    while (ancestor.MoveNext());
                }
            }
        }
    }

    public static IEnumerable<Node> AncestorNodes(this Node node, Node rootNode)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (rootNode is null)
        {
            throw new ArgumentNullException(nameof(rootNode));
        }

        if (node == rootNode)
        {
            return Enumerable.Empty<Node>();
        }

        var parents = new Stack<Node>();
        Search(rootNode, node, parents);
        return parents;

        static bool Search(Node node, Node targetNode, Stack<Node> parents)
        {
            parents.Push(node);
            foreach (var childNode in node.ChildNodes)
            {
                if (ReferenceEquals(childNode, targetNode))
                {
                    return true;
                }

                if (Search(childNode, targetNode, parents))
                {
                    return true;
                }
            }

            parents.Pop();
            return false;
        }
    }

    public static IEnumerable<Node> AncestorNodesAndSelf(this Node node, Func<Node, Node?> parentAccessor)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (parentAccessor is null)
        {
            throw new ArgumentNullException(nameof(parentAccessor));
        }

        return Impl(node, parentAccessor);

        static IEnumerable<Node> Impl(Node node, Func<Node, Node?> parentAccessor)
        {
            do
            {
                yield return node;
            }
            while ((node = parentAccessor(node!)!) is not null);
        }
    }

    public static IEnumerable<Node> AncestorNodes(this Node node, Func<Node, Node?> parentAccessor)
    {
        return AncestorNodesAndSelf(node, parentAccessor).Skip(1);
    }

    public static IEnumerable<Node> DescendantNodesAndSelf(this Node node, Func<Node, bool>? descendIntoChildren = null)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        return descendIntoChildren is null ? Impl(node) : ImplWithFilter(node, descendIntoChildren);

        static IEnumerable<Node> Impl(Node node)
        {
            var nodes = new ArrayList<Node>() { node };
            do
            {
                node = nodes.Pop();

                yield return node;

                var lastIndex = nodes.Count;

                foreach (var childNode in node.ChildNodes)
                {
                    nodes.Push(childNode);
                }

                nodes.AsSpan().Slice(lastIndex, nodes.Count - lastIndex).Reverse();
            }
            while (nodes.Count > 0);
        }

        static IEnumerable<Node> ImplWithFilter(Node node, Func<Node, bool> descendIntoChildren)
        {
            var nodes = new ArrayList<Node>() { node };
            do
            {
                node = nodes.Pop();

                yield return node;

                if (descendIntoChildren(node))
                {
                    var lastIndex = nodes.Count;

                    foreach (var childNode in node.ChildNodes)
                    {
                        nodes.Push(childNode);
                    }

                    nodes.AsSpan().Slice(lastIndex, nodes.Count - lastIndex).Reverse();
                }
            }
            while (nodes.Count > 0);
        }
    }

    public static IEnumerable<Node> DescendantNodes(this Node node, Func<Node, bool>? descendIntoChildren = null)
    {
        return DescendantNodesAndSelf(node, descendIntoChildren).Skip(1);
    }
}
