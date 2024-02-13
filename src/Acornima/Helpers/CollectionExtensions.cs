namespace System.Collections.Generic;

#if !(NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER)
internal static class CollectionExtensions
{
    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
            return true;
        }

        return false;
    }
}
#endif
