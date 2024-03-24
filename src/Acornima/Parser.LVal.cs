using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Acornima.Ast;
using Acornima.Helpers;
using Acornima.Properties;

namespace Acornima;

using static Unsafe;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js

public partial class Parser
{
    // Convert existing expression atom to assignable pattern
    // if possible.
    [return: NotNullIfNotNull(nameof(node))]
    private Node? ToAssignable(Node? node, ref DestructuringErrors destructuringErrors, bool isBinding, bool isParam = false, LeftHandSideKind lhsKind = LeftHandSideKind.Unknown)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.toAssignable = function`

        if (node is not null && _tokenizerOptions._ecmaVersion >= EcmaVersion.ES6)
        {
            Node convertedNode;
            NodeList<Node?> convertedNodes;

        Reenter:
            switch (node.Type)
            {
                case NodeType.Identifier:
                    if (InAsync() && node.As<Identifier>().Name == "await")
                    {
                        // Raise(node.Start, "Can not use 'await' as identifier inside an async function"); // original acornjs error reporting
                        Raise(node.Start, SyntaxErrorMessages.AwaitBindingIdentifier);
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
                        Raise(node.Start, SyntaxErrorMessages.InvalidPropertyBindingPattern);
                    }
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
                    var property = node.As<ObjectProperty>();

                    // Original acornjs error reporting
                    //if (property.Kind != PropertyKind.Init)
                    //{
                    //    Raise(property.Key.Start, "Object pattern can't contain getter or setter");
                    //}

                    if (property.Kind != PropertyKind.Init || property.Value is FunctionExpression)
                    {
                        Raise(property.Start, SyntaxErrorMessages.InvalidDestructuringTarget);
                    }

                    convertedNode = ToAssignable(property.Value, ref NullRef<DestructuringErrors>(), isBinding);

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

                    convertedNode = ToAssignable(argument, ref NullRef<DestructuringErrors>(), isBinding);
                    if (convertedNode.Type == NodeType.AssignmentPattern)
                    {
                        // Raise(argument.Start, "Rest elements cannot have a default value"); // original acornjs error reporting
                        if (isParam)
                        {
                            Raise(argument.Start, SyntaxErrorMessages.RestDefaultInitializer);
                        }
                        else
                        {
                            Raise(node.Start, SyntaxErrorMessages.InvalidDestructuringTarget);
                        }
                    }

                    node = ReinterpretNode(node, new RestElement(argument: convertedNode));
                    break;

                case NodeType.AssignmentExpression:
                    var assignmentExpression = node.As<AssignmentExpression>();

                    if (assignmentExpression.Operator != Operator.Assignment)
                    {
                        // Raise(assignmentExpression.Left.End, "Only '=' operator can be used for specifying default value."); // original acornjs error reporting
                        Raise(assignmentExpression.Left.Start, SyntaxErrorMessages.InvalidDestructuringTarget);
                    }

                    convertedNode = ToAssignable(assignmentExpression.Left, ref NullRef<DestructuringErrors>(), isBinding, lhsKind: lhsKind);

                    node = ReinterpretNode(node, new AssignmentPattern(left: convertedNode, assignmentExpression.Right));
                    break;

                case NodeType.ParenthesizedExpression:
                    // NOTE: Original acornjs implementation does a recursive call here, but we can optimize that into a loop to keep the call stack shallow.
                    node = node.As<ParenthesizedExpression>().Expression;
                    goto Reenter;

                // Original acornjs error reporting
                //case NodeType.ChainExpression:
                //    RaiseRecoverable(node.Start, "Optional chaining cannot appear in left-hand side");
                //    break;

                default:
                    // Raise(node.Start, "Assigning to rvalue"); // original acornjs error reporting
                    HandleLeftHandSideError(node, isBinding, lhsKind);
                    break;
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
            var prop = ToAssignable(properties[i], ref NullRef<DestructuringErrors>(), isBinding);

            // Early error:
            //   AssignmentRestProperty[Yield, Await] :
            //     `...` DestructuringAssignmentTarget[Yield, Await]
            //
            //   It is a Syntax Error if |DestructuringAssignmentTarget| is an |ArrayLiteral| or an |ObjectLiteral|.
            if (prop is RestElement restElement
                && (restElement.Argument.Type is NodeType.ArrayPattern or NodeType.ObjectPattern))
            {
                // Raise(restElement.Argument.Start, "Unexpected token"); // original acornjs error reporting
                Raise(restElement.Argument.Start, SyntaxErrorMessages.InvalidRestAssignmentPattern);
            }

            assignmentProperties[i] = prop;
        }

        return NodeList.From(ref assignmentProperties);
    }

    // Convert list of expression atoms to binding list.
    private NodeList<Node?> ToAssignableList(in NodeList<Node?> exprList, bool isBinding, bool isParams = false)
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
                element = ToAssignable(element, ref NullRef<DestructuringErrors>(), isBinding, isParams);
            }
            bindingList[i] = element;
        }

        var last = bindingList.LastItemRef();
        if (isBinding && _tokenizerOptions._ecmaVersion == EcmaVersion.ES6
            && last is RestElement restElement && restElement.Argument.Type != NodeType.Identifier)
        {
            // Unexpected(restElement.Argument.Start); // original acornjs error reporting
            _tokenizer.MoveTo(restElement.Argument.Start, expressionAllowed: false);
            Next(ignoreEscapeSequenceInKeyword: true);
            Unexpected();
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
            // Unexpected(); // original acornjs error reporting
            Raise(_tokenizer._start, SyntaxErrorMessages.InvalidDestructuringTarget);
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
                    Raise(rest.Argument.Start, close == TokenType.ParenRight ? SyntaxErrorMessages.ParamAfterRest : SyntaxErrorMessages.ElementAfterRest);
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

    private void CheckLValSimple(Node expr, BindingType bindingType = BindingType.None, HashSet<string>? checkClashes = null, LeftHandSideKind lhsKind = LeftHandSideKind.Unknown)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/lval.js > `pp.checkLValSimple = function`

        var isBind = bindingType != BindingType.None;

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
                        RaiseRecoverable(identifier.Start, SyntaxErrorMessages.StrictEvalArguments);
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
                        Raise(identifier.Start, SyntaxErrorMessages.LetInLexicalBinding);
                    }

                    if (checkClashes is not null)
                    {
                        if (checkClashes.Contains(identifier.Name))
                        {
                            // RaiseRecoverable(identifier.Start, "Argument name clash"); // original acornjs error reporting
                            Raise(identifier.Start, SyntaxErrorMessages.ParamDupe);
                        }

                        checkClashes.Add(identifier.Name);
                    }

                    if (bindingType != BindingType.Outside)
                    {
                        DeclareName(identifier.Name, bindingType, identifier.Start);
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
                    Raise(expr.Start, SyntaxErrorMessages.InvalidPropertyBindingPattern);
                }
                break;

            case NodeType.ParenthesizedExpression:
                var parenthesizedExpression = expr.As<ParenthesizedExpression>();
                if (isBind)
                {
                    // RaiseRecoverable(parenthesizedExpression.Start, "Binding parenthesized expression"); // original acornjs error reporting
                    Raise(parenthesizedExpression.Start, SyntaxErrorMessages.InvalidDestructuringTarget);
                }

                // NOTE: Original acornjs implementation does a recursive call here, but we can optimize that into a loop to keep the call stack shallow.
                expr = parenthesizedExpression.Expression;
                goto Reenter;

            default:
                // Raise(expr.Start, $"{(isBind ? "Binding" : "Assigning to")} rvalue"); // original acornjs error reporting
                HandleLeftHandSideError(expr, isBind, lhsKind);
                break;
        }
    }

    private void CheckLValPattern(Node expr, BindingType bindingType = BindingType.None, HashSet<string>? checkClashes = null, LeftHandSideKind lhsKind = LeftHandSideKind.Unknown)
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
                CheckLValSimple(expr, bindingType, checkClashes, lhsKind);
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
        ref var scope = ref NullRef<Scope>();
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
            // RaiseRecoverable(pos, $"Identifier '{name}' has already been declared"); // original acornjs error reporting
            Raise(pos, string.Format(SyntaxErrorMessages.VarRedeclaration, name));
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

    [DoesNotReturn]
    private void HandleLeftHandSideError(Node node, bool isBinding, LeftHandSideKind lhsKind)
    {
        if (isBinding)
        {
            _tokenizer.MoveTo(node.Start, expressionAllowed: false);
            Next(ignoreEscapeSequenceInKeyword: true);
            Unexpected();
        }
        else
        {
            Raise(node.Start, lhsKind switch
            {
                LeftHandSideKind.Assignment => SyntaxErrorMessages.InvalidLhsInAssignment,
                LeftHandSideKind.PrefixUpdate => SyntaxErrorMessages.InvalidLhsInPrefixOp,
                LeftHandSideKind.PostfixUpdate => SyntaxErrorMessages.InvalidLhsInPostfixOp,
                LeftHandSideKind.ForInOf => SyntaxErrorMessages.InvalidLhsInFor,
                _ => SyntaxErrorMessages.InvalidDestructuringTarget,
            });
        }
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
