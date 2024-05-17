using System.Collections.Generic;
using Acornima;
using Acornima.Ast;
using Acornima.Jsx;
using Acornima.Jsx.Ast;
using Acornima.Tests.Helpers;

namespace JsxTranspiler;

// This approach to transpiling JSX is far from optimal, it is just a demonstration of
// Acornima's AST transformation and code generation capabilities.

internal class JsxToJavaScriptTranspiler : JsxAstRewriter
{
    private const string ElementConstructorName = "__$El";
    private const string WrapSpreadAttributeFunctionName = "__$spreadAttrs";
    private const string BuildAttributeMapFunctionName = "__$buildAttrs";

    private readonly NullLiteral _nullLiteral;
    private readonly BooleanLiteral _falseLiteral;
    private readonly BooleanLiteral _trueLiteral;

    private readonly Identifier _elementConstructorId;
    private readonly Identifier _wrapSpreadAttributeFunctionId;
    private readonly Identifier _buildAttributeMapFunctionId;

    private bool _needsElementConstructor;
    private bool _needsWrapSpreadAttributeFunction;
    private bool _needsBuildAttributeMapFunction;

    private bool _isStrict;

    public JsxToJavaScriptTranspiler()
    {
        _nullLiteral = new NullLiteral("null");
        _falseLiteral = new BooleanLiteral(false, "false");
        _trueLiteral = new BooleanLiteral(true, "true");

        _elementConstructorId = new Identifier(ElementConstructorName);
        _wrapSpreadAttributeFunctionId = new Identifier(WrapSpreadAttributeFunctionName);
        _buildAttributeMapFunctionId = new Identifier(BuildAttributeMapFunctionName);
    }

    public Acornima.Ast.Program Transpile(Acornima.Ast.Program root)
    {
        _needsElementConstructor = _needsWrapSpreadAttributeFunction = _needsBuildAttributeMapFunction = false;
        _isStrict = false;

        return VisitAndConvert(root);
    }

    protected override object? VisitProgram(Acornima.Ast.Program node)
    {
        var oldIsStrict = _isStrict;
        _isStrict = node.Strict;
        var result = (Acornima.Ast.Program)base.VisitProgram(node)!;
        _isStrict = oldIsStrict;

        if (_needsElementConstructor)
        {
            var statements = new List<Statement>(result.Body);

            if (_needsBuildAttributeMapFunction)
            {
                statements.Add(MakeBuildAttributeMapFunction());
            }

            if (_needsWrapSpreadAttributeFunction)
            {
                statements.Add(MakeWrapSpreadAttributeFunction());
            }

            statements.Add(MakeElementConstructor());

            if (node is Module)
            {
                var elementConstructorExportSpecifier = new ExportSpecifier(_elementConstructorId, new Identifier("JSXElement"));
                statements.Add(new ExportNamedDeclaration(declaration: null, NodeList.From(elementConstructorExportSpecifier), source: null, NodeList.Empty<ImportAttribute>()));
            }

            result = result.UpdateWith(NodeList.From(statements));
        }

        return result;
    }

    protected override object? VisitFunctionBody(FunctionBody node)
    {
        var oldIsStrict = _isStrict;
        _isStrict = node.Strict;
        var result = base.VisitFunctionBody(node);
        _isStrict = oldIsStrict;
        return result;
    }

    protected override object? VisitStaticBlock(StaticBlock node)
    {
        var oldIsStrict = _isStrict;
        _isStrict = node.As<IHoistingScope>().Strict;
        var result = base.VisitStaticBlock(node);
        _isStrict = oldIsStrict;
        return result;
    }

    public override object? VisitJsxAttribute(JsxAttribute node)
    {
        var attributeName = MakeString(node.Name.GetQualifiedName());

        Expression attributeValueProvider;
        if (node.Value is null)
        {
            attributeValueProvider = _trueLiteral;
        }
        else
        {
            var attributeValue = VisitAndConvert(node.Value);
            attributeValueProvider = new ArrowFunctionExpression(NodeList.Empty<Node>(), attributeValue, expression: true, async: false);
        }

        return new ObjectProperty(PropertyKind.Init, attributeName, attributeValueProvider, computed: false, shorthand: false, method: false);
    }

    public override object? VisitJsxElement(JsxElement node)
    {
        _needsElementConstructor = true;

        var elementNameArg = MakeString(node.OpeningElement.Name.GetQualifiedName());

        Expression attributesArg;
        if (node.OpeningElement.Attributes.Count > 0)
        {
            _needsBuildAttributeMapFunction = true;

            var properties = new List<Node>(capacity: node.OpeningElement.Attributes.Count);
            foreach (var attribute in node.OpeningElement.Attributes)
            {
                properties.Add((Node)Visit(attribute)!);
            }

            var attributeMap = new ObjectExpression(NodeList.From(properties));
            attributesArg = new CallExpression(_buildAttributeMapFunctionId, NodeList.From<Expression>(attributeMap), optional: false);
        }
        else
        {
            attributesArg = _nullLiteral;
        }

        var childrenArg = node.OpeningElement.SelfClosing
            ? _falseLiteral
            : VisitJsxElementChildren(node.Children);

        return new NewExpression(_elementConstructorId, NodeList.From(elementNameArg, attributesArg, childrenArg));
    }

    private Expression VisitJsxElementChildren(in NodeList<JsxNode> children)
    {
        if (children.Count > 0)
        {
            var elements = new List<Expression?>(capacity: children.Count);

            foreach (var child in children)
            {
                var element = (Expression)Visit(child)!;
                if (element is not JsxEmptyExpression)
                {
                    elements.Add(element);
                }
            }

            return new ArrayExpression(NodeList.From(elements));
        }
        else
        {
            return _nullLiteral;
        }
    }

    public override object? VisitJsxFragment(JsxFragment node)
    {
        _needsElementConstructor = true;

        var elementNameArg = _nullLiteral;
        var childrenArg = VisitJsxElementChildren(node.Children);
        return new NewExpression(_elementConstructorId, NodeList.From(elementNameArg, _nullLiteral, childrenArg));
    }

    public override object? VisitJsxExpressionContainer(JsxExpressionContainer node)
    {
        return VisitAndConvert(node.Expression);
    }

    public override object? VisitJsxSpreadAttribute(JsxSpreadAttribute node)
    {
        _needsWrapSpreadAttributeFunction = true;

        var wrapCall = new CallExpression(_wrapSpreadAttributeFunctionId, NodeList.From(node.Argument), optional: false);
        return new SpreadElement(wrapCall);
    }

    public override object? VisitJsxText(JsxText node)
    {
        return MakeString(node.Value);
    }

    private static StringLiteral MakeString(string unencodedText)
    {
        return new StringLiteral(unencodedText, JavaScriptString.Encode(unencodedText, addDoubleQuotes: true));
    }

    private static FunctionDeclaration MakeElementConstructor()
    {
        var function = new Parser().ParseScript(
            $$"""
            function {{ElementConstructorName}}(name, attrs, children) {
                function htmlEncDefault(value) {
                    const escapeMap = {
                        "<": "&lt;",
                        ">": "&gt;",
                        "&": "&amp;",
                        '"': "&quot;",
                        "'": "&#39;",
                        "`": "&#96;",
                    };
                    return value.replace(/[<>&"'`]/g, ch => escapeMap[ch]);
                }

                function* flatMap(array) {
                    for (const item of array) {
                        if (Symbol.iterator in Object(item)) yield* item;
                        else yield item;
                    }
                }

                this.render = (format = true, write, htmlEnc, attrEnc, indent = '') => {
                    if (!write) {
                        let s = "";
                        write = content => s += content;
                    }

                    let incIndent, decIndent, newLine;
                    if (format) {
                        incIndent = () => indent += "  ";
                        decIndent = () => indent = indent.slice(0, -2);
                        newLine = () => write('\n');
                    }
                    else incIndent = decIndent = newLine = () => { };

                    if (!htmlEnc) htmlEnc = htmlEncDefault;

                    if (!attrEnc) attrEnc = htmlEnc;

                    if (name) {
                        write(indent), write(`<${name}`);
                        if (attrs != null) {
                            for (const key of Object.keys(attrs)) {
                                let value = attrs[key];
                                if (value == null) {
                                    write(` ${key}`);
                                }
                                else {
                                    write(` ${key}="`);
                                    if (value instanceof {{ElementConstructorName}}) {
                                        value = value.render(false, null, htmlEnc, attrEnc);
                                    }
                                    write(attrEnc(value + ""));
                                    write('"');
                                }
                            }
                        }

                        if (children === false) return write("/>");

                        write(">");
                    }

                    if (children != null && children.length) {
                        let prevNewLine = !name, prevElement = !prevNewLine;
                        if (name) incIndent();

                        for (const child of flatMap(children)) {
                            if (child instanceof {{ElementConstructorName}}) {
                                if (!prevNewLine) newLine();
                                child.render(format, write, htmlEnc, attrEnc, indent);
                                prevElement = true, prevNewLine = false;
                            }
                            else {
                                let content = htmlEnc(child + "");
                                if (prevElement && format) {
                                    newLine(), write(indent);
                                    content = content.trimStart();
                                }
                                write(content);
                                prevElement = false, prevNewLine = content.charCodeAt(content.length - 1) == 10;
                            }
                        }

                        if (name) {
                            if (!prevNewLine) newLine();
                            decIndent();
                        }
                    }

                    return name
                        ? (write(indent), write(`</${name}>`))
                        : write("");
                };
            }
            """).Body[0].As<FunctionDeclaration>();
        return function;
    }

    private static FunctionDeclaration MakeWrapSpreadAttributeFunction()
    {
        var function = new Parser().ParseScript(
            $$"""
            function {{WrapSpreadAttributeFunctionName}}(attrs) {
                const wrappedAttrs = {};
                for (const key of Object.keys(attrs)) {
                    const value = attrs[key];
                    if (typeof value === "boolean") wrappedAttrs[key] = value;
                    else wrappedAttrs[key] = () => value;
                }
                return wrappedAttrs;
            }
            """).Body[0].As<FunctionDeclaration>();
        return function;
    }

    private static FunctionDeclaration MakeBuildAttributeMapFunction()
    {
        var function = new Parser().ParseScript(
            $$"""
            function {{BuildAttributeMapFunctionName}}(attrs) {
                for (const key of Object.keys(attrs)) {
                    let value;
                    const valueProvider = attrs[key];
                    if (valueProvider === true) attrs[key] = null;
                    else if (typeof valueProvider !== "boolean" && (value = valueProvider()) != null) attrs[key] = value;
                    else delete attrs[key];
                }
                return attrs;
            }
            """).Body[0].As<FunctionDeclaration>();
        return function;
    }
}
