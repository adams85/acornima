using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Acornima.SourceGenerators.Helpers;

namespace Acornima.SourceGenerators;

public partial class VisitationBoilerplateGenerator
{
    private sealed class ChildNodesEnumeratorHelperMethodSignatureEqualityComparer : IEqualityComparer<IChildNodesEnumeratorHelperParamInfo[]>
    {
        public bool Equals(IChildNodesEnumeratorHelperParamInfo[] x, IChildNodesEnumeratorHelperParamInfo[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                var paramInfo1 = x[i];
                var paramInfo2 = y[i];

                if (paramInfo1.IsNodeList != paramInfo2.IsNodeList || paramInfo1.IsOptional != paramInfo2.IsOptional)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(IChildNodesEnumeratorHelperParamInfo[] obj)
        {
            var hashCode = 1327044938;
            foreach (var paramInfo in obj)
            {
                hashCode = hashCode * -1521134295 + paramInfo.IsOptional.GetHashCode();
                hashCode = hashCode * -1521134295 + paramInfo.IsNodeList.GetHashCode();
            }
            return hashCode;
        }
    }

    private static void GenerateChildNodesEnumeratorHelpers(SourceBuilder sb, IEnumerable<VisitableNodeInfo> nodeInfos, CancellationToken cancellationToken)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace Acornima.Ast;");
        sb.AppendLine();

        sb.AppendLine("partial struct ChildNodes");
        sb.AppendLine("{");
        sb.IncreaseIndent();

        sb.AppendLine("partial struct Enumerator");
        sb.AppendLine("{");
        sb.IncreaseIndent();

        var methodSignatures = nodeInfos
            .Where(nodeInfo => nodeInfo.GenerateNextChildNodeMethod && nodeInfo.ChildPropertyInfos.Length > 0)
            .Select(nodeInfo => nodeInfo.ChildPropertyInfos)
            .Distinct(new ChildNodesEnumeratorHelperMethodSignatureEqualityComparer())
            .OrderBy(signature => signature.Length)
            .ThenBy(signature => signature.Count(paramInfo => paramInfo.IsOptional))
            .ThenBy(signature => signature
                .Select((paramInfo, index) => (paramInfo, index))
                .Aggregate(0UL, (weight, item) => item.paramInfo.IsOptional ? weight | 1UL << item.index : weight))
            .ThenBy(signature => signature.Count(paramInfo => paramInfo.IsNodeList))
            .ThenBy(signature => signature
                .Select((paramInfo, index) => (paramInfo, index))
                .Aggregate(0UL, (weight, item) => item.paramInfo.IsNodeList ? weight | 1UL << item.index : weight));

        string? methodSeparator = null;
        foreach (var methodSignature in methodSignatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            sb.Append(methodSeparator);
            methodSeparator = Environment.NewLine;

            AppendChildNodesEnumeratorHelperMethod(sb, methodSignature);
        }

        sb.DecreaseIndent();
        sb.AppendLine("}");

        sb.DecreaseIndent();
        sb.AppendLine("}");
    }

    private static void AppendChildNodesEnumeratorHelperMethod(SourceBuilder sb, IChildNodesEnumeratorHelperParamInfo[] methodSignature)
    {
        sb.Append($"internal {NodeCSharpTypeName}? ");
        AppendChildNodesEnumeratorHelperMethodName(sb, methodSignature);

        var isGeneric = false;
        for (var i = 0; i < methodSignature.Length; i++)
        {
            var paramInfo = methodSignature[i];

            if (paramInfo.IsNodeList)
            {
                if (!isGeneric)
                {
                    isGeneric = true;
                    sb.Append("<");
                }
                else
                {
                    sb.Append(", ");
                }

                sb.Append("T").Append(i.ToString(CultureInfo.InvariantCulture));
            }
        }

        if (isGeneric)
        {
            sb.Append(">");
        }

        sb.Append("(");

        string? paramSeparator = null;
        for (var i = 0; i < methodSignature.Length; i++)
        {
            sb.Append(paramSeparator);
            paramSeparator = ", ";

            var paramInfo = methodSignature[i];
            var paramIndex = i.ToString(CultureInfo.InvariantCulture);

            if (paramInfo.IsNodeList)
            {
                sb.Append("in ").Append(NodeListOfTCSharpTypeName, 0, NodeListOfTCSharpTypeName.Length - 2);
                sb.Append("<T").Append(paramIndex);
                if (paramInfo.IsOptional)
                {
                    sb.Append("?");
                }
                sb.Append(">");
            }
            else
            {
                sb.Append(NodeCSharpTypeName);
                if (paramInfo.IsOptional)
                {
                    sb.Append("?");
                }
            }

            sb.Append(" arg").Append(paramIndex);
        }

        sb.Append(")");

        if (isGeneric)
        {
            sb.IncreaseIndent();
            for (var i = 0; i < methodSignature.Length; i++)
            {
                var paramInfo = methodSignature[i];

                if (paramInfo.IsNodeList)
                {
                    sb.AppendLine();
                    sb.Append("where T").Append(i.ToString(CultureInfo.InvariantCulture))
                        .Append(" : ").Append(NodeCSharpTypeName);
                }
            }
            sb.DecreaseIndent();
        }

        sb.AppendLine();

        sb.AppendLine("{");
        sb.IncreaseIndent();

        AppendChildNodesEnumeratorHelperMethodBody(sb, methodSignature);

        sb.DecreaseIndent();
        sb.AppendLine("}");
    }

    private static void AppendChildNodesEnumeratorHelperMethodName(SourceBuilder sb, IChildNodesEnumeratorHelperParamInfo[] methodSignature)
    {
        // We can't use a single overloaded method name as NRT annotations are not part of the method signature.
        // Thus, to disambiguate method resolution, we encode nullability of the parameters into the method name as follows:
        // * In case of a single parameter: 'MoveNext' when parameter/element type is not nullable,
        //   otherwise 'MoveNextNullable'.
        // * In case of multiple parameters: 'MoveNext' when all parameter/element types are not nullable,
        //   otherwise 'MoveNextNullableAt{NULLABLE_PARAM_INDICES_SEPARATED_BY_UNDERSCORE}'.

        sb.Append("MoveNext");
        if (methodSignature.Length == 1)
        {
            if (methodSignature[0].IsOptional)
            {
                sb.Append("Nullable");
            }
        }
        else
        {
            var prefix = "NullableAt";
            for (var i = 0; i < methodSignature.Length; i++)
            {
                var propertyInfo = methodSignature[i];

                if (propertyInfo.IsOptional)
                {
                    sb.Append(prefix);
                    prefix = "_";

                    sb.Append(i.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
    }

    private static void AppendChildNodesEnumeratorHelperMethodBody(SourceBuilder sb, IChildNodesEnumeratorHelperParamInfo[] methodSignature)
    {
        sb.AppendLine("switch (_propertyIndex)");
        sb.AppendLine("{");
        sb.IncreaseIndent();

        var itemVariable = NodeTypeName + "? item";
        for (int i = 0, n = methodSignature.Length; i < n; i++)
        {
            var paramInfo = methodSignature[i];
            var paramIndex = i.ToString(CultureInfo.InvariantCulture);
            var paramName = "arg" + paramIndex;

            sb.AppendLine($"case {paramIndex}:");
            sb.IncreaseIndent();

            if (paramInfo.IsNodeList)
            {
                sb.AppendLine($"if (_listIndex >= {paramName}.Count)");
                sb.AppendLine("{");
                sb.IncreaseIndent();

                sb.AppendLine("_listIndex = 0;");
                sb.AppendLine("_propertyIndex++;");
                sb.AppendLine($"goto {GetJumpLabel(i + 1, n)};");

                sb.DecreaseIndent();
                sb.AppendLine("}");
                sb.AppendLine();
                sb.AppendLine($"{itemVariable} = {paramName}[_listIndex++];");
                sb.AppendLine();

                itemVariable = "item";

                if (paramInfo.IsOptional)
                {
                    sb.AppendLine($"if ({itemVariable} is null)");
                    sb.AppendLine("{");
                    sb.IncreaseIndent();

                    sb.AppendLine($"goto {GetJumpLabel(i, n)};");

                    sb.DecreaseIndent();
                    sb.AppendLine("}");
                    sb.AppendLine();
                }

                sb.AppendLine($"return {itemVariable};");
            }
            else
            {
                sb.AppendLine("_propertyIndex++;");
                sb.AppendLine();

                if (paramInfo.IsOptional)
                {
                    sb.AppendLine($"if ({paramName} is null)");
                    sb.AppendLine("{");
                    sb.IncreaseIndent();

                    sb.AppendLine($"goto {GetJumpLabel(i + 1, n)};");

                    sb.DecreaseIndent();
                    sb.AppendLine("}");
                    sb.AppendLine();
                }

                sb.AppendLine($"return {paramName};");
            }

            sb.DecreaseIndent();
        }

        sb.AppendLine("default:");
        sb.IncreaseIndent();

        sb.AppendLine("return null;");
        sb.DecreaseIndent();

        sb.DecreaseIndent();
        sb.AppendLine("}");

        static string GetJumpLabel(int targetParamIndex, int paramCount)
        {
            return targetParamIndex >= paramCount ? "default" : $"case {targetParamIndex.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
