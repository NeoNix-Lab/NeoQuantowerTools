using Neo.Quantower.Abstractions.Models;

namespace Neo.Quantower.Abstractions.Tests;

public class RingBufferTests
{
    [Fact]
    public void InsertBeyondCapacity_OverwriteOldest()
    {
        //Arrange
        var buffer = new RingBuffer<int>(3);
        buffer.Add(1);
        buffer.Add(2);
        buffer.Add(3);

        //Act
        buffer.Add(4);

        //Assert
        Assert.Collection(buffer.GetRange(0, 2),
            item => Assert.Equal(2, item),
            item => Assert.Equal(3, item),
            item => Assert.Equal(4, item));

    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Ctor_InvalidCapacity_Throws(int cap)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new RingBuffer<Guid>(cap));
    }

    [Fact]
    public void IndexAccess_InvalidCapacity_Throws()
    {
        //Arrange
        var buffer = new RingBuffer<int>(3, false);
        buffer.Add(1);
        buffer.Add(2);
        buffer.Add(3);

        //Act Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer[3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer[-1]);

    }

    [Fact]
    public void InsertBeyondCapacity_OverwriteOldest_Reversal()
    {
        //Arrange
        var buffer = new RingBuffer<int>(3, false);
        buffer.Add(1);
        buffer.Add(2);
        buffer.Add(3);

        //Act
        buffer.Add(4);

        //Assert
        Assert.Equal(4, buffer.GetWithReversal(0));

    }

    [Fact]
    public void InsertBeyondCapacity_OverwriteOldest_ReversalRange()
    {
        //Arrange
        var buffer = new RingBuffer<int>(3, false);
        buffer.Add(1);
        buffer.Add(2);
        buffer.Add(3);

        //Act
        buffer.Add(4);

        //Assert
        Assert.Collection(buffer.GetRange(0, 0),
            item => Assert.Equal(4, item));
        //item => Assert.Equal(3, item),
        //item => Assert.Equal(2, item));

        //Assert.Equal(2, buffer[2]);

    }

    [Fact]
    public void Get_Index_In_Reversal()
    {
        //Arrange
        var buffer = new RingBuffer<int>(3, false);
        buffer.Add(1);
        buffer.Add(2);
        buffer.Add(3);
        buffer.Add(4);
        buffer.Add(5);
        buffer.Add(6);

        //Act
        var item = buffer[0];

        //Assert
        Assert.True(item == 6);
    }

    [Fact]
    public void Get_Count()
    {
        //Arrange
        var buffer = new RingBuffer<int>(3);
        buffer.Add(1);
        buffer.Add(2);
        buffer.Add(3);

        //Act
        var count = buffer.Count;

        //Assert
        Assert.True(count == 3);
    }

    [Fact]
    public void Get_Items()
    {
        //Arrange
        var buffer = new RingBuffer<int>(3);
        buffer.Add(1);
        buffer.Add(2);
        buffer.Add(3);

        //Act
        var items = buffer.GetItems();

        //Assert
        Assert.Collection(items,
           item => Assert.Equal(1, item),
           item => Assert.Equal(2, item),
           item => Assert.Equal(3, item));
    }

    [Fact]
    public void Get_Items_Reversal()
    {
        //Arrange
        var buffer = new RingBuffer<int>(3, false);
        buffer.Add(1);
        buffer.Add(2);
        buffer.Add(3);
        buffer.Add(4);

        //Act
        var items = buffer.GetItems();

        //Assert
        Assert.Collection(items,
           item => Assert.Equal(4, item),
           item => Assert.Equal(3, item),
           item => Assert.Equal(2, item));
    }
}