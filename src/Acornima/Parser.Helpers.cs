using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

public partial class Parser
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Next(bool ignoreEscapeSequenceInKeyword = false, bool requireValidEscapeSequenceInTemplate = true)
    {
        _tokenizer.Next(new TokenizerContext(_strict, ignoreEscapeSequenceInKeyword, requireValidEscapeSequenceInTemplate));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Marker StartNode()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/node.js > `pp.startNode = function`

        return new Marker(_tokenizer._start, _tokenizer._startLocation);
    }

    private T FinishNodeAt<T>(in Marker startMarker, in Marker endMarker, T node) where T : Node
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/node.js > `function finishNodeAt`, `pp.finishNodeAt = function`

        node._range = new Range(startMarker.Index, endMarker.Index);
        node._location = new SourceLocation(startMarker.Position, endMarker.Position, _tokenizer._sourceFile);
        _options._onNode?.Invoke(node);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T FinishNode<T>(in Marker startMarker, T node) where T : Node
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/node.js > `pp.finishNode = function`

        return FinishNodeAt(startMarker, new Marker(_tokenizer._lastTokenEnd, _tokenizer._lastTokenEndLocation), node);
    }

    private T ReinterpretNode<T>(Node originalNode, T node) where T : Node
    {
        node._range = originalNode._range;
        node._location = originalNode._location;
        _options._onNode?.Invoke(node);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnterRecursion()
    {
        _recursionDepth++;
        StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T ExitRecursion<T>(T node)
    {
        _recursionDepth--;
        return node;
    }

    // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/identifier.js

    // TODO: add tests to verify identical behavior to acornjs

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsKeyword(ReadOnlySpan<char> word, EcmaVersion ecmaVersion)
    {
        return TokenType.GetKeywordBy(word) is { } tokenType && tokenType.EcmaVersion <= ecmaVersion;
    }

    [StringMatcher("in", "instanceof")]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    internal static partial bool IsKeywordRelationalOperator(ReadOnlySpan<char> word);

    // Don't alter the enum values, the reserved word detection logic relies on them heavily.
    internal enum ReservedWordKind : sbyte
    {
        None = 0,
        OptionalModule = 1 << 0,
        Optional = 1 << 1,
        Strict = 1 << 2,
        StrictBind = unchecked((sbyte)(1 << 7 | Strict)) // same as Strict but with the sign bit set
    }

    #region Reserved words (sloppy mode)

    [StringMatcher(
        "abstract" /* => ReservedWordKind.Optional */,
        "boolean" /* => ReservedWordKind.Optional */,
        "byte" /* => ReservedWordKind.Optional */,
        "char" /* => ReservedWordKind.Optional */,
        "class" /* => ReservedWordKind.Optional */,
        "double" /* => ReservedWordKind.Optional */,
        "enum" /* => ReservedWordKind.Optional */,
        "export" /* => ReservedWordKind.Optional */,
        "extends" /* => ReservedWordKind.Optional */,
        "final" /* => ReservedWordKind.Optional */,
        "float" /* => ReservedWordKind.Optional */,
        "goto" /* => ReservedWordKind.Optional */,
        "implements" /* => ReservedWordKind.Optional */,
        "import" /* => ReservedWordKind.Optional */,
        "int" /* => ReservedWordKind.Optional */,
        "interface" /* => ReservedWordKind.Optional */,
        "long" /* => ReservedWordKind.Optional */,
        "native" /* => ReservedWordKind.Optional */,
        "package" /* => ReservedWordKind.Optional */,
        "private" /* => ReservedWordKind.Optional */,
        "protected" /* => ReservedWordKind.Optional */,
        "public" /* => ReservedWordKind.Optional */,
        "short" /* => ReservedWordKind.Optional */,
        "static" /* => ReservedWordKind.Optional */,
        "super" /* => ReservedWordKind.Optional */,
        "synchronized" /* => ReservedWordKind.Optional */,
        "throws" /* => ReservedWordKind.Optional */,
        "transient" /* => ReservedWordKind.Optional */,
        "volatile" /* => ReservedWordKind.Optional */
    )]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    private static partial ReservedWordKind IsReservedWordES3Sloppy(ReadOnlySpan<char> word);

    [StringMatcher(
        "class" /* => ReservedWordKind.Optional */,
        "enum" /* => ReservedWordKind.Optional */,
        "extends" /* => ReservedWordKind.Optional */,
        "super" /* => ReservedWordKind.Optional */,
        "const" /* => ReservedWordKind.Optional */,
        "export" /* => ReservedWordKind.Optional */,
        "import" /* => ReservedWordKind.Optional */
    )]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    private static partial ReservedWordKind IsReservedWordES5Sloppy(ReadOnlySpan<char> word);

    [StringMatcher(
        "await" /* => ReservedWordKind.OptionalModule */,
        "enum" /* => ReservedWordKind.Optional */
    )]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    private static partial ReservedWordKind IsReservedWordES6Sloppy(ReadOnlySpan<char> word);

    #endregion

    #region Reserved words (strict mode)

    [StringMatcher(
        "class" /* => ReservedWordKind.Optional */,
        "enum" /* => ReservedWordKind.Optional */,
        "extends" /* => ReservedWordKind.Optional */,
        "super" /* => ReservedWordKind.Optional */,
        "const" /* => ReservedWordKind.Optional */,
        "export" /* => ReservedWordKind.Optional */,
        "import" /* => ReservedWordKind.Optional */,
        "implements" /* => ReservedWordKind.Strict */,
        "interface" /* => ReservedWordKind.Strict */,
        "let" /* => ReservedWordKind.Strict */,
        "package" /* => ReservedWordKind.Strict */,
        "private" /* => ReservedWordKind.Strict */,
        "protected" /* => ReservedWordKind.Strict */,
        "public" /* => ReservedWordKind.Strict */,
        "static" /* => ReservedWordKind.Strict */,
        "yield" /* => ReservedWordKind.Strict */,
        "eval" /* => ReservedWordKind.StrictBind */,
        "arguments" /* => ReservedWordKind.StrictBind */
    )]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    private static partial ReservedWordKind IsReservedWordES5Strict(ReadOnlySpan<char> word);

    [StringMatcher(
        "await" /* => ReservedWordKind.OptionalModule */,
        "enum" /* => ReservedWordKind.Optional */,
        "implements" /* => ReservedWordKind.Strict */,
        "interface" /* => ReservedWordKind.Strict */,
        "let" /* => ReservedWordKind.Strict */,
        "package" /* => ReservedWordKind.Strict */,
        "private" /* => ReservedWordKind.Strict */,
        "protected" /* => ReservedWordKind.Strict */,
        "public" /* => ReservedWordKind.Strict */,
        "static" /* => ReservedWordKind.Strict */,
        "yield" /* => ReservedWordKind.Strict */,
        "eval" /* => ReservedWordKind.StrictBind */,
        "arguments" /* => ReservedWordKind.StrictBind */
    )]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    internal static partial ReservedWordKind IsReservedWordES6Strict(ReadOnlySpan<char> word);

    #endregion

    [DebuggerDisplay($"{nameof(Index)} = {{{nameof(Index)}}}, {nameof(Position)} = {{{nameof(Position)}}}")]
    private readonly struct Marker
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Marker(int index, Position position)
        {
            Index = index;
            Position = position;
        }

        public int Index { get; }
        public Position Position { get; }
    }

    // Used in checkLVal and declareName to determine the type of a binding
    private enum BindingType : byte
    {
        None = 0, // Not a binding
        Var = 1, // Var-style binding
        Lexical = 2, // Let- or const-style binding
        Function = 3, // Function declaration
        SimpleCatch = 4, // Simple (identifier pattern) catch binding
        Outside = 5 // Special case for function names as bound inside the function
    }

    [Flags]
    private enum StatementContext : byte
    {
        Default = 0,

        // This flag can be combined with the other ones to
        // indicate a labeled statement within another statement.
        Label = 1 << 0,

        Do = 1 << 1,
        For = 1 << 2,
        If = 1 << 3,
        With = 1 << 4,
        While = 1 << 5,
    }

    [Flags]
    private enum ExpressionContext : byte
    {
        Default = 0,
        ForInit = 1 << 1,
        AwaitForInit = ForInit | 1,
        ForNew = 1 << 2,
        Decorator = 1 << 3,
    }

    private struct DestructuringErrors
    {
        public DestructuringErrors()
        {
            ShorthandAssign = -1;
            TrailingComma = -1;
            ParenthesizedAssign = -1;
            ParenthesizedBind = -1;
            DoubleProto = -1;
        }

        public int ShorthandAssign;
        public int TrailingComma;
        public int ParenthesizedAssign;
        public int ParenthesizedBind;
        public int DoubleProto;
    }

    [Flags]
    private enum FunctionOrClassFlags : byte
    {
        None = 0,
        Statement = 1 << 0,
        HangingStatement = 1 << 1,
        NullableId = 1 << 2,
    }
}
