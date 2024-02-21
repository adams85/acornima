using System.IO;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

public record class KnRJavaScriptTextFormatterOptions : JavaScriptTextFormatterOptions
{
    public static new readonly KnRJavaScriptTextFormatterOptions Default = new();

    public KnRJavaScriptTextFormatterOptions()
    {
        KeepEmptyBlockBodyInLine = true;
    }

    public bool UseEgyptianBraces { get; init; } = true;

    protected override JavaScriptTextFormatter CreateFormatter(TextWriter writer) => new KnRJavaScriptTextFormatter(writer, this);
}

/// <summary>
/// JavaScript code formatter which implements the most commonly used <see href="https://en.wikipedia.org/wiki/Indentation_style#K&amp;R_style">K&amp;R style</see>.
/// </summary>
public class KnRJavaScriptTextFormatter : JavaScriptTextFormatter
{
    public KnRJavaScriptTextFormatter(TextWriter writer, KnRJavaScriptTextFormatterOptions options) : base(writer, options)
    {
        UseEgyptianBraces = options.UseEgyptianBraces;
    }

    protected bool UseEgyptianBraces { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected override void WriteWhiteSpaceBetweenTokenAndKeyword(string value, TokenFlags flags, ref WriteContext context)
    {
        if (flags.HasFlagFast(TokenFlags.FollowsStatementBody))
        {
            if (UseEgyptianBraces && CanUseEgyptianBraces(ref context))
            {
                WriteSpace();
            }
            else
            {
                WriteLine();
            }
        }
        else if (flags.HasFlagFast(TokenFlags.LeadingSpaceRecommended) || LastTokenFlags.HasFlagFast(TokenFlags.TrailingSpaceRecommended))
        {
            WriteSpace();
        }
        else
        {
            WriteRequiredSpaceBetweenTokenAndKeyword();
        }
    }

    protected virtual bool CanUseEgyptianBraces(ref WriteContext context)
    {
        return KeepEmptyBlockBodyInLine
            ? RetrieveStatementBodyFromContext(ref context) is NestedBlockStatement { Body.Count: > 0 }
            : RetrieveStatementBodyFromContext(ref context) is NestedBlockStatement;
    }
}
