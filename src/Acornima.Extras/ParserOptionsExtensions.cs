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

    /// <remarks>
    /// WARNING: Setting <see cref="ParserOptions.OnNode"/> after enabling this setting will cancel parent node recording.
    /// </remarks>
    public static ParserOptions RecordParentNodeInUserData(this ParserOptions options, bool enable)
    {
        Delegate.RemoveAll(options.OnNode, s_parentSetter);

        if (enable)
        {
            options._onNode += s_parentSetter;
        }

        return options;
    }
}
