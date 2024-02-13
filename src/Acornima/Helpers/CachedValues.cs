namespace Acornima.Helpers;

internal static class CachedValues
{
    public static readonly object True = true;
    public static readonly object False = false;

    public static object AsCachedObject(this bool value) => value ? True : False;
}
