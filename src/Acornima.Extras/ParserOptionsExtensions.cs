using Acornima.Ast;

namespace Acornima;

public static class ParserOptionsExtensions
{
    public static TOptions RecordParentNodeInUserData<TOptions>(this TOptions options, bool enable = true)
        where TOptions : ParserOptions
    {
        var helper = options._onNode?.Target as OnNodeHelper;
        if (enable)
        {
            (helper ?? new OnNodeHelper()).EnableParentNodeRecoding(options);
        }
        else
        {
            helper?.DisableParentNodeRecoding(options);
        }

        return options;
    }

    private sealed class OnNodeHelper : IOnNodeHandlerWrapper
    {
        private OnNodeHandler? _onNode;
        public OnNodeHandler? OnNode { get => _onNode; set => _onNode = value; }

        public void EnableParentNodeRecoding(ParserOptions options)
        {
            if (!ReferenceEquals(options._onNode?.Target, this))
            {
                _onNode = options._onNode;
                options._onNode = SetParentNode;
            }
        }

        public void DisableParentNodeRecoding(ParserOptions options)
        {
            if (options._onNode == SetParentNode)
            {
                options._onNode = _onNode;
            }
        }

        private void SetParentNode(Node node, OnNodeContext context)
        {
            foreach (var child in node.ChildNodes)
            {
                child.UserData = node;
            }

            _onNode?.Invoke(node, context);
        }
    }
}
