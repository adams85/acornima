using System.Text.RegularExpressions;
using Acornima.Helpers;

namespace Acornima;

public readonly struct RegExpParseResult
{
    private readonly object? _regexOrConversionError;
    private readonly ArrayList<Tokenizer.RegExpCapturingGroup> _capturingGroups;

    internal RegExpParseResult(Regex regex, ArrayList<Tokenizer.RegExpCapturingGroup> capturingGroups)
    {
        _regexOrConversionError = regex;
        _capturingGroups = capturingGroups;
    }

    internal RegExpParseResult(ParseError? conversionError)
    {
        // NOTE: We can't use null to represent success for validation-only parsing (RegExpParseMode.Validation)
        // because in that case default(RegExpParseResult) would indicate success.
        // However, we can do that by using an instance of whatever type except for Regex and ParseError.
        _regexOrConversionError = conversionError ?? (object)nameof(Success);
    }

    public bool Success => _regexOrConversionError is not (null or ParseError);

    public ParseError? ConversionError => _regexOrConversionError as ParseError;

    public Regex? Regex => _regexOrConversionError as Regex;

    public int ActualRegexGroupCount => _capturingGroups.Count + 1;

    public string? GetRegexGroupName(int number)
    {
        return (uint)--number < (uint)_capturingGroups.Count
            ? _capturingGroups[number].Name
            : null;
    }
}
