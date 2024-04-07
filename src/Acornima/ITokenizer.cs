namespace Acornima;

public interface ITokenizer
{
    string Input { get; }
    Range Range { get; }
    SourceType SourceType { get; }
    string? SourceFile { get; }

    TokenizerOptions Options { get; }

    Token Current { get; }

    Token GetToken(in TokenizerContext context = default);

    void Next(in TokenizerContext context = default);

    void Reset(string input, int start, int length, SourceType sourceType = SourceType.Script, string? sourceFile = null);
}
