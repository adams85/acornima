#region Copyright (c) 2004 Atif Aziz. All rights reserved.

//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#endregion

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Acornima.Helpers;

namespace Acornima;

/// <summary>
/// Represents a writer that provides a fast, non-cached, forward-only
/// way of generating streams or files containing JSON Text according
/// to the grammar rules laid out in
/// <a href="http://www.ietf.org/rfc/rfc4627.txt">RFC 4627</a>.
/// </summary>

// Following implementation is derived from [Jayrock] & [ELMAH].
//
//   [Jayrock]: https://github.com/atifaziz/Jayrock
//   [ELMAH]: https://elmah.github.io/
internal sealed class JsonTextWriter : JsonWriter
{
    private enum StructureKind : byte { Array, Object }

    private readonly TextWriter _writer;
    private readonly string _indent;
    private ArrayList<StructureKind> _structures;
    private ArrayList<int> _counters;
    private string? _memberName;

    public JsonTextWriter(TextWriter writer) :
        this(writer, null)
    {
    }

    public JsonTextWriter(TextWriter writer, string? indent)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));

        if (!string.IsNullOrWhiteSpace(indent))
            throw new ArgumentException(ExtrasExceptionMessages.InvalidIndent, nameof(indent));
        _indent = indent ?? "";

        _counters = new ArrayList<int>();
        _structures = new ArrayList<StructureKind>();
    }

    public int Depth => _structures.Count;

    public override void StartObject()
    {
        StartStructured(StructureKind.Object);
    }

    public override void EndObject()
    {
        EndStructured();
    }

    public override void StartArray()
    {
        StartStructured(StructureKind.Array);
    }

    public override void EndArray()
    {
        EndStructured();
    }

    public override void Member(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        if (Depth == 0 || _structures.Peek() != StructureKind.Object)
            throw new InvalidOperationException(ExtrasExceptionMessages.JsonMemberOutsideObject);

        if (_memberName != null)
            throw new InvalidOperationException(string.Format(ExtrasExceptionMessages.MissingJsonMemberValue, _memberName));

        _memberName = name;
    }

    public override void String(string? str)
    {
        if (str == null)
        {
            Null();
        }
        else
        {
            Write(str, TokenKind.String);
        }
    }

    public override void Null()
    {
        Write("null", TokenKind.Scalar);
    }

    public override void Boolean(bool flag)
    {
        Write(flag ? "true" : "false", TokenKind.Scalar);
    }

    public void Number(int n)
    {
        Write(n.ToString(CultureInfo.InvariantCulture), TokenKind.Scalar);
    }

    public override void Number(long n)
    {
        Write(n.ToString(CultureInfo.InvariantCulture), TokenKind.Scalar);
    }

    public override void Number(double n)
    {
        if (double.IsNaN(n) || double.IsInfinity(n))
            throw new ArgumentOutOfRangeException(nameof(n), n, null);

        Write(n.ToString(CultureInfo.InvariantCulture), TokenKind.Scalar);
    }

    private bool Pretty => _indent != "";

    private void Eol()
    {
        if (!Pretty)
            return;

        _writer.WriteLine();
    }

    private void Indent(int? depth = null)
    {
        if (!Pretty)
            return;

        var n = depth ?? Depth;
        for (var i = 0; i < n; i++)
        {
            _writer.Write(_indent);
        }
    }

    private void StartStructured(StructureKind kind)
    {
        Write(kind == StructureKind.Array ? "[" : "{", TokenKind.Structure);
        _counters.Push(0);
        _structures.Push(kind);
    }

    private void EndStructured()
    {
        if (Depth == 0)
            throw new InvalidOperationException(ExtrasExceptionMessages.NoUnterminatedJsonStructure);

        if (_memberName != null)
            throw new InvalidOperationException(string.Format(ExtrasExceptionMessages.MissingJsonMemberValue, _memberName));

        if (_counters.Peek() > 0)
        {
            Eol();
            Indent(Depth - 1);
        }

        _writer.Write(_structures.Pop() == StructureKind.Array ? "]" : "}");
        _counters.Pop();
    }

    private enum TokenKind { Scalar, String, Structure }

    private void Write(string token, TokenKind kind)
    {
        Debug.Assert(kind == TokenKind.String || !string.IsNullOrEmpty(token));

        if (Depth == 0 && kind == TokenKind.Scalar)
            throw new InvalidOperationException(ExtrasExceptionMessages.JsonTextMustStartWithObjectOrArray);

        var writer = _writer;

        if (Depth > 0)
        {
            if (_structures.Peek() == StructureKind.Object && _memberName == null)
                throw new InvalidOperationException(ExtrasExceptionMessages.UndefinedJsonMemberName);

            if (_counters.Peek() > 0)
                writer.Write(',');

            Eol();
        }

        var name = _memberName;
        _memberName = null;

        if (name != null)
        {
            Indent();
            Enquote(name, writer);
            writer.Write(Pretty ? ": " : ":");
        }

        if (Depth > 0 && _structures.Peek() == StructureKind.Array)
            Indent();

        if (kind == TokenKind.String)
        {
            Enquote(token, writer);
        }
        else
        {
            writer.Write(token);
        }

        if (Depth > 0)
            _counters.PeekRef() += 1;
    }

    private static void Enquote(string s, TextWriter writer)
    {
        Debug.Assert(writer != null);

        var length = (s ?? string.Empty).Length;

        writer!.Write('"');

        for (var index = 0; index < length; index++)
        {
            Debug.Assert(s != null);
            var ch = s![index];

            switch (ch)
            {
                case '\\':
                case '"':
                    {
                        writer.Write('\\');
                        writer.Write(ch);
                        break;
                    }

                case '\b':
                    writer.Write("\\b");
                    break;
                case '\t':
                    writer.Write("\\t");
                    break;
                case '\n':
                    writer.Write("\\n");
                    break;
                case '\f':
                    writer.Write("\\f");
                    break;
                case '\r':
                    writer.Write("\\r");
                    break;

                default:
                    {
                        if (ch < ' ')
                        {
                            writer.Write("\\u");
                            writer.Write(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            writer.Write(ch);
                        }

                        break;
                    }
            }
        }

        writer.Write('"');
    }
}
