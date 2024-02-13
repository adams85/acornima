using System;
using System.Collections.Generic;
using System.Linq;
using Acornima.Ast;
using Xunit;

namespace Acornima.Tests;

public class AstTests
{
    [Theory]
    [InlineData("root", true, 1)]
    [InlineData("root", false, 0)]
    [InlineData("intermediate", true, 2)]
    [InlineData("intermediate", false, 1)]
    [InlineData("leaf", true, 5)]
    [InlineData("leaf", false, 4)]
    public void AncestorNodesTest(string nodeKind, bool includeSelf, int expectedCount)
    {
        var parserOptions = new ParserOptions
        {
            OnNode = node =>
            {
                foreach (var child in node.ChildNodes)
                {
                    child.UserData = node;
                }
            }
        };
        var parser = new Parser(parserOptions);
        var program = parser.ParseScript("function f(x) { return [x + 2 * 3] } var [y] = f(1); console.log(y)");

        var node = nodeKind switch
        {
            "root" => program,
            "intermediate" => program.Body[1],
            "leaf" => program.Body[1].As<VariableDeclaration>().Declarations[0].Id.As<ArrayPattern>().Elements[0]!,
            _ => throw new ArgumentOutOfRangeException(nameof(nodeKind), nodeKind, null)
        };

        var parentAccessor = (Node node) => (Node?)node.UserData;
        var actualNodesUsingRootNode = (includeSelf ? node.AncestorNodesAndSelf(program) : node.AncestorNodes(program)).ToArray();
        var actualNodesUsingParentAccessor = (includeSelf ? node.AncestorNodesAndSelf(parentAccessor) : node.AncestorNodes(parentAccessor)).ToArray();
        Assert.Equal(expectedCount, actualNodesUsingRootNode.Length);
        Assert.Equal(expectedCount, actualNodesUsingParentAccessor.Length);

        var expectedNodes = includeSelf ? new List<Node> { node } : new List<Node>();
        while ((node = (Node?)node!.UserData) is not null)
        {
            expectedNodes.Add(node);
        }

        Assert.Equal(expectedNodes, actualNodesUsingRootNode);
        Assert.Equal(expectedNodes, actualNodesUsingParentAccessor);
    }

    [Theory]
    [InlineData("root", true, 25)]
    [InlineData("root", false, 24)]
    [InlineData("intermediate", true, 7)]
    [InlineData("intermediate", false, 6)]
    [InlineData("leaf", true, 1)]
    [InlineData("leaf", false, 0)]
    public void DescendantNodesTest(string nodeKind, bool includeSelf, int expectedCount)
    {
        var parser = new Parser();
        var program = parser.ParseScript("function f(x) { return [x + 2 * 3] } var [y] = f(1); console.log(y)");

        var node = nodeKind switch
        {
            "root" => program,
            "intermediate" => program.Body[1],
            "leaf" => program.Body[1].As<VariableDeclaration>().Declarations[0].Id.As<ArrayPattern>().Elements[0]!,
            _ => throw new ArgumentOutOfRangeException(nameof(nodeKind), nodeKind, null)
        };

        var actualNodes = (includeSelf ? node.DescendantNodesAndSelf() : node.DescendantNodes()).ToArray();
        Assert.Equal(expectedCount, actualNodes.Length);

        var expectedNodes = (includeSelf ? VisitorBasedDescendantsAndSelf(node) : VisitorBasedDescendants(node)).ToArray();
        Assert.Equal(expectedNodes, actualNodes);
    }

    private static IEnumerable<Node> VisitorBasedDescendants(Node node)
    {
        return VisitorBasedDescendantsAndSelf(node).Skip(1);
    }

    private static IEnumerable<Node> VisitorBasedDescendantsAndSelf(Node node)
    {
        var stack = new List<Node> { node };
        var visitor = new ChildrenVisitor(stack);
        do
        {
            node = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);

            yield return node;

            var lastIndex = stack.Count;
            visitor.Visit(node);
            stack.Reverse(lastIndex, stack.Count - lastIndex);
        }
        while (stack.Count > 0);
    }

    private sealed class ChildrenVisitor : AstVisitor
    {
        private readonly List<Node> _stack;
        private bool _isChild;

        public ChildrenVisitor(List<Node> stack)
        {
            _stack = stack;
        }

        public override object? Visit(Node node)
        {
            if (!_isChild)
            {
                _isChild = true;
                node = (Node)base.Visit(node)!;
                _isChild = false;
            }
            else
            {
                _stack.Add(node);
            }
            return node;
        }
    }
}
