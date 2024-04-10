//HintName: Acornima.AstVisitor.g.cs
#nullable enable

namespace Acornima;

partial class AstVisitor
{
    protected internal virtual object? VisitAccessorProperty(Acornima.Ast.AccessorProperty node)
    {
        ref readonly var decorators = ref node.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i]);
        }
        
        Visit(node.Key);
        
        if (node.Value is not null)
        {
            Visit(node.Value);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitArrayExpression(Acornima.Ast.ArrayExpression node)
    {
        ref readonly var elements = ref node.Elements;
        for (var i = 0; i < elements.Count; i++)
        {
            var elementsItem = elements[i];
            if (elementsItem is not null)
            {
                Visit(elementsItem);
            }
        }
        
        return node;
    }
    
    protected internal virtual object? VisitArrayPattern(Acornima.Ast.ArrayPattern node)
    {
        ref readonly var elements = ref node.Elements;
        for (var i = 0; i < elements.Count; i++)
        {
            var elementsItem = elements[i];
            if (elementsItem is not null)
            {
                Visit(elementsItem);
            }
        }
        
        return node;
    }
    
    protected internal virtual object? VisitArrowFunctionExpression(Acornima.Ast.ArrowFunctionExpression node)
    {
        ref readonly var @params = ref node.Params;
        for (var i = 0; i < @params.Count; i++)
        {
            Visit(@params[i]);
        }
        
        Visit(node.Body);
        
        return node;
    }
    
    protected internal virtual object? VisitAssignmentExpression(Acornima.Ast.AssignmentExpression node)
    {
        Visit(node.Left);
        
        Visit(node.Right);
        
        return node;
    }
    
    protected internal virtual object? VisitAssignmentPattern(Acornima.Ast.AssignmentPattern node)
    {
        Visit(node.Left);
        
        Visit(node.Right);
        
        return node;
    }
    
    protected internal virtual object? VisitAwaitExpression(Acornima.Ast.AwaitExpression node)
    {
        Visit(node.Argument);
        
        return node;
    }
    
    protected internal virtual object? VisitBinaryExpression(Acornima.Ast.BinaryExpression node)
    {
        Visit(node.Left);
        
        Visit(node.Right);
        
        return node;
    }
    
    protected internal virtual object? VisitBlockStatement(Acornima.Ast.BlockStatement node)
    {
        ref readonly var body = ref node.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitBreakStatement(Acornima.Ast.BreakStatement node)
    {
        if (node.Label is not null)
        {
            Visit(node.Label);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitCallExpression(Acornima.Ast.CallExpression node)
    {
        Visit(node.Callee);
        
        ref readonly var arguments = ref node.Arguments;
        for (var i = 0; i < arguments.Count; i++)
        {
            Visit(arguments[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitCatchClause(Acornima.Ast.CatchClause node)
    {
        if (node.Param is not null)
        {
            Visit(node.Param);
        }
        
        Visit(node.Body);
        
        return node;
    }
    
    protected internal virtual object? VisitChainExpression(Acornima.Ast.ChainExpression node)
    {
        Visit(node.Expression);
        
        return node;
    }
    
    protected internal virtual object? VisitClassBody(Acornima.Ast.ClassBody node)
    {
        ref readonly var body = ref node.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitClassDeclaration(Acornima.Ast.ClassDeclaration node)
    {
        ref readonly var decorators = ref node.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i]);
        }
        
        if (node.Id is not null)
        {
            Visit(node.Id);
        }
        
        if (node.SuperClass is not null)
        {
            Visit(node.SuperClass);
        }
        
        Visit(node.Body);
        
        return node;
    }
    
    protected internal virtual object? VisitClassExpression(Acornima.Ast.ClassExpression node)
    {
        ref readonly var decorators = ref node.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i]);
        }
        
        if (node.Id is not null)
        {
            Visit(node.Id);
        }
        
        if (node.SuperClass is not null)
        {
            Visit(node.SuperClass);
        }
        
        Visit(node.Body);
        
        return node;
    }
    
    protected internal virtual object? VisitConditionalExpression(Acornima.Ast.ConditionalExpression node)
    {
        Visit(node.Test);
        
        Visit(node.Consequent);
        
        Visit(node.Alternate);
        
        return node;
    }
    
    protected internal virtual object? VisitContinueStatement(Acornima.Ast.ContinueStatement node)
    {
        if (node.Label is not null)
        {
            Visit(node.Label);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitDebuggerStatement(Acornima.Ast.DebuggerStatement node)
    {
        return node;
    }
    
    protected internal virtual object? VisitDecorator(Acornima.Ast.Decorator node)
    {
        Visit(node.Expression);
        
        return node;
    }
    
    protected internal virtual object? VisitDoWhileStatement(Acornima.Ast.DoWhileStatement node)
    {
        Visit(node.Body);
        
        Visit(node.Test);
        
        return node;
    }
    
    protected internal virtual object? VisitEmptyStatement(Acornima.Ast.EmptyStatement node)
    {
        return node;
    }
    
    protected internal virtual object? VisitExportAllDeclaration(Acornima.Ast.ExportAllDeclaration node)
    {
        if (node.Exported is not null)
        {
            Visit(node.Exported);
        }
        
        Visit(node.Source);
        
        ref readonly var attributes = ref node.Attributes;
        for (var i = 0; i < attributes.Count; i++)
        {
            Visit(attributes[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitExportDefaultDeclaration(Acornima.Ast.ExportDefaultDeclaration node)
    {
        Visit(node.Declaration);
        
        return node;
    }
    
    protected internal virtual object? VisitExportNamedDeclaration(Acornima.Ast.ExportNamedDeclaration node)
    {
        if (node.Declaration is not null)
        {
            Visit(node.Declaration);
        }
        
        ref readonly var specifiers = ref node.Specifiers;
        for (var i = 0; i < specifiers.Count; i++)
        {
            Visit(specifiers[i]);
        }
        
        if (node.Source is not null)
        {
            Visit(node.Source);
        }
        
        ref readonly var attributes = ref node.Attributes;
        for (var i = 0; i < attributes.Count; i++)
        {
            Visit(attributes[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitExpressionStatement(Acornima.Ast.ExpressionStatement node)
    {
        Visit(node.Expression);
        
        return node;
    }
    
    protected internal virtual object? VisitForInStatement(Acornima.Ast.ForInStatement node)
    {
        Visit(node.Left);
        
        Visit(node.Right);
        
        Visit(node.Body);
        
        return node;
    }
    
    protected internal virtual object? VisitForOfStatement(Acornima.Ast.ForOfStatement node)
    {
        Visit(node.Left);
        
        Visit(node.Right);
        
        Visit(node.Body);
        
        return node;
    }
    
    protected internal virtual object? VisitForStatement(Acornima.Ast.ForStatement node)
    {
        if (node.Init is not null)
        {
            Visit(node.Init);
        }
        
        if (node.Test is not null)
        {
            Visit(node.Test);
        }
        
        if (node.Update is not null)
        {
            Visit(node.Update);
        }
        
        Visit(node.Body);
        
        return node;
    }
    
    protected internal virtual object? VisitFunctionBody(Acornima.Ast.FunctionBody node)
    {
        ref readonly var body = ref node.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitFunctionDeclaration(Acornima.Ast.FunctionDeclaration node)
    {
        if (node.Id is not null)
        {
            Visit(node.Id);
        }
        
        ref readonly var @params = ref node.Params;
        for (var i = 0; i < @params.Count; i++)
        {
            Visit(@params[i]);
        }
        
        Visit(node.Body);
        
        return node;
    }
    
    protected internal virtual object? VisitFunctionExpression(Acornima.Ast.FunctionExpression node)
    {
        if (node.Id is not null)
        {
            Visit(node.Id);
        }
        
        ref readonly var @params = ref node.Params;
        for (var i = 0; i < @params.Count; i++)
        {
            Visit(@params[i]);
        }
        
        Visit(node.Body);
        
        return node;
    }
    
    protected internal virtual object? VisitIdentifier(Acornima.Ast.Identifier node)
    {
        return node;
    }
    
    protected internal virtual object? VisitIfStatement(Acornima.Ast.IfStatement node)
    {
        Visit(node.Test);
        
        Visit(node.Consequent);
        
        if (node.Alternate is not null)
        {
            Visit(node.Alternate);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitImportAttribute(Acornima.Ast.ImportAttribute node)
    {
        Visit(node.Key);
        
        Visit(node.Value);
        
        return node;
    }
    
    protected internal virtual object? VisitImportDeclaration(Acornima.Ast.ImportDeclaration node)
    {
        ref readonly var specifiers = ref node.Specifiers;
        for (var i = 0; i < specifiers.Count; i++)
        {
            Visit(specifiers[i]);
        }
        
        Visit(node.Source);
        
        ref readonly var attributes = ref node.Attributes;
        for (var i = 0; i < attributes.Count; i++)
        {
            Visit(attributes[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitImportDefaultSpecifier(Acornima.Ast.ImportDefaultSpecifier node)
    {
        Visit(node.Local);
        
        return node;
    }
    
    protected internal virtual object? VisitImportExpression(Acornima.Ast.ImportExpression node)
    {
        Visit(node.Source);
        
        if (node.Options is not null)
        {
            Visit(node.Options);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitImportNamespaceSpecifier(Acornima.Ast.ImportNamespaceSpecifier node)
    {
        Visit(node.Local);
        
        return node;
    }
    
    protected internal virtual object? VisitLabeledStatement(Acornima.Ast.LabeledStatement node)
    {
        Visit(node.Label);
        
        Visit(node.Body);
        
        return node;
    }
    
    protected internal virtual object? VisitLiteral(Acornima.Ast.Literal node)
    {
        return node;
    }
    
    protected internal virtual object? VisitMemberExpression(Acornima.Ast.MemberExpression node)
    {
        Visit(node.Object);
        
        Visit(node.Property);
        
        return node;
    }
    
    protected internal virtual object? VisitMetaProperty(Acornima.Ast.MetaProperty node)
    {
        Visit(node.Meta);
        
        Visit(node.Property);
        
        return node;
    }
    
    protected internal virtual object? VisitMethodDefinition(Acornima.Ast.MethodDefinition node)
    {
        ref readonly var decorators = ref node.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i]);
        }
        
        Visit(node.Key);
        
        Visit(node.Value);
        
        return node;
    }
    
    protected internal virtual object? VisitNewExpression(Acornima.Ast.NewExpression node)
    {
        Visit(node.Callee);
        
        ref readonly var arguments = ref node.Arguments;
        for (var i = 0; i < arguments.Count; i++)
        {
            Visit(arguments[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitObjectExpression(Acornima.Ast.ObjectExpression node)
    {
        ref readonly var properties = ref node.Properties;
        for (var i = 0; i < properties.Count; i++)
        {
            Visit(properties[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitObjectPattern(Acornima.Ast.ObjectPattern node)
    {
        ref readonly var properties = ref node.Properties;
        for (var i = 0; i < properties.Count; i++)
        {
            Visit(properties[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitParenthesizedExpression(Acornima.Ast.ParenthesizedExpression node)
    {
        Visit(node.Expression);
        
        return node;
    }
    
    protected internal virtual object? VisitPrivateIdentifier(Acornima.Ast.PrivateIdentifier node)
    {
        return node;
    }
    
    protected internal virtual object? VisitProgram(Acornima.Ast.Program node)
    {
        ref readonly var body = ref node.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitPropertyDefinition(Acornima.Ast.PropertyDefinition node)
    {
        ref readonly var decorators = ref node.Decorators;
        for (var i = 0; i < decorators.Count; i++)
        {
            Visit(decorators[i]);
        }
        
        Visit(node.Key);
        
        if (node.Value is not null)
        {
            Visit(node.Value);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitRestElement(Acornima.Ast.RestElement node)
    {
        Visit(node.Argument);
        
        return node;
    }
    
    protected internal virtual object? VisitReturnStatement(Acornima.Ast.ReturnStatement node)
    {
        if (node.Argument is not null)
        {
            Visit(node.Argument);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitSequenceExpression(Acornima.Ast.SequenceExpression node)
    {
        ref readonly var expressions = ref node.Expressions;
        for (var i = 0; i < expressions.Count; i++)
        {
            Visit(expressions[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitSpreadElement(Acornima.Ast.SpreadElement node)
    {
        Visit(node.Argument);
        
        return node;
    }
    
    protected internal virtual object? VisitStaticBlock(Acornima.Ast.StaticBlock node)
    {
        ref readonly var body = ref node.Body;
        for (var i = 0; i < body.Count; i++)
        {
            Visit(body[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitSuper(Acornima.Ast.Super node)
    {
        return node;
    }
    
    protected internal virtual object? VisitSwitchCase(Acornima.Ast.SwitchCase node)
    {
        if (node.Test is not null)
        {
            Visit(node.Test);
        }
        
        ref readonly var consequent = ref node.Consequent;
        for (var i = 0; i < consequent.Count; i++)
        {
            Visit(consequent[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitSwitchStatement(Acornima.Ast.SwitchStatement node)
    {
        Visit(node.Discriminant);
        
        ref readonly var cases = ref node.Cases;
        for (var i = 0; i < cases.Count; i++)
        {
            Visit(cases[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitTaggedTemplateExpression(Acornima.Ast.TaggedTemplateExpression node)
    {
        Visit(node.Tag);
        
        Visit(node.Quasi);
        
        return node;
    }
    
    protected internal virtual object? VisitTemplateElement(Acornima.Ast.TemplateElement node)
    {
        return node;
    }
    
    protected internal virtual object? VisitThisExpression(Acornima.Ast.ThisExpression node)
    {
        return node;
    }
    
    protected internal virtual object? VisitThrowStatement(Acornima.Ast.ThrowStatement node)
    {
        Visit(node.Argument);
        
        return node;
    }
    
    protected internal virtual object? VisitTryStatement(Acornima.Ast.TryStatement node)
    {
        Visit(node.Block);
        
        if (node.Handler is not null)
        {
            Visit(node.Handler);
        }
        
        if (node.Finalizer is not null)
        {
            Visit(node.Finalizer);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitUnaryExpression(Acornima.Ast.UnaryExpression node)
    {
        Visit(node.Argument);
        
        return node;
    }
    
    protected internal virtual object? VisitVariableDeclaration(Acornima.Ast.VariableDeclaration node)
    {
        ref readonly var declarations = ref node.Declarations;
        for (var i = 0; i < declarations.Count; i++)
        {
            Visit(declarations[i]);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitVariableDeclarator(Acornima.Ast.VariableDeclarator node)
    {
        Visit(node.Id);
        
        if (node.Init is not null)
        {
            Visit(node.Init);
        }
        
        return node;
    }
    
    protected internal virtual object? VisitWhileStatement(Acornima.Ast.WhileStatement node)
    {
        Visit(node.Test);
        
        Visit(node.Body);
        
        return node;
    }
    
    protected internal virtual object? VisitWithStatement(Acornima.Ast.WithStatement node)
    {
        Visit(node.Object);
        
        Visit(node.Body);
        
        return node;
    }
    
    protected internal virtual object? VisitYieldExpression(Acornima.Ast.YieldExpression node)
    {
        if (node.Argument is not null)
        {
            Visit(node.Argument);
        }
        
        return node;
    }
}
