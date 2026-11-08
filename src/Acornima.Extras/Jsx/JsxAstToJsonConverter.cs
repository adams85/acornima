using Acornima.Jsx.Ast;

namespace Acornima.Jsx;

public class JsxAstToJsonConverter : AstToJsonConverter, IJsxAstVisitor
{
    public JsxAstToJsonConverter(JsonWriter writer, JsxAstToJsonOptions options)
        : base(writer, options) { }

    object? IJsxAstVisitor.VisitJsxAttribute(JsxAttribute node)
    {
        StartNodeObject(node);
        Member("name", node.Name);
        Member("value", node.Value);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxClosingElement(JsxClosingElement node)
    {
        StartNodeObject(node);
        Member("name", node.Name);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxClosingFragment(JsxClosingFragment node)
    {
        StartNodeObject(node);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxElement(JsxElement node)
    {
        StartNodeObject(node);
        Member("openingElement", node.OpeningElement);
        Member("children", node.Children);
        Member("closingElement", node.ClosingElement);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxEmptyExpression(JsxEmptyExpression node)
    {
        StartNodeObject(node);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxExpressionContainer(JsxExpressionContainer node)
    {
        StartNodeObject(node);
        Member("expression", node.Expression);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxFragment(JsxFragment node)
    {
        StartNodeObject(node);
        Member("openingFragment", node.OpeningFragment);
        Member("children", node.Children);
        Member("closingFragment", node.ClosingFragment);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxIdentifier(JsxIdentifier node)
    {
        StartNodeObject(node);
        Member("name", node.Name);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxMemberExpression(JsxMemberExpression node)
    {
        StartNodeObject(node);
        Member("object", node.Object);
        Member("property", node.Property);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxNamespacedName(JsxNamespacedName node)
    {
        StartNodeObject(node);
        Member("namespace", node.Namespace);
        Member("name", node.Name);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxOpeningElement(JsxOpeningElement node)
    {
        StartNodeObject(node);
        Member("name", node.Name);
        Member("attributes", node.Attributes);
        Member("selfClosing", node.SelfClosing);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxOpeningFragment(JsxOpeningFragment node)
    {
        StartNodeObject(node);
        Member("selfClosing", node.SelfClosing);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxSpreadAttribute(JsxSpreadAttribute node)
    {
        StartNodeObject(node);
        Member("argument", node.Argument);
        EndNodeObject(node);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxText(JsxText node)
    {
        StartNodeObject(node);
        Member("value", node.Value);
        Member("raw", node.Raw);
        EndNodeObject(node);

        return node;
    }
}
