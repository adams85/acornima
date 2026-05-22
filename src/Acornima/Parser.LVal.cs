using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static SyntaxErrorMessages;
using static Unsafe;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js

public partial class Parser
{
    // Convert existing expression atom to assignable pattern
    // if possible.
    [return: NotNullIfNotNull(nameof(node))]
    private Node? ToAssignable(Node node, ref DestructuringErrors destructuringErrors, bool isBinding,
        bool isInPattern = false, bool allowCall = false, LeftHandSideKind lhsKind = LeftHandSideKind.Unknown)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.toAssignable = function`

        Debug.Assert(!isBinding || !allowCall);

        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6)
        {
            Node? parenthesizedExpression = null;
            Node convertedNode;
            NodeList<Node?> convertedNodes;

        Reenter:
            switch (node.Type)
            {
                case NodeType.Identifier:
                    if (InAsync && node.As<Identifier>().Name == "await")
                    {
                        // Raise(node.Start, "Can not use 'await' as identifier inside an async function"); // original acornjs error reporting
                        Raise(node.Start, AwaitBindingIdentifier);
                    }
                    break;

                case NodeType.ObjectPattern:
                case NodeType.Property when node is AssignmentProperty: // AssignmentProperty has Type == NodeType.Property
                case NodeType.ArrayPattern:
                case NodeType.AssignmentPattern:
                case NodeType.RestElement:
                    break;

                case NodeType.MemberExpression:
                    //  Original acornjs error reporting is different (just falls through to the default case)
                    if (isBinding)
                    {
                        Raise(node.Start, InvalidPropertyBindingPattern);
                    }
                    break;

                case NodeType.ObjectExpression:
                    if (!IsNullRef(ref destructuringErrors))
                    {
                        Debug.Assert(!isBinding);
                        CheckPatternErrors(ref destructuringErrors, isAssign: true, isInPattern, lhsKind, node.Start);
                    }

                    convertedNodes = ToAssignableProperties(node.As<ObjectExpression>().Properties, isBinding)!;

                    node = ReinterpretNode(node, new ObjectPattern(properties: convertedNodes!));
                    break;

                case NodeType.Property:
                    var property = node.As<ObjectProperty>();

                    // Original acornjs error reporting
                    //if (property.Kind != PropertyKind.Init)
                    //{
                    //    Raise(property.Key.Start, "Object pattern can't contain getter or setter");
                    //}

                    if (property.Kind != PropertyKind.Init || property.Value.Type == NodeType.FunctionExpression)
                    {
                        Raise(property.Start, InvalidDestructuringTarget);
                    }

                    convertedNode = ToAssignable(property.Value, ref NullRef<DestructuringErrors>(), isBinding, isInPattern: true);

                    if (property.Value is AssignmentPattern assignmentPattern)
                    {
                        // Even though ParsePropertyValue creates AssignmentPattern for shorthand properties with a default value,
                        // OnNode is deferred for consistency. Now is the time to invoke OnNode.
                        _options._onNode?.Invoke(assignmentPattern, new OnNodeContext(_tokenizer, default, _scopeStack));
                    }

                    node = ReinterpretNode(node, new AssignmentProperty(property.Key, value: convertedNode, computed: property.Computed, shorthand: property.Shorthand));
                    break;

                case NodeType.ArrayExpression:
                    if (!IsNullRef(ref destructuringErrors))
                    {
                        Debug.Assert(!isBinding);
                        CheckPatternErrors(ref destructuringErrors, isAssign: true, isInPattern, lhsKind, node.Start);
                    }

                    convertedNodes = ToAssignableList(node.As<ArrayExpression>().Elements.AsNodes(), isBinding);

                    node = ReinterpretNode(node, new ArrayPattern(elements: convertedNodes));
                    break;

                case NodeType.SpreadElement:
                    // - A rest element with a default value in an array pattern (isBinding: false, isInPattern: true),
                    // - or a rest element in a parameter list (isBinding: true, isInPattern: true),

                    var argument = node.As<SpreadElement>().Argument;

                    convertedNode = ToAssignable(argument, ref NullRef<DestructuringErrors>(), isBinding, isInPattern: true);
                    if (convertedNode.Type == NodeType.AssignmentPattern)
                    {
                        // Raise(argument.Start, "Rest elements cannot have a default value"); // original acornjs error reporting
                        if (isBinding)
                        {
                            Raise(argument.Start, RestDefaultInitializer);
                        }
                        else
                        {
                            Raise(node.Start, InvalidDestructuringTarget);
                        }
                    }

                    node = ReinterpretNode(node, new RestElement(argument: convertedNode));
                    break;

                case NodeType.AssignmentExpression:
                    // - An element with a default value in an array pattern (isBinding: false, isInPattern: true),
                    // - a non-shorthand property with a default value in an object pattern (isBinding: false, isInPattern: true)
                    //   (for a shorthand object property with a default value, AssignmentPattern is created in the first place),
                    // - an assignment in a for in/of loop (isBinding: false, isInPattern: false),
                    // - a parameter with a default value in an arrow function (isBinding: true, isInPattern: true).

                    var assignmentExpression = node.As<AssignmentExpression>();

                    if (assignmentExpression.Operator != Operator.Assignment)
                    {
                        // Raise(assignmentExpression.Left.End, "Only '=' operator can be used for specifying default value."); // original acornjs error reporting
                        node = assignmentExpression.Left;
                        goto default;
                    }

                    convertedNode = ToAssignable(assignmentExpression.Left, ref NullRef<DestructuringErrors>(), isBinding, isInPattern, lhsKind: lhsKind);

                    node = ReinterpretNode(node, new AssignmentPattern(left: convertedNode, assignmentExpression.Right));
                    break;

                case NodeType.ParenthesizedExpression:
                    // NOTE: Original acornjs implementation does a recursive call here, but we can optimize that into a loop to keep the call stack shallow.
                    parenthesizedExpression ??= node;
                    node = node.As<ParenthesizedExpression>().Expression;
                    goto Reenter;

                // Original acornjs error reporting
                //case NodeType.ChainExpression:
                //    RaiseRecoverable(node.Start, "Optional chaining cannot appear in left-hand side");
                //    break;

                case NodeType.CallExpression when allowCall && !isInPattern:
                    // Annex B.3.9: In non-strict mode, allow CallExpression as assignment target.
                    // The runtime should throw a ReferenceError instead.
                    break;

                default:
                    // Raise(node.Start, "Assigning to rvalue"); // original acornjs error reporting
                    HandleLeftHandSideError(node.Start, isBinding, isInPattern, lhsKind);
                    break;
            }

            if (parenthesizedExpression is not null)
            {
                node = parenthesizedExpression;
            }
        }
        else if (!IsNullRef(ref destructuringErrors))
        {
            Debug.Assert(!isBinding && !isInPattern);
            CheckPatternErrors(ref destructuringErrors, isAssign: true, isInPattern: false, lhsKind, node.Start);
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
            var prop = ToAssignable(properties[i], ref NullRef<DestructuringErrors>(), isBinding, isInPattern: true);

            // Early error:
            //   AssignmentRestProperty[Yield, Await] :
            //     `...` DestructuringAssignmentTarget[Yield, Await]
            //
            //   It is a Syntax Error if |DestructuringAssignmentTarget| is an |ArrayLiteral| or an |ObjectLiteral|.
            if (prop is RestElement restElement
                && (restElement.Argument.Type is NodeType.ArrayPattern or NodeType.ObjectPattern))
            {
                // Raise(restElement.Argument.Start, "Unexpected token"); // original acornjs error reporting
                Raise(restElement.Argument.Start, InvalidRestAssignmentPattern);
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
                element = ToAssignable(element, ref NullRef<DestructuringErrors>(), isBinding, isInPattern: true);
            }
            bindingList[i] = element;
        }

        var last = bindingList.LastItemRef();
        if (isBinding && _tokenizerOptions._ecmaVersion == EcmaVersion.ES6
            && last is RestElement restElement && restElement.Argument.Type != NodeType.Identifier)
        {
            // Unexpected(restElement.Argument.Start); // original acornjs error reporting
            Raise(restElement.Argument.Start, InvalidDestructuringTarget);
        }

        return NodeList.From(ref bindingList);
    }

    // Parses spread element.
    private SpreadElement ParseSpread(ref DestructuringErrors destructuringErrors)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.parseSpread = function`

        var startMarker = StartNode();
        Next();

        var oldSuppressOnNode = _suppressOnNode;
        _suppressOnNode = false;
        var argument = ParseMaybeAssign(ref destructuringErrors, ExpressionContext.Default);
        _suppressOnNode = oldSuppressOnNode;

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
            // Unexpected(); // original acornjs error reporting
            Raise(_tokenizer._start, InvalidDestructuringTarget);
        }

        var argument = ParseBindingAtom();

        return FinishNode(startMarker, new RestElement(argument));
    }

    // Parses lvalue (assignable) atom.
    private Node ParseBindingAtom()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.parseBindingAtom = function`

        EnterRecursion();

        Node node;
        if (_tokenizerOptions._ecmaVersion >= EcmaVersion.ES6)
        {
            if (_tokenizer._type == TokenType.BracketLeft)
            {
                var startMarker = StartNode();
                Next();

                _bindingPatternDepth++;
                var elements = ParseBindingList(TokenType.BracketRight, allowEmptyElement: true, allowTrailingComma: true);
                _bindingPatternDepth--;
                return ExitRecursion(FinishNode(startMarker, new ArrayPattern(elements)));
            }

            if (_tokenizer._type == TokenType.BraceLeft)
            {
                _bindingPatternDepth++;
                node = ParseObject(isPattern: true, ref NullRef<DestructuringErrors>());
                _bindingPatternDepth--;
                return ExitRecursion(node);
            }
        }

        _bindingPatternDepth++;
        node = ParseIdentifier();
        _bindingPatternDepth--;
        return ExitRecursion(node);
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

                // We deviate a bit from the original acornjs implementation here to make trailing comma errors recoverable.
                if (AfterTrailingComma(close, allowTrailingComma))
                {
                    break;
                }
            }
            else
            {
                first = false;
            }

            if (allowEmptyElement && _tokenizer._type == TokenType.Comma)
            {
                elements.Add(null);
            }
            else if (_tokenizer._type == TokenType.Ellipsis)
            {
                var rest = ParseRestBinding();
                elements.Add(rest);
                if (_tokenizer._type == TokenType.Comma)
                {
                    // Raise(_tokenizer._start, "Comma is not permitted after the rest element"); // original acornjs error reporting

                    // As opposed to the original acornjs implementation, we report the position of the rest argument.
                    if (close == TokenType.ParenRight)
                    {
                        Raise(rest.Argument.Start, ParamAfterRest);
                    }
                    else
                    {
                        Raise(rest.Argument.Start, ElementAfterRest);
                    }
                }

                Expect(close);
                break;
            }
            else
            {
                // Original acornjs implementation does a call to `pp.parseAssignableListItem` here but
                // this function is not called from elsewhere, so we inline it to keep the call stack shallow.
                // elements.Add(ParseAssignableListItem());
                var startMarker = StartNode();
                elements.Add(ParseMaybeDefault(startMarker));
            }
        }

        return NodeList.From(ref elements);
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

        var oldBindingPatternDepth = _bindingPatternDepth;
        _bindingPatternDepth = 0;
        var right = ParseMaybeAssign(ref NullRef<DestructuringErrors>());
        _bindingPatternDepth = oldBindingPatternDepth;
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

    private void CheckLValSimple(Node expr, BindingType bindingType = BindingType.None, HashSet<string>? checkClashes = null,
        bool isInPattern = false, bool allowCall = false, LeftHandSideKind lhsKind = LeftHandSideKind.Unknown)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.checkLValSimple = function`

        var isBind = bindingType != BindingType.None;
        Debug.Assert(!isBind || !allowCall);

    Reenter:
        switch (expr.Type)
        {
            case NodeType.Identifier:
                var identifier = expr.As<Identifier>();

                if (_isReservedWordBind(identifier.Name.AsSpan(), _strict))
                {
                    // RaiseRecoverable(identifier.Start, $"{(isBind ? "Binding " : "Assigning to ")}{identifier.Name} in strict mode"); // original acornjs error reporting
                    if (identifier.Name is "eval" or "arguments")
                    {
                        RaiseRecoverable(identifier.Start, StrictEvalArguments);
                    }
                    else
                    {
                        HandleReservedWordError(identifier);
                    }
                }

                if (isBind)
                {
                    if (bindingType == BindingType.Lexical && identifier.Name == "let")
                    {
                        // RaiseRecoverable(identifier.Start, "let is disallowed as a lexically bound name"); // original acornjs error reporting
                        Raise(identifier.Start, LetInLexicalBinding);
                    }

                    if (checkClashes is not null && !checkClashes.Add(identifier.Name))
                    {
                        // RaiseRecoverable(identifier.Start, "Argument name clash"); // original acornjs error reporting
                        Raise(identifier.Start, ParamDupe);
                    }

                    if (bindingType != BindingType.Outside)
                    {
                        DeclareName(identifier, bindingType);
                    }
                }
                break;

            // Original acornjs error reporting
            //case NodeType.ChainExpression:
            //    RaiseRecoverable(expr.Start, "Optional chaining cannot appear in left-hand side");
            //    break;

            case NodeType.MemberExpression:
                if (isBind)
                {
                    // RaiseRecoverable(expr.Start, "Binding member expression"); // original acornjs error reporting
                    Raise(expr.Start, InvalidPropertyBindingPattern);
                }
                break;

            case NodeType.ParenthesizedExpression:
                var parenthesizedExpression = expr.As<ParenthesizedExpression>();
                if (isBind)
                {
                    // RaiseRecoverable(parenthesizedExpression.Start, "Binding parenthesized expression"); // original acornjs error reporting
                    Raise(parenthesizedExpression.Start, InvalidDestructuringTarget);
                }

                // NOTE: Original acornjs implementation does a recursive call here, but we can optimize that into a loop to keep the call stack shallow.
                expr = parenthesizedExpression.Expression;
                goto Reenter;

            case NodeType.CallExpression when allowCall && !isInPattern:
                // Annex B.3.9: In non-strict mode, allow CallExpression as assignment target.
                // The runtime should throw a ReferenceError instead.
                // Does NOT apply to logical assignments (&&=, ||=, ??=), which require 'simple' target.
                break;

            default:
                // Raise(expr.Start, $"{(isBind ? "Binding" : "Assigning to")} rvalue"); // original acornjs error reporting
                HandleLeftHandSideError(expr.Start, isBind, isInPattern, lhsKind);
                break;
        }
    }

    private void CheckLValPattern(Node expr, BindingType bindingType = BindingType.None, HashSet<string>? checkClashes = null,
        bool isInPattern = false, bool allowCall = false, LeftHandSideKind lhsKind = LeftHandSideKind.Unknown)
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
                CheckLValSimple(expr, bindingType, checkClashes, isInPattern, allowCall, lhsKind);
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
                CheckLValPattern(pattern.As<AssignmentPattern>().Left, bindingType, checkClashes, isInPattern: true);
                break;

            case NodeType.RestElement:
                CheckLValPattern(pattern.As<RestElement>().Argument, bindingType, checkClashes, isInPattern: true);
                break;

            default:
                CheckLValPattern(pattern, bindingType, checkClashes, isInPattern: true);
                break;
        }
    }

    private void DeclareName(Identifier id, BindingType bindingType)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.declareName = function`

        var redeclared = false;
        var name = id.Name;
        switch (bindingType)
        {
            case BindingType.Lexical:
                ref var scope = ref CurrentScope;
                redeclared = scope._lexical.Contains(name) || scope._functions.Contains(name) || scope._var.Contains(name);
                scope._lexical.Add(id);
                if (_inModule && (scope._flags & ScopeFlags.Top) != 0)
                {
                    _undefinedExports!.Remove(name);
                }
                break;

            case BindingType.SimpleCatch:
                scope = ref CurrentScope;
                scope._lexical.Add(id);
                break;

            case BindingType.Function:
                scope = ref CurrentScope;
                redeclared = (scope._flags & _functionsAsVarInScopeFlags) != 0
                    ? scope._lexical.Contains(name)
                    : scope._lexical.Contains(name) || scope._var.Contains(name);
                scope._functions.Add(id);
                break;

            default:
                for (var i = _scopeStack.Count - 1; i >= 0; --i)
                {
                    scope = ref _scopeStack.GetItemRef(i);
                    if (scope._lexical.Contains(name) && !((scope._flags & ScopeFlags.SimpleCatch) != 0 && scope._lexical[0] == name)
                        || (scope._flags & _functionsAsVarInScopeFlags) == 0 && scope._functions.Contains(name))
                    {
                        redeclared = true;
                        break;
                    }

                    scope._var.Add(id);
                    if (_inModule && (scope._flags & ScopeFlags.Top) != 0)
                    {
                        _undefinedExports!.Remove(name);
                    }
                    if ((scope._flags & ScopeFlags.Var) != 0)
                    {
                        break;
                    }
                }
                break;
        }

        if (redeclared)
        {
            // RaiseRecoverable(id.Start, $"Identifier '{name}' has already been declared"); // original acornjs error reporting
            Raise(id.Start, VarRedeclaration, new object[] { name });
        }
    }

    private void CheckLocalExport(Identifier id)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.checkLocalExport = function`

        ref readonly var rootScope = ref _scopeStack.GetItemRef(0);
        // scope.functions must be empty as Module code is always strict.
        if (!rootScope._lexical.Contains(id.Name)
            && !rootScope._var.Contains(id.Name))
        {
            _undefinedExports![id.Name] = id.Start;
        }
    }

    [DoesNotReturn]
    private void HandleLeftHandSideError(int position, bool isBinding, bool isInPattern, LeftHandSideKind lhsKind)
    {
        if (!isBinding && !isInPattern)
        {
            switch (lhsKind)
            {
                case LeftHandSideKind.Assignment:
                    Raise(position, InvalidLhsInAssignment);
                    break;

                case LeftHandSideKind.PrefixUpdate:
                    Raise(position, InvalidLhsInPrefixOp);
                    break;

                case LeftHandSideKind.PostfixUpdate:
                    Raise(position, InvalidLhsInPostfixOp);
                    break;

                case LeftHandSideKind.ForInOf:
                    Raise(position, InvalidLhsInFor);
                    break;
            }
        }

        Raise(position, InvalidDestructuringTarget);
    }

    private enum LeftHandSideKind : byte
    {
        Unknown,
        Assignment,
        PrefixUpdate,
        PostfixUpdate,
        ForInOf,
    }
}
