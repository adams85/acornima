using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System;

namespace Acornima;

using static Helpers.ExceptionHelper;

public readonly struct Position : IEquatable<Position>, IComparable<Position>
{
    // https://github.com/acornjs/acorn/blob/8.10.0/acorn/src/locutil.js > `export class Position`

    /// <summary>
    /// Line number (1-indexed).
    /// </summary>
    /// <remarks>
    /// A position where <see cref="Line"/> and <see cref="Column"/> are zero is an allowed value
    /// (since it's the <see langword="default"/> value of the struct) but considered an invalid position.
    /// </remarks>
    public readonly int Line;

    /// <summary>
    /// Column number (0-indexed).
    /// </summary>
    public readonly int Column;

    private static bool Validate(int line, int column, bool throwOnError)
    {
        if (line < 0 || line == 0 && column != 0)
        {
            if (throwOnError)
            {
                throw new ArgumentOutOfRangeException(nameof(line), line, null);
            }
            return false;
        }

        if (column < 0)
        {
            if (throwOnError)
            {
                throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
            return false;
        }

        return true;
    }

    public static Position From(int line, int column)
    {
        Validate(line, column, throwOnError: true);
        return new Position(line, column);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Position(int line, int column)
    {
        Debug.Assert(Validate(line, column, throwOnError: false));

        Line = line;
        Column = column;
    }

    public override bool Equals(object? obj)
    {
        return obj is Position other && Equals(other);
    }

    public bool Equals(Position other)
    {
        return Line == other.Line && Column == other.Column;
    }

    public int CompareTo(Position other)
    {
        return
            Line < other.Line ? -1 :
            Line > other.Line ? 1 :
            Column < other.Column ? -1 :
            Column > other.Column ? 1 :
            0;
    }

    public static bool operator ==(Position left, Position right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Position left, Position right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(Position left, Position right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Position left, Position right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(Position left, Position right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Position left, Position right)
    {
        return left.CompareTo(right) >= 0;
    }

    public override int GetHashCode()
    {
        var hashCode = -1456208474;
        hashCode = hashCode * -1521134295 + Line.GetHashCode();
        hashCode = hashCode * -1521134295 + Column.GetHashCode();
        return hashCode;
    }

    public override string ToString()
    {
        return Line.ToString(CultureInfo.InvariantCulture)
            + ","
            + Column.ToString(CultureInfo.InvariantCulture);
    }

    private static bool TryParseCore(ReadOnlySpan<char> s, bool throwIfInvalid, out Position result)
    {
        if (s.Length < 3)
        {
            goto InvalidFormat;
        }

        if (!s.TryReadInt(out var line))
        {
            goto InvalidFormat;
        }

        if (s.Length < 2 || s[0] != ',')
        {
            goto InvalidFormat;
        }
        s = s.Slice(1);

        if (!s.TryReadInt(out var column) || s.Length > 0)
        {
            goto InvalidFormat;
        }

        if (Validate(line, column, throwIfInvalid))
        {
            result = new Position(line, column);
            return true;
        }

    InvalidFormat:
        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> s, out Position result) => TryParseCore(s, throwIfInvalid: false, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string s, out Position result) => TryParse(s.AsSpan(), out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Parse(ReadOnlySpan<char> s)
    {
        return TryParseCore(s, throwIfInvalid: true, out var result) ? result : ThrowFormatException<Position>("Input string was not in a correct format.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Position Parse(string s) => Parse(s.AsSpan());

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out int line, out int column)
    {
        line = Line;
        column = Column;
    }
}
