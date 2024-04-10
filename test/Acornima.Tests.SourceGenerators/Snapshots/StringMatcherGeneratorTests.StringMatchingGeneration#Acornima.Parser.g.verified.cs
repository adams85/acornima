//HintName: Acornima.Parser.g.cs
#nullable enable

using System;

namespace Acornima;

partial class Parser
{
    internal static partial bool IsKeywordRelationalOperator(System.ReadOnlySpan<char> word)
    {
        switch (word.Length)
        {
            case 2:
            {
                return word[0] == 'i' && word[1] == 'n';
            }
            case 10:
            {
                return word.SequenceEqual("instanceof".AsSpan());
            }
            default:
                return false;
        }
    }
    
    private static partial Acornima.Parser.ReservedWordKind IsReservedWordES3NonStrict(System.ReadOnlySpan<char> word)
    {
        switch (word.Length)
        {
            case 3:
            {
                return word[0] == 'i' && word[1] == 'n' && word[2] == 't' ? ReservedWordKind.Optional : default;
            }
            case 4:
            {
                return word[0] switch
                {
                    'b' => word[1] == 'y' && word[2] == 't' && word[3] == 'e' ? ReservedWordKind.Optional : default,
                    'c' => word[1] == 'h' && word[2] == 'a' && word[3] == 'r' ? ReservedWordKind.Optional : default,
                    'e' => word[1] == 'n' && word[2] == 'u' && word[3] == 'm' ? ReservedWordKind.Optional : default,
                    'g' => word[1] == 'o' && word[2] == 't' && word[3] == 'o' ? ReservedWordKind.Optional : default,
                    'l' => word[1] == 'o' && word[2] == 'n' && word[3] == 'g' ? ReservedWordKind.Optional : default,
                    _ => default
                };
            }
            case 5:
            {
                return word[0] switch
                {
                    'c' => word[1] == 'l' && word[2] == 'a' && word[3] == 's' && word[4] == 's' ? ReservedWordKind.Optional : default,
                    'f' => word[1] switch
                    {
                        'i' => word[2] == 'n' && word[3] == 'a' && word[4] == 'l' ? ReservedWordKind.Optional : default,
                        'l' => word[2] == 'o' && word[3] == 'a' && word[4] == 't' ? ReservedWordKind.Optional : default,
                        _ => default
                    },
                    's' => word[1] switch
                    {
                        'h' => word[2] == 'o' && word[3] == 'r' && word[4] == 't' ? ReservedWordKind.Optional : default,
                        'u' => word[2] == 'p' && word[3] == 'e' && word[4] == 'r' ? ReservedWordKind.Optional : default,
                        _ => default
                    },
                    _ => default
                };
            }
            case 6:
            {
                return word[0] switch
                {
                    'd' => word[1] == 'o' && word[2] == 'u' && word[3] == 'b' && word[4] == 'l' && word[5] == 'e' ? ReservedWordKind.Optional : default,
                    'e' => word[1] == 'x' && word[2] == 'p' && word[3] == 'o' && word[4] == 'r' && word[5] == 't' ? ReservedWordKind.Optional : default,
                    'i' => word[1] == 'm' && word[2] == 'p' && word[3] == 'o' && word[4] == 'r' && word[5] == 't' ? ReservedWordKind.Optional : default,
                    'n' => word[1] == 'a' && word[2] == 't' && word[3] == 'i' && word[4] == 'v' && word[5] == 'e' ? ReservedWordKind.Optional : default,
                    'p' => word[1] == 'u' && word[2] == 'b' && word[3] == 'l' && word[4] == 'i' && word[5] == 'c' ? ReservedWordKind.Optional : default,
                    's' => word[1] == 't' && word[2] == 'a' && word[3] == 't' && word[4] == 'i' && word[5] == 'c' ? ReservedWordKind.Optional : default,
                    't' => word[1] == 'h' && word[2] == 'r' && word[3] == 'o' && word[4] == 'w' && word[5] == 's' ? ReservedWordKind.Optional : default,
                    _ => default
                };
            }
            case 7:
            {
                return word[1] switch
                {
                    'o' => word[0] == 'b' && word[2] == 'o' && word[3] == 'l' && word[4] == 'e' && word[5] == 'a' && word[6] == 'n' ? ReservedWordKind.Optional : default,
                    'x' => word[0] == 'e' && word[2] == 't' && word[3] == 'e' && word[4] == 'n' && word[5] == 'd' && word[6] == 's' ? ReservedWordKind.Optional : default,
                    'a' => word[0] == 'p' && word[2] == 'c' && word[3] == 'k' && word[4] == 'a' && word[5] == 'g' && word[6] == 'e' ? ReservedWordKind.Optional : default,
                    'r' => word[0] == 'p' && word[2] == 'i' && word[3] == 'v' && word[4] == 'a' && word[5] == 't' && word[6] == 'e' ? ReservedWordKind.Optional : default,
                    _ => default
                };
            }
            case 8:
            {
                return word[0] switch
                {
                    'a' => word[1] == 'b' && word[2] == 's' && word[3] == 't' && word[4] == 'r' && word[5] == 'a' && word[6] == 'c' && word[7] == 't' ? ReservedWordKind.Optional : default,
                    'v' => word[1] == 'o' && word[2] == 'l' && word[3] == 'a' && word[4] == 't' && word[5] == 'i' && word[6] == 'l' && word[7] == 'e' ? ReservedWordKind.Optional : default,
                    _ => default
                };
            }
            case 9:
            {
                return word[0] switch
                {
                    'i' => word.SequenceEqual("interface".AsSpan()) ? ReservedWordKind.Optional : default,
                    'p' => word.SequenceEqual("protected".AsSpan()) ? ReservedWordKind.Optional : default,
                    't' => word.SequenceEqual("transient".AsSpan()) ? ReservedWordKind.Optional : default,
                    _ => default
                };
            }
            case 10:
            {
                return word.SequenceEqual("implements".AsSpan()) ? ReservedWordKind.Optional : default;
            }
            case 12:
            {
                return word.SequenceEqual("synchronized".AsSpan()) ? ReservedWordKind.Optional : default;
            }
            default:
                return default;
        }
    }
    
    private static partial Acornima.Parser.ReservedWordKind IsReservedWordES5NonStrict(System.ReadOnlySpan<char> word)
    {
        switch (word.Length)
        {
            case 4:
            {
                return word[0] == 'e' && word[1] == 'n' && word[2] == 'u' && word[3] == 'm' ? ReservedWordKind.Optional : default;
            }
            case 5:
            {
                return word[1] switch
                {
                    'l' => word[0] == 'c' && word[2] == 'a' && word[3] == 's' && word[4] == 's' ? ReservedWordKind.Optional : default,
                    'o' => word[0] == 'c' && word[2] == 'n' && word[3] == 's' && word[4] == 't' ? ReservedWordKind.Optional : default,
                    'u' => word[0] == 's' && word[2] == 'p' && word[3] == 'e' && word[4] == 'r' ? ReservedWordKind.Optional : default,
                    _ => default
                };
            }
            case 6:
            {
                return word[0] switch
                {
                    'e' => word[1] == 'x' && word[2] == 'p' && word[3] == 'o' && word[4] == 'r' && word[5] == 't' ? ReservedWordKind.Optional : default,
                    'i' => word[1] == 'm' && word[2] == 'p' && word[3] == 'o' && word[4] == 'r' && word[5] == 't' ? ReservedWordKind.Optional : default,
                    _ => default
                };
            }
            case 7:
            {
                return word[0] == 'e' && word[1] == 'x' && word[2] == 't' && word[3] == 'e' && word[4] == 'n' && word[5] == 'd' && word[6] == 's' ? ReservedWordKind.Optional : default;
            }
            default:
                return default;
        }
    }
    
    private static partial Acornima.Parser.ReservedWordKind IsReservedWordES6NonStrict(System.ReadOnlySpan<char> word)
    {
        switch (word.Length)
        {
            case 4:
            {
                return word[0] == 'e' && word[1] == 'n' && word[2] == 'u' && word[3] == 'm' ? ReservedWordKind.Optional : default;
            }
            case 5:
            {
                return word[0] == 'a' && word[1] == 'w' && word[2] == 'a' && word[3] == 'i' && word[4] == 't' ? ReservedWordKind.OptionalModule : default;
            }
            default:
                return default;
        }
    }
    
    private static partial Acornima.Parser.ReservedWordKind IsReservedWordES5Strict(System.ReadOnlySpan<char> word)
    {
        switch (word.Length)
        {
            case 3:
            {
                return word[0] == 'l' && word[1] == 'e' && word[2] == 't' ? ReservedWordKind.Strict : default;
            }
            case 4:
            {
                return word[1] switch
                {
                    'n' => word[0] == 'e' && word[2] == 'u' && word[3] == 'm' ? ReservedWordKind.Optional : default,
                    'v' => word[0] == 'e' && word[2] == 'a' && word[3] == 'l' ? ReservedWordKind.StrictBind : default,
                    _ => default
                };
            }
            case 5:
            {
                return word[1] switch
                {
                    'l' => word[0] == 'c' && word[2] == 'a' && word[3] == 's' && word[4] == 's' ? ReservedWordKind.Optional : default,
                    'o' => word[0] == 'c' && word[2] == 'n' && word[3] == 's' && word[4] == 't' ? ReservedWordKind.Optional : default,
                    'u' => word[0] == 's' && word[2] == 'p' && word[3] == 'e' && word[4] == 'r' ? ReservedWordKind.Optional : default,
                    'i' => word[0] == 'y' && word[2] == 'e' && word[3] == 'l' && word[4] == 'd' ? ReservedWordKind.Strict : default,
                    _ => default
                };
            }
            case 6:
            {
                return word[0] switch
                {
                    'e' => word[1] == 'x' && word[2] == 'p' && word[3] == 'o' && word[4] == 'r' && word[5] == 't' ? ReservedWordKind.Optional : default,
                    'i' => word[1] == 'm' && word[2] == 'p' && word[3] == 'o' && word[4] == 'r' && word[5] == 't' ? ReservedWordKind.Optional : default,
                    'p' => word[1] == 'u' && word[2] == 'b' && word[3] == 'l' && word[4] == 'i' && word[5] == 'c' ? ReservedWordKind.Strict : default,
                    's' => word[1] == 't' && word[2] == 'a' && word[3] == 't' && word[4] == 'i' && word[5] == 'c' ? ReservedWordKind.Strict : default,
                    _ => default
                };
            }
            case 7:
            {
                return word[1] switch
                {
                    'x' => word[0] == 'e' && word[2] == 't' && word[3] == 'e' && word[4] == 'n' && word[5] == 'd' && word[6] == 's' ? ReservedWordKind.Optional : default,
                    'a' => word[0] == 'p' && word[2] == 'c' && word[3] == 'k' && word[4] == 'a' && word[5] == 'g' && word[6] == 'e' ? ReservedWordKind.Strict : default,
                    'r' => word[0] == 'p' && word[2] == 'i' && word[3] == 'v' && word[4] == 'a' && word[5] == 't' && word[6] == 'e' ? ReservedWordKind.Strict : default,
                    _ => default
                };
            }
            case 9:
            {
                return word[0] switch
                {
                    'a' => word.SequenceEqual("arguments".AsSpan()) ? ReservedWordKind.StrictBind : default,
                    'i' => word.SequenceEqual("interface".AsSpan()) ? ReservedWordKind.Strict : default,
                    'p' => word.SequenceEqual("protected".AsSpan()) ? ReservedWordKind.Strict : default,
                    _ => default
                };
            }
            case 10:
            {
                return word.SequenceEqual("implements".AsSpan()) ? ReservedWordKind.Strict : default;
            }
            default:
                return default;
        }
    }
    
    internal static partial Acornima.Parser.ReservedWordKind IsReservedWordES6Strict(System.ReadOnlySpan<char> word)
    {
        switch (word.Length)
        {
            case 3:
            {
                return word[0] == 'l' && word[1] == 'e' && word[2] == 't' ? ReservedWordKind.Strict : default;
            }
            case 4:
            {
                return word[1] switch
                {
                    'n' => word[0] == 'e' && word[2] == 'u' && word[3] == 'm' ? ReservedWordKind.Optional : default,
                    'v' => word[0] == 'e' && word[2] == 'a' && word[3] == 'l' ? ReservedWordKind.StrictBind : default,
                    _ => default
                };
            }
            case 5:
            {
                return word[0] switch
                {
                    'a' => word[1] == 'w' && word[2] == 'a' && word[3] == 'i' && word[4] == 't' ? ReservedWordKind.OptionalModule : default,
                    'y' => word[1] == 'i' && word[2] == 'e' && word[3] == 'l' && word[4] == 'd' ? ReservedWordKind.Strict : default,
                    _ => default
                };
            }
            case 6:
            {
                return word[0] switch
                {
                    'p' => word[1] == 'u' && word[2] == 'b' && word[3] == 'l' && word[4] == 'i' && word[5] == 'c' ? ReservedWordKind.Strict : default,
                    's' => word[1] == 't' && word[2] == 'a' && word[3] == 't' && word[4] == 'i' && word[5] == 'c' ? ReservedWordKind.Strict : default,
                    _ => default
                };
            }
            case 7:
            {
                return word[1] switch
                {
                    'a' => word[0] == 'p' && word[2] == 'c' && word[3] == 'k' && word[4] == 'a' && word[5] == 'g' && word[6] == 'e' ? ReservedWordKind.Strict : default,
                    'r' => word[0] == 'p' && word[2] == 'i' && word[3] == 'v' && word[4] == 'a' && word[5] == 't' && word[6] == 'e' ? ReservedWordKind.Strict : default,
                    _ => default
                };
            }
            case 9:
            {
                return word[0] switch
                {
                    'a' => word.SequenceEqual("arguments".AsSpan()) ? ReservedWordKind.StrictBind : default,
                    'i' => word.SequenceEqual("interface".AsSpan()) ? ReservedWordKind.Strict : default,
                    'p' => word.SequenceEqual("protected".AsSpan()) ? ReservedWordKind.Strict : default,
                    _ => default
                };
            }
            case 10:
            {
                return word.SequenceEqual("implements".AsSpan()) ? ReservedWordKind.Strict : default;
            }
            default:
                return default;
        }
    }
}
