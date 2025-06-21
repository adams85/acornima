namespace Acornima.TypeScript;

public record class TypeScriptParserOptions : ParserOptions
{
    public static new readonly TypeScriptParserOptions Default = new();

    public TypeScriptParserOptions() : base(new TypeScriptTokenizerOptions()) { }

    public new TypeScriptTokenizerOptions GetTokenizerOptions() => (TypeScriptTokenizerOptions)base.GetTokenizerOptions();
}
