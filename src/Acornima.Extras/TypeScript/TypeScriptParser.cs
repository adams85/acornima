using Acornima.Ast;

namespace Acornima.TypeScript;

/// <summary>
/// A TypeScript parser that extends the standard JavaScript parser to handle TypeScript syntax.
/// It uses the TypeScript tokenizer to skip TypeScript-specific constructs at the token level.
/// </summary>
public sealed class TypeScriptParser : IParser, Parser.IExtension
{
    private readonly TypeScriptParserOptions _options;
    private readonly Parser _parser;

    public TypeScriptParser() : this(TypeScriptParserOptions.Default) { }

    public TypeScriptParser(TypeScriptParserOptions options)
    {
        _options = options;
        _parser = new Parser(options, extension: this);
    }

    public TypeScriptParserOptions Options => _options;
    ParserOptions IParser.Options => _options;

    Tokenizer Parser.IExtension.CreateTokenizer(TokenizerOptions tokenizerOptions) =>
        new TypeScriptTokenizer((TypeScriptTokenizerOptions)tokenizerOptions)._tokenizer;

    Expression Parser.IExtension.ParseExprAtom()
    {
        // TypeScript doesn't add new expression types, just delegate to the base parser
        return _parser.Unexpected<Expression>();
    }

    public Script ParseScript(string input, int start, int length, string? sourceFile = null, bool strict = false)
        => _parser.ParseScript(input, start, length, sourceFile, strict);

    public Module ParseModule(string input, int start, int length, string? sourceFile = null)
        => _parser.ParseModule(input, start, length, sourceFile);

    public Expression ParseExpression(string input, int start, int length, string? sourceFile = null, bool strict = false)
        => _parser.ParseExpression(input, start, length, sourceFile, strict);
}
