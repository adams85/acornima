using System.IO;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

public enum LocationMembersPlacement
{
    End,
    Start
}

public record class AstToJsonOptions
{
    public static readonly AstToJsonOptions Default = new();

    public bool IncludeLineColumn { get; init; }
    public bool IncludeRange { get; init; }
    public LocationMembersPlacement LocationMembersPlacement { get; init; }

    protected internal virtual AstToJsonConverter CreateConverter(JsonWriter writer) => new AstToJsonConverter(writer, this);
}

public static class AstToJson
{
    public static string ToJson(this Node node)
    {
        return ToJson(node, indent: null);
    }

    public static string ToJson(this Node node, string? indent)
    {
        return ToJson(node, AstToJsonOptions.Default, indent);
    }

    public static string ToJson(this Node node, AstToJsonOptions options)
    {
        return ToJson(node, options, indent: null);
    }

    public static string ToJson(this Node node, AstToJsonOptions options, string? indent)
    {
        using (var writer = new StringWriter())
        {
            WriteJson(node, writer, options, indent);
            return writer.ToString();
        }
    }

    public static void WriteJson(this Node node, TextWriter writer)
    {
        WriteJson(node, writer, indent: null);
    }

    public static void WriteJson(this Node node, TextWriter writer, string? indent)
    {
        WriteJson(node, writer, AstToJsonOptions.Default, indent);
    }

    public static void WriteJson(this Node node, TextWriter writer, AstToJsonOptions options)
    {
        WriteJson(node, writer, options, indent: null);
    }

    public static void WriteJson(this Node node, TextWriter writer, AstToJsonOptions options, string? indent)
    {
        WriteJson(node, new JsonTextWriter(writer, indent), options);
    }

    public static void WriteJson(this Node node, JsonWriter writer, AstToJsonOptions options)
    {
        if (options is null)
        {
            ThrowArgumentNullException<object>(nameof(options));
        }

        options.CreateConverter(writer).Convert(node);
    }
}
