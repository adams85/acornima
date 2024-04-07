using System.Diagnostics;

namespace Acornima;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js

// The algorithm used to determine whether a regexp can appear at a
// given point in the program is loosely based on sweet.js' approach.
// See https://github.com/mozilla/sweet.js/wiki/design
#if DEBUG
[DebuggerDisplay($"{{{nameof(Kind)}}}, {nameof(IsExpression)} = {{{nameof(IsExpression)}}}, {nameof(Generator)} = {{{nameof(Generator)}}}")]
#endif
internal sealed class TokenContext
{
    public static readonly TokenContext BracketsInStatement = new(TokenContextKind.BraceLeft, isExpression: false);
    public static readonly TokenContext BracketsInExpression = new(TokenContextKind.BraceLeft, isExpression: true);
    public static readonly TokenContext BracketsInTemplate = new(TokenContextKind.DollarBraceLeft, isExpression: false);
    public static readonly TokenContext ParensInStatement = new(TokenContextKind.ParenLeft, isExpression: false);
    public static readonly TokenContext ParensInExpression = new(TokenContextKind.ParenLeft, isExpression: true);
    public static readonly TokenContext QuoteInTemplate = new(TokenContextKind.BackQuote, isExpression: true, preserveSpace: true);
    public static readonly TokenContext FunctionInStatement = new(TokenContextKind.Function, isExpression: false);
    public static readonly TokenContext FunctionInExpression = new(TokenContextKind.Function, isExpression: true);
    public static readonly TokenContext GeneratorFunctionInStatement = new(TokenContextKind.Function, isExpression: false, generator: true);
    public static readonly TokenContext GeneratorFunctionInExpression = new(TokenContextKind.Function, isExpression: true, generator: true);

    public TokenContext(TokenContextKind kind, bool isExpression, bool preserveSpace = false, bool generator = false)
    {
        Kind = kind;
        IsExpression = isExpression;
        PreserveSpace = preserveSpace;
        Generator = generator;
    }

    public readonly TokenContextKind Kind;
    public readonly bool IsExpression;
    public readonly bool PreserveSpace;
    public readonly bool Generator;
}
