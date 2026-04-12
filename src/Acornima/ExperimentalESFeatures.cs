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
    [Obsolete($"This language feature is part of the standard since ES2025, so it will be removed from {nameof(ExperimentalESFeatures)} in the next major version.")]
    ImportAttributes = 1 << 1,

    /// <summary>
    /// Duplicate named capturing groups feature as specified by this <seealso href="https://github.com/tc39/proposal-duplicate-named-capturing-groups">proposal</seealso>.
    /// </summary>
    [Obsolete($"This language feature is part of the standard since ES2025, so it will be removed from {nameof(ExperimentalESFeatures)} in the next major version.")]
    RegExpDuplicateNamedCapturingGroups = 1 << 2,

    /// <summary>
    /// Explicit resource management feature as specified by this <seealso href="https://github.com/tc39/proposal-explicit-resource-management">proposal</seealso>. Available only when <see cref="ParserOptions.EcmaVersion"/> >= ES2017.
    /// </summary>
    [Obsolete($"This language feature is part of the standard since ES2026, so it will be removed from {nameof(ExperimentalESFeatures)} in the next major version.")]
    ExplicitResourceManagement = 1 << 3,

    /// <summary>
    /// Regular expression modifiers feature as specified by this <seealso href="https://github.com/tc39/proposal-regexp-modifiers">proposal</seealso>. Available only when <see cref="ParserOptions.EcmaVersion"/> >= ES2018.
    /// </summary>
    [Obsolete($"This language feature is part of the standard since ES2025, so it will be removed from {nameof(ExperimentalESFeatures)} in the next major version.")]
    RegExpModifiers = 1 << 4,

    /// <summary>
    /// Source phase imports feature as specified by this <seealso href="https://github.com/tc39/proposal-source-phase-imports">proposal</seealso>. Available only when <see cref="ParserOptions.EcmaVersion"/> >= ES2020.
    /// </summary>
    SourcePhaseImports = 1 << 5,

    /// <summary>
    /// Import defer feature as specified by this <seealso href="https://github.com/tc39/proposal-defer-import-eval">proposal</seealso>. Available only when <see cref="ParserOptions.EcmaVersion"/> >= ES2020.
    /// </summary>
    ImportDefer = 1 << 6,

    All = Decorators
#pragma warning disable CS0618 // Type or member is obsolete
        | ImportAttributes
        | RegExpDuplicateNamedCapturingGroups
        | ExplicitResourceManagement
        | RegExpModifiers
#pragma warning restore CS0618 // Type or member is obsolete
        | SourcePhaseImports
        | ImportDefer
}
