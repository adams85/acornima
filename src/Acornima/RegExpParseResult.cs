using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

public readonly struct RegExpParseResult
{
    private static readonly object s_boxedDefaultResult = new ValueHolder(); // placeholder for no conversion result

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static RegExpParseResult ForValid()
        => new RegExpParseResult(s_boxedDefaultResult, additionalData: null);

    public static RegExpParseResult ForSuccess(object? conversionResult = null, object? additionalData = null)
        => new RegExpParseResult(
            conversionResult switch
            {
                null => s_boxedDefaultResult,
                ParseError => ThrowArgumentOutOfRangeException(nameof(conversionResult), typeof(ParseError), null),
                _ => conversionResult,
            },
            additionalData);

    public static RegExpParseResult ForFailure(ParseError? conversionError = null, object? additionalData = null)
        => new RegExpParseResult(conversionError, additionalData);

    private readonly object? _conversionResultOrError; // s_boxedDefaultResult indicates success with no conversion result
    private readonly object? _additionalData;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RegExpParseResult(object? conversionResultOrError, object? additionalData)
    {
        _conversionResultOrError = conversionResultOrError;
        _additionalData = additionalData;
    }

    public bool Success
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _conversionResultOrError is not (null or ParseError);
    }

    public object? ConversionResult
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Success && !ReferenceEquals(_conversionResultOrError, s_boxedDefaultResult)
            ? _conversionResultOrError
            : null;
    }

    public ParseError? ConversionError
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _conversionResultOrError as ParseError;
    }

    public object? AdditionalData { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _additionalData; }

    public Regex? Regex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _conversionResultOrError as Regex;
    }
}
