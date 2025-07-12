using System;

namespace Acornima;

[Flags]
public enum ExperimentalESFeatures
{
    None,

    /// <summary>
    /// Decorators feature as specified by this <seealso href="https://github.com/tc39/proposal-decorators">proposal</seealso>. Available only when <see cref="ParserOptions.EcmaVersion"/> >= ES2022.
    /// </summary>
    Decorators = 1 << 0,

    /// <summary>
    /// Import Attributes feature as specified by this <seealso href="https://github.com/tc39/proposal-import-attributes">proposal</seealso>. Available only when <see cref="ParserOptions.EcmaVersion"/> >= ES2020.
    /// </summary>
    ImportAttributes = 1 << 1,

    /// <summary>
    /// Duplicate named capturing groups feature as specified by this <seealso href="https://github.com/tc39/proposal-duplicate-named-capturing-groups">proposal</seealso>.
    /// </summary>
    RegExpDuplicateNamedCapturingGroups = 1 << 2,

    /// <summary>
    /// Explicit resource management feature as specified by this <seealso href="https://github.com/tc39/proposal-explicit-resource-management">proposal</seealso>. Available only when <see cref="ParserOptions.EcmaVersion"/> >= ES2017.
    /// </summary>
    ExplicitResourceManagement = 1 << 3,

    All = Decorators | ImportAttributes | RegExpDuplicateNamedCapturingGroups | ExplicitResourceManagement
}
