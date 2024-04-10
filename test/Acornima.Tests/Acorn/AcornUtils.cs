using System.Text.RegularExpressions;

namespace Acornima.Tests.Acorn;

internal static class AcornUtils
{
    public static Regex WordsRegexp(string words)
    {
        var pattern = $"^(?:{words.Replace(' ', '|')})$";
        return new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.ECMAScript);
    }
}
