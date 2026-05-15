using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Acornima;

public readonly ref struct RegExpParsingContext
{
    private readonly Tokenizer.RegExpParser _parser;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal RegExpParsingContext(Tokenizer.RegExpParser parser, Range range, in SourceLocation location)
    {
        _parser = parser;
        _range = range;
        _location = location;
    }

    public string Pattern { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _parser._pattern; }

    public string Flags { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _parser._flags; }

    internal readonly Range _range;
    public Range Range { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _range; }

    internal readonly SourceLocation _location;
    public SourceLocation Location { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _location; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Validate() => _parser.Validate();

    [DoesNotReturn]
    public void ReportSyntaxError(int index, string messageFormat,
        [CallerArgumentExpression(nameof(messageFormat))] string code = Tokenizer.UnknownError)
        => _parser.ReportSyntaxError(index - _parser._patternStartIndex, messageFormat, code);

    public ParseError ReportRecoverableError(int index, string message, ParseError.Factory errorFactory,
        [CallerArgumentExpression(nameof(message))] string code = Tokenizer.UnknownError)
        => _parser.ReportRecoverableError(index - _parser._patternStartIndex, message, errorFactory, code);
}
