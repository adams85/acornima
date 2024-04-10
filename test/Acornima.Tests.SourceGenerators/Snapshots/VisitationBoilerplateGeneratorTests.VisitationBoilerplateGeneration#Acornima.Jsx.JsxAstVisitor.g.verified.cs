//HintName: Acornima.Jsx.JsxAstVisitor.g.cs
#nullable enable

namespace Acornima.Jsx;

partial class JsxAstVisitor
{
    public virtual object? VisitJsxAttribute(Acornima.Jsx.Ast.JsxAttribute node)
    {
        _visitor.Visit(node.Name);
        
        if (node.Value is not null)
        {
            _visitor.Visit(node.Value);
        }
        
        return node;
    }
    
    public virtual object? VisitJsxClosingElement(Acornima.Jsx.Ast.JsxClosingElement node)
    {
        _visitor.Visit(node.Name);
        
        return node;
    }
    
    public virtual object? VisitJsxClosingFragment(Acornima.Jsx.Ast.JsxClosingFragment node)
    {
        return node;
    }
    
    public virtual object? VisitJsxElement(Acornima.Jsx.Ast.JsxElement node)
    {
        _visitor.Visit(node.OpeningElement);
        
        ref readonly var children = ref node.Children;
        for (var i = 0; i < children.Count; i++)
        {
            _visitor.Visit(children[i]);
        }
        
        if (node.ClosingElement is not null)
        {
            _visitor.Visit(node.ClosingElement);
        }
        
        return node;
    }
    
    public virtual object? VisitJsxEmptyExpression(Acornima.Jsx.Ast.JsxEmptyExpression node)
    {
        return node;
    }
    
    public virtual object? VisitJsxExpressionContainer(Acornima.Jsx.Ast.JsxExpressionContainer node)
    {
        _visitor.Visit(node.Expression);
        
        return node;
    }
    
    public virtual object? VisitJsxFragment(Acornima.Jsx.Ast.JsxFragment node)
    {
        _visitor.Visit(node.OpeningFragment);
        
        ref readonly var children = ref node.Children;
        for (var i = 0; i < children.Count; i++)
        {
            _visitor.Visit(children[i]);
        }
        
        _visitor.Visit(node.ClosingFragment);
        
        return node;
    }
    
    public virtual object? VisitJsxIdentifier(Acornima.Jsx.Ast.JsxIdentifier node)
    {
        return node;
    }
    
    public virtual object? VisitJsxMemberExpression(Acornima.Jsx.Ast.JsxMemberExpression node)
    {
        _visitor.Visit(node.Object);
        
        _visitor.Visit(node.Property);
        
        return node;
    }
    
    public virtual object? VisitJsxNamespacedName(Acornima.Jsx.Ast.JsxNamespacedName node)
    {
        _visitor.Visit(node.Name);
        
        _visitor.Visit(node.Namespace);
        
        return node;
    }
    
    public virtual object? VisitJsxOpeningElement(Acornima.Jsx.Ast.JsxOpeningElement node)
    {
        _visitor.Visit(node.Name);
        
        ref readonly var attributes = ref node.Attributes;
        for (var i = 0; i < attributes.Count; i++)
        {
            _visitor.Visit(attributes[i]);
        }
        
        return node;
    }
    
    public virtual object? VisitJsxOpeningFragment(Acornima.Jsx.Ast.JsxOpeningFragment node)
    {
        return node;
    }
    
    public virtual object? VisitJsxSpreadAttribute(Acornima.Jsx.Ast.JsxSpreadAttribute node)
    {
        _visitor.Visit(node.Argument);
        
        return node;
    }
    
    public virtual object? VisitJsxText(Acornima.Jsx.Ast.JsxText node)
    {
        return node;
    }
}
