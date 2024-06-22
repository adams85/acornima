using System;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

public readonly ref struct OnNodeContext
{
    internal readonly ReadOnlyRef<Scope> _scope;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal OnNodeContext(ReadOnlyRef<Scope> scope, ArrayList<Scope> scopeStack)
    {
        _scope = scope;
        ScopeStack = scopeStack.AsReadOnlySpan();
    }

    public bool HasScope { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => !Unsafe.IsNullRef(ref Unsafe.AsRef(in _scope.Value)); }

    public ref readonly Scope Scope
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ref readonly var scope = ref _scope.Value;
            if (Unsafe.IsNullRef(ref Unsafe.AsRef(in scope)))
            {
                ThrowInvalidOperationException<object>();
            }
            return ref scope;
        }
    }

    public ReadOnlySpan<Scope> ScopeStack { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
