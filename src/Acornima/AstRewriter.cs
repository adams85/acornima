using System;
using System.Collections.Generic;
using Acornima.Ast;

namespace Acornima;

[AutoGeneratedAstVisitor]
public partial class AstRewriter : AstVisitor
{
    public virtual T VisitAndConvert<T>(T node, bool allowNull = false)
        where T : Node?
    {
        if (node is null)
        {
            return allowNull ? null! : throw new ArgumentNullException(nameof(node));
        }

        return Visit(node) switch
        {
            T convertedNode => convertedNode,
            null when allowNull => null!,
            null => throw MustRewriteToSameNodeNonNullable(typeof(T)),
            _ => throw (allowNull ? MustRewriteToSameNodeNullable(typeof(T)) : MustRewriteToSameNodeNonNullable(typeof(T)))
        };

        static Exception MustRewriteToSameNodeNonNullable(Type nodeType) =>
            new InvalidOperationException(string.Format(null, ExceptionMessages.MustRewriteToSameNodeNonNullable, nodeType));

        static Exception MustRewriteToSameNodeNullable(Type nodeType) =>
            new InvalidOperationException(string.Format(null, ExceptionMessages.MustRewriteToSameNodeNullable, nodeType));
    }

    public virtual bool VisitAndConvert<T>(in NodeList<T> nodes, out NodeList<T> newNodes, bool allowNullElement = false)
        where T : Node?
    {
        List<T>? newNodeList = null;
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];

            var newNode = VisitAndConvert(node, allowNull: allowNullElement);

            if (newNodeList is not null)
            {
                newNodeList.Add(newNode);
            }
            else if (!ReferenceEquals(newNode, node))
            {
                newNodeList = new List<T>();
                for (var j = 0; j < i; j++)
                {
                    newNodeList.Add(nodes[j]);
                }

                newNodeList.Add(newNode);
            }
        }

        if (newNodeList is not null)
        {
            newNodes = new NodeList<T>(newNodeList);
            return true;
        }

        newNodes = nodes;
        return false;
    }

    protected internal override object? VisitAssignmentProperty(AssignmentProperty node)
    {
        Expression? key;
        Node value;

        if (node.Shorthand)
        {
            value = VisitAndConvert(node.Value);
            key = (value is AssignmentPattern assignmentPattern
                ? assignmentPattern.Left
                : value).As<Identifier>();
        }
        else
        {
            key = VisitAndConvert(node.Key);
            value = VisitAndConvert(node.Value);
        }

        return node.UpdateWith(key, value);
    }

    protected internal override object? VisitExportSpecifier(ExportSpecifier node)
    {
        Expression local;
        Expression exported;

        if (ReferenceEquals(node.Exported, node.Local))
        {
            exported = local = VisitAndConvert(node.Local);
        }
        else
        {
            local = VisitAndConvert(node.Local);
            exported = VisitAndConvert(node.Exported);
        }

        return node.UpdateWith(local, exported);
    }

    protected internal override object? VisitImportSpecifier(ImportSpecifier node)
    {
        Expression imported;
        Identifier local;

        if (ReferenceEquals(node.Imported, node.Local))
        {
            imported = local = VisitAndConvert(node.Local);
        }
        else
        {
            imported = VisitAndConvert(node.Imported);
            local = VisitAndConvert(node.Local);
        }

        return node.UpdateWith(imported, local);
    }

    protected internal override object? VisitObjectProperty(ObjectProperty node)
    {
        Expression? key;
        Node value;

        if (node.Shorthand)
        {
            value = VisitAndConvert(node.Value);
            key = value.As<Identifier>();
        }
        else
        {
            key = VisitAndConvert(node.Key);
            value = VisitAndConvert(node.Value);
        }

        return node.UpdateWith(key, value);
    }
}
