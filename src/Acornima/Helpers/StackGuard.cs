using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Acornima.Helpers;

// Based on: https://github.com/dotnet/roslyn/blob/VSCode-CSharp-2.18.15/src/Compilers/Core/Portable/InternalUtilities/StackGuard.cs

internal static class StackGuard
{
    public const int MaxUncheckedRecursionDepth = 20;

#if DEBUG
    [DebuggerStepThrough]
#endif
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureSufficientExecutionStack(int recursionDepth)
    {
        if (recursionDepth > MaxUncheckedRecursionDepth)
        {
            // Makes sure that
            // * on 32-bit platforms at least 64 kB
            // * on 64-bit platforms at least 128 kB
            // of stack space is available.
            // See also: https://github.com/dotnet/runtime/blob/v8.0.2/src/coreclr/vm/threads.cpp#L6352
            RuntimeHelpers.EnsureSufficientExecutionStack();
        }
    }
}
