using System;
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
        _tokenizer._stringPool = default;
        _tokenizerOptions._errorHandler.Reset();

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
            currentThisScopeIndex = (flags & ScopeFlags.Arrow) == 0 ? _scopeStack.Count : _scopeStack.PeekRef().CurrentThisScopeIndex;
        }
        else
        {
            ref readonly var currentScope = ref _scopeStack.PeekRef();
            currentVarScopeIndex = currentScope.CurrentVarScopeIndex;
            currentThisScopeIndex = currentScope.CurrentThisScopeIndex;
        }

        _scopeStack.Push(new Scope(flags, currentVarScopeIndex, currentThisScopeIndex));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExitScope()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.exitScope = function`

        _scopeStack.Pop();
    }

    // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.currentScope = function`
    private ref Scope CurrentScope { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _scopeStack.PeekRef(); }

    private ref Scope CurrentVarScope(out int index)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.currentVarScope = function`

        // NOTE: to improve performance, we calculate and store the index of the current var scope on the fly
        // instead of looking it up at every call as it's done in acornjs (see also `EnterScope`).

        index = _scopeStack.PeekRef().CurrentVarScopeIndex;
        return ref _scopeStack.GetItemRef(index);
    }

    // Could be useful for `this`, `new.target`, `super()`, `super.property`, and `super[property]`.
    private ref Scope CurrentThisScope(out int index)
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.currentThisScope = function`

        // NOTE: to improve performance, we calculate and store the index of the current this scope on the fly
        // instead of looking it up at every call as it's done in acornjs (see also `EnterScope`).

        index = _scopeStack.PeekRef().CurrentThisScopeIndex;
        return ref _scopeStack.GetItemRef(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool InFunction()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get inFunction`

        return (CurrentVarScope(out _).Flags & ScopeFlags.Function) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool InGenerator()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get inGenerator`

        return (CurrentVarScope(out _).Flags & (ScopeFlags.Generator | ScopeFlags.InClassFieldInit)) == ScopeFlags.Generator;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool InAsync()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get inAsync`

        return (CurrentVarScope(out _).Flags & (ScopeFlags.Async | ScopeFlags.InClassFieldInit)) == ScopeFlags.Async;
    }

    private bool CanAwait()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get canAwait`

        for (var i = _scopeStack.Count - 1; i >= 0; i--)
        {
            ref readonly var scope = ref _scopeStack.GetItemRef(i);

            if ((scope.Flags & (ScopeFlags.InClassFieldInit | ScopeFlags.ClassStaticBlock)) != 0)
            {
                return false;
            }

            if ((scope.Flags & ScopeFlags.Function) != 0)
            {
                return (scope.Flags & ScopeFlags.Async) != 0;
            }
        }

        return _options._allowAwaitOutsideFunction || _topLevelAwaitAllowed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AllowSuper()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get allowSuper`

        return _options._allowSuperOutsideMethod || (CurrentThisScope(out _).Flags & (ScopeFlags.Super | ScopeFlags.InClassFieldInit)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AllowDirectSuper()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get allowDirectSuper`

        return (CurrentThisScope(out _).Flags & ScopeFlags.DirectSuper) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TreatFunctionsAsVar()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get treatFunctionsAsVar`
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `pp.treatFunctionsAsVarInScope = function`

        // NOTE: to improve performance, we calculate and store this flag on the fly
        // instead of recalculating it at every call as it's done in acornjs.

        return (CurrentScope.Flags & _functionsAsVarInScopeFlags) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AllowNewDotTarget()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get allowNewDotTarget`

        return (CurrentThisScope(out _).Flags & (ScopeFlags.Function | ScopeFlags.ClassStaticBlock | ScopeFlags.InClassFieldInit)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool InClassStaticBlock()
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/state.js > `get inClassStaticBlock`

        return (CurrentVarScope(out _).Flags & ScopeFlags.ClassStaticBlock) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool InClassFieldInit()
    {
        return (CurrentThisScope(out _).Flags & ScopeFlags.InClassFieldInit) != 0;
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
        public Label(LabelKind kind, string? name = null, int statementStart = 0)
        {
            Kind = kind;
            Name = name;
            StatementStart = statementStart;
        }

        public LabelKind Kind;
        public string? Name;
        public int StatementStart;
    }

    // Each scope gets a bitset that may contain these flags
    [Flags]
    private enum ScopeFlags : ushort
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scopeflags.js

        None = 0,
        Top = 1 << 0,
        Function = 1 << 1,
        Async = 1 << 2,
        Generator = 1 << 3,
        Arrow = 1 << 4,
        SimpleCatch = 1 << 5,
        Super = 1 << 6,
        DirectSuper = 1 << 7,
        ClassStaticBlock = 1 << 8,

        Var = Top | Function | ClassStaticBlock,

        // A switch to disallow the identifier reference 'arguments'
        InClassFieldInit = 1 << 15,
    }

    private struct Scope
    {
        // https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scope.js > `class Scope`

        public Scope(ScopeFlags flags, int currentVarScopeIndex, int currentThisScopeIndex)
        {
            Flags = flags;
            CurrentVarScopeIndex = currentVarScopeIndex;
            CurrentThisScopeIndex = currentThisScopeIndex;
        }

        public ScopeFlags Flags;

        public readonly int CurrentVarScopeIndex;
        public readonly int CurrentThisScopeIndex;

        /// <summary>
        /// A list of var-declared names in the current lexical scope.
        /// </summary>
        public ArrayList<string> Var;

        /// <summary>
        /// A list of lexically-declared names in the current lexical scope.
        /// </summary>
        public ArrayList<string> Lexical;

        /// <summary>
        /// A list of lexically-declared FunctionDeclaration names in the current lexical scope.
        /// </summary>
        public ArrayList<string> Functions;
    }

    private struct PrivateNameStatus
    {
        public Dictionary<string, int>? Declared;
        public ArrayList<PrivateIdentifier> Used;
    }
}
