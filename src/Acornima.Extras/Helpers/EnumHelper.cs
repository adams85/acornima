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
    public static bool HasFlagFast<TEnum>(this TEnum flags, TEnum flag) where TEnum : unmanaged, Enum
    {
        // Based on: https://github.com/dotnet/csharplang/discussions/1993#discussioncomment-104840

        if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<byte>())
        {
            return (Unsafe.As<TEnum, byte>(ref flags) & Unsafe.As<TEnum, byte>(ref flag)) == Unsafe.As<TEnum, byte>(ref flag);
        }
        else if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<ushort>())
        {
            return (Unsafe.As<TEnum, ushort>(ref flags) & Unsafe.As<TEnum, ushort>(ref flag)) == Unsafe.As<TEnum, ushort>(ref flag);
        }
        else if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<uint>())
        {
            return (Unsafe.As<TEnum, uint>(ref flags) & Unsafe.As<TEnum, uint>(ref flag)) == Unsafe.As<TEnum, uint>(ref flag);
        }
        else if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<ulong>())
        {
            return (Unsafe.As<TEnum, ulong>(ref flags) & Unsafe.As<TEnum, ulong>(ref flag)) == Unsafe.As<TEnum, ulong>(ref flag);
        }
        else
        {
            return ThrowInvalidOperationException<bool>();
        }
    }
}
