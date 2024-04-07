using System;
using System.Buffers;
using System.Collections.Generic;

namespace Acornima.Jsx.Ast;

public abstract class JsxName : JsxNode
{
    private protected JsxName(JsxNodeType type)
        : base(type) { }

    // Transforms JSX element name to string.

    public string GetQualifiedName()
    {
        // https://github.com/acornjs/acorn-jsx/blob/f5c107b85872230d5016dbb97d71788575cda9c3/index.js > `function getQualifiedJSXName`

        if (this is JsxIdentifier identifier)
        {
            return identifier.Name;
        }
        else if (this is JsxNamespacedName namespacedName)
        {
            return namespacedName.Namespace.Name + ':' + namespacedName.Name.Name;
        }
        else if (this is JsxMemberExpression memberExpression)
        {
            identifier = (memberExpression.Object as JsxIdentifier)!;
            if (identifier is not null)
            {
                return identifier.Name + "." + memberExpression.Property.Name;
            }

            namespacedName = (memberExpression.Object as JsxNamespacedName)!;
            if (namespacedName is not null)
            {
                return namespacedName.Namespace.Name + ':' + namespacedName.Name.Name + "." + memberExpression.Property.Name;
            }

            return GetComplexQualifiedName();
        }

        throw new InvalidOperationException(); // execution should never reach this line
    }

    private string GetComplexQualifiedName()
    {
        // 1. Calculate the required length.

        var length = 0;
        var name = this;
        for (; ; )
        {
            if (name is JsxIdentifier identifier)
            {
                length += identifier.Name.Length;
                break;
            }
            else if (name is JsxNamespacedName namespacedName)
            {
                length += namespacedName.Namespace.Name.Length + 1 + namespacedName.Name.Name.Length;
                break;
            }
            else if (name is JsxMemberExpression memberExpression)
            {
                length += 1 + memberExpression.Property.Name.Length;
                name = memberExpression.Object;
            }
            else
            {
                throw new InvalidOperationException(); // execution should never reach this line
            }
        }

        // 2. Allocate buffer and build the name.

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        return string.Create(length, this, s_buildQualifiedName);
#else
        var chars = new char[length];
        s_buildQualifiedName(chars.AsSpan(), this);
        return new string(chars);
#endif
    }

    private static readonly SpanAction<char, JsxName> s_buildQualifiedName = (span, name) =>
    {
        var index = span.Length;

        for (; ; )
        {
            if (name is JsxIdentifier identifier)
            {
                Write(identifier, span, ref index);
                break;
            }
            else if (name is JsxNamespacedName namespacedName)
            {
                Write(namespacedName.Name, span, ref index);
                span[--index] = ':';
                Write(namespacedName.Namespace, span, ref index);
                break;
            }
            else if (name is JsxMemberExpression memberExpression)
            {
                Write(memberExpression.Property, span, ref index);
                span[--index] = '.';
                name = memberExpression.Object;
            }
            else
            {
                throw new InvalidOperationException(); // execution should never reach this line
            }
        }

        static void Write(JsxIdentifier identifier, Span<char> span, ref int index)
        {
            index -= identifier.Name.Length;
            identifier.Name.AsSpan().CopyTo(span.Slice(index));
        }
    };

    public sealed class ValueEqualityComparer : IEqualityComparer<JsxName>
    {
        public static readonly ValueEqualityComparer Default = new();

        private ValueEqualityComparer() { }

        public bool Equals(JsxName? x, JsxName? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            for (; ; )
            {
                if (x is JsxIdentifier identifier)
                {
                    return y is JsxIdentifier identifier2
                        && identifier.Name == identifier2.Name;
                }
                else if (x is JsxNamespacedName namespacedName)
                {
                    return y is JsxNamespacedName namespacedName2
                        && namespacedName.Name.Name == namespacedName2.Name.Name
                        && namespacedName.Namespace.Name == namespacedName2.Namespace.Name;
                }
                else if (x is JsxMemberExpression memberExpression)
                {
                    if (y is JsxMemberExpression memberExpression2
                        && memberExpression.Property.Name == memberExpression2.Property.Name)
                    {
                        x = memberExpression.Object;
                        y = memberExpression2.Object;
                        continue;
                    }
                }
                return false;
            }
        }

        public int GetHashCode(JsxName obj)
        {
            if (obj is null)
            {
                return 0;
            }

            var hashCode = -2072340198;

            for (; ; )
            {
                if (obj is JsxIdentifier identifier)
                {
                    hashCode = hashCode * -1521134295 + identifier.Name.GetHashCode();
                    break;
                }
                else if (obj is JsxNamespacedName namespacedName)
                {
                    hashCode = hashCode * -1521134295 + namespacedName.Name.Name.GetHashCode();
                    hashCode = hashCode * -1521134295 + namespacedName.Namespace.Name.GetHashCode();
                    break;
                }
                else if (obj is JsxMemberExpression memberExpression)
                {
                    hashCode = hashCode * -1521134295 + memberExpression.Property.Name.GetHashCode();
                    obj = memberExpression.Object;
                }
                else
                {
                    throw new InvalidOperationException(); // execution should never reach this line
                }
            }

            return hashCode;
        }
    }
}
