using Acornima.Ast;
using Acornima.Jsx.Ast;

namespace Acornima.Jsx;

using static JavaScriptTextWriter;

// JSX spec: https://facebook.github.io/jsx/
public class JsxAstToJavaScriptConverter : AstToJavaScriptConverter, IJsxAstVisitor
{
    public JsxAstToJavaScriptConverter(JavaScriptTextWriter writer, JsxAstToJavaScriptOptions options)
        : base(writer, options) { }

    object? IJsxAstVisitor.VisitJsxAttribute(JsxAttribute node)
    {
        Writer.SpaceRecommendedAfterLastToken();

        WriteContext.SetNodeProperty(nameof(node.Name), static node => node.As<JsxAttribute>().Name);
        VisitAuxiliaryNode(node.Name);

        if (node.Value is not null)
        {
            WriteContext.ClearNodeProperty();
            Writer.WritePunctuator("=", TokenFlags.InBetween, ref WriteContext);

            WriteContext.SetNodeProperty(nameof(node.Value), static node => node.As<JsxAttribute>().Value);
            VisitAuxiliaryNode(node.Value);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxClosingElement(JsxClosingElement node)
    {
        Writer.WritePunctuator("<", TokenFlags.Leading, ref WriteContext);
        Writer.WritePunctuator("/", ref WriteContext);

        WriteContext.SetNodeProperty(nameof(node.Name), static node => node.As<JsxClosingElement>().Name);
        VisitAuxiliaryNode(node.Name);

        WriteContext.ClearNodeProperty();
        Writer.WritePunctuator(">", TokenFlags.Trailing, ref WriteContext);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxClosingFragment(JsxClosingFragment node)
    {
        Writer.WritePunctuator("<", TokenFlags.Leading, ref WriteContext);
        Writer.WritePunctuator("/", ref WriteContext);
        Writer.WritePunctuator(">", TokenFlags.Trailing, ref WriteContext);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxElement(JsxElement node)
    {
        WriteContext.SetNodeProperty(nameof(node.OpeningElement), static node => node.As<JsxElement>().OpeningElement);
        VisitAuxiliaryNode(node.OpeningElement);

        WriteContext.SetNodeProperty(nameof(node.Children), static node => ref node.As<JsxElement>().Children);
        VisitAuxiliaryNodeList(in node.Children, separator: string.Empty);

        if (node.ClosingElement is not null)
        {
            WriteContext.SetNodeProperty(nameof(node.ClosingElement), static node => node.As<JsxElement>().ClosingElement);
            VisitAuxiliaryNode(node.ClosingElement);
        }

        return node;
    }

    object? IJsxAstVisitor.VisitJsxEmptyExpression(JsxEmptyExpression node)
    {
        return node;
    }

    object? IJsxAstVisitor.VisitJsxExpressionContainer(JsxExpressionContainer node)
    {
        Writer.WritePunctuator("{", TokenFlags.Leading, ref WriteContext);

        WriteContext.SetNodeProperty(nameof(node.Expression), static node => node.As<JsxExpressionContainer>().Expression);
        VisitRootExpression(node.Expression, RootExpressionFlags(needsParens: false));

        WriteContext.ClearNodeProperty();
        Writer.WritePunctuator("}", TokenFlags.Trailing, ref WriteContext);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxFragment(JsxFragment node)
    {
        WriteContext.SetNodeProperty(nameof(node.OpeningFragment), static node => node.As<JsxFragment>().OpeningFragment);
        VisitAuxiliaryNode(node.OpeningFragment);

        WriteContext.SetNodeProperty(nameof(node.Children), static node => ref node.As<JsxFragment>().Children);
        VisitAuxiliaryNodeList(in node.Children, separator: string.Empty);

        WriteContext.SetNodeProperty(nameof(node.ClosingFragment), static node => node.As<JsxFragment>().ClosingFragment);
        VisitAuxiliaryNode(node.ClosingFragment);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxIdentifier(JsxIdentifier node)
    {
        WriteContext.SetNodeProperty(nameof(node.Name), static node => node.As<JsxIdentifier>().Name);
        Writer.WriteIdentifier(node.Name, ref WriteContext);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxMemberExpression(JsxMemberExpression node)
    {
        WriteContext.SetNodeProperty(nameof(node.Object), static node => node.As<JsxMemberExpression>().Object);
        VisitAuxiliaryNode(node.Object);

        WriteContext.ClearNodeProperty();
        Writer.WritePunctuator(".", TokenFlags.InBetween, ref WriteContext);

        WriteContext.SetNodeProperty(nameof(node.Property), static node => node.As<JsxMemberExpression>().Property);
        VisitAuxiliaryNode(node.Property);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxNamespacedName(JsxNamespacedName node)
    {
        WriteContext.SetNodeProperty(nameof(node.Namespace), static node => node.As<JsxNamespacedName>().Namespace);
        VisitAuxiliaryNode(node.Namespace);

        WriteContext.ClearNodeProperty();
        Writer.WritePunctuator(":", TokenFlags.InBetween, ref WriteContext);

        WriteContext.SetNodeProperty(nameof(node.Name), static node => node.As<JsxNamespacedName>().Name);
        VisitAuxiliaryNode(node.Name);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxOpeningElement(JsxOpeningElement node)
    {
        Writer.WritePunctuator("<", TokenFlags.Leading, ref WriteContext);

        WriteContext.SetNodeProperty(nameof(node.Name), static node => node.As<JsxOpeningElement>().Name);
        VisitAuxiliaryNode(node.Name);

        WriteContext.SetNodeProperty(nameof(node.Attributes), static node => ref node.As<JsxOpeningElement>().Attributes);
        VisitAuxiliaryNodeList(in node.Attributes, separator: string.Empty);

        if (node.SelfClosing)
        {
            WriteContext.SetNodeProperty(nameof(node.SelfClosing), static node => node.As<JsxOpeningElement>().SelfClosing);
            Writer.WritePunctuator("/", TokenFlags.LeadingSpaceRecommended, ref WriteContext);
        }
        WriteContext.ClearNodeProperty();
        Writer.WritePunctuator(">", TokenFlags.Trailing, ref WriteContext);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxOpeningFragment(JsxOpeningFragment node)
    {
        Writer.WritePunctuator("<", TokenFlags.Leading, ref WriteContext);
        Writer.WritePunctuator(">", TokenFlags.Trailing, ref WriteContext);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxSpreadAttribute(JsxSpreadAttribute node)
    {
        Writer.WritePunctuator("{", TokenFlags.Leading | TokenFlags.LeadingSpaceRecommended, ref WriteContext);

        var argumentNeedsBrackets = UnaryOperandNeedsParens(node, node.Argument);

        WriteContext.SetNodeProperty(nameof(node.Argument), static node => node.As<JsxSpreadAttribute>().Argument);
        Writer.WritePunctuator("...", TokenFlags.Leading, ref WriteContext);

        VisitRootExpression(node.Argument, RootExpressionFlags(argumentNeedsBrackets));

        WriteContext.ClearNodeProperty();
        Writer.WritePunctuator("}", TokenFlags.Trailing, ref WriteContext);

        return node;
    }

    object? IJsxAstVisitor.VisitJsxText(JsxText node)
    {
        Writer.WriteLiteral(node.Raw, TokenKind.Extension, ref WriteContext);

        return node;
    }

    protected override int GetOperatorPrecedence(Expression expression, out int associativity)
    {
        if (expression.Type == NodeType.Extension && expression is JsxNode jsxNode)
        {
            const int undefinedAssociativity = 0;

            switch (jsxNode.Type)
            {
                case JsxNodeType.Element:
                    associativity = undefinedAssociativity;
                    return int.MaxValue;
                case JsxNodeType.SpreadAttribute:
                    associativity = undefinedAssociativity;
                    return 200;
            }
        }

        return base.GetOperatorPrecedence(expression, out associativity);
    }
}
