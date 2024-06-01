using System;

namespace Acornima;

public static class ParserOptionsExtensions
{
    private static readonly OnNodeHandler s_parentSetter = node =>
    {
        foreach (var child in node.ChildNodes)
        {
            child.UserData = node;
        }
    };

    public static TOptions RecordParentNodeInUserData<TOptions>(this TOptions options, bool enable = true)
        where TOptions : ParserOptions
    {
        options._onNode = (OnNodeHandler?)Delegate.RemoveAll(options._onNode, s_parentSetter);

        if (enable)
        {
            options._onNode += s_parentSetter;
        }

        return options;
    }
}
