using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static Unsafe;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js

public partial class Parser
{
    // Convert existing expression atom to assignable pattern
    // if possible.
    [return: NotNullIfNotNull(nameof(node))]
    private Node? ToAssignable(Node? node, bool isBinding, ref DestructuringErrors destructuringErrors)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.toAssignable = function`

        if (node is not null && _tokenizerOptions._ecmaVersion >= EcmaVersion.ES6)
        {
            // TODO: revise CheckPatternErrors checks as they produces recoverable errors

            Node convertedNode;
            NodeList<Node?> convertedNodes;
            switch (node.Type)
            {
                case NodeType.Identifier:
                    if (InAsync() && node.As<Identifier>().Name == "await")
                    {
                        return Raise<Node>(node.Start, "Can not use 'await' as identifier inside an async function");
                    }
                    break;

                case NodeType.ObjectPattern:
                case NodeType.Property when node is AssignmentProperty: // AssignmentProperty has Type == NodeType.Property
                case NodeType.ArrayPattern:
                case NodeType.AssignmentPattern:
                case NodeType.RestElement:
                case NodeType.MemberExpression when !isBinding:
                    break;

                case NodeType.ObjectExpression:
                    if (!IsNullRef(ref destructuringErrors))
                    {
                        CheckPatternErrors(ref destructuringErrors, isAssign: true);
                    }

                    convertedNodes = ToAssignableProperties(node.As<ObjectExpression>().Properties, isBinding)!;

                    node = ReinterpretNode(node, new ObjectPattern(properties: convertedNodes!));
                    break;

                case NodeType.Property:
                    var property = node.As<Property>();

                    if (property.Kind != PropertyKind.Init)
                    {
                        return Raise<Node>(property.Key.Start, "Object pattern can't contain getter or setter");
                    }

                    convertedNode = ToAssignable(property.Value, isBinding, ref NullRef<DestructuringErrors>());

                    node = ReinterpretNode(node, new AssignmentProperty(property.Key, value: convertedNode, computed: property.Computed, shorthand: property.Shorthand));
                    break;

                case NodeType.ArrayExpression:
                    if (!IsNullRef(ref destructuringErrors))
                    {
                        CheckPatternErrors(ref destructuringErrors, isAssign: true);
                    }

                    convertedNodes = ToAssignableList(node.As<ArrayExpression>().Elements.AsNodes(), isBinding);

                    node = ReinterpretNode(node, new ArrayPattern(elements: convertedNodes));
                    break;

                case NodeType.SpreadElement:
                    var argument = node.As<SpreadElement>().Argument;

                    convertedNode = ToAssignable(argument, isBinding, ref NullRef<DestructuringErrors>());
                    if (convertedNode.Type == NodeType.AssignmentPattern)
                    {
                        return Raise<Node>(argument.Start, "Rest elements cannot have a default value");
                    }

                    node = ReinterpretNode(node, new RestElement(argument: convertedNode));
                    break;

                case NodeType.AssignmentExpression:
                    var assignmentExpression = node.As<AssignmentExpression>();

                    if (assignmentExpression.Operator != Operator.Assignment)
                    {
                        return Raise<Node>(assignmentExpression.Left.End, "Only '=' operator can be used for specifying default value.");
                    }

                    convertedNode = ToAssignable(assignmentExpression.Left, isBinding, ref NullRef<DestructuringErrors>());

                    node = ReinterpretNode(node, new AssignmentPattern(left: convertedNode, assignmentExpression.Right));
                    break;

                case NodeType.ParenthesizedExpression:
                    ToAssignable(node.As<ParenthesizedExpression>().Expression, isBinding, ref destructuringErrors);
                    break;

                case NodeType.ChainExpression:
                    RaiseRecoverable(node.Start, "Optional chaining cannot appear in left-hand side");
                    break;

                default:
                    return Raise<Node>(node.Start, "Assigning to rvalue");
            }
        }
        else if (!IsNullRef(ref destructuringErrors))
        {
            CheckPatternErrors(ref destructuringErrors, isAssign: true);
        }

        return node;
    }

    private NodeList<Node> ToAssignableProperties(in NodeList<Node> properties, bool isBinding)
    {
        if (properties.Count == 0)
        {
            return new NodeList<Node>();
        }

        var assignmentProperties = new ArrayList<Node>(new Node[properties.Count]);

        for (var i = 0; i < properties.Count; i++)
        {
            var prop = ToAssignable(properties[i], isBinding, ref NullRef<DestructuringErrors>());

            // Early error:
            //   AssignmentRestProperty[Yield, Await] :
            //     `...` DestructuringAssignmentTarget[Yield, Await]
            //
            //   It is a Syntax Error if |DestructuringAssignmentTarget| is an |ArrayLiteral| or an |ObjectLiteral|.
            if (prop is RestElement restElement
                && (restElement.Argument.Type is NodeType.ArrayPattern or NodeType.ObjectPattern))
            {
                Raise(restElement.Argument.Start, "Unexpected token");
            }

            assignmentProperties[i] = prop;
        }

        return NodeList.From(ref assignmentProperties);
    }

    // Convert list of expression atoms to binding list.
    private NodeList<Node?> ToAssignableList(in NodeList<Node?> exprList, bool isBinding)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.toAssignableList = function`

        if (exprList.Count == 0)
        {
            return new NodeList<Node?>();
        }

        var bindingList = new ArrayList<Node?>(new Node?[exprList.Count]);

        for (var i = 0; i < exprList.Count; i++)
        {
            Node? element = exprList[i];
            if (element is not null)
            {
                element = ToAssignable(element, isBinding, ref NullRef<DestructuringErrors>());
            }
            bindingList[i] = element;
        }

        var last = bindingList.LastItemRef();
        if (isBinding && _tokenizerOptions._ecmaVersion == EcmaVersion.ES6
            && last is RestElement restElement && restElement.Argument.Type != NodeType.Identifier)
        {
            Unexpected(restElement.Argument.Start);
        }

        return NodeList.From(ref bindingList);
    }

    // Parses spread element.
    private SpreadElement ParseSpread(ref DestructuringErrors destructuringErrors)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.parseSpread = function`

        var startMarker = StartNode();
        Next();

        var argument = ParseMaybeAssign(ref destructuringErrors, ExpressionContext.Default);
        return FinishNode(startMarker, new SpreadElement(argument));
    }

    private RestElement ParseRestBinding()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.parseRestBinding = function`

        var startMarker = StartNode();
        Next();

        // RestElement inside of a function parameter must be an identifier
        if (_tokenizerOptions._ecmaVersion == EcmaVersion.ES6 && _tokenizer._type != TokenType.Name)
        {
            Unexpected();
        }

        var argument = ParseBindingAtom();

        return FinishNode(startMarker, new RestElement(argument));
    }

    // Parses lvalue (assignable) atom.
    private Node ParseBindingAtom()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.parseBindingAtom = function`

        EnterRecursion();

        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6)
        {
            if (_tokenizer._type == TokenType.BracketLeft)
            {
                var startMarker = StartNode();
                Next();

                var elements = ParseBindingList(TokenType.BracketRight, allowEmptyElement: true, allowTrailingComma: true);
                return ExitRecursion(FinishNode(startMarker, new ArrayPattern(elements)));
            }

            if (_tokenizer._type == TokenType.BraceLeft)
            {
                return ExitRecursion(ParseObject(isPattern: true, ref NullRef<DestructuringErrors>()));
            }
        }

        return ExitRecursion(ParseIdentifier());
    }

    private NodeList<Node?> ParseBindingList(TokenType close, bool allowEmptyElement, bool allowTrailingComma)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.parseBindingList = function`

        var elements = new ArrayList<Node?>();
        var first = true;
        while (!Eat(close))
        {
            if (!first)
            {
                Expect(TokenType.Comma);
            }
            else
            {
                first = false;
            }

            if (allowEmptyElement && _tokenizer._type == TokenType.Comma)
            {
                elements.Add(null);
            }
            else if (allowTrailingComma && AfterTrailingComma(close))
            {
                break;
            }
            else if (_tokenizer._type == TokenType.Ellipsis)
            {
                var rest = ParseRestBinding();
                elements.Add(rest);
                if (_tokenizer._type == TokenType.Comma)
                {
                    Raise(_tokenizer._start, "Comma is not permitted after the rest element");
                }

                Expect(close);
                break;
            }
            else
            {
                elements.Add(ParseAssignableListItem());
            }
        }

        return NodeList.From(ref elements);
    }

    private Node ParseAssignableListItem()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.parseAssignableListItem = function`

        var startMarker = StartNode();
        var element = ParseMaybeDefault(startMarker);
        return element;
    }

    // Parses assignment pattern around given atom if possible.
    private Node ParseMaybeDefault(in Marker startMarker, Node? left = null)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.parseMaybeDefault = function`

        left ??= ParseBindingAtom();
        if (_tokenizerOptions._ecmaVersion < EcmaVersion.ES6 || !Eat(TokenType.Eq))
        {
            return left;
        }

        var right = ParseMaybeAssign(ref NullRef<DestructuringErrors>());
        return FinishNode(startMarker, new AssignmentPattern(left, right));
    }

    // The following three functions all verify that a node is an lvalue —
    // something that can be bound, or assigned to. In order to do so, they perform
    // a variety of checks:
    //
    // - Check that none of the bound/assigned-to identifiers are reserved words.
    // - Record name declarations for bindings in the appropriate scope.
    // - Check duplicate argument names, if checkClashes is set.
    //
    // If a complex binding pattern is encountered (e.g., object and array
    // destructuring), the entire pattern is recursively checked.
    //
    // There are three versions of checkLVal*() appropriate for different
    // circumstances:
    //
    // - checkLValSimple() shall be used if the syntactic construct supports
    //   nothing other than identifiers and member expressions. Parenthesized
    //   expressions are also correctly handled. This is generally appropriate for
    //   constructs for which the spec says
    //
    //   > It is a Syntax Error if AssignmentTargetType of [the production] is not
    //   > simple.
    //
    //   It is also appropriate for checking if an identifier is valid and not
    //   defined elsewhere, like import declarations or function/class identifiers.
    //
    //   Examples where this is used include:
    //     a += …;
    //     import a from '…';
    //   where a is the node to be checked.
    //
    // - checkLValPattern() shall be used if the syntactic construct supports
    //   anything checkLValSimple() supports, as well as object and array
    //   destructuring patterns. This is generally appropriate for constructs for
    //   which the spec says
    //
    //   > It is a Syntax Error if [the production] is neither an ObjectLiteral nor
    //   > an ArrayLiteral and AssignmentTargetType of [the production] is not
    //   > simple.
    //
    //   Examples where this is used include:
    //     (a = …);
    //     const a = …;
    //     try { … } catch (a) { … }
    //   where a is the node to be checked.
    //
    // - checkLValInnerPattern() shall be used if the syntactic construct supports
    //   anything checkLValPattern() supports, as well as default assignment
    //   patterns, rest elements, and other constructs that may appear within an
    //   object or array destructuring pattern.
    //
    //   As a special case, function parameters also use checkLValInnerPattern(),
    //   as they also support defaults and rest constructs.
    //
    // These functions deliberately support both assignment and binding constructs,
    // as the logic for both is exceedingly similar. If the node is the target of
    // an assignment, then bindingType should be set to BIND_NONE. Otherwise, it
    // should be set to the appropriate BIND_* constant, like BIND_VAR or
    // BIND_LEXICAL.
    //
    // If the function is called with a non-BIND_NONE bindingType, then
    // additionally a checkClashes object may be specified to allow checking for
    // duplicate argument names. checkClashes is ignored if the provided construct
    // is an assignment (i.e., bindingType is BIND_NONE).

    private void CheckLValSimple(Node expr, BindingType bindingType = BindingType.None, HashSet<string>? checkClashes = null)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.checkLValSimple = function`

        var isBind = bindingType != BindingType.None;

        switch (expr.Type)
        {
            case NodeType.Identifier:
                var identifier = expr.As<Identifier>();

                if (_isReservedWordBind(identifier.Name.AsSpan(), _strict))
                {
                    RaiseRecoverable(identifier.Start, $"{(isBind ? "Binding " : "Assigning to ")}{identifier.Name} in strict mode");
                }

                if (isBind)
                {
                    if (bindingType == BindingType.Lexical && identifier.Name == "let")
                    {
                        RaiseRecoverable(identifier.Start, "let is disallowed as a lexically bound name");
                    }

                    if (checkClashes is not null)
                    {
                        if (checkClashes.Contains(identifier.Name))
                        {
                            RaiseRecoverable(identifier.Start, "Argument name clash");
                        }

                        checkClashes.Add(identifier.Name);
                    }

                    if (bindingType != BindingType.Outside)
                    {
                        DeclareName(identifier.Name, bindingType, identifier.Start);
                    }
                }
                break;

            case NodeType.ChainExpression:
                RaiseRecoverable(expr.Start, "Optional chaining cannot appear in left-hand side");
                break;

            case NodeType.MemberExpression:
                if (isBind)
                {
                    RaiseRecoverable(expr.Start, "Binding member expression");
                }
                break;

            case NodeType.ParenthesizedExpression:
                var parenthesizedExpression = expr.As<ParenthesizedExpression>();
                if (isBind)
                {
                    RaiseRecoverable(parenthesizedExpression.Start, "Binding parenthesized expression");
                }

                CheckLValSimple(parenthesizedExpression.Expression, bindingType, checkClashes);
                break;

            default:
                Raise(expr.Start, $"{(isBind ? "Binding" : "Assigning to")} rvalue");
                break;
        }
    }

    private void CheckLValPattern(Node expr, BindingType bindingType = BindingType.None, HashSet<string>? checkClashes = null)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.checkLValPattern = function`

        switch (expr.Type)
        {
            case NodeType.ObjectPattern:
                var properties = expr.As<ObjectPattern>().Properties;
                for (var i = 0; i < properties.Count; i++)
                {
                    CheckLValInnerPattern(properties[i], bindingType, checkClashes);
                }
                break;

            case NodeType.ArrayPattern:
                var elements = expr.As<ArrayPattern>().Elements;
                for (var i = 0; i < elements.Count; i++)
                {
                    if (elements[i] is { } elem)
                    {
                        CheckLValInnerPattern(elem, bindingType, checkClashes);
                    }
                }
                break;

            default:
                CheckLValSimple(expr, bindingType, checkClashes);
                break;
        }
    }

    private void CheckLValInnerPattern(Node pattern, BindingType bindingType = BindingType.None, HashSet<string>? checkClashes = null)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.checkLValInnerPattern = function`

        switch (pattern.Type)
        {
            case NodeType.Property when pattern is AssignmentProperty assignmentProperty: // AssignmentProperty has Type == NodeType.Property
                CheckLValInnerPattern(assignmentProperty.Value, bindingType, checkClashes);
                break;

            case NodeType.AssignmentPattern:
                CheckLValPattern(pattern.As<AssignmentPattern>().Left, bindingType, checkClashes);
                break;

            case NodeType.RestElement:
                CheckLValPattern(pattern.As<RestElement>().Argument, bindingType, checkClashes);
                break;

            default:
                CheckLValPattern(pattern, bindingType, checkClashes);
                break;
        }
    }

    private void DeclareName(string name, BindingType bindingType, int pos)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.declareName = function`

        var redeclared = false;
        ref var scope = ref Unsafe.NullRef<Scope>();
        switch (bindingType)
        {
            case BindingType.Lexical:
                scope = ref CurrentScope;
                redeclared = scope.Lexical.IndexOf(name) >= 0 || scope.Functions.IndexOf(name) >= 0 || scope.Var.IndexOf(name) >= 0;
                scope.Lexical.Add(name);
                if (_inModule && (scope.Flags & ScopeFlags.Top) != 0)
                {
                    _undefinedExports!.Remove(name);
                }
                break;

            case BindingType.SimpleCatch:
                scope = ref CurrentScope;
                scope.Lexical.Add(name);
                break;

            case BindingType.Function:
                scope = ref CurrentScope;
                redeclared = (scope.Flags & _functionsAsVarInScopeFlags) != 0
                    ? scope.Lexical.IndexOf(name) >= 0
                    : scope.Lexical.IndexOf(name) >= 0 || scope.Var.IndexOf(name) >= 0;
                scope.Functions.Add(name);
                break;

            default:
                for (var i = _scopeStack.Count - 1; i >= 0; --i)
                {
                    scope = ref _scopeStack.GetItemRef(i);
                    if (scope.Lexical.IndexOf(name) >= 0 && !((scope.Flags & ScopeFlags.SimpleCatch) != 0 && scope.Lexical[0] == name)
                        || (scope.Flags & _functionsAsVarInScopeFlags) == 0 && scope.Functions.IndexOf(name) >= 0)
                    {
                        redeclared = true;
                        break;
                    }

                    scope.Var.Add(name);
                    if (_inModule && (scope.Flags & ScopeFlags.Top) != 0)
                    {
                        _undefinedExports!.Remove(name);
                    }
                    if ((scope.Flags & ScopeFlags.Var) != 0)
                    {
                        break;
                    }
                }
                break;
        }

        if (redeclared)
        {
            RaiseRecoverable(pos, $"Identifier '{name}' has already been declared");
        }
    }

    private void CheckLocalExport(Identifier id)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.checkLocalExport = function`

        ref readonly var rootScope = ref _scopeStack.GetItemRef(0);
        // scope.functions must be empty as Module code is always strict.
        if (rootScope.Lexical.IndexOf(id.Name) < 0
            && rootScope.Var.IndexOf(id.Name) < 0)
        {
            _undefinedExports![id.Name] = id.Start;
        }
    }
}
