using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

public static class TokenizerExtensions
{
    public static void Reset(this ITokenizer tokenizer, string input, SourceType sourceType = SourceType.Script, string? sourceFile = null)
        => tokenizer.Reset(input, start: 0, sourceType, sourceFile);

    public static void Reset(this ITokenizer tokenizer, string input, int start, SourceType sourceType = SourceType.Script, string? sourceFile = null)
        => tokenizer.Reset(input ?? ThrowArgumentNullException<string>(nameof(input)), start, input.Length - start, sourceType, sourceFile);
}
