using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Acornima.Ast;
using Xunit;

namespace Acornima.Tests;

public class AstTests
{
    [Fact]
    public void LiteralValueShouldBeCached()
    {
        var parser = new Parser();
        var ast = parser.ParseExpression("false + 1 + 2n");

        Literal literal = ast.DescendantNodes().OfType<BooleanLiteral>().First();
        var value = literal.Value;
        Assert.Equal(false, value);
        Assert.Same(value, literal.Value);

        literal = ast.DescendantNodes().OfType<NumericLiteral>().First();
        value = literal.Value;
        Assert.Equal(1d, value);
        Assert.Same(value, literal.Value);

        literal = ast.DescendantNodes().OfType<BigIntLiteral>().First();
        value = literal.Value;
        Assert.Equal(new BigInteger(2), value);
        Assert.Same(value, literal.Value);
    }

    [Theory]
    [InlineData("root", true, 1)]
    [InlineData("root", false, 0)]
    [InlineData("intermediate", true, 2)]
    [InlineData("intermediate", false, 1)]
    [InlineData("leaf", true, 5)]
    [InlineData("leaf", false, 4)]
    public void AncestorNodesTest(string nodeKind, bool includeSelf, int expectedCount)
    {
        var parserOptions = new ParserOptions().RecordParentNodeInUserData(true);
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

    [Fact]
    public void AncestorNodesShouldHandleNullNodes()
    {
        var source = File.ReadAllText(Path.Combine(ParserTests.GetFixturesPath(), ParserTests.FixturesDirName, "3rdparty", "raptor_frida_ios_trace.js"));
        var parserOptions = new ParserOptions().RecordParentNodeInUserData(true);
        var parser = new Parser(parserOptions);
        var script = parser.ParseScript(source);

        var variableDeclarations = script.DescendantNodesAndSelf()
            .SelectMany(z => z.AncestorNodesAndSelf(script))
            .ToList();

        Assert.Equal(4348, variableDeclarations.Count);

        var variableDeclarations2 = script.DescendantNodesAndSelf()
            .SelectMany(z => z.AncestorNodesAndSelf(node => (Node?)node.UserData))
            .ToList();

        Assert.Equal(variableDeclarations, variableDeclarations2);
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

    [Theory]
    [InlineData("let x = a + 1", false, NodeType.Program, new NodeType[0])]
    [InlineData("let x = a + 1", true, NodeType.Program, new[] { NodeType.Program })]
    [InlineData("let x = a + 1", false, NodeType.BinaryExpression, new[] { NodeType.VariableDeclaration, NodeType.VariableDeclarator, NodeType.Identifier, NodeType.BinaryExpression })]
    [InlineData("let x = a + 1", true, NodeType.BinaryExpression, new[] { NodeType.Program, NodeType.VariableDeclaration, NodeType.VariableDeclarator, NodeType.Identifier, NodeType.BinaryExpression })]
    public void DescendantNodesDescendIntoChildrenShouldWork(string input, bool includeSelf, NodeType filterType, NodeType[] expectedNodeTypes)
    {
        var parser = new Parser();
        var program = parser.ParseScript(input);

        var descendIntoChildren = (Node node) => node.Type != filterType;
        var actualNodes = (includeSelf ? program.DescendantNodesAndSelf(descendIntoChildren) : program.DescendantNodes(descendIntoChildren)).ToArray();
        Assert.Equal(expectedNodeTypes, actualNodes.Select(node => node.Type));
    }

    [Fact]
    public void DescendantNodesShouldHandleNullNodes()
    {
        var source = File.ReadAllText(Path.Combine(ParserTests.GetFixturesPath(), ParserTests.FixturesDirName, "3rdparty", "raptor_frida_ios_trace.js"));
        var parser = new Parser();
        var script = parser.ParseScript(source);

        var variableDeclarations = script.ChildNodes
            .SelectMany(z => z!.DescendantNodesAndSelf().OfType<VariableDeclaration>())
            .ToList();

        Assert.Equal(8, variableDeclarations.Count);
    }

    public static (Operator[] Operators, Func<Operator, string?> GetToken, Func<string, Operator> ParseToken)[] OperatorTokenConversionsData =>
        new (Operator[], Func<Operator, string?>, Func<string, Operator>)[]
        {
            (
                new[]
                {
                    Operator.Assignment,
                    Operator.NullishCoalescingAssignment,
                    Operator.LogicalOrAssignment,
                    Operator.LogicalAndAssignment,
                    Operator.BitwiseOrAssignment,
                    Operator.BitwiseXorAssignment,
                    Operator.BitwiseAndAssignment,
                    Operator.LeftShiftAssignment,
                    Operator.RightShiftAssignment,
                    Operator.UnsignedRightShiftAssignment,
                    Operator.AdditionAssignment,
                    Operator.SubtractionAssignment,
                    Operator.MultiplicationAssignment,
                    Operator.DivisionAssignment,
                    Operator.RemainderAssignment,
                    Operator.ExponentiationAssignment,
                },
                AssignmentExpression.OperatorToString,
                AssignmentExpression.OperatorFromString
            ),
            (
                new[]
                {
                    Operator.NullishCoalescing,
                    Operator.LogicalOr,
                    Operator.LogicalAnd,
                },
                LogicalExpression.OperatorToString,
                LogicalExpression.OperatorFromString
            ),
            (
                new[]
                {
                    Operator.BitwiseOr,
                    Operator.BitwiseXor,
                    Operator.BitwiseAnd,
                    Operator.Equality,
                    Operator.Inequality,
                    Operator.StrictEquality,
                    Operator.StrictInequality,
                    Operator.LessThan,
                    Operator.LessThanOrEqual,
                    Operator.GreaterThan,
                    Operator.GreaterThanOrEqual,
                    Operator.In,
                    Operator.InstanceOf,
                    Operator.LeftShift,
                    Operator.RightShift,
                    Operator.UnsignedRightShift,
                    Operator.Addition,
                    Operator.Subtraction,
                    Operator.Multiplication,
                    Operator.Division,
                    Operator.Remainder,
                    Operator.Exponentiation,
                },
                NonLogicalBinaryExpression.OperatorToString,
                NonLogicalBinaryExpression.OperatorFromString
            ),
            (
                new[]
                {
                    Operator.Increment,
                    Operator.Decrement,
                },
                UpdateExpression.OperatorToString,
                UpdateExpression.OperatorFromString
            ),
            (
                new[]
                {
                    Operator.LogicalNot,
                    Operator.BitwiseNot,
                    Operator.UnaryPlus,
                    Operator.UnaryNegation,
                    Operator.TypeOf,
                    Operator.Void,
                    Operator.Delete,
                },
                NonUpdateUnaryExpression.OperatorToString,
                NonUpdateUnaryExpression.OperatorFromString
            ),
        };

    [Fact]
    public void OperatorTokenConversions()
    {
        var opLookup = OperatorTokenConversionsData
            .SelectMany(data => data.Operators.Select(op => (op, data)))
            .ToDictionary(it => it.op, it => it.data);

        foreach (Operator op in Enum.GetValues(typeof(Operator)))
        {
            if (op != Operator.Unknown)
            {
                var data = opLookup[op];
                Assert.Equal(op, data.ParseToken(data.GetToken(op)!));
            }
        }
    }

    [Fact]
    public void ChildNodesAndVisitorMustBeInSync()
    {
        var source = File.ReadAllText(Path.Combine(ParserTests.GetFixturesPath(), ParserTests.FixturesDirName, "3rdparty", "bundle.js"));

        var parser = new Parser();
        var script = parser.ParseScript(source);

        new ChildNodesVerifier().Visit(script);
    }

    private sealed class CustomNode : Node
    {
        public CustomNode(Node node1, Node node2) : base(NodeType.Unknown)
        {
            Node1 = node1;
            Node2 = node2;
        }

        public Node Node1 { get; }
        public Node Node2 { get; }

        protected internal override object? Accept(AstVisitor visitor) => throw new NotSupportedException();

        protected internal override IEnumerator<Node>? GetChildNodes()
        {
            yield return Node1;
            yield return Node2;
        }
    }

    [Fact]
    public void ChildNodesCanBeImplementedByInheritors()
    {
        var id1 = new Identifier("a");
        var id2 = new Identifier("b");

        var customNode = new CustomNode(id1, id2);

        Assert.Equal(new[] { id1, id2 }, customNode.ChildNodes);
    }

    public static IEnumerable<object[]> ReusedNodeInstancesData => new[]
{
        new object[]
        {
            "export { a }; var a",
            (IEnumerable<Node> nodes) => nodes.OfType<Identifier>().Where(id => ((Node?)id.UserData) is ExportSpecifier && id.Name == "a")
        },
        new object[]
        {
            "import { b } from 'x'",
            (IEnumerable<Node> nodes) => nodes.OfType<Identifier>().Where(id => id.Name == "b")
        },
        new object[]
        {
            "({ c })",
            (IEnumerable<Node> nodes) => nodes.OfType<Identifier>().Where(id => id.Name == "c")
        },
        new object[]
        {
            "var { v } = { }",
            (IEnumerable<Node> nodes) => nodes.OfType<Identifier>().Where(id => id.Name == "v")
        },
        new object[]
        {
            "var { v = 0 } = { }",
            (IEnumerable<Node> nodes) => nodes.OfType<Identifier>().Where(id => id.Name == "v")
        },
    };

    [Theory]
    [MemberData(nameof(ReusedNodeInstancesData))]
    public void ReusedNodeInstancesEnumeratedOnlyOnce(string source, Func<IEnumerable<Node>, IEnumerable<Node>> reusedNodeSelector)
    {
        var parserOptions = new ParserOptions().RecordParentNodeInUserData(true);
        var parser = new Parser(parserOptions);
        var module = parser.ParseModule(source);

        var nodes = module.DescendantNodes();

        Assert.Single(reusedNodeSelector(nodes));
    }

    [Theory]
    [MemberData(nameof(ReusedNodeInstancesData))]
    public void ReusedNodeInstancesVisitedOnlyOnce(string source, Func<IEnumerable<Node>, IEnumerable<Node>> reusedNodeSelector)
    {
        var parserOptions = new ParserOptions().RecordParentNodeInUserData(true);
        var parser = new Parser(parserOptions);
        var module = parser.ParseModule(source);

        var nodes = new VisitedNodesCollector().Collect(module);

        Assert.Single(reusedNodeSelector(nodes));
    }

    [Theory]
    [MemberData(nameof(ReusedNodeInstancesData))]
    public void ReusedNodeInstancesRewrittenOnlyOnce(string source, Func<IEnumerable<Node>, IEnumerable<Node>> reusedNodeSelector)
    {
        var parserOptions = new ParserOptions().RecordParentNodeInUserData(true);
        var parser = new Parser(parserOptions);
        var module = parser.ParseModule(source);

        var nodes = new RewrittenNodesCollector().Collect(module);

        Assert.Single(reusedNodeSelector(nodes));
    }

    private static IEnumerable<Node> VisitorBasedDescendants(Node node)
    {
        return VisitorBasedDescendantsAndSelf(node).Skip(1);
    }

    private static IEnumerable<Node> VisitorBasedDescendantsAndSelf(Node node)
    {
        var stack = new List<Node> { node };
        var visitor = new ChildNodesCollector(stack);
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

    private sealed class ChildNodesCollector : AstVisitor
    {
        private readonly List<Node> _stack;
        private bool _isChild;

        public ChildNodesCollector(List<Node> stack)
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

    private sealed class ChildNodesVerifier : AstVisitor
    {
        private Node? _parentNode;

        public override object? Visit(Node node)
        {
            // Save visited child nodes into parent's additional data.
            if (_parentNode is not null)
            {
                var children = (List<Node>?)_parentNode.UserData;
                if (children is null)
                {
                    _parentNode.UserData = children = new List<Node>();
                }
                children.Add(node);
            }

            var originalParentNode = _parentNode;
            _parentNode = node;

            var result = base.Visit(node);

            _parentNode = originalParentNode;

            // Verify that the list of visited children matches ChildNodes.
            Assert.True(node.ChildNodes.SequenceEqualUnordered((IEnumerable<Node>?)node.UserData ?? Enumerable.Empty<Node>()));

            return result;
        }
    }

    private sealed class VisitedNodesCollector : AstVisitor
    {
        private readonly List<Node> _nodes = new List<Node>();

        public IReadOnlyList<Node> Collect(Node node)
        {
            _nodes.Clear();
            base.Visit(node);
            return _nodes;
        }

        public override object? Visit(Node node)
        {
            _nodes.Add(node);
            return base.Visit(node);
        }
    }

    private sealed class RewrittenNodesCollector : AstRewriter
    {
        private readonly List<Node> _nodes = new List<Node>();

        public IReadOnlyList<Node> Collect(Node node)
        {
            _nodes.Clear();
            base.Visit(node);
            return _nodes;
        }

        public override object? Visit(Node node)
        {
            _nodes.Add(node);
            return base.Visit(node);
        }
    }
}
