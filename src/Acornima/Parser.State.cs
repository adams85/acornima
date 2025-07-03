using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Ast;
using Acornima.Helpers;

namespace Acornima;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js

public partial class Parser
{
    internal IsReservedWordDelegate _isReservedWord;
    internal IsReservedWordDelegate _isReservedWordBind;

    private bool _inModule;
    private bool _strict;

    private bool _topLevelAwaitAllowed;
    private ScopeFlags _functionsAsVarInScopeFlags;

    // Used to signify the start of a potential arrow function
    private int _potentialArrowAt;
    private int _forInitPosition;

    // Positions to delayed-check that yield/await does not exist in default parameters.
    private int _yieldPosition, _awaitPosition, _awaitIdentifierPosition;

    // Labels in scope.
    private ArrayList<Label> _labels;

    private HashSet<string>? _exports;

    // Thus-far undefined exports.
    private Dictionary<string, int>? _undefinedExports;

    // Scope tracking for duplicate variable names
    private int _scopeId;
    private ArrayList<Scope> _scopeStack;

    // The following functions keep track of declared variables in the current scope in order to detect duplicate variable names.

    // The stack of private names.
    // Each element has two properties: 'declared' and 'used'.
    // When it exited from the outermost class definition, all used private names must be declared.
    private ArrayList<PrivateNameStatus> _privateNameStack;

    private ArrayList<Decorator> _decorators;

    private int _bindingPatternDepth, _recursionDepth;

    internal void Reset(string input, int start, int length, SourceType sourceType, string? sourceFile, bool strict)
    {
        _tokenizer.ResetInternal(input, start, length, sourceType, sourceFile, trackRegExpContext: _options.OnToken is not null);

        _inModule = _tokenizer._inModule;
        _strict = _tokenizer._strict || strict;

        var ecmaVersion = _options.EcmaVersion;

        if (_inModule)
        {
            _topLevelAwaitAllowed = ecmaVersion >= EcmaVersion.ES13;
            _functionsAsVarInScopeFlags = ScopeFlags.Function;
        }
        else
        {
            _topLevelAwaitAllowed = sourceType == SourceType.Unknown && ecmaVersion >= EcmaVersion.ES8;
            // The spec says:
            // > At the top level of a function, or script, function declarations are
            // > treated like var declarations rather than like lexical declarations.
            _functionsAsVarInScopeFlags = ScopeFlags.Function | ScopeFlags.Top;
        }

        if (_exports is not null)
        {
            _exports.Clear();
        }
        else if (_inModule || _options._allowImportExportEverywhere)
        {
            _exports = new HashSet<string>();
        }

        if (_undefinedExports is not null)
        {
            _undefinedExports.Clear();
        }
        else if (_inModule)
        {
            _undefinedExports = new Dictionary<string, int>();
        }

        var allowReserved = _options._allowReserved;
        if (allowReserved == AllowReservedOption.Default)
        {
            allowReserved = ecmaVersion >= EcmaVersion.ES5 ? AllowReservedOption.No : AllowReservedOption.Yes;
        }

        GetIsReservedWord(_inModule, ecmaVersion, allowReserved, out _isReservedWord, out _isReservedWordBind);

        _potentialArrowAt = -1;
        _forInitPosition = _yieldPosition = _awaitPosition = _awaitIdentifierPosition = 0;

        _labels.Clear();

        _scopeId = 0;
        _scopeStack.Clear();
        EnterScope(ScopeFlags.Top);

        _privateNameStack.Clear();

        _decorators.Clear();

        _recursionDepth = _bindingPatternDepth = 0;
    }

    private void ReleaseLargeBuffers()
    {
        _decorators.Clear();
        if (_decorators.Capacity > 64)
        {
            _decorators.Capacity = 64;
        }

        _labels.Clear();
        if (_labels.Capacity > 64)
        {
            _labels.Capacity = 64;
        }

        _exports = null;
        _undefinedExports = null;

        _scopeStack.Clear();
        if (_scopeStack.Capacity > 64)
        {
            _scopeStack.Capacity = 64;
        }

        _privateNameStack.Clear();
        if (_privateNameStack.Capacity > 64)
        {
            _privateNameStack.Capacity = 64;
        }

        _tokenizer.ReleaseLargeBuffers();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ScopeFlags FunctionFlags(bool async, bool generator)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scopeflags.js > `export function functionFlags`

        var flags = ScopeFlags.Function;
        if (async)
        {
            flags |= ScopeFlags.Async;
        }
        if (generator)
        {
            flags |= ScopeFlags.Generator;
        }
        return flags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnterScope(ScopeFlags flags)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.enterScope = function`

        int currentVarScopeIndex, currentThisScopeIndex;
        if ((flags & ScopeFlags.Var) != 0)
        {
            currentVarScopeIndex = _scopeStack.Count;
            currentThisScopeIndex = (flags & ScopeFlags.Arrow) == 0 ? _scopeStack.Count : _scopeStack.PeekRef()._currentThisScopeIndex;
        }
        else
        {
            ref readonly var currentScope = ref _scopeStack.PeekRef();
            currentVarScopeIndex = currentScope._currentVarScopeIndex;
            currentThisScopeIndex = currentScope._currentThisScopeIndex;
        }

        _scopeStack.PushRef().Reset(_scopeId++, flags, currentVarScopeIndex, currentThisScopeIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlyRef<Scope> ExitScope()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.exitScope = function`

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return new ReadOnlyRef<Scope>(ref _scopeStack.PopRef());
#else
        var scope = Scope.GetScopeRef(_scopeStack, _scopeStack.Count - 1);
        _scopeStack.PopRef();
        return scope;
#endif
    }

    private ref Scope CurrentScope
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.currentScope = function`

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _scopeStack.PeekRef();
    }

    private ref Scope CurrentVarScope
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.currentVarScope = function`

        // NOTE: to improve performance, we calculate and store the index of the current var scope on the fly
        // instead of looking it up at every call as it's done in acornjs (see also `EnterScope`).

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _scopeStack.GetItemRef(_scopeStack.PeekRef()._currentVarScopeIndex);
    }

    // Could be useful for `this`, `new.target`, `super()`, `super.property`, and `super[property]`.
    private ref Scope CurrentThisScope
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.currentThisScope = function`

        // NOTE: to improve performance, we calculate and store the index of the current this scope on the fly
        // instead of looking it up at every call as it's done in acornjs (see also `EnterScope`).

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _scopeStack.GetItemRef(_scopeStack.PeekRef()._currentThisScopeIndex);
    }

    private bool InFunction
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get inFunction`

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (CurrentVarScope._flags & ScopeFlags.Function) != 0;
    }

    private bool InGenerator
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get inGenerator`

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (CurrentVarScope._flags & (ScopeFlags.Generator | ScopeFlags.InClassFieldInit)) == ScopeFlags.Generator;
    }

    private bool InAsync
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get inAsync`

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (CurrentVarScope._flags & (ScopeFlags.Async | ScopeFlags.InClassFieldInit)) == ScopeFlags.Async;
    }

    private bool CanAwait
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get canAwait`

        get
        {
            ref var scope = ref CurrentVarScope;
            if ((scope._flags & ScopeFlags.Function) != 0)
            {
                return (scope._flags & (ScopeFlags.Async | ScopeFlags.InClassFieldInit)) == ScopeFlags.Async;
            }
            if ((scope._flags & ScopeFlags.Top) != 0)
            {
                return (_options._allowAwaitOutsideFunction || _topLevelAwaitAllowed) && (scope._flags & ScopeFlags.InClassFieldInit) == 0;
            }
            return false;
        }
    }

    private bool AllowSuper
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get allowSuper`

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _options._allowSuperOutsideMethod || (CurrentThisScope._flags & (ScopeFlags.Super | ScopeFlags.InClassFieldInit)) != 0;
    }

    private bool AllowDirectSuper
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get allowDirectSuper`

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (CurrentThisScope._flags & ScopeFlags.DirectSuper) != 0;
    }

    private bool TreatFunctionsAsVar
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get treatFunctionsAsVar`
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.treatFunctionsAsVarInScope = function`

        // NOTE: to improve performance, we calculate and store this flag on the fly
        // instead of recalculating it at every call as it's done in acornjs.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (CurrentScope._flags & _functionsAsVarInScopeFlags) != 0;
    }

    private bool AllowNewDotTarget
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get allowNewDotTarget`

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _options._allowNewTargetOutsideFunction
            || (CurrentThisScope._flags & (ScopeFlags.Function | ScopeFlags.ClassStaticBlock | ScopeFlags.InClassFieldInit)) != 0;
    }

    private bool AllowUsing
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get allowUsing`

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ScopeFlags flags = CurrentVarScope._flags;
            return (flags & ScopeFlags.Switch) == 0 && (_inModule || (flags & ScopeFlags.Top) == 0 || _options._allowTopLevelUsing);
        }
    }

    private bool InClassStaticBlock
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get inClassStaticBlock`

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (CurrentVarScope._flags & ScopeFlags.ClassStaticBlock) != 0;
    }

    private bool InClassFieldInit
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (CurrentThisScope._flags & ScopeFlags.InClassFieldInit) != 0;
    }

    private enum LabelKind : byte
    {
        None,
        Loop,
        Switch
    }

#if DEBUG
    [DebuggerDisplay($"{{{nameof(Kind)}}}, {nameof(Name)} = {{{nameof(Name)}}}")]
#endif
    private struct Label
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset(LabelKind kind, string? name = null, int statementStart = 0)
        {
            Kind = kind;
            Name = name;
            StatementStart = statementStart;
        }

        public LabelKind Kind;
        public string? Name;
        public int StatementStart;
    }

    private struct PrivateNameStatus
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            Declared?.Clear();
            Used.Clear();
        }

        public Dictionary<string, int>? Declared;
        public ArrayList<PrivateIdentifier> Used;
    }
}
