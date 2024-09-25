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
    public void ErrorTest()
    {
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Error<decimal, Exception>(new ArgumentException())
                     select x + y;
        Assert.IsType<ErrorResult<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((ErrorResult<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task AsyncOkTest()
    {
        var actual = await (
            from x in GetOkAsync(2)
            from y in GetOkAsync(3)
            select x + y
        );
        Assert.IsType<OkResult<decimal, Exception>>(actual);
        Assert.Equal(5, ((OkResult<decimal, Exception>)actual).Value);
    }

    private Task<Result<decimal, Exception>> GetOkAsync(int value)
    {
        return Task.FromResult(Result.Ok<decimal, Exception>(value));
    }

    private Task<Result<decimal, Exception>> GetErrorAsync(Exception ex)
    {
        return Task.FromResult(Result.Error<decimal, Exception>(ex));
    }

    [Fact]
    public async Task AsyncErrorTest()
    {
        var actual = await (
            from x in GetOkAsync(2)
            from y in GetErrorAsync(new ArgumentException())
            select x + y
        );
        Assert.IsType<ErrorResult<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((ErrorResult<decimal, Exception>)actual).Value);
    }
}