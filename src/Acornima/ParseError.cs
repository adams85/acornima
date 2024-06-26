using System;

namespace Acornima;

public abstract class ParseError
{
    internal delegate ParseError Factory(string code, string description, int index, Position position, string? sourceFile);

    public string Code { get; }

    public string Description { get; }

    public bool IsIndexDefined => Index >= 0;

    /// <summary>
    /// Zero-based index within the parsed code string. (Can be negative if location information is available.)
    /// </summary>
    public int Index { get; }

    public bool IsPositionDefined => Position.Line > 0;

    public Position Position { get; }

    /// <summary>
    /// One-based line number. (Can be zero if location information is not available.)
    /// </summary>
    public int LineNumber => Position.Line;

    /// <summary>
    /// Zero-based column index.
    /// </summary>
    public int Column => Position.Column;

    public string? SourceFile { get; }

    public ParseError(string code, string description, int index = -1, Position position = default, string? sourceFile = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Index = index;
        Position = position;
        SourceFile = sourceFile;
    }

    public override string ToString()
    {
        return SourceFile is null
            ? (IsPositionDefined ? $"{Description} ({LineNumber}:{Column + 1})" : Description)
            : (IsPositionDefined ? $"{Description} ({SourceFile}:{LineNumber}:{Column + 1})" : $"{Description} ({SourceFile})");
    }

    public abstract ParseErrorException ToException();
}
