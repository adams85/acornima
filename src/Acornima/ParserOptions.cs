using System;
using System.Text.RegularExpressions;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/options.js

public delegate void OnInsertedSemicolonHandler(int lastTokenEnd, Position lastTokenEndLocation);

public delegate void OnTrailingCommaHandler(int lastTokenEnd, Position lastTokenEndLocation);

public delegate void OnNodeHandler(Node node, OnNodeContext context);

internal interface IOnNodeHandlerWrapper
{
    OnNodeHandler? OnNode { get; set; }

    void ReleaseLargeBuffers();
}

public record class ParserOptions
{
    public static readonly ParserOptions Default = new();

    public ParserOptions() : this(new TokenizerOptions()) { }

    protected ParserOptions(TokenizerOptions tokenizerOptions)
    {
        _tokenizerOptions = tokenizerOptions;
    }

    protected ParserOptions(ParserOptions original)
    {
        _tokenizerOptions = original._tokenizerOptions with { };
        _allowReserved = original._allowReserved;
        _allowReturnOutsideFunction = original._allowReturnOutsideFunction;
        _allowImportExportEverywhere = original._allowImportExportEverywhere;
        _allowAwaitOutsideFunction = original._allowAwaitOutsideFunction;
        _allowNewTargetOutsideFunction = original._allowNewTargetOutsideFunction;
        _allowSuperOutsideMethod = original._allowSuperOutsideMethod;
        _checkPrivateFields = original._checkPrivateFields;
        _onInsertedSemicolon = original._onInsertedSemicolon;
        _onTrailingComma = original._onTrailingComma;
        _onNode = original._onNode;
        _preserveParens = original._preserveParens;
    }

    private readonly TokenizerOptions _tokenizerOptions;

    public TokenizerOptions GetTokenizerOptions() => _tokenizerOptions;

    /// <summary>
    /// Gets or sets the ECMAScript version to parse.
    /// Must be either ES3, ES5, ES6 (or ES2015), ES7 (ES2016), ES8 (ES2017), ES9 (ES2018), ES10
    /// (ES2019), ES11 (ES2020), ES12 (ES2021), ES13 (ES2022), ES14 (ES2023), or Latest
    /// (the latest version the library supports). Defaults to <see cref="EcmaVersion.Latest"/>.
    /// </summary>
    /// <remarks>
    /// This influences support for strict mode, the set of reserved words, and support
    /// for new syntax features.
    /// </remarks>
    public EcmaVersion EcmaVersion
    {
        get => _tokenizerOptions._ecmaVersion;
        init => _tokenizerOptions._ecmaVersion = value;
    }

    /// <summary>
    /// Gets or sets which experimental ECMAScript language features to enable.
    /// Defaults to <see cref="ExperimentalESFeatures.None"/>.
    /// </summary>
    public ExperimentalESFeatures ExperimentalESFeatures
    {
        get => _tokenizerOptions._experimentalESFeatures;
        init => _tokenizerOptions._experimentalESFeatures = value;
    }

    internal readonly AllowReservedOption _allowReserved;
    /// <summary>
    /// Gets or sets whether to enforce reserved words. Defaults to <see cref="AllowReservedOption.Default"/>,
    /// in which case reserved words are only enforced if <see cref="EcmaVersion"/> >= ES5.
    /// </summary>
    /// <remarks>
    /// Set <see cref="AllowReserved"/> to <see cref="AllowReservedOption.Yes"/> or <see cref="AllowReservedOption.No"/>
    /// to explicitly enable or disable this behavior. When this option has the value <see cref="AllowReservedOption.Never"/>,
    /// reserved words and keywords can not even be used as property names.
    /// </remarks>
    public AllowReservedOption AllowReserved { get => _allowReserved; init => _allowReserved = value; }

    internal readonly bool _allowReturnOutsideFunction;
    /// <summary>
    /// Gets or sets whether to allow return statements at the top level.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool AllowReturnOutsideFunction { get => _allowReturnOutsideFunction; init => _allowReturnOutsideFunction = value; }

    internal readonly bool _allowImportExportEverywhere;
    /// <summary>
    /// Gets or sets whether to allow import/export statements at locations other than the top level.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool AllowImportExportEverywhere { get => _allowImportExportEverywhere; init => _allowImportExportEverywhere = value; }

    internal readonly bool _allowAwaitOutsideFunction;
    /// <summary>
    /// Gets or sets whether to allow await identifiers in the top-level scope.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// They will not be allowed in non-async functions even when enabling this option.
    /// </remarks>
    public bool AllowAwaitOutsideFunction { get => _allowAwaitOutsideFunction; init => _allowAwaitOutsideFunction = value; }

    internal readonly bool _allowNewTargetOutsideFunction;
    /// <summary>
    /// Gets or sets whether to allow new.target meta-properties in the top-level scope.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool AllowNewTargetOutsideFunction { get => _allowNewTargetOutsideFunction; init => _allowNewTargetOutsideFunction = value; }

    internal readonly bool _allowSuperOutsideMethod;
    /// <summary>
    /// Gets or sets whether to allow super identifiers to appear outside methods.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool AllowSuperOutsideMethod { get => _allowSuperOutsideMethod; init => _allowSuperOutsideMethod = value; }

    internal readonly bool _allowTopLevelUsing;
    /// <summary>
    /// Gets or sets whether to allow using declarations to appear at the top level of scripts as well.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool AllowTopLevelUsing { get => _allowTopLevelUsing; init => _allowTopLevelUsing = value; }

    /// <summary>
    /// Gets or sets whether to allow hashbang directive at the beginning of file and treat it as a line comment.
    /// Defaults to <see langword="null"/>, in which case hashbang comment is allowed if <see cref="EcmaVersion"/> >= ES2023.
    /// </summary>
    public bool? AllowHashBang { get => _tokenizerOptions._allowHashBang; init => _tokenizerOptions._allowHashBang = value; }

    internal readonly bool _checkPrivateFields = true;
    /// <summary>
    /// Gets or sets whether to verify that private properties are only used in places where they are valid and have been declared.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool CheckPrivateFields { get => _checkPrivateFields; init => _checkPrivateFields = value; }

    internal readonly bool _preserveParens;
    /// <summary>
    /// Gets or sets whether to represent parenthesized expressions by (non-standard) <see cref="ParenthesizedExpression"/> nodes in the AST.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool PreserveParens { get => _preserveParens; init => _preserveParens = value; }

    /// <summary>
    /// Gets or sets how regular expressions should be parsed. Defaults to <see cref="RegExpParseMode.Validate"/>.
    /// </summary>
    public RegExpParseMode RegExpParseMode { get => _tokenizerOptions._regExpParseMode; init => _tokenizerOptions._regExpParseMode = value; }

    /// <summary>
    /// Gets or sets the default timeout for created <see cref="Regex"/> instances. Defaults to 5 seconds.
    /// </summary>
    public TimeSpan RegexTimeout { get => _tokenizerOptions._regexTimeout; init => _tokenizerOptions._regexTimeout = value; }

    /// <summary>
    /// Gets or sets whether to ignore minor errors that do not affect the semantics of the parsed program.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool Tolerant { get => _tokenizerOptions._tolerant; init => _tokenizerOptions._tolerant = value; }

    /// <summary>
    /// Gets or sets the <see cref="ParseErrorHandler"/> to use. Defaults to <see cref="ParseErrorHandler.Default"/>.
    /// </summary>
    public ParseErrorHandler ErrorHandler
    {
        get => _tokenizerOptions._errorHandler;
        init => _tokenizerOptions._errorHandler = value ?? ThrowArgumentNullException<ParseErrorHandler>(nameof(value));
    }

    /// <summary>
    /// Gets or sets an optional callback function which will be called whenever a token is read.
    /// </summary>
    /// <remarks>
    /// It will be passed the parameters of the token as a <see cref="Token"/> object,
    /// in the same format as returned by <see cref="Tokenizer.GetToken"/>.
    /// </remarks>
    public OnTokenHandler? OnToken { get => _tokenizerOptions._onToken; init => _tokenizerOptions._onToken = value; }

    /// <summary>
    /// Gets or sets an optional callback function which will be called whenever a comment is skipped.
    /// </summary>
    /// <remarks>
    /// It will be passed the parameters of the comment as a <see cref="Comment"/> object.
    /// </remarks>
    public OnCommentHandler? OnComment { get => _tokenizerOptions._onComment; init => _tokenizerOptions._onComment = value; }

    internal readonly OnInsertedSemicolonHandler? _onInsertedSemicolon;
    /// <summary>
    /// Gets or sets an optional callback function which will be called when a semicolon is automatically inserted.
    /// </summary>
    /// <remarks>
    /// It will be passed the position of the inserted semicolon as an offset and the location as a <see cref="Position"/> object.
    /// </remarks>
    public OnInsertedSemicolonHandler? OnInsertedSemicolon { get => _onInsertedSemicolon; init => _onInsertedSemicolon = value; }

    internal readonly OnTrailingCommaHandler? _onTrailingComma;
    /// <summary>
    /// Gets or sets an optional callback function which will be called when a trailing comma is encountered.
    /// </summary>
    /// <remarks>
    /// It will be passed the position of the trailing comma as an offset and the location as a <see cref="Position"/> object.
    /// </remarks>
    public OnTrailingCommaHandler? OnTrailingComma { get => _onTrailingComma; init => _onTrailingComma = value; }

    internal OnNodeHandler? _onNode;
    /// <summary>
    /// Gets or sets an optional callback which will be called whenever an AST node is parsed.
    /// </summary>
    /// <remarks>
    /// This callback allows you to make changes to the nodes created by the parser.
    /// E.g. you can use it to store a reference to the parent node for later use:
    /// <code>
    /// OnNode = (node, _) =>
    /// {
    ///     foreach (var child in node.ChildNodes)
    ///     {
    ///         child.UserData = node;
    ///     }
    /// };
    /// </code>
    /// Please note that the callback is also executed on nodes which are reinterpreted
    /// later during parsing, that is, on nodes which won't become a part of the final AST.
    /// </remarks>
    public OnNodeHandler? OnNode
    {
        get => _onNode?.Target is IOnNodeHandlerWrapper wrapper ? wrapper.OnNode : _onNode;
        init
        {
            if (_onNode?.Target is IOnNodeHandlerWrapper wrapper)
            {
                wrapper.OnNode = value;
            }
            else
            {
                _onNode = value;
            }
        }
    }
}
