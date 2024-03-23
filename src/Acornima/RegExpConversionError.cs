namespace Acornima;

public sealed class RegExpConversionError : ParseError
{
    internal static readonly Factory s_factory = (description, index, position, sourceFile) => new RegExpConversionError(description, index, position, sourceFile);

    public RegExpConversionError(string description, int index = -1, Position position = default, string? source = null)
        : base(description, index, position, source) { }

    public override ParseErrorException ToException()
    {
        return new RegExpConversionErrorException(this);
    }
}
