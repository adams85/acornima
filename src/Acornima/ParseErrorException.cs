using System;

namespace Acornima;

public class ParseErrorException : Exception
{
    public ParseErrorException(ParseError error, Exception? innerException = null)
    : base((error ?? throw new ArgumentNullException(nameof(error))).ToString(), innerException)
    {
        Error = error;
    }

    public ParseError Error { get; }

    public string Description => Error.Description;

    /// <summary>
    /// Zero-based index within the parsed code string. (Can be negative if location information is available.)
    /// </summary>
    public int Index => Error.Index;

    /// <summary>
    /// One-based line number. (Can be zero if location information is not available.)
    /// </summary>
    public int LineNumber => Error.LineNumber;

    /// <summary>
    /// One-based column index.
    /// </summary>
    public int Column => Error.Column;

    public string? SourceFile => Error.SourceFile;
}
