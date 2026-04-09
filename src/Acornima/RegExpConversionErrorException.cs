using System;

namespace Acornima;

[Obsolete("This type is deprecated as JS RegExp to .NET Regex conversion will be removed from the library in the next major version.")]
public sealed class RegExpConversionErrorException : ParseErrorException
{
    public RegExpConversionErrorException(RegExpConversionError error, Exception? innerException = null)
        : base(error, innerException) { }

    public new RegExpConversionError Error => (RegExpConversionError)base.Error;
}
