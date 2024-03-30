using System.Runtime.CompilerServices;
using System.Threading;
using Acornima.Helpers;
using Xunit;

namespace Acornima.Tests;

public class AdditionalDataSlotTests
{
    [Fact]
    public void PrimaryData_SingleValue()
    {
        var slot = new AdditionalDataSlot();
        Assert.Null(slot.PrimaryData);
        Assert.Null(slot[0]);

        slot.PrimaryData = string.Empty;
        Assert.Equal(string.Empty, slot.PrimaryData);
        Assert.Equal(string.Empty, slot[0]);

        slot.PrimaryData = 0;
        Assert.Equal(0, slot.PrimaryData);
        Assert.Equal(0, slot[0]);

        slot.PrimaryData = null;
        Assert.Null(slot.PrimaryData);
        Assert.Null(slot[0]);
    }

    [Fact]
    public void PrimaryData_PromotionToArray_BeforeSet()
    {
        var slot = new AdditionalDataSlot();
        const int capacity = 2;

        slot.SetItem(1, 1, capacity);
        Assert.Equal(1, slot[1]);
        slot.PrimaryData = 0;
        Assert.Equal(0, slot.PrimaryData);
        Assert.Equal(0, slot[0]);
        Assert.Equal(1, slot[1]);

        slot.PrimaryData = null;
        Assert.Null(slot.PrimaryData);
        Assert.Null(slot[0]);
        Assert.Equal(1, slot[1]);
    }

    [Fact]
    public void PrimaryData_PromotionToArray_AfterSet()
    {
        var slot = new AdditionalDataSlot();
        const int capacity = 2;

        slot.PrimaryData = 0;
        Assert.Null(slot[1]);
        slot.SetItem(1, 1, capacity);
        Assert.Equal(0, slot.PrimaryData);
        Assert.Equal(0, slot[0]);
        Assert.Equal(1, slot[1]);

        slot.PrimaryData = null;
        Assert.Null(slot.PrimaryData);
        Assert.Null(slot[0]);
        Assert.Equal(1, slot[1]);
    }

    [Fact]
    public void Indexer_PrimaryData_SingleValue()
    {
        var slot = new AdditionalDataSlot();
        const int capacity = 2;

        Assert.Null(slot.PrimaryData);
        Assert.Null(slot[0]);

        slot.SetItem(0, string.Empty, capacity);
        Assert.Equal(string.Empty, slot.PrimaryData);
        Assert.Equal(string.Empty, slot[0]);

        slot.SetItem(0, 0, capacity);
        Assert.Equal(0, slot.PrimaryData);
        Assert.Equal(0, slot[0]);

        slot.SetItem(0, null, capacity);
        Assert.Null(slot.PrimaryData);
        Assert.Null(slot[0]);
    }

    [Fact]
    public void Indexer_PrimaryData_PromotionToArray_BeforeSet()
    {
        var slot = new AdditionalDataSlot();
        const int capacity = 2;

        slot.SetItem(1, 1, capacity);
        Assert.Equal(1, slot[1]);
        slot.SetItem(0, 0, capacity);
        Assert.Equal(0, slot.PrimaryData);
        Assert.Equal(0, slot[0]);
        Assert.Equal(1, slot[1]);

        slot.SetItem(0, null, capacity);
        Assert.Null(slot.PrimaryData);
        Assert.Null(slot[0]);
        Assert.Equal(1, slot[1]);
    }

    [Fact]
    public void Indexer_PrimaryData_PromotionToArray_AfterSet()
    {
        var slot = new AdditionalDataSlot();
        const int capacity = 2;

        slot.SetItem(0, 0, capacity);
        Assert.Null(slot[1]);
        slot.SetItem(1, 1, capacity);
        Assert.Equal(0, slot.PrimaryData);
        Assert.Equal(0, slot[0]);
        Assert.Equal(1, slot[1]);

        slot.SetItem(0, null, capacity);
        Assert.Null(slot.PrimaryData);
        Assert.Null(slot[0]);
        Assert.Equal(1, slot[1]);
    }

    [Fact]
    public void ShouldPreventOverwriteWithStaleValueDuringPromotionToArray()
    {
        var startSignal1 = new object();
        var startSignal2 = new object();
        var finishSignal = new CountdownEvent(initialCount: 2);

        var boxedSlot = new StrongBox<AdditionalDataSlot>();

        var thread1 = new Thread(() =>
        {
            for (; ; )
            {
                lock (startSignal1)
                {
                    finishSignal.Signal();
                    Monitor.Wait(startSignal1);
                    boxedSlot.Value.PrimaryData = 0;
                }
            }
        });

        var thread2 = new Thread(() =>
        {
            for (; ; )
            {
                lock (startSignal2)
                {
                    finishSignal.Signal();
                    Monitor.Wait(startSignal2);
                    boxedSlot.Value.SetItem(1, 1, capacity: 2);
                }
            }
        });

        thread1.Start();
        thread2.Start();

        finishSignal.Wait();

        for (var i = 0; i < 10000; i++)
        {
            finishSignal.Reset();

            lock (startSignal1)
            {
                lock (startSignal2)
                {
                    Monitor.Pulse(startSignal1);
                    Monitor.Pulse(startSignal2);
                }
            }

            finishSignal.Wait();

            Assert.Equal(0, boxedSlot.Value.PrimaryData);
            Assert.Equal(1, boxedSlot.Value[1]);
            boxedSlot.Value = default;
        }
    }
}
