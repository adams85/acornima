//HintName: Acornima.Jsx.Ast.VisitableNodes.g.cs
#nullable enable

namespace Acornima.Jsx.Ast;

partial class JsxAttribute
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt1(Name, Value);
    
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxAttribute(this);
    
    public JsxAttribute UpdateWith(Acornima.Jsx.Ast.JsxName name, Acornima.Ast.Expression? value)
    {
        if (ReferenceEquals(name, Name) && ReferenceEquals(value, Value))
        {
            return this;
        }
        
        return Rewrite(name, value);
    }
}

partial class JsxClosingElement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Name);
    
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxClosingElement(this);
    
    public JsxClosingElement UpdateWith(Acornima.Jsx.Ast.JsxName name)
    {
        if (ReferenceEquals(name, Name))
        {
            return this;
        }
        
        return Rewrite(name);
    }
}

partial class JsxClosingFragment
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxClosingFragment(this);
}

partial class JsxElement
{
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxElement(this);
    
    public JsxElement UpdateWith(Acornima.Jsx.Ast.JsxOpeningElement openingElement, in Acornima.Ast.NodeList<Acornima.Jsx.Ast.JsxNode> children, Acornima.Jsx.Ast.JsxClosingElement? closingElement)
    {
        if (ReferenceEquals(openingElement, OpeningElement) && children.IsSameAs(Children) && ReferenceEquals(closingElement, ClosingElement))
        {
            return this;
        }
        
        return Rewrite(openingElement, children, closingElement);
    }
}

partial class JsxEmptyExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxEmptyExpression(this);
}

partial class JsxExpressionContainer
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Expression);
    
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxExpressionContainer(this);
    
    public JsxExpressionContainer UpdateWith(Acornima.Ast.Expression expression)
    {
        if (ReferenceEquals(expression, Expression))
        {
            return this;
        }
        
        return Rewrite(expression);
    }
}

partial class JsxFragment
{
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxFragment(this);
    
    public JsxFragment UpdateWith(Acornima.Jsx.Ast.JsxOpeningFragment openingFragment, in Acornima.Ast.NodeList<Acornima.Jsx.Ast.JsxNode> children, Acornima.Jsx.Ast.JsxClosingFragment closingFragment)
    {
        if (ReferenceEquals(openingFragment, OpeningFragment) && children.IsSameAs(Children) && ReferenceEquals(closingFragment, ClosingFragment))
        {
            return this;
        }
        
        return Rewrite(openingFragment, children, closingFragment);
    }
}

partial class JsxIdentifier
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxIdentifier(this);
}

partial class JsxMemberExpression
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Object, Property);
    
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxMemberExpression(this);
    
    public JsxMemberExpression UpdateWith(Acornima.Jsx.Ast.JsxName @object, Acornima.Jsx.Ast.JsxIdentifier property)
    {
        if (ReferenceEquals(@object, Object) && ReferenceEquals(property, Property))
        {
            return this;
        }
        
        return Rewrite(@object, property);
    }
}

partial class JsxNamespacedName
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Name, Namespace);
    
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxNamespacedName(this);
    
    public JsxNamespacedName UpdateWith(Acornima.Jsx.Ast.JsxIdentifier name, Acornima.Jsx.Ast.JsxIdentifier @namespace)
    {
        if (ReferenceEquals(name, Name) && ReferenceEquals(@namespace, Namespace))
        {
            return this;
        }
        
        return Rewrite(name, @namespace);
    }
}

partial class JsxOpeningElement
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Name, Attributes);
    
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxOpeningElement(this);
    
    public JsxOpeningElement UpdateWith(Acornima.Jsx.Ast.JsxName name, in Acornima.Ast.NodeList<Acornima.Jsx.Ast.JsxAttributeLike> attributes)
    {
        if (ReferenceEquals(name, Name) && attributes.IsSameAs(Attributes))
        {
            return this;
        }
        
        return Rewrite(name, attributes);
    }
}

partial class JsxOpeningFragment
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxOpeningFragment(this);
}

partial class JsxSpreadAttribute
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);
    
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxSpreadAttribute(this);
    
    public JsxSpreadAttribute UpdateWith(Acornima.Ast.Expression argument)
    {
        if (ReferenceEquals(argument, Argument))
        {
            return this;
        }
        
        return Rewrite(argument);
    }
}

partial class JsxText
{
    internal override Acornima.Ast.Node? NextChildNode(ref Acornima.Ast.ChildNodes.Enumerator enumerator) => null;
    
    protected internal override object? Accept(Acornima.Jsx.IJsxAstVisitor visitor) => visitor.VisitJsxText(this);
}
