using System;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;
using static JavaScriptTextWriter;

public partial class AstToJavaScriptConverter : AstVisitor
{
    // Notes for maintainers:
    // Don't visit nodes by directly calling Visit unless it's necessary for some special reason (but in that case you'll need to setup the context for the visitation manually!)
    // For examples of special reason, see VisitArrayExpression, VisitObjectExpression, VisitImport, etc. In usual cases just use the following predefined visitation helper methods:
    // * Visit statements using VisitStatement / VisitStatementList.
    // * Visit expressions using VisitRootExpression and sub-expressions (expressions inside another expression) using VisitSubExpression / VisitSubExpressionList.
    // * Visit identifiers using VisitAuxiliaryNode when they are binding identifiers (declarations) and visit them using VisitRootExpression when they are identifier references (actual expressions).
    // * Visit any other nodes using VisitAuxiliaryNode / VisitAuxiliaryNodeList.

    private static readonly object s_lastSwitchCaseFlag = new();
    private static readonly object s_forLoopInitDeclarationFlag = new();
    private static readonly object s_bindingPatternAllowsExpressionsFlag = new(); // automatically propagated to sub-patterns

    private readonly bool _ignoreExtensions;

    private WriteContext _writeContext;
    private StatementFlags _currentStatementFlags;
    private ExpressionFlags _currentExpressionFlags;
    private object? _currentAuxiliaryNodeContext;

    public AstToJavaScriptConverter(JavaScriptTextWriter writer, AstToJavaScriptOptions options)
    {
        Writer = writer ?? throw new ArgumentNullException(nameof(writer));

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _ignoreExtensions = options.IgnoreExtensions;
    }

    public JavaScriptTextWriter Writer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected ref WriteContext WriteContext { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _writeContext; }

    protected Node? ParentNode { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _writeContext.ParentNode; }

    protected override void Reset()
    {
        base.Reset();

        _writeContext = default;
        _currentStatementFlags = StatementFlags.None;
        _currentExpressionFlags = ExpressionFlags.None;
        _currentAuxiliaryNodeContext = null;
    }

    public void Convert(Node node)
    {
        if (node is null)
        {
            ThrowArgumentNullException<Node>(nameof(node));
        }

        Reset();

        Visit(node);

        Writer.Finish();
    }

    public override object? Visit(Node node)
    {
        var originalWriteContext = _writeContext;
        _writeContext = new WriteContext(originalWriteContext.Node, node);

        var result = base.Visit(node);

        _writeContext = originalWriteContext;

        return result;
    }

    protected internal override object? VisitAccessorProperty(AccessorProperty node)
    {
        if (node.Decorators.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(node.Decorators), static node => ref node.As<AccessorProperty>().Decorators);
            VisitAuxiliaryNodeList(node.Decorators, separator: string.Empty);

            _writeContext.ClearNodeProperty();
        }

        if (node.Static)
        {
            _writeContext.SetNodeProperty(nameof(node.Static), static node => node.As<AccessorProperty>().Static);
            Writer.WriteKeyword("static", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.ClearNodeProperty();
        }
        else
        {
            Writer.SpaceRecommendedAfterLastToken();
        }

        Writer.WriteKeyword("accessor", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Key), static node => node.As<AccessorProperty>().Key);
        VisitPropertyKey(node.Key, node.Computed, leadingParenFlags: TokenFlags.LeadingSpaceRecommended);

        if (node.Value is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WritePunctuator("=", TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(node.Value), static node => node.As<AccessorProperty>().Value);
            VisitRootExpression(node.Value, RootExpressionFlags(needsParens: ExpressionNeedsParensInList(node.Value)));
        }

        Writer.WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        return node;
    }

    protected internal override object? VisitArrayExpression(ArrayExpression node)
    {
        _writeContext.SetNodeProperty(nameof(node.Elements), static node => ref node.As<ArrayExpression>().Elements);

        Writer.StartArray(node.Elements.Count, ref _writeContext);

        // Elements need special care because it may contain null values denoting omitted elements.

        Writer.StartExpressionList(node.Elements.Count, ref _writeContext);

        for (var i = 0; i < node.Elements.Count; i++)
        {
            var element = node.Elements[i];

            if (element is not null)
            {
                VisitExpressionListItem(element, i, node.Elements.Count, static (@this, node, _, _) =>
                    s_getCombinedSubExpressionFlags(@this, node, SubExpressionFlags(@this.ExpressionNeedsParensInList(node), isLeftMost: false)));
            }
            else
            {
                var originalExpressionFlags = _currentExpressionFlags;
                _currentExpressionFlags = PropagateExpressionFlags(SubExpressionFlags(needsParens: false, isLeftMost: false));

                _writeContext.SetNodePropertyItemIndex(i);
                Writer.StartExpressionListItem(i, node.Elements.Count, (JavaScriptTextWriter.ExpressionFlags)_currentExpressionFlags, ref _writeContext);
                Writer.EndExpressionListItem(i, node.Elements.Count, (JavaScriptTextWriter.ExpressionFlags)_currentExpressionFlags, ref _writeContext);

                _currentExpressionFlags = originalExpressionFlags;
            }
        }

        Writer.EndExpressionList(node.Elements.Count, ref _writeContext);

        Writer.EndArray(node.Elements.Count, ref _writeContext);

        return node;
    }

    protected internal override object? VisitArrayPattern(ArrayPattern node)
    {
        _writeContext.SetNodeProperty(nameof(node.Elements), static node => ref node.As<ArrayPattern>().Elements);

        Writer.StartArray(node.Elements.Count, ref _writeContext);

        // Elements need special care because it may contain null values denoting omitted elements.

        Writer.StartAuxiliaryNodeList<Node?>(node.Elements.Count, ref _writeContext);

        for (var i = 0; i < node.Elements.Count; i++)
        {
            var element = node.Elements[i];

            if (element is not null)
            {
                if (_currentAuxiliaryNodeContext != s_bindingPatternAllowsExpressionsFlag)
                {
                    VisitAuxiliaryNodeListItem(element, i, node.Elements.Count, separator: ",", static delegate { return null; });
                }
                else if (element is not Expression expression)
                {
                    VisitAuxiliaryNodeListItem(element, i, node.Elements.Count, separator: ",", static delegate { return s_bindingPatternAllowsExpressionsFlag; }); // propagate flag to sub-patterns
                }
                else
                {
                    var originalAuxiliaryNodeContext = _currentAuxiliaryNodeContext;
                    _currentAuxiliaryNodeContext = null;

                    _writeContext.SetNodePropertyItemIndex(i);
                    Writer.StartAuxiliaryNodeListItem<Node?>(i, node.Elements.Count, separator: ",", _currentAuxiliaryNodeContext, ref _writeContext);
                    VisitRootExpression(expression, RootExpressionFlags(needsParens: ExpressionNeedsParensInList(expression)));
                    Writer.EndAuxiliaryNodeListItem<Node?>(i, node.Elements.Count, separator: ",", _currentAuxiliaryNodeContext, ref _writeContext);

                    _currentAuxiliaryNodeContext = originalAuxiliaryNodeContext;
                }
            }
            else
            {
                var originalAuxiliaryNodeContext = _currentAuxiliaryNodeContext;
                _currentAuxiliaryNodeContext = null;

                _writeContext.SetNodePropertyItemIndex(i);
                Writer.StartAuxiliaryNodeListItem<Node?>(i, node.Elements.Count, separator: ",", _currentAuxiliaryNodeContext, ref _writeContext);
                Writer.EndAuxiliaryNodeListItem<Node?>(i, node.Elements.Count, separator: ",", _currentAuxiliaryNodeContext, ref _writeContext);

                _currentAuxiliaryNodeContext = originalAuxiliaryNodeContext;
            }
        }

        Writer.EndAuxiliaryNodeList<Node?>(node.Elements.Count, ref _writeContext);

        Writer.EndArray(node.Elements.Count, ref _writeContext);

        return node;
    }

    protected internal override object? VisitArrowFunctionExpression(ArrowFunctionExpression node)
    {
        if (node.Async)
        {
            _writeContext.SetNodeProperty(nameof(node.Async), static node => node.As<ArrowFunctionExpression>().Async);
            Writer.WriteKeyword("async", TokenFlags.TrailingSpaceRecommended, ref _writeContext);
        }

        _writeContext.SetNodeProperty(nameof(node.Params), static node => ref node.As<ArrowFunctionExpression>().Params);

        if (node.Params.Count == 1 && node.Params[0].Type == NodeType.Identifier)
        {
            VisitAuxiliaryNodeList(in node.Params, separator: ",");
        }
        else
        {
            Writer.WritePunctuator("(", TokenFlags.Leading, ref _writeContext);
            VisitAuxiliaryNodeList(in node.Params, separator: ",");
            Writer.WritePunctuator(")", TokenFlags.Trailing, ref _writeContext);
        }

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator("=>", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<ArrowFunctionExpression>().Body);
        if (node.Body is FunctionBody functionBody)
        {
            VisitStatement(functionBody, StatementFlags.IsRightMost);
        }
        else
        {
            var bodyExpression = node.Body.As<Expression>();
            var bodyNeedsParens = UnaryOperandNeedsParens(node, bodyExpression);
            VisitExpression(bodyExpression, SubExpressionFlags(bodyNeedsParens, isLeftMost: false), static (@this, node, flags) =>
                @this.DisambiguateExpression(node, ExpressionFlags.IsInsideArrowFunctionBody | ExpressionFlags.IsLeftMostInArrowFunctionBody | @this.PropagateExpressionFlags(flags)));
        }

        return node;
    }

    protected internal override object? VisitAssignmentExpression(AssignmentExpression node)
    {
        _writeContext.SetNodeProperty(nameof(node.Left), static node => node.As<AssignmentExpression>().Left);
        if (node.Left is Expression leftExpression)
        {
            VisitSubExpression(leftExpression, SubExpressionFlags(needsParens: false, isLeftMost: true));
        }
        else
        {
            VisitAuxiliaryNode(node.Left, static delegate { return s_bindingPatternAllowsExpressionsFlag; });
        }

        var op = AssignmentExpression.OperatorToString(node.Operator)
            ?? throw new InvalidOperationException(ExtrasExceptionMessages.InvalidAssignmentOperator);

        _writeContext.SetNodeProperty(nameof(node.Operator), static node => node.As<AssignmentExpression>().Operator);
        Writer.WritePunctuator(op, TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended | TokenFlags.IsAssignmentOperator, ref _writeContext);

        // AssignmentExpression is not a real binary operation because its left side is not an expression. 
        var rightNeedsParens = GetOperatorPrecedence(node, out _) > GetOperatorPrecedence(node.Right, out _);

        _writeContext.SetNodeProperty(nameof(node.Right), static node => node.As<AssignmentExpression>().Right);
        VisitSubExpression(node.Right, SubExpressionFlags(rightNeedsParens, isLeftMost: false));

        return node;
    }

    protected internal override object? VisitAssignmentPattern(AssignmentPattern node)
    {
        _writeContext.SetNodeProperty(nameof(node.Left), static node => node.As<AssignmentPattern>().Left);
        if (_currentAuxiliaryNodeContext != s_bindingPatternAllowsExpressionsFlag)
        {
            VisitAuxiliaryNode(node.Left);
        }
        else if (node.Left is not Expression leftExpression)
        {
            VisitAuxiliaryNode(node.Left, static delegate { return s_bindingPatternAllowsExpressionsFlag; }); // propagate flag to sub-patterns
        }
        else
        {
            VisitRootExpression(leftExpression, RootExpressionFlags(needsParens: ExpressionNeedsParensInList(leftExpression)));
        }

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator("=", TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Right), static node => node.As<AssignmentPattern>().Right);
        VisitRootExpression(node.Right, RootExpressionFlags(needsParens: ExpressionNeedsParensInList(node.Right)));

        return node;
    }

    protected internal override object? VisitAssignmentProperty(AssignmentProperty node)
    {
        if (!node.Shorthand)
        {
            _writeContext.SetNodeProperty(nameof(node.Key), static node => node.As<Property>().Key);
            VisitPropertyKey(node.Key, node.Computed, leadingParenFlags: TokenFlags.LeadingSpaceRecommended);
            Writer.WritePunctuator(":", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);
        }

        _writeContext.SetNodeProperty(nameof(node.Value), static node => node.As<Property>().Value);

        if (_currentAuxiliaryNodeContext != s_bindingPatternAllowsExpressionsFlag)
        {
            VisitAuxiliaryNode(node.Value);
        }
        else if (node.Value is not Expression valueExpression)
        {
            VisitAuxiliaryNode(node.Value, static delegate { return s_bindingPatternAllowsExpressionsFlag; }); // propagate flag to sub-patterns
        }
        else
        {
            VisitRootExpression(valueExpression, RootExpressionFlags(needsParens: ExpressionNeedsParensInList(valueExpression)));
        }

        return node;
    }

    protected internal override object? VisitAwaitExpression(AwaitExpression node)
    {
        Writer.WriteKeyword("await", TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        var argumentNeedsParens = UnaryOperandNeedsParens(node, node.Argument);

        _writeContext.SetNodeProperty(nameof(node.Argument), static node => node.As<AwaitExpression>().Argument);
        VisitSubExpression(node.Argument, SubExpressionFlags(argumentNeedsParens, isLeftMost: false));

        return node;
    }

    protected internal override object? VisitBinaryExpression(BinaryExpression node)
    {
        var operationFlags = BinaryOperandsNeedParens(node, node.Left, node.Right);
        if (!operationFlags.HasFlagFast(BinaryOperationFlags.LeftOperandNeedsParens))
        {
            if (
                // The operand of unary operators cannot be an exponentiation without grouping.
                // E.g. -1 ** 2 is syntactically unambiguous but the language requires (-1) ** 2 instead.
                node.Operator == Operator.Exponentiation && node.Left.Type == NodeType.UnaryExpression ||
                // Logical expressions which mix nullish coalescing and logical AND/OR operators (e.g. (a ?? b) || c or (a && b) ?? c)
                // needs to be parenthesized despite the operator of the parenthesized sub-expression having the same or higher precedence.
                node.Operator == Operator.NullishCoalescing && node.Left is LogicalExpression { Operator: Operator.LogicalAnd or Operator.LogicalOr } ||
                node.Operator is Operator.LogicalAnd or Operator.LogicalOr && node.Left is LogicalExpression { Operator: Operator.NullishCoalescing })
            {
                operationFlags |= BinaryOperationFlags.LeftOperandNeedsParens;
            }
        }

        _writeContext.SetNodeProperty(nameof(node.Left), static node => node.As<BinaryExpression>().Left);
        VisitSubExpression(node.Left, SubExpressionFlags(operationFlags.HasFlagFast(BinaryOperationFlags.LeftOperandNeedsParens), isLeftMost: true));

        var op = (node.Type == NodeType.LogicalExpression
            ? LogicalExpression.OperatorToString(node.Operator)
            : NonLogicalBinaryExpression.OperatorToString(node.Operator))
            ?? throw new InvalidOperationException(ExtrasExceptionMessages.InvalidBinaryOperator);

        _writeContext.SetNodeProperty(nameof(node.Operator), static node => node.As<BinaryExpression>().Operator);
        if (op[0].IsBasicLatinLetter())
        {
            Writer.WriteKeyword(op, TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        }
        else
        {
            Writer.WritePunctuator(op, TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended | TokenFlags.IsBinaryOperator, ref _writeContext);

            if (!operationFlags.HasFlagFast(BinaryOperationFlags.RightOperandNeedsParens))
            {
                if (
                    // Logical expressions which mix nullish coalescing and logical AND operators (e.g. a ?? (b && c))
                    // needs to be parenthesized despite the operator of the parenthesized sub-expression having higher precedence.
                    node.Operator == Operator.NullishCoalescing && node.Right is LogicalExpression { Operator: Operator.LogicalAnd })
                {
                    operationFlags |= BinaryOperationFlags.RightOperandNeedsParens;
                }
            }
        }

        _writeContext.SetNodeProperty(nameof(node.Right), static node => node.As<BinaryExpression>().Right);
        VisitSubExpression(node.Right, SubExpressionFlags(operationFlags.HasFlagFast(BinaryOperationFlags.RightOperandNeedsParens), isLeftMost: false));

        return node;
    }

    protected internal override object? VisitBlockStatement(BlockStatement node)
    {
        _writeContext.SetNodeProperty(nameof(node.Body), static node => ref node.As<BlockStatement>().Body);
        Writer.StartBlock(node.Body.Count, ref _writeContext);

        VisitStatementList(in node.Body);

        Writer.EndBlock(node.Body.Count, ref _writeContext);

        return node;
    }

    protected internal override object? VisitBreakStatement(BreakStatement node)
    {
        Writer.WriteKeyword("break", TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        if (node.Label is not null)
        {
            _writeContext.SetNodeProperty(nameof(node.Label), static node => node.As<BreakStatement>().Label);
            VisitRootExpression(node.Label, RootExpressionFlags(needsParens: false));
        }

        StatementNeedsSemicolon();

        return node;
    }

    protected internal override object? VisitCallExpression(CallExpression node)
    {
        var calleeNeedsParens = UnaryOperandNeedsParens(node, node.Callee);

        _writeContext.SetNodeProperty(nameof(node.Callee), static node => node.As<CallExpression>().Callee);
        VisitSubExpression(node.Callee, SubExpressionFlags(calleeNeedsParens, isLeftMost: true));

        if (node.Optional)
        {
            _writeContext.ClearNodeProperty();
            Writer.WritePunctuator("?.", TokenFlags.InBetween, ref _writeContext);
        }

        _writeContext.SetNodeProperty(nameof(node.Arguments), static node => ref node.As<CallExpression>().Arguments);
        Writer.WritePunctuator("(", TokenFlags.Leading, ref _writeContext);
        VisitSubExpressionList(in node.Arguments);
        Writer.WritePunctuator(")", TokenFlags.Trailing, ref _writeContext);

        return node;
    }

    protected internal override object? VisitCatchClause(CatchClause node)
    {
        if (node.Param is not null)
        {
            _writeContext.SetNodeProperty(nameof(node.Param), static node => node.As<CatchClause>().Param);
            Writer.WritePunctuator("(", TokenFlags.Leading | TokenFlags.LeadingSpaceRecommended, ref _writeContext);
            VisitAuxiliaryNode(node.Param);
            Writer.WritePunctuator(")", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);
        }

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<CatchClause>().Body);
        VisitStatement(node.Body, StatementBodyFlags(isRightMost: ParentNode?.As<TryStatement>().Finalizer is null));

        return node;
    }

    protected internal override object? VisitChainExpression(ChainExpression node)
    {
        _writeContext.SetNodeProperty(nameof(node.Expression), static node => node.As<ChainExpression>().Expression);
        VisitSubExpression(node.Expression, SubExpressionFlags(needsParens: false, isLeftMost: true));

        return node;
    }

    protected internal override object? VisitClassBody(ClassBody node)
    {
        _writeContext.SetNodeProperty(nameof(node.Body), static node => ref node.As<ClassBody>().Body);
        Writer.StartBlock(node.Body.Count, ref _writeContext);

        VisitAuxiliaryNodeList(in node.Body, separator: string.Empty);

        Writer.EndBlock(node.Body.Count, ref _writeContext);

        return node;
    }

    protected internal override object? VisitClassDeclaration(ClassDeclaration node)
    {
        if (node.Decorators.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(node.Decorators), static node => ref node.As<ClassDeclaration>().Decorators);
            VisitAuxiliaryNodeList(node.Decorators, separator: string.Empty);

            _writeContext.ClearNodeProperty();
        }

        Writer.WriteKeyword("class", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        if (node.Id is not null)
        {
            _writeContext.SetNodeProperty(nameof(node.Id), static node => node.As<ClassDeclaration>().Id);
            VisitAuxiliaryNode(node.Id);
        }

        if (node.SuperClass is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("extends", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(node.SuperClass), static node => node.As<ClassDeclaration>().SuperClass);
            VisitRootExpression(node.SuperClass, ExpressionFlags.IsInsideLeftHandSideExpression | ExpressionFlags.IsLeftMostInLeftHandSideExpression | RootExpressionFlags(needsParens: false));
        }

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<ClassDeclaration>().Body);
        VisitAuxiliaryNode(node.Body);

        return node;
    }

    protected internal override object? VisitClassExpression(ClassExpression node)
    {
        if (node.Decorators.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(node.Decorators), static node => ref node.As<ClassExpression>().Decorators);
            VisitAuxiliaryNodeList(node.Decorators, separator: string.Empty);

            _writeContext.ClearNodeProperty();
        }

        Writer.WriteKeyword("class", TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        if (node.Id is not null)
        {
            _writeContext.SetNodeProperty(nameof(node.Id), static node => node.As<ClassExpression>().Id);
            VisitAuxiliaryNode(node.Id);
        }

        if (node.SuperClass is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("extends", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(node.SuperClass), static node => node.As<ClassExpression>().SuperClass);
            VisitRootExpression(node.SuperClass, ExpressionFlags.IsInsideLeftHandSideExpression | ExpressionFlags.IsLeftMostInLeftHandSideExpression | RootExpressionFlags(needsParens: false));
        }

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<ClassExpression>().Body);
        VisitAuxiliaryNode(node.Body);

        return node;
    }

    protected internal override object? VisitConditionalExpression(ConditionalExpression node)
    {
        // Test expressions with the same precendence as ternary operator (such as nested conditional expression, assignment, yield, etc.) also needs parentheses.
        var operandNeedsParens = GetOperatorPrecedence(node, out _) >= GetOperatorPrecedence(node.Test, out _);

        _writeContext.SetNodeProperty(nameof(node.Test), static node => node.As<ConditionalExpression>().Test);
        VisitSubExpression(node.Test, SubExpressionFlags(operandNeedsParens, isLeftMost: true));

        // Consequent expressions with the same precendence as ternary operator are unambiguous without parentheses.
        operandNeedsParens = GetOperatorPrecedence(node, out _) > GetOperatorPrecedence(node.Consequent, out _);

        _writeContext.SetNodeProperty(nameof(node.Consequent), static node => node.As<ConditionalExpression>().Consequent);
        Writer.WritePunctuator("?", TokenFlags.Leading | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        VisitExpression(node.Consequent, SubExpressionFlags(operandNeedsParens, isLeftMost: false), static (@this, node, flags) =>
            // Edge case: 'in' operators in for...in loop declarations are not ambigous when they are in the consequent part of the conditional expression.
            @this.DisambiguateExpression(node, ~ExpressionFlags.IsInAmbiguousInOperatorContext & @this.PropagateExpressionFlags(flags)));

        // Alternate expressions with the same precendence as ternary operator are unambiguous without parentheses, even conditional expressions because of right-to-left associativity.
        operandNeedsParens = GetOperatorPrecedence(node, out _) > GetOperatorPrecedence(node.Alternate, out _);

        _writeContext.SetNodeProperty(nameof(node.Alternate), static node => node.As<ConditionalExpression>().Alternate);
        Writer.WritePunctuator(":", TokenFlags.Leading | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        VisitSubExpression(node.Alternate, SubExpressionFlags(operandNeedsParens, isLeftMost: false));

        return node;
    }

    protected internal override object? VisitContinueStatement(ContinueStatement node)
    {
        Writer.WriteKeyword("continue", TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        if (node.Label is not null)
        {
            _writeContext.SetNodeProperty(nameof(node.Label), static node => node.As<ContinueStatement>().Label);
            VisitRootExpression(node.Label, RootExpressionFlags(needsParens: false));
        }

        StatementNeedsSemicolon();

        return node;
    }

    protected internal override object? VisitDebuggerStatement(DebuggerStatement node)
    {
        Writer.WriteKeyword("debugger", TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        StatementNeedsSemicolon();

        return node;
    }

    protected internal override object? VisitDecorator(Decorator node)
    {
        // https://github.com/tc39/proposal-decorators

        Writer.WritePunctuator("@", TokenFlags.Leading | (ParentNode is not Expression).ToFlag(TokenFlags.LeadingSpaceRecommended), ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Expression), static node => node.As<Decorator>().Expression);
        VisitRootExpression(node.Expression, ExpressionFlags.IsInsideDecorator | RootExpressionFlags(needsParens: false));

        Writer.SpaceRecommendedAfterLastToken();

        return node;
    }

    protected internal override object? VisitDoWhileStatement(DoWhileStatement node)
    {
        Writer.WriteKeyword("do", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<DoWhileStatement>().Body);
        StatementFlags bodyFlags;
        VisitStatement(node.Body, bodyFlags = StatementBodyFlags(isRightMost: false));

        _writeContext.ClearNodeProperty();
        Writer.WriteKeyword("while", TokenFlags.SurroundingSpaceRecommended | StatementBodyFlagsToKeywordFlags(bodyFlags), ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Test), static node => node.As<DoWhileStatement>().Test);
        VisitRootExpression(node.Test, ExpressionFlags.SpaceBeforeParensRecommended | RootExpressionFlags(needsParens: true));

        return node;
    }

    protected internal override object? VisitEmptyStatement(EmptyStatement node)
    {
        Writer.WritePunctuator(";", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        return node;
    }

    protected internal override object? VisitExportAllDeclaration(ExportAllDeclaration node)
    {
        Writer.WriteKeyword("export", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        Writer.WritePunctuator("*", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        if (node.Exported is not null)
        {
            _writeContext.SetNodeProperty(nameof(node.Exported), static node => node.As<ExportAllDeclaration>().Exported);
            Writer.WriteKeyword("as", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            VisitExportOrImportSpecifierIdentifier(node.Exported);
        }

        _writeContext.ClearNodeProperty();
        Writer.WriteKeyword("from", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Source), static node => node.As<ExportAllDeclaration>().Source);
        VisitRootExpression(node.Source, RootExpressionFlags(needsParens: false));

        if (node.Attributes.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(node.Attributes), static node => ref node.As<ExportAllDeclaration>().Attributes);
            VisitImportAttributes(in node.Attributes);
        }

        StatementNeedsSemicolon();

        return node;
    }

    protected internal override object? VisitExportDefaultDeclaration(ExportDefaultDeclaration node)
    {
        Writer.WriteKeyword("export", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        Writer.WriteKeyword("default", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Declaration), static node => node.As<ExportDefaultDeclaration>().Declaration);
        if (node.Declaration is Declaration declaration)
        {
            VisitStatement(declaration, StatementFlags.IsRightMost);
        }
        else
        {
            VisitRootExpression(node.Declaration.As<Expression>(), ExpressionFlags.IsInsideExportDefaultExpression | RootExpressionFlags(needsParens: false));

            StatementNeedsSemicolon();
        }

        return node;
    }

    protected internal override object? VisitExportNamedDeclaration(ExportNamedDeclaration node)
    {
        Writer.WriteKeyword("export", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        if (node.Declaration is not null)
        {
            _writeContext.SetNodeProperty(nameof(node.Declaration), static node => node.As<ExportNamedDeclaration>().Declaration);
            VisitStatement(node.Declaration.As<Declaration>(), StatementFlags.IsRightMost);
        }
        else
        {
            _writeContext.SetNodeProperty(nameof(node.Specifiers), static node => ref node.As<ExportNamedDeclaration>().Specifiers);
            Writer.WritePunctuator("{", TokenFlags.Leading | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
            VisitAuxiliaryNodeList(in node.Specifiers, separator: ",");
            Writer.WritePunctuator("}", TokenFlags.Trailing | TokenFlags.LeadingSpaceRecommended, ref _writeContext);

            if (node.Source is not null)
            {
                _writeContext.ClearNodeProperty();
                Writer.WriteKeyword("from", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

                _writeContext.SetNodeProperty(nameof(node.Source), static node => node.As<ExportNamedDeclaration>().Source);
                VisitRootExpression(node.Source, RootExpressionFlags(needsParens: false));

                if (node.Attributes.Count > 0)
                {
                    _writeContext.SetNodeProperty(nameof(node.Attributes), static node => ref node.As<ExportNamedDeclaration>().Attributes);
                    VisitImportAttributes(in node.Attributes);
                }
            }

            StatementNeedsSemicolon();
        }

        return node;
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier node)
    {
        _writeContext.SetNodeProperty(nameof(node.Local), static node => node.As<ExportSpecifier>().Local);
        VisitExportOrImportSpecifierIdentifier(node.Local);

        if (!ReferenceEquals(node.Local, node.Exported))
        {
            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("as", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(node.Exported), static node => node.As<ExportSpecifier>().Exported);
            VisitExportOrImportSpecifierIdentifier(node.Exported);
        }

        return node;
    }

    protected internal override object? VisitExpressionStatement(ExpressionStatement node)
    {
        Writer.SpaceRecommendedAfterLastToken();

        _writeContext.SetNodeProperty(nameof(node.Expression), static node => node.As<ExpressionStatement>().Expression);
        VisitRootExpression(node.Expression, ExpressionFlags.IsInsideStatementExpression | RootExpressionFlags(needsParens: false));

        StatementNeedsSemicolon();

        return node;
    }

    protected internal override object? VisitExtension(Node node)
    {
        if (_ignoreExtensions)
        {
            Writer.WriteBlockComment(new[] { $" Unsupported node type ({node.GetType()}). " }, TriviaFlags.None);
            return node;
        }
        else
        {
            return base.VisitExtension(node);
        }
    }

    protected internal override object? VisitForInStatement(ForInStatement node)
    {
        Writer.WriteKeyword("for", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        Writer.WritePunctuator("(", TokenFlags.Leading | TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Left), static node => node.As<ForInStatement>().Left);

        if (node.Left is VariableDeclaration variableDeclaration)
        {
            VisitStatement(variableDeclaration, StatementFlags.NestedVariableDeclaration);
        }
        else if (node.Left is Expression leftExpression)
        {
            VisitRootExpression(leftExpression, RootExpressionFlags(needsParens: false));
        }
        else
        {
            VisitAuxiliaryNode(node.Left, static delegate { return s_bindingPatternAllowsExpressionsFlag; });
        }

        _writeContext.ClearNodeProperty();
        Writer.WriteKeyword("in", TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Right), static node => node.As<ForInStatement>().Right);
        VisitRootExpression(node.Right, RootExpressionFlags(needsParens: false));

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator(")", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<ForInStatement>().Body);
        VisitStatement(node.Body, StatementBodyFlags(isRightMost: true));

        return node;
    }

    protected internal override object? VisitForOfStatement(ForOfStatement node)
    {
        Writer.WriteKeyword("for", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        if (node.Await)
        {
            _writeContext.SetNodeProperty(nameof(node.Await), static node => node.As<ForOfStatement>().Await);
            Writer.WriteKeyword("await", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        }

        Writer.WritePunctuator("(", TokenFlags.Leading | TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Left), static node => node.As<ForOfStatement>().Left);

        if (node.Left is VariableDeclaration variableDeclaration)
        {
            VisitStatement(variableDeclaration, StatementFlags.NestedVariableDeclaration);
        }
        else if (node.Left is Expression leftExpression)
        {
            VisitRootExpression(leftExpression, RootExpressionFlags(needsParens: false));
        }
        else
        {
            VisitAuxiliaryNode(node.Left, static delegate { return s_bindingPatternAllowsExpressionsFlag; });
        }

        _writeContext.ClearNodeProperty();
        Writer.WriteKeyword("of", TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Right), static node => node.As<ForOfStatement>().Right);
        VisitRootExpression(node.Right, RootExpressionFlags(needsParens: ExpressionNeedsParensInList(node.Right)));

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator(")", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<ForOfStatement>().Body);
        VisitStatement(node.Body, StatementBodyFlags(isRightMost: true));

        return node;
    }

    protected internal override object? VisitForStatement(ForStatement node)
    {
        Writer.WriteKeyword("for", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        Writer.WritePunctuator("(", TokenFlags.Leading | TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Init), static node => node.As<ForStatement>().Init);

        if (node.Init is not null)
        {
            if (node.Init is VariableDeclaration variableDeclaration)
            {
                VisitStatement(variableDeclaration, StatementFlags.NestedVariableDeclaration);
            }
            else
            {
                VisitRootExpression(node.Init.As<Expression>(), ExpressionFlags.IsInAmbiguousInOperatorContext | RootExpressionFlags(needsParens: false));
            }
        }

        Writer.WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Test), static node => node.As<ForStatement>().Test);

        if (node.Test is not null)
        {
            VisitRootExpression(node.Test, RootExpressionFlags(needsParens: false));
        }

        Writer.WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        if (node.Update is not null)
        {
            _writeContext.SetNodeProperty(nameof(node.Update), static node => node.As<ForStatement>().Update);

            VisitRootExpression(node.Update, RootExpressionFlags(needsParens: false));
        }

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator(")", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<ForStatement>().Body);
        VisitStatement(node.Body, StatementBodyFlags(isRightMost: true));

        return node;
    }

    protected internal override object? VisitFunctionBody(FunctionBody node)
    {
        return VisitBlockStatement(node);
    }

    protected internal override object? VisitFunctionDeclaration(FunctionDeclaration node)
    {
        if (node.Async)
        {
            _writeContext.SetNodeProperty(nameof(node.Async), static node => node.As<FunctionDeclaration>().Async);
            Writer.WriteKeyword("async", TokenFlags.LeadingSpaceRecommended, ref _writeContext);

            _writeContext.ClearNodeProperty();
        }

        Writer.WriteKeyword("function", TokenFlags.LeadingSpaceRecommended, ref _writeContext);

        if (node.Generator)
        {
            _writeContext.SetNodeProperty(nameof(node.Generator), static node => node.As<FunctionDeclaration>().Generator);
            Writer.WritePunctuator("*", (node.Id is not null).ToFlag(TokenFlags.TrailingSpaceRecommended), ref _writeContext);
        }

        if (node.Id is not null)
        {
            _writeContext.SetNodeProperty(nameof(node.Id), static node => node.As<FunctionDeclaration>().Id);
            VisitAuxiliaryNode(node.Id);
        }

        _writeContext.SetNodeProperty(nameof(node.Params), static node => ref node.As<FunctionDeclaration>().Params);
        Writer.WritePunctuator("(", TokenFlags.Leading, ref _writeContext);
        VisitAuxiliaryNodeList(in node.Params, separator: ",");
        Writer.WritePunctuator(")", TokenFlags.Trailing, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<FunctionDeclaration>().Body);
        VisitStatement(node.Body, StatementBodyFlags(isRightMost: true));

        return node;
    }

    protected internal override object? VisitFunctionExpression(FunctionExpression node)
    {
        if (!_currentExpressionFlags.HasFlagFast(ExpressionFlags.IsMethod))
        {
            if (node.Async)
            {
                _writeContext.SetNodeProperty(nameof(node.Async), static node => node.As<FunctionExpression>().Async);
                Writer.WriteKeyword("async", ref _writeContext);

                _writeContext.ClearNodeProperty();
            }

            Writer.WriteKeyword("function", ref _writeContext);

            if (node.Generator)
            {
                _writeContext.SetNodeProperty(nameof(node.Generator), static node => node.As<FunctionExpression>().Generator);
                Writer.WritePunctuator("*", (node.Id is not null).ToFlag(TokenFlags.TrailingSpaceRecommended), ref _writeContext);
            }

            if (node.Id is not null)
            {
                _writeContext.SetNodeProperty(nameof(node.Id), static node => node.As<FunctionExpression>().Id);
                VisitAuxiliaryNode(node.Id);
            }
        }
        else
        {
            var keyIsFirstToken = true;

            if (node.Async)
            {
                _writeContext.SetNodeProperty(nameof(node.Async), static node => node.As<FunctionExpression>().Async);
                Writer.WriteKeyword("async", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

                keyIsFirstToken = false;
            }

            if (node.Generator)
            {
                _writeContext.SetNodeProperty(nameof(node.Generator), static node => node.As<FunctionExpression>().Generator);
                Writer.WritePunctuator("*", TokenFlags.LeadingSpaceRecommended, ref _writeContext);

                keyIsFirstToken = false;
            }

            _writeContext.SetNodeProperty(nameof(node.Id), static node => node.As<FunctionExpression>().Id);
            var property = (IProperty)ParentNode!;
            if (property.Kind != PropertyKind.Constructor || property.Key.Type == NodeType.Literal)
            {
                if (keyIsFirstToken && !property.Computed)
                {
                    Writer.SpaceRecommendedAfterLastToken();
                }

                VisitPropertyKey(property.Key, property.Computed, leadingParenFlags: keyIsFirstToken.ToFlag(TokenFlags.LeadingSpaceRecommended));
            }
            else
            {
                Writer.WriteKeyword("constructor", TokenFlags.LeadingSpaceRecommended, ref _writeContext);
            }
        }

        _writeContext.SetNodeProperty(nameof(node.Params), static node => ref node.As<FunctionExpression>().Params);
        Writer.WritePunctuator("(", TokenFlags.Leading, ref _writeContext);
        VisitAuxiliaryNodeList(in node.Params, separator: ",");
        Writer.WritePunctuator(")", TokenFlags.Trailing, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<FunctionExpression>().Body);
        VisitStatement(node.Body, StatementBodyFlags(isRightMost: true));

        return node;
    }

    protected internal override object? VisitIdentifier(Identifier node)
    {
        _writeContext.SetNodeProperty(nameof(node.Name), static node => node.As<Identifier>().Name);
        Writer.WriteIdentifier(node.Name, ref _writeContext);

        return node;
    }

    protected internal override object? VisitIfStatement(IfStatement node)
    {
        Writer.WriteKeyword("if", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Test), static node => node.As<IfStatement>().Test);
        VisitRootExpression(node.Test, ExpressionFlags.SpaceAroundParensRecommended | RootExpressionFlags(needsParens: true));

        _writeContext.SetNodeProperty(nameof(node.Consequent), static node => node.As<IfStatement>().Consequent);
        StatementFlags bodyFlags;
        VisitStatement(node.Consequent, bodyFlags = StatementBodyFlags(isRightMost: node.Alternate is null));

        if (node.Alternate is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("else", TokenFlags.SurroundingSpaceRecommended | StatementBodyFlagsToKeywordFlags(bodyFlags), ref _writeContext);

            _writeContext.SetNodeProperty(nameof(node.Alternate), static node => node.As<IfStatement>().Alternate);
            VisitStatement(node.Alternate, StatementBodyFlags(isRightMost: true));
        }

        return node;
    }

    protected internal override object? VisitImportAttribute(ImportAttribute node)
    {
        // https://github.com/tc39/proposal-import-attributes#import-statements

        _writeContext.SetNodeProperty(nameof(node.Key), static node => node.As<ImportAttribute>().Key);
        VisitPropertyKey(node.Key, computed: false);
        Writer.WritePunctuator(":", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Value), static node => node.As<ImportAttribute>().Value);
        VisitRootExpression(node.Value, RootExpressionFlags(needsParens: false));

        return node;
    }

    protected internal override object? VisitImportDeclaration(ImportDeclaration node)
    {
        Writer.WriteKeyword("import", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        // Specifiers need special care because of the unusual syntax.

        _writeContext.SetNodeProperty(nameof(node.Specifiers), static node => ref node.As<ImportDeclaration>().Specifiers);
        Writer.StartAuxiliaryNodeList<ImportDeclarationSpecifier>(node.Specifiers.Count, ref _writeContext);

        if (node.Specifiers.Count == 0)
        {
            Writer.EndAuxiliaryNodeList<ImportDeclarationSpecifier>(count: 0, ref _writeContext);

            goto WriteSource;
        }

        var index = 0;
        Func<AstToJavaScriptConverter, Node?, int, int, object?> getNodeContext = static delegate { return null; };

        if (node.Specifiers[index].Type == NodeType.ImportDefaultSpecifier)
        {
            VisitAuxiliaryNodeListItem(node.Specifiers[index], index, node.Specifiers.Count, ",", getNodeContext);

            if (++index >= node.Specifiers.Count)
            {
                goto EndSpecifiers;
            }
        }

        if (node.Specifiers[index].Type == NodeType.ImportNamespaceSpecifier)
        {
            VisitAuxiliaryNodeListItem(node.Specifiers[index], index, node.Specifiers.Count, ",", getNodeContext);

            if (++index >= node.Specifiers.Count)
            {
                goto EndSpecifiers;
            }
        }

        Writer.WritePunctuator("{", TokenFlags.Leading | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        for (; index < node.Specifiers.Count; index++)
        {
            VisitAuxiliaryNodeListItem(node.Specifiers[index], index, node.Specifiers.Count, ",", getNodeContext);
        }

        Writer.WritePunctuator("}", TokenFlags.Trailing | TokenFlags.LeadingSpaceRecommended, ref _writeContext);

    EndSpecifiers:
        Writer.EndAuxiliaryNodeList<ImportDeclarationSpecifier>(node.Specifiers.Count, ref _writeContext);

        _writeContext.ClearNodeProperty();
        Writer.WriteKeyword("from", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

    WriteSource:
        _writeContext.SetNodeProperty(nameof(node.Source), static node => node.As<ImportDeclaration>().Source);
        VisitRootExpression(node.Source, RootExpressionFlags(needsParens: false));

        if (node.Attributes.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(node.Attributes), static node => ref node.As<ImportDeclaration>().Attributes);
            VisitImportAttributes(in node.Attributes);
        }

        StatementNeedsSemicolon();

        return node;
    }

    protected internal override object? VisitImportDefaultSpecifier(ImportDefaultSpecifier node)
    {
        _writeContext.SetNodeProperty(nameof(node.Local), static node => node.As<ImportDefaultSpecifier>().Local);
        VisitAuxiliaryNode(node.Local);

        return node;
    }

    protected internal override object? VisitImportExpression(ImportExpression node)
    {
        Writer.WriteKeyword("import", ref _writeContext);

        Writer.WritePunctuator("(", TokenFlags.Leading, ref _writeContext);

        // ImportExpression arguments need special care because of the unusual model (separate expressions instead of an expression list).

        var paramCount = node.Options is null ? 1 : 2;
        Writer.StartExpressionList(paramCount, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(ImportExpression.Source), static node => node.As<ImportExpression>().Source);
        VisitExpressionListItem(node.Source, 0, paramCount, static (@this, node, _, _) =>
            s_getCombinedSubExpressionFlags(@this, node, SubExpressionFlags(@this.ExpressionNeedsParensInList(node), isLeftMost: false)));

        if (node.Options is not null)
        {
            // https://github.com/tc39/proposal-import-attributes#dynamic-import

            _writeContext.SetNodeProperty(nameof(ImportExpression.Options), static node => node.As<ImportExpression>().Options);
            VisitExpressionListItem(node.Options, 1, paramCount, static (@this, node, _, _) =>
                s_getCombinedSubExpressionFlags(@this, node, SubExpressionFlags(@this.ExpressionNeedsParensInList(node), isLeftMost: false)));
        }

        Writer.EndExpressionList(paramCount, ref _writeContext);

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator(")", TokenFlags.Trailing, ref _writeContext);

        return node;
    }

    protected internal override object? VisitImportNamespaceSpecifier(ImportNamespaceSpecifier node)
    {
        Writer.WritePunctuator("*", TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        Writer.WriteKeyword("as", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Local), static node => node.As<ImportNamespaceSpecifier>().Local);
        VisitAuxiliaryNode(node.Local);

        return node;
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier node)
    {
        if (!ReferenceEquals(node.Imported, node.Local))
        {
            _writeContext.SetNodeProperty(nameof(node.Imported), static node => node.As<ImportSpecifier>().Imported);
            VisitExportOrImportSpecifierIdentifier(node.Imported);

            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("as", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        }

        _writeContext.SetNodeProperty(nameof(node.Local), static node => node.As<ImportSpecifier>().Local);
        VisitAuxiliaryNode(node.Local);

        return node;
    }

    protected internal override object? VisitLabeledStatement(LabeledStatement node)
    {
        Writer.SpaceRecommendedAfterLastToken();

        _writeContext.SetNodeProperty(nameof(node.Label), static node => node.As<LabeledStatement>().Label);
        VisitAuxiliaryNode(node.Label);

        Writer.WritePunctuator(":", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<LabeledStatement>().Body);
        VisitStatement(node.Body, StatementFlags.IsRightMost);

        return node;
    }

    protected internal override object? VisitLiteral(Literal node)
    {
        _writeContext.SetNodeProperty(nameof(node.Raw), static node => node.As<Literal>().Raw);
        Writer.WriteLiteral(node.Raw, node.Kind, ref _writeContext);

        return node;
    }

    protected internal override object? VisitMemberExpression(MemberExpression node)
    {
        var operationFlags = BinaryOperandsNeedParens(node, node.Object, node.Property);

        // Cases like 1.toString() must be disambiguated with parentheses.
        if (!operationFlags.HasFlagFast(BinaryOperationFlags.LeftOperandNeedsParens) &&
            node is { Computed: false, Optional: false, Object: NumericLiteral numericLiteral } &&
            numericLiteral.Raw.IndexOf('.') < 0)
        {
            operationFlags |= BinaryOperationFlags.LeftOperandNeedsParens;
        }

        _writeContext.SetNodeProperty(nameof(node.Object), static node => node.As<MemberExpression>().Object);
        VisitSubExpression(node.Object, SubExpressionFlags(operationFlags.HasFlagFast(BinaryOperationFlags.LeftOperandNeedsParens), isLeftMost: true));

        if (node.Computed)
        {
            if (node.Optional)
            {
                _writeContext.ClearNodeProperty();
                Writer.WritePunctuator("?.", TokenFlags.InBetween, ref _writeContext);
            }

            _writeContext.SetNodeProperty(nameof(node.Property), static node => node.As<MemberExpression>().Property);
            Writer.WritePunctuator("[", TokenFlags.Leading, ref _writeContext);
            VisitSubExpression(node.Property, SubExpressionFlags(needsParens: false, isLeftMost: false));
            Writer.WritePunctuator("]", TokenFlags.Trailing, ref _writeContext);
        }
        else
        {
            _writeContext.ClearNodeProperty();
            Writer.WritePunctuator(node.Optional ? "?." : ".", TokenFlags.InBetween, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(node.Property), static node => node.As<MemberExpression>().Property);
            VisitSubExpression(node.Property, SubExpressionFlags(needsParens: false, isLeftMost: false));
        }

        return node;
    }

    protected internal override object? VisitMetaProperty(MetaProperty node)
    {
        _writeContext.SetNodeProperty(nameof(node.Meta), static node => node.As<MetaProperty>().Meta);
        Writer.WriteKeyword(node.Meta.Name, ref _writeContext);

        _writeContext.ClearNodeProperty();
        Writer.WritePunctuator(".", TokenFlags.InBetween, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Property), static node => node.As<MetaProperty>().Property);
        VisitSubExpression(node.Property, SubExpressionFlags(needsParens: false, isLeftMost: false));

        return node;
    }

    protected internal override object? VisitMethodDefinition(MethodDefinition node)
    {
        if (node.Decorators.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(node.Decorators), static node => ref node.As<MethodDefinition>().Decorators);
            VisitAuxiliaryNodeList(node.Decorators, separator: string.Empty);

            _writeContext.ClearNodeProperty();
        }

        if (node.Static)
        {
            _writeContext.SetNodeProperty(nameof(node.Static), static node => node.As<MethodDefinition>().Static);
            Writer.WriteKeyword("static", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        }

        switch (node.Kind)
        {
            case PropertyKind.Get:
                _writeContext.SetNodeProperty(nameof(node.Kind), static node => node.As<MethodDefinition>().Kind);
                Writer.WriteKeyword("get", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
                break;
            case PropertyKind.Set:
                _writeContext.SetNodeProperty(nameof(node.Kind), static node => node.As<MethodDefinition>().Kind);
                Writer.WriteKeyword("set", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
                break;
        }

        _writeContext.SetNodeProperty(nameof(node.Value), static node => node.As<MethodDefinition>().Value);
        VisitRootExpression(node.Value, ExpressionFlags.IsMethod | RootExpressionFlags(needsParens: false));

        return node;
    }

    protected internal override object? VisitNewExpression(NewExpression node)
    {
        Writer.WriteKeyword("new", TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        var calleeNeedsParens = UnaryOperandNeedsParens(node, node.Callee);

        _writeContext.SetNodeProperty(nameof(node.Callee), static node => node.As<NewExpression>().Callee);
        VisitExpression(node.Callee, SubExpressionFlags(calleeNeedsParens, isLeftMost: false), static (@this, node, flags) =>
            @this.DisambiguateExpression(node, ExpressionFlags.IsInsideNewCallee | ExpressionFlags.IsLeftMostInNewCallee | @this.PropagateExpressionFlags(flags)));

        if (node.Arguments.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(node.Arguments), static node => ref node.As<NewExpression>().Arguments);
            Writer.WritePunctuator("(", TokenFlags.Leading, ref _writeContext);
            VisitSubExpressionList(in node.Arguments);
            Writer.WritePunctuator(")", TokenFlags.Trailing, ref _writeContext);
        }

        return node;
    }

    protected internal override object? VisitObjectExpression(ObjectExpression node)
    {
        _writeContext.SetNodeProperty(nameof(node.Properties), static node => ref node.As<ObjectExpression>().Properties);

        Writer.StartObject(node.Properties.Count, ref _writeContext);

        // Properties need special care because it may contain spread elements, which are actual expressions (as opposed to normal properties).

        Writer.StartAuxiliaryNodeList<Node>(node.Properties.Count, ref _writeContext);

        for (var i = 0; i < node.Properties.Count; i++)
        {
            var property = node.Properties[i];
            if (property is SpreadElement spreadElement)
            {
                var originalAuxiliaryNodeContext = _currentAuxiliaryNodeContext;
                _currentAuxiliaryNodeContext = null;

                _writeContext.SetNodePropertyItemIndex(i);
                Writer.StartAuxiliaryNodeListItem<Node>(i, node.Properties.Count, separator: ",", _currentAuxiliaryNodeContext, ref _writeContext);
                VisitRootExpression(spreadElement, RootExpressionFlags(needsParens: ExpressionNeedsParensInList(spreadElement)));
                Writer.EndAuxiliaryNodeListItem<Node>(i, node.Properties.Count, separator: ",", _currentAuxiliaryNodeContext, ref _writeContext);

                _currentAuxiliaryNodeContext = originalAuxiliaryNodeContext;
            }
            else
            {
                VisitAuxiliaryNodeListItem(property, i, node.Properties.Count, separator: ",", static delegate { return null; });
            }
        }

        Writer.EndAuxiliaryNodeList<Node>(node.Properties.Count, ref _writeContext);

        Writer.EndObject(node.Properties.Count, ref _writeContext);

        return node;
    }

    protected internal override object? VisitObjectPattern(ObjectPattern node)
    {
        _writeContext.SetNodeProperty(nameof(node.Properties), static node => ref node.As<ObjectPattern>().Properties);

        Writer.StartObject(node.Properties.Count, ref _writeContext);

        VisitAuxiliaryNodeList(in node.Properties, separator: ",", static (@this, _, _, _) =>
            @this._currentAuxiliaryNodeContext == s_bindingPatternAllowsExpressionsFlag
                ? s_bindingPatternAllowsExpressionsFlag // propagate flag to sub-patterns
                : null);

        Writer.EndObject(node.Properties.Count, ref _writeContext);

        return node;
    }

    protected internal override object? VisitObjectProperty(ObjectProperty node)
    {
        bool isMethod;

        switch (node.Kind)
        {
            case PropertyKind.Get:
                _writeContext.SetNodeProperty(nameof(node.Kind), static node => node.As<Property>().Kind);
                Writer.WriteKeyword("get", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

                isMethod = true;
                break;
            case PropertyKind.Set:
                _writeContext.SetNodeProperty(nameof(node.Kind), static node => node.As<Property>().Kind);
                Writer.WriteKeyword("set", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

                isMethod = true;
                break;
            case PropertyKind.Init when node.Method:
                isMethod = true;
                break;
            default:
                if (!node.Shorthand)
                {
                    _writeContext.SetNodeProperty(nameof(node.Key), static node => node.As<Property>().Key);
                    VisitPropertyKey(node.Key, node.Computed, leadingParenFlags: TokenFlags.LeadingSpaceRecommended);
                    Writer.WritePunctuator(":", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);
                }

                isMethod = false;
                break;
        }

        _writeContext.SetNodeProperty(nameof(node.Value), static node => node.As<Property>().Value);

        var valueExpression = node.Value.As<Expression>();
        VisitRootExpression(valueExpression, isMethod.ToFlag(ExpressionFlags.IsMethod) | RootExpressionFlags(needsParens: ExpressionNeedsParensInList(valueExpression)));

        return node;
    }

    protected internal override object? VisitParenthesizedExpression(ParenthesizedExpression node)
    {
        _writeContext.SetNodeProperty(nameof(node.Expression), static node => node.As<ParenthesizedExpression>().Expression);
        VisitSubExpression(node.Expression, SubExpressionFlags(needsParens: true, isLeftMost: true));

        return node;
    }

    protected internal override object? VisitPrivateIdentifier(PrivateIdentifier node)
    {
        _writeContext.SetNodeProperty(nameof(node.Name), static node => node.As<PrivateIdentifier>().Name);
        Writer.WritePunctuator("#", TokenFlags.Leading, ref _writeContext);
        Writer.WriteIdentifier(node.Name, ref _writeContext);

        return node;
    }

    protected internal override object? VisitProgram(Program node)
    {
        _writeContext.SetNodeProperty(nameof(node.Body), static node => ref node.As<Program>().Body);
        VisitStatementList(in node.Body);

        return node;
    }

    protected internal override object? VisitPropertyDefinition(PropertyDefinition node)
    {
        if (node.Decorators.Count > 0)
        {
            _writeContext.SetNodeProperty(nameof(node.Decorators), static node => ref node.As<PropertyDefinition>().Decorators);
            VisitAuxiliaryNodeList(node.Decorators, separator: string.Empty);

            _writeContext.ClearNodeProperty();
        }

        if (node.Static)
        {
            _writeContext.SetNodeProperty(nameof(node.Static), static node => node.As<PropertyDefinition>().Static);
            Writer.WriteKeyword("static", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);
        }
        else
        {
            Writer.SpaceRecommendedAfterLastToken();
        }

        _writeContext.SetNodeProperty(nameof(node.Key), static node => node.As<PropertyDefinition>().Key);
        VisitPropertyKey(node.Key, node.Computed, leadingParenFlags: TokenFlags.LeadingSpaceRecommended);

        if (node.Value is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WritePunctuator("=", TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(node.Value), static node => node.As<PropertyDefinition>().Value);
            VisitRootExpression(node.Value, RootExpressionFlags(needsParens: ExpressionNeedsParensInList(node.Value)));
        }

        Writer.WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        return node;
    }

    protected internal override object? VisitRestElement(RestElement node)
    {
        _writeContext.SetNodeProperty(nameof(node.Argument), static node => node.As<RestElement>().Argument);
        Writer.WritePunctuator("...", TokenFlags.Leading, ref _writeContext);

        if (_currentAuxiliaryNodeContext != s_bindingPatternAllowsExpressionsFlag)
        {
            VisitAuxiliaryNode(node.Argument);
        }
        else if (node.Argument is not Expression argumentExpression)
        {
            VisitAuxiliaryNode(node.Argument, static delegate { return s_bindingPatternAllowsExpressionsFlag; }); // propagate flag to sub-patterns
        }
        else
        {
            VisitRootExpression(argumentExpression, RootExpressionFlags(needsParens: ExpressionNeedsParensInList(argumentExpression)));
        }

        return node;
    }

    protected internal override object? VisitReturnStatement(ReturnStatement node)
    {
        Writer.WriteKeyword("return", (node.Argument is not null).ToFlag(TokenFlags.SurroundingSpaceRecommended, TokenFlags.LeadingSpaceRecommended), ref _writeContext);

        if (node.Argument is not null)
        {
            _writeContext.SetNodeProperty(nameof(node.Argument), static node => node.As<ReturnStatement>().Argument);
            VisitRootExpression(node.Argument, RootExpressionFlags(needsParens: false));
        }

        StatementNeedsSemicolon();

        return node;
    }

    protected internal override object? VisitSequenceExpression(SequenceExpression node)
    {
        _writeContext.SetNodeProperty(nameof(node.Expressions), static node => ref node.As<SequenceExpression>().Expressions);

        VisitExpressionList(in node.Expressions, static (@this, node, index, _) =>
            s_getCombinedSubExpressionFlags(@this, node, SubExpressionFlags(@this.ExpressionNeedsParensInList(node), isLeftMost: index == 0)));

        return node;
    }

    protected internal override object? VisitSpreadElement(SpreadElement node)
    {
        var argumentNeedsParens = UnaryOperandNeedsParens(node, node.Argument);

        _writeContext.SetNodeProperty(nameof(node.Argument), static node => node.As<SpreadElement>().Argument);
        Writer.WritePunctuator("...", TokenFlags.Leading, ref _writeContext);

        VisitSubExpression(node.Argument, SubExpressionFlags(argumentNeedsParens, isLeftMost: false));

        return node;
    }

    protected internal override object? VisitStaticBlock(StaticBlock node)
    {
        Writer.WriteKeyword("static", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Body), static node => ref node.As<StaticBlock>().Body);
        Writer.StartBlock(node.Body.Count, ref _writeContext);

        VisitStatementList(in node.Body);

        Writer.EndBlock(node.Body.Count, ref _writeContext);

        return node;
    }

    protected internal override object? VisitSuper(Super node)
    {
        Writer.WriteKeyword("super", ref _writeContext);

        return node;
    }

    protected internal override object? VisitSwitchCase(SwitchCase node)
    {
        if (node.Test is not null)
        {
            Writer.WriteKeyword("case", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(node.Test), static node => node.As<SwitchCase>().Test);
            VisitRootExpression(node.Test, RootExpressionFlags(needsParens: false));

            _writeContext.ClearNodeProperty();
        }
        else
        {
            Writer.WriteKeyword("default", TokenFlags.LeadingSpaceRecommended, ref _writeContext);
        }

        Writer.WritePunctuator(":", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Consequent), static node => ref node.As<SwitchCase>().Consequent);

        if (_currentAuxiliaryNodeContext == s_lastSwitchCaseFlag)
        {
            // If this is the last case, then the right-most semicolon can be omitted.
            VisitStatementList(in node.Consequent);
        }
        else
        {
            // If this isn't the last case, then the right-most semicolon must not be omitted!
            VisitStatementList(in node.Consequent, static delegate { return StatementFlags.None; });
        }

        return node;
    }

    protected internal override object? VisitSwitchStatement(SwitchStatement node)
    {
        Writer.WriteKeyword("switch", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Discriminant), static node => node.As<SwitchStatement>().Discriminant);
        VisitRootExpression(node.Discriminant, ExpressionFlags.SpaceAroundParensRecommended | RootExpressionFlags(needsParens: true));

        _writeContext.SetNodeProperty(nameof(node.Cases), static node => ref node.As<SwitchStatement>().Cases);
        Writer.StartBlock(node.Cases.Count, ref _writeContext);

        // Passes contextual information about whether it's the last one in the statement or not to each SwitchCase.
        VisitAuxiliaryNodeList(in node.Cases, separator: string.Empty, static (_, _, index, count) =>
            index == count - 1 ? s_lastSwitchCaseFlag : null);

        Writer.EndBlock(node.Cases.Count, ref _writeContext);

        return node;
    }

    protected internal override object? VisitTaggedTemplateExpression(TaggedTemplateExpression node)
    {
        _writeContext.SetNodeProperty(nameof(node.Tag), static node => node.As<TaggedTemplateExpression>().Tag);
        VisitExpression(node.Tag, SubExpressionFlags(needsParens: false, isLeftMost: true), static (@this, node, flags) =>
            @this.DisambiguateExpression(node, ExpressionFlags.IsInsideLeftHandSideExpression | ExpressionFlags.IsLeftMostInLeftHandSideExpression | @this.PropagateExpressionFlags(flags)));

        _writeContext.SetNodeProperty(nameof(node.Quasi), static node => node.As<TaggedTemplateExpression>().Quasi);
        VisitSubExpression(node.Quasi, SubExpressionFlags(needsParens: false, isLeftMost: false));

        return node;
    }

    protected internal override object? VisitTemplateElement(TemplateElement node)
    {
        _writeContext.SetNodeProperty(nameof(node.Value), static node => node.As<TemplateElement>().Value);
        Writer.WriteLiteral(node.Value.Raw, TokenKind.Template, ref _writeContext);

        return node;
    }

    protected internal override object? VisitTemplateLiteral(TemplateLiteral node)
    {
        Writer.WritePunctuator("`", TokenFlags.Leading, ref _writeContext);

        TemplateElement quasi;
        int i;
        for (i = 0; !(quasi = node.Quasis[i]).Tail; i++)
        {
            _writeContext.SetNodeProperty(nameof(node.Quasis), static node => ref node.As<TemplateLiteral>().Quasis);
            _writeContext.SetNodePropertyItemIndex(i);
            VisitAuxiliaryNode(quasi);

            _writeContext.SetNodeProperty(nameof(node.Expressions), static node => ref node.As<TemplateLiteral>().Expressions);
            _writeContext.SetNodePropertyItemIndex(i);
            Writer.WritePunctuator("${", TokenFlags.Leading, ref _writeContext);
            VisitRootExpression(node.Expressions[i], RootExpressionFlags(needsParens: false));
            Writer.WritePunctuator("}", TokenFlags.Trailing, ref _writeContext);
        }

        _writeContext.SetNodeProperty(nameof(node.Quasis), static node => ref node.As<TemplateLiteral>().Quasis);
        _writeContext.SetNodePropertyItemIndex(i);
        VisitAuxiliaryNode(quasi);

        Writer.WritePunctuator("`", TokenFlags.Trailing, ref _writeContext);

        return node;
    }

    protected internal override object? VisitThisExpression(ThisExpression node)
    {
        Writer.WriteKeyword("this", ref _writeContext);

        return node;
    }

    protected internal override object? VisitThrowStatement(ThrowStatement node)
    {
        Writer.WriteKeyword("throw", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Argument), static node => node.As<ThrowStatement>().Argument);
        VisitRootExpression(node.Argument, RootExpressionFlags(needsParens: false));

        StatementNeedsSemicolon();

        return node;
    }

    protected internal override object? VisitTryStatement(TryStatement node)
    {
        Writer.WriteKeyword("try", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Block), static node => node.As<TryStatement>().Block);
        StatementFlags bodyFlags;
        VisitStatement(node.Block, bodyFlags = StatementBodyFlags(isRightMost: false));

        if (node.Handler is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("catch", TokenFlags.SurroundingSpaceRecommended | StatementBodyFlagsToKeywordFlags(bodyFlags), ref _writeContext);

            _writeContext.SetNodeProperty(nameof(node.Handler), static node => node.As<TryStatement>().Handler);
            VisitAuxiliaryNode(node.Handler);
            bodyFlags = StatementBodyFlags(isRightMost: node.Finalizer is null);
        }

        if (node.Finalizer is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WriteKeyword("finally", TokenFlags.SurroundingSpaceRecommended | StatementBodyFlagsToKeywordFlags(bodyFlags), ref _writeContext);

            _writeContext.SetNodeProperty(nameof(node.Finalizer), static node => node.As<TryStatement>().Finalizer);
            VisitStatement(node.Finalizer, StatementBodyFlags(isRightMost: true));
        }

        return node;
    }

    protected internal override object? VisitUnaryExpression(UnaryExpression node)
    {
        var argumentNeedsParens = UnaryOperandNeedsParens(node, node.Argument);
        var op = (node.Type == NodeType.UpdateExpression
            ? UpdateExpression.OperatorToString(node.Operator)
            : NonUpdateUnaryExpression.OperatorToString(node.Operator))
            ?? throw new InvalidOperationException(ExtrasExceptionMessages.InvalidUnaryOperator);

        if (node.Prefix)
        {
            _writeContext.SetNodeProperty(nameof(node.Operator), static node => node.As<UnaryExpression>().Operator);
            if (op[0].IsBasicLatinLetter())
            {
                Writer.WriteKeyword(op, TokenFlags.TrailingSpaceRecommended, ref _writeContext);
            }
            else
            {
                // Cases like +(+x) or +(++x) must be disambiguated. However, this can be done in multiple ways: e.g. + +x or +(+x).
                // It depends on the formatting which way to choose, so disambiguation must be implemented by JavaScriptTextWriter.
                Writer.WritePunctuator(op, TokenFlags.Leading | TokenFlags.IsUnaryOperator, ref _writeContext);
            }

            _writeContext.SetNodeProperty(nameof(node.Argument), static node => node.As<UnaryExpression>().Argument);
            VisitSubExpression(node.Argument, SubExpressionFlags(argumentNeedsParens, isLeftMost: false));
        }
        else
        {
            _writeContext.SetNodeProperty(nameof(node.Argument), static node => node.As<UnaryExpression>().Argument);
            VisitSubExpression(node.Argument, SubExpressionFlags(argumentNeedsParens, isLeftMost: true));

            _writeContext.SetNodeProperty(nameof(node.Operator), static node => node.As<UnaryExpression>().Operator);
            Writer.WritePunctuator(op, TokenFlags.Trailing | TokenFlags.IsUnaryOperator, ref _writeContext);
        }

        return node;
    }

    protected internal override object? VisitVariableDeclaration(VariableDeclaration node)
    {
        _writeContext.SetNodeProperty(nameof(node.Kind), static node => node.As<VariableDeclaration>().Kind);
        Writer.WriteKeyword(VariableDeclaration.GetVariableDeclarationKindToken(node.Kind),
            _currentStatementFlags.HasFlagFast(StatementFlags.NestedVariableDeclaration).ToFlag(TokenFlags.TrailingSpaceRecommended, TokenFlags.SurroundingSpaceRecommended), ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Declarations), static node => ref node.As<VariableDeclaration>().Declarations);

        if (!_currentStatementFlags.HasFlagFast(StatementFlags.NestedVariableDeclaration))
        {
            VisitAuxiliaryNodeList(in node.Declarations, separator: ",");

            StatementNeedsSemicolon();
        }
        else if (ParentNode is not { Type: NodeType.ForStatement or NodeType.ForInStatement })
        {
            VisitAuxiliaryNodeList(in node.Declarations, separator: ",");
        }
        else
        {
            VisitAuxiliaryNodeList(in node.Declarations, separator: ",", static delegate { return s_forLoopInitDeclarationFlag; });
        }

        return node;
    }

    protected internal override object? VisitVariableDeclarator(VariableDeclarator node)
    {
        _writeContext.SetNodeProperty(nameof(node.Id), static node => node.As<VariableDeclarator>().Id);
        VisitAuxiliaryNode(node.Id);

        if (node.Init is not null)
        {
            _writeContext.ClearNodeProperty();
            Writer.WritePunctuator("=", TokenFlags.InBetween | TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

            _writeContext.SetNodeProperty(nameof(node.Init), static node => node.As<VariableDeclarator>().Init);

            if (_currentAuxiliaryNodeContext != s_forLoopInitDeclarationFlag)
            {
                VisitRootExpression(node.Init, RootExpressionFlags(needsParens: ExpressionNeedsParensInList(node.Init)));
            }
            else
            {
                VisitRootExpression(node.Init, ExpressionFlags.IsInAmbiguousInOperatorContext | RootExpressionFlags(needsParens: ExpressionNeedsParensInList(node.Init)));
            }
        }

        return node;
    }

    protected internal override object? VisitWhileStatement(WhileStatement node)
    {
        Writer.WriteKeyword("while", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Test), static node => node.As<WhileStatement>().Test);
        VisitRootExpression(node.Test, ExpressionFlags.SpaceAroundParensRecommended | RootExpressionFlags(needsParens: true));

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<WhileStatement>().Body);
        VisitStatement(node.Body, StatementBodyFlags(isRightMost: true));

        return node;
    }

    protected internal override object? VisitWithStatement(WithStatement node)
    {
        Writer.WriteKeyword("with", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        _writeContext.SetNodeProperty(nameof(node.Object), static node => node.As<WithStatement>().Object);
        VisitRootExpression(node.Object, ExpressionFlags.SpaceAroundParensRecommended | RootExpressionFlags(needsParens: true));

        _writeContext.SetNodeProperty(nameof(node.Body), static node => node.As<WithStatement>().Body);
        VisitStatement(node.Body, StatementBodyFlags(isRightMost: true));

        return node;
    }

    protected internal override object? VisitYieldExpression(YieldExpression node)
    {
        Writer.WriteKeyword("yield", (!node.Delegate && node.Argument is not null).ToFlag(TokenFlags.TrailingSpaceRecommended), ref _writeContext);

        if (node.Delegate)
        {
            _writeContext.SetNodeProperty(nameof(node.Delegate), static node => node.As<YieldExpression>().Delegate);
            Writer.WritePunctuator("*", (node.Argument is not null).ToFlag(TokenFlags.TrailingSpaceRecommended), ref _writeContext);
        }

        if (node.Argument is not null)
        {
            var argumentNeedsParens = UnaryOperandNeedsParens(node, node.Argument);

            _writeContext.SetNodeProperty(nameof(node.Argument), static node => node.As<YieldExpression>().Argument);
            VisitSubExpression(node.Argument, SubExpressionFlags(argumentNeedsParens, isLeftMost: false));
        }

        return node;
    }
}
