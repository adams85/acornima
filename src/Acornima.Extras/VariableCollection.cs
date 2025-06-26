using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Acornima.Ast;

namespace Acornima;

[DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
[DebuggerTypeProxy(typeof(DebugView))]
public readonly struct VariableCollection : IReadOnlyCollection<Identifier>
{
    private readonly Identifier[]? _items;

    internal VariableCollection(ReadOnlySpan<Identifier> items, Identifier? additionalItem)
    {
        if (additionalItem is not null)
        {
            if (items.Length == 0)
            {
                _items = new[] { additionalItem };
                return;
            }
            _items = new Identifier[items.Length + 1];
            _items[0] = additionalItem;
            items.CopyTo(_items.AsSpan(1));
        }
        else
        {
            if (items.Length == 0)
            {
                return;
            }
            _items = items.ToArray();
        }

        Array.Sort(_items, NameComparer.Instance);
    }

    public VariableCollection(ReadOnlySpan<Identifier> items)
        : this(items, additionalItem: null) { }

    public VariableCollection(IEnumerable<Identifier> items)
    {
        _items = items.ToArray();
        Array.Sort(_items, NameComparer.Instance);
    }

    public int Count { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _items?.Length ?? 0; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Identifier item)
    {
        return _items is not null && Array.IndexOf(_items, item) >= 0;
    }

    public bool Contains(string name)
    {
        for (int lo = 0, hi = Count - 1; lo <= hi;)
        {
            var i = lo + ((hi - lo) >> 1);
            var order = string.CompareOrdinal(_items![i].Name, name);
            if (order < 0)
            {
                lo = i + 1;
            }
            else if (order > 0)
            {
                hi = i - 1;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new Enumerator(this);

    IEnumerator<Identifier> IEnumerable<Identifier>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<Identifier>
    {
        private readonly Identifier[]? _items;
        private readonly int _count;
        private int _index;

        internal Enumerator(VariableCollection list)
        {
            _items = list._items;
            _count = _items?.Length ?? 0;
            _index = -1;
        }

        public readonly void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            var index = _index + 1;
            if (index < _count)
            {
                _index = index;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _index = -1;
        }

        /// <remarks>
        /// According to the <see href="https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerator-1.current#remarks">specification</see>,
        /// accessing <see cref="Current"/> before calling <see cref="MoveNext"/> or after <see cref="MoveNext"/> returning <see langword="false"/> is undefined behavior.
        /// Thus, to maximize performance, this implementation doesn't do any null or range checks, just let the default exceptions occur on invalid access.
        /// </remarks>
        public readonly Identifier Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _items![_index]; }

        readonly object? IEnumerator.Current => Current;
    }

    private sealed class NameComparer : IComparer<Identifier>
    {
        public static readonly NameComparer Instance = new();

        private NameComparer() { }

        public int Compare(Identifier? x, Identifier? y) => string.CompareOrdinal(x!.Name, y!.Name);
    }

    [DebuggerNonUserCode]
    private sealed class DebugView
    {
        private readonly VariableCollection _collection;

        public DebugView(VariableCollection collection)
        {
            _collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public Identifier[] Items => _collection.ToArray();
    }
}
