using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Acornima.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acornima.SourceGenerators;

// Spec for incremental generators: https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
// How to implement:
// * https://andrewlock.net/exploring-dotnet-6-part-9-source-generator-updates-incremental-generators/
// * https://www.thinktecture.com/en/net/roslyn-source-generators-performance/
// How to debug: https://stackoverflow.com/a/71314452/8656352
[Generator]
public class StringMatcherGenerator : IIncrementalGenerator
{
    // IIncrementalGenerator has an Initialize method that is called by the host exactly once,
    // regardless of the number of further compilations that may occur.
    // For instance a host with multiple loaded projects may share the same generator instance across multiple projects,
    // and will only call Initialize a single time for the lifetime of the host.
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<StringMatcherMethod> helperMethodInfos = context.SyntaxProvider
            .CreateSyntaxProvider(IsSyntaxTargetForGeneration, GetSemanticTargetForGeneration)
            .Where(item => item is not null)!;

        context.RegisterSourceOutput(helperMethodInfos.Collect(), Execute);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken cancellationToken)
    {
        return
            node is MethodDeclarationSyntax methodDeclarationSyntax &&
            methodDeclarationSyntax.Body is null;
    }

    private static StringMatcherMethod? GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        // 1.  Discover methods annotated with the expected attribute

        var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;
        var method = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax, cancellationToken);

        AttributeData? attribute;

        if (method is null
            || !method.IsPartialDefinition
            || method.DeclaringSyntaxReferences.Length != 1
            || method.IsGenericMethod
            || method.Parameters.Length != 1
            || (attribute = method.GetAttributes().FirstOrDefault(attribute => attribute.AttributeClass?.Name == "StringMatcherAttribute")) is null
            || attribute.AttributeClass!.ContainingType is not null
            || attribute.AttributeClass.ContainingNamespace.ToString() != "Acornima")
        {
            return null;
        }

        StringMatcherParamType paramType;
        var param = method.Parameters[0];
        if (param.RefKind != RefKind.None)
        {
            return null;
        }
        else if (param.Type.SpecialType == SpecialType.System_String)
        {
            paramType = param.Type.NullableAnnotation == NullableAnnotation.Annotated ? StringMatcherParamType.NullableString : StringMatcherParamType.String;
        }
        else if (param.Type.NullableAnnotation != NullableAnnotation.Annotated
            && param.Type is INamedTypeSymbol { ContainingType: null } namedType
            && namedType.TypeArguments.Length == 1
            && namedType.TypeArguments[0].SpecialType == SpecialType.System_Char
            && namedType.Name == nameof(ReadOnlySpan<char>)
            && namedType.ContainingNamespace.ToString() == typeof(ReadOnlySpan<char>).Namespace)
        {
            paramType = StringMatcherParamType.ReadOnlySpanOfChar;
        }
        else
        {
            return null;
        }

        var attributeSyntax = (AttributeSyntax)attribute.ApplicationSyntaxReference!.GetSyntax(cancellationToken);

        bool hasCustomReturn;
        var argList = attributeSyntax.ArgumentList;
        var alternatives = argList switch
        {
            null => GetAlternatives<ExpressionSyntax>(default, out hasCustomReturn),
            { Arguments.Count: 1 } when argList.Arguments[0].Expression is ArrayCreationExpressionSyntax arrayCreationExpressionSyntax => arrayCreationExpressionSyntax.Initializer is not null
                ? GetAlternatives(arrayCreationExpressionSyntax.Initializer.Expressions, out hasCustomReturn)
                : GetAlternatives<ExpressionSyntax>(default, out hasCustomReturn),
            { Arguments.Count: 1 } when argList.Arguments[0].Expression is ImplicitArrayCreationExpressionSyntax implicitArrayCreationExpressionSyntax =>
                GetAlternatives(implicitArrayCreationExpressionSyntax.Initializer.Expressions, out hasCustomReturn),
            _ => GetAlternatives(argList.Arguments, out hasCustomReturn, node => node.Expression)
        };

        StringMatcherReturnType returnType;
        if (!hasCustomReturn)
        {
            switch (method.ReturnType.SpecialType)
            {
                case SpecialType.System_Boolean:
                    returnType = StringMatcherReturnType.Boolean;
                    break;
                case SpecialType.System_String:
                    returnType = StringMatcherReturnType.String;
                    break;
                default:
                    return null;
            }
        }
        else
        {
            returnType = StringMatcherReturnType.Custom;
        }

        var containingTypeName = CSharpTypeName.From(method.ContainingType)!;
        var methodSignature = methodDeclarationSyntax.GetMethodSignatureWithFullyQualifiedTypeNames(context.SemanticModel);

        return new StringMatcherMethod(containingTypeName, methodSignature, param.Name, paramType, returnType, alternatives);

        static string? GetReturnExpression(ExpressionSyntax expressionSyntax, ref bool hasCustomReturn)
        {
            var trailingTrivia = expressionSyntax.GetTrailingTrivia();
            if (!trailingTrivia.Any())
            {
                return null;
            }

            var match = trailingTrivia
                .Where(trivia => trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                .Select(trivia => Regex.Match(trivia.ToString(), @"^/\*\s*=>\s*(\S|\S.*\S)\s*\*/$"))
                .FirstOrDefault(match => match.Success);
            if (match is null)
            {
                return null;
            }

            hasCustomReturn = true;
            return match.Groups[1].Value;
        }

        static StringMatcherAlternative[] GetAlternatives<TNode>(SeparatedSyntaxList<TNode> elementSyntaxList, out bool hasCustomReturn, Func<TNode, ExpressionSyntax>? getExpression = null)
            where TNode : CSharpSyntaxNode
        {
            hasCustomReturn = false;

            if (!elementSyntaxList.Any())
            {
                return Array.Empty<StringMatcherAlternative>();
            }

            getExpression ??= node => (ExpressionSyntax)(CSharpSyntaxNode)node;

            var alternatives = new StringMatcherAlternative[elementSyntaxList.Count];
            for (var i = 0; i < elementSyntaxList.Count; i++)
            {
                var expressionSyntax = getExpression(elementSyntaxList[i]);
                var target = expressionSyntax.ResolveStringConstantExpression();
                if (target is not null)
                {
                    var returnExpression = GetReturnExpression(expressionSyntax, ref hasCustomReturn);
                    alternatives[i] = new StringMatcherAlternative(target, returnExpression);
                }
            }
            return alternatives;
        }
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<StringMatcherMethod> methods)
    {
        var sb = new SourceBuilder();
        foreach (var methodGroup in methods.GroupBy(method => method.ContainingTypeName))
        {
            var className = methodGroup.Key;
            if (className.BareName.Container is not null)
            {
                throw new NotImplementedException("Support for nested classes is not implemented yet.");
            }

            GenerateContainingType(sb, className, methodGroup, context.CancellationToken);

            var sourceFileName = className is { BareName.IsGeneric: true }
                ? className.ToNonGeneric().ToString() + "`"
                    + className.BareName.GenericArguments.Length.ToString(CultureInfo.InvariantCulture)
                : className.ToString();

            context.AddSource($"{sourceFileName}.g.cs", sb.ToString());

            sb.Reset();
        }
    }

    private static void GenerateContainingType(SourceBuilder sb, CSharpTypeName className, IEnumerable<StringMatcherMethod> methods, CancellationToken cancellationToken)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine();

        if (className.Namespace is not null)
        {
            sb.AppendLine($"namespace {className.Namespace};");
            sb.AppendLine();
        }

        var type = className.TypeKind == TypeKind.Struct ? "struct" : "class";
        sb.Append($"partial {type} ").AppendTypeBareName(className.BareName).AppendLine();
        sb.AppendLine("{");
        sb.IncreaseIndent();

        string? methodSeparator = null;
        foreach (var method in methods)
        {
            cancellationToken.ThrowIfCancellationRequested();

            sb.Append(methodSeparator);
            methodSeparator = Environment.NewLine;

            AppendStringMatcherMethod(sb, method, cancellationToken);
        }

        sb.DecreaseIndent();
        sb.AppendLine("}");
    }

    private static void AppendStringMatcherMethod(SourceBuilder sb, StringMatcherMethod method, CancellationToken cancellationToken)
    {
        sb.AppendLine(method.MethodSignature);
        sb.AppendLine("{");
        sb.IncreaseIndent();

        AppendLookup(sb, method.Alternatives, method.ParamName, method.ParamType, method.ReturnType, cancellationToken);

        sb.DecreaseIndent();
        sb.AppendLine("}");
    }

    /// <summary>
    /// Builds optimized value lookup using known facts about keys.
    /// </summary>
    private static void AppendLookup(SourceBuilder sb, StringMatcherAlternative[] alternatives,
        string paramName, StringMatcherParamType paramType, StringMatcherReturnType returnType, CancellationToken cancellationToken)
    {
        var distinctAlternatives = new HashSet<StringMatcherAlternative>(alternatives, StringMatcherAlternative.TargetComparer);

        var groupsByLength = distinctAlternatives
            .ToLookup(x => x.Target.Length)
            .OrderBy(x => x.Key)
            .Select(x => (x.Key, x.OrderBy(x => x.Target).ToArray()))
            .ToArray();

        if (paramType == StringMatcherParamType.NullableString)
        {
            sb.AppendLine($"if ({paramName} is null)");
            sb.AppendLine("{");
            sb.IncreaseIndent();

            sb.AppendLine($"return {GetDefaultReturnValue(returnType)};");

            sb.DecreaseIndent();
            sb.AppendLine("}");
        }

        if (groupsByLength.Length > 1)
        {
            sb.AppendLine($"switch ({paramName}.Length)");
            sb.AppendLine("{");
            sb.IncreaseIndent();
        }

        foreach (var (targetLength, group) in groupsByLength)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (groupsByLength.Length > 1)
            {
                sb.AppendLine($"case {targetLength}:");
                sb.AppendLine("{");
                sb.IncreaseIndent();
            }

            if (group.Length == 1)
            {
                var item = group[0];
                sb.Append("return ");
                AppendStringEqualityCheck(sb, item.Target, paramName, paramType, startIndex: 0, discriminatorIndex: -1);
                AppendConditionalReturnValue(sb, item, returnType);
                sb.AppendLine(";");
            }
            else
            {
                var discriminatorIndex = FindDiscriminatorIndex(group, 0);

                if (discriminatorIndex == -1)
                {
                    // try next best effort
                    sb.Append("return ");
                    AppendSwitchForStringContent(sb, discriminatorIndex: 0, targetLength, group, paramName, paramType, returnType);
                    sb.AppendLine(";");
                }
                else
                {
                    AppendDiscriminatorMatching(sb, discriminatorIndex, group, paramName, paramType, returnType);
                }
            }

            if (groupsByLength.Length > 1)
            {
                sb.DecreaseIndent();
                sb.AppendLine("}");
            }
        }

        if (groupsByLength.Length > 1)
        {
            sb.AppendLine("default:");
            sb.IncreaseIndent();

            sb.AppendLine($"return {GetDefaultReturnValue(returnType)};");
            sb.DecreaseIndent();

            sb.DecreaseIndent();
            sb.AppendLine("}");
        }
    }

    private static void AppendStringEqualityCheck(SourceBuilder sb, string target, string paramName, StringMatcherParamType paramType,
        int startIndex, int discriminatorIndex)
    {
        // TODO: with net7 should be faster to do the sequence equals always
        // if we ever add a target for net7/8 we should revisit this equality checking
        var lengthToCheck = target.Length - startIndex;
        if (lengthToCheck <= 8)
        {
            // check char by char
            var addAnd = false;
            for (var i = startIndex; i < target.Length; i++)
            {
                if (i == discriminatorIndex)
                {
                    // no need to check
                    continue;
                }

                if (addAnd)
                {
                    sb.Append(" && ");
                }

                sb
                    .Append($"{paramName}[{i}]")
                    .Append(" == '")
                    .AppendEscaped(target[i]).Append("'");

                addAnd = true;
            }
        }
        else
        {
            sb.Append(paramName);
            if (startIndex > 0 || paramType == StringMatcherParamType.ReadOnlySpanOfChar)
            {
                if (paramType != StringMatcherParamType.ReadOnlySpanOfChar)
                {
                    sb.Append($"{paramName}.AsSpan({startIndex})");
                }
                else if (startIndex > 0)
                {
                    sb.Append($"{paramName}.Slice({startIndex})");
                }
                sb.Append(".SequenceEqual(\"")
                    .AppendEscaped(target.AsSpan(startIndex).ToString())
                    .Append("\".AsSpan())");
            }
            else
            {
                sb.Append($" == \"").AppendEscaped(target).Append("\"");
            }
        }
    }

    private static void AppendSwitchForStringContent(SourceBuilder sb, int discriminatorIndex, int length,
        StringMatcherAlternative[] alternatives, string paramName, StringMatcherParamType paramType, StringMatcherReturnType returnType)
    {
        var subGroups = alternatives
            .GroupBy(x => x.Target[discriminatorIndex])
            .OrderBy(x => x.Key)
            .Select(x => (x.Key, x.OrderBy(x => x.Target).ToArray()));

        sb.AppendLine($"{paramName}[{discriminatorIndex}] switch");
        sb.AppendLine("{");
        sb.IncreaseIndent();

        foreach (var (discriminator, subGroup) in subGroups)
        {
            sb.Append("'").AppendEscaped(discriminator).Append("' => ");

            if (subGroup.Length == 1 ||
                discriminatorIndex == length - 1) // Guard against duplicate strings.
            {
                var item = subGroup.First();
                if (discriminatorIndex < length - 1)
                {
                    AppendStringEqualityCheck(sb, item.Target, paramName, paramType, discriminatorIndex + 1, discriminatorIndex: discriminatorIndex);
                    AppendConditionalReturnValue(sb, item, returnType);
                }
                else
                {
                    AppendUnconditionalReturnValue(sb, item, returnType);
                }
            }
            else
            {
                AppendSwitchForStringContent(sb, discriminatorIndex + 1, length, subGroup, paramName, paramType, returnType);
            }

            sb.AppendLine(",");
        }

        sb.AppendLine($"_ => {GetDefaultReturnValue(returnType)}");

        sb.DecreaseIndent();
        sb.Append("}");
    }

    private static void AppendDiscriminatorMatching(SourceBuilder sb, int discriminatorIndex,
        StringMatcherAlternative[] group, string paramName, StringMatcherParamType paramType, StringMatcherReturnType returnType)
    {
        sb.AppendLine($"return {paramName}[{discriminatorIndex}] switch");
        sb.AppendLine("{");
        sb.IncreaseIndent();

        foreach (var item in group)
        {
            sb.Append("'").AppendEscaped(item.Target[discriminatorIndex]).Append("' => ");

            if (group.Length == 1)
            {
                throw new NotImplementedException("Support for single element groups is not implemented.");
            }

            if (item.Target.Length > 1)
            {
                AppendStringEqualityCheck(sb, item.Target, paramName, paramType, startIndex: 0, discriminatorIndex);
                AppendConditionalReturnValue(sb, item, returnType);
            }
            else
            {
                AppendUnconditionalReturnValue(sb, item, returnType);
            }

            sb.AppendLine(",");
        }
        sb.AppendLine($"_ => {GetDefaultReturnValue(returnType)}");

        sb.DecreaseIndent();
        sb.AppendLine("};");
    }

    private static void AppendConditionalReturnValue(SourceBuilder sb, StringMatcherAlternative item, StringMatcherReturnType returnType)
    {
        if (returnType == StringMatcherReturnType.Custom && item.ReturnExpression is not null)
        {
            sb.Append($" ? {item.ReturnExpression} : default");
        }
        else if (returnType != StringMatcherReturnType.Boolean)
        {
            sb.Append(" ? ").Append("\"").AppendEscaped(item.Target).Append("\"").Append(" : null");
        }
    }

    private static void AppendUnconditionalReturnValue(SourceBuilder sb, StringMatcherAlternative item, StringMatcherReturnType returnType)
    {
        if (returnType == StringMatcherReturnType.Custom && item.ReturnExpression is not null)
        {
            sb.Append(item.ReturnExpression);
        }
        else if (returnType != StringMatcherReturnType.Boolean)
        {
            sb.Append("\"").AppendEscaped(item.Target).Append("\"");
        }
        else
        {
            sb.Append("true");
        }
    }

    private static int FindDiscriminatorIndex(StringMatcherAlternative[] grouping, int start)
    {
        var chars = new HashSet<char>();
        for (var i = start; i < grouping[0].Target.Length; i++)
        {
            chars.Clear();
            var allDifferent = true;
            foreach (var item in grouping)
            {
                allDifferent = chars.Add(item.Target[i]);
                if (!allDifferent)
                {
                    break;
                }
            }

            if (allDifferent)
            {
                return i;
            }
        }

        // not found
        return -1;
    }

    private static string GetDefaultReturnValue(StringMatcherReturnType returnType) => returnType switch
    {
        StringMatcherReturnType.Boolean => "false",
        StringMatcherReturnType.String => "null",
        _ => "default",
    };
}

internal enum StringMatcherParamType
{
    String,
    NullableString,
    ReadOnlySpanOfChar
}

internal enum StringMatcherReturnType
{
    Boolean,
    String,
    Custom,
}

internal sealed record class StringMatcherMethod(CSharpTypeName ContainingTypeName, string MethodSignature,
    string ParamName, StringMatcherParamType ParamType, StringMatcherReturnType ReturnType, StringMatcherAlternative[] Alternatives)
{
    private StructuralEqualityWrapper<StringMatcherAlternative[]> _alternatives = Alternatives;
    public StringMatcherAlternative[] Alternatives { get => _alternatives.Target; init => _alternatives = value; }
}

internal record struct StringMatcherAlternative(string Target, string? ReturnExpression)
{
    public static readonly IEqualityComparer<StringMatcherAlternative> TargetComparer = new TargetComparerImpl();

    private sealed class TargetComparerImpl : IEqualityComparer<StringMatcherAlternative>
    {
        public bool Equals(StringMatcherAlternative x, StringMatcherAlternative y) => x.Target == y.Target;
        public int GetHashCode(StringMatcherAlternative obj) => obj.Target.GetHashCode();
    }
}
