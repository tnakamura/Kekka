namespace Kekka.Tests;

public class ResultTest
{
    [Fact]
    public void OkTest()
    {
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Ok<decimal, Exception>(3)
                     select x + y;
        Assert.IsType<Ok<decimal, Exception>>(actual);
        Assert.Equal(5, ((Ok<decimal, Exception>)actual).Value);
    }

    [Fact]
    public void OkTest2()
    {
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Ok<decimal, Exception>(x)
                     from z in Result.Ok<decimal, Exception>(y)
                     select x + y + z;
        Assert.IsType<Ok<decimal, Exception>>(actual);
        Assert.Equal(6, ((Ok<decimal, Exception>)actual).Value);
    }

    [Fact]
    public void ErrorTest()
    {
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Error<decimal, Exception>(new ArgumentException())
                     select x + y;
        Assert.IsType<Error<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((Error<decimal, Exception>)actual).Value);
    }

    [Fact]
    public void ErrorTest2()
    {
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Error<decimal, Exception>(new ArgumentException())
                     from z in Result.Ok<decimal, Exception>(y)
                     select x + y + z;
        Assert.IsType<Error<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((Error<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task OkTest_Task()
    {
        var actual = await (
            from x in Task.FromResult(Result.Ok<decimal, Exception>(2))
            from y in Task.FromResult(Result.Ok<decimal, Exception>(3))
            select x + y
        );
        Assert.IsType<Ok<decimal, Exception>>(actual);
        Assert.Equal(5, ((Ok<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task OkTest_Task2()
    {
        var actual = await (
            from x in Task.FromResult(Result.Ok<decimal, Exception>(2))
            from y in Task.FromResult(Result.Ok<decimal, Exception>(x))
            from z in Task.FromResult(Result.Ok<decimal, Exception>(y))
            select x + y + z
        );
        Assert.IsType<Ok<decimal, Exception>>(actual);
        Assert.Equal(6, ((Ok<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task ErrorTest_Task()
    {
        var actual = await (
            from x in Task.FromResult(Result.Ok<decimal, Exception>(2))
            from y in Task.FromResult(Result.Error<decimal, Exception>(new ArgumentException()))
            select x + y
        );
        Assert.IsType<Error<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((Error<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task ErrorTest_Task2()
    {
        var actual = await (
            from x in Task.FromResult(Result.Ok<decimal, Exception>(2))
            from y in Task.FromResult(Result.Error<decimal, Exception>(new ArgumentException()))
            from z in Task.FromResult(Result.Ok<decimal, Exception>(y))
            select x + y + z
        );
        Assert.IsType<Error<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((Error<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task OkTest_ValueTask()
    {
        var actual = await (
            from x in new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(2))
            from y in new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(3))
            select x + y
        );
        Assert.IsType<Ok<decimal, Exception>>(actual);
        Assert.Equal(5, ((Ok<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task OkTest_ValueTask2()
    {
        var actual = await (
            from x in new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(2))
            from y in new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(x))
            from z in new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(y))
            select x + y + z
        );
        Assert.IsType<Ok<decimal, Exception>>(actual);
        Assert.Equal(6, ((Ok<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task ErrorTest_ValueTask()
    {
        var actual = await (
            from x in new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(2))
            from y in new ValueTask<Result<decimal, Exception>>(Result.Error<decimal, Exception>(new ArgumentException()))
            select x + y
        );
        Assert.IsType<Error<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((Error<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task ErrorTest_ValueTask2()
    {
        var actual = await (
            from x in new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(2))
            from y in new ValueTask<Result<decimal, Exception>>(Result.Error<decimal, Exception>(new ArgumentException()))
            from z in new ValueTask<Result<decimal, Exception>>(Result.Ok<decimal, Exception>(y))
            select x + y + z
        );
        Assert.IsType<Error<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((Error<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task RailwayTest_Task()
    {
        var pipeline = from id in GetProductIdAsync(10)
                       from name in GetProductNameAsync(id)
                       from price in GetProductPriceAsync(id)
                       select $"{id} {name} {price}‰~";
        var actual = await pipeline;
        Assert.IsType<Ok<string, Exception>>(actual);
        Assert.Equal("TKNKNST ‚½‚¯‚Ì‚±‚Ì—¢ 150‰~", ((Ok<string, Exception>)actual).Value);

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
            return Task.FromResult(Result.Ok<string, Exception>("‚½‚¯‚Ì‚±‚Ì—¢"));
        }
    }

    [Fact]
    public async Task RailwayTest_ValueTask()
    {
        var pipeline = from id in GetProductIdAsync(10)
                       from name in GetProductNameAsync(id)
                       from price in GetProductPriceAsync(id)
                       select $"{id} {name} {price}‰~";
        var actual = await pipeline;
        Assert.IsType<Ok<string, Exception>>(actual);
        Assert.Equal("TKNKNST ‚½‚¯‚Ì‚±‚Ì—¢ 150‰~", ((Ok<string, Exception>)actual).Value);

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
            return new ValueTask<Result<string, Exception>>(Result.Ok<string, Exception>("‚½‚¯‚Ì‚±‚Ì—¢"));
        }
    }
}