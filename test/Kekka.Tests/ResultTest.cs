namespace Kekka.Tests;

public class ResultTest
{
    [Fact]
    public void PatternMatchTest_Ok()
    {
        var actual = from x in Result.Ok<string, Exception>("A")
                     from y in Result.Ok<string, Exception>(x)
                     from z in Result.Ok<string, Exception>(y)
                     select x + y + z;

        if (actual is { Value: { } value })
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
    public void TryGetValueTest_Ok()
    {
        var actual = from x in Result.Ok<string, Exception>("A")
                     from y in Result.Ok<string, Exception>(x)
                     from z in Result.Ok<string, Exception>(y)
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
        var actual = from x in Result.Ok<string, Exception>("A")
                     from y in Result.Error<string, Exception>(new ArgumentException())
                     from z in Result.Ok<string, Exception>(y)
                     select x + y + z;

        Assert.False(actual.TryGetValue(out var value));
        Assert.Null(value);
    }

    [Fact]
    public void PatternMatchTest_Error()
    {
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Error<decimal, Exception>(new ArgumentException())
                     from z in Result.Ok<decimal, Exception>(y)
                     select x + y + z;
        if (actual is { Error: { } error })
        {
            Assert.IsType<ArgumentException>(error);
        }
        else
        {
            Assert.Fail();
        }
    }

    [Fact]
    public void TryGetErrorTest_Error()
    {
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Error<decimal, Exception>(new ArgumentException())
                     from z in Result.Ok<decimal, Exception>(y)
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
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Ok<decimal, Exception>(x)
                     from z in Result.Ok<decimal, Exception>(y)
                     select x + y + z;
        Assert.False(actual.TryGetError(out var error));
        Assert.Null(error);
    }

    [Fact]
    public void TryGetTest_Ok()
    {
        var actual = from x in Result.Ok<string, Exception>("A")
                     from y in Result.Ok<string, Exception>(x)
                     from z in Result.Ok<string, Exception>(y)
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
        var actual = from x in Result.Ok<string, Exception>("A")
                     from y in Result.Error<string, Exception>(new ArgumentException())
                     from z in Result.Ok<string, Exception>(y)
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
    public async Task TryGetValueTest_TaskOk()
    {
        var actual = await (
            from x in Task.FromResult(Result.Ok<decimal, Exception>(2))
            from y in Task.FromResult(Result.Ok<decimal, Exception>(x))
            from z in Task.FromResult(Result.Ok<decimal, Exception>(y))
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
            from x in Task.FromResult(Result.Ok<decimal, Exception>(2))
            from y in Task.FromResult(Result.Error<decimal, Exception>(new ArgumentException()))
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
    public async Task TryGetValueTest_ValueTaskOk()
    {
        var actual = await (
            from x in new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(2))
            from y in new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(x))
            from z in new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(y))
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
            from x in new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(2))
            from y in new ValueTask<Result<decimal, Exception>>(Result.Error<decimal, Exception>(new ArgumentException()))
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

        Task<Result<string, Exception>> GetProductIdAsync(int code)
        {
            return Task.FromResult(Result.Ok<string, Exception>("TKNKNST"));
        }

        Task<Result<decimal, Exception>> GetProductPriceAsync(string productId)
        {
            return Task.FromResult(Result.Ok<decimal, Exception>(150m));
        }

        Task<Result<string, Exception>> GetProductNameAsync(string productId)
        {
            return Task.FromResult(Result.Ok<string, Exception>("たけのこの里"));
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

        ValueTask<Result<string, Exception>> GetProductIdAsync(int code)
        {
            return new ValueTask<Result<string, Exception>>(Result.Ok<string, Exception>("TKNKNST"));
        }

        ValueTask<Result<decimal, Exception>> GetProductPriceAsync(string productId)
        {
            return new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(150m));
        }

        ValueTask<Result<string, Exception>> GetProductNameAsync(string productId)
        {
            return new ValueTask<Result<string, Exception>>(Result.Ok<string, Exception>("たけのこの里"));
        }
    }
}
