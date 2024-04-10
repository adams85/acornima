//HintName: Acornima.Ast.VisitableNodes.g.cs
#nullable enable

namespace Acornima.Ast;

partial class AccessorProperty
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt2(Decorators, Key, Value);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitAccessorProperty(this);
    
    public AccessorProperty UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Decorator> decorators, Acornima.Ast.Expression key, Acornima.Ast.Expression? value)
    {
        if (decorators.IsSameAs(Decorators) && ReferenceEquals(key, Key) && ReferenceEquals(value, Value))
        {
            return this;
        }
        
        return Rewrite(decorators, key, value);
    }
}

partial class ArrayExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullable(Elements);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitArrayExpression(this);
    
    public ArrayExpression UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Expression?> elements)
    {
        if (elements.IsSameAs(Elements))
        {
            return this;
        }
        
        return Rewrite(elements);
    }
}

partial class ArrayPattern
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullable(Elements);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitArrayPattern(this);
    
    public ArrayPattern UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Node?> elements)
    {
        if (elements.IsSameAs(Elements))
        {
            return this;
        }
        
        return Rewrite(elements);
    }
}

partial class ArrowFunctionExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Params, Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitArrowFunctionExpression(this);
    
    public ArrowFunctionExpression UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Node> @params, Acornima.Ast.StatementOrExpression body)
    {
        if (@params.IsSameAs(Params) && ReferenceEquals(body, Body))
        {
            return this;
        }
        
        return Rewrite(@params, body);
    }
}

partial class AssignmentExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Left, Right);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitAssignmentExpression(this);
    
    public AssignmentExpression UpdateWith(Acornima.Ast.Node left, Acornima.Ast.Expression right)
    {
        if (ReferenceEquals(left, Left) && ReferenceEquals(right, Right))
        {
            return this;
        }
        
        return Rewrite(left, right);
    }
}

partial class AssignmentPattern
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Left, Right);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitAssignmentPattern(this);
    
    public AssignmentPattern UpdateWith(Acornima.Ast.Node left, Acornima.Ast.Expression right)
    {
        if (ReferenceEquals(left, Left) && ReferenceEquals(right, Right))
        {
            return this;
        }
        
        return Rewrite(left, right);
    }
}

partial class AssignmentProperty
{
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitAssignmentProperty(this);
    
    public AssignmentProperty UpdateWith(Acornima.Ast.Expression key, Acornima.Ast.Node value)
    {
        if (ReferenceEquals(key, Key) && ReferenceEquals(value, Value))
        {
            return this;
        }
        
        return Rewrite(key, value);
    }
}

partial class AwaitExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitAwaitExpression(this);
    
    public AwaitExpression UpdateWith(Acornima.Ast.Expression argument)
    {
        if (ReferenceEquals(argument, Argument))
        {
            return this;
        }
        
        return Rewrite(argument);
    }
}

partial class BinaryExpression
{
    internal sealed override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Left, Right);
    
    protected internal sealed override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitBinaryExpression(this);
    
    public BinaryExpression UpdateWith(Acornima.Ast.Expression left, Acornima.Ast.Expression right)
    {
        if (ReferenceEquals(left, Left) && ReferenceEquals(right, Right))
        {
            return this;
        }
        
        return Rewrite(left, right);
    }
}

partial class BlockStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitBlockStatement(this);
    
    public BlockStatement UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Statement> body)
    {
        if (body.IsSameAs(Body))
        {
            return this;
        }
        
        return Rewrite(body);
    }
}

partial class BreakStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullable(Label);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitBreakStatement(this);
    
    public BreakStatement UpdateWith(Acornima.Ast.Identifier? label)
    {
        if (ReferenceEquals(label, Label))
        {
            return this;
        }
        
        return Rewrite(label);
    }
}

partial class CallExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Callee, Arguments);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitCallExpression(this);
    
    public CallExpression UpdateWith(Acornima.Ast.Expression callee, in Acornima.Ast.NodeList<Acornima.Ast.Expression> arguments)
    {
        if (ReferenceEquals(callee, Callee) && arguments.IsSameAs(Arguments))
        {
            return this;
        }
        
        return Rewrite(callee, arguments);
    }
}

partial class CatchClause
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt0(Param, Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitCatchClause(this);
    
    public CatchClause UpdateWith(Acornima.Ast.Node? param, Acornima.Ast.NestedBlockStatement body)
    {
        if (ReferenceEquals(param, Param) && ReferenceEquals(body, Body))
        {
            return this;
        }
        
        return Rewrite(param, body);
    }
}

partial class ChainExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Expression);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitChainExpression(this);
    
    public ChainExpression UpdateWith(Acornima.Ast.Expression expression)
    {
        if (ReferenceEquals(expression, Expression))
        {
            return this;
        }
        
        return Rewrite(expression);
    }
}

partial class ClassBody
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitClassBody(this);
    
    public ClassBody UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Node> body)
    {
        if (body.IsSameAs(Body))
        {
            return this;
        }
        
        return Rewrite(body);
    }
}

partial class ClassDeclaration
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt1_2(Decorators, Id, SuperClass, Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitClassDeclaration(this);
    
    public ClassDeclaration UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Decorator> decorators, Acornima.Ast.Identifier? id, Acornima.Ast.Expression? superClass, Acornima.Ast.ClassBody body)
    {
        if (decorators.IsSameAs(Decorators) && ReferenceEquals(id, Id) && ReferenceEquals(superClass, SuperClass) && ReferenceEquals(body, Body))
        {
            return this;
        }
        
        return Rewrite(decorators, id, superClass, body);
    }
}

partial class ClassExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt1_2(Decorators, Id, SuperClass, Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitClassExpression(this);
    
    public ClassExpression UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Decorator> decorators, Acornima.Ast.Identifier? id, Acornima.Ast.Expression? superClass, Acornima.Ast.ClassBody body)
    {
        if (decorators.IsSameAs(Decorators) && ReferenceEquals(id, Id) && ReferenceEquals(superClass, SuperClass) && ReferenceEquals(body, Body))
        {
            return this;
        }
        
        return Rewrite(decorators, id, superClass, body);
    }
}

partial class ConditionalExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Test, Consequent, Alternate);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitConditionalExpression(this);
    
    public ConditionalExpression UpdateWith(Acornima.Ast.Expression test, Acornima.Ast.Expression consequent, Acornima.Ast.Expression alternate)
    {
        if (ReferenceEquals(test, Test) && ReferenceEquals(consequent, Consequent) && ReferenceEquals(alternate, Alternate))
        {
            return this;
        }
        
        return Rewrite(test, consequent, alternate);
    }
}

partial class ContinueStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullable(Label);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitContinueStatement(this);
    
    public ContinueStatement UpdateWith(Acornima.Ast.Identifier? label)
    {
        if (ReferenceEquals(label, Label))
        {
            return this;
        }
        
        return Rewrite(label);
    }
}

partial class DebuggerStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitDebuggerStatement(this);
}

partial class Decorator
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Expression);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitDecorator(this);
    
    public Decorator UpdateWith(Acornima.Ast.Expression expression)
    {
        if (ReferenceEquals(expression, Expression))
        {
            return this;
        }
        
        return Rewrite(expression);
    }
}

partial class DoWhileStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Body, Test);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitDoWhileStatement(this);
    
    public DoWhileStatement UpdateWith(Acornima.Ast.Statement body, Acornima.Ast.Expression test)
    {
        if (ReferenceEquals(body, Body) && ReferenceEquals(test, Test))
        {
            return this;
        }
        
        return Rewrite(body, test);
    }
}

partial class EmptyStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitEmptyStatement(this);
}

partial class ExportAllDeclaration
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt0(Exported, Source, Attributes);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitExportAllDeclaration(this);
    
    public ExportAllDeclaration UpdateWith(Acornima.Ast.Expression? exported, Acornima.Ast.StringLiteral source, in Acornima.Ast.NodeList<Acornima.Ast.ImportAttribute> attributes)
    {
        if (ReferenceEquals(exported, Exported) && ReferenceEquals(source, Source) && attributes.IsSameAs(Attributes))
        {
            return this;
        }
        
        return Rewrite(exported, source, attributes);
    }
}

partial class ExportDefaultDeclaration
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Declaration);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitExportDefaultDeclaration(this);
    
    public ExportDefaultDeclaration UpdateWith(Acornima.Ast.StatementOrExpression declaration)
    {
        if (ReferenceEquals(declaration, Declaration))
        {
            return this;
        }
        
        return Rewrite(declaration);
    }
}

partial class ExportNamedDeclaration
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt0_2(Declaration, Specifiers, Source, Attributes);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitExportNamedDeclaration(this);
    
    public ExportNamedDeclaration UpdateWith(Acornima.Ast.Declaration? declaration, in Acornima.Ast.NodeList<Acornima.Ast.ExportSpecifier> specifiers, Acornima.Ast.StringLiteral? source, in Acornima.Ast.NodeList<Acornima.Ast.ImportAttribute> attributes)
    {
        if (ReferenceEquals(declaration, Declaration) && specifiers.IsSameAs(Specifiers) && ReferenceEquals(source, Source) && attributes.IsSameAs(Attributes))
        {
            return this;
        }
        
        return Rewrite(declaration, specifiers, source, attributes);
    }
}

partial class ExportSpecifier
{
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitExportSpecifier(this);
    
    public ExportSpecifier UpdateWith(Acornima.Ast.Expression local, Acornima.Ast.Expression exported)
    {
        if (ReferenceEquals(local, Local) && ReferenceEquals(exported, Exported))
        {
            return this;
        }
        
        return Rewrite(local, exported);
    }
}

partial class ExpressionStatement
{
    internal sealed override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Expression);
    
    protected internal sealed override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitExpressionStatement(this);
    
    public ExpressionStatement UpdateWith(Acornima.Ast.Expression expression)
    {
        if (ReferenceEquals(expression, Expression))
        {
            return this;
        }
        
        return Rewrite(expression);
    }
}

partial class ForInStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Left, Right, Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitForInStatement(this);
    
    public ForInStatement UpdateWith(Acornima.Ast.Node left, Acornima.Ast.Expression right, Acornima.Ast.Statement body)
    {
        if (ReferenceEquals(left, Left) && ReferenceEquals(right, Right) && ReferenceEquals(body, Body))
        {
            return this;
        }
        
        return Rewrite(left, right, body);
    }
}

partial class ForOfStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Left, Right, Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitForOfStatement(this);
    
    public ForOfStatement UpdateWith(Acornima.Ast.Node left, Acornima.Ast.Expression right, Acornima.Ast.Statement body)
    {
        if (ReferenceEquals(left, Left) && ReferenceEquals(right, Right) && ReferenceEquals(body, Body))
        {
            return this;
        }
        
        return Rewrite(left, right, body);
    }
}

partial class ForStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt0_1_2(Init, Test, Update, Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitForStatement(this);
    
    public ForStatement UpdateWith(Acornima.Ast.StatementOrExpression? init, Acornima.Ast.Expression? test, Acornima.Ast.Expression? update, Acornima.Ast.Statement body)
    {
        if (ReferenceEquals(init, Init) && ReferenceEquals(test, Test) && ReferenceEquals(update, Update) && ReferenceEquals(body, Body))
        {
            return this;
        }
        
        return Rewrite(init, test, update, body);
    }
}

partial class FunctionBody
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitFunctionBody(this);
}

partial class FunctionDeclaration
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt0(Id, Params, Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitFunctionDeclaration(this);
    
    public FunctionDeclaration UpdateWith(Acornima.Ast.Identifier? id, in Acornima.Ast.NodeList<Acornima.Ast.Node> @params, Acornima.Ast.FunctionBody body)
    {
        if (ReferenceEquals(id, Id) && @params.IsSameAs(Params) && ReferenceEquals(body, Body))
        {
            return this;
        }
        
        return Rewrite(id, @params, body);
    }
}

partial class FunctionExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt0(Id, Params, Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitFunctionExpression(this);
    
    public FunctionExpression UpdateWith(Acornima.Ast.Identifier? id, in Acornima.Ast.NodeList<Acornima.Ast.Node> @params, Acornima.Ast.FunctionBody body)
    {
        if (ReferenceEquals(id, Id) && @params.IsSameAs(Params) && ReferenceEquals(body, Body))
        {
            return this;
        }
        
        return Rewrite(id, @params, body);
    }
}

partial class Identifier
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitIdentifier(this);
}

partial class IfStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt2(Test, Consequent, Alternate);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitIfStatement(this);
    
    public IfStatement UpdateWith(Acornima.Ast.Expression test, Acornima.Ast.Statement consequent, Acornima.Ast.Statement? alternate)
    {
        if (ReferenceEquals(test, Test) && ReferenceEquals(consequent, Consequent) && ReferenceEquals(alternate, Alternate))
        {
            return this;
        }
        
        return Rewrite(test, consequent, alternate);
    }
}

partial class ImportAttribute
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Key, Value);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitImportAttribute(this);
    
    public ImportAttribute UpdateWith(Acornima.Ast.Expression key, Acornima.Ast.StringLiteral value)
    {
        if (ReferenceEquals(key, Key) && ReferenceEquals(value, Value))
        {
            return this;
        }
        
        return Rewrite(key, value);
    }
}

partial class ImportDeclaration
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Specifiers, Source, Attributes);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitImportDeclaration(this);
    
    public ImportDeclaration UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.ImportDeclarationSpecifier> specifiers, Acornima.Ast.StringLiteral source, in Acornima.Ast.NodeList<Acornima.Ast.ImportAttribute> attributes)
    {
        if (specifiers.IsSameAs(Specifiers) && ReferenceEquals(source, Source) && attributes.IsSameAs(Attributes))
        {
            return this;
        }
        
        return Rewrite(specifiers, source, attributes);
    }
}

partial class ImportDefaultSpecifier
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Local);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitImportDefaultSpecifier(this);
    
    public ImportDefaultSpecifier UpdateWith(Acornima.Ast.Identifier local)
    {
        if (ReferenceEquals(local, Local))
        {
            return this;
        }
        
        return Rewrite(local);
    }
}

partial class ImportExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt1(Source, Options);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitImportExpression(this);
    
    public ImportExpression UpdateWith(Acornima.Ast.Expression source, Acornima.Ast.Expression? options)
    {
        if (ReferenceEquals(source, Source) && ReferenceEquals(options, Options))
        {
            return this;
        }
        
        return Rewrite(source, options);
    }
}

partial class ImportNamespaceSpecifier
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Local);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitImportNamespaceSpecifier(this);
    
    public ImportNamespaceSpecifier UpdateWith(Acornima.Ast.Identifier local)
    {
        if (ReferenceEquals(local, Local))
        {
            return this;
        }
        
        return Rewrite(local);
    }
}

partial class ImportSpecifier
{
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitImportSpecifier(this);
    
    public ImportSpecifier UpdateWith(Acornima.Ast.Expression imported, Acornima.Ast.Identifier local)
    {
        if (ReferenceEquals(imported, Imported) && ReferenceEquals(local, Local))
        {
            return this;
        }
        
        return Rewrite(imported, local);
    }
}

partial class LabeledStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Label, Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitLabeledStatement(this);
    
    public LabeledStatement UpdateWith(Acornima.Ast.Identifier label, Acornima.Ast.Statement body)
    {
        if (ReferenceEquals(label, Label) && ReferenceEquals(body, Body))
        {
            return this;
        }
        
        return Rewrite(label, body);
    }
}

partial class Literal
{
    internal sealed override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal sealed override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitLiteral(this);
}

partial class MemberExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Object, Property);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitMemberExpression(this);
    
    public MemberExpression UpdateWith(Acornima.Ast.Expression @object, Acornima.Ast.Expression property)
    {
        if (ReferenceEquals(@object, Object) && ReferenceEquals(property, Property))
        {
            return this;
        }
        
        return Rewrite(@object, property);
    }
}

partial class MetaProperty
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Meta, Property);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitMetaProperty(this);
    
    public MetaProperty UpdateWith(Acornima.Ast.Identifier meta, Acornima.Ast.Identifier property)
    {
        if (ReferenceEquals(meta, Meta) && ReferenceEquals(property, Property))
        {
            return this;
        }
        
        return Rewrite(meta, property);
    }
}

partial class MethodDefinition
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Decorators, Key, Value);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitMethodDefinition(this);
    
    public MethodDefinition UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Decorator> decorators, Acornima.Ast.Expression key, Acornima.Ast.FunctionExpression value)
    {
        if (decorators.IsSameAs(Decorators) && ReferenceEquals(key, Key) && ReferenceEquals(value, Value))
        {
            return this;
        }
        
        return Rewrite(decorators, key, value);
    }
}

partial class NewExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Callee, Arguments);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitNewExpression(this);
    
    public NewExpression UpdateWith(Acornima.Ast.Expression callee, in Acornima.Ast.NodeList<Acornima.Ast.Expression> arguments)
    {
        if (ReferenceEquals(callee, Callee) && arguments.IsSameAs(Arguments))
        {
            return this;
        }
        
        return Rewrite(callee, arguments);
    }
}

partial class ObjectExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Properties);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitObjectExpression(this);
    
    public ObjectExpression UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Node> properties)
    {
        if (properties.IsSameAs(Properties))
        {
            return this;
        }
        
        return Rewrite(properties);
    }
}

partial class ObjectPattern
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Properties);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitObjectPattern(this);
    
    public ObjectPattern UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Node> properties)
    {
        if (properties.IsSameAs(Properties))
        {
            return this;
        }
        
        return Rewrite(properties);
    }
}

partial class ObjectProperty
{
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitObjectProperty(this);
    
    public ObjectProperty UpdateWith(Acornima.Ast.Expression key, Acornima.Ast.Node value)
    {
        if (ReferenceEquals(key, Key) && ReferenceEquals(value, Value))
        {
            return this;
        }
        
        return Rewrite(key, value);
    }
}

partial class ParenthesizedExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Expression);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitParenthesizedExpression(this);
    
    public ParenthesizedExpression UpdateWith(Acornima.Ast.Expression expression)
    {
        if (ReferenceEquals(expression, Expression))
        {
            return this;
        }
        
        return Rewrite(expression);
    }
}

partial class PrivateIdentifier
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitPrivateIdentifier(this);
}

partial class Program
{
    internal sealed override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Body);
    
    protected internal sealed override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitProgram(this);
    
    public Program UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Statement> body)
    {
        if (body.IsSameAs(Body))
        {
            return this;
        }
        
        return Rewrite(body);
    }
}

partial class PropertyDefinition
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt2(Decorators, Key, Value);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitPropertyDefinition(this);
    
    public PropertyDefinition UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Decorator> decorators, Acornima.Ast.Expression key, Acornima.Ast.Expression? value)
    {
        if (decorators.IsSameAs(Decorators) && ReferenceEquals(key, Key) && ReferenceEquals(value, Value))
        {
            return this;
        }
        
        return Rewrite(decorators, key, value);
    }
}

partial class RestElement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitRestElement(this);
    
    public RestElement UpdateWith(Acornima.Ast.Node argument)
    {
        if (ReferenceEquals(argument, Argument))
        {
            return this;
        }
        
        return Rewrite(argument);
    }
}

partial class ReturnStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullable(Argument);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitReturnStatement(this);
    
    public ReturnStatement UpdateWith(Acornima.Ast.Expression? argument)
    {
        if (ReferenceEquals(argument, Argument))
        {
            return this;
        }
        
        return Rewrite(argument);
    }
}

partial class SequenceExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Expressions);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitSequenceExpression(this);
    
    public SequenceExpression UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.Expression> expressions)
    {
        if (expressions.IsSameAs(Expressions))
        {
            return this;
        }
        
        return Rewrite(expressions);
    }
}

partial class SpreadElement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitSpreadElement(this);
    
    public SpreadElement UpdateWith(Acornima.Ast.Expression argument)
    {
        if (ReferenceEquals(argument, Argument))
        {
            return this;
        }
        
        return Rewrite(argument);
    }
}

partial class StaticBlock
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitStaticBlock(this);
}

partial class Super
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitSuper(this);
}

partial class SwitchCase
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt0(Test, Consequent);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitSwitchCase(this);
    
    public SwitchCase UpdateWith(Acornima.Ast.Expression? test, in Acornima.Ast.NodeList<Acornima.Ast.Statement> consequent)
    {
        if (ReferenceEquals(test, Test) && consequent.IsSameAs(Consequent))
        {
            return this;
        }
        
        return Rewrite(test, consequent);
    }
}

partial class SwitchStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Discriminant, Cases);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitSwitchStatement(this);
    
    public SwitchStatement UpdateWith(Acornima.Ast.Expression discriminant, in Acornima.Ast.NodeList<Acornima.Ast.SwitchCase> cases)
    {
        if (ReferenceEquals(discriminant, Discriminant) && cases.IsSameAs(Cases))
        {
            return this;
        }
        
        return Rewrite(discriminant, cases);
    }
}

partial class TaggedTemplateExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Tag, Quasi);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitTaggedTemplateExpression(this);
    
    public TaggedTemplateExpression UpdateWith(Acornima.Ast.Expression tag, Acornima.Ast.TemplateLiteral quasi)
    {
        if (ReferenceEquals(tag, Tag) && ReferenceEquals(quasi, Quasi))
        {
            return this;
        }
        
        return Rewrite(tag, quasi);
    }
}

partial class TemplateElement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitTemplateElement(this);
}

partial class TemplateLiteral
{
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitTemplateLiteral(this);
    
    public TemplateLiteral UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.TemplateElement> quasis, in Acornima.Ast.NodeList<Acornima.Ast.Expression> expressions)
    {
        if (quasis.IsSameAs(Quasis) && expressions.IsSameAs(Expressions))
        {
            return this;
        }
        
        return Rewrite(quasis, expressions);
    }
}

partial class ThisExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitThisExpression(this);
}

partial class ThrowStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitThrowStatement(this);
    
    public ThrowStatement UpdateWith(Acornima.Ast.Expression argument)
    {
        if (ReferenceEquals(argument, Argument))
        {
            return this;
        }
        
        return Rewrite(argument);
    }
}

partial class TryStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt1_2(Block, Handler, Finalizer);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitTryStatement(this);
    
    public TryStatement UpdateWith(Acornima.Ast.NestedBlockStatement block, Acornima.Ast.CatchClause? handler, Acornima.Ast.NestedBlockStatement? finalizer)
    {
        if (ReferenceEquals(block, Block) && ReferenceEquals(handler, Handler) && ReferenceEquals(finalizer, Finalizer))
        {
            return this;
        }
        
        return Rewrite(block, handler, finalizer);
    }
}

partial class UnaryExpression
{
    internal sealed override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);
    
    protected internal sealed override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitUnaryExpression(this);
    
    public UnaryExpression UpdateWith(Acornima.Ast.Expression argument)
    {
        if (ReferenceEquals(argument, Argument))
        {
            return this;
        }
        
        return Rewrite(argument);
    }
}

partial class VariableDeclaration
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Declarations);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitVariableDeclaration(this);
    
    public VariableDeclaration UpdateWith(in Acornima.Ast.NodeList<Acornima.Ast.VariableDeclarator> declarations)
    {
        if (declarations.IsSameAs(Declarations))
        {
            return this;
        }
        
        return Rewrite(declarations);
    }
}

partial class VariableDeclarator
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt1(Id, Init);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitVariableDeclarator(this);
    
    public VariableDeclarator UpdateWith(Acornima.Ast.Node id, Acornima.Ast.Expression? init)
    {
        if (ReferenceEquals(id, Id) && ReferenceEquals(init, Init))
        {
            return this;
        }
        
        return Rewrite(id, init);
    }
}

partial class WhileStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Test, Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitWhileStatement(this);
    
    public WhileStatement UpdateWith(Acornima.Ast.Expression test, Acornima.Ast.Statement body)
    {
        if (ReferenceEquals(test, Test) && ReferenceEquals(body, Body))
        {
            return this;
        }
        
        return Rewrite(test, body);
    }
}

partial class WithStatement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Object, Body);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitWithStatement(this);
    
    public WithStatement UpdateWith(Acornima.Ast.Expression @object, Acornima.Ast.Statement body)
    {
        if (ReferenceEquals(@object, Object) && ReferenceEquals(body, Body))
        {
            return this;
        }
        
        return Rewrite(@object, body);
    }
}

partial class YieldExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullable(Argument);
    
    protected internal override object? Accept(Acornima.AstVisitor visitor) => visitor.VisitYieldExpression(this);
    
    public YieldExpression UpdateWith(Acornima.Ast.Expression? argument)
    {
        if (ReferenceEquals(argument, Argument))
        {
            return this;
        }
        
        return Rewrite(argument);
    }
}
