//HintName: Acornima.TokenType.g.cs
#nullable enable

using System;

namespace Acornima;

partial class TokenType
{
    public static partial Acornima.TokenType? GetKeywordBy(System.ReadOnlySpan<char> word)
    {
        switch (word.Length)
        {
            case 2:
            {
                return word[1] switch
                {
                    'o' => word[0] == 'd' ? Do : default,
                    'f' => word[0] == 'i' ? If : default,
                    'n' => word[0] == 'i' ? In : default,
                    _ => default
                };
            }
            case 3:
            {
                return word[0] switch
                {
                    'f' => word[1] == 'o' && word[2] == 'r' ? For : default,
                    'n' => word[1] == 'e' && word[2] == 'w' ? New : default,
                    't' => word[1] == 'r' && word[2] == 'y' ? Try : default,
                    'v' => word[1] == 'a' && word[2] == 'r' ? Var : default,
                    _ => default
                };
            }
            case 4:
            {
                return word[1] switch
                {
                    'a' => word[0] == 'c' && word[2] == 's' && word[3] == 'e' ? Case : default,
                    'l' => word[0] == 'e' && word[2] == 's' && word[3] == 'e' ? Else : default,
                    'u' => word[0] == 'n' && word[2] == 'l' && word[3] == 'l' ? Null : default,
                    'h' => word[0] == 't' && word[2] == 'i' && word[3] == 's' ? This : default,
                    'r' => word[0] == 't' && word[2] == 'u' && word[3] == 'e' ? True : default,
                    'o' => word[0] == 'v' && word[2] == 'i' && word[3] == 'd' ? Void : default,
                    'i' => word[0] == 'w' && word[2] == 't' && word[3] == 'h' ? With : default,
                    _ => default
                };
            }
            case 5:
            {
                return word[2] switch
                {
                    'e' => word[0] == 'b' && word[1] == 'r' && word[3] == 'a' && word[4] == 'k' ? Break : default,
                    't' => word[0] == 'c' && word[1] == 'a' && word[3] == 'c' && word[4] == 'h' ? Catch : default,
                    'a' => word[0] == 'c' && word[1] == 'l' && word[3] == 's' && word[4] == 's' ? Class : default,
                    'n' => word[0] == 'c' && word[1] == 'o' && word[3] == 's' && word[4] == 't' ? Const : default,
                    'l' => word[0] == 'f' && word[1] == 'a' && word[3] == 's' && word[4] == 'e' ? False : default,
                    'p' => word[0] == 's' && word[1] == 'u' && word[3] == 'e' && word[4] == 'r' ? Super : default,
                    'r' => word[0] == 't' && word[1] == 'h' && word[3] == 'o' && word[4] == 'w' ? Throw : default,
                    'i' => word[0] == 'w' && word[1] == 'h' && word[3] == 'l' && word[4] == 'e' ? While : default,
                    _ => default
                };
            }
            case 6:
            {
                return word[0] switch
                {
                    'd' => word[1] == 'e' && word[2] == 'l' && word[3] == 'e' && word[4] == 't' && word[5] == 'e' ? Delete : default,
                    'e' => word[1] == 'x' && word[2] == 'p' && word[3] == 'o' && word[4] == 'r' && word[5] == 't' ? Export : default,
                    'i' => word[1] == 'm' && word[2] == 'p' && word[3] == 'o' && word[4] == 'r' && word[5] == 't' ? Import : default,
                    'r' => word[1] == 'e' && word[2] == 't' && word[3] == 'u' && word[4] == 'r' && word[5] == 'n' ? Return : default,
                    's' => word[1] == 'w' && word[2] == 'i' && word[3] == 't' && word[4] == 'c' && word[5] == 'h' ? Switch : default,
                    't' => word[1] == 'y' && word[2] == 'p' && word[3] == 'e' && word[4] == 'o' && word[5] == 'f' ? TypeOf : default,
                    _ => default
                };
            }
            case 7:
            {
                return word[0] switch
                {
                    'd' => word[1] == 'e' && word[2] == 'f' && word[3] == 'a' && word[4] == 'u' && word[5] == 'l' && word[6] == 't' ? Default : default,
                    'e' => word[1] == 'x' && word[2] == 't' && word[3] == 'e' && word[4] == 'n' && word[5] == 'd' && word[6] == 's' ? Extends : default,
                    'f' => word[1] == 'i' && word[2] == 'n' && word[3] == 'a' && word[4] == 'l' && word[5] == 'l' && word[6] == 'y' ? Finally : default,
                    _ => default
                };
            }
            case 8:
            {
                return word[0] switch
                {
                    'c' => word[1] == 'o' && word[2] == 'n' && word[3] == 't' && word[4] == 'i' && word[5] == 'n' && word[6] == 'u' && word[7] == 'e' ? Continue : default,
                    'd' => word[1] == 'e' && word[2] == 'b' && word[3] == 'u' && word[4] == 'g' && word[5] == 'g' && word[6] == 'e' && word[7] == 'r' ? Debugger : default,
                    'f' => word[1] == 'u' && word[2] == 'n' && word[3] == 'c' && word[4] == 't' && word[5] == 'i' && word[6] == 'o' && word[7] == 'n' ? Function : default,
                    _ => default
                };
            }
            case 10:
            {
                return word.SequenceEqual("instanceof".AsSpan()) ? InstanceOf : default;
            }
            default:
                return default;
        }
    }
}
