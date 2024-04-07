using Acornima.Ast;
using Acornima.Jsx.Ast;

namespace Acornima.Jsx;

public class JsxAstToJsonConverter : AstToJsonConverter, IJsxAstVisitor
{
    public JsxAstToJsonConverter(JsonWriter writer, JsxAstToJsonOptions options)
        : base(writer, options) { }

    object? IJsxAstVisitor.VisitJsxAttribute(JsxAttribute node)
    {
        using (StartNodeObject(node))
        {
            Member("name", node.Name);
            Member("value", node.Value);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxClosingElement(JsxClosingElement node)
    {
        using (StartNodeObject(node))
        {
            Member("name", node.Name);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxClosingFragment(JsxClosingFragment node)
    {
        using (StartNodeObject(node))
        {
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxElement(JsxElement node)
    {
        using (StartNodeObject(node))
        {
            Member("openingElement", node.OpeningElement);
            Member("children", node.Children);
            Member("closingElement", node.ClosingElement);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxEmptyExpression(JsxEmptyExpression node)
    {
        using (StartNodeObject(node))
        {
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxExpressionContainer(JsxExpressionContainer node)
    {
        using (StartNodeObject(node))
        {
            Member("expression", node.Expression);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxFragment(JsxFragment node)
    {
        using (StartNodeObject(node))
        {
            Member("openingFragment", node.OpeningFragment);
            Member("children", node.Children);
            Member("closingFragment", node.ClosingFragment);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxIdentifier(JsxIdentifier node)
    {
        using (StartNodeObject(node))
        {
            Member("name", node.Name);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxMemberExpression(JsxMemberExpression node)
    {
        using (StartNodeObject(node))
        {
            Member("object", node.Object);
            Member("property", node.Property);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxNamespacedName(JsxNamespacedName node)
    {
        using (StartNodeObject(node))
        {
            Member("namespace", node.Namespace);
            Member("name", node.Name);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxOpeningElement(JsxOpeningElement node)
    {
        using (StartNodeObject(node))
        {
            Member("name", node.Name);
            Member("attributes", node.Attributes);
            Member("selfClosing", node.SelfClosing);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxOpeningFragment(JsxOpeningFragment node)
    {
        using (StartNodeObject(node))
        {
            Member("selfClosing", node.SelfClosing);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxSpreadAttribute(JsxSpreadAttribute node)
    {
        using (StartNodeObject(node))
        {
            Member("argument", node.Argument);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxText(JsxText node)
    {
        using (StartNodeObject(node))
        {
            Member("value", node.Value);
            Member("raw", node.Raw);
        }

        return node;
    }
}
