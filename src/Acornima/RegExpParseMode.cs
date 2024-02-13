using System.Text.RegularExpressions;

namespace Acornima;

/// <summary>
/// Specifies how the tokenizer should parse regular expressions.
/// </summary>
public enum RegExpParseMode
{
    /// <summary>
    /// Scan regular expressions without checking that they are syntactically correct.
    /// </summary>
    Skip,
    /// <summary>
    /// Scan regular expressions and check that they are syntactically correct (throw <see cref="ParseErrorException"/> if an invalid regular expression is encountered)
    /// but don't attempt to convert them to an equivalent <see cref="Regex"/>.
    /// </summary>
    Validate,
    /// <summary>
    /// Scan regular expressions, check that they are syntactically correct (throw <see cref="ParseErrorException"/> if an invalid regular expression is encountered)
    /// and attempt to convert them to an equivalent <see cref="Regex"/> without the <see cref="RegexOptions.Compiled"/> option.
    /// </summary>
    /// <remarks>
    /// In the case of a valid regular expression for which an equivalent <see cref="Regex"/> cannot be constructed, either <see cref="ParseErrorException"/> is thrown
    /// or a <see cref="Token"/> is created with the <see cref="Token.Value"/> property set to <see langword="null"/>, depending on the <see cref="TokenizerOptions.Tolerant"/> option.
    /// </remarks>
    AdaptToInterpreted,
    /// <summary>
    /// Scan regular expressions, check that they are syntactically correct (throw <see cref="ParseErrorException"/> if an invalid regular expression is encountered)
    /// and attempt to convert them to an equivalent <see cref="Regex"/> with the <see cref="RegexOptions.Compiled"/> option.
    /// </summary>
    /// <remarks>
    /// In the case of a valid regular expression for which an equivalent <see cref="Regex"/> cannot be constructed, either <see cref="ParseErrorException"/> is thrown
    /// or a <see cref="Token"/> is created with the <see cref="Token.Value"/> property set to <see langword="null"/>, depending on the <see cref="TokenizerOptions.Tolerant"/> option.
    /// </remarks>
    AdaptToCompiled,
}
