using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Acornima.Helpers;

using static ExceptionHelper;

/// <summary>
/// This structure is like <see cref="List{T}"/> from the BCL except the only allocation
/// required on the heap is the backing array storage for the elements.
/// An empty list, however causes no heap allocation; that is, the array is
/// allocated on first addition.
/// </summary>
/// <remarks>
/// WARNING: Having a struct intended for modification can introduce some very
/// subtle and ugly bugs if not used carefully. For example, two copies
/// of the struct start with the same base array and modifying either
/// list may also modify the other. Consider the following:
///
/// <code>
/// var a = new ArrayList&lt;int&gt;();
/// a.Add(1);
/// a.Add(2);
/// a.Add(3);
/// var b = a;
/// b.Add(4);
/// b.RemoveAt(0);
/// </code>
///
/// Both `a` and `b` will see the same changes. However, they'll appear
/// to change independently if the example is changed as follows:
///
/// <code>
/// var a = new ArrayList&lt;int&gt;();
/// a.Add(1);
/// a.Add(2);
/// a.Add(3);
/// var b = a;
/// b.Add(4);
/// b.Add(5);        // &lt;-- only new change
/// b.RemoveAt(0);
/// </code>
///
/// When 5 is added to `b`, `b` re-allocates its array to make space
/// and consequently further changes are only visible in `b`. To help
/// avoid these subtle bugs, the debug version of this implementation
/// tracks changes. It maintains a local and a boxed version number.
/// The boxed version gets shared by all copies of the struct. If a
/// modification is made via any copy then the boxed version number is
/// updated. Any subsequent use (even if for reading only) of other
/// copies check that their local version numbers haven't diverged from
/// the shared one. In effect, if a copy is made and modified then the
/// original will throw if ever used. For the example above, this
/// means while it's safe to continue to use copy `b` after
/// modification, `a` will become useless:
///
/// <code>
/// var a = new ArrayList&lt;int&gt;();
/// a.Add(1);
/// a.Add(2);
/// a.Add(3);
/// var b = a;
/// b.Add(4);
/// b.Add(5);
/// b.RemoveAt(0);
/// Console.WriteLine(b.Count); // safe to continue to use
/// Console.WriteLine(a.Count); // will throw
/// </code>
/// </remarks>
#if DEBUG
[DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}, {nameof(Capacity)} = {{{nameof(Capacity)}}}, Version = {{{nameof(_localVersion)}}}")]
[DebuggerTypeProxy(typeof(ArrayList<>.DebugView))]
#endif
internal struct ArrayList<T> : IList<T>
{
    private const int MinAllocatedCount = 4;

    private T[]? _items;
    private int _count;

#if DEBUG
    private StrongBox<int>? _sharedVersion;
    private int _localVersion;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ArrayList(int initialCapacity)
    {
        Debug.Assert(initialCapacity >= 0);

        _items = initialCapacity > 0 ? new T[initialCapacity] : null;
        _count = 0;

#if DEBUG
        _localVersion = 0;
        _sharedVersion = null;
#endif
    }

    /// <remarks>
    /// WARNING: Expects ownership of the array.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ArrayList(T[] items)
    {
        _items = items;
        _count = items.Length;

#if DEBUG
        _localVersion = 0;
        _sharedVersion = null;
#endif
    }

    [Conditional("DEBUG")]
    private readonly void AssertUnchanged()
    {
#if DEBUG
        if (_localVersion != (_sharedVersion?.Value ?? 0))
        {
            ThrowInvalidOperationException<T>();
        }
#endif
    }

    [Conditional("DEBUG")]
    private void OnChanged()
    {
#if DEBUG
        _sharedVersion ??= new StrongBox<int>();

        ref var version = ref _sharedVersion.Value;
        version++;
        _localVersion = version;
#endif
    }

    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            AssertUnchanged();
            return _items?.Length ?? 0;
        }
        set
        {
            AssertUnchanged();

            if (value < _count)
            {
                ThrowArgumentOutOfRangeException(nameof(value), value, null);
            }
            else if (value == (_items?.Length ?? 0))
            {
                return;
            }
            else if (value > 0)
            {
                var array = new T[value];
                if (_count > 0)
                {
                    Array.Copy(_items!, 0, array, 0, _count);
                }
                _items = array;
            }
            else
            {
                _items = null;
            }

            OnChanged();
        }
    }

    public readonly int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            AssertUnchanged();
            return _count;
        }
    }

    public readonly bool IsReadOnly => false;

    public readonly T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            AssertUnchanged();

            // Following trick can reduce the range check by one
            if ((uint)index >= (uint)_count)
            {
                return ThrowIndexOutOfRangeException<T>();
            }

            return _items![index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            AssertUnchanged();

            // Following trick can reduce the range check by one
            if ((uint)index < (uint)_count)
            {
                _items![index] = value;
                return;
            }

            ThrowIndexOutOfRangeException<T>();
        }
    }

    /// <remarks>
    /// WARNING: Items should not be added or removed from the <see cref="ArrayList{T}"/> while the returned reference is in use.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly ref T GetItemRef(int index)
    {
        AssertUnchanged();
        return ref _items![index];
    }

    /// <remarks>
    /// WARNING: Items should not be added or removed from the <see cref="ArrayList{T}"/> while the returned reference is in use.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly ref T LastItemRef() => ref GetItemRef(_count - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GrowCapacity(int capacity)
    {
        // NOTE: Using a growth factor of 3/2 yields better benchmark results than 2.
        // It also results in less excess when the underlying array is returned directly wrapped in a NodeList, Span, etc.
        return (capacity * 3L) >> 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        PushRef() = item;
    }

    public void AddRange(ReadOnlySpan<T> items)
    {
        AssertUnchanged();

        var itemCount = items.Length;
        if (itemCount == 0)
        {
            return;
        }

        var oldCount = _count;
        var newCount = oldCount + itemCount;

        if (Capacity < newCount)
        {
            Array.Resize(ref _items, Math.Max(newCount, MinAllocatedCount));
        }

        Debug.Assert(_items is not null);
        items.CopyTo(_items.AsSpan(oldCount, itemCount));
        _count = newCount;

        OnChanged();
    }

    public readonly bool Contains(T item)
    {
        return IndexOf(item) >= 0;
    }

    public void Clear()
    {
        AssertUnchanged();

        if (_count != 0)
        {
            Array.Clear(_items!, 0, _count);
            _count = 0;
        }

        OnChanged();
    }

    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(_items ?? Array.Empty<T>(), 0, array, arrayIndex, _count);
    }

    public readonly int IndexOf(T item)
    {
        AssertUnchanged();
        return _count != 0 ? Array.IndexOf(_items!, item, 0, _count) : -1;
    }

    public void Insert(int index, T item)
    {
        AssertUnchanged();

        if ((uint)index > (uint)_count)
        {
            ThrowIndexOutOfRangeException<T>();
        }

        var capacity = Capacity;

        if (_count == capacity)
        {
            Array.Resize(ref _items, Math.Max(checked((int)GrowCapacity(capacity)), MinAllocatedCount));
        }

        Debug.Assert(_items is not null);
        Array.Copy(_items, index, _items, index + 1, Count - index);
        _items![index] = item;
        _count++;

        OnChanged();
    }

    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        AssertUnchanged();

        if ((uint)index >= (uint)_count)
        {
            ThrowIndexOutOfRangeException<T>();
        }

        _count--;

        if (index < _count)
        {
            Array.Copy(_items!, index + 1, _items!, index, Count - index);
        }

        _items![_count] = default!;

        OnChanged();
    }

    public void Sort(IComparer<T>? comparer = null)
    {
        AssertUnchanged();

        if (_count > 1)
        {
            Array.Sort(_items!, 0, _count, comparer);
        }

        OnChanged();
    }

    public void TrimExcess(int threshold = MinAllocatedCount)
    {
        AssertUnchanged();

        if (Capacity - _count > threshold)
        {
            Capacity = _count;
        }
    }

    /// <remarks>
    /// WARNING: Items should not be added or removed from the <see cref="ArrayList{T}"/> while the returned reference is in use.
    /// </remarks>
    public ref T PushRef()
    {
        AssertUnchanged();

        var capacity = Capacity;

        if (_count == capacity)
        {
            Array.Resize(ref _items, Math.Max(checked((int)GrowCapacity(capacity)), MinAllocatedCount));
        }

        Debug.Assert(_items is not null);
        ref var item = ref _items![_count++];

        OnChanged();

        return ref item!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T item)
    {
        PushRef() = item;
    }

    /// <remarks>
    /// WARNING: Items should not be added or removed from the <see cref="ArrayList{T}"/> while the returned reference is in use.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly ref T PeekRef() => ref GetItemRef(_count - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T Peek() => PeekRef();

    /// <remarks>
    /// WARNING: Items should not be added or removed from the <see cref="ArrayList{T}"/> while the returned reference is in use.<br/>
    /// Also note that this operation doesn't actually remove the item from the underlying data structure, so objects referenced by
    /// the item will not be eligible for garbage collection.
    /// </remarks>
    public ref T PopRef()
    {
        AssertUnchanged();

        var lastIndex = _count - 1;
        ref var last = ref _items![lastIndex];
        _count = lastIndex;

        OnChanged();

        return ref last!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Pop()
    {
        ref var lastRef = ref PopRef();
        var last = lastRef;
        lastRef = default;
        return last;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Yield(out T[]? items, out int count)
    {
        AssertUnchanged();

        items = _items;
        count = _count;
        this = default;
    }

    /// <remarks>
    /// WARNING: Items should not be added or removed from the <see cref="ArrayList{T}"/> while the returned <see cref="Span{T}"/> is in use.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly Span<T> AsSpan()
    {
        AssertUnchanged();
        return new Span<T>(_items, 0, _count);
    }

    /// <remarks>
    /// WARNING: Items should not be added or removed from the <see cref="ArrayList{T}"/> while the returned <see cref="ReadOnlySpan{T}"/> is in use.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly ReadOnlySpan<T> AsReadOnlySpan()
    {
        AssertUnchanged();
        return new ReadOnlySpan<T>(_items, 0, _count);
    }

    public readonly T[] ToArray()
    {
        AssertUnchanged();

        if (_count == 0)
        {
            return Array.Empty<T>();
        }

        var array = new T[_count];
        Array.Copy(_items!, 0, array, 0, _count);
        return array;
    }

    public readonly Enumerator GetEnumerator()
    {
        AssertUnchanged();
        return new Enumerator(_items, _count);
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <remarks>
    /// This implementation does not detect changes to the list during iteration
    /// and therefore the behaviour is undefined under those conditions.
    /// </remarks>
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

#if DEBUG
    [DebuggerNonUserCode]
    private sealed class DebugView
    {
        private readonly ArrayList<T> _list;

        public DebugView(ArrayList<T> list)
        {
            _list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _list.ToArray();
    }
#endif
}
