//HintName: Acornima.AstRewriter.g.cs
#nullable enable

namespace Acornima;

partial class AstRewriter
{
    protected internal override object? VisitAccessorProperty(Acornima.Ast.AccessorProperty node)
    {
        VisitAndConvert(node.Decorators, out var decorators);
        
        var key = VisitAndConvert(node.Key);
        
        var value = VisitAndConvert(node.Value, allowNull: true);
        
        return node.UpdateWith(decorators, key, value);
    }
    
    protected internal override object? VisitArrayExpression(Acornima.Ast.ArrayExpression node)
    {
        VisitAndConvert(node.Elements, out var elements, allowNullElement: true);
        
        return node.UpdateWith(elements);
    }
    
    protected internal override object? VisitArrayPattern(Acornima.Ast.ArrayPattern node)
    {
        VisitAndConvert(node.Elements, out var elements, allowNullElement: true);
        
        return node.UpdateWith(elements);
    }
    
    protected internal override object? VisitArrowFunctionExpression(Acornima.Ast.ArrowFunctionExpression node)
    {
        VisitAndConvert(node.Params, out var @params);
        
        var body = VisitAndConvert(node.Body);
        
        return node.UpdateWith(@params, body);
    }
    
    protected internal override object? VisitAssignmentExpression(Acornima.Ast.AssignmentExpression node)
    {
        var left = VisitAndConvert(node.Left);
        
        var right = VisitAndConvert(node.Right);
        
        return node.UpdateWith(left, right);
    }
    
    protected internal override object? VisitAssignmentPattern(Acornima.Ast.AssignmentPattern node)
    {
        var left = VisitAndConvert(node.Left);
        
        var right = VisitAndConvert(node.Right);
        
        return node.UpdateWith(left, right);
    }
    
    protected internal override object? VisitAwaitExpression(Acornima.Ast.AwaitExpression node)
    {
        var argument = VisitAndConvert(node.Argument);
        
        return node.UpdateWith(argument);
    }
    
    protected internal override object? VisitBinaryExpression(Acornima.Ast.BinaryExpression node)
    {
        var left = VisitAndConvert(node.Left);
        
        var right = VisitAndConvert(node.Right);
        
        return node.UpdateWith(left, right);
    }
    
    protected internal override object? VisitBlockStatement(Acornima.Ast.BlockStatement node)
    {
        VisitAndConvert(node.Body, out var body);
        
        return node.UpdateWith(body);
    }
    
    protected internal override object? VisitBreakStatement(Acornima.Ast.BreakStatement node)
    {
        var label = VisitAndConvert(node.Label, allowNull: true);
        
        return node.UpdateWith(label);
    }
    
    protected internal override object? VisitCallExpression(Acornima.Ast.CallExpression node)
    {
        var callee = VisitAndConvert(node.Callee);
        
        VisitAndConvert(node.Arguments, out var arguments);
        
        return node.UpdateWith(callee, arguments);
    }
    
    protected internal override object? VisitCatchClause(Acornima.Ast.CatchClause node)
    {
        var param = VisitAndConvert(node.Param, allowNull: true);
        
        var body = VisitAndConvert(node.Body);
        
        return node.UpdateWith(param, body);
    }
    
    protected internal override object? VisitChainExpression(Acornima.Ast.ChainExpression node)
    {
        var expression = VisitAndConvert(node.Expression);
        
        return node.UpdateWith(expression);
    }
    
    protected internal override object? VisitClassBody(Acornima.Ast.ClassBody node)
    {
        VisitAndConvert(node.Body, out var body);
        
        return node.UpdateWith(body);
    }
    
    protected internal override object? VisitClassDeclaration(Acornima.Ast.ClassDeclaration node)
    {
        VisitAndConvert(node.Decorators, out var decorators);
        
        var id = VisitAndConvert(node.Id, allowNull: true);
        
        var superClass = VisitAndConvert(node.SuperClass, allowNull: true);
        
        var body = VisitAndConvert(node.Body);
        
        return node.UpdateWith(decorators, id, superClass, body);
    }
    
    protected internal override object? VisitClassExpression(Acornima.Ast.ClassExpression node)
    {
        VisitAndConvert(node.Decorators, out var decorators);
        
        var id = VisitAndConvert(node.Id, allowNull: true);
        
        var superClass = VisitAndConvert(node.SuperClass, allowNull: true);
        
        var body = VisitAndConvert(node.Body);
        
        return node.UpdateWith(decorators, id, superClass, body);
    }
    
    protected internal override object? VisitConditionalExpression(Acornima.Ast.ConditionalExpression node)
    {
        var test = VisitAndConvert(node.Test);
        
        var consequent = VisitAndConvert(node.Consequent);
        
        var alternate = VisitAndConvert(node.Alternate);
        
        return node.UpdateWith(test, consequent, alternate);
    }
    
    protected internal override object? VisitContinueStatement(Acornima.Ast.ContinueStatement node)
    {
        var label = VisitAndConvert(node.Label, allowNull: true);
        
        return node.UpdateWith(label);
    }
    
    protected internal override object? VisitDecorator(Acornima.Ast.Decorator node)
    {
        var expression = VisitAndConvert(node.Expression);
        
        return node.UpdateWith(expression);
    }
    
    protected internal override object? VisitDoWhileStatement(Acornima.Ast.DoWhileStatement node)
    {
        var body = VisitAndConvert(node.Body);
        
        var test = VisitAndConvert(node.Test);
        
        return node.UpdateWith(body, test);
    }
    
    protected internal override object? VisitExportAllDeclaration(Acornima.Ast.ExportAllDeclaration node)
    {
        var exported = VisitAndConvert(node.Exported, allowNull: true);
        
        var source = VisitAndConvert(node.Source);
        
        VisitAndConvert(node.Attributes, out var attributes);
        
        return node.UpdateWith(exported, source, attributes);
    }
    
    protected internal override object? VisitExportDefaultDeclaration(Acornima.Ast.ExportDefaultDeclaration node)
    {
        var declaration = VisitAndConvert(node.Declaration);
        
        return node.UpdateWith(declaration);
    }
    
    protected internal override object? VisitExportNamedDeclaration(Acornima.Ast.ExportNamedDeclaration node)
    {
        var declaration = VisitAndConvert(node.Declaration, allowNull: true);
        
        VisitAndConvert(node.Specifiers, out var specifiers);
        
        var source = VisitAndConvert(node.Source, allowNull: true);
        
        VisitAndConvert(node.Attributes, out var attributes);
        
        return node.UpdateWith(declaration, specifiers, source, attributes);
    }
    
    protected internal override object? VisitExpressionStatement(Acornima.Ast.ExpressionStatement node)
    {
        var expression = VisitAndConvert(node.Expression);
        
        return node.UpdateWith(expression);
    }
    
    protected internal override object? VisitForInStatement(Acornima.Ast.ForInStatement node)
    {
        var left = VisitAndConvert(node.Left);
        
        var right = VisitAndConvert(node.Right);
        
        var body = VisitAndConvert(node.Body);
        
        return node.UpdateWith(left, right, body);
    }
    
    protected internal override object? VisitForOfStatement(Acornima.Ast.ForOfStatement node)
    {
        var left = VisitAndConvert(node.Left);
        
        var right = VisitAndConvert(node.Right);
        
        var body = VisitAndConvert(node.Body);
        
        return node.UpdateWith(left, right, body);
    }
    
    protected internal override object? VisitForStatement(Acornima.Ast.ForStatement node)
    {
        var init = VisitAndConvert(node.Init, allowNull: true);
        
        var test = VisitAndConvert(node.Test, allowNull: true);
        
        var update = VisitAndConvert(node.Update, allowNull: true);
        
        var body = VisitAndConvert(node.Body);
        
        return node.UpdateWith(init, test, update, body);
    }
    
    protected internal override object? VisitFunctionBody(Acornima.Ast.FunctionBody node)
    {
        VisitAndConvert(node.Body, out var body);
        
        return node.UpdateWith(body);
    }
    
    protected internal override object? VisitFunctionDeclaration(Acornima.Ast.FunctionDeclaration node)
    {
        var id = VisitAndConvert(node.Id, allowNull: true);
        
        VisitAndConvert(node.Params, out var @params);
        
        var body = VisitAndConvert(node.Body);
        
        return node.UpdateWith(id, @params, body);
    }
    
    protected internal override object? VisitFunctionExpression(Acornima.Ast.FunctionExpression node)
    {
        var id = VisitAndConvert(node.Id, allowNull: true);
        
        VisitAndConvert(node.Params, out var @params);
        
        var body = VisitAndConvert(node.Body);
        
        return node.UpdateWith(id, @params, body);
    }
    
    protected internal override object? VisitIfStatement(Acornima.Ast.IfStatement node)
    {
        var test = VisitAndConvert(node.Test);
        
        var consequent = VisitAndConvert(node.Consequent);
        
        var alternate = VisitAndConvert(node.Alternate, allowNull: true);
        
        return node.UpdateWith(test, consequent, alternate);
    }
    
    protected internal override object? VisitImportAttribute(Acornima.Ast.ImportAttribute node)
    {
        var key = VisitAndConvert(node.Key);
        
        var value = VisitAndConvert(node.Value);
        
        return node.UpdateWith(key, value);
    }
    
    protected internal override object? VisitImportDeclaration(Acornima.Ast.ImportDeclaration node)
    {
        VisitAndConvert(node.Specifiers, out var specifiers);
        
        var source = VisitAndConvert(node.Source);
        
        VisitAndConvert(node.Attributes, out var attributes);
        
        return node.UpdateWith(specifiers, source, attributes);
    }
    
    protected internal override object? VisitImportDefaultSpecifier(Acornima.Ast.ImportDefaultSpecifier node)
    {
        var local = VisitAndConvert(node.Local);
        
        return node.UpdateWith(local);
    }
    
    protected internal override object? VisitImportExpression(Acornima.Ast.ImportExpression node)
    {
        var source = VisitAndConvert(node.Source);
        
        var options = VisitAndConvert(node.Options, allowNull: true);
        
        return node.UpdateWith(source, options);
    }
    
    protected internal override object? VisitImportNamespaceSpecifier(Acornima.Ast.ImportNamespaceSpecifier node)
    {
        var local = VisitAndConvert(node.Local);
        
        return node.UpdateWith(local);
    }
    
    protected internal override object? VisitLabeledStatement(Acornima.Ast.LabeledStatement node)
    {
        var label = VisitAndConvert(node.Label);
        
        var body = VisitAndConvert(node.Body);
        
        return node.UpdateWith(label, body);
    }
    
    protected internal override object? VisitMemberExpression(Acornima.Ast.MemberExpression node)
    {
        var @object = VisitAndConvert(node.Object);
        
        var property = VisitAndConvert(node.Property);
        
        return node.UpdateWith(@object, property);
    }
    
    protected internal override object? VisitMetaProperty(Acornima.Ast.MetaProperty node)
    {
        var meta = VisitAndConvert(node.Meta);
        
        var property = VisitAndConvert(node.Property);
        
        return node.UpdateWith(meta, property);
    }
    
    protected internal override object? VisitMethodDefinition(Acornima.Ast.MethodDefinition node)
    {
        VisitAndConvert(node.Decorators, out var decorators);
        
        var key = VisitAndConvert(node.Key);
        
        var value = VisitAndConvert(node.Value);
        
        return node.UpdateWith(decorators, key, value);
    }
    
    protected internal override object? VisitNewExpression(Acornima.Ast.NewExpression node)
    {
        var callee = VisitAndConvert(node.Callee);
        
        VisitAndConvert(node.Arguments, out var arguments);
        
        return node.UpdateWith(callee, arguments);
    }
    
    protected internal override object? VisitObjectExpression(Acornima.Ast.ObjectExpression node)
    {
        VisitAndConvert(node.Properties, out var properties);
        
        return node.UpdateWith(properties);
    }
    
    protected internal override object? VisitObjectPattern(Acornima.Ast.ObjectPattern node)
    {
        VisitAndConvert(node.Properties, out var properties);
        
        return node.UpdateWith(properties);
    }
    
    protected internal override object? VisitParenthesizedExpression(Acornima.Ast.ParenthesizedExpression node)
    {
        var expression = VisitAndConvert(node.Expression);
        
        return node.UpdateWith(expression);
    }
    
    protected internal override object? VisitProgram(Acornima.Ast.Program node)
    {
        VisitAndConvert(node.Body, out var body);
        
        return node.UpdateWith(body);
    }
    
    protected internal override object? VisitPropertyDefinition(Acornima.Ast.PropertyDefinition node)
    {
        VisitAndConvert(node.Decorators, out var decorators);
        
        var key = VisitAndConvert(node.Key);
        
        var value = VisitAndConvert(node.Value, allowNull: true);
        
        return node.UpdateWith(decorators, key, value);
    }
    
    protected internal override object? VisitRestElement(Acornima.Ast.RestElement node)
    {
        var argument = VisitAndConvert(node.Argument);
        
        return node.UpdateWith(argument);
    }
    
    protected internal override object? VisitReturnStatement(Acornima.Ast.ReturnStatement node)
    {
        var argument = VisitAndConvert(node.Argument, allowNull: true);
        
        return node.UpdateWith(argument);
    }
    
    protected internal override object? VisitSequenceExpression(Acornima.Ast.SequenceExpression node)
    {
        VisitAndConvert(node.Expressions, out var expressions);
        
        return node.UpdateWith(expressions);
    }
    
    protected internal override object? VisitSpreadElement(Acornima.Ast.SpreadElement node)
    {
        var argument = VisitAndConvert(node.Argument);
        
        return node.UpdateWith(argument);
    }
    
    protected internal override object? VisitStaticBlock(Acornima.Ast.StaticBlock node)
    {
        VisitAndConvert(node.Body, out var body);
        
        return node.UpdateWith(body);
    }
    
    protected internal override object? VisitSwitchCase(Acornima.Ast.SwitchCase node)
    {
        var test = VisitAndConvert(node.Test, allowNull: true);
        
        VisitAndConvert(node.Consequent, out var consequent);
        
        return node.UpdateWith(test, consequent);
    }
    
    protected internal override object? VisitSwitchStatement(Acornima.Ast.SwitchStatement node)
    {
        var discriminant = VisitAndConvert(node.Discriminant);
        
        VisitAndConvert(node.Cases, out var cases);
        
        return node.UpdateWith(discriminant, cases);
    }
    
    protected internal override object? VisitTaggedTemplateExpression(Acornima.Ast.TaggedTemplateExpression node)
    {
        var tag = VisitAndConvert(node.Tag);
        
        var quasi = VisitAndConvert(node.Quasi);
        
        return node.UpdateWith(tag, quasi);
    }
    
    protected internal override object? VisitTemplateLiteral(Acornima.Ast.TemplateLiteral node)
    {
        VisitAndConvert(node.Quasis, out var quasis);
        
        VisitAndConvert(node.Expressions, out var expressions);
        
        return node.UpdateWith(quasis, expressions);
    }
    
    protected internal override object? VisitThrowStatement(Acornima.Ast.ThrowStatement node)
    {
        var argument = VisitAndConvert(node.Argument);
        
        return node.UpdateWith(argument);
    }
    
    protected internal override object? VisitTryStatement(Acornima.Ast.TryStatement node)
    {
        var block = VisitAndConvert(node.Block);
        
        var handler = VisitAndConvert(node.Handler, allowNull: true);
        
        var finalizer = VisitAndConvert(node.Finalizer, allowNull: true);
        
        return node.UpdateWith(block, handler, finalizer);
    }
    
    protected internal override object? VisitUnaryExpression(Acornima.Ast.UnaryExpression node)
    {
        var argument = VisitAndConvert(node.Argument);
        
        return node.UpdateWith(argument);
    }
    
    protected internal override object? VisitVariableDeclaration(Acornima.Ast.VariableDeclaration node)
    {
        VisitAndConvert(node.Declarations, out var declarations);
        
        return node.UpdateWith(declarations);
    }
    
    protected internal override object? VisitVariableDeclarator(Acornima.Ast.VariableDeclarator node)
    {
        var id = VisitAndConvert(node.Id);
        
        var init = VisitAndConvert(node.Init, allowNull: true);
        
        return node.UpdateWith(id, init);
    }
    
    protected internal override object? VisitWhileStatement(Acornima.Ast.WhileStatement node)
    {
        var test = VisitAndConvert(node.Test);
        
        var body = VisitAndConvert(node.Body);
        
        return node.UpdateWith(test, body);
    }
    
    protected internal override object? VisitWithStatement(Acornima.Ast.WithStatement node)
    {
        var @object = VisitAndConvert(node.Object);
        
        var body = VisitAndConvert(node.Body);
        
        return node.UpdateWith(@object, body);
    }
    
    protected internal override object? VisitYieldExpression(Acornima.Ast.YieldExpression node)
    {
        var argument = VisitAndConvert(node.Argument, allowNull: true);
        
        return node.UpdateWith(argument);
    }
}
