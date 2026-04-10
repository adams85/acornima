using System;

namespace Acornima;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class StringMatcherAttribute : Attribute
{
    public StringMatcherAttribute(params string[] targets)
    {
        Targets = targets;
    }

    public string[] Targets { get; }
}
