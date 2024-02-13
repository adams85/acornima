using System.Runtime.CompilerServices;

namespace Acornima;

public readonly struct TokenizerContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TokenizerContext(bool strict, bool ignoreEscapeSequenceInKeyword = false, bool requireValidEscapeSequenceInTemplate = false)
    {
        Strict = strict;
        IgnoreEscapeSequenceInKeyword = ignoreEscapeSequenceInKeyword;
        RequireValidEscapeSequenceInTemplate = requireValidEscapeSequenceInTemplate;
    }

    public readonly bool Strict;
    public readonly bool IgnoreEscapeSequenceInKeyword;
    public readonly bool RequireValidEscapeSequenceInTemplate;
}
