namespace Kekka.Tests;

public class ResultTest
{
    [Fact]
    public void OkTest()
    {
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Ok<decimal, Exception>(3)
                     select x + y;
        Assert.IsType<OkResult<decimal, Exception>>(actual);
        Assert.Equal(5, ((OkResult<decimal, Exception>)actual).Value);
    }

    [Fact]
    public void OkTest2()
    {
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Ok<decimal, Exception>(x)
                     from z in Result.Ok<decimal, Exception>(y)
                     select x + y + z;
        Assert.IsType<OkResult<decimal, Exception>>(actual);
        Assert.Equal(6, ((OkResult<decimal, Exception>)actual).Value);
    }

    [Fact]
    public void ErrorTest()
    {
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Error<decimal, Exception>(new ArgumentException())
                     select x + y;
        Assert.IsType<ErrorResult<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((ErrorResult<decimal, Exception>)actual).Error);
    }

    [Fact]
    public void ErrorTest2()
    {
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Error<decimal, Exception>(new ArgumentException())
                     from z in Result.Ok<decimal, Exception>(y)
                     select x + y + z;
        Assert.IsType<ErrorResult<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((ErrorResult<decimal, Exception>)actual).Error);
    }

    [Fact]
    public async Task AsyncOkTest()
    {
        var actual = await (
            from x in Task.FromResult(Result.Ok<decimal, Exception>(2))
            from y in Task.FromResult(Result.Ok<decimal, Exception>(3))
            select x + y
        );
        Assert.IsType<OkResult<decimal, Exception>>(actual);
        Assert.Equal(5, ((OkResult<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task AsyncOkTest2()
    {
        var actual = await (
            from x in Task.FromResult(Result.Ok<decimal, Exception>(2))
            from y in Task.FromResult(Result.Ok<decimal, Exception>(x))
            from z in Task.FromResult(Result.Ok<decimal, Exception>(y))
            select x + y + z
        );
        Assert.IsType<OkResult<decimal, Exception>>(actual);
        Assert.Equal(6, ((OkResult<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task AsyncErrorTest()
    {
        var actual = await (
            from x in Task.FromResult(Result.Ok<decimal, Exception>(2))
            from y in Task.FromResult(Result.Error<decimal, Exception>(new ArgumentException()))
            select x + y
        );
        Assert.IsType<ErrorResult<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((ErrorResult<decimal, Exception>)actual).Error);
    }

    [Fact]
    public async Task AsyncErrorTest2()
    {
        var actual = await (
            from x in Task.FromResult(Result.Ok<decimal, Exception>(2))
            from y in Task.FromResult(Result.Error<decimal, Exception>(new ArgumentException()))
            from z in Task.FromResult(Result.Ok<decimal, Exception>(y))
            select x + y + z
        );
        Assert.IsType<ErrorResult<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((ErrorResult<decimal, Exception>)actual).Error);
    }

    [Fact]
    public async Task RailwayTest()
    {
        var pipeline = from id in GetProductIdAsync(10)
                       from name in GetProductNameAsync(id)
                       from price in GetProductPriceAsync(id)
                       select $"{id} {name} {price}‰~";
        var actual = await pipeline;
        Assert.IsType<OkResult<string, Exception>>(actual);
        Assert.Equal("TKNKNST ‚½‚¯‚Ì‚±‚Ì—¢ 150‰~", ((OkResult<string, Exception>)actual).Value);

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
}