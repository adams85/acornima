using System;
using System.Runtime.CompilerServices;

namespace Acornima.Helpers;

using static ExceptionHelper;

internal static class EnumHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum ToFlag<TEnum>(this bool value, TEnum flag) where TEnum : unmanaged, Enum =>
        value.ToFlag(flag, default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum ToFlag<TEnum>(this bool value, TEnum flag, TEnum fallbackFlag) where TEnum : unmanaged, Enum =>
        value ? flag : fallbackFlag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasFlagFast<TEnum>(this TEnum flags, TEnum flag) where TEnum : unmanaged, Enum
    {
        // Based on: https://github.com/dotnet/csharplang/discussions/1993#discussioncomment-104840

        // NOTE: We could as well use the Unsafe class to avoid pointer casting.
        // However, AllowUnsafeBlocks needs to be enabled anyway because of SkipLocalsInit, plus
        // Unsafe.As achieves pretty much the same while resulting in code that's harder to read.

        if (sizeof(TEnum) == sizeof(sbyte))
        {
            return (*(sbyte*)&flags & *(sbyte*)&flag) == *(sbyte*)&flag;
        }
        else if (sizeof(TEnum) == sizeof(short))
        {
            return (*(short*)&flags & *(short*)&flag) == *(short*)&flag;
        }
        else if (sizeof(TEnum) == sizeof(int))
        {
            return (*(int*)&flags & *(int*)&flag) == *(int*)&flag;
        }
        else if (sizeof(TEnum) == sizeof(long))
        {
            return (*(long*)&flags & *(long*)&flag) == *(long*)&flag;
        }
        else
        {
            return ThrowInvalidOperationException<bool>();
        }
    }
}
