using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

public static class ParserExtensions
{
    public static Script ParseScript(this IParser parser, string input, string? sourceFile = null, bool strict = false)
        => parser.ParseScript(input ?? ThrowArgumentNullException<string>(nameof(input)), 0, input.Length, sourceFile, strict);

    public static Module ParseModule(this IParser parser, string input, string? sourceFile = null)
        => parser.ParseModule(input ?? ThrowArgumentNullException<string>(nameof(input)), 0, input.Length, sourceFile);

    public static Expression ParseExpression(this IParser parser, string input, string? sourceFile = null, bool strict = false)
        => parser.ParseExpression(input ?? ThrowArgumentNullException<string>(nameof(input)), 0, input.Length, sourceFile, strict);
}
