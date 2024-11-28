namespace Kekka.Tests;

public class ValueResultTest
{
    [Fact]
    public void IsOkTest()
    {
        var actual = from x in ValueResult.Ok<decimal, Exception>(2)
                     from y in ValueResult.Ok<decimal, Exception>(3)
                     select x + y;
        Assert.True(actual.IsOk);
    }

    [Fact]
    public void IsOkTest2()
    {
        var actual = from x in ValueResult.Ok<decimal, Exception>(2)
                     from y in ValueResult.Ok<decimal, Exception>(x)
                     from z in ValueResult.Ok<decimal, Exception>(y)
                     select x + y + z;
        Assert.True(actual.IsOk);
    }

    [Fact]
    public void TryGetValueTest_Ok()
    {
        var actual = from x in ValueResult.Ok<string, Exception>("A")
                     from y in ValueResult.Ok<string, Exception>(x)
                     from z in ValueResult.Ok<string, Exception>(y)
                     select x + y + z;

        if (actual.TryGetValue(out var value))
        {
            Assert.Equal(3, value.Length);
            Assert.Equal(expected: "AAA", actual: value);
        }
        else
        {
            Assert.Fail();
        }
    }

    [Fact]
    public void TryGetValueTest_Error()
    {
        var actual = from x in ValueResult.Ok<string, Exception>("A")
                     from y in ValueResult.Error<string, Exception>(new ArgumentException())
                     from z in ValueResult.Ok<string, Exception>(y)
                     select x + y + z;

        Assert.False(actual.TryGetValue(out var value));
        Assert.Null(value);
    }

    [Fact]
    public void TryGetErrorTest_Error()
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
    public void TryGetErrorTest_Ok()
    {
        var actual = from x in ValueResult.Ok<decimal, Exception>(2)
                     from y in ValueResult.Ok<decimal, Exception>(x)
                     from z in ValueResult.Ok<decimal, Exception>(y)
                     select x + y + z;
        Assert.False(actual.TryGetError(out var error));
        Assert.Null(error);
    }

    [Fact]
    public void TryGetTest_Ok()
    {
        var actual = from x in ValueResult.Ok<string, Exception>("A")
                     from y in ValueResult.Ok<string, Exception>(x)
                     from z in ValueResult.Ok<string, Exception>(y)
                     select x + y + z;

        if (actual.TryGet(out var value, out var error))
        {
            Assert.Equal(3, value.Length);
            Assert.Equal(expected: "AAA", actual: value);
        }
        else
        {
            Assert.Fail();
        }
    }

    [Fact]
    public void TryGetTest_Error()
    {
        var actual = from x in ValueResult.Ok<string, Exception>("A")
                     from y in ValueResult.Error<string, Exception>(new ArgumentException())
                     from z in ValueResult.Ok<string, Exception>(y)
                     select x + y + z;

        if (actual.TryGet(out var value, out var error))
        {
            Assert.Fail();
        }
        else
        {
            Assert.IsType<ArgumentException>(error);
        }
    }

    [Fact]
    public async Task IsOkTest_TaskOk()
    {
        var actual = await (
            from x in Task.FromResult(ValueResult.Ok<decimal, Exception>(2))
            from y in Task.FromResult(ValueResult.Ok<decimal, Exception>(3))
            select x + y
        );
        Assert.True(actual.IsOk);
    }

    [Fact]
    public async Task TryGetValueTest_TaskOk()
    {
        var actual = await (
            from x in Task.FromResult(ValueResult.Ok<decimal, Exception>(2))
            from y in Task.FromResult(ValueResult.Ok<decimal, Exception>(x))
            from z in Task.FromResult(ValueResult.Ok<decimal, Exception>(y))
            select x + y + z
        );
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
    public async Task TryGetErrorTest_TaskError()
    {
        var actual = await (
            from x in Task.FromResult(ValueResult.Ok<decimal, Exception>(2))
            from y in Task.FromResult(ValueResult.Error<decimal, Exception>(new ArgumentException()))
            select x + y
        );
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
    public async Task IsOkTest_ValueTaskOk()
    {
        var actual = await (
            from x in new ValueTask<ValueResult<decimal, Exception>>(ValueResult.Ok<decimal, Exception>(2))
            from y in new ValueTask<ValueResult<decimal, Exception>>(ValueResult.Ok<decimal, Exception>(3))
            select x + y
        );
        Assert.True(actual.IsOk);
    }

    [Fact]
    public async Task TryGetValueTest_ValueTaskOk()
    {
        var actual = await (
            from x in new ValueTask<ValueResult<decimal, Exception>>(ValueResult.Ok<decimal, Exception>(2))
            from y in new ValueTask<ValueResult<decimal, Exception>>(ValueResult.Ok<decimal, Exception>(x))
            from z in new ValueTask<ValueResult<decimal, Exception>>(ValueResult.Ok<decimal, Exception>(y))
            select x + y + z
        );
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
    public async Task TryGetErrorTest_ValueTaskError()
    {
        var actual = await (
            from x in new ValueTask<ValueResult<decimal, Exception>>(ValueResult.Ok<decimal, Exception>(2))
            from y in new ValueTask<ValueResult<decimal, Exception>>(ValueResult.Error<decimal, Exception>(new ArgumentException()))
            select x + y
        );
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
    public async Task RailwayTest_Task()
    {
        var pipeline = from id in GetProductIdAsync(10)
                       from name in GetProductNameAsync(id)
                       from price in GetProductPriceAsync(id)
                       select $"{id} {name} {price}円";
        var actual = await pipeline;
        if (actual.TryGetValue(out var value))
        {
            Assert.Equal("TKNKNST たけのこの里 150円", value);
        }
        else
        {
            Assert.Fail();
        }

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

    [Fact]
    public async Task RailwayTest_ValueTask()
    {
        var pipeline = from id in GetProductIdAsync(10)
                       from name in GetProductNameAsync(id)
                       from price in GetProductPriceAsync(id)
                       select $"{id} {name} {price}円";
        var actual = await pipeline;
        if (actual.TryGetValue(out var value))
        {
            Assert.Equal("TKNKNST たけのこの里 150円", value);
        }
        else
        {
            Assert.Fail();
        }

        ValueTask<ValueResult<string, Exception>> GetProductIdAsync(int code)
        {
            return new ValueTask<ValueResult<string, Exception>>(ValueResult.Ok<string, Exception>("TKNKNST"));
        }

        ValueTask<ValueResult<decimal, Exception>> GetProductPriceAsync(string productId)
        {
            return new ValueTask<ValueResult<decimal, Exception>>(ValueResult.Ok<decimal, Exception>(150m));
        }

        ValueTask<ValueResult<string, Exception>> GetProductNameAsync(string productId)
        {
            return new ValueTask<ValueResult<string, Exception>>(ValueResult.Ok<string, Exception>("たけのこの里"));
        }
    }
}
