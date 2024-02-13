using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima.Ast;

using static Helpers.ExceptionHelper;

[DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
[DebuggerTypeProxy(typeof(NodeList<>.DebugView))]
public readonly struct NodeList<T> : IReadOnlyList<T> where T : Node?
{
    private readonly T[]? _items;
    private readonly int _count;

    internal NodeList(ICollection<T> collection)
    {
        if (collection is null)
        {
            ThrowArgumentNullException();
        }

        _count = collection!.Count;
        if (_count > 0)
        {
            _items = new T[_count];
            collection.CopyTo(_items, 0);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowArgumentNullException()
        {
            ThrowArgumentNullException<T>(nameof(collection));
        }
    }

    /// <remarks>
    /// WARNING: Expects ownership of the array.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal NodeList(T[]? items, int count)
    {
        Debug.Assert(count <= (items?.Length ?? 0));

        _items = items;
        _count = count;
    }

    /// <remarks>
    /// WARNING: Expects ownership of the array.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal NodeList(params T[] items) : this(items, items.Length) { }

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            // Following trick can reduce the range check by one
            if ((uint)index < (uint)_count)
            {
                return _items![index];
            }

            return ThrowIndexOutOfRangeException<T>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsSameAs(in NodeList<T> other) => ReferenceEquals(_items, other._items);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NodeList<Node?> AsNodes()
    {
        return new NodeList<Node?>(_items /* conversion by co-variance! */, _count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NodeList<TTo> As<TTo>() where TTo : Node?
    {
        return new NodeList<TTo>((TTo[]?)(object?)_items, _count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsSpan() => new ReadOnlySpan<T>(_items, 0, _count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyMemory<T> AsMemory() => new ReadOnlyMemory<T>(_items, 0, _count);

    public T[] ToArray()
    {
        if (_count == 0)
        {
            return Array.Empty<T>();
        }

        var array = new T[_count];
        Array.Copy(_items!, 0, array, 0, _count);
        return array;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(_items, Count);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <remarks>
    /// This implementation does not detect changes to the list
    /// during iteration and therefore the behaviour is undefined
    /// under those conditions.
    /// </remarks>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly T[]? _items; // Usually null when count is zero
        private readonly int _count;

        private int _index;
        private T? _current;

        internal Enumerator(T[]? items, int count) : this()
        {
            _index = 0;
            _items = items;
            _count = count;
        }

        public readonly void Dispose()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (_index < _count)
            {
                _current = _items![_index];
                _index++;
                return true;
            }

            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            _index = _count + 1;
            _current = default;
            return false;
        }

        public void Reset()
        {
            _index = 0;
            _current = default;
        }

        public readonly T Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _current!; }

        readonly object? IEnumerator.Current
        {
            get
            {
                if (_index == 0 || _index == _count + 1)
                {
                    throw new InvalidOperationException();
                }

                return Current;
            }
        }
    }

    [DebuggerNonUserCode]
    private sealed class DebugView
    {
        private readonly NodeList<T> _list;

        public DebugView(NodeList<T> list)
        {
            _list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _list.ToArray();
    }
}

public static class NodeList
{
    internal static NodeList<T> From<T>(ref ArrayList<T> arrayList) where T : Node?
    {
        arrayList.Yield(out var items, out var count);

        // TODO: trim excess?
        return new NodeList<T>(items, count);
    }

    public static NodeList<T> Create<T>(IEnumerable<T> source) where T : Node?
    {
        switch (source)
        {
            case null:
                throw new ArgumentNullException(nameof(source));

            case NodeList<T> nodeList:
                return nodeList;

            case ICollection<T> collection:
                return collection.Count > 0
                    ? new NodeList<T>(collection)
                    : default;

            case IReadOnlyList<T> sourceList:
                if (sourceList.Count == 0)
                {
                    return default;
                }

                var list = new ArrayList<T>(sourceList.Count);
                for (var i = 0; i < sourceList.Count; i++)
                {
                    list.Add(sourceList[i]);
                }

                return From(ref list);

            default:
                var count = source is IReadOnlyCollection<T> readOnlyCollection ? readOnlyCollection.Count : -1;

                list = count is int initialCapacity
                    ? new ArrayList<T>(initialCapacity)
                    : new ArrayList<T>();

                if (count != 0)
                {
                    foreach (var item in source)
                    {
                        list.Add(item);
                    }
                }

                return From(ref list);
        }
    }
}
