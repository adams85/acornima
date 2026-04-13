using System;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

public readonly ref struct OnNodeContext
{
    private readonly ITokenizer _tokenizer;
    internal readonly ReadOnlyRef<Scope> _scope;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal OnNodeContext(ITokenizer tokenizer, ReadOnlyRef<Scope> scope, ArrayList<Scope> scopeStack)
    {
        _tokenizer = tokenizer;
        _scope = scope;
        ScopeStack = scopeStack.AsReadOnlySpan();
    }

    public string Input { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _tokenizer.Input; }
    public Range InputRange { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _tokenizer.Range; }
    public SourceType SourceType { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _tokenizer.SourceType; }
    public string? SourceFile { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _tokenizer.SourceFile; }

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
