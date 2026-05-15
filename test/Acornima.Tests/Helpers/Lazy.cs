using System;

namespace Acornima.Tests.Helpers;

internal static class Lazy
{
    public static Lazy<T, TMetadata> Create<T, TMetadata>(TMetadata metadata, Func<T> factory)
    {
        return new Lazy<T, TMetadata>(factory, metadata);
    }
}
