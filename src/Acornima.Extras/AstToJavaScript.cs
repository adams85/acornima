using System.IO;
using Acornima.Ast;
using Acornima.Helpers;
using Acornima.Jsx;

namespace Acornima;

using static ExceptionHelper;

public record class AstToJavaScriptOptions
{
    public static readonly AstToJavaScriptOptions Default = new();

    internal bool IgnoreExtensions { get; init; }

    protected internal virtual AstToJavaScriptConverter CreateConverter(JavaScriptTextWriter writer) => new AstToJavaScriptConverter(writer, this);
}

public static class AstToJavaScript
{
    private static readonly JsxAstToJavaScriptOptions s_nodeToDebugDisplayTextOptions = JsxAstToJavaScriptOptions.Default with { IgnoreExtensions = true };

    internal static string ToDebugDisplayText(this Node node) => node.ToJavaScript(KnRJavaScriptTextFormatterOptions.Default, s_nodeToDebugDisplayTextOptions);

    public static string ToJavaScript(this Node node)
    {
        return ToJavaScript(node, JavaScriptTextWriterOptions.Default, AstToJavaScriptOptions.Default);
    }

    public static string ToJavaScript(this Node node, KnRJavaScriptTextFormatterOptions formattingOptions)
    {
        return ToJavaScript(node, formattingOptions, AstToJavaScriptOptions.Default);
    }

    public static string ToJavaScript(this Node node, bool format)
    {
        return ToJavaScript(node, format ? KnRJavaScriptTextFormatterOptions.Default : JavaScriptTextWriterOptions.Default, AstToJavaScriptOptions.Default);
    }

    public static string ToJavaScript(this Node node, JavaScriptTextWriterOptions writerOptions, AstToJavaScriptOptions options)
    {
        using (var writer = new StringWriter())
        {
            WriteJavaScript(node, writer, writerOptions, options);
            return writer.ToString();
        }
    }

    public static void WriteJavaScript(this Node node, TextWriter writer)
    {
        WriteJavaScript(node, writer, JavaScriptTextWriterOptions.Default, AstToJavaScriptOptions.Default);
    }

    public static void WriteJavaScript(this Node node, TextWriter writer, KnRJavaScriptTextFormatterOptions formattingOptions)
    {
        WriteJavaScript(node, writer, formattingOptions, AstToJavaScriptOptions.Default);
    }

    public static void WriteJavaScript(this Node node, TextWriter writer, bool format)
    {
        WriteJavaScript(node, writer, format ? KnRJavaScriptTextFormatterOptions.Default : JavaScriptTextWriterOptions.Default, AstToJavaScriptOptions.Default);
    }

    public static void WriteJavaScript(this Node node, TextWriter writer, JavaScriptTextWriterOptions writerOptions, AstToJavaScriptOptions options)
    {
        if (writerOptions is null)
        {
            ThrowArgumentNullException<object>(nameof(writerOptions));
        }

        WriteJavaScript(node, writerOptions.CreateWriter(writer), options);
    }

    public static void WriteJavaScript(this Node node, JavaScriptTextWriter writer, AstToJavaScriptOptions options)
    {
        if (options is null)
        {
            ThrowArgumentNullException<object>(nameof(options));
        }

        options.CreateConverter(writer).Convert(node);
    }
}
