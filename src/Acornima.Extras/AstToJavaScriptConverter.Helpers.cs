using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static JavaScriptTextWriter;

public partial class AstToJavaScriptConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static StatementFlags StatementBodyFlags(bool isRightMost)
    {
        return StatementFlags.IsStatementBody | isRightMost.ToFlag(StatementFlags.IsRightMost);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static TokenFlags StatementBodyFlagsToKeywordFlags(StatementFlags previousBodyFlags)
    {
        // Maps IsStatementBody to keyword flags.
        return (TokenFlags)(previousBodyFlags & StatementFlags.IsStatementBody);
    }

    protected StatementFlags PropagateStatementFlags(StatementFlags flags)
    {
        // Caller must not set NeedsSemicolon or MayOmitRightMostSemicolon.
        // NeedsSemicolon is set by the visitation handler of statement via the StatementNeedsSemicolon method,
        // MayOmitRightMostSemicolon is set by VisitStatementList.
        Debug.Assert((flags & (StatementFlags.NeedsSemicolon | StatementFlags.MayOmitRightMostSemicolon)) == 0);

        // Combines IsRightMost of parent and current statement to determine its effective value for the current statement list.
        flags &= ~StatementFlags.IsRightMost | _currentStatementFlags & StatementFlags.IsRightMost;

        // Propagates MayOmitRightMostSemicolon to current statement.
        flags |= _currentStatementFlags & StatementFlags.MayOmitRightMostSemicolon;

        return flags;
    }

    private protected static readonly Func<AstToJavaScriptConverter, Statement, StatementFlags, StatementFlags> s_getCombinedStatementFlags = static (@this, node, flags) =>
        @this.PropagateStatementFlags(flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitStatement(Statement node, StatementFlags flags)
    {
        VisitStatement(node, flags, s_getCombinedStatementFlags);
    }

    protected void VisitStatement(Statement node, StatementFlags flags, Func<AstToJavaScriptConverter, Statement, StatementFlags, StatementFlags> getCombinedFlags)
    {
        var originalStatementFlags = _currentStatementFlags;
        _currentStatementFlags = getCombinedFlags(this, node, flags);

        Writer.StartStatement((JavaScriptTextWriter.StatementFlags)_currentStatementFlags, ref _writeContext);
        Visit(node);
        Writer.EndStatement((JavaScriptTextWriter.StatementFlags)_currentStatementFlags, ref _writeContext);

        _currentStatementFlags = originalStatementFlags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitStatementList(in NodeList<Statement> nodeList)
    {
        VisitStatementList(in nodeList, static (_, _, index, count) =>
            (index == count - 1).ToFlag(StatementFlags.IsRightMost | StatementFlags.MayOmitRightMostSemicolon));
    }

    protected void VisitStatementList(in NodeList<Statement> nodeList, Func<AstToJavaScriptConverter, Statement, int, int, StatementFlags> getCombinedItemFlags)
    {
        Writer.StartStatementList(nodeList.Count, ref _writeContext);

        for (var i = 0; i < nodeList.Count; i++)
        {
            VisitStatementListItem(nodeList[i], i, nodeList.Count, getCombinedItemFlags);
        }

        Writer.EndStatementList(nodeList.Count, ref _writeContext);
    }

    protected void VisitStatementListItem(Statement node, int index, int count, Func<AstToJavaScriptConverter, Statement, int, int, StatementFlags> getCombinedFlags)
    {
        var originalStatementFlags = _currentStatementFlags;
        _currentStatementFlags = getCombinedFlags(this, node, index, count);

        _writeContext.SetNodePropertyItemIndex(index);
        Writer.StartStatementListItem(index, count, (JavaScriptTextWriter.StatementFlags)_currentStatementFlags, ref _writeContext);
        Visit(node);
        Writer.EndStatementListItem(index, count, (JavaScriptTextWriter.StatementFlags)_currentStatementFlags, ref _writeContext);

        _currentStatementFlags = originalStatementFlags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void StatementNeedsSemicolon() => _currentStatementFlags |= StatementFlags.NeedsSemicolon;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ExpressionFlags RootExpressionFlags(bool needsParens)
    {
        return ExpressionFlags.IsRootExpression | ExpressionFlags.IsLeftMost | needsParens.ToFlag(ExpressionFlags.NeedsParens);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ExpressionFlags SubExpressionFlags(bool needsParens, bool isLeftMost)
    {
        return needsParens.ToFlag(ExpressionFlags.NeedsParens) | isLeftMost.ToFlag(ExpressionFlags.IsLeftMost);
    }

    protected ExpressionFlags PropagateExpressionFlags(ExpressionFlags flags)
    {
        const ExpressionFlags isLeftMostFlags =
            ExpressionFlags.IsLeftMost |
            ExpressionFlags.IsLeftMostInArrowFunctionBody |
            ExpressionFlags.IsLeftMostInNewCallee |
            ExpressionFlags.IsLeftMostInLeftHandSideExpression;

        // Combines IsLeftMost* flags of parent and current statement to determine their effective values for the current expression tree.
        if (_currentExpressionFlags.HasFlagFast(ExpressionFlags.NeedsParens) || !flags.HasFlagFast(ExpressionFlags.IsLeftMost))
        {
            flags &= ~isLeftMostFlags;
        }
        else
        {
            flags = flags & ~isLeftMostFlags | _currentExpressionFlags & isLeftMostFlags;
        }

        // Propagates IsInAmbiguousInOperatorContext and IsInside* flags to current expression.
        flags |= _currentExpressionFlags & ExpressionFlags.IsInPotentiallyAmbiguousContext;

        return flags;
    }

    protected ExpressionFlags DisambiguateExpression(Expression node, ExpressionFlags flags)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsParens))
        {
            return flags & ~ExpressionFlags.IsInAmbiguousInOperatorContext;
        }

        // Puts the left-most expression in parentheses if necessary (in cases where it would be interpreted differently without parentheses).
        if ((flags & ExpressionFlags.IsInPotentiallyAmbiguousContext) != 0)
        {
            var isAmbiguousExpression = flags.HasFlag(ExpressionFlags.IsLeftMost) &&
                (flags.HasFlagFast(ExpressionFlags.IsInsideStatementExpression) && ExpressionIsAmbiguousAsStatementExpression(node) ||
                 flags.HasFlagFast(ExpressionFlags.IsInsideExportDefaultExpression) && ExpressionIsAmbiguousAsExportDefaultExpression(node) ||
                 flags.HasFlagFast(ExpressionFlags.IsInsideDecorator) && DecoratorLeftMostExpressionIsParenthesized(node, isRoot: flags.HasFlagFast(ExpressionFlags.IsRootExpression)));

            isAmbiguousExpression = isAmbiguousExpression ||
                flags.HasFlagFast(ExpressionFlags.IsInsideArrowFunctionBody | ExpressionFlags.IsLeftMostInArrowFunctionBody) && ExpressionIsAmbiguousAsArrowFunctionBody(node) ||
                flags.HasFlagFast(ExpressionFlags.IsInsideNewCallee | ExpressionFlags.IsLeftMostInNewCallee) && ExpressionIsAmbiguousAsNewCallee(node) ||
                flags.HasFlagFast(ExpressionFlags.IsInsideLeftHandSideExpression | ExpressionFlags.IsLeftMostInLeftHandSideExpression) && LeftHandSideExpressionIsParenthesized(node);

            // Edge case: for (var a = b = (c in d in e) in x);
            isAmbiguousExpression = isAmbiguousExpression ||
                flags.HasFlagFast(ExpressionFlags.IsInAmbiguousInOperatorContext) && node is NonLogicalBinaryExpression { Operator: Operator.In };

            if (isAmbiguousExpression)
            {
                return (flags | ExpressionFlags.NeedsParens) & ~ExpressionFlags.IsInAmbiguousInOperatorContext;
            }
        }

        return flags;
    }

    private protected static readonly Func<AstToJavaScriptConverter, Expression, ExpressionFlags, ExpressionFlags> s_getCombinedRootExpressionFlags = static (@this, node, flags) =>
        @this.DisambiguateExpression(node, flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitRootExpression(Expression node, ExpressionFlags flags)
    {
        VisitExpression(node, flags, s_getCombinedRootExpressionFlags);
    }

    private protected static readonly Func<AstToJavaScriptConverter, Expression, ExpressionFlags, ExpressionFlags> s_getCombinedSubExpressionFlags = static (@this, node, flags) =>
        @this.DisambiguateExpression(node, @this.PropagateExpressionFlags(flags));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitSubExpression(Expression node, ExpressionFlags flags)
    {
        VisitExpression(node, flags, s_getCombinedSubExpressionFlags);
    }

    protected void VisitExpression(Expression node, ExpressionFlags flags, Func<AstToJavaScriptConverter, Expression, ExpressionFlags, ExpressionFlags> getCombinedFlags)
    {
        var originalExpressionFlags = _currentExpressionFlags;
        _currentExpressionFlags = getCombinedFlags(this, node, flags);

        Writer.StartExpression((JavaScriptTextWriter.ExpressionFlags)_currentExpressionFlags, ref _writeContext);
        Visit(node);
        Writer.EndExpression((JavaScriptTextWriter.ExpressionFlags)_currentExpressionFlags, ref _writeContext);

        _currentExpressionFlags = originalExpressionFlags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitSubExpressionList(in NodeList<Expression> nodeList)
    {
        VisitExpressionList(in nodeList, static (@this, node, index, _) =>
            s_getCombinedSubExpressionFlags(@this, node, SubExpressionFlags(@this.ExpressionNeedsParensInList(node), isLeftMost: false)));
    }

    protected void VisitExpressionList(in NodeList<Expression> nodeList, Func<AstToJavaScriptConverter, Expression, int, int, ExpressionFlags> getCombinedItemFlags)
    {
        Writer.StartExpressionList(nodeList.Count, ref _writeContext);

        for (var i = 0; i < nodeList.Count; i++)
        {
            VisitExpressionListItem(nodeList[i], i, nodeList.Count, getCombinedItemFlags);
        }

        Writer.EndExpressionList(nodeList.Count, ref _writeContext);
    }

    protected void VisitExpressionListItem(Expression node, int index, int count, Func<AstToJavaScriptConverter, Expression, int, int, ExpressionFlags> getCombinedFlags)
    {
        var originalExpressionFlags = _currentExpressionFlags;
        _currentExpressionFlags = getCombinedFlags(this, node, index, count);

        _writeContext.SetNodePropertyItemIndex(index);
        Writer.StartExpressionListItem(index, count, (JavaScriptTextWriter.ExpressionFlags)_currentExpressionFlags, ref _writeContext);
        Visit(node);
        Writer.EndExpressionListItem(index, count, (JavaScriptTextWriter.ExpressionFlags)_currentExpressionFlags, ref _writeContext);

        _currentExpressionFlags = originalExpressionFlags;
    }

    private void VisitImportAttributes(in NodeList<ImportAttribute> nodeList)
    {
        // https://github.com/tc39/proposal-import-attributes#import-statements

        Writer.WriteKeyword("with", TokenFlags.SurroundingSpaceRecommended, ref _writeContext);

        Writer.StartObject(nodeList.Count, ref _writeContext);

        VisitAuxiliaryNodeList(in nodeList, separator: ",");

        Writer.EndObject(nodeList.Count, ref _writeContext);
    }

    private void VisitExportOrImportSpecifierIdentifier(Expression node)
    {
        if (node is Identifier { Name: "default" })
        {
            Writer.WriteKeyword("default", ref _writeContext);
        }
        else
        {
            VisitRootExpression(node, RootExpressionFlags(needsParens: false));
        }
    }

    private void VisitPropertyKey(Expression node, bool computed, TokenFlags leadingParenFlags = TokenFlags.None, TokenFlags trailingParenFlags = TokenFlags.None)
    {
        if (computed)
        {
            Writer.WritePunctuator("[", TokenFlags.Leading | leadingParenFlags, ref _writeContext);
            VisitRootExpression(node, RootExpressionFlags(needsParens: ExpressionNeedsParensInList(node)));
            Writer.WritePunctuator("]", TokenFlags.Trailing | trailingParenFlags, ref _writeContext);
        }
        else if (node.Type == NodeType.Identifier)
        {
            VisitAuxiliaryNode(node);
        }
        else
        {
            VisitRootExpression(node, RootExpressionFlags(needsParens: false));
        }
    }

    protected virtual bool ExpressionIsAmbiguousAsStatementExpression(Expression node)
    {
        switch (node.Type)
        {
            case NodeType.ClassExpression:
            case NodeType.FunctionExpression:
            case NodeType.ObjectExpression:
            case NodeType.AssignmentExpression when node.As<AssignmentExpression>() is { Left.Type: NodeType.ObjectPattern }:
            case NodeType.Identifier when (Parser.IsReservedWordES6Strict(node.As<Identifier>().Name.AsSpan()) & Parser.ReservedWordKind.StrictBind) == Parser.ReservedWordKind.Strict:
                return true;
        }

        return false;
    }

    protected virtual bool ExpressionIsAmbiguousAsExportDefaultExpression(Expression node)
    {
        switch (node.Type)
        {
            case NodeType.ClassExpression:
            case NodeType.FunctionExpression:
            case NodeType.Identifier when (Parser.IsReservedWordES6Strict(node.As<Identifier>().Name.AsSpan()) & Parser.ReservedWordKind.StrictBind) == Parser.ReservedWordKind.Strict:
            case NodeType.SequenceExpression:
                return true;
        }

        return false;
    }

    protected virtual bool ExpressionIsAmbiguousAsArrowFunctionBody(Expression node)
    {
        switch (node.Type)
        {
            case NodeType.ObjectExpression:
            case NodeType.AssignmentExpression when node.As<AssignmentExpression>() is { Left.Type: NodeType.ObjectPattern }:
                return true;
        }

        return false;
    }

    protected virtual bool ExpressionIsAmbiguousAsNewCallee(Expression node)
    {
        switch (node.Type)
        {
            case NodeType.CallExpression:
                return true;
        }

        return false;
    }

    protected virtual bool LeftHandSideExpressionIsParenthesized(Expression node)
    {
        // https://tc39.es/ecma262/#sec-left-hand-side-expressions

        switch (node.Type)
        {
            case NodeType.ArrowFunctionExpression:
            case NodeType.AssignmentExpression:
            case NodeType.AwaitExpression:
            case NodeType.BinaryExpression:
            case NodeType.ChainExpression:
            case NodeType.ConditionalExpression:
            case NodeType.LogicalExpression:
            case NodeType.SequenceExpression:
            case NodeType.UnaryExpression:
            case NodeType.UpdateExpression:
            case NodeType.YieldExpression:
            case NodeType.ClassExpression when node.As<ClassExpression>().Decorators.Count > 0:
            case NodeType.NewExpression when node.As<NewExpression>().Arguments.Count == 0:
                return true;
        }

        return false;
    }

    protected virtual bool DecoratorLeftMostExpressionIsParenthesized(Expression node, bool isRoot)
    {
        // https://tc39.es/proposal-decorators/

        switch (node.Type)
        {
            case NodeType.Identifier:
            case NodeType.MemberExpression when node.As<MemberExpression>() is { Computed: false }:
                return false;
            case NodeType.CallExpression:
                return !isRoot;
        }

        return true;
    }

    protected virtual bool ExpressionNeedsParensInList(Expression node)
    {
        return node.Type is
            NodeType.SequenceExpression;
    }

    protected virtual int GetOperatorPrecedence(Expression expression, out int associativity)
    {
        var result = MapToOperatorPrecedence(expression, out associativity);
        if (result >= 0)
        {
            return result;
        }
        else if (_ignoreExtensions)
        {
            return int.MinValue;
        }
        else
        {
            throw new NotImplementedException(string.Format(null, ExtrasExceptionMessages.OperatorPrecedenceNotDefined, expression.GetType()));
        }
    }

    /// <summary>
    /// Maps expression to an integer operator precedence value.
    /// </summary>
    /// <param name="expression">The expression representing the operation.</param>
    /// <param name="associativity">
    /// If less than zero, the operation has left-to-right associativity.<br/>
    /// If zero, associativity is not defined for the operation.<br/>
    /// If greater than zero, the operation has right-to-left associativity.
    /// </param>
    /// <returns>
    /// Precedence value as defined based on <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Operator_Precedence#table">this table</see>. Higher value means higher precedence.
    /// Negative value is returned if the precedence is not defined for the specified expression. <see cref="int.MaxValue"/> is returned for primitive expressions like <see cref="Identifier"/>.
    /// </returns>
    private static int MapToOperatorPrecedence(Expression expression, out int associativity)
    {
        const int leftToRightAssociativity = -1;
        const int undefinedAssociativity = 0;
        const int rightToLeftAssociativity = 1;

        associativity = undefinedAssociativity;

        switch (expression.Type)
        {
            case NodeType.ArrayExpression:
            case NodeType.ClassExpression:
            case NodeType.FunctionExpression:
            case NodeType.Identifier:
            case NodeType.Literal:
            case NodeType.ObjectExpression:
            case NodeType.PrivateIdentifier:
            case NodeType.Super:
            case NodeType.TaggedTemplateExpression:
            case NodeType.TemplateLiteral:
            case NodeType.ThisExpression:
                return int.MaxValue;

            case NodeType.ParenthesizedExpression:
                return 1800;

            case NodeType.MemberExpression when !expression.As<MemberExpression>().Computed:
            case NodeType.MetaProperty:
                associativity = leftToRightAssociativity;
                goto case NodeType.MemberExpression;
            case NodeType.MemberExpression:
            case NodeType.CallExpression:
            case NodeType.ImportExpression:
            case NodeType.NewExpression when expression.As<NewExpression>().Arguments.Count > 0:
                return 1700;

            case NodeType.NewExpression:
                return 1600;

            case NodeType.ChainExpression:
                return 1550;

            case NodeType.UpdateExpression when !expression.As<UpdateExpression>().Prefix:
                return 1500;

            case NodeType.UpdateExpression:
            case NodeType.UnaryExpression:
            case NodeType.AwaitExpression:
                return 1400;

            case NodeType.BinaryExpression:
                switch (expression.As<BinaryExpression>().Operator)
                {
                    case Operator.Exponentiation:
                        associativity = rightToLeftAssociativity;
                        return 1300;

                    case Operator.Multiplication:
                    case Operator.Division:
                    case Operator.Remainder:
                        associativity = leftToRightAssociativity;
                        return 1200;

                    case Operator.Addition:
                    case Operator.Subtraction:
                        associativity = leftToRightAssociativity;
                        return 1100;

                    case Operator.LeftShift:
                    case Operator.RightShift:
                    case Operator.UnsignedRightShift:
                        associativity = leftToRightAssociativity;
                        return 1000;

                    case Operator.LessThan:
                    case Operator.LessThanOrEqual:
                    case Operator.GreaterThan:
                    case Operator.GreaterThanOrEqual:
                    case Operator.In:
                    case Operator.InstanceOf:
                        associativity = leftToRightAssociativity;
                        return 900;

                    case Operator.Equality:
                    case Operator.Inequality:
                    case Operator.StrictEquality:
                    case Operator.StrictInequality:
                        associativity = leftToRightAssociativity;
                        return 800;

                    case Operator.BitwiseAnd:
                        associativity = leftToRightAssociativity;
                        return 700;

                    case Operator.BitwiseXor:
                        associativity = leftToRightAssociativity;
                        return 600;

                    case Operator.BitwiseOr:
                        associativity = leftToRightAssociativity;
                        return 500;
                }
                break;

            case NodeType.LogicalExpression:
                switch (expression.As<LogicalExpression>().Operator)
                {
                    case Operator.LogicalAnd:
                        associativity = leftToRightAssociativity;
                        return 400;
                    case Operator.LogicalOr:
                    case Operator.NullishCoalescing:
                        associativity = leftToRightAssociativity;
                        return 300;
                }
                break;

            case NodeType.AssignmentExpression:
            case NodeType.ConditionalExpression:
                associativity = rightToLeftAssociativity;
                goto case NodeType.ArrowFunctionExpression;
            case NodeType.ArrowFunctionExpression:
            case NodeType.YieldExpression:
            case NodeType.SpreadElement:
                return 200;

            case NodeType.SequenceExpression:
                associativity = leftToRightAssociativity;
                return 100;
        }

        return -1;
    }

    protected bool UnaryOperandNeedsParens(Expression operation, Expression operand) =>
         GetOperatorPrecedence(operation, out _) > GetOperatorPrecedence(operand, out _);

    protected BinaryOperationFlags BinaryOperandsNeedParens(Expression operation, Expression leftOperand, Expression rightOperand)
    {
        var operationPrecedence = GetOperatorPrecedence(operation, out var associativity);
        var leftOperandPrecedence = GetOperatorPrecedence(leftOperand, out _);
        var rightOperandPrecedence = GetOperatorPrecedence(rightOperand, out _);

        var result = BinaryOperationFlags.None;

        if (operationPrecedence > leftOperandPrecedence || operationPrecedence == leftOperandPrecedence && associativity > 0) // right-to-left associativity
        {
            result |= BinaryOperationFlags.LeftOperandNeedsParens;
        }

        if (operationPrecedence > rightOperandPrecedence || operationPrecedence == rightOperandPrecedence && associativity < 0) // left-to-right associativity
        {
            result |= BinaryOperationFlags.RightOperandNeedsParens;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitAuxiliaryNode(Node node)
    {
        VisitAuxiliaryNode(node, static delegate { return null; });
    }

    protected void VisitAuxiliaryNode(Node node, Func<AstToJavaScriptConverter, Node, object?> getNodeContext)
    {
        var originalAuxiliaryNodeContext = _currentAuxiliaryNodeContext;
        _currentAuxiliaryNodeContext = getNodeContext(this, node);

        Writer.StartAuxiliaryNode(_currentAuxiliaryNodeContext, ref _writeContext);
        Visit(node);
        Writer.EndAuxiliaryNode(_currentAuxiliaryNodeContext, ref _writeContext);

        _currentAuxiliaryNodeContext = originalAuxiliaryNodeContext;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void VisitAuxiliaryNodeList<TNode>(in NodeList<TNode> nodeList, string separator)
        where TNode : Node
    {
        VisitAuxiliaryNodeList(in nodeList, separator, static delegate { return null; });
    }

    protected void VisitAuxiliaryNodeList<TNode>(in NodeList<TNode> nodeList, string separator, Func<AstToJavaScriptConverter, Node?, int, int, object?> getNodeContext)
        where TNode : Node
    {
        Writer.StartAuxiliaryNodeList<TNode>(nodeList.Count, ref _writeContext);

        for (var i = 0; i < nodeList.Count; i++)
        {
            VisitAuxiliaryNodeListItem(nodeList[i], i, nodeList.Count, separator, getNodeContext);
        }

        Writer.EndAuxiliaryNodeList<TNode>(nodeList.Count, ref _writeContext);
    }

    protected void VisitAuxiliaryNodeListItem<TNode>(TNode node, int index, int count, string separator, Func<AstToJavaScriptConverter, Node?, int, int, object?> getNodeContext)
        where TNode : Node
    {
        var originalAuxiliaryNodeContext = _currentAuxiliaryNodeContext;
        _currentAuxiliaryNodeContext = getNodeContext(this, node, index, count);

        _writeContext.SetNodePropertyItemIndex(index);
        Writer.StartAuxiliaryNodeListItem<TNode>(index, count, separator, _currentAuxiliaryNodeContext, ref _writeContext);
        Visit(node);
        Writer.EndAuxiliaryNodeListItem<TNode>(index, count, separator, _currentAuxiliaryNodeContext, ref _writeContext);

        _currentAuxiliaryNodeContext = originalAuxiliaryNodeContext;
    }
}
