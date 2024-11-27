namespace Kekka.Tests;

public class ValueResultTest
{
    [Fact]
    public void OkTest()
    {
        var actual = from x in ValueResult.Ok<decimal, Exception>(2)
                     from y in ValueResult.Ok<decimal, Exception>(3)
                     select x + y;
        Assert.True(actual.IsSucceeded());
        Assert.Equal(expected: 5, actual: actual.GetValue());
    }

    [Fact]
    public void OkTest2()
    {
        var actual = from x in ValueResult.Ok<decimal, Exception>(2)
            from y in ValueResult.Ok<decimal, Exception>(x)
            from z in ValueResult.Ok<decimal, Exception>(y)
            select x + y + z;
        Assert.True(actual.IsSucceeded());
        Assert.Equal(expected: 6, actual: actual.GetValue());
    }
    
    [Fact]
    public void TryGetValueTest()
    {
        var actual = from x in ValueResult.Ok<decimal, Exception>(2)
            from y in ValueResult.Ok<decimal, Exception>(x)
            from z in ValueResult.Ok<decimal, Exception>(y)
            select x + y + z;

        if (actual.TryGetValue(out var value))
        {
            Assert.Equal(expected: 6, actual: value);
        }
        else
        {
            Assert.Fail();
        }
    }

    [Fact]
    public void ErrorTest()
    {
        var actual = from x in ValueResult.Ok<decimal, Exception>(2)
                     from y in ValueResult.Error<decimal, Exception>(new ArgumentException())
                     select x + y;
        Assert.False(actual.IsSucceeded());
        Assert.IsType<ArgumentException>(actual.GetError());
    }

    [Fact]
    public void ErrorTest2()
    {
        var actual = from x in ValueResult.Ok<decimal, Exception>(2)
                     from y in ValueResult.Error<decimal, Exception>(new ArgumentException())
                     from z in ValueResult.Ok<decimal, Exception>(y)
                     select x + y + z;
        Assert.False(actual.IsSucceeded());
        Assert.IsType<ArgumentException>(actual.GetError());
    }

    [Fact]
    public void TryGetErrorTest()
    {
        var actual = from x in ValueResult.Ok<decimal, Exception>(2)
                     from y in ValueResult.Error<decimal, Exception>(new ArgumentException())
                     from z in ValueResult.Ok<decimal, Exception>(y)
                     select x + y + z;
        if (actual.TryGetError(out var error))
        {
            Assert.IsType<ArgumentException>(error);
        }
        else
        {
            Assert.Fail();
        }
    }

    [Fact]
    public async Task AsyncOkTest()
    {
        var actual = await (
            from x in Task.FromResult(ValueResult.Ok<decimal, Exception>(2))
            from y in Task.FromResult(ValueResult.Ok<decimal, Exception>(3))
            select x + y
        );
        Assert.True(actual.IsSucceeded());
        Assert.Equal(5, actual.GetValue());
    }

    [Fact]
    public async Task AsyncOkTest2()
    {
        var actual = await (
            from x in Task.FromResult(ValueResult.Ok<decimal, Exception>(2))
            from y in Task.FromResult(ValueResult.Ok<decimal, Exception>(x))
            from z in Task.FromResult(ValueResult.Ok<decimal, Exception>(y))
            select x + y + z
        );
        Assert.True(actual.IsSucceeded());
        Assert.Equal(6, actual.GetValue());
    }

    [Fact]
    public async Task AsyncErrorTest()
    {
        var actual = await (
            from x in Task.FromResult(ValueResult.Ok<decimal, Exception>(2))
            from y in Task.FromResult(ValueResult.Error<decimal, Exception>(new ArgumentException()))
            select x + y
        );
        Assert.False(actual.IsSucceeded());
        Assert.IsType<ArgumentException>(actual.GetError());
    }

    [Fact]
    public async Task AsyncErrorTest2()
    {
        var actual = await (
            from x in Task.FromResult(ValueResult.Ok<decimal, Exception>(2))
            from y in Task.FromResult(ValueResult.Error<decimal, Exception>(new ArgumentException()))
            from z in Task.FromResult(ValueResult.Ok<decimal, Exception>(y))
            select x + y + z
        );
        Assert.False(actual.IsSucceeded());
        Assert.IsType<ArgumentException>(actual.GetError());
    }

    [Fact]
    public async Task RailwayTest()
    {
        var pipeline = from id in GetProductIdAsync(10)
                       from name in GetProductNameAsync(id)
                       from price in GetProductPriceAsync(id)
                       select $"{id} {name} {price}円";
        var actual = await pipeline;
        Assert.True(actual.IsSucceeded());
        Assert.Equal("TKNKNST たけのこの里 150円", actual.GetValue());

        Task<ValueResult<string, Exception>> GetProductIdAsync(int code)
        {
            return Task.FromResult(ValueResult.Ok<string, Exception>("TKNKNST"));
        }

        Task<ValueResult<decimal, Exception>> GetProductPriceAsync(string productId)
        {
            return Task.FromResult(ValueResult.Ok<decimal, Exception>(150m));
        }

        Task<ValueResult<string, Exception>> GetProductNameAsync(string productId)
        {
            return Task.FromResult(ValueResult.Ok<string, Exception>("たけのこの里"));
        }
    }
}
