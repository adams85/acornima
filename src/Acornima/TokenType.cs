using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima;

using KeywordEnum = Keyword;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/tokentype.js

// ## Token types

// The assignment of fine-grained, information-carrying type objects
// allows the tokenizer to store the information it has about a
// token in a way that is very cheap for the parser to look up.

// The `beforeExpr` property is used to disambiguate between regular
// expressions and divisions. It is set on all token types that can
// be followed by an expression (thus, a slash after them would be a
// regular expression).
//
// The `startsExpr` property is used to check if the token ends a
// `yield` expression. It is set on all token types that either can
// directly start an expression (like a quotation mark) or can
// continue an expression (like the body of a string).
//
// `isLoop` marks a keyword as starting a loop, which is important
// to know when parsing a label, in order to allow or disallow
// continue jumps to that label.

[DebuggerDisplay($"{{{nameof(Kind)}}}, {nameof(Label)} = {{{nameof(Label)}}}")]
internal sealed partial class TokenType
{
    public static readonly TokenType EOF = new TokenType("eof", TokenKind.EOF);

    // Identifier token types.
    public static readonly TokenType Name = Identifier("name", startsExpression: true, updateContext: Tokenizer.UpdateContext_Name);
    public static readonly TokenType PrivateId = Identifier("privateId", startsExpression: true);

    // Literal token types.
    public static readonly TokenType Number = Literal("number", TokenKind.NumericLiteral, startsExpression: true);
    public static readonly TokenType BigInt = Literal("bigint", TokenKind.BigIntLiteral, startsExpression: true);
    public static readonly TokenType RegExp = Literal("regexp", TokenKind.RegExpLiteral, startsExpression: true);
    public static readonly TokenType String = Literal("string", TokenKind.StringLiteral, startsExpression: true);
    public static readonly TokenType Template = Literal("template", TokenKind.Template);
    public static readonly TokenType InvalidTemplate = Literal("invalidTemplate", TokenKind.Template);

    // Punctuation token types.
    public static readonly TokenType BracketLeft = Punctuator("[", beforeExpression: true, startsExpression: true);
    public static readonly TokenType BracketRight = Punctuator("]");
    public static readonly TokenType BraceLeft = Punctuator("{", beforeExpression: true, startsExpression: true, updateContext: Tokenizer.UpdateContext_BraceLeft);
    public static readonly TokenType BraceRight = Punctuator("}", updateContext: Tokenizer.UpdateContext_ParenOrBraceRight);
    public static readonly TokenType ParenLeft = Punctuator("(", beforeExpression: true, startsExpression: true, updateContext: Tokenizer.UpdateContext_ParenLeft);
    public static readonly TokenType ParenRight = Punctuator(")", updateContext: Tokenizer.UpdateContext_ParenOrBraceRight);
    public static readonly TokenType Comma = Punctuator(",", beforeExpression: true);
    public static readonly TokenType Semicolon = Punctuator(";", beforeExpression: true);
    public static readonly TokenType Colon = Punctuator(":", beforeExpression: true, updateContext: Tokenizer.UpdateContext_Colon);
    public static readonly TokenType Dot = Punctuator(".");
    public static readonly TokenType Question = Punctuator("?", beforeExpression: true);
    public static readonly TokenType QuestionDot = Punctuator("?.");
    public static readonly TokenType Arrow = Punctuator("=>", beforeExpression: true);
    public static readonly TokenType Ellipsis = Punctuator("...", beforeExpression: true);
    public static readonly TokenType BackQuote = Punctuator("`", startsExpression: true, updateContext: Tokenizer.UpdateContext_BackQuote);
    public static readonly TokenType DollarBraceLeft = Punctuator("${", beforeExpression: true, startsExpression: true, updateContext: Tokenizer.UpdateContext_DollarBraceLeft);

    // Operators. These carry several kinds of properties to help the
    // parser use them properly (the presence of these properties is
    // what categorizes them as operators).
    //
    // `binop`, when present, specifies that this operator is a binary
    // operator, and will refer to its precedence.
    //
    // `prefix` and `postfix` mark the operator as a prefix or postfix
    // unary operator.
    //
    // `isAssign` marks all of `=`, `+=`, `-=` etcetera, which act as
    // binary operators with a very low precedence, that should result
    // in AssignmentExpression nodes.
    public static readonly TokenType Eq = PunctuatorOperator("=", beforeExpression: true, isAssignment: true);
    public static readonly TokenType Assign = PunctuatorOperator("_=", beforeExpression: true, isAssignment: true);
    public static readonly TokenType IncDec = PunctuatorOperator("++/--", prefix: true, postfix: true, startsExpression: true, updateContext: Tokenizer.UpdateContext_IncDec);
    public static readonly TokenType PrefixOp = PunctuatorOperator("!/~", beforeExpression: true, prefix: true, startsExpression: true);
    public static readonly TokenType LogicalOr = PunctuatorOperator("||", beforeExpression: true, precedence: 1);
    public static readonly TokenType LogicalAnd = PunctuatorOperator("&&", beforeExpression: true, precedence: 2);
    public static readonly TokenType BitwiseOr = PunctuatorOperator("|", beforeExpression: true, precedence: 3);
    public static readonly TokenType BitwiseXor = PunctuatorOperator("^", beforeExpression: true, precedence: 4);
    public static readonly TokenType BitwiseAnd = PunctuatorOperator("&", beforeExpression: true, precedence: 5);
    public static readonly TokenType Equality = PunctuatorOperator("==/!=/===/!==", beforeExpression: true, precedence: 6);
    public static readonly TokenType Relational = PunctuatorOperator("</>/<=/>=", beforeExpression: true, precedence: 7);
    public static readonly TokenType BitShift = PunctuatorOperator("<</>>/>>>", beforeExpression: true, precedence: 8);
    public static readonly TokenType PlusMinus = PunctuatorOperator("+/-", beforeExpression: true, precedence: 9, prefix: true, startsExpression: true);
    public static readonly TokenType Modulo = PunctuatorOperator("%", beforeExpression: true, precedence: 10);
    public static readonly TokenType Star = PunctuatorOperator("*", beforeExpression: true, precedence: 10, updateContext: Tokenizer.UpdateContext_Star);
    public static readonly TokenType Slash = PunctuatorOperator("/", beforeExpression: true, precedence: 10);
    public static readonly TokenType StarStar = PunctuatorOperator("**", beforeExpression: true);
    public static readonly TokenType Coalesce = PunctuatorOperator("??", beforeExpression: true, precedence: 1);

    // Keyword token types.
    public static readonly TokenType Break = Keyword_("break", KeywordEnum.Break);
    public static readonly TokenType Case = Keyword_("case", KeywordEnum.Case, beforeExpression: true);
    public static readonly TokenType Catch = Keyword_("catch", KeywordEnum.Catch);
    public static readonly TokenType Continue = Keyword_("continue", KeywordEnum.Continue);
    public static readonly TokenType Debugger = Keyword_("debugger", KeywordEnum.Debugger);
    public static readonly TokenType Default = Keyword_("default", KeywordEnum.Default, beforeExpression: true);
    public static readonly TokenType Do = Keyword_("do", KeywordEnum.Do, isLoop: true, beforeExpression: true);
    public static readonly TokenType Else = Keyword_("else", KeywordEnum.Else, beforeExpression: true);
    public static readonly TokenType Finally = Keyword_("finally", KeywordEnum.Finally);
    public static readonly TokenType For = Keyword_("for", KeywordEnum.For, isLoop: true);
    public static readonly TokenType Function = Keyword_("function", KeywordEnum.Function, startsExpression: true, updateContext: Tokenizer.UpdateContext_FunctionOrClass);
    public static readonly TokenType If = Keyword_("if", KeywordEnum.If);
    public static readonly TokenType Return = Keyword_("return", KeywordEnum.Return, beforeExpression: true);
    public static readonly TokenType Switch = Keyword_("switch", KeywordEnum.Switch);
    public static readonly TokenType Throw = Keyword_("throw", KeywordEnum.Throw, beforeExpression: true);
    public static readonly TokenType Try = Keyword_("try", KeywordEnum.Try);
    public static readonly TokenType Var = Keyword_("var", KeywordEnum.Var);
    public static readonly TokenType Const = Keyword_("const", KeywordEnum.Const, ecmaVersion: EcmaVersion.ES6);
    public static readonly TokenType While = Keyword_("while", KeywordEnum.While, isLoop: true);
    public static readonly TokenType With = Keyword_("with", KeywordEnum.With);
    public static readonly TokenType New = Keyword_("new", KeywordEnum.New, beforeExpression: true, startsExpression: true);
    public static readonly TokenType This = Keyword_("this", KeywordEnum.This, startsExpression: true);
    public static readonly TokenType Super = Keyword_("super", KeywordEnum.Super, ecmaVersion: EcmaVersion.ES6, startsExpression: true);
    public static readonly TokenType Class = Keyword_("class", KeywordEnum.Class, ecmaVersion: EcmaVersion.ES6, startsExpression: true, updateContext: Tokenizer.UpdateContext_FunctionOrClass);
    public static readonly TokenType Extends = Keyword_("extends", KeywordEnum.Extends, ecmaVersion: EcmaVersion.ES6, beforeExpression: true);
    public static readonly TokenType Export = Keyword_("export", KeywordEnum.Export, ecmaVersion: EcmaVersion.ES6);
    public static readonly TokenType Import = Keyword_("import", KeywordEnum.Import, ecmaVersion: EcmaVersion.ES6, startsExpression: true);
    public static readonly TokenType Null = Keyword_("null", KeywordEnum.Null, TokenKind.NullLiteral, value: null, startsExpression: true);
    public static readonly TokenType True = Keyword_("true", KeywordEnum.True, TokenKind.BooleanLiteral, CachedValues.True, startsExpression: true);
    public static readonly TokenType False = Keyword_("false", KeywordEnum.False, TokenKind.BooleanLiteral, CachedValues.False, startsExpression: true);
    public static readonly TokenType In = KeywordOperator("in", KeywordEnum.In, beforeExpression: true, precedence: 7);
    public static readonly TokenType InstanceOf = KeywordOperator("instanceof", KeywordEnum.InstanceOf, beforeExpression: true, precedence: 7);
    public static readonly TokenType TypeOf = KeywordOperator("typeof", KeywordEnum.TypeOf, beforeExpression: true, prefix: true, startsExpression: true);
    public static readonly TokenType Void = KeywordOperator("void", KeywordEnum.Void, beforeExpression: true, prefix: true, startsExpression: true);
    public static readonly TokenType Delete = KeywordOperator("delete", KeywordEnum.Delete, beforeExpression: true, prefix: true, startsExpression: true);

    private static TokenType Identifier(string name, bool beforeExpression = false, bool startsExpression = false,
        Action<Tokenizer, TokenType>? updateContext = null)
    {
        return new TokenType(label: name, TokenKind.Identifier,
            beforeExpression: beforeExpression,
            startsExpression: startsExpression,
            updateContext: updateContext);
    }

    private static TokenType Literal(string label, TokenKind kind, bool beforeExpression = false, bool startsExpression = false)
    {
        return new TokenType(label, kind,
            beforeExpression: beforeExpression,
            startsExpression: startsExpression);
    }

    private static TokenType Punctuator(string label, bool beforeExpression = false, bool startsExpression = false,
        Action<Tokenizer, TokenType>? updateContext = null)
    {
        return new TokenType(label, TokenKind.Punctuator,
            beforeExpression: beforeExpression,
            startsExpression: startsExpression,
            updateContext: updateContext);
    }

    private static TokenType PunctuatorOperator(string label, bool beforeExpression = false, bool startsExpression = false,
        bool isAssignment = false, bool prefix = false, bool postfix = false, int precedence = 0,
        Action<Tokenizer, TokenType>? updateContext = null)
    {
        return new TokenType(label, TokenKind.Punctuator,
            beforeExpression: beforeExpression,
            startsExpression: startsExpression,
            isAssignment: isAssignment,
            prefix: prefix,
            postfix: postfix,
            precedence: precedence,
            updateContext: updateContext);
    }

    private static TokenType Keyword_(string label, KeywordEnum keyword, EcmaVersion ecmaVersion = EcmaVersion.ES3,
        bool beforeExpression = false, bool startsExpression = false, bool isLoop = false,
        Action<Tokenizer, TokenType>? updateContext = null)
    {
        return Keyword_(label, keyword, TokenKind.Keyword, value: label, ecmaVersion: ecmaVersion,
            beforeExpression: beforeExpression, startsExpression: startsExpression, isLoop: isLoop, updateContext: updateContext);
    }

    private static TokenType Keyword_(string label, KeywordEnum keyword, TokenKind kind, object? value, EcmaVersion ecmaVersion = EcmaVersion.ES3,
        bool beforeExpression = false, bool startsExpression = false, bool isLoop = false,
        Action<Tokenizer, TokenType>? updateContext = null)
    {
        return new TokenType(label, kind,
            keyword: keyword,
            ecmaVersion: ecmaVersion,
            value: value,
            beforeExpression: beforeExpression,
            startsExpression: startsExpression,
            isLoop: isLoop,
            updateContext: updateContext);
    }

    private static TokenType KeywordOperator(string label, KeywordEnum keyword, EcmaVersion ecmaVersion = EcmaVersion.ES3,
        bool beforeExpression = false, bool startsExpression = false, bool isLoop = false,
        bool isAssignment = false, bool prefix = false, bool postfix = false, int precedence = 0)
    {
        return new TokenType(label, TokenKind.Keyword,
            keyword: keyword,
            ecmaVersion: ecmaVersion,
            value: label,
            beforeExpression: beforeExpression,
            startsExpression: startsExpression,
            isLoop: isLoop,
            isAssignment: isAssignment,
            prefix: prefix,
            postfix: postfix,
            precedence: precedence);
    }

    public TokenType(
        string label,
        TokenKind kind,
        KeywordEnum? keyword = null,
        EcmaVersion ecmaVersion = EcmaVersion.Unknown,
        object? value = null,
        bool beforeExpression = false,
        bool startsExpression = false,
        bool isLoop = false,
        bool isAssignment = false,
        bool prefix = false,
        bool postfix = false,
        int precedence = -1,
        Action<Tokenizer, TokenType>? updateContext = null)
    {
        Label = label;
        Kind = kind;
        Keyword = keyword;
        EcmaVersion = ecmaVersion;
        Value = value;
        BeforeExpression = beforeExpression;
        StartsExpression = startsExpression;
        IsLoop = isLoop;
        IsAssignment = isAssignment;
        Prefix = prefix;
        Postfix = postfix;
        Precedence = precedence;
        UpdateContext = updateContext;
    }

    public readonly string Label;
    public readonly TokenKind Kind;
    public readonly KeywordEnum? Keyword;
    public readonly EcmaVersion EcmaVersion;
    public readonly object? Value;
    public readonly bool BeforeExpression;
    public readonly bool StartsExpression;
    public readonly bool IsLoop;
    public readonly bool IsAssignment;
    public readonly bool Prefix;
    public readonly bool Postfix;
    public readonly int Precedence;

    // NOTE: While switch dispatch for a few cases tends to be faster than dynamic dispatch,
    // in the case of this many branches, dynamic dispatch performs better.
    public readonly Action<Tokenizer, TokenType>? UpdateContext;

    // Map keyword names to token types.

    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    [StringMatcher(
        "break" /* => Break */,
        "case" /* => Case */,
        "catch" /* => Catch */,
        "class" /* => Class */,
        "const" /* => Const */,
        "continue" /* => Continue */,
        "debugger" /* => Debugger */,
        "default" /* => Default */,
        "delete" /* => Delete */,
        "do" /* => Do */,
        "else" /* => Else */,
        "export" /* => Export */,
        "extends" /* => Extends */,
        "false" /* => False */,
        "finally" /* => Finally */,
        "for" /* => For */,
        "function" /* => Function */,
        "if" /* => If */,
        "import" /* => Import */,
        "in" /* => In */,
        "instanceof" /* => InstanceOf */,
        "new" /* => New */,
        "null" /* => Null */,
        "return" /* => Return */,
        "super" /* => Super */,
        "switch" /* => Switch */,
        "this" /* => This */,
        "throw" /* => Throw */,
        "true" /* => True */,
        "try" /* => Try */,
        "typeof" /* => TypeOf */,
        "var" /* => Var */,
        "void" /* => Void */,
        "while" /* => While */,
        "with" /* => With */
    )]
    public static partial TokenType? GetKeywordBy(ReadOnlySpan<char> word);
}
