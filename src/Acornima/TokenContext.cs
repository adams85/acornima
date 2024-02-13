using System.Diagnostics;

namespace Acornima;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokencontext.js

// The algorithm used to determine whether a regexp can appear at a
// given point in the program is loosely based on sweet.js' approach.
// See https://github.com/mozilla/sweet.js/wiki/design
[DebuggerDisplay($"{{{nameof(Kind)}}}, {nameof(IsExpression)} = {{{nameof(IsExpression)}}}, {nameof(Generator)} = {{{nameof(Generator)}}}")]
internal sealed class TokenContext
{
    public static readonly TokenContext BracketsInStatement = new(TokenContextKind.BraceLeft, false);
    public static readonly TokenContext BracketsInExpression = new(TokenContextKind.BraceLeft, true);
    public static readonly TokenContext BracketsInTemplate = new(TokenContextKind.DollarBraceLeft, false);
    public static readonly TokenContext ParensInStatement = new(TokenContextKind.ParenLeft, false);
    public static readonly TokenContext ParensInExpression = new(TokenContextKind.ParenLeft, true);
    public static readonly TokenContext QuoteInTemplate = new(TokenContextKind.BackQuote, true, preserveSpace: true);
    public static readonly TokenContext FunctionInStatement = new(TokenContextKind.Function, false);
    public static readonly TokenContext FunctionInExpression = new(TokenContextKind.Function, true);
    public static readonly TokenContext GeneratorFunctionInStatement = new(TokenContextKind.Function, false, generator: true);
    public static readonly TokenContext GeneratorFunctionInExpression = new(TokenContextKind.Function, true, generator: true);

    private TokenContext(TokenContextKind kind, bool isExpression, bool preserveSpace = false, bool generator = false)
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
