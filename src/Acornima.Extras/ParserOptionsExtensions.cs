using System;
using Acornima.Ast;
using Acornima.Helpers;

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

    public static TOptions RecordScopeInfoInUserData<TOptions>(this TOptions options, bool enable = true)
        where TOptions : ParserOptions
    {
        var helper = options._onNode?.Target as OnNodeHelper;
        if (enable)
        {
            (helper ?? new OnNodeHelper()).EnableScopeInfoRecoding(options);
        }
        else
        {
            helper?.DisableScopeInfoRecoding(options);
        }

        return options;
    }

    private sealed class OnNodeHelper : IOnNodeHandlerWrapper
    {
        private OnNodeHandler? _onNode;
        public OnNodeHandler? OnNode { get => _onNode; set => _onNode = value; }

        private ArrayList<ScopeInfo> _scopes;

        public void ReleaseLargeBuffers()
        {
            _scopes.Clear();
            if (_scopes.Capacity > 64)
            {
                _scopes.Capacity = 64;
            }
        }

        public void EnableParentNodeRecoding(ParserOptions options)
        {
            if (!ReferenceEquals(options._onNode?.Target, this))
            {
                _onNode = options._onNode;
                options._onNode = SetParentNode;
            }
            else if (options._onNode == SetScopeInfo)
            {
                options._onNode = SetParentNodeAndScopeInfo;
            }
        }

        public void DisableParentNodeRecoding(ParserOptions options)
        {
            if (options._onNode == SetParentNodeAndScopeInfo)
            {
                options._onNode = SetScopeInfo;
            }
            else if (options._onNode == SetParentNode)
            {
                options._onNode = _onNode;
            }
        }

        public void EnableScopeInfoRecoding(ParserOptions options)
        {
            if (!ReferenceEquals(options._onNode?.Target, this))
            {
                _onNode = options._onNode;
                options._onNode = SetScopeInfo;
            }
            else if (options._onNode == SetParentNode)
            {
                options._onNode = SetParentNodeAndScopeInfo;
            }
        }

        public void DisableScopeInfoRecoding(ParserOptions options)
        {
            if (options._onNode == SetParentNodeAndScopeInfo)
            {
                options._onNode = SetParentNode;
            }
            else if (options._onNode == SetScopeInfo)
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

        private void SetScopeInfo(Node node, OnNodeContext context)
        {
            if (context.HasScope)
            {
                SetScopeInfoCore(node, context._scope.Value, context.ScopeStack);
            }

            _onNode?.Invoke(node, context);
        }

        private void SetParentNodeAndScopeInfo(Node node, OnNodeContext context)
        {
            if (context.HasScope)
            {
                SetScopeInfoCore(node, context._scope.Value, context.ScopeStack);
            }

            foreach (var child in node.ChildNodes)
            {
                if (child.UserData is ScopeInfo scopeInfo)
                {
                    scopeInfo.UserData = node;
                }
                else
                {
                    child.UserData = node;
                }
            }

            _onNode?.Invoke(node, context);
        }

        private void SetScopeInfoCore(Node node, in Scope scope, ReadOnlySpan<Scope> scopeStack)
        {
            for (var n = scope.Id - _scopes.Count; n >= 0; n--)
            {
                ref var scopeInfoRef = ref _scopes.PushRef();
                scopeInfoRef ??= new ScopeInfo();
            }

            var scopeInfo = _scopes.GetItemRef(scope.Id);
            ref readonly var parentScope = ref scopeStack.Last();
            var parentScopeInfo = scope.Id != scopeStack[0].Id ? _scopes[parentScope.Id] : null;

            var varVariables = scope.VarVariables;
            var lexicalVariables = scope.LexicalVariables;
            Identifier? additionalLexicalVariable = null;

            // In the case of function and catch clause scopes, we need to create a separate scope for parameters,
            // otherwise variables declared in the body would be "visible" to the parameter nodes.

            switch (node.Type)
            {
                case NodeType.CatchClause:
                    var catchClause = node.As<CatchClause>();

                    node.UserData = parentScopeInfo = new ScopeInfo().Initialize(
                       node,
                       parent: parentScopeInfo,
                       varScope: _scopes[scopeStack[scope.CurrentVarScopeIndex].Id],
                       thisScope: _scopes[scopeStack[scope.CurrentThisScopeIndex].Id],
                       varVariables,
                       lexicalVariables: lexicalVariables.Slice(0, scope.LexicalParamCount),
                       functions: default);

                    node = catchClause.Body;
                    lexicalVariables = lexicalVariables.Slice(scope.LexicalParamCount);
                    break;

                case NodeType.ArrowFunctionExpression or NodeType.FunctionDeclaration or NodeType.FunctionExpression:
                    var function = node.As<IFunction>();
                    var functionBody = function.Body as FunctionBody;

                    node.UserData = parentScopeInfo = (functionBody is not null ? new ScopeInfo() : scopeInfo).Initialize(
                        node,
                        parent: parentScopeInfo,
                        varScope: _scopes[scopeStack[parentScope.CurrentVarScopeIndex].Id],
                        thisScope: _scopes[scopeStack[parentScope.CurrentThisScopeIndex].Id],
                        varVariables: varVariables.Slice(0, scope.VarParamCount),
                        lexicalVariables: default,
                        functions: default,
                        additionalVarVariable: function.Id);

                    if (functionBody is null)
                    {
                        return;
                    }

                    node = functionBody;
                    varVariables = varVariables.Slice(scope.VarParamCount);
                    break;

                case NodeType.ClassDeclaration or NodeType.ClassExpression:
                    additionalLexicalVariable = node.As<IClass>()?.Id;
                    break;
            }

            node.UserData = scopeInfo.Initialize(
                node,
                parent: parentScopeInfo,
                varScope: scope.CurrentVarScopeIndex == scopeStack.Length ? scopeInfo : _scopes[scopeStack[scope.CurrentVarScopeIndex].Id],
                thisScope: scope.CurrentThisScopeIndex == scopeStack.Length ? scopeInfo : _scopes[scopeStack[scope.CurrentThisScopeIndex].Id],
                varVariables,
                lexicalVariables,
                functions: scope.Functions,
                additionalLexicalVariable: additionalLexicalVariable);
        }
    }
}
