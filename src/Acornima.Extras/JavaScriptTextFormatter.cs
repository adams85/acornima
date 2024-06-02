using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

public abstract record class JavaScriptTextFormatterOptions : JavaScriptTextWriterOptions
{
    public string? Indent { get; init; }
    public bool KeepSingleStatementBodyInLine { get; init; }
    public bool KeepEmptyBlockBodyInLine { get; init; }
    public int MultiLineArrayLiteralThreshold { get; init; } = 7;
    public int MultiLineObjectLiteralThreshold { get; init; } = 3;

    protected abstract JavaScriptTextFormatter CreateFormatter(TextWriter writer);

    protected internal sealed override JavaScriptTextWriter CreateWriter(TextWriter writer) => CreateFormatter(writer);
}

/// <summary>
/// Base class for JavaScript code formatters.
/// </summary>
public abstract class JavaScriptTextFormatter : JavaScriptTextWriter
{
    private readonly string _indent;
    private int _indentionLevel;

    protected JavaScriptTextFormatter(TextWriter writer, JavaScriptTextFormatterOptions options) : base(writer, options)
    {
        if (!string.IsNullOrWhiteSpace(options.Indent))
        {
            throw new ArgumentException(ExtrasExceptionMessages.InvalidIndent, nameof(options));
        }

        _indent = options.Indent ?? "  ";

        KeepSingleStatementBodyInLine = options.KeepSingleStatementBodyInLine;
        KeepEmptyBlockBodyInLine = options.KeepEmptyBlockBodyInLine;
        MultiLineArrayLiteralThreshold = options.MultiLineArrayLiteralThreshold >= 0 ? options.MultiLineArrayLiteralThreshold : int.MaxValue;
        MultiLineObjectLiteralThreshold = options.MultiLineObjectLiteralThreshold >= 0 ? options.MultiLineObjectLiteralThreshold : int.MaxValue;
    }

    protected bool KeepSingleStatementBodyInLine { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    protected bool KeepEmptyBlockBodyInLine { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    protected int MultiLineArrayLiteralThreshold { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    protected int MultiLineObjectLiteralThreshold { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected void IncreaseIndent()
    {
        _indentionLevel++;
    }

    protected void DecreaseIndent()
    {
        _indentionLevel--;
    }

    protected void WriteIndent()
    {
        for (var n = _indentionLevel; n > 0; n--)
        {
            WriteWhiteSpace(_indent);
        }
    }

    protected override void WriteLine()
    {
        WriteEndOfLine();
        WriteIndent();
    }

    protected override void WriteLineCommentCore(TextWriter writer, string line, TriviaFlags flags)
    {
        if (!LastTriviaType.HasFlag(WhiteSpaceTriviaFlag))
        {
            WriteSpace();
        }

        base.WriteLineCommentCore(writer, line, flags);
    }

    protected override void WriteBlockCommentLine(TextWriter writer, string line, bool isFirst)
    {
        if (!isFirst)
        {
            for (var n = _indentionLevel; n > 0; n--)
            {
                writer.Write(_indent);
            }
        }

        base.WriteBlockCommentLine(writer, line, isFirst);
    }

    protected override void WriteBlockCommentCore(TextWriter writer, IEnumerable<string> lines, TriviaFlags flags)
    {
        if (!LastTriviaType.HasFlag(WhiteSpaceTriviaFlag))
        {
            WriteSpace();
        }

        base.WriteBlockCommentCore(writer, lines, flags);
    }

    protected virtual void WriteWhiteSpaceBetweenTokenAndIdentifier(string value, TokenFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
        {
            WriteSpace();
        }
        else
        {
            WriteRequiredSpaceBetweenTokenAndIdentifier();
        }
    }

    protected override void StartIdentifier(string value, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            WriteWhiteSpaceBetweenTokenAndIdentifier(value, flags, ref context);
        }
        else if (!LastTriviaType.HasFlag(WhiteSpaceTriviaFlag))
        {
            WriteSpace();
        }
    }

    protected virtual void WriteWhiteSpaceBetweenTokenAndKeyword(string value, TokenFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(TokenFlags.FollowsStatementBody))
        {
            WriteLine();
        }
        else if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
        {
            WriteSpace();
        }
        else
        {
            WriteRequiredSpaceBetweenTokenAndKeyword();
        }
    }

    protected override void StartKeyword(string value, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            WriteWhiteSpaceBetweenTokenAndKeyword(value, flags, ref context);
        }
        else if (!LastTriviaType.HasFlag(WhiteSpaceTriviaFlag))
        {
            WriteSpace();
        }
    }

    protected virtual void WriteWhiteSpaceBetweenTokenAndLiteral(string value, TokenKind kind, TokenFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
        {
            WriteSpace();
        }
        else
        {
            WriteRequiredSpaceBetweenTokenAndLiteral(kind);
        }
    }

    protected override void StartLiteral(string value, TokenKind kind, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            WriteWhiteSpaceBetweenTokenAndLiteral(value, kind, flags, ref context);
        }
        else if (!LastTriviaType.HasFlag(WhiteSpaceTriviaFlag))
        {
            WriteSpace();
        }
    }

    protected virtual void WriteWhiteSpaceBetweenTokenAndPunctuator(string value, TokenFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
        {
            WriteSpace();
        }
        else
        {
            WriteRequiredSpaceBetweenTokenAndPunctuator(value, flags, ref context);
        }
    }

    protected override void StartPunctuator(string value, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            WriteWhiteSpaceBetweenTokenAndPunctuator(value, flags, ref context);
        }
        else if (!LastTriviaType.HasFlag(WhiteSpaceTriviaFlag))
        {
            WriteSpace();
        }
    }

    public override void StartArray(int elementCount, ref WriteContext context)
    {
        base.StartArray(elementCount, ref context);

        if (!CanKeepArrayInLine(elementCount, ref context))
        {
            WriteEndOfLine();
            IncreaseIndent();
        }
    }

    public override void EndArray(int elementCount, ref WriteContext context)
    {
        if (!CanKeepArrayInLine(elementCount, ref context))
        {
            DecreaseIndent();
            WriteIndent();
        }

        base.EndArray(elementCount, ref context);
    }

    protected virtual bool CanKeepArrayInLine(int elementCount, ref WriteContext context)
    {
        return context.Node.Type != NodeType.ArrayExpression || elementCount < MultiLineArrayLiteralThreshold;
    }

    public override void StartObject(int propertyCount, ref WriteContext context)
    {
        base.StartObject(propertyCount, ref context);

        if (!CanKeepObjectInLine(propertyCount, ref context))
        {
            WriteEndOfLine();
            IncreaseIndent();
        }
    }

    public override void EndObject(int propertyCount, ref WriteContext context)
    {
        if (!CanKeepObjectInLine(propertyCount, ref context))
        {
            DecreaseIndent();
            WriteIndent();
        }

        base.EndObject(propertyCount, ref context);
    }

    protected virtual bool CanKeepObjectInLine(int propertyCount, ref WriteContext context)
    {
        return context.Node.Type != NodeType.ObjectExpression || propertyCount < MultiLineObjectLiteralThreshold;
    }

    public override void StartBlock(int statementCount, ref WriteContext context)
    {
        base.StartBlock(statementCount, ref context);

        if (!CanKeepBlockInLine(statementCount, ref context))
        {
            WriteEndOfLine();
            IncreaseIndent();
        }
    }

    public override void EndBlock(int statementCount, ref WriteContext context)
    {
        if (!CanKeepBlockInLine(statementCount, ref context))
        {
            DecreaseIndent();
            WriteIndent();
        }

        base.EndBlock(statementCount, ref context);
    }

    protected virtual bool CanKeepBlockInLine(int statementCount, ref WriteContext context)
    {
        return statementCount == 0 && KeepEmptyBlockBodyInLine;
    }

#pragma warning disable CA1822 // Mark members as static
    protected void StoreStatementBodyIntoContext(Statement statement, ref WriteContext context)
#pragma warning restore CA1822 // Mark members as static
    {
        context._additionalDataSlot.PrimaryData = statement;
    }

#pragma warning disable CA1822 // Mark members as static
    protected Statement RetrieveStatementBodyFromContext(ref WriteContext context)
#pragma warning restore CA1822 // Mark members as static
    {
        return (Statement)(context._additionalDataSlot.PrimaryData ?? ThrowInvalidOperationException<object>());
    }

    public override void StartStatement(StatementFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(StatementFlags.IsStatementBody))
        {
            var statement = context.GetNodePropertyValue<Statement>();
            StoreStatementBodyIntoContext(statement, ref context);

            // Is single statement body?
            if (statement.Type != NodeType.BlockStatement)
            {
                if (CanKeepSingleStatementBodyInLine(statement, flags, ref context))
                {
                    WriteSpace();
                }
                else
                {
                    WriteEndOfLine();
                    IncreaseIndent();
                    WriteIndent();
                }
            }
        }
    }

    public override void EndStatement(StatementFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(StatementFlags.IsStatementBody))
        {
            var statement = RetrieveStatementBodyFromContext(ref context);

            // Is single statement body?
            if (statement.Type != NodeType.BlockStatement)
            {
                if (!CanKeepSingleStatementBodyInLine(statement, flags, ref context))
                {
                    DecreaseIndent();
                }
            }
        }

        if (flags.HasFlagFast(StatementFlags.NeedsSemicolon) || ShouldTerminateStatementAnyway(context.GetNodePropertyValue<Statement>(), flags, ref context))
        {
            WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref context);
        }
    }

    public override void StartStatementList(int count, ref WriteContext context)
    {
        if (context.Node.Type == NodeType.SwitchCase)
        {
            if (count == 1 && context.GetNodePropertyListValue<Statement>()[0].Type == NodeType.BlockStatement)
            {
                WriteSpace();
            }
            else
            {
                WriteEndOfLine();
                IncreaseIndent();
            }
        }
    }

    public override void StartStatementListItem(int index, int count, StatementFlags flags, ref WriteContext context)
    {
        if (context.Node.Type == NodeType.SwitchCase)
        {
            if (index == 0 && count == 1 && context.GetNodePropertyListValue<Statement>()[0].Type == NodeType.BlockStatement)
            {
                return;
            }
        }

        WriteIndent();
    }

    public override void EndStatementListItem(int index, int count, StatementFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(StatementFlags.NeedsSemicolon) || ShouldTerminateStatementAnyway(context.GetNodePropertyListValue<Statement>()[index], flags, ref context))
        {
            WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref context);
        }

        WriteEndOfLine();
    }

    public override void EndStatementList(int count, ref WriteContext context)
    {
        if (context.Node.Type == NodeType.SwitchCase)
        {
            if (!(count == 1 && context.GetNodePropertyListValue<Statement>()[0].Type == NodeType.BlockStatement))
            {
                DecreaseIndent();
            }
        }
    }

    protected virtual bool CanKeepSingleStatementBodyInLine(Statement statement, StatementFlags flags, ref WriteContext context)
    {
        return statement.Type switch
        {
            // Statements
            NodeType.BreakStatement or
            NodeType.ContinueStatement or
            NodeType.DebuggerStatement or
            NodeType.EmptyStatement or
            NodeType.ExpressionStatement or
            NodeType.ReturnStatement or
            NodeType.ThrowStatement =>
                KeepSingleStatementBodyInLine,

            NodeType.BlockStatement or
            NodeType.DoWhileStatement or
            NodeType.ForInStatement or
            NodeType.ForOfStatement or
            NodeType.ForStatement or
            NodeType.LabeledStatement or
            NodeType.SwitchStatement or
            NodeType.TryStatement or
            NodeType.WhileStatement or
            NodeType.WithStatement =>
                false,

            NodeType.IfStatement =>
                context is { Node: IfStatement, NodePropertyName: nameof(IfStatement.Alternate) },

            // Declarations
            NodeType.FunctionDeclaration or
            NodeType.VariableDeclaration =>
                KeepSingleStatementBodyInLine,

            NodeType.ClassDeclaration or
            NodeType.ImportDeclaration or
            NodeType.ExportAllDeclaration or
            NodeType.ExportDefaultDeclaration or
            NodeType.ExportNamedDeclaration =>
                throw new ArgumentException(string.Format(null, ExtrasExceptionMessages.OperationNotDefinedForNodeType, statement.Type), nameof(statement)),

            // Extensions
            _ => false,
        };
    }

    protected virtual bool ShouldTerminateStatementAnyway(Statement statement, StatementFlags flags, ref WriteContext context)
    {
        return statement.Type switch
        {
            NodeType.DoWhileStatement => true,
            _ => false
        };
    }

    public override void StartExpression(ExpressionFlags flags, ref WriteContext context)
    {
        if (!flags.HasFlagFast(ExpressionFlags.NeedsParens))
        {
            var expression = !context.NodePropertyHasListValue
                ? context.GetNodePropertyValue<Expression>()
                : context.GetNodePropertyListItem<Expression>();
            var forceParens = ShouldWrapExpressionInParensAnyway(expression, flags, ref context);
            context._additionalDataSlot.PrimaryData = forceParens.AsCachedObject();
            if (forceParens)
            {
                flags |= ExpressionFlags.NeedsParens;
            }
        }

        base.StartExpression(flags, ref context);
    }

    public override void EndExpression(ExpressionFlags flags, ref WriteContext context)
    {
        if (!flags.HasFlagFast(ExpressionFlags.NeedsParens))
        {
            var forceParens = (bool)context._additionalDataSlot.PrimaryData!;
            if (forceParens)
            {
                flags |= ExpressionFlags.NeedsParens;
            }
        }

        base.EndExpression(flags, ref context);
    }

    public override void StartExpressionListItem(int index, int count, ExpressionFlags flags, ref WriteContext context)
    {
        if (context.Node.Type == NodeType.ArrayExpression && count >= MultiLineArrayLiteralThreshold)
        {
            WriteIndent();
        }

        base.StartExpressionListItem(index, count, flags, ref context);
    }

    public override void EndExpressionListItem(int index, int count, ExpressionFlags flags, ref WriteContext context)
    {
        base.EndExpressionListItem(index, count, flags, ref context);

        if (context.Node.Type == NodeType.ArrayExpression && count >= MultiLineArrayLiteralThreshold)
        {
            WriteEndOfLine();
        }
    }

    protected virtual bool ShouldWrapExpressionInParensAnyway(Expression expression, ExpressionFlags flags, ref WriteContext context)
    {
        return
            LastTokenKind == TokenKind.Punctuator &&
            LastTokenFlags.HasFlagFast(TokenFlags.IsUnaryOperator) &&
            (expression is NonUpdateUnaryExpression unaryExpression &&
                !(NonUpdateUnaryExpression.OperatorToString(unaryExpression.Operator) ?? throw new InvalidOperationException(ExtrasExceptionMessages.InvalidUnaryOperator))[0].IsBasicLatinLetter() ||
             expression is UpdateExpression { Prefix: true } updateExpression &&
                !(UpdateExpression.OperatorToString(updateExpression.Operator) ?? throw new InvalidOperationException(ExtrasExceptionMessages.InvalidUnaryOperator))[0].IsBasicLatinLetter());
    }

    public override void StartAuxiliaryNodeListItem<T>(int index, int count, string separator, object? nodeContext, ref WriteContext context)
    {
        if (typeof(T) == typeof(SwitchCase) ||
            context.Node.Type == NodeType.ClassBody ||
            context.Node.Type == NodeType.ObjectExpression && count >= MultiLineObjectLiteralThreshold)
        {
            WriteIndent();
        }
    }

    public override void EndAuxiliaryNodeListItem<T>(int index, int count, string separator, object? nodeContext, ref WriteContext context)
    {
        base.EndAuxiliaryNodeListItem<T>(index, count, separator, nodeContext, ref context);

        if (context.Node.Type == NodeType.ClassBody ||
            context.Node.Type == NodeType.ObjectExpression && count >= MultiLineObjectLiteralThreshold)
        {
            WriteEndOfLine();
        }
        else if (typeof(T) == typeof(Decorator))
        {
            WriteLine();
        }
    }

    public override void Finish()
    {
        if (LastTriviaType.HasFlagFast(CommentTriviaFlag))
        {
            WriteEndOfLine();
        }

        base.Finish();
    }
}
