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
    /// Source phase imports feature as specified by this <seealso href="https://github.com/tc39/proposal-source-phase-imports">proposal</seealso>. Available only when <see cref="ParserOptions.EcmaVersion"/> >= ES2020.
    /// </summary>
    SourcePhaseImports = 1 << 5,

    /// <summary>
    /// Deferring module evaluation feature as specified by this <seealso href="https://github.com/tc39/proposal-defer-import-eval">proposal</seealso>. Available only when <see cref="ParserOptions.EcmaVersion"/> >= ES2020.
    /// </summary>
    DeferImportEvaluation = 1 << 6,

    All = Decorators
        | SourcePhaseImports
        | DeferImportEvaluation
}
