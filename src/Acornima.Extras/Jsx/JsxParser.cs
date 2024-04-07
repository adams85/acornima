using System;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;
using Acornima.Jsx.Ast;

namespace Acornima.Jsx;

using static Unsafe;
using static Parser;
using static ExceptionHelper;
using static JsxSyntaxErrorMessages;

// https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js

public sealed class JsxParser : IParser, IExtension
{
    private readonly JsxParserOptions _options;
    private readonly Parser _parser;

    public JsxParser() : this(JsxParserOptions.Default) { }

    public JsxParser(JsxParserOptions options)
    {
        _options = options;
        _parser = new Parser(options, extension: this);
    }

    public JsxParserOptions Options => _options;
    ParserOptions IParser.Options => _options;

    Tokenizer IExtension.CreateTokenizer(TokenizerOptions tokenizerOptions) => new JsxTokenizer((JsxTokenizerOptions)tokenizerOptions)._tokenizer;

    public Script ParseScript(string input, int start, int length, string? sourceFile = null, bool strict = false)
        => _parser.ParseScript(input, start, length, sourceFile, strict);

    public Module ParseModule(string input, int start, int length, string? sourceFile = null)
        => _parser.ParseModule(input, start, length, sourceFile);

    public Expression ParseExpression(string input, int start, int length, string? sourceFile = null, bool strict = false)
        => _parser.ParseExpression(input, start, length, sourceFile, strict);

    Expression IExtension.ParseExprAtom()
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `parseExprAtom(refShortHandDefaultPos) {`

        Marker startMarker;
        ref readonly var tokenizer = ref _parser._tokenizer;
        if (tokenizer._type == JsxTokenType.Text)
        {
            startMarker = _parser.StartNode();
            return ParseText(startMarker);
        }
        else if (tokenizer._type == JsxTokenType.TagStart)
        {
            startMarker = _parser.StartNode();
            _parser.Next();
            return ParseElement(startMarker);
        }
        else
        {
            return _parser.Unexpected<Expression>();
        }
    }

    // Parse JSX text

    private JsxText ParseText(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_parseText() {`

        ref readonly var tokenizer = ref _parser._tokenizer;
        var value = JsxToken.UnwrapStringValue(tokenizer._value);
        var raw = Tokenizer.DeduplicateString(tokenizer._input.SliceBetween(tokenizer._start, tokenizer._end), ref tokenizer._stringPool, Tokenizer.NonIdentifierDeduplicationThreshold);

        _parser.Next();
        return _parser.FinishNode(startMarker, new JsxText(value, raw));
    }

    // Parses entire JSX element from current position.

    private JsxElementOrFragment ParseElement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_parseElement() {`, `jsx_parseElementAt(startPos, startLoc) {`

        // NOTE: `jsx_parseElement` and `jsx_parseElementAt` was merged into this single method to keep the call stack shallow.

        _parser.EnterRecursion();

        JsxOpeningElement? openingElement;
        var children = new ArrayList<JsxNode>();
        JsxClosingElement? closingElement;

        var openingTag = ParseOpeningElement(startMarker);
        JsxNode? closingTag = null;
        Marker childStartMarker;

        ref readonly var tokenizer = ref _parser._tokenizer;
        if (!openingTag.SelfClosing)
        {
            for (; ; )
            {
                childStartMarker = _parser.StartNode();
                if (tokenizer._type == JsxTokenType.TagStart)
                {
                    _parser.Next();
                    if (_parser.Eat(TokenType.Slash))
                    {
                        closingTag = ParseClosingElement(childStartMarker);
                        break;
                    }
                    else
                    {
                        children.Add(ParseElement(childStartMarker));
                    }
                }
                else if (tokenizer._type == JsxTokenType.Text)
                {
                    children.Add(ParseText(childStartMarker));
                }
                else if (tokenizer._type == TokenType.BraceLeft)
                {
                    children.Add(ParseExpressionContainer(childStartMarker));
                }
                else
                {
                    _parser.Unexpected();
                }
            }

            openingElement = openingTag as JsxOpeningElement;
            closingElement = closingTag as JsxClosingElement;
            if ((openingElement is null) ^ (closingElement is null)
                || openingElement is not null && !JsxName.ValueEqualityComparer.Default.Equals(openingElement.Name, closingElement!.Name))
            {
                // _parser.Raise(closingTag.Start, $"Expected corresponding JSX closing tag for <{openingElement?.Name.GetQualifiedName()}>"); // original acornjs error reporting
                _parser.Raise(closingTag.Start, JsxMissingClosingTagElement, new object?[] { openingElement?.Name.GetQualifiedName() });
            }
        }
        else
        {
            openingElement = (JsxOpeningElement)openingTag;
            closingElement = null;
        }

        if (tokenizer._type == TokenType.Relational && '<'.ToStringCached().Equals(tokenizer._value.Value))
        {
            // _parser.Raise(tokenizer._start, "Adjacent JSX elements must be wrapped in an enclosing tag"); // original acornjs error reporting
            _parser.Raise(tokenizer._start, JsxUnwrappedAdjacentElements);
        }

        return _parser.ExitRecursion(_parser.FinishNode<JsxElementOrFragment>(startMarker, openingElement is not null
            ? new JsxElement(openingElement, NodeList.From(ref children), closingElement)
            : new JsxFragment((JsxOpeningFragment)openingTag, NodeList.From(ref children), (JsxClosingFragment)closingTag!)));
    }

    // Parses JSX opening tag starting after '<'.

    private JsxOpeningTag ParseOpeningElement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_parseOpeningElementAt(startPos, startLoc) {`

        var attributes = new ArrayList<JsxAttributeLike>();
        var nodeName = ParseElementName();
        ref readonly var tokenizer = ref _parser._tokenizer;
        while (tokenizer._type != TokenType.Slash && tokenizer._type != JsxTokenType.TagEnd)
        {
            attributes.Add(ParseAttribute());
        }
        var selfClosing = _parser.Eat(TokenType.Slash);
        _parser.Expect(JsxTokenType.TagEnd);
        return _parser.FinishNode<JsxOpeningTag>(startMarker, nodeName is not null
            ? new JsxOpeningElement(nodeName, NodeList.From(ref attributes), selfClosing)
            : new JsxOpeningFragment());
    }

    // Parses JSX closing tag starting after '</'.

    private JsxClosingTag ParseClosingElement(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_parseClosingElementAt(startPos, startLoc) {`

        var nodeName = ParseElementName();
        _parser.Expect(JsxTokenType.TagEnd);
        return _parser.FinishNode<JsxClosingTag>(startMarker, nodeName is not null
            ? new JsxClosingElement(nodeName)
            : new JsxClosingFragment());
    }

    // Parses element name in any form - namespaced, member
    // or single identifier.

    private JsxName? ParseElementName()
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_parseElementName() {`

        ref readonly var tokenizer = ref _parser._tokenizer;
        if (tokenizer._type == JsxTokenType.TagEnd)
        {
            return null;
        }
        var startMarker = _parser.StartNode();
        var node = ParseNamespacedName();
        if (tokenizer._type == TokenType.Dot && node.Type == JsxNodeType.NamespacedName && !_options._jsxAllowNamespacedObjects)
        {
            _parser.Unexpected();
        }
        while (_parser.Eat(TokenType.Dot))
        {
            var obj = node;
            var property = ParseIdentifier();
            node = _parser.FinishNode(startMarker, new JsxMemberExpression(obj, property));
        }
        return node;
    }

    // Parse namespaced identifier.

    private JsxName ParseNamespacedName()
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_parseNamespacedName() {`

        var startMarker = _parser.StartNode();
        var name = ParseIdentifier();
        if (!_options._jsxAllowNamespaces || !_parser.Eat(TokenType.Colon))
        {
            return name;
        }
        var @namespace = name;
        name = ParseIdentifier();
        return _parser.FinishNode(startMarker, new JsxNamespacedName(@namespace, name));
    }

    // Parse next token as JSX identifier

    private JsxIdentifier ParseIdentifier()
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_parseIdentifier() {`

        var startMarker = _parser.StartNode();
        ref readonly var tokenizer = ref _parser._tokenizer;
        string name;
        if (tokenizer._type == JsxTokenType.Name)
        {
            name = JsxToken.UnwrapStringValue(tokenizer._value);
        }
        else if (tokenizer._type.Keyword is not null)
        {
            name = tokenizer._type.Label;
        }
        else
        {
            return _parser.Unexpected<JsxIdentifier>();
        }
        _parser.Next();
        return _parser.FinishNode(startMarker, new JsxIdentifier(name));
    }

    // Parses following JSX attribute name-value pair.

    private JsxAttributeLike ParseAttribute()
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_parseAttribute() {`

        var startMarker = _parser.StartNode();
        if (_parser.Eat(TokenType.BraceLeft))
        {
            _parser.Expect(TokenType.Ellipsis);
            var argument = _parser.ParseMaybeAssign(ref NullRef<DestructuringErrors>());
            _parser.Expect(TokenType.BraceRight);
            return _parser.FinishNode(startMarker, new JsxSpreadAttribute(argument));
        }
        var name = ParseNamespacedName();
        var value = _parser.Eat(TokenType.Eq) ? ParseAttributeValue() : null;
        return _parser.FinishNode(startMarker, new JsxAttribute(name, value));
    }

    // Parses any type of JSX attribute value.

    private Expression ParseAttributeValue()
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_parseExpressionContainer() {`

        Marker startMarker;
        ref readonly var tokenizer = ref _parser._tokenizer;
        if (tokenizer._type == TokenType.BraceLeft)
        {
            startMarker = _parser.StartNode();
            var node = ParseExpressionContainer(startMarker);
            if (node.Expression is JsxEmptyExpression)
            {
                // _parser.Raise(node.Start, "JSX attributes must only be assigned a non-empty expression"); // original acornjs error reporting
                _parser.Raise(node.Start, JsxAttributeIsEmpty);
            }
            return node;
        }
        else if (tokenizer._type == JsxTokenType.TagStart)
        {
            startMarker = _parser.StartNode();
            _parser.Next();
            return ParseElement(startMarker);
        }
        else if (tokenizer._type == TokenType.String)
        {
            return _parser.ParseExprAtom(ref NullRef<DestructuringErrors>());
        }
        else
        {
            // _parser.Raise(tokenizer._start, "JSX value should be either an expression or a quoted JSX text"); // original acornjs error reporting
            _parser.Raise(tokenizer._start, JsxUnsupportedValue);
            return default;
        }
    }

    // Parses JSX expression enclosed into curly brackets.

    private JsxExpressionContainer ParseExpressionContainer(in Marker startMarker)
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_parseExpressionContainer() {`

        _parser.Next();

        ref readonly var tokenizer = ref _parser._tokenizer;
        var expression = tokenizer._type == TokenType.BraceRight
            ? ParseEmptyExpression()
            : _parser.ParseExpression(ref NullRef<DestructuringErrors>());
        _parser.Expect(TokenType.BraceRight);
        return _parser.FinishNode(startMarker, new JsxExpressionContainer(expression));
    }

    // JSXEmptyExpression is unique type since it doesn't actually parse anything,
    // and so it should start at the end of last read token (left brace) and finish
    // at the beginning of the next one (right brace).

    private JsxEmptyExpression ParseEmptyExpression()
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `jsx_parseEmptyExpression() {`

        ref readonly var tokenizer = ref _parser._tokenizer;
        var startMarker = new Marker(tokenizer._lastTokenEnd, tokenizer._lastTokenEndLocation);
        return _parser.FinishNodeAt(startMarker, _parser.StartNode(), new JsxEmptyExpression());
    }
}
