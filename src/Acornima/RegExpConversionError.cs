using System;

namespace Acornima;

[Obsolete("This type is deprecated as JS RegExp to .NET Regex conversion will be removed from the library in the next major version.")]
public sealed class RegExpConversionError : ParseError
{
    internal static readonly Factory s_factory = (code, description, index, position, sourceFile) => new RegExpConversionError(code, description, index, position, sourceFile);

    public RegExpConversionError(string code, string description, int index = -1, Position position = default, string? source = null)
        : base(code, description, index, position, source) { }

    public override ParseErrorException ToException()
    {
        return new RegExpConversionErrorException(this);
    }
}
