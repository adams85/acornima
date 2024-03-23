#if NET6_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Acornima.Helpers;
using Acornima.Tests.Helpers;
using Xunit;

namespace Acornima.Tests;

/// <summary>
/// Helper to generate some character lookup data.
/// </summary>
public class CharMaskGenerator
{
    [Fact]
    public void GenerateMasks()
    {
        var sb = new StringBuilder();

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine();
        sb.AppendLine("namespace Acornima;");
        sb.AppendLine();

        sb.AppendLine($"// Generated using {nameof(CharMaskGenerator)}.{nameof(GenerateMasks)}");
        sb.AppendLine("public partial class Tokenizer");
        sb.AppendLine("{");

        GenerateBmpMasks(sb);

        sb.AppendLine();

        GenerateAstralRanges(sb);

        sb.AppendLine("}");

        // because xunit is what is it, take it from debugger...
        var result = sb.ToString();
    }

    private static void GenerateBmpMasks(StringBuilder sb)
    {
        var masks = new byte[(char.MaxValue + 1) / 2];
        for (int c = char.MinValue; c <= char.MaxValue; c++)
        {
            var mask = Tokenizer.CharFlags.None;
            var commentStart = JavaScriptCharacter.IsCommentStart(c);
            if (commentStart || JavaScriptCharacter.IsLineTerminator(c))
            {
                mask |= Tokenizer.CharFlags.LineTerminator;
            }
            if (commentStart || JavaScriptCharacter.IsWhiteSpace(c))
            {
                mask |= Tokenizer.CharFlags.WhiteSpace;
            }
            if (JavaScriptCharacter.IsIdentifierStart(c))
            {
                mask |= Tokenizer.CharFlags.IdentifierStart;
            }
            if (JavaScriptCharacter.IsIdentifierPart(c))
            {
                mask |= Tokenizer.CharFlags.IdentifierPart;
            }

            masks[c >> 1] |= (byte)((byte)mask << ((c & 1) << 2));
        }

        sb.AppendLine("    private static ReadOnlySpan<byte> CharacterData");
        sb.AppendLine("    {");
        sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("        get => new byte[]");
        sb.AppendLine("        {");
        foreach (var chunk in masks.Chunk(32))
        {
            sb.Append("            ");
            foreach (var value in chunk)
            {
                sb.Append("0x").Append(value.ToString("X2"));
                sb.Append(", ");
            }

            sb.AppendLine();
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
    }

    private static void GenerateAstralRanges(StringBuilder sb)
    {
        var lengthLookup = new List<int> { 0 };

        foreach (var (match, name) in new[]
        {
            (new Predicate<int>(JavaScriptCharacter.IsLineTerminator), "LineTerminator"),
            (new Predicate<int>(JavaScriptCharacter.IsWhiteSpace), "WhiteSpace"),
            (JavaScriptCharacter.IsIdentifierStart, "IdentifierStart"),
            (JavaScriptCharacter.IsIdentifierPart, "IdentifierPart"),
        })
        {
            var ranges = new ArrayList<CodePointRange>();

            CodePointRange.AddRanges(ref ranges, match, start: 0x10000);

            if (ranges.Count == 0)
            {
                continue;
            }

            sb.AppendLine($"    private static ReadOnlySpan<int> {name}AstralRanges");
            sb.AppendLine("    {");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        get => new int[]");
            sb.AppendLine("        {");

            foreach (var chunk in ranges.Chunk(16))
            {
                sb.Append("            ");
                foreach (var range in chunk)
                {
                    sb.Append("0x").Append(EncodeRange(range, lengthLookup).ToString("X8", CultureInfo.InvariantCulture));
                    sb.Append(", ");
                }

                sb.AppendLine();
            }

            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("    private static ReadOnlySpan<int> RangeLengthLookup");
        sb.AppendLine("    {");
        sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("        get => new int[]");
        sb.AppendLine("        {");

        foreach (var chunk in lengthLookup.Chunk(40))
        {
            sb.Append("            ");
            foreach (var length in chunk)
            {
                sb.Append(length.ToString(CultureInfo.InvariantCulture));
                sb.Append(", ");
            }

            sb.AppendLine();
        }
        sb.AppendLine("        };");
        sb.AppendLine("    }");
    }

    private static int EncodeRange(CodePointRange range, List<int> lengthLookup)
    {
        var lengthMinusOne = range.End - range.Start;
        var lengthLookupIndex = lengthLookup.IndexOf(lengthMinusOne);
        if (lengthLookupIndex == -1)
        {
            lengthLookupIndex = lengthLookup.Count;
            Assert.True(lengthLookup.Count < byte.MaxValue);
            lengthLookup.Add(lengthMinusOne);
        }
        return range.Start << 8 | checked((byte)lengthLookupIndex);
    }

    [Fact]
    public void LookupWorks()
    {
        foreach (var (actual, expectedBmp, expectedAstral) in new[]
        {
            (new Predicate<int>(JavaScriptCharacter.IsLineTerminator), new Predicate<char>(Tokenizer.IsNewLine), new Predicate<int>(_ => false)),
            (JavaScriptCharacter.IsWhiteSpace, Tokenizer.IsWhiteSpace, _ => false),
            (JavaScriptCharacter.IsIdentifierStart, ch => Tokenizer.IsIdentifierStart(ch), cp => Tokenizer.IsIdentifierStart(cp)),
            (JavaScriptCharacter.IsIdentifierPart, ch => Tokenizer.IsIdentifierChar(ch), cp =>  Tokenizer.IsIdentifierChar(cp)),
        })
        {
            for (int ch = char.MinValue; ch <= char.MaxValue; ch++)
            {
                Assert.Equal(actual(ch), expectedBmp((char)ch));
            }

            for (var cp = char.MaxValue + 1; cp < UnicodeHelper.LastCodePoint; cp++)
            {
                Assert.Equal(actual(cp), expectedAstral(cp));
            }
        }
    }
}

#endif
