using System;

namespace Acornima.Ast;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class VisitableNodeAttribute : Attribute
{
    public Type? VisitorType { get; set; }
    public string[]? ChildProperties { get; set; }
    public bool SealOverrideMethods { get; set; }
}
