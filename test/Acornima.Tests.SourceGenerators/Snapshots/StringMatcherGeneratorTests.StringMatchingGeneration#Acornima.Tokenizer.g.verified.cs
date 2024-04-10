//HintName: Acornima.Tokenizer.g.cs
#nullable enable

using System;

namespace Acornima;

partial class Tokenizer
{
    private static partial string? TryGetInternedString(System.ReadOnlySpan<char> source)
    {
        switch (source.Length)
        {
            case 2:
            {
                return source[0] switch
                {
                    'a' => source[1] == 's' ? "as" : null,
                    'd' => source[1] == 'o' ? "do" : null,
                    'i' => source[1] switch
                    {
                        'f' => "if",
                        'n' => "in",
                        _ => null
                    },
                    'o' => source[1] == 'f' ? "of" : null,
                    _ => null
                };
            }
            case 3:
            {
                return source[0] switch
                {
                    'f' => source[1] == 'o' && source[2] == 'r' ? "for" : null,
                    'g' => source[1] == 'e' && source[2] == 't' ? "get" : null,
                    'k' => source[1] == 'e' && source[2] == 'y' ? "key" : null,
                    'l' => source[1] == 'e' && source[2] == 't' ? "let" : null,
                    'n' => source[1] == 'e' && source[2] == 'w' ? "new" : null,
                    'o' => source[1] == 'b' && source[2] == 'j' ? "obj" : null,
                    's' => source[1] == 'e' && source[2] == 't' ? "set" : null,
                    't' => source[1] == 'r' && source[2] == 'y' ? "try" : null,
                    'v' => source[1] == 'a' && source[2] == 'r' ? "var" : null,
                    _ => null
                };
            }
            case 4:
            {
                return source[0] switch
                {
                    'M' => source[1] == 'a' && source[2] == 't' && source[3] == 'h' ? "Math" : null,
                    'a' => source[1] == 'r' && source[2] == 'g' && source[3] == 's' ? "args" : null,
                    'c' => source[1] == 'a' && source[2] == 's' && source[3] == 'e' ? "case" : null,
                    'd' => source[1] switch
                    {
                        'a' => source[2] == 't' && source[3] == 'a' ? "data" : null,
                        'o' => source[2] == 'n' && source[3] == 'e' ? "done" : null,
                        _ => null
                    },
                    'e' => source[1] switch
                    {
                        'l' => source[2] == 's' && source[3] == 'e' ? "else" : null,
                        'n' => source[2] == 'u' && source[3] == 'm' ? "enum" : null,
                        _ => null
                    },
                    'f' => source[1] == 'r' && source[2] == 'o' && source[3] == 'm' ? "from" : null,
                    'n' => source[1] switch
                    {
                        'a' => source[2] == 'm' && source[3] == 'e' ? "name" : null,
                        'u' => source[2] == 'l' && source[3] == 'l' ? "null" : null,
                        _ => null
                    },
                    's' => source[1] == 'e' && source[2] == 'l' && source[3] == 'f' ? "self" : null,
                    't' => source[1] switch
                    {
                        'h' => source[2] == 'i' && source[3] == 's' ? "this" : null,
                        'r' => source[2] == 'u' && source[3] == 'e' ? "true" : null,
                        _ => null
                    },
                    'v' => source[1] == 'o' && source[2] == 'i' && source[3] == 'd' ? "void" : null,
                    'w' => source[1] == 'i' && source[2] == 't' && source[3] == 'h' ? "with" : null,
                    _ => null
                };
            }
            case 5:
            {
                return source[0] switch
                {
                    'A' => source[1] == 'r' && source[2] == 'r' && source[3] == 'a' && source[4] == 'y' ? "Array" : null,
                    'a' => source[1] switch
                    {
                        's' => source[2] == 'y' && source[3] == 'n' && source[4] == 'c' ? "async" : null,
                        'w' => source[2] == 'a' && source[3] == 'i' && source[4] == 't' ? "await" : null,
                        _ => null
                    },
                    'b' => source[1] == 'r' && source[2] == 'e' && source[3] == 'a' && source[4] == 'k' ? "break" : null,
                    'c' => source[1] switch
                    {
                        'a' => source[2] == 't' && source[3] == 'c' && source[4] == 'h' ? "catch" : null,
                        'l' => source[2] == 'a' && source[3] == 's' && source[4] == 's' ? "class" : null,
                        'o' => source[2] == 'n' && source[3] == 's' && source[4] == 't' ? "const" : null,
                        _ => null
                    },
                    'f' => source[1] == 'a' && source[2] == 'l' && source[3] == 's' && source[4] == 'e' ? "false" : null,
                    's' => source[1] == 'u' && source[2] == 'p' && source[3] == 'e' && source[4] == 'r' ? "super" : null,
                    't' => source[1] == 'h' && source[2] == 'r' && source[3] == 'o' && source[4] == 'w' ? "throw" : null,
                    'v' => source[1] == 'a' && source[2] == 'l' && source[3] == 'u' && source[4] == 'e' ? "value" : null,
                    'w' => source[1] == 'h' && source[2] == 'i' && source[3] == 'l' && source[4] == 'e' ? "while" : null,
                    'y' => source[1] == 'i' && source[2] == 'e' && source[3] == 'l' && source[4] == 'd' ? "yield" : null,
                    _ => null
                };
            }
            case 6:
            {
                return source[0] switch
                {
                    'O' => source[1] == 'b' && source[2] == 'j' && source[3] == 'e' && source[4] == 'c' && source[5] == 't' ? "Object" : null,
                    'S' => source[1] == 'y' && source[2] == 'm' && source[3] == 'b' && source[4] == 'o' && source[5] == 'l' ? "Symbol" : null,
                    'd' => source[1] == 'e' && source[2] == 'l' && source[3] == 'e' && source[4] == 't' && source[5] == 'e' ? "delete" : null,
                    'e' => source[1] == 'x' && source[2] == 'p' && source[3] == 'o' && source[4] == 'r' && source[5] == 't' ? "export" : null,
                    'i' => source[1] == 'm' && source[2] == 'p' && source[3] == 'o' && source[4] == 'r' && source[5] == 't' ? "import" : null,
                    'l' => source[1] == 'e' && source[2] == 'n' && source[3] == 'g' && source[4] == 't' && source[5] == 'h' ? "length" : null,
                    'o' => source[1] == 'b' && source[2] == 'j' && source[3] == 'e' && source[4] == 'c' && source[5] == 't' ? "object" : null,
                    'r' => source[1] == 'e' && source[2] == 't' && source[3] == 'u' && source[4] == 'r' && source[5] == 'n' ? "return" : null,
                    's' => source[1] switch
                    {
                        't' => source[2] == 'a' && source[3] == 't' && source[4] == 'i' && source[5] == 'c' ? "static" : null,
                        'w' => source[2] == 'i' && source[3] == 't' && source[4] == 'c' && source[5] == 'h' ? "switch" : null,
                        _ => null
                    },
                    't' => source[1] == 'y' && source[2] == 'p' && source[3] == 'e' && source[4] == 'o' && source[5] == 'f' ? "typeof" : null,
                    _ => null
                };
            }
            case 7:
            {
                return source[0] switch
                {
                    'd' => source[1] == 'e' && source[2] == 'f' && source[3] == 'a' && source[4] == 'u' && source[5] == 'l' && source[6] == 't' ? "default" : null,
                    'e' => source[1] == 'x' && source[2] == 't' && source[3] == 'e' && source[4] == 'n' && source[5] == 'd' && source[6] == 's' ? "extends" : null,
                    'f' => source[1] == 'i' && source[2] == 'n' && source[3] == 'a' && source[4] == 'l' && source[5] == 'l' && source[6] == 'y' ? "finally" : null,
                    'o' => source[1] == 'p' && source[2] == 't' && source[3] == 'i' && source[4] == 'o' && source[5] == 'n' && source[6] == 's' ? "options" : null,
                    _ => null
                };
            }
            case 8:
            {
                return source[0] switch
                {
                    'c' => source[1] == 'o' && source[2] == 'n' && source[3] == 't' && source[4] == 'i' && source[5] == 'n' && source[6] == 'u' && source[7] == 'e' ? "continue" : null,
                    'd' => source[1] == 'e' && source[2] == 'b' && source[3] == 'u' && source[4] == 'g' && source[5] == 'g' && source[6] == 'e' && source[7] == 'r' ? "debugger" : null,
                    'f' => source[1] == 'u' && source[2] == 'n' && source[3] == 'c' && source[4] == 't' && source[5] == 'i' && source[6] == 'o' && source[7] == 'n' ? "function" : null,
                    _ => null
                };
            }
            case 9:
            {
                return source[0] switch
                {
                    'a' => source.SequenceEqual("arguments".AsSpan()) ? "arguments" : null,
                    'p' => source.SequenceEqual("prototype".AsSpan()) ? "prototype" : null,
                    'u' => source.SequenceEqual("undefined".AsSpan()) ? "undefined" : null,
                    _ => null
                };
            }
            case 10:
            {
                return source[0] switch
                {
                    'i' => source.SequenceEqual("instanceof".AsSpan()) ? "instanceof" : null,
                    'u' => source.SequenceEqual("use strict".AsSpan()) ? "use strict" : null,
                    _ => null
                };
            }
            case 11:
            {
                return source.SequenceEqual("constructor".AsSpan()) ? "constructor" : null;
            }
            case 12:
            {
                return source.SequenceEqual("\"use strict\"".AsSpan()) ? "\"use strict\"" : null;
            }
            default:
                return null;
        }
    }
}
