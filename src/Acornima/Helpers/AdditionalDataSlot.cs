using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Acornima.Helpers;

// This type is semi-thread-safe: promotion of a single value to an array is thread-safe but updating items is not.
internal struct AdditionalDataSlot
{
    private volatile object? _data;

    public object? PrimaryData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            var data = _data;
            return data is not ValueHolder[] array ? data : Volatile.Read(ref array[0].Data);
        }
        set
        {
            var data = _data;
            for (; ; )
            {
                object? newData;
                if (data is not ValueHolder[] array)
                {
                    newData = value;
                }
                else
                {
                    Volatile.Write(ref array[0].Data, value);
                    return;
                }

                var currentData = Interlocked.CompareExchange(ref _data, newData, data);
                if (currentData == data)
                {
                    break;
                }
                data = currentData;
            }
        }
    }

    public readonly object? this[int index]
    {
        get
        {
            Debug.Assert(index >= 0, "Index is out of range.");

            var data = _data;
            if (index == 0)
            {
                return PrimaryData;
            }

            var array = data as ValueHolder[];
            return array is not null && (uint)index < (uint)array.Length ? Volatile.Read(ref array[index].Data) : null;
        }
    }

    public object? SetItem(int index, object? value, int capacity)
    {
        Debug.Assert(index >= 0 && index < capacity, "Index is out of range.");
        Debug.Assert(((_data as ValueHolder[])?.Length ?? capacity) == capacity, "Capacity changed.");

        if (index == 0)
        {
            PrimaryData = value;
            return value;
        }

        var data = _data;
        for (; ; )
        {
            var array = data as ValueHolder[];
            if (array is not null)
            {
                Volatile.Write(ref array[index].Data, value);
                return value;
            }
            else if (value is not null)
            {
                array = new ValueHolder[capacity];
                array[0].Data = data;
                array[index].Data = value;
            }
            else
            {
                return value;
            }

            var currentData = Interlocked.CompareExchange(ref _data, array, data);
            if (currentData == data)
            {
                break;
            }
            data = currentData;
        }

        return value;
    }

    // NOTE: We need an internal type to make our logic foolproof. (See the type check in the AdditionalDataSlot.PrimaryData property below.)
    private struct ValueHolder
    {
        public object? Data;
    }
}
