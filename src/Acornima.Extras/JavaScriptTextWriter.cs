using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

public record class JavaScriptTextWriterOptions
{
    public static readonly JavaScriptTextWriterOptions Default = new();

    protected internal virtual JavaScriptTextWriter CreateWriter(TextWriter writer) => new JavaScriptTextWriter(writer, this);
}

/// <summary>
/// Base JavaScript text writer (code formatter) which uses the minimal (most compact possible) format.
/// </summary>
public partial class JavaScriptTextWriter
{
    private readonly TextWriter _writer;
    private TokenSequence _lookbehind;

    public JavaScriptTextWriter(TextWriter writer, JavaScriptTextWriterOptions options)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        LastTokenKind = TokenKind.EOF;
        LastTriviaType = TriviaType.EndOfLine;
        CurrentLineIsEmptyOrWhiteSpace = true;
    }

    protected TokenKind LastTokenKind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }
    protected TokenFlags LastTokenFlags { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

    protected TriviaType LastTriviaType { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

    protected bool CurrentLineIsEmptyOrWhiteSpace { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }
    protected bool PendingRequiredNewLine { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] private set; }

    protected virtual void OnTriviaWritten(TriviaType type, TriviaFlags flags)
    {
        LastTriviaType = type;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void WriteSpace()
    {
        WriteWhiteSpace(" ");
    }

    protected void WriteWhiteSpace(string value)
    {
        _writer.Write(value);
        OnTriviaWritten(TriviaType.WhiteSpace, TriviaFlags.None);
        _lookbehind = TokenSequence.None;
    }

    protected void WriteEndOfLine()
    {
        _writer.WriteLine();
        OnTriviaWritten(TriviaType.EndOfLine, TriviaFlags.None);
        CurrentLineIsEmptyOrWhiteSpace = true;
        PendingRequiredNewLine = false;
        _lookbehind = TokenSequence.None;
    }

    protected virtual void WriteLine()
    {
        WriteEndOfLine();
    }

    protected virtual void WriteLineCommentCore(TextWriter writer, string line, TriviaFlags flags)
    {
        writer.Write("//");
        writer.Write(line);
    }

    public void WriteLineComment(string line, TriviaFlags flags)
    {
        if (PendingRequiredNewLine || !CurrentLineIsEmptyOrWhiteSpace && flags.HasFlagFast(TriviaFlags.LeadingNewLineRequired))
        {
            WriteLine();
        }

        WriteLineCommentCore(_writer, line, flags);
        OnTriviaWritten(TriviaType.LineComment, flags);
        CurrentLineIsEmptyOrWhiteSpace = false;
        PendingRequiredNewLine = true; // New line after line comments is always required.
        _lookbehind = TokenSequence.None;
    }

    protected virtual void WriteBlockCommentLine(TextWriter writer, string line, bool isFirst)
    {
        writer.Write(line);
    }

    protected virtual void WriteBlockCommentCore(TextWriter writer, IEnumerable<string> lines, TriviaFlags flags)
    {
        writer.Write("/*");
        using (var enumerator = lines.GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                WriteBlockCommentLine(writer, enumerator.Current, isFirst: true);

                while (enumerator.MoveNext())
                {
                    writer.WriteLine();
                    WriteBlockCommentLine(writer, enumerator.Current, isFirst: false);
                }
            }
        }
        writer.Write("*/");
    }

    public void WriteBlockComment(IEnumerable<string> lines, TriviaFlags flags)
    {
        if (PendingRequiredNewLine || !CurrentLineIsEmptyOrWhiteSpace && flags.HasFlagFast(TriviaFlags.LeadingNewLineRequired))
        {
            WriteLine();
        }

        WriteBlockCommentCore(_writer, lines, flags);
        OnTriviaWritten(TriviaType.BlockComment, flags);
        CurrentLineIsEmptyOrWhiteSpace = false;
        PendingRequiredNewLine = flags.HasFlagFast(TriviaFlags.TrailingNewLineRequired);
        _lookbehind = TokenSequence.None;
    }

    protected virtual void OnTokenWritten(TokenKind kind, TokenFlags flags)
    {
        LastTokenKind = kind;
        LastTokenFlags = flags;

        LastTriviaType = TriviaType.None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SpaceRecommendedAfterLastToken()
    {
        LastTokenFlags |= TokenFlags.TrailingSpaceRecommended;
    }

    protected void WriteRequiredSpaceBetweenTokenAndIdentifier()
    {
        switch (LastTokenKind)
        {
            case TokenKind.BigIntLiteral:
            case TokenKind.BooleanLiteral:
            case TokenKind.Identifier:
            case TokenKind.Keyword:
            case TokenKind.NullLiteral:
            case TokenKind.NumericLiteral:
            case TokenKind.RegExpLiteral:
                WriteSpace();
                break;
            case TokenKind.EOF:
            case TokenKind.Punctuator:
            case TokenKind.StringLiteral:
            case TokenKind.Template:
                break;
            default:
                throw new InvalidOperationException(ExtrasExceptionMessages.InvalidTokenType);
        }
    }

    protected virtual void StartIdentifier(string value, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            WriteRequiredSpaceBetweenTokenAndIdentifier();
        }
    }

    public void WriteIdentifier(string value, TokenFlags flags, ref WriteContext context)
    {
        if (PendingRequiredNewLine)
        {
            WriteLine();
        }

        StartIdentifier(value, flags, ref context);
        _writer.Write(value);
        EndIdentifier(value, flags, ref context);

        OnTokenWritten(TokenKind.Identifier, flags);
        CurrentLineIsEmptyOrWhiteSpace = false;
        _lookbehind = TokenSequence.None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteIdentifier(string value, ref WriteContext context)
    {
        WriteIdentifier(value, TokenFlags.None, ref context);
    }

    protected virtual void EndIdentifier(string value, TokenFlags flags, ref WriteContext context) { }

    protected void WriteRequiredSpaceBetweenTokenAndKeyword()
    {
        switch (LastTokenKind)
        {
            case TokenKind.BigIntLiteral:
            case TokenKind.BooleanLiteral:
            case TokenKind.Identifier:
            case TokenKind.Keyword:
            case TokenKind.NullLiteral:
            case TokenKind.NumericLiteral:
            case TokenKind.RegExpLiteral:
                WriteSpace();
                break;
            case TokenKind.EOF:
            case TokenKind.Punctuator:
            case TokenKind.StringLiteral:
            case TokenKind.Template:
                break;
            default:
                throw new InvalidOperationException(ExtrasExceptionMessages.InvalidTokenType);
        }
    }

    protected virtual void StartKeyword(string value, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            WriteRequiredSpaceBetweenTokenAndKeyword();
        }
    }

    public void WriteKeyword(string value, TokenFlags flags, ref WriteContext context)
    {
        if (PendingRequiredNewLine)
        {
            WriteLine();
        }

        StartKeyword(value, flags, ref context);
        _writer.Write(value);
        EndKeyword(value, flags, ref context);

        OnTokenWritten(TokenKind.Keyword, flags);
        CurrentLineIsEmptyOrWhiteSpace = false;
        _lookbehind = TokenSequence.None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteKeyword(string value, ref WriteContext context)
    {
        WriteKeyword(value, TokenFlags.None, ref context);
    }

    protected virtual void EndKeyword(string value, TokenFlags flags, ref WriteContext context) { }

    protected void WriteRequiredSpaceBetweenTokenAndLiteral(TokenKind kind)
    {
        switch (LastTokenKind)
        {
            case TokenKind.BigIntLiteral:
            case TokenKind.BooleanLiteral:
            case TokenKind.Identifier:
            case TokenKind.Keyword:
            case TokenKind.NullLiteral:
            case TokenKind.NumericLiteral:
            case TokenKind.RegExpLiteral:
                if (kind is not (TokenKind.StringLiteral or TokenKind.RegExpLiteral))
                {
                    WriteSpace();
                }
                break;
            case TokenKind.EOF:
            case TokenKind.StringLiteral:
            case TokenKind.Template:
                break;
            case TokenKind.Punctuator:
                if (kind == TokenKind.RegExpLiteral && _lookbehind == TokenSequence.Division)
                {
                    WriteSpace();
                }
                break;
            default:
                throw new InvalidOperationException(ExtrasExceptionMessages.InvalidTokenType);
        }
    }

    protected virtual void StartLiteral(string value, TokenKind kind, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            WriteRequiredSpaceBetweenTokenAndLiteral(kind);
        }
    }

    public void WriteLiteral(string value, TokenKind kind, TokenFlags flags, ref WriteContext context)
    {
        if (PendingRequiredNewLine)
        {
            WriteLine();
        }

        StartLiteral(value, kind, flags, ref context);
        _writer.Write(value);
        EndLiteral(value, kind, flags, ref context);

        OnTokenWritten(kind, flags);
        CurrentLineIsEmptyOrWhiteSpace = false;
        _lookbehind = TokenSequence.None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLiteral(string value, TokenKind kind, ref WriteContext context)
    {
        WriteLiteral(value, kind, TokenFlags.None, ref context);
    }

    protected virtual void EndLiteral(string value, TokenKind kind, TokenFlags flags, ref WriteContext context) { }

    protected virtual bool ShouldDisambiguatePunctuator(string value, TokenFlags flags, ref WriteContext context)
    {
        if (_lookbehind != TokenSequence.None)
        {
            switch (_lookbehind)
            {
                case TokenSequence.Addition or TokenSequence.UnaryPlus when value[0] == '+':
                case TokenSequence.Subtraction or TokenSequence.UnaryNegation when value[0] == '-':
                case TokenSequence.LessThanThenLogicalNot when value[0] == '-' && value.CharCodeAt(1) == '-':
                case TokenSequence.UnaryPostfixDecrement when value[0] == '>':
                    return true;
            }
        }

        return false;
    }

    protected void WriteRequiredSpaceBetweenTokenAndPunctuator(string value, TokenFlags flags, ref WriteContext context)
    {
        if (ShouldDisambiguatePunctuator(value, flags, ref context))
        {
            WriteSpace();
        }
    }

    protected virtual void StartPunctuator(string value, TokenFlags flags, ref WriteContext context)
    {
        if (LastTriviaType == TriviaType.None)
        {
            WriteRequiredSpaceBetweenTokenAndPunctuator(value, flags, ref context);
        }
    }

    public void WritePunctuator(string value, TokenFlags flags, ref WriteContext context)
    {
        if (PendingRequiredNewLine)
        {
            WriteLine();
        }

        StartPunctuator(value, flags, ref context);
        _writer.Write(value);
        EndPunctuator(value, flags, ref context);

        OnTokenWritten(TokenKind.Punctuator, flags);
        CurrentLineIsEmptyOrWhiteSpace = false;

        if ((flags & (TokenFlags.IsBinaryOperator | TokenFlags.IsUnaryOperator)) != 0)
        {
            if (_lookbehind == TokenSequence.LessThan && context.Node is NonUpdateUnaryExpression { Operator: Operator.LogicalNot })
            {
                _lookbehind = TokenSequence.LessThanThenLogicalNot;
            }
            else
            {
                _lookbehind = context.Node switch
                {
                    NonLogicalBinaryExpression { Operator: Operator.Addition } => TokenSequence.Addition,
                    NonLogicalBinaryExpression { Operator: Operator.Subtraction } => TokenSequence.Subtraction,
                    NonLogicalBinaryExpression { Operator: Operator.Division } => TokenSequence.Division,
                    NonLogicalBinaryExpression { Operator: Operator.LessThan } => TokenSequence.LessThan,
                    NonUpdateUnaryExpression { Operator: Operator.UnaryPlus } => TokenSequence.UnaryPlus,
                    NonUpdateUnaryExpression { Operator: Operator.UnaryNegation } => TokenSequence.UnaryNegation,
                    UpdateExpression { Operator: Operator.Decrement, Prefix: false } => TokenSequence.UnaryPostfixDecrement,
                    _ => TokenSequence.None
                };
            }
        }
        else
        {
            _lookbehind = TokenSequence.None;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WritePunctuator(string value, ref WriteContext context)
    {
        WritePunctuator(value, TokenFlags.None, ref context);
    }

    protected virtual void EndPunctuator(string value, TokenFlags flags, ref WriteContext context) { }

    public virtual void StartArray(int elementCount, ref WriteContext context)
    {
        WritePunctuator("[", TokenFlags.Leading, ref context);
    }

    public virtual void EndArray(int elementCount, ref WriteContext context)
    {
        WritePunctuator("]", TokenFlags.Trailing, ref context);
    }

    public virtual void StartObject(int propertyCount, ref WriteContext context)
    {
        WritePunctuator("{", propertyCount > 0 ? TokenFlags.Leading | TokenFlags.TrailingSpaceRecommended : TokenFlags.Leading, ref context);
    }

    public virtual void EndObject(int propertyCount, ref WriteContext context)
    {
        WritePunctuator("}", propertyCount > 0 ? TokenFlags.Trailing | TokenFlags.LeadingSpaceRecommended : TokenFlags.Trailing, ref context);
    }

    public virtual void StartBlock(int statementCount, ref WriteContext context)
    {
        WritePunctuator("{", TokenFlags.Leading | TokenFlags.SurroundingSpaceRecommended, ref context);
    }

    public virtual void EndBlock(int statementCount, ref WriteContext context)
    {
        WritePunctuator("}", TokenFlags.Trailing | TokenFlags.LeadingSpaceRecommended, ref context);
    }

    public virtual void StartStatement(StatementFlags flags, ref WriteContext context) { }

    public virtual void EndStatement(StatementFlags flags, ref WriteContext context)
    {
        // Writes statement terminator unless it can be omitted.
        if (flags.HasFlagFast(StatementFlags.NeedsSemicolon) && !flags.HasFlagFast(StatementFlags.MayOmitRightMostSemicolon | StatementFlags.IsRightMost))
        {
            WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref context);
        }
    }

    public virtual void StartStatementList(int count, ref WriteContext context) { }

    public virtual void StartStatementListItem(int index, int count, StatementFlags flags, ref WriteContext context) { }

    public virtual void EndStatementListItem(int index, int count, StatementFlags flags, ref WriteContext context)
    {
        // Writes statement terminator unless it can be omitted.
        if (flags.HasFlagFast(StatementFlags.NeedsSemicolon) && !flags.HasFlagFast(StatementFlags.MayOmitRightMostSemicolon | StatementFlags.IsRightMost))
        {
            WritePunctuator(";", TokenFlags.Trailing | TokenFlags.TrailingSpaceRecommended, ref context);
        }
    }

    public virtual void EndStatementList(int count, ref WriteContext context) { }

    public virtual void StartExpression(ExpressionFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsParens))
        {
            WritePunctuator("(", TokenFlags.Leading | flags.HasFlagFast(ExpressionFlags.SpaceAroundParensRecommended).ToFlag(TokenFlags.LeadingSpaceRecommended), ref context);
        }
    }

    public virtual void EndExpression(ExpressionFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsParens))
        {
            WritePunctuator(")", TokenFlags.Trailing | flags.HasFlagFast(ExpressionFlags.SpaceAroundParensRecommended).ToFlag(TokenFlags.TrailingSpaceRecommended), ref context);
        }
    }

    public virtual void StartExpressionList(int count, ref WriteContext context) { }

    public virtual void StartExpressionListItem(int index, int count, ExpressionFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsParens))
        {
            WritePunctuator("(", TokenFlags.Leading, ref context);
        }
    }

    public virtual void EndExpressionListItem(int index, int count, ExpressionFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(ExpressionFlags.NeedsParens))
        {
            WritePunctuator(")", TokenFlags.Trailing, ref context);
        }

        if (index < count - 1)
        {
            WritePunctuator(",", TokenFlags.InBetween | TokenFlags.TrailingSpaceRecommended, ref context);
        }
    }

    public virtual void EndExpressionList(int count, ref WriteContext context) { }

    public virtual void StartAuxiliaryNode(object? nodeContext, ref WriteContext context) { }

    public virtual void EndAuxiliaryNode(object? nodeContext, ref WriteContext context) { }

    public virtual void StartAuxiliaryNodeList<T>(int count, ref WriteContext context) where T : Node? { }

    public virtual void StartAuxiliaryNodeListItem<T>(int index, int count, string separator, object? nodeContext, ref WriteContext context) where T : Node? { }

    public virtual void EndAuxiliaryNodeListItem<T>(int index, int count, string separator, object? nodeContext, ref WriteContext context) where T : Node?
    {
        if (separator.Length > 0 && index < count - 1)
        {
            WritePunctuator(separator, TokenFlags.InBetween | TokenFlags.TrailingSpaceRecommended, ref context);
        }
    }

    public virtual void EndAuxiliaryNodeList<T>(int count, ref WriteContext context) where T : Node? { }

    public virtual void Finish()
    {
        OnTokenWritten(TokenKind.EOF, TokenFlags.None);
        _lookbehind = TokenSequence.None;
    }
}
