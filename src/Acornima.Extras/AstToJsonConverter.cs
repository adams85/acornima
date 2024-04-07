using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima;

public class AstToJsonConverter : AstVisitor
{
    private readonly JsonWriter _writer;
    private protected readonly bool _includeLineColumn;
    private protected readonly bool _includeRange;
    private protected readonly LocationMembersPlacement _locationMembersPlacement;

    public AstToJsonConverter(JsonWriter writer, AstToJsonOptions options)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _includeLineColumn = options.IncludeLineColumn;
        _includeRange = options.IncludeRange;
        _locationMembersPlacement = options.LocationMembersPlacement;
    }

    private void WriteLocationInfo(Node node)
    {
        if (_includeRange)
        {
            _writer.Member("range");
            _writer.StartArray();
            _writer.Number(node.Range.Start);
            _writer.Number(node.Range.End);
            _writer.EndArray();
        }

        if (_includeLineColumn)
        {
            _writer.Member("loc");
            _writer.StartObject();
            _writer.Member("start");
            Write(node.Location.Start);
            _writer.Member("end");
            Write(node.Location.End);
            _writer.EndObject();
        }

        void Write(Position position)
        {
            _writer.StartObject();
            Member("line", position.Line);
            Member("column", position.Column);
            _writer.EndObject();
        }
    }

    private void WriteRegexValue(RegExpValue value)
    {
        _writer.StartObject();
        Member("pattern", value.Pattern);
        Member("flags", value.Flags);
        _writer.EndObject();
    }

    private void OnStartNodeObject(Node node)
    {
        _writer.StartObject();

        if ((_includeLineColumn || _includeRange)
            && _locationMembersPlacement == LocationMembersPlacement.Start)
        {
            WriteLocationInfo(node);
        }
    }

    private void OnEndNodeObject(Node element)
    {
        if ((_includeLineColumn || _includeRange)
            && _locationMembersPlacement == LocationMembersPlacement.End)
        {
            WriteLocationInfo(element);
        }

        _writer.EndObject();
    }

    protected readonly struct NodeObject : IDisposable
    {
        private readonly AstToJsonConverter _converter;
        private readonly Node _node;

        public NodeObject(AstToJsonConverter converter, Node node)
        {
            _converter = converter;
            _node = node;
        }

        public void Dispose()
        {
            _converter.OnEndNodeObject(_node);
        }
    }

    protected NodeObject StartNodeObject(Node node)
    {
        OnStartNodeObject(node);
        Member("type", node.TypeText);
        return new NodeObject(this, node);
    }

    protected void EmptyNodeObject(Node node)
    {
        using (StartNodeObject(node)) { }
    }

    protected void Member(string name)
    {
        _writer.Member(name);
    }

    protected void Member(string name, Node? node)
    {
        Member(name);
        Visit(node);
    }

    protected void Member(string name, string? value)
    {
        Member(name);
        _writer.String(value);
    }

    protected void Member(string name, bool value)
    {
        Member(name);
        _writer.Boolean(value);
    }

    protected void Member(string name, int value)
    {
        Member(name);
        _writer.Number(value);
    }

    private static readonly ConditionalWeakTable<Type, IDictionary> s_enumMap = new();

    protected void Member<T>(string name, T value) where T : struct, Enum
    {
        var map = (Dictionary<T, string>)s_enumMap.GetValue(value.GetType(), t =>
            t.GetRuntimeFields()
                .Where(f => f.IsStatic)
                .ToDictionary(f => (T)f.GetValue(null)!, f => f.Name.ToLowerInvariant()));

        Member(name, map[value]);
    }

    protected void Member<T>(string name, in NodeList<T> nodes) where T : Node?
    {
        Member(name);

        _writer.StartArray();
        foreach (var item in nodes)
        {
            Visit(item);
        }
        _writer.EndArray();
    }

    public void Convert(Node node)
    {
        Visit(node ?? throw new ArgumentNullException(nameof(node)));
    }

    public override object? Visit(Node? node)
    {
        if (node is not null)
        {
            return base.Visit(node);
        }
        else
        {
            _writer.Null();
            return node!;
        }
    }

    protected internal override object? VisitAccessorProperty(AccessorProperty node)
    {
        using (StartNodeObject(node))
        {
            Member("key", node.Key);
            Member("computed", node.Computed);
            Member("value", node.Value);
            Member("kind", node.Kind);
            Member("static", node.Static);
            if (node.Decorators.Count > 0)
            {
                Member("decorators", node.Decorators);
            }
        }

        return node;
    }

    protected internal override object? VisitArrayExpression(ArrayExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("elements", node.Elements);
        }

        return node;
    }

    protected internal override object? VisitArrayPattern(ArrayPattern node)
    {
        using (StartNodeObject(node))
        {
            Member("elements", node.Elements);
        }

        return node;
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression node)
    {
        return VisitFunction(node);
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("operator", AssignmentExpression.OperatorToString(node.Operator));
            Member("left", node.Left);
            Member("right", node.Right);
        }

        return node;
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern node)
    {
        using (StartNodeObject(node))
        {
            Member("left", node.Left);
            Member("right", node.Right);
        }

        return node;
    }

    protected internal override object? VisitAssignmentProperty(AssignmentProperty node)
    {
        return VisitProperty(node);
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("argument", node.Argument);
        }

        return node;
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("operator", node.Type == NodeType.LogicalExpression
                ? LogicalExpression.OperatorToString(node.Operator)
                : NonLogicalBinaryExpression.OperatorToString(node.Operator));
            Member("left", node.Left);
            Member("right", node.Right);
        }

        return node;
    }

    protected internal override object? VisitBlockStatement(BlockStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("body", node.Body);

            if (node is FunctionBody functionBody)
            {
                Member("strict", functionBody.Strict);
            }
        }

        return node;
    }

    protected internal override object? VisitBreakStatement(BreakStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("label", node.Label);
        }

        return node;
    }

    protected internal override object? VisitCallExpression(CallExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("callee", node.Callee);
            Member("arguments", node.Arguments);
            Member("optional", node.Optional);
        }

        return node;
    }

    protected internal override object? VisitCatchClause(CatchClause node)
    {
        using (StartNodeObject(node))
        {
            Member("param", node.Param);
            Member("body", node.Body);
        }

        return node;
    }

    protected internal override object? VisitChainExpression(ChainExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("expression", node.Expression);
        }

        return node;
    }

    protected internal override object? VisitClassBody(ClassBody node)
    {
        using (StartNodeObject(node))
        {
            Member("body", node.Body);
        }

        return node;
    }

    private object? VisitClass(IClass node)
    {
        using (StartNodeObject(node.As<Node>()))
        {
            Member("id", node.Id);
            Member("superClass", node.SuperClass);
            Member("body", node.Body);
            if (node.Decorators.Count > 0)
            {
                Member("decorators", node.Decorators);
            }
        }

        return node;
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration node)
    {
        return VisitClass(node);
    }

    protected internal override object? VisitClassExpression(ClassExpression node)
    {
        return VisitClass(node);
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("test", node.Test);
            Member("consequent", node.Consequent);
            Member("alternate", node.Alternate);
        }

        return node;
    }

    protected internal override object? VisitContinueStatement(ContinueStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("label", node.Label);
        }

        return node;
    }

    protected internal override object? VisitDebuggerStatement(DebuggerStatement node)
    {
        EmptyNodeObject(node);
        return node;
    }

    protected internal override object? VisitDecorator(Decorator node)
    {
        using (StartNodeObject(node))
        {
            Member("expression", node.Expression);
        }

        return node;
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("body", node.Body);
            Member("test", node.Test);
        }

        return node;
    }

    protected internal override object? VisitEmptyStatement(EmptyStatement node)
    {
        EmptyNodeObject(node);
        return node;
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration node)
    {
        using (StartNodeObject(node))
        {
            Member("source", node.Source);
            Member("exported", node.Exported);
            if (node.Attributes.Count > 0)
            {
                Member("attributes", node.Attributes);
            }
        }

        return node;
    }

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration node)
    {
        using (StartNodeObject(node))
        {
            Member("declaration", node.Declaration);
        }

        return node;
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration node)
    {
        using (StartNodeObject(node))
        {
            Member("declaration", node.Declaration);
            Member("specifiers", node.Specifiers);
            Member("source", node.Source);
            if (node.Attributes.Count > 0)
            {
                Member("attributes", node.Attributes);
            }
        }

        return node;
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier node)
    {
        using (StartNodeObject(node))
        {
            Member("exported", node.Exported);
            Member("local", node.Local);
        }

        return node;
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement node)
    {
        using (StartNodeObject(node))
        {
            if (node is Directive directive)
            {
                Member("directive", directive.Value);
            }

            Member("expression", node.Expression);
        }

        return node;
    }

    protected internal override object? VisitForInStatement(ForInStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("left", node.Left);
            Member("right", node.Right);
            Member("body", node.Body);
            Member("each", false);
        }

        return node;
    }

    protected internal override object? VisitForOfStatement(ForOfStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("await", node.Await);
            Member("left", node.Left);
            Member("right", node.Right);
            Member("body", node.Body);
        }

        return node;
    }

    protected internal override object? VisitForStatement(ForStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("init", node.Init);
            Member("test", node.Test);
            Member("update", node.Update);
            Member("body", node.Body);
        }

        return node;
    }

    private object? VisitFunction(IFunction node)
    {
        using (StartNodeObject(node.As<Node>()))
        {
            Member("id", node.Id);
            Member("params", node.Params);
            Member("body", node.Body);
            Member("generator", node.Generator);
            Member("expression", node.Expression);
            Member("async", node.Async);
        }

        return node;
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration node)
    {
        return VisitFunction(node);
    }

    protected internal override object? VisitFunctionExpression(FunctionExpression node)
    {
        return VisitFunction(node);
    }

    protected internal override object? VisitIdentifier(Identifier node)
    {
        using (StartNodeObject(node))
        {
            Member("name", node.Name);
        }

        return node;
    }

    protected internal override object? VisitIfStatement(IfStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("test", node.Test);
            Member("consequent", node.Consequent);
            Member("alternate", node.Alternate);
        }

        return node;
    }

    protected internal override object? VisitImportAttribute(ImportAttribute node)
    {
        using (StartNodeObject(node))
        {
            Member("key", node.Key);
            Member("value", node.Value);
        }

        return node;
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration node)
    {
        using (StartNodeObject(node))
        {
            Member("specifiers", node.Specifiers);
            Member("source", node.Source);
            if (node.Attributes.Count > 0)
            {
                Member("attributes", node.Attributes);
            }
        }

        return node;
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier node)
    {
        using (StartNodeObject(node))
        {
            Member("local", node.Local);
        }

        return node;
    }

    protected internal override object? VisitImportExpression(ImportExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("source", node.Source);

            if (node.Options is not null)
            {
                Member("options", node.Options);
            }
        }

        return node;
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier node)
    {
        using (StartNodeObject(node))
        {
            Member("local", node.Local);
        }

        return node;
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier node)
    {
        using (StartNodeObject(node))
        {
            Member("local", node.Local);
            Member("imported", node.Imported);
        }

        return node;
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("label", node.Label);
            Member("body", node.Body);
        }

        return node;
    }

    protected internal override object? VisitLiteral(Literal node)
    {
        using (StartNodeObject(node))
        {
            _writer.Member("value");

            switch (node.Kind)
            {
                case TokenKind.NullLiteral:
                case TokenKind.RegExpLiteral when node.Value is null:
                    _writer.Null();
                    break;

                case TokenKind.BooleanLiteral:
                    _writer.Boolean(node.As<BooleanLiteral>().Value);
                    break;

                case TokenKind.RegExpLiteral:
                    _writer.StartObject();
                    _writer.EndObject();
                    break;

                case TokenKind.NumericLiteral when node.As<NumericLiteral>().Value is var doubleValue
                    && !double.IsPositiveInfinity(doubleValue):

                    _writer.Number(doubleValue);
                    break;

                default:
                    _writer.String(System.Convert.ToString(node.Value, CultureInfo.InvariantCulture));
                    break;
            }

            Member("raw", node.Raw);

            if (node is RegExpLiteral regExpLiteral)
            {
                _writer.Member("regex");
                WriteRegexValue(regExpLiteral.RegExp);
            }
            else if (node is BigIntLiteral bigIntLiteral)
            {
                Member("bigint", bigIntLiteral.BigInt);
            }
        }

        return node;
    }

    protected internal override object? VisitMemberExpression(MemberExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("computed", node.Computed);
            Member("object", node.Object);
            Member("property", node.Property);
            Member("optional", node.Optional);
        }

        return node;
    }

    protected internal override object? VisitMetaProperty(MetaProperty node)
    {
        using (StartNodeObject(node))
        {
            Member("meta", node.Meta);
            Member("property", node.Property);
        }

        return node;
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition node)
    {
        using (StartNodeObject(node))
        {
            Member("key", node.Key);
            Member("computed", node.Computed);
            Member("value", node.Value);
            Member("kind", node.Kind);
            Member("static", node.Static);
            if (node.Decorators.Count > 0)
            {
                Member("decorators", node.Decorators);
            }
        }

        return node;
    }

    protected internal override object? VisitNewExpression(NewExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("callee", node.Callee);
            Member("arguments", node.Arguments);
        }

        return node;
    }

    protected internal override object? VisitObjectExpression(ObjectExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("properties", node.Properties);
        }

        return node;
    }

    protected internal override object? VisitObjectPattern(ObjectPattern node)
    {
        using (StartNodeObject(node))
        {
            Member("properties", node.Properties);
        }

        return node;
    }

    protected internal override object? VisitObjectProperty(ObjectProperty node)
    {
        return VisitProperty(node);
    }

    protected internal override object? VisitPrivateIdentifier(PrivateIdentifier node)
    {
        using (StartNodeObject(node))
        {
            Member("name", node.Name);
        }

        return node;
    }

    protected internal override object? VisitParenthesizedExpression(ParenthesizedExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("expression", node.Expression);
        }

        return node;
    }

    protected internal override object? VisitProgram(Program node)
    {
        using (StartNodeObject(node))
        {
            Member("body", node.Body);
            Member("sourceType", node.SourceType);

            if (node is Script script)
            {
                Member("strict", script.Strict);
            }
        }

        return node;
    }

    private object? VisitProperty(Property node)
    {
        using (StartNodeObject(node))
        {
            Member("key", node.Key);
            Member("computed", node.Computed);
            Member("value", node.Value);
            Member("kind", node.Kind);
            Member("shorthand", node.Shorthand);
            Member("method", node.Method);
        }

        return node;
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition node)
    {
        using (StartNodeObject(node))
        {
            Member("key", node.Key);
            Member("computed", node.Computed);
            Member("value", node.Value);
            Member("kind", node.Kind);
            Member("static", node.Static);
            if (node.Decorators.Count > 0)
            {
                Member("decorators", node.Decorators);
            }
        }

        return node;
    }

    protected internal override object? VisitRestElement(RestElement node)
    {
        using (StartNodeObject(node))
        {
            Member("argument", node.Argument);
        }

        return node;
    }

    protected internal override object? VisitReturnStatement(ReturnStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("argument", node.Argument);
        }

        return node;
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("expressions", node.Expressions);
        }

        return node;
    }

    protected internal override object? VisitSpreadElement(SpreadElement node)
    {
        using (StartNodeObject(node))
        {
            Member("argument", node.Argument);
        }

        return node;
    }

    protected internal override object? VisitStaticBlock(StaticBlock node)
    {
        using (StartNodeObject(node))
        {
            Member("body", node.Body);
        }

        return node;
    }

    protected internal override object? VisitSuper(Super node)
    {
        EmptyNodeObject(node);
        return node;
    }

    protected internal override object? VisitSwitchCase(SwitchCase node)
    {
        using (StartNodeObject(node))
        {
            Member("test", node.Test);
            Member("consequent", node.Consequent);
        }

        return node;
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("discriminant", node.Discriminant);
            Member("cases", node.Cases);
        }

        return node;
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("tag", node.Tag);
            Member("quasi", node.Quasi);
        }

        return node;
    }

    protected internal override object? VisitTemplateElement(TemplateElement node)
    {
        using (StartNodeObject(node))
        {
            _writer.Member("value");
            _writer.StartObject();
            Member("raw", node.Value.Raw);
            Member("cooked", node.Value.Cooked);
            _writer.EndObject();
            Member("tail", node.Tail);
        }

        return node;
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral node)
    {
        using (StartNodeObject(node))
        {
            Member("quasis", node.Quasis);
            Member("expressions", node.Expressions);
        }

        return node;
    }

    protected internal override object? VisitThisExpression(ThisExpression node)
    {
        EmptyNodeObject(node);
        return node;
    }

    protected internal override object? VisitThrowStatement(ThrowStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("argument", node.Argument);
        }

        return node;
    }

    protected internal override object? VisitTryStatement(TryStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("block", node.Block);
            Member("handler", node.Handler);
            Member("finalizer", node.Finalizer);
        }

        return node;
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("operator", node.Type == NodeType.UpdateExpression
                ? UpdateExpression.OperatorToString(node.Operator)
                : NonUpdateUnaryExpression.OperatorToString(node.Operator));
            Member("argument", node.Argument);
            Member("prefix", node.Prefix);
        }

        return node;
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration node)
    {
        using (StartNodeObject(node))
        {
            Member("declarations", node.Declarations);
            Member("kind", node.Kind);
        }

        return node;
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator node)
    {
        using (StartNodeObject(node))
        {
            Member("id", node.Id);
            Member("init", node.Init);
        }

        return node;
    }

    protected internal override object? VisitWhileStatement(WhileStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("test", node.Test);
            Member("body", node.Body);
        }

        return node;
    }

    protected internal override object? VisitWithStatement(WithStatement node)
    {
        using (StartNodeObject(node))
        {
            Member("object", node.Object);
            Member("body", node.Body);
        }

        return node;
    }

    protected internal override object? VisitYieldExpression(YieldExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("argument", node.Argument);
            Member("delegate", node.Delegate);
        }

        return node;
    }
}
