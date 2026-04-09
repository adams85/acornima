using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/options.js

public delegate void OnTokenHandler(in Token token);

public delegate void OnCommentHandler(in Comment comment);

public delegate RegExpParseResult OnRegExpHandler(in RegExpParsingContext context);

public record class TokenizerOptions
{
    public static readonly TokenizerOptions Default = new();

    internal EcmaVersion _ecmaVersion = EcmaVersion.Latest;
    /// <summary>
    /// Gets or sets the ECMAScript version to parse.
    /// Must be either ES3, ES5, ES6 (or ES2015), ES7 (ES2016), ES8 (ES2017), ES9 (ES2018),
    /// ES10 (ES2019), ES11 (ES2020), ES12 (ES2021), ES13 (ES2022), ES14 (ES2023), ES15 (ES2024),
    /// ES16 (ES2025), ES17 (ES2026) or Latest (the latest version the library supports).
    /// </summary>
    /// <remarks>
    /// This influences support for strict mode, the set of reserved words, and support
    /// for new syntax features.
    /// </remarks>
    public EcmaVersion EcmaVersion { get => _ecmaVersion; init => _ecmaVersion = value; }

    internal ExperimentalESFeatures _experimentalESFeatures;
    /// <summary>
    /// Gets or sets which experimental ECMAScript language features to enable.
    /// Defaults to <see cref="ExperimentalESFeatures.None"/>.
    /// </summary>
    public ExperimentalESFeatures ExperimentalESFeatures { get => _experimentalESFeatures; init => _experimentalESFeatures = value; }

    internal bool? _allowHashBang;
    /// <summary>
    /// Gets or sets whether to allow hashbang directive at the beginning of file and treat it as a line comment.
    /// Defaults to <see langword="null"/>, in which case hashbang comment is allowed if <see cref="EcmaVersion"/> >= ES2023.
    /// </summary>
    public bool? AllowHashBang { get => _allowHashBang; init => _allowHashBang = value; }

#pragma warning disable CS0618 // Type or member is obsolete
    internal RegExpParseMode _regExpParseMode = RegExpParseMode.Validate;
    /// <summary>
    /// Gets or sets how regular expressions should be parsed. Defaults to <see cref="RegExpParseMode.Validate"/>.
    /// </summary>
    [Obsolete("This option is deprecated as JS RegExp to .NET Regex conversion will be removed from the library in the next major version. Use the `OnRegExp` option instead.")]
    public RegExpParseMode RegExpParseMode { get => _regExpParseMode; init => _regExpParseMode = value; }
#pragma warning restore CS0618 // Type or member is obsolete

    internal TimeSpan _regexTimeout = TimeSpan.FromSeconds(5);
    /// <summary>
    /// Gets or sets the default timeout for created <see cref="Regex"/> instances. Defaults to 5 seconds.
    /// </summary>
    [Obsolete("This option is deprecated as JS RegExp to .NET Regex conversion will be removed from the library in the next major version.")]
    public TimeSpan RegexTimeout { get => _regexTimeout; init => _regexTimeout = value; }

    internal bool _tolerant;
    /// <summary>
    /// Gets or sets whether to ignore minor errors that do not affect the semantics of the parsed program.
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
        init => _errorHandler = value ?? ThrowArgumentNullException<ParseErrorHandler>(nameof(value));
    }

    internal OnTokenHandler? _onToken;
    /// <summary>
    /// Gets or sets an optional callback function which will be called whenever a token is read.
    /// </summary>
    /// <remarks>
    /// Parameters of the token will be passed to the callback as a <see cref="Token"/> object,
    /// in the same format as returned by <see cref="Tokenizer.GetToken"/>.<br/>
    /// Note that you should not call the tokenizer from the callback as that would corrupt its internal state.
    /// </remarks>
    public OnTokenHandler? OnToken { get => _onToken; init => _onToken = value; }

    internal OnCommentHandler? _onComment;
    /// <summary>
    /// Gets or sets an optional callback function which will be called whenever a comment is skipped.
    /// </summary>
    /// <remarks>
    /// Parameters of the comment will be passed to the callback as a <see cref="Comment"/> object.
    /// </remarks>
    public OnCommentHandler? OnComment { get => _onComment; init => _onComment = value; }

    internal OnRegExpHandler? _onRegExp;
    /// <summary>
    /// Gets or sets an optional callback function which will be called instead of the built-in parsing logic
    /// whenever a regular expression is read.
    /// </summary>
    /// <remarks>
    /// You can use the callback to hook into regular expression parsing, e.g., to parse regular expressions
    /// into ASTs, convert them to .NET objects that can execute them or skip parsing altogether).
    /// </remarks>
    public OnRegExpHandler? OnRegExp { get => _onRegExp; init => _onRegExp = value; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AllowDecorators()
    {
        // NOTE: Static class initialization block, which is part of this feature, is only available since ES2022.
        return _ecmaVersion >= EcmaVersion.ES13 && (_experimentalESFeatures & ExperimentalESFeatures.Decorators) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AllowImportAttributes()
    {
        return _ecmaVersion >= EcmaVersion.ES16
#pragma warning disable CS0618 // Type or member is obsolete
            // NOTE: Dynamic import, which is part of this feature, is only available since ES2020.
            || _ecmaVersion >= EcmaVersion.ES11 && (_experimentalESFeatures & ExperimentalESFeatures.ImportAttributes) != 0;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AllowRegExpDuplicateNamedCapturingGroups()
    {
        return _ecmaVersion >= EcmaVersion.ES16
#pragma warning disable CS0618 // Type or member is obsolete
            || (_experimentalESFeatures & ExperimentalESFeatures.RegExpDuplicateNamedCapturingGroups) != 0;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AllowExplicitResourceManagement()
    {
        return _ecmaVersion >= EcmaVersion.ES17
#pragma warning disable CS0618 // Type or member is obsolete
            // NOTE: Async/await, which is part of this feature, is only available since ES2017.
            || _ecmaVersion >= EcmaVersion.ES8 && (_experimentalESFeatures & ExperimentalESFeatures.ExplicitResourceManagement) != 0;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool AllowRegExpModifiers()
    {
        return _ecmaVersion >= EcmaVersion.ES16
#pragma warning disable CS0618 // Type or member is obsolete
            // NOTE: The dotAll mode (flag 's'), which is part of this feature, is only available since ES2018.
            || _ecmaVersion >= EcmaVersion.ES9 && (_experimentalESFeatures & ExperimentalESFeatures.RegExpModifiers) != 0;
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
