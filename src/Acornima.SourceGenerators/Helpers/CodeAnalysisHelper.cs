using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acornima.SourceGenerators.Helpers;

/// <summary>
/// Helpers from mostly based Andrew Lock's work: https://andrewlock.net/series/creating-a-source-generator/
/// </summary>
internal static class CodeAnalysisHelper
{
    public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this ITypeSymbol type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            yield return current;
        }
    }

    public static bool InheritsFrom(this ITypeSymbol type, ITypeSymbol baseType)
    {
        return type.GetBaseTypes().Any(type => SymbolEqualityComparer.Default.Equals(type, baseType));
    }

    public static bool InheritsFromOrIsSameAs(this ITypeSymbol type, ITypeSymbol baseType)
    {
        return SymbolEqualityComparer.Default.Equals(type, baseType) || type.InheritsFrom(baseType);
    }

    public static IEnumerable<ISymbol> GetMembersIncludingInherited(this ITypeSymbol type, string name)
    {
        do
        {
            foreach (var member in type.GetMembers(name))
            {
                yield return member;
            }
        }
        while ((type = type!.BaseType!) is not null);
    }

    public static string? ResolveStringConstantExpression(this ExpressionSyntax expressionSyntax)
    {
        if (expressionSyntax is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }

        var values = new List<string>();

        foreach (var node in expressionSyntax.DescendantNodesAndSelf())
        {
            switch (node)
            {
                case BinaryExpressionSyntax binaryExpression:
                    if (!binaryExpression.IsKind(SyntaxKind.AddExpression))
                    {
                        return null;
                    }
                    break;

                case LiteralExpressionSyntax literalExpression:
                    if (!literalExpression.IsKind(SyntaxKind.StringLiteralExpression))
                    {
                        return null;
                    }
                    values.Add(literalExpression.Token.ValueText);
                    break;

                case ParenthesizedExpressionSyntax:
                    break;

                default:
                    return null;
            }
        }

        return string.Concat(values);
    }

    public static TypeSyntax? ToFullyQualifiedName(this TypeSyntax type, SemanticModel semanticModel, StringBuilder? tempSb = null)
    {
        if (semanticModel.GetSymbolInfo(type).Symbol is not ITypeSymbol typeSymbol
            || CSharpTypeName.From(typeSymbol) is not { } typeName)
        {
            return null;
        }

        if (typeName.BareName.SpecialTypeName is not null)
        {
            return type;
        }

        tempSb = tempSb is not null ? tempSb.Clear() : new StringBuilder();

        typeName.AppendTo(tempSb, static _ => true);

        var qualifiedTypeSyntax = SyntaxFactory.ParseTypeName(tempSb.ToString());

        if (type.HasLeadingTrivia)
        {
            qualifiedTypeSyntax = qualifiedTypeSyntax.WithLeadingTrivia(type.GetLeadingTrivia());
        }

        if (type.HasTrailingTrivia)
        {
            qualifiedTypeSyntax = qualifiedTypeSyntax.WithTrailingTrivia(type.GetTrailingTrivia());
        }

        return qualifiedTypeSyntax;
    }

    public static string GetMethodSignatureWithFullyQualifiedTypeNames(this MethodDeclarationSyntax methodDeclarationSyntax, SemanticModel semanticModel)
    {
        var rewriter = new MethodSignatureRewriter(semanticModel);
        methodDeclarationSyntax = (MethodDeclarationSyntax)rewriter.Visit(methodDeclarationSyntax);
        return methodDeclarationSyntax.ToString();
    }

    private sealed class MethodSignatureRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;

        private StringBuilder? _tempSb;
        private StringBuilder TempSb => _tempSb ??= new StringBuilder();

        public MethodSignatureRewriter(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            return node.Update(attributeLists: default, VisitList(node.Modifiers), (TypeSyntax?)Visit(node.ReturnType) ?? throw new ArgumentNullException("returnType"), (ExplicitInterfaceSpecifierSyntax?)Visit(node.ExplicitInterfaceSpecifier), VisitToken(node.Identifier), (TypeParameterListSyntax?)Visit(node.TypeParameterList), (ParameterListSyntax?)Visit(node.ParameterList) ?? throw new ArgumentNullException("parameterList"), VisitList(node.ConstraintClauses), (BlockSyntax?)Visit(node.Body), (ArrowExpressionClauseSyntax?)Visit(node.ExpressionBody), default);
        }

        public override SyntaxNode? VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
        {
            // already fully qualified
            return node;
        }

        public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
        {
            // may not be fully qualified, e.g. List<int>.Enumerator)
            return node.Right.ToFullyQualifiedName(_semanticModel, TempSb) ?? node;
        }

        public override SyntaxNode? VisitGenericName(GenericNameSyntax node)
        {
            return node.ToFullyQualifiedName(_semanticModel, TempSb) ?? node;
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            return node.ToFullyQualifiedName(_semanticModel, TempSb) ?? node;
        }
    }
}
