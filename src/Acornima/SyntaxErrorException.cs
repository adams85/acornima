using System;

namespace Acornima;

public sealed class SyntaxErrorException : ParseErrorException
{
    public SyntaxErrorException(SyntaxError error, Exception? innerException = null)
        : base(error, innerException) { }

    public new SyntaxError Error => (SyntaxError)base.Error;
}
