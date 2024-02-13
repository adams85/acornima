using System;
using System.Diagnostics;

namespace Acornima.Helpers;

// NOTE: We need an internal type to make our logic foolproof. (See the type check in the AdditionalDataSlot.PrimaryData property below.)
internal struct AdditionalDataHolder
{
    public object? Data;
}

internal struct AdditionalDataSlot
{
    private object? _data;

    public object? PrimaryData
    {
        readonly get => _data is not AdditionalDataHolder[] array ? _data : array[0].Data;
        set
        {
            Debug.Assert(value is not AdditionalDataHolder[], $"Value of type {typeof(AdditionalDataHolder[])} is not allowed.");
            (_data is not AdditionalDataHolder[] array ? ref _data : ref array[0].Data) = value;
        }
    }

    public object? this[int index]
    {
        readonly get
        {
            Debug.Assert(index >= 0, "Index must be greater than or equal to 0.");

            var array = _data as AdditionalDataHolder[];
            if (index == 0)
            {
                return array is null ? _data : array[0].Data;
            }
            else
            {
                return array is not null && (uint)index < (uint)array.Length ? array[index].Data : null;
            }
        }
        set
        {
            Debug.Assert(value is not AdditionalDataHolder[], $"Value of type {typeof(AdditionalDataHolder[])} is not allowed.");
            Debug.Assert(index >= 0, "Index must be greater than or equal to 0.");

            var array = _data as AdditionalDataHolder[];
            if (index == 0)
            {
                (array is null ? ref _data : ref array[0].Data) = value;
                return;
            }

            if (array is not null)
            {
                if ((uint)index >= (uint)array.Length)
                {
                    if (value is null)
                        return;

                    Array.Resize(ref array, index + 1);
                    _data = array;
                }
            }
            else
            {
                if (value is null)
                    return;

                array = new AdditionalDataHolder[index + 1];
                array[0].Data = _data;
                _data = array;
            }

            array[index].Data = value;
        }
    }
}
