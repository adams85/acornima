using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Acornima.Helpers;

namespace Acornima;

using static Unsafe;

public readonly struct RegExpParseResult
{
    private static readonly object s_boxedDefaultResult = new ValueHolder();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static RegExpParseResult ForValid()
        => new RegExpParseResult(s_boxedDefaultResult, additionalData: null);

    public static RegExpParseResult ForSuccess(object? conversionResult = null, object? additionalData = null)
        => new RegExpParseResult(conversionResult is not null ? new ValueHolder(conversionResult) : s_boxedDefaultResult, additionalData);

    public static RegExpParseResult ForFailure(ParseError? conversionError = null, object? additionalData = null)
        => new RegExpParseResult(conversionError, additionalData);

    private readonly object? _wrappedResultOrError;
    private readonly object? _additionalData;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RegExpParseResult(object? wrappedResultOrError, object? additionalData)
    {
        _wrappedResultOrError = wrappedResultOrError;
        _additionalData = additionalData;
    }

    public bool Success
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _wrappedResultOrError is ValueHolder;
    }

    public object? ConversionResult
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _wrappedResultOrError is ValueHolder
            ? Unbox<ValueHolder>(_wrappedResultOrError).Data
            : null;
    }

    public ParseError? ConversionError
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _wrappedResultOrError as ParseError;
    }

    public object? AdditionalData { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _additionalData; }

    public Regex? Regex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _wrappedResultOrError is ValueHolder
            ? Unbox<ValueHolder>(_wrappedResultOrError).Data as Regex
            : null;
    }
}
