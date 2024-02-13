using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Acornima.Cli.Helpers;
using McMaster.Extensions.CommandLineUtils;

namespace Acornima.Cli.Commands;

[Command(CommandName, Description = "Tokenize JS code and print collected tokens in JSON format.")]
internal sealed class TokenizeCommand
{
    public const string CommandName = "tokenize";

    private readonly IConsole _console;

    public TokenizeCommand(IConsole console)
    {
        _console = console;
    }

    [Option("--type", Description = "Type of the JS code to parse.")]
    public SourceType SourceType { get; set; } = SourceType.Script;

    [Option("-c|--comments", Description = "Also include comments.")]
    public bool Comments { get; set; }

    [Option("-t|--tolerant", Description = "Tolerate noncritical syntax errors.")]
    public bool Tolerant { get; set; }

    // TODO: more options

    [Argument(0, Description = "The JS code to tokenize. If omitted, the code will be read from the standard input.")]
    public string? Code { get; }

    public int OnExecute()
    {
        Console.InputEncoding = System.Text.Encoding.UTF8;

        var code = Code ?? _console.ReadString();

        var comments = new List<CommentData>();

        var tokenizerOptions = new TokenizerOptions
        {
            Tolerant = Tolerant,
            OnComment = Comments
                ? (in Comment comment) => comments.Add(CommentData.From(comment, code))
                : null
        };

        var tokenizer = new Tokenizer(code, SourceType, sourceFile: null, tokenizerOptions);

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

        var serializerOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter()
            },
        };

        _console.WriteLine(JsonSerializer.Serialize(tokensAndComments, serializerOptions));

        return 0;
    }

    private abstract record class SyntaxElementData
    {
        public abstract string Type { get; }
    }

    private sealed record class TokenData(TokenKind Kind, object? Value, string RawValue, Range Range, SourceLocation Location) : SyntaxElementData
    {
        public static TokenData From(in Token token, string code)
        {
            return new TokenData(token.Kind, token.Value, token.GetRawValue(code).ToString(), token.Range, token.Location);
        }

        [JsonPropertyOrder(-1)]
        public override string Type => "Token";
    }

    private sealed record class CommentData(CommentKind Kind, string Content, Range Range, SourceLocation Location) : SyntaxElementData
    {
        public static CommentData From(in Comment comment, string code)
        {
            return new CommentData(comment.Kind, comment.GetContent(code).ToString(), comment.Range, comment.Location);
        }

        [JsonPropertyOrder(-1)]
        public override string Type => "Comment";
    }
}
