using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Acornima.SourceGenerators.Helpers;

internal sealed class SourceBuilder
{
    private readonly StringBuilder _sb;
    private readonly string? _indent;
    private int _indentLevel;

    public SourceBuilder() : this("    ") { }

    public SourceBuilder(string indent)
    {
        _sb = new StringBuilder();
        _indent = indent;
    }

    public SourceBuilder Reset()
    {
        _sb.Clear();
        if (_sb.Capacity > 16384)
        {
            _sb.Capacity = 16384;
        }
        _indentLevel = 0;
        return this;
    }

    public SourceBuilder IncreaseIndent()
    {
        _indentLevel++;
        return this;
    }

    public SourceBuilder DecreaseIndent()
    {
        Debug.Assert(_indentLevel > 0, "Indentation got invalid.");
        _indentLevel--;
        return this;
    }

    private SourceBuilder AppendIndentIfNeeded()
    {
        if (_sb.Length == 0 || _sb[_sb.Length - 1] is '\r' or '\n')
        {
            _sb.Insert(_sb.Length, _indent, count: _indentLevel);
        }
        return this;
    }

    public SourceBuilder AppendLine()
    {
        AppendIndentIfNeeded()._sb.AppendLine();
        return this;
    }

    public SourceBuilder Append(string? value)
    {
        AppendIndentIfNeeded()._sb.Append(value);
        return this;
    }

    public SourceBuilder Append(string value, int startIndex, int count)
    {
        AppendIndentIfNeeded()._sb.Append(value, startIndex, count);
        return this;
    }

    public SourceBuilder AppendLine(string? value)
    {
        Append(value)._sb.AppendLine();
        return this;
    }

    public SourceBuilder Append([InterpolatedStringHandlerArgument("")] ref AppendInterpolatedStringHandler value)
    {
        return this;
    }

    public SourceBuilder AppendLine([InterpolatedStringHandlerArgument("")] ref AppendInterpolatedStringHandler value)
    {
        _sb.AppendLine();
        return this;
    }

    public SourceBuilder AppendLiteral(char c)
    {
        return Append(SymbolDisplay.FormatLiteral(c, quote: true));
    }

    public SourceBuilder AppendLiteral(string s)
    {
        return Append(SymbolDisplay.FormatLiteral(s, quote: true));
    }

    public SourceBuilder AppendTypeName(CSharpTypeName typeName, Predicate<CSharpTypeName>? includeNamespace = null)
    {
        AppendIndentIfNeeded();
        typeName.AppendTo(_sb, includeNamespace ?? (static _ => true));
        return this;
    }

    public SourceBuilder AppendTypeBareName(CSharpTypeBareName typeBareName, Predicate<CSharpTypeName>? includeNamespace = null)
    {
        AppendIndentIfNeeded();
        typeBareName.AppendTo(_sb, includeNamespace ?? (static _ => true));
        return this;
    }

    public override string ToString()
    {
        return _sb.ToString();
    }

    // Based on: https://github.com/dotnet/runtime/blob/v6.0.24/src/libraries/System.Private.CoreLib/src/System/Text/StringBuilder.cs
    [EditorBrowsable(EditorBrowsableState.Never)]
    [InterpolatedStringHandler]
    public readonly struct AppendInterpolatedStringHandler
    {
        private readonly StringBuilder _sb;

        public AppendInterpolatedStringHandler(int literalLength, int formattedCount, SourceBuilder sourceBuilder)
        {
            sourceBuilder.AppendIndentIfNeeded();
            _sb = sourceBuilder._sb;
        }

        public void AppendLiteral(string value) => _sb.Append(value);

        public void AppendFormatted(char value) => _sb.Append(value);

        public void AppendFormatted(string? value) => _sb.Append(value);

        public void AppendFormatted<T>(T value)
        {
            if (value is IFormattable)
            {
                _sb.Append(((IFormattable)value).ToString(format: null, CultureInfo.InvariantCulture));
            }
            else if (value is not null)
            {
                _sb.Append(value.ToString());
            }
        }

        public void AppendFormatted<T>(T value, string? format)
        {
            if (value is IFormattable)
            {
                _sb.Append(((IFormattable)value).ToString(format, CultureInfo.InvariantCulture));
            }
            else if (value is not null)
            {
                _sb.Append(value.ToString());
            }
        }

        public void AppendFormatted<T>(T value, int alignment) => AppendFormatted(value, alignment, format: null);

        public void AppendFormatted<T>(T value, int alignment, string? format)
        {
            if (alignment == 0)
            {
                AppendFormatted(value, format);
            }
            else
            {
                var start = _sb.Length;
                AppendFormatted(value, format);
                var paddingRequired = _sb.Length - start;
                if (alignment > 0)
                {
                    paddingRequired = alignment - (_sb.Length - start);
                    if (paddingRequired > 0)
                    {
                        _sb.Insert(start, " ", paddingRequired);
                    }
                }
                else
                {
                    paddingRequired = -alignment - paddingRequired;
                    if (paddingRequired > 0)
                    {
                        _sb.Insert(_sb.Length, " ", paddingRequired);
                    }
                }
            }

        }

        public void AppendFormatted(string? value, int alignment = 0, string? format = null) => AppendFormatted<string?>(value, alignment, format);

        public void AppendFormatted(object? value, int alignment = 0, string? format = null) => AppendFormatted<object?>(value, alignment, format);
    }
}
