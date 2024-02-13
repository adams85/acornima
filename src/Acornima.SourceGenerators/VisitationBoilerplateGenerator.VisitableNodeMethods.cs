using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acornima.SourceGenerators.Helpers;

namespace Acornima.SourceGenerators;

public partial class VisitationBoilerplateGenerator
{
    private static void GenerateVisitableNodeClasses(SourceBuilder sb, string? @namespace, IEnumerable<VisitableNodeInfo> nodeInfos, CancellationToken cancellationToken)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        if (@namespace is not null)
        {
            sb.AppendLine($"namespace {@namespace};");
            sb.AppendLine();
        }

        string? classSeparator = null;
        foreach (var nodeInfo in nodeInfos.OrderBy(nodeInfo => nodeInfo.ClassName.TypeName))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (nodeInfo.GenerateNextChildNodeMethod || nodeInfo.GenerateAcceptMethod || nodeInfo.GenerateUpdateWithMethod)
            {
                AppendVisitableNodeClass(sb, nodeInfo, ref classSeparator);
            }
        }
    }

    private static void AppendVisitableNodeClass(SourceBuilder sb, VisitableNodeInfo nodeInfo, ref string? classSeparator)
    {
        sb.Append(classSeparator);
        classSeparator = Environment.NewLine;

        sb.AppendLine($"partial class {nodeInfo.ClassName.TypeName}");
        sb.AppendLine("{");
        sb.IncreaseIndent();

        string? methodSeparator = null;

        if (nodeInfo.GenerateNextChildNodeMethod)
        {
            sb.Append(methodSeparator);
            methodSeparator = Environment.NewLine;

            AppendVisitableNodeNextChildNodeMethod(sb, nodeInfo);
        }

        if (nodeInfo.GenerateAcceptMethod)
        {
            sb.Append(methodSeparator);
            methodSeparator = Environment.NewLine;

            AppendVisitableNodeAcceptMethod(sb, nodeInfo);
        }

        if (nodeInfo.GenerateUpdateWithMethod)
        {
            sb.Append(methodSeparator);

            AppendVisitableNodeUpdateWithMethod(sb, nodeInfo);
        }

        sb.DecreaseIndent();
        sb.AppendLine("}");
    }

    private static void AppendVisitableNodeAcceptMethod(SourceBuilder sb, VisitableNodeInfo nodeInfo)
    {
        var @sealed = nodeInfo.SealOverrideMethods ? "sealed " : null;

        sb.Append($"protected internal {@sealed}override object? Accept(");
        (nodeInfo.VisitorTypeName is not null ? sb.AppendTypeName(nodeInfo.VisitorTypeName) : sb.Append(AstVisitorCSharpTypeName))
            .AppendLine($" visitor) => visitor.Visit{nodeInfo.ClassName.TypeName}(this);");
    }

    private static void AppendVisitableNodeNextChildNodeMethod(SourceBuilder sb, VisitableNodeInfo nodeInfo)
    {
        var @sealed = nodeInfo.SealOverrideMethods ? "sealed " : null;

        sb.Append($"internal {@sealed}override {NodeCSharpTypeName}? NextChildNode(ref {ChildNodesEnumeratorCSharpTypeName} enumerator) => ");
        if (nodeInfo.ChildPropertyInfos.Length > 0)
        {
            sb.Append("enumerator.");
            AppendChildNodesEnumeratorHelperMethodName(sb, nodeInfo.ChildPropertyInfos);
            sb.Append("(");

            string? paramSeparator = null;
            foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
            {
                sb.Append(paramSeparator);
                paramSeparator = ", ";

                sb.Append(propertyInfo.PropertyName);
            }

            sb.Append(")");
        }
        else
        {
            sb.Append("null");
        }
        sb.AppendLine(";");
    }

    private static void AppendVisitableNodeUpdateWithMethod(SourceBuilder sb, VisitableNodeInfo nodeInfo)
    {
        sb.Append($"public {nodeInfo.ClassName.TypeName} UpdateWith(");

        string? paramSeparator = null;
        foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
        {
            sb.Append(paramSeparator);
            paramSeparator = ", ";

            if (propertyInfo.IsRefReadonly)
            {
                sb.Append("in ");
            }

            sb.AppendTypeName(propertyInfo.PropertyTypeName).Append(" ").Append(propertyInfo.VariableName);
        }

        sb.AppendLine(")");
        sb.AppendLine("{");
        sb.IncreaseIndent();

        sb.Append("if (");

        string? conditionSeparator = null;
        foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
        {
            sb.Append(conditionSeparator);
            conditionSeparator = " && ";

            if (propertyInfo.IsNodeList)
            {
                sb.Append(propertyInfo.VariableName).Append(".IsSameAs(").Append(propertyInfo.PropertyName).Append(")");
            }
            else
            {
                sb.Append("ReferenceEquals(").Append(propertyInfo.VariableName).Append(", ").Append(propertyInfo.PropertyName).Append(")");
            }
        }
        sb.AppendLine(")");

        sb.AppendLine("{");
        sb.IncreaseIndent();

        sb.AppendLine("return this;");

        sb.DecreaseIndent();
        sb.AppendLine("}");

        sb.AppendLine();

        sb.Append("return Rewrite(");

        paramSeparator = null;
        foreach (var propertyInfo in nodeInfo.ChildPropertyInfos)
        {
            sb.Append(paramSeparator);
            paramSeparator = ", ";

            sb.Append(propertyInfo.VariableName);
        }

        sb.AppendLine(");");

        sb.DecreaseIndent();
        sb.AppendLine("}");
    }
}
