using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acornima.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;

namespace Acornima.SourceGenerators;

public partial class VisitationBoilerplateGenerator
{
    private static void GenerateAstVisitorClass(SourceBuilder sb, AstVisitorInfo astVisitorInfo, IEnumerable<VisitableNodeInfo> nodeInfos, CancellationToken cancellationToken)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        if (astVisitorInfo.ClassName.Namespace is not null)
        {
            sb.AppendLine($"namespace {astVisitorInfo.ClassName.Namespace};");
            sb.AppendLine();
        }

        sb.Append("partial class ").AppendTypeBareName(astVisitorInfo.ClassName.BareName).AppendLine();
        sb.AppendLine("{");
        sb.IncreaseIndent();

        var visitMethodModifiers =
            astVisitorInfo.VisitorTypeName is { TypeKind: TypeKind.Interface } ? "public virtual" :
            astVisitorInfo is { VisitorTypeName: null, Kind: VisitorKind.Visitor } ? "protected internal virtual" :
            "protected internal override";

        var (visitMethodFilter, visitMethodBodyAppender) = astVisitorInfo.Kind switch
        {
            VisitorKind.Visitor =>
            (
                new Func<string, HashSet<string>, VisitableNodeInfo, AstVisitorInfo, bool>(static (visitMethodName, definedVisitMethods, _, _) =>
                    !definedVisitMethods.Contains(visitMethodName)),
                new AstVisitorVisitMethodBodyAppender(AppendAstVisitorVisitMethodBody)
            ),
            VisitorKind.Rewriter =>
            (
                static (visitMethodName, definedVisitMethods, nodeInfo, astVisitorInfo) =>
                    (astVisitorInfo.BaseVisitorFieldName is not null || nodeInfo.ChildPropertyInfos.Length > 0)
                    && !definedVisitMethods.Contains(visitMethodName),
                AppendAstRewriterVisitMethodBody
            ),
            _ => throw new InvalidOperationException()
        };

        var definedVisitMethods = new HashSet<string>(astVisitorInfo.DefinedVisitMethods);

        string? methodSeparator = null;
        foreach (var nodeInfo in nodeInfos.OrderBy(nodeInfo => nodeInfo.ClassName.TypeName))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var visitMethodName = VisitMethodNamePrefix + nodeInfo.ClassName.TypeName;

            if (visitMethodFilter(visitMethodName, definedVisitMethods, nodeInfo, astVisitorInfo))
            {
                sb.Append(methodSeparator);
                methodSeparator = Environment.NewLine;

                AppendAstVisitorVisitMethod(sb, visitMethodName, visitMethodModifiers, visitMethodBodyAppender, nodeInfo, astVisitorInfo);
            }
        }

        sb.DecreaseIndent();
        sb.AppendLine("}");
    }

    private static void AppendAstVisitorVisitMethod(SourceBuilder sb, string methodName, string methodModifiers, AstVisitorVisitMethodBodyAppender bodyAppender,
        VisitableNodeInfo nodeInfo, AstVisitorInfo astVisitorInfo)
    {
        sb.Append($"{methodModifiers} object? {methodName}(")
            .AppendTypeName(nodeInfo.ClassName).Append(" ").Append(nodeInfo.VariableName).AppendLine(")");

        sb.AppendLine("{");
        sb.IncreaseIndent();

        bodyAppender(sb, methodName, nodeInfo, astVisitorInfo);

        sb.DecreaseIndent();
        sb.AppendLine("}");
    }

    private delegate void AstVisitorVisitMethodBodyAppender(SourceBuilder sb, string methodName, VisitableNodeInfo nodeInfo, AstVisitorInfo astVisitorInfo);

    private static void AppendAstVisitorVisitMethodBody(SourceBuilder sb, string methodName, VisitableNodeInfo nodeInfo, AstVisitorInfo astVisitorInfo)
    {
        var thisVisitorFieldAccess = astVisitorInfo.TargetVisitorFieldName is not null ? astVisitorInfo.TargetVisitorFieldName + "." : null;

        foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
        {
            if (propertyInfo.IsNodeList)
            {
                sb.Append("ref readonly var ").Append(propertyInfo.VariableName).Append(" = ref ")
                    .Append(nodeInfo.VariableName).Append(".").Append(propertyInfo.PropertyName).AppendLine(";");

                sb.Append("for (var i = 0; i < ").Append(propertyInfo.VariableName).AppendLine(".Count; i++)");
                sb.AppendLine("{");
                sb.IncreaseIndent();

                string itemExpression;
                if (propertyInfo.IsOptional)
                {
                    itemExpression = propertyInfo.VariableName + "Item";
                    sb.Append("var ").Append(itemExpression).Append(" = ").Append(propertyInfo.VariableName).AppendLine("[i];");
                    sb.Append("if (").Append(itemExpression).AppendLine(" is not null)");
                    sb.AppendLine("{");
                    sb.IncreaseIndent();
                }
                else
                {
                    itemExpression = propertyInfo.VariableName + "[i]";
                }

                sb.Append(thisVisitorFieldAccess).Append("Visit(").Append(itemExpression).AppendLine(");");

                if (propertyInfo.IsOptional)
                {
                    sb.DecreaseIndent();
                    sb.AppendLine("}");
                }

                sb.DecreaseIndent();
                sb.AppendLine("}");
            }
            else
            {
                if (propertyInfo.IsOptional)
                {
                    sb.Append("if (").Append(nodeInfo.VariableName).Append(".").Append(propertyInfo.PropertyName).AppendLine(" is not null)");
                    sb.AppendLine("{");
                    sb.IncreaseIndent();
                }

                sb.Append(thisVisitorFieldAccess).Append("Visit(").Append(nodeInfo.VariableName).Append(".").Append(propertyInfo.PropertyName).AppendLine(");");

                if (propertyInfo.IsOptional)
                {
                    sb.DecreaseIndent();
                    sb.AppendLine("}");
                }
            }

            sb.AppendLine();
        }

        sb.Append("return ").Append(nodeInfo.VariableName).AppendLine(";");
    }

    private static void AppendAstRewriterVisitMethodBody(SourceBuilder sb, string methodName, VisitableNodeInfo nodeInfo, AstVisitorInfo astVisitorInfo)
    {
        var thisVisitorFieldAccess = astVisitorInfo.TargetVisitorFieldName is not null ? astVisitorInfo.TargetVisitorFieldName + "." : null;
        var baseVisitorFieldAccess = astVisitorInfo.BaseVisitorFieldName is not null ? astVisitorInfo.BaseVisitorFieldName + "." : null;

        if (nodeInfo.ChildPropertyInfos.Length > 0)
        {
            foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
            {
                if (propertyInfo.IsNodeList)
                {
                    sb.Append(thisVisitorFieldAccess).Append("VisitAndConvert(")
                        .Append(nodeInfo.VariableName).Append(".").Append(propertyInfo.PropertyName)
                        .Append(", out var ").Append(propertyInfo.VariableName);

                    if (propertyInfo.IsOptional)
                    {
                        sb.Append(", allowNullElement: true");
                    }
                    sb.AppendLine(");");
                }
                else
                {
                    sb.Append("var ").Append(propertyInfo.VariableName).Append(" = ")
                        .Append(thisVisitorFieldAccess).Append("VisitAndConvert(")
                        .Append(nodeInfo.VariableName).Append(".").Append(propertyInfo.PropertyName);

                    if (propertyInfo.IsOptional)
                    {
                        sb.Append(", allowNull: true");
                    }
                    sb.AppendLine(");");
                }

                sb.AppendLine();
            }

            sb.Append("return ").Append(nodeInfo.VariableName).Append(".UpdateWith(");

            string? argumentSeparator = null;
            foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
            {
                sb.Append(argumentSeparator);
                argumentSeparator = ", ";

                sb.Append(propertyInfo.VariableName);
            }

            sb.AppendLine(");");
        }
        else
        {
            sb.Append("return ").Append(baseVisitorFieldAccess).Append(methodName).Append("(")
                .Append(nodeInfo.VariableName).AppendLine(");");
        }
    }

    private static void GenerateAstVisitorInterface(SourceBuilder sb, AstVisitorInfo astVisitorInfo, IEnumerable<VisitableNodeInfo> nodeInfos, CancellationToken cancellationToken)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        if (astVisitorInfo.VisitorTypeName!.Namespace is not null)
        {
            sb.AppendLine($"namespace {astVisitorInfo.VisitorTypeName.Namespace};");
            sb.AppendLine();
        }

        sb.Append("partial interface ").AppendTypeBareName(astVisitorInfo.VisitorTypeName.BareName).AppendLine();
        sb.AppendLine("{");
        sb.IncreaseIndent();

        string? methodSeparator = null;
        foreach (var nodeInfo in nodeInfos.OrderBy(nodeInfo => nodeInfo.ClassName.TypeName))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var visitMethodName = VisitMethodNamePrefix + nodeInfo.ClassName.TypeName;

            sb.Append(methodSeparator);
            methodSeparator = Environment.NewLine;

            AppendAstVisitorInterfaceVisitMethod(sb, visitMethodName, nodeInfo);
        }

        sb.DecreaseIndent();
        sb.AppendLine("}");
    }

    private static void AppendAstVisitorInterfaceVisitMethod(SourceBuilder sb, string methodName, VisitableNodeInfo nodeInfo)
    {
        sb.Append($"object? {methodName}(")
            .AppendTypeName(nodeInfo.ClassName).Append(" ").Append(nodeInfo.VariableName).AppendLine(");");
    }
}
