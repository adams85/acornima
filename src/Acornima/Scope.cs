using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

using static ExceptionHelper;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `class Scope`

/// <summary>
/// Stores variable scope information.
/// </summary>
/// <remarks>
/// Scopes are created for exactly the following types of AST nodes:
/// <list type="bullet">
/// <item><see cref="Program"/></item>
/// <item><see cref="IFunction"/> (be aware that no separate scopes are created for the parameter list and the body of the function, see also <seealso cref="VarParamCount"/>)</item>
/// <item><see cref="IClass"/></item>
/// <item><see cref="StaticBlock"/></item>
/// <item><see cref="NestedBlockStatement"/></item>
/// <item><see cref="CatchClause"/> (be aware that no separate scopes are created for the parameter list and the body of the catch clause, see also <seealso cref="LexicalParamCount"/>)</item>
/// <item><see cref="ForStatement"/>, <see cref="ForInStatement"/>, <see cref="ForOfStatement"/> (a separate scope is created for the initialization part of the statement)</item>
/// <item><see cref="SwitchStatement"/> (a scope is created for the body of the statement; be aware that the discriminant expression is not part of this scope)</item>
/// </list>
/// </remarks>
public struct Scope
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlyRef<Scope> GetScopeRef(ArrayList<Scope> scopeStack, int index)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return new ReadOnlyRef<Scope>(ref scopeStack.GetItemRef(index));
#else
        return new ReadOnlyRef<Scope>(scopeStack.AsReadOnlySpan(), index);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Reset(int id, ScopeFlags flags, int currentVarScopeIndex, int currentThisScopeIndex)
    {
        _id = id;
        _flags = flags;
        _currentVarScopeIndex = currentVarScopeIndex;
        _currentThisScopeIndex = currentThisScopeIndex;
        _var.Reset();
        _lexical.Reset();
        _functions.Reset();
    }

    internal int _id;
    /// <remarks>
    /// It is guaranteed that <see cref="Id"/> values are assigned sequentially, starting with zero (assigned to the root scope).
    /// </remarks>
    public readonly int Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _id; }

    internal ScopeFlags _flags;

    internal int _currentVarScopeIndex;
    public readonly int CurrentVarScopeIndex { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _currentVarScopeIndex; }

    internal int _currentThisScopeIndex;
    public readonly int CurrentThisScopeIndex { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _currentThisScopeIndex; }

    internal VariableList _var;

    /// <summary>
    /// A list of var-declared variables in the current lexical scope. In the case of function scopes, also includes parameters (listed at the beginning of the span).
    /// </summary>
    /// <remarks>
    /// Variables declared in a nested statement block are hoisted, meaning that the variable identifier will be included in the <see cref="VarVariables"/> span of
    /// the parent scopes, up to the root var scope (i.e. the scope introduced by the closest <see cref="Program"/>, <see cref="IFunction"/> or <see cref="StaticBlock"/> node).
    /// </remarks>
    public readonly ReadOnlySpan<Identifier> VarVariables { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _var.AsReadOnlySpan(); }

    /// <summary>
    /// The number of parameter names at the beginning of the <see cref="VarVariables"/> span.
    /// </summary>
    public readonly int VarParamCount { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _var.ParamCount; }

    internal VariableList _lexical;

    /// <summary>
    /// A list of lexically-declared variables in the current lexical scope. In the case of catch clause scopes, also includes parameters (listed at the beginning of the span).
    /// </summary>
    public readonly ReadOnlySpan<Identifier> LexicalVariables { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _lexical.AsReadOnlySpan(); }

    /// <summary>
    /// The number of parameter names at the beginning of the <see cref="LexicalVariables"/> span.
    /// </summary>
    public readonly int LexicalParamCount { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _lexical.ParamCount; }

    internal VariableList _functions;

    /// <summary>
    /// A list of lexically-declared <see cref="FunctionDeclaration"/>s in the current lexical scope.
    /// </summary>
    /// <remarks>
    /// Functions declared in a nested statement block are not hoisted, meaning that the function identifiers will not be included in the <see cref="Functions"/> span of
    /// the parent scopes. (This is relevant only in non-strict contexts as strict mode prevents functions from being hoisted out of the scope in they are declared.)
    /// </remarks>
    public readonly ReadOnlySpan<Identifier> Functions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _functions.AsReadOnlySpan(); }

    // This is a heavily slimmed down version of ArrayList<T> for collecting variable and parameter names.
    // Ideally, we'd just use an ArrayList<Identifier> for this purpose. However, we also need to store an extra 32-bit integer
    // that indicates the number of parameters in the case of function and catch clause scopes.
    // The layout of the Scope struct is so unfortunate that adding the two extra int fields to it would increase its size by 8 bytes
    // while there'd be 3*4=12 bytes of wasted space as ArrayLists need only 12 bytes but are padded to 16 bytes on x64 architectures.
    // Unfortunately, there seems to be no cleaner and safer way to utilize that wasted space than duplicating some code of ArrayList<T>.
#if DEBUG
    [DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
    [DebuggerTypeProxy(typeof(DebugView))]
#endif
    internal struct VariableList
    {
        private Identifier[]? _items;
        private int _count;

        public int ParamCount;

        public readonly string this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // Following trick can reduce the range check by one
                if ((uint)index >= (uint)_count)
                {
                    return ThrowIndexOutOfRangeException<string>();
                }

                return _items![index].Name;
            }
        }

        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public void Add(Identifier item)
        {
            var capacity = _items?.Length ?? 0;

            if (_count == capacity)
            {
                Array.Resize(ref _items, Math.Max(checked((int)ArrayList<Identifier>.GrowCapacity(capacity)), ArrayList<Identifier>.MinAllocatedCount));
            }

            Debug.Assert(_items is not null);
            _items![_count++] = item;
        }

        public void Reset()
        {
            if (_count != 0)
            {
                Array.Clear(_items!, 0, _count);
                _count = 0;
            }
            ParamCount = 0;
        }

        public readonly bool Contains(string name)
        {
            for (var i = 0; i < _count; i++)
            {
                if (_items![i].Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        /// <remarks>
        /// WARNING: Items should not be added or removed from the <see cref="VariableList"/> while the returned <see cref="ReadOnlySpan{T}"/> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly ReadOnlySpan<Identifier> AsReadOnlySpan()
        {
            return new ReadOnlySpan<Identifier>(_items, 0, _count);
        }

#if DEBUG
        public readonly Identifier[] ToArray()
        {
            if (_count == 0)
            {
                return Array.Empty<Identifier>();
            }

            var array = new Identifier[_count];
            Array.Copy(_items!, 0, array, 0, _count);
            return array;
        }

        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly VariableList _list;

            public DebugView(VariableList list)
            {
                _list = list;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Identifier[] Items => _list.ToArray();
        }
#endif
    }
}
