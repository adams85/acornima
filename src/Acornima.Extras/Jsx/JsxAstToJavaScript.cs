using System.IO;
using Acornima.Ast;

namespace Acornima.Jsx;

public record class JsxAstToJavaScriptOptions : AstToJavaScriptOptions
{
    public static new readonly JsxAstToJavaScriptOptions Default = new();

    protected internal override AstToJavaScriptConverter CreateConverter(JavaScriptTextWriter writer) => new JsxAstToJavaScriptConverter(writer, this);
}

public static class JsxAstToJavaScript
{
    public static string ToJsx(this Node node)
    {
        return ToJsx(node, JavaScriptTextWriterOptions.Default, JsxAstToJavaScriptOptions.Default);
    }

    public static string ToJsx(this Node node, KnRJavaScriptTextFormatterOptions formattingOptions)
    {
        return ToJsx(node, formattingOptions, JsxAstToJavaScriptOptions.Default);
    }

    public static string ToJsx(this Node node, bool format)
    {
        return ToJsx(node, format ? KnRJavaScriptTextFormatterOptions.Default : JavaScriptTextWriterOptions.Default, JsxAstToJavaScriptOptions.Default);
    }

    public static string ToJsx(this Node node, JavaScriptTextWriterOptions writerOptions, JsxAstToJavaScriptOptions options)
    {
        return node.ToJavaScript(writerOptions, options);
    }

    public static void WriteJsx(this Node node, TextWriter writer)
    {
        WriteJsx(node, writer, JavaScriptTextWriterOptions.Default, JsxAstToJavaScriptOptions.Default);
    }

    public static void WriteJsx(this Node node, TextWriter writer, KnRJavaScriptTextFormatterOptions formattingOptions)
    {
        WriteJsx(node, writer, formattingOptions, JsxAstToJavaScriptOptions.Default);
    }

    public static void WriteJsx(this Node node, TextWriter writer, bool format)
    {
        WriteJsx(node, writer, format ? KnRJavaScriptTextFormatterOptions.Default : JavaScriptTextWriterOptions.Default, JsxAstToJavaScriptOptions.Default);
    }

    public static void WriteJsx(this Node node, TextWriter writer, JavaScriptTextWriterOptions writerOptions, JsxAstToJavaScriptOptions options)
    {
        node.WriteJavaScript(writer, writerOptions, options);
    }

    public static void WriteJsx(this Node node, JavaScriptTextWriter writer, JsxAstToJavaScriptOptions options)
    {
        node.WriteJavaScript(writer, options);
    }
}
