using System.Text.RegularExpressions;

namespace Acornima.Tests.Acorn;

internal static class AcornWhitespace
{
    public static readonly Regex NonASCIIwhitespace = new Regex("[\u1680\u2000-\u200a\u202f\u205f\u3000\ufeff]");
}
