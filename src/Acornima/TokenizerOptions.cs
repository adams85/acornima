using System;
using System.Text.RegularExpressions;

namespace Acornima;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/options.js

public delegate void OnTokenHandler(in Token token);

public delegate void OnCommentHandler(in Comment comment);

public record class TokenizerOptions
{
    public static readonly TokenizerOptions Default = new();

    internal EcmaVersion _ecmaVersion = EcmaVersion.Latest;
    /// <summary>
    /// <see cref="EcmaVersion"/> indicates the ECMAScript version to parse. Must be
    /// either ES3, ES5, ES6 (or ES2015), ES7 (ES2016), ES8 (ES2017), ES9 (ES2018), ES10
    /// (ES2019), ES11 (ES2020), ES12 (ES2021), ES13 (ES2022), ES14 (ES2023), or Latest
    /// (the latest version the library supports).<br/>
    /// This influences
    /// support for strict mode, the set of reserved words, and support
    /// for new syntax features.
    /// </summary>
    public EcmaVersion EcmaVersion { get => _ecmaVersion; init => _ecmaVersion = value; }

    internal bool? _allowHashBang;
    /// <summary>
    /// When enabled, hashbang directive at the beginning of file is
    /// allowed and treated as a line comment. Enabled by default when
    /// <see cref="EcmaVersion"/> >= ES2023.
    /// </summary>
    public bool? AllowHashBang { get => _allowHashBang; init => _allowHashBang = value; }

    internal RegExpParseMode _regExpParseMode = RegExpParseMode.Validate;
    /// <summary>
    /// Gets or sets how regular expressions should be parsed. Defaults to <see cref="RegExpParseMode.Validate"/>.
    /// </summary>
    public RegExpParseMode RegExpParseMode { get => _regExpParseMode; init => _regExpParseMode = value; }

    internal TimeSpan _regexTimeout = TimeSpan.FromSeconds(5);
    /// <summary>
    /// Default timeout for created <see cref="Regex"/> instances. Defaults to 5 seconds.
    /// </summary>
    public TimeSpan RegexTimeout { get => _regexTimeout; init => _regexTimeout = value; }

    internal bool _tolerant;
    /// <summary>
    /// Gets or sets whether the tokenizer should ignore minor errors that do not affect the semantics of the parsed program.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    public bool Tolerant { get => _tolerant; init => _tolerant = value; }

    internal ParseErrorHandler _errorHandler = ParseErrorHandler.Default;
    /// <summary>
    /// Gets or sets the <see cref="ParseErrorHandler"/> to use. Defaults to <see cref="ParseErrorHandler.Default"/>.
    /// </summary>
    public ParseErrorHandler ErrorHandler
    {
        get => _errorHandler;
        init => _errorHandler = value ?? throw new ArgumentNullException(nameof(value));
    }

    internal OnTokenHandler? _onToken;
    /// <summary>
    /// A function can be passed as <see cref="OnToken"/> option, which will
    /// cause the tokenizer to call that function with object in the same
    /// format as tokens returned from `<see cref="Tokenizer.GetToken"/>`.<br/>
    /// Note that you are not allowed to call the tokenizer from the
    /// callback — that will corrupt its internal state.
    /// </summary>
    public OnTokenHandler? OnToken { get => _onToken; init => _onToken = value; }

    internal OnCommentHandler? _onComment;
    /// <summary>
    /// A function can be passed as <see cref="OnComment"/> option, which will
    /// cause the tokenizer to call that function with
    /// the parameters of the comment whenever a comment is skipped.
    /// </summary>
    public OnCommentHandler? OnComment { get => _onComment; init => _onComment = value; }
}
