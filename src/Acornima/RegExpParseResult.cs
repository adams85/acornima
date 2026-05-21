using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

#pragma warning disable CS0618 // Type or member is obsolete

public readonly struct RegExpParseResult
{
    private static readonly object s_boxedDefaultResult = new ValueHolder(); // placeholder for no conversion result

    public static RegExpParseResult ForSuccess(object? conversionResult = null, object? additionalData = null)
        => new RegExpParseResult(
            conversionResult switch
            {
                null => s_boxedDefaultResult,
                ParseError => ThrowArgumentOutOfRangeException(nameof(conversionResult), typeof(ParseError), null),
                _ => conversionResult
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal RegExpParseResult(Regex regex, Tokenizer.RegExpCapturingGroup[] capturingGroups)
        : this((object)regex, capturingGroups) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal RegExpParseResult(RegExpConversionError? conversionError)
        : this(conversionError ?? s_boxedDefaultResult, additionalData: null) { }

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

    [Obsolete("This property is deprecated as JS RegExp to .NET Regex conversion will be removed from the library in the next major version.")]
    public int ActualRegexGroupCount => _additionalData is Tokenizer.RegExpCapturingGroup[] capturingGroups
        ? capturingGroups.Length + 1
        : ThrowInvalidOperationException<int>();

    [Obsolete("This method is deprecated as JS RegExp to .NET Regex conversion will be removed from the library in the next major version.")]
    public string? GetRegexGroupName(int number)
    {
        if (_additionalData is Tokenizer.RegExpCapturingGroup[] capturingGroups)
        {
            return (uint)--number < (uint)capturingGroups.Length
                ? capturingGroups[number].Name
                : null;
        }

        return ThrowInvalidOperationException<string>();
    }
}
