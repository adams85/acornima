using System;

namespace Acornima;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/scopeflags.js

// Each scope gets a bitset that may contain these flags
[Flags]
internal enum ScopeFlags
{
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
