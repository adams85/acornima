using System.Diagnostics.CodeAnalysis;

namespace Acornima;

public class ParseErrorHandler
{
    public static readonly ParseErrorHandler Default = new();

    protected internal virtual void Reset() { }

    protected virtual void RecordError(ParseError error) { }

    [DoesNotReturn]
#pragma warning disable CA1822 // Mark members as static
    internal void ThrowError(ParseError error)
#pragma warning restore CA1822 // Mark members as static
    {
        throw error.ToException();
    }

    internal ParseError TolerateError(ParseError error, bool tolerant)
    {
        if (!tolerant)
        {
            ThrowError(error);
        }

        RecordError(error);
        return error;
    }
}
