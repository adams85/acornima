namespace Acornima;

public sealed class SyntaxError : ParseError
{
    public SyntaxError(string description, int index = -1, Position position = default, string? source = null)
        : base(description, index, position, source) { }

    public override ParseErrorException ToException()
    {
        return new SyntaxErrorException(this);
    }
}
