using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;

namespace Acornima.Cli.Helpers;

// Based on: https://andrewlock.net/creating-an-ascii-art-tree-in-csharp/
internal sealed class TreePrinter
{
    // Constants for drawing lines and spaces
    private const string Cross = " ├─";
    private const string Corner = " └─";
    private const string Vertical = " │ ";
    private const string Space = "   ";

    private readonly IConsole _console;

    public TreePrinter(IConsole console)
    {
        _console = console;
    }

    public void Print<TNode, TChildren>(IEnumerable<TNode> topLevelNodes, Func<TNode, TChildren> getChildren, Func<TNode, string> getDisplayText)
        where TChildren : IEnumerable<TNode>
    {
        foreach (var node in topLevelNodes)
        {
            // Print the top level nodes. We start with an empty indent.
            // Also, all "top nodes" are effectively the "last child" in
            // their respective sub-trees
            PrintNode(node, indent: "", getChildren, getDisplayText);
        }
    }

    private void PrintNode<TNode, TChildren>(TNode node, string indent, Func<TNode, TChildren> getChildren, Func<TNode, string> getDisplayText)
        where TChildren : IEnumerable<TNode>
    {
        _console.WriteLine(getDisplayText(node));

        // Loop through the children recursively, passing in the
        // indent, and the isLast parameter

        using (var enumerator = getChildren(node).GetEnumerator())
        {
            if (!enumerator.MoveNext())
            {
                return;
            }

            var child = enumerator.Current;
            bool isLast;
            for (; ; )
            {
                isLast = !enumerator.MoveNext();

                PrintChildNode(child, indent, isLast, getChildren, getDisplayText);

                if (isLast)
                {
                    break;
                }

                child = enumerator.Current;
            }
        }
    }

    private void PrintChildNode<TNode, TChildren>(TNode node, string indent, bool isLast, Func<TNode, TChildren> getChildren, Func<TNode, string> getDisplayText)
        where TChildren : IEnumerable<TNode>
    {
        // Print the provided pipes/spaces indent
        _console.Write(indent);

        // Depending if this node is a last child, print the
        // corner or cross, and calculate the indent that will
        // be passed to its children
        if (isLast)
        {
            _console.Write(Corner);
            indent += Space;
        }
        else
        {
            _console.Write(Cross);
            indent += Vertical;
        }

        PrintNode(node, indent, getChildren, getDisplayText);
    }
}
