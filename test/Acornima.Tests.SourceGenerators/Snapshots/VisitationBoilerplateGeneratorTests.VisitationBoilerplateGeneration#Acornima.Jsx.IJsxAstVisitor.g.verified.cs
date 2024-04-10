//HintName: Acornima.Jsx.IJsxAstVisitor.g.cs
#nullable enable

namespace Acornima.Jsx;

partial interface IJsxAstVisitor
{
    object? VisitJsxAttribute(Acornima.Jsx.Ast.JsxAttribute node);
    
    object? VisitJsxClosingElement(Acornima.Jsx.Ast.JsxClosingElement node);
    
    object? VisitJsxClosingFragment(Acornima.Jsx.Ast.JsxClosingFragment node);
    
    object? VisitJsxElement(Acornima.Jsx.Ast.JsxElement node);
    
    object? VisitJsxEmptyExpression(Acornima.Jsx.Ast.JsxEmptyExpression node);
    
    object? VisitJsxExpressionContainer(Acornima.Jsx.Ast.JsxExpressionContainer node);
    
    object? VisitJsxFragment(Acornima.Jsx.Ast.JsxFragment node);
    
    object? VisitJsxIdentifier(Acornima.Jsx.Ast.JsxIdentifier node);
    
    object? VisitJsxMemberExpression(Acornima.Jsx.Ast.JsxMemberExpression node);
    
    object? VisitJsxNamespacedName(Acornima.Jsx.Ast.JsxNamespacedName node);
    
    object? VisitJsxOpeningElement(Acornima.Jsx.Ast.JsxOpeningElement node);
    
    object? VisitJsxOpeningFragment(Acornima.Jsx.Ast.JsxOpeningFragment node);
    
    object? VisitJsxSpreadAttribute(Acornima.Jsx.Ast.JsxSpreadAttribute node);
    
    object? VisitJsxText(Acornima.Jsx.Ast.JsxText node);
}
