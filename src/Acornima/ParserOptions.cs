using System;
using System.Text.RegularExpressions;
using Acornima.Ast;

namespace Acornima;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/options.js

public delegate void OnInsertedSemicolonHandler(int lastTokenEnd, Position lastTokenEndLocation);

public delegate void OnTrailingCommaHandler(int lastTokenEnd, Position lastTokenEndLocation);

public delegate void OnNodeHandler(Node node);

public record class ParserOptions
{
    public static readonly ParserOptions Default = new();

    protected ParserOptions(ParserOptions original)
    {
        _tokenizerOptions = original._tokenizerOptions with { };
        _allowReserved = original._allowReserved;
        _allowReturnOutsideFunction = original._allowReturnOutsideFunction;
        _allowImportExportEverywhere = original._allowImportExportEverywhere;
        _allowAwaitOutsideFunction = original._allowAwaitOutsideFunction;
        _allowSuperOutsideMethod = original._allowSuperOutsideMethod;
        _checkPrivateFields = original._checkPrivateFields;
        _onInsertedSemicolon = original._onInsertedSemicolon;
        _onTrailingComma = original._onTrailingComma;
        _onNode = original._onNode;
        _preserveParens = original._preserveParens;
    }

    private readonly TokenizerOptions _tokenizerOptions = new();

    public TokenizerOptions GetTokenizerOptions() => _tokenizerOptions;

    /// <summary>
    /// <see cref="EcmaVersion"/> indicates the ECMAScript version to parse. Must be
    /// either ES3, ES5, ES6 (or ES2015), ES7 (ES2016), ES8 (ES2017), ES9 (ES2018), ES10
    /// (ES2019), ES11 (ES2020), ES12 (ES2021), ES13 (ES2022), ES14 (ES2023), or Latest
    /// (the latest version the library supports).<br/>
    /// This influences
    /// support for strict mode, the set of reserved words, and support
    /// for new syntax features.
    /// </summary>
    public EcmaVersion EcmaVersion
    {
        get => _tokenizerOptions._ecmaVersion;
        init => _tokenizerOptions._ecmaVersion = value;
    }

    internal readonly AllowReservedOption _allowReserved;
    /// <summary>
    /// By default, reserved words are only enforced if <see cref="EcmaVersion"/> >= ES5.
    /// Set <see cref="AllowReserved"/> to a boolean value to explicitly enable or disable this behavior.<br/>
    /// When this option has the value <see cref="AllowReservedOption.Never"/>, reserved words
    /// and keywords can also not be used as property names.
    /// </summary>
    public AllowReservedOption AllowReserved { get => _allowReserved; init => _allowReserved = value; }

    internal readonly bool _allowReturnOutsideFunction;
    /// <summary>
    /// When enabled, a return at the top level is not considered an
    /// error.
    /// </summary>
    public bool AllowReturnOutsideFunction { get => _allowReturnOutsideFunction; init => _allowReturnOutsideFunction = value; }

    internal readonly bool _allowImportExportEverywhere;
    /// <summary>
    /// When enabled, import/export statements are not constrained to
    /// appearing at the top of the program.
    /// </summary>
    public bool AllowImportExportEverywhere { get => _allowImportExportEverywhere; init => _allowImportExportEverywhere = value; }

    internal readonly bool _allowAwaitOutsideFunction;
    /// <summary>
    /// When enabled, await identifiers are allowed to appear at the top-level scope,
    /// but they are still not allowed in non-async functions.
    /// </summary>
    public bool AllowAwaitOutsideFunction { get => _allowAwaitOutsideFunction; init => _allowAwaitOutsideFunction = value; }

    internal readonly bool _allowSuperOutsideMethod;
    /// <summary>
    /// When enabled, super identifiers are not constrained to
    /// appearing in methods and do not raise an error when they appear elsewhere.
    /// </summary>
    public bool AllowSuperOutsideMethod { get => _allowSuperOutsideMethod; init => _allowSuperOutsideMethod = value; }

    /// <summary>
    /// When enabled, hashbang directive at the beginning of file is
    /// allowed and treated as a line comment. Enabled by default when
    /// <see cref="EcmaVersion"/> >= ES2023.
    /// </summary>
    public bool? AllowHashBang { get => _tokenizerOptions._allowHashBang; init => _tokenizerOptions._allowHashBang = value; }

    internal readonly bool _checkPrivateFields = true;
    /// <summary>
    /// By default, the parser will verify that private properties are
    /// only used in places where they are valid and have been declared.
    /// Set this to <see langword="false"/> to turn such checks off.
    /// </summary>
    public bool CheckPrivateFields { get => _checkPrivateFields; init => _checkPrivateFields = value; }

    internal readonly bool _preserveParens;
    /// <summary>
    /// When enabled, parenthesized expressions are represented by
    /// (non-standard) <see cref="ParenthesizedExpression"/> nodes.
    /// </summary>
    public bool PreserveParens { get => _preserveParens; init => _preserveParens = value; }

    /// <summary>
    /// Gets or sets how regular expressions should be parsed. Defaults to <see cref="RegExpParseMode.Validate"/>.
    /// </summary>
    public RegExpParseMode RegExpParseMode { get => _tokenizerOptions._regExpParseMode; init => _tokenizerOptions._regExpParseMode = value; }

    /// <summary>
    /// Default timeout for created <see cref="Regex"/> instances. Defaults to 5 seconds.
    /// </summary>
    public TimeSpan RegexTimeout { get => _tokenizerOptions._regexTimeout; init => _tokenizerOptions._regexTimeout = value; }

    /// <summary>
    /// Gets or sets whether the parser should ignore minor errors that do not affect the semantics of the parsed program.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool Tolerant { get => _tokenizerOptions._tolerant; init => _tokenizerOptions._tolerant = value; }

    /// <summary>
    /// Gets or sets the <see cref="ParseErrorHandler"/> to use. Defaults to <see cref="ParseErrorHandler.Default"/>.
    /// </summary>
    public ParseErrorHandler ErrorHandler
    {
        get => _tokenizerOptions._errorHandler;
        init => _tokenizerOptions._errorHandler = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// A function can be passed as <see cref="OnToken"/> option, which will
    /// cause the tokenizer to call that function with object in the same
    /// format as tokens returned from `<see cref="Tokenizer.GetToken"/>`.
    /// </summary>
    public OnTokenHandler? OnToken { get => _tokenizerOptions._onToken; init => _tokenizerOptions._onToken = value; }

    /// <summary>
    /// A function can be passed as <see cref="OnComment"/> option, which will
    /// cause the tokenizer to call that function with
    /// the parameters of the comment whenever a comment is skipped.
    /// </summary>
    public OnCommentHandler? OnComment { get => _tokenizerOptions._onComment; init => _tokenizerOptions._onComment = value; }

    internal readonly OnInsertedSemicolonHandler? _onInsertedSemicolon;
    /// <summary>
    /// <see cref="OnInsertedSemicolon"/> can be a callback that will be called
    /// when a semicolon is automatically inserted.<br/>
    /// It will be passed the position of the inserted semicolon as an offset, and
    /// it is given the location as a <see cref="Position"/> object as second argument.
    /// </summary>
    public OnInsertedSemicolonHandler? OnInsertedSemicolon { get => _onInsertedSemicolon; init => _onInsertedSemicolon = value; }

    internal readonly OnTrailingCommaHandler? _onTrailingComma;
    /// <summary>
    /// <see cref="OnTrailingComma"/> is similar to  <see cref="OnInsertedSemicolon"/>, but for
    /// trailing commas.
    /// </summary>
    public OnTrailingCommaHandler? OnTrailingComma { get => _onTrailingComma; init => _onTrailingComma = value; }

    internal OnNodeHandler? _onNode;
    /// <summary>
    /// An optional callback to execute on each parsed node.
    /// </summary>
    /// <remarks>
    /// This callback allows you to make changes to the nodes created by the parser.
    /// E.g. you can use it to store a reference to the parent node for later use:
    /// <code>
    /// OnNode = node =>
    /// {
    ///     foreach (var child in node.ChildNodes)
    ///     {
    ///         child.UserData = node;
    ///     }
    /// };
    /// </code>
    /// Please note that the callback is executed on nodes which are reinterpreted
    /// later during parsing, thus, won't become a part of the final AST.
    /// </remarks>
    public OnNodeHandler? OnNode { get => _onNode; init => _onNode = value; }
}
