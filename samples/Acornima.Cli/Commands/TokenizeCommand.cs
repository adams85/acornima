using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Acornima.Cli.Helpers;
using Acornima.Jsx;
using McMaster.Extensions.CommandLineUtils;

namespace Acornima.Cli.Commands;

[Command(CommandName, Description = "Tokenize JS code and print collected tokens in JSON format.")]
internal sealed class TokenizeCommand
{
    public const string CommandName = "tokenize";


    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        IncludeFields = true,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        },
    };

    private readonly IConsole _console;

    public TokenizeCommand(IConsole console)
    {
        _console = console;
    }

    [Option("--type", Description = "Type of the JS code to parse.")]
    public SourceType SourceType { get; set; } = SourceType.Script;

    [Option("--jsx", Description = "Allow JSX expressions.")]
    public bool AllowJsx { get; set; }

    [Option("-c|--comments", Description = "Also include comments.")]
    public bool Comments { get; set; }

    [Option("-t|--tolerant", Description = "Tolerate noncritical syntax errors.")]
    public bool Tolerant { get; set; }

    // TODO: more options

    [Argument(0, Description = "The JS code to tokenize. If omitted, the code will be read from the standard input.")]
    public string? Code { get; }

    private T CreateTokenizerOptions<T>(OnCommentHandler? commentHandler) where T : TokenizerOptions, new() => new T
    {
        Tolerant = Tolerant,
        OnComment = commentHandler
    };

    public int OnExecute()
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;

        var code = Code ?? _console.ReadString();

        var comments = new List<CommentData>();
        OnCommentHandler? commentHandler = Comments
            ? (in Comment comment) => comments.Add(CommentData.From(comment, code))
            : null;

        ITokenizer tokenizer = AllowJsx
            ? new JsxTokenizer(code, SourceType, sourceFile: null, CreateTokenizerOptions<JsxTokenizerOptions>(commentHandler))
            : new Tokenizer(code, SourceType, sourceFile: null, CreateTokenizerOptions<TokenizerOptions>(commentHandler));

        var tokensAndComments = new List<object>();
        tokensAndComments.AddRange(comments);

        Token token;
        do
        {
            comments.Clear();
            token = tokenizer.GetToken();
            tokensAndComments.AddRange(comments);
            tokensAndComments.Add(TokenData.From(in token, code));
        }
        while (token.Kind != TokenKind.EOF);

        _console.WriteLine(JsonSerializer.Serialize(tokensAndComments, serializerOptions));

        return 0;
    }

    private abstract record class SyntaxElementData
    {
        public abstract string Type { get; }
    }

    private sealed record class TokenData(string Kind, object? Value, string RawValue, Range Range, in SourceLocation Location) : SyntaxElementData
    {
        public static TokenData From(in Token token, string code)
        {
            return new TokenData(token.KindText, token.Value, token.GetRawValue(code).ToString(), token.Range, token.LocationRef());
        }

        [JsonPropertyOrder(-1)]
        public override string Type => "Token";
    }

    private sealed record class CommentData(CommentKind Kind, string Content, Range Range, in SourceLocation Location) : SyntaxElementData
    {
        public static CommentData From(in Comment comment, string code)
        {
            return new CommentData(comment.Kind, comment.GetContent(code).ToString(), comment.Range, comment.LocationRef());
        }

        [JsonPropertyOrder(-1)]
        public override string Type => "Comment";
    }
}
