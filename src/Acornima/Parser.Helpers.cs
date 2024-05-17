using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static SyntaxErrorMessages;

public partial class Parser
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Next(bool ignoreEscapeSequenceInKeyword = false, bool requireValidEscapeSequenceInTemplate = true)
    {
        _tokenizer.Next(new TokenizerContext(_strict, ignoreEscapeSequenceInKeyword, requireValidEscapeSequenceInTemplate));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Marker StartNode()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/node.js > `pp.startNode = function`

        return new Marker(_tokenizer._start, _tokenizer._startLocation);
    }

    internal T FinishNodeAt<T>(in Marker startMarker, in Marker endMarker, T node) where T : Node
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/node.js > `function finishNodeAt`, `pp.finishNodeAt = function`

        node._range = new Range(startMarker.Index, endMarker.Index);
        node._location = new SourceLocation(startMarker.Position, endMarker.Position, _tokenizer._sourceFile);
        _options._onNode?.Invoke(node);
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal T FinishNode<T>(in Marker startMarker, T node) where T : Node
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
    internal void EnterRecursion()
    {
        _recursionDepth++;
        StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal T ExitRecursion<T>(T node) where T : Node
    {
        _recursionDepth--;
        return node;
    }

    // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/identifier.js

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsKeyword(ReadOnlySpan<char> word, EcmaVersion ecmaVersion, [NotNullWhen(true)] out TokenType? tokenType)
    {
        tokenType = TokenType.GetKeywordBy(word);
        return tokenType is not null && tokenType.EcmaVersion <= ecmaVersion;
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

    #region Reserved words (non-strict mode)

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
    private static partial ReservedWordKind IsReservedWordES3NonStrict(ReadOnlySpan<char> word);

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
    private static partial ReservedWordKind IsReservedWordES5NonStrict(ReadOnlySpan<char> word);

    [StringMatcher(
        "await" /* => ReservedWordKind.OptionalModule */,
        "enum" /* => ReservedWordKind.Optional */
    )]
    [MethodImpl((MethodImplOptions)512 /* AggressiveOptimization */)]
    private static partial ReservedWordKind IsReservedWordES6NonStrict(ReadOnlySpan<char> word);

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

    internal delegate bool IsReservedWordDelegate(ReadOnlySpan<char> word, bool strict);

    internal static void GetIsReservedWord(bool inModule, EcmaVersion ecmaVersion, AllowReservedOption allowReserved,
        out IsReservedWordDelegate isReservedWord, out IsReservedWordDelegate isReservedWordBind)
    {
        switch (ecmaVersion)
        {
            case EcmaVersion.ES3:
                if (!inModule)
                {
                    if (allowReserved != AllowReservedOption.Yes)
                    {
                        isReservedWord = static (word, strict) =>
                        {
                            Debug.Assert(!strict, "Invalid combination of options");
                            return IsReservedWordES3NonStrict(word) != ReservedWordKind.None;
                        };
                    }
                    else
                    {
                        isReservedWord = static (_, strict) =>
                        {
                            Debug.Assert(!strict, "Invalid combination of options");
                            return false;
                        };
                    }
                    isReservedWordBind = static (_, strict) =>
                    {
                        Debug.Assert(!strict, "Invalid combination of options");
                        return false;
                    };
                }
                else
                {
                    Debug.Assert(false, "Invalid combination of options");
                    isReservedWord = isReservedWordBind = default!;
                }
                break;

            case EcmaVersion.ES5:
                if (!inModule)
                {
                    if (allowReserved != AllowReservedOption.Yes)
                    {
                        isReservedWord = static (word, strict) => (strict ? IsReservedWordES5Strict(word) : IsReservedWordES5NonStrict(word)) >= ReservedWordKind.Optional;
                        isReservedWordBind = static (word, strict) => strict && (IsReservedWordES5Strict(word) & (ReservedWordKind.Optional | ReservedWordKind.Strict)) != 0;
                    }
                    else
                    {
                        isReservedWord = static (word, strict) => (strict ? IsReservedWordES5Strict(word) : IsReservedWordES5NonStrict(word)) == ReservedWordKind.Strict;
                        isReservedWordBind = static (word, strict) => strict && (IsReservedWordES5Strict(word) & ReservedWordKind.Strict) != 0;
                    }
                }
                else
                {
                    Debug.Assert(false, "Invalid combination of options");
                    isReservedWord = isReservedWordBind = default!;
                }
                break;

            case >= EcmaVersion.ES6:
                if (!inModule)
                {
                    if (allowReserved != AllowReservedOption.Yes)
                    {
                        isReservedWord = static (word, strict) => (strict ? IsReservedWordES6Strict(word) : IsReservedWordES6NonStrict(word)) >= ReservedWordKind.Optional;
                        isReservedWordBind = static (word, strict) => strict && (IsReservedWordES6Strict(word) & (ReservedWordKind.Optional | ReservedWordKind.Strict)) != 0;
                    }
                    else
                    {
                        isReservedWord = static (word, strict) => (strict ? IsReservedWordES6Strict(word) : IsReservedWordES6NonStrict(word)) == ReservedWordKind.Strict;
                        isReservedWordBind = static (word, strict) => strict && (IsReservedWordES6Strict(word) & ReservedWordKind.Strict) != 0;
                    }
                }
                else
                {
                    if (allowReserved != AllowReservedOption.Yes)
                    {
                        isReservedWord = static (word, strict) =>
                        {
                            Debug.Assert(strict, "Invalid combination of options");
                            return IsReservedWordES6Strict(word) > ReservedWordKind.None;
                        };
                        isReservedWordBind = static (word, strict) =>
                        {
                            Debug.Assert(strict, "Invalid combination of options");
                            return IsReservedWordES6Strict(word) != ReservedWordKind.None;
                        };
                    }
                    else
                    {
                        isReservedWord = static (word, strict) =>
                        {
                            Debug.Assert(strict, "Invalid combination of options");
                            return IsReservedWordES6Strict(word) == ReservedWordKind.Strict;
                        };
                        isReservedWordBind = static (word, strict) =>
                        {
                            Debug.Assert(strict, "Invalid combination of options");
                            return (IsReservedWordES6Strict(word) & ReservedWordKind.Strict) == ReservedWordKind.Strict;
                        };
                    }
                }
                break;

            default:
                throw new InvalidOperationException(string.Format(ExceptionMessages.UnsupportedEcmaVersion, ecmaVersion));
        }
    }

    private static ReservedWordKind GetReservedWordKind(ReadOnlySpan<char> word, bool strict, EcmaVersion ecmaVersion)
    {
        switch (ecmaVersion)
        {
            case EcmaVersion.ES3:
                if (strict)
                {
                    Debug.Assert(!strict, "Invalid combination of options");
                }
                return IsReservedWordES3NonStrict(word);

            case EcmaVersion.ES5:
                return strict ? IsReservedWordES5Strict(word) : IsReservedWordES5NonStrict(word);

            case >= EcmaVersion.ES6:
                return strict ? IsReservedWordES6Strict(word) : IsReservedWordES6NonStrict(word);

            default:
                throw new InvalidOperationException(string.Format(ExceptionMessages.UnsupportedEcmaVersion, ecmaVersion));
        }
    }

    private void HandleReservedWordError(Identifier id)
    {
        if ((GetReservedWordKind(id.Name.AsSpan(), _strict, _options.EcmaVersion) & ReservedWordKind.Strict) != 0)
        {
            Raise(id.Start, UnexpectedStrictReserved);
        }
        else
        {
            Raise(id.Start, UnexpectedReserved);
        }
    }

#if DEBUG
    [DebuggerDisplay($"{nameof(Index)} = {{{nameof(Index)}}}, {nameof(Position)} = {{{nameof(Position)}}}")]
#endif
    internal readonly struct Marker
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Marker(int index, Position position)
        {
            Index = index;
            Position = position;
        }

        public readonly int Index;
        public readonly Position Position;
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
    internal enum ExpressionContext : byte
    {
        Default = 0,
        ForInit = 1 << 1,
        AwaitForInit = ForInit | (1 << 0),
        ForNew = 1 << 2,
        Decorator = 1 << 3,
    }

    internal struct DestructuringErrors
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DestructuringErrors()
        {
            ShorthandAssign = TrailingComma = ParenthesizedAssign = ParenthesizedBind = DoubleProto = -1;
        }

        public int ShorthandAssign;
        public int TrailingComma; // we use the sign bit to indicate whether it's a position of a rest parameter or an element (see also CheckPatternErrors)
        public int ParenthesizedAssign;
        public int ParenthesizedBind;
        public int DoubleProto;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int GetTrailingComma()
        {
            return TrailingComma == -1 ? -1 : TrailingComma & 0x7FFF_FFFF;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTrailingComma(int index, bool isParam)
        {
            Debug.Assert(index >= 0);
            // NOTE: (int.MaxValue | (1 << 31)) == -1, which value is used to indicate no error. That is, we can't use it to represent an error position,
            // so let's resort to int.MaxValue in this extreme case as it's better to report a wrong error than not reporting the error at all.
            TrailingComma = !isParam || index == int.MaxValue ? index : index | (1 << 31);
        }
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
