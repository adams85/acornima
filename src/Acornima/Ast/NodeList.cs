using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Helpers;

namespace Acornima.Ast;

using static ExceptionHelper;

[DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
[DebuggerTypeProxy(typeof(NodeList<>.DebugView))]
public readonly struct NodeList<T> : IReadOnlyList<T> where T : Node?
{
    private readonly T[]? _items;
    private readonly int _count;

    internal NodeList(ICollection<T> collection)
    {
        Debug.Assert(collection is not null);

        _count = collection!.Count;
        if (_count > 0)
        {
            _items = new T[_count];
            collection.CopyTo(_items, 0);
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
        return new Enumerator(_items, Count);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly T[]? _items; // Usually null when count is zero
        private readonly int _count;
        private int _index;

        internal Enumerator(T[]? items, int count)
        {
            _items = items;
            _count = count;
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
        public readonly T Current { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _items![_index]; }

        readonly object? IEnumerator.Current => Current;
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
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is NodeList<T> nodeList)
        {
            return nodeList;
        }

        if (source is ICollection<T> collection)
        {
            return new NodeList<T>(collection);
        }

        ArrayList<T> list;

        if (source is IReadOnlyCollection<T> readOnlyCollection)
        {
            var count = readOnlyCollection.Count;
            if (count == 0)
            {
                return new NodeList<T>();
            }

            list = new ArrayList<T>(count);

            if (readOnlyCollection is IReadOnlyList<T> readOnlyList)
            {
                for (var i = 0; i < count; i++)
                {
                    list.Add(readOnlyList[i]);
                }
                return From(ref list);
            }
        }
        else
        {
            list = new ArrayList<T>();
        }

        foreach (var item in source)
        {
            list.Add(item);
        }

        return From(ref list);
    }
}
