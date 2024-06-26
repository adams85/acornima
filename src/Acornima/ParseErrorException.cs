using System;

namespace Acornima;

public abstract class ParseErrorException : Exception
{
    protected ParseErrorException(ParseError error, Exception? innerException = null)
        : base((error ?? throw new ArgumentNullException(nameof(error))).ToString(), innerException)
    {
        Error = error;
    }

    public ParseError Error { get; }

    public string Description => Error.Description;

    /// <summary>
    /// One-based line number. (Can be zero if location information is not available.)
    /// </summary>
    public int LineNumber => Error.LineNumber;

    /// <summary>
    /// Zero-based column index.
    /// </summary>
    public int Column => Error.Column;

    public string? SourceFile => Error.SourceFile;
}
