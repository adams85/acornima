using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

public readonly struct SourceLocation : IEquatable<SourceLocation>
{
    // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/locutil.js > `export class SourceLocation`

    public readonly Position Start;
    public readonly Position End;
    public readonly string? SourceFile;

    private static bool Validate(Position start, Position end, bool throwOnError)
    {
        if (start == default && end != default)
        {
            if (throwOnError)
            {
                throw new ArgumentOutOfRangeException(nameof(start), start, null);
            }
            return false;
        }

        if (end == default ? start != default : end < start)
        {
            if (throwOnError)
            {
                throw new ArgumentOutOfRangeException(nameof(end), end, null);
            }
            return false;
        }

        return true;
    }

    public static SourceLocation From(Position start, Position end, string? sourceFile = null)
    {
        Validate(start, end, throwOnError: true);
        return new SourceLocation(start, end, sourceFile);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal SourceLocation(Position start, Position end, string? sourceFile)
    {
        Debug.Assert(Validate(start, end, throwOnError: false));

        Start = start;
        End = end;
        SourceFile = sourceFile;
    }

    public SourceLocation WithPosition(Position start, Position end)
    {
        return From(start, end, SourceFile);
    }

    public SourceLocation WithSourceFile(string sourceFile)
    {
        return new SourceLocation(Start, End, sourceFile);
    }

    public override bool Equals(object? obj)
    {
        return obj is SourceLocation other && Equals(other);
    }

    public bool Equals(SourceLocation other)
    {
        return Start.Equals(other.Start)
               && End.Equals(other.End)
               && string.Equals(SourceFile, other.SourceFile);
    }

    bool IEquatable<SourceLocation>.Equals(SourceLocation other) => Equals(other);

    public static bool operator ==(SourceLocation left, SourceLocation right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SourceLocation left, SourceLocation right)
    {
        return !left.Equals(right);
    }

    public override int GetHashCode()
    {
        var hashCode = -1949435337;
        hashCode = hashCode * -1521134295 + Start.GetHashCode();
        hashCode = hashCode * -1521134295 + End.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(SourceFile!);
        return hashCode;
    }

    // TODO: make consistent with ParseError.ToString()?
    public override string ToString()
    {
        string interval;
        if (Start.Line != End.Line)
        {
            interval = Start.Column != End.Column
                ? string.Format(CultureInfo.InvariantCulture, "[{0},{1}..{2},{3})", Start.Line, Start.Column, End.Line, End.Column)
                : string.Format(CultureInfo.InvariantCulture, "[{0}..{1},{2})", Start.Line, End.Line, Start.Column);
        }
        else
        {
            interval = Start.Column != End.Column
                ? string.Format(CultureInfo.InvariantCulture, "[{0},{1}..{2})", Start.Line, Start.Column, End.Column)
                : string.Format(CultureInfo.InvariantCulture, "[{0},{1})", Start.Line, Start.Column);
        }

        return SourceFile is not null ? interval + ": " + SourceFile : interval;
    }

    private static bool TryParseCore(ReadOnlySpan<char> s, bool throwIfInvalid, out SourceLocation result)
    {
        int i;
        if (s[0] != '[' || (i = s.IndexOf(')')) < 0 || ++i < 5)
        {
            goto InvalidFormat;
        }

        var sourceFile = s.Slice(i);
        if (sourceFile.Length > 0 && sourceFile[0] != ':')
        {
            goto InvalidFormat;
        }

        s = s.Slice(1, i - 2);

        int startLine, startColumn, endLine, endColumn;
        if (!s.TryReadInt(out startLine))
        {
            goto InvalidFormat;
        }

        if (s.Length >= 5 && s[0] == '.' && s[1] == '.')
        {
            startColumn = -1;
            goto EndPart;
        }
        else if (s.Length < 2 || s[0] != ',')
        {
            goto InvalidFormat;
        }
        s = s.Slice(1);

        if (!s.TryReadInt(out startColumn))
        {
            goto InvalidFormat;
        }

        if (s.Length == 0)
        {
            endLine = startLine;
            endColumn = startColumn;
            goto SourcePart;
        }
        else if (s.Length < 3 || s[0] != '.' || s[1] != '.')
        {
            goto InvalidFormat;
        }

    EndPart:
        s = s.Slice(2);

        if (!s.TryReadInt(out var number))
        {
            goto InvalidFormat;
        }

        if (s.Length == 0)
        {
            endLine = startLine;
            endColumn = number;
            goto SourcePart;
        }
        else if (s.Length < 2 || s[0] != ',')
        {
            goto InvalidFormat;
        }
        endLine = number;
        s = s.Slice(1);

        if (!s.TryReadInt(out endColumn) || s.Length > 0)
        {
            goto InvalidFormat;
        }

        if (startColumn < 0)
        {
            startColumn = endColumn;
        }

    SourcePart:
        if (sourceFile.Length > 0)
        {
            sourceFile = sourceFile.Slice(1).Trim();
            if (sourceFile.Length == 0)
            {
                goto InvalidFormat;
            }
        }

        var start = new Position(startLine, startColumn);
        var end = new Position(endLine, endColumn);

        if (Validate(start, end, throwIfInvalid))
        {
            result = new SourceLocation(start, end, sourceFile.Length > 0 ? sourceFile.ToString() : null);
            return true;
        }

    InvalidFormat:
        result = default;
        return false;
    }

    public static bool TryParse(ReadOnlySpan<char> s, out SourceLocation result) => TryParseCore(s, throwIfInvalid: false, out result);

    public static bool TryParse(string s, out SourceLocation result) => TryParse(s.AsSpan(), out result);

    public static SourceLocation Parse(ReadOnlySpan<char> s)
    {
        return TryParseCore(s, throwIfInvalid: true, out var result) ? result : ThrowFormatException<SourceLocation>(ExceptionMessages.InvalidFormat);
    }

    public static SourceLocation Parse(string s) => Parse(s.AsSpan());

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out Position start, out Position end)
    {
        start = Start;
        end = End;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out Position start, out Position end, out string? sourceFile)
    {
        start = Start;
        end = End;
        sourceFile = SourceFile;
    }
}
