using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acornima.Properties;

namespace Acornima.Helpers;

/// <summary>
/// A heavily slimmed down version of <see cref="HashSet{String}"/> which can be used to reduce memory allocations when dissecting a string.
/// </summary>
// Based on:
// * https://github.com/dotnet/runtime/blob/v6.0.8/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/HashSet.cs
// * https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/v7.1.2/Microsoft.Toolkit.HighPerformance/Buffers/StringPool.cs
internal struct StringPool
{
    private const int MinAllocatedCount = 4;

    private int[]? _buckets;
    private Entry[]? _entries;
    private int _count;

    public readonly int Count { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _count; }

    /// <summary>
    /// Initializes buckets and slots arrays. Uses suggested capacity by finding next prime
    /// greater than or equal to capacity.
    /// </summary>
    private int Initialize(int capacity)
    {
        int size = capacity;
        var buckets = new int[size];
        var entries = new Entry[size];

        // Assign member variables after both arrays are allocated to guard against corruption from OOM if second fails.
        _buckets = buckets;
        _entries = entries;

        return size;
    }

    private void Resize(int newSize)
    {
        Debug.Assert(_entries is not null, "_entries should be non-null");
        Debug.Assert(newSize >= _entries!.Length);

        var entries = new Entry[newSize];

        int count = _count;
        Array.Copy(_entries, entries, count);

        // Assign member variables after both arrays allocated to guard against corruption from OOM if second fails
        _buckets = new int[newSize];

        for (int i = 0; i < count; i++)
        {
            ref Entry entry = ref entries[i];
            if (entry.Next >= -1)
            {
                ref int bucket = ref GetBucketRef(entry.HashCode);
                entry.Next = bucket - 1; // Value in _buckets is 1-based
                bucket = i + 1;
            }
        }

        _entries = entries;
    }

    /// <summary>Gets a reference to the specified hashcode's bucket, containing an index into <see cref="_entries"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ref int GetBucketRef(int hashCode)
    {
        int[] buckets = _buckets!;
        return ref buckets[(uint)hashCode % (uint)buckets.Length];
    }

    /// <summary>Adds the specified string to the <see cref="StringPool"/> object if it's not already contained.</summary>
    /// <param name="value">The string to add.</param>
    /// <returns>The stored string instance.</returns>
    public string GetOrCreate(ReadOnlySpan<char> value)
    {
        if (_buckets is null)
        {
            Initialize(MinAllocatedCount);
        }
        Debug.Assert(_buckets is not null);

        Entry[]? entries = _entries;
        Debug.Assert(entries is not null, "expected entries to be non-null");

        uint collisionCount = 0;

        int hashCode = value.GetSequenceHashCode();
        ref int bucket = ref GetBucketRef(hashCode);
        int i = bucket - 1; // Value in _buckets is 1-based

        while (i >= 0)
        {
            ref Entry entry = ref entries![i];
            if (entry.HashCode == hashCode && entry.Value.AsSpan().SequenceEqual(value))
            {
                return entry.Value;
            }
            i = entry.Next;

            collisionCount++;
            if (collisionCount > (uint)entries.Length)
            {
                // The chain of entries forms a loop, which means a concurrent update has happened.
                throw new InvalidOperationException(ExceptionMessages.ConcurrentUpdatePerformed);
            }
        }

        int index;
        {
            int count = _count;
            if (count == entries!.Length)
            {
                Resize(checked(_count + _count / 2));
                bucket = ref GetBucketRef(hashCode);
            }
            index = count;
            _count = count + 1;
            entries = _entries;
        }

        {
            ref Entry entry = ref entries![index];
            entry.HashCode = hashCode;
            entry.Next = bucket - 1; // Value in _buckets is 1-based
            entry.Value = value.ToString();
            bucket = index + 1;

            return entry.Value;
        }
    }

    private struct Entry
    {
        public int HashCode;
        /// <summary>
        /// 0-based index of next entry in chain: -1 means end of chain
        /// also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
        /// so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
        /// </summary>
        public int Next;
        public string Value;
    }
}
