//HintName: Acornima.Jsx.JsxAstRewriter.g.cs
#nullable enable

namespace Acornima.Jsx;

partial class JsxAstRewriter
{
    public virtual object? VisitJsxAttribute(Acornima.Jsx.Ast.JsxAttribute node)
    {
        var name = _rewriter.VisitAndConvert(node.Name);
        
        var value = _rewriter.VisitAndConvert(node.Value, allowNull: true);
        
        return node.UpdateWith(name, value);
    }
    
    public virtual object? VisitJsxClosingElement(Acornima.Jsx.Ast.JsxClosingElement node)
    {
        var name = _rewriter.VisitAndConvert(node.Name);
        
        return node.UpdateWith(name);
    }
    
    public virtual object? VisitJsxClosingFragment(Acornima.Jsx.Ast.JsxClosingFragment node)
    {
        return _jsxVisitor.VisitJsxClosingFragment(node);
    }
    
    public virtual object? VisitJsxElement(Acornima.Jsx.Ast.JsxElement node)
    {
        var openingElement = _rewriter.VisitAndConvert(node.OpeningElement);
        
        _rewriter.VisitAndConvert(node.Children, out var children);
        
        var closingElement = _rewriter.VisitAndConvert(node.ClosingElement, allowNull: true);
        
        return node.UpdateWith(openingElement, children, closingElement);
    }
    
    public virtual object? VisitJsxEmptyExpression(Acornima.Jsx.Ast.JsxEmptyExpression node)
    {
        return _jsxVisitor.VisitJsxEmptyExpression(node);
    }
    
    public virtual object? VisitJsxExpressionContainer(Acornima.Jsx.Ast.JsxExpressionContainer node)
    {
        var expression = _rewriter.VisitAndConvert(node.Expression);
        
        return node.UpdateWith(expression);
    }
    
    public virtual object? VisitJsxFragment(Acornima.Jsx.Ast.JsxFragment node)
    {
        var openingFragment = _rewriter.VisitAndConvert(node.OpeningFragment);
        
        _rewriter.VisitAndConvert(node.Children, out var children);
        
        var closingFragment = _rewriter.VisitAndConvert(node.ClosingFragment);
        
        return node.UpdateWith(openingFragment, children, closingFragment);
    }
    
    public virtual object? VisitJsxIdentifier(Acornima.Jsx.Ast.JsxIdentifier node)
    {
        return _jsxVisitor.VisitJsxIdentifier(node);
    }
    
    public virtual object? VisitJsxMemberExpression(Acornima.Jsx.Ast.JsxMemberExpression node)
    {
        var @object = _rewriter.VisitAndConvert(node.Object);
        
        var property = _rewriter.VisitAndConvert(node.Property);
        
        return node.UpdateWith(@object, property);
    }
    
    public virtual object? VisitJsxNamespacedName(Acornima.Jsx.Ast.JsxNamespacedName node)
    {
        var name = _rewriter.VisitAndConvert(node.Name);
        
        var @namespace = _rewriter.VisitAndConvert(node.Namespace);
        
        return node.UpdateWith(name, @namespace);
    }
    
    public virtual object? VisitJsxOpeningElement(Acornima.Jsx.Ast.JsxOpeningElement node)
    {
        var name = _rewriter.VisitAndConvert(node.Name);
        
        _rewriter.VisitAndConvert(node.Attributes, out var attributes);
        
        return node.UpdateWith(name, attributes);
    }
    
    public virtual object? VisitJsxOpeningFragment(Acornima.Jsx.Ast.JsxOpeningFragment node)
    {
        return _jsxVisitor.VisitJsxOpeningFragment(node);
    }
    
    public virtual object? VisitJsxSpreadAttribute(Acornima.Jsx.Ast.JsxSpreadAttribute node)
    {
        var argument = _rewriter.VisitAndConvert(node.Argument);
        
        return node.UpdateWith(argument);
    }
    
    public virtual object? VisitJsxText(Acornima.Jsx.Ast.JsxText node)
    {
        return _jsxVisitor.VisitJsxText(node);
    }
}
