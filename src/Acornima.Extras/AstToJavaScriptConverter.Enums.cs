using System;

namespace Acornima;

public partial class AstToJavaScriptConverter
{
    [Flags]
    protected internal enum BinaryOperationFlags
    {
        None = 0,
        LeftOperandNeedsParens = 1 << 0,
        RightOperandNeedsParens = 1 << 1,
        BothOperandsNeedParens = LeftOperandNeedsParens | RightOperandNeedsParens
    }

    [Flags]
    protected internal enum StatementFlags
    {
        None = 0,

        NeedsSemicolon = JavaScriptTextWriter.StatementFlags.NeedsSemicolon,
        MayOmitRightMostSemicolon = JavaScriptTextWriter.StatementFlags.MayOmitRightMostSemicolon,
        IsRightMost = JavaScriptTextWriter.StatementFlags.IsRightMost,
        IsStatementBody = JavaScriptTextWriter.StatementFlags.IsStatementBody,

        NestedVariableDeclaration = 1 << 16,
    }

    [Flags]
    protected internal enum ExpressionFlags
    {
        None = 0,

        NeedsParens = JavaScriptTextWriter.ExpressionFlags.NeedsParens,
        IsLeftMost = JavaScriptTextWriter.ExpressionFlags.IsLeftMost,

        SpaceBeforeParensRecommended = JavaScriptTextWriter.ExpressionFlags.SpaceBeforeParensRecommended,
        SpaceAfterParensRecommended = JavaScriptTextWriter.ExpressionFlags.SpaceAfterParensRecommended,
        SpaceAroundParensRecommended = JavaScriptTextWriter.ExpressionFlags.SpaceAroundParensRecommended,

        IsRootExpression = 1 << 16,

        IsMethod = 1 << 17,

        IsInsideExportDefaultExpression = 1 << 21, // automatically propagated to sub-expressions

        IsInsideDecorator = 1 << 22, // automatically propagated to sub-expressions

        IsInAmbiguousInOperatorContext = 1 << 24, // automatically propagated to sub-expressions

        IsLeftMostInArrowFunctionBody = 1 << 25,  // automatically combined and propagated to sub-expressions
        IsInsideArrowFunctionBody = 1 << 26, // automatically propagated to sub-expressions

        // https://stackoverflow.com/a/17587899/8656352
        IsLeftMostInNewCallee = 1 << 27,  // automatically combined and propagated to sub-expressions
        IsInsideNewCallee = 1 << 28, // automatically propagated to sub-expressions

        IsLeftMostInLeftHandSideExpression = 1 << 29, // automatically combined and propagated to sub-expressions
        IsInsideLeftHandSideExpression = 1 << 30, // automatically propagated to sub-expressions

        IsInsideStatementExpression = 1 << 31, // automatically propagated to sub-expressions

        IsInPotentiallyAmbiguousContext = IsInAmbiguousInOperatorContext | IsInsideArrowFunctionBody | IsInsideDecorator | IsInsideNewCallee | IsInsideLeftHandSideExpression | IsInsideStatementExpression | IsInsideExportDefaultExpression,
    }
}
