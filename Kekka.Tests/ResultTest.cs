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
        Assert.IsType<ArgumentException>(((ErrorResult<decimal, Exception>)actual).Value);
    }

    [Fact]
    public void ErrorTest2()
    {
        var actual = from x in Result.Ok<decimal, Exception>(2)
                     from y in Result.Error<decimal, Exception>(new ArgumentException())
                     from z in Result.Ok<decimal, Exception>(y)
                     select x + y + z;
        Assert.IsType<ErrorResult<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((ErrorResult<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task AsyncOkTest()
    {
        var actual = await (
            from x in Result.OkAsync<decimal, Exception>(2)
            from y in Result.OkAsync<decimal, Exception>(3)
            select x + y
        );
        Assert.IsType<OkResult<decimal, Exception>>(actual);
        Assert.Equal(5, ((OkResult<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task AsyncOkTest2()
    {
        var actual = await (
            from x in Result.OkAsync<decimal, Exception>(2)
            from y in Result.OkAsync<decimal, Exception>(x)
            from z in Result.OkAsync<decimal, Exception>(y)
            select x + y + z
        );
        Assert.IsType<OkResult<decimal, Exception>>(actual);
        Assert.Equal(6, ((OkResult<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task SyncAsyncOkTest()
    {
        var actual = await (
            from x in Result.OkAsync<decimal, Exception>(2)
            from y in Result.Ok<decimal, Exception>(x).AsTask()
            from z in Result.OkAsync<decimal, Exception>(y)
            select x + y + z
        );
        Assert.IsType<OkResult<decimal, Exception>>(actual);
        Assert.Equal(6, ((OkResult<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task AsyncErrorTest()
    {
        var actual = await (
            from x in Result.OkAsync<decimal, Exception>(2)
            from y in Result.ErrorAsync<decimal, Exception>(new ArgumentException())
            select x + y
        );
        Assert.IsType<ErrorResult<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((ErrorResult<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task AsyncErrorTest2()
    {
        var actual = await (
            from x in Result.OkAsync<decimal, Exception>(2)
            from y in Result.ErrorAsync<decimal, Exception>(new ArgumentException())
            from z in Result.OkAsync<decimal, Exception>(y)
            select x + y + z
        );
        Assert.IsType<ErrorResult<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((ErrorResult<decimal, Exception>)actual).Value);
    }

    [Fact]
    public async Task SyncAsyncErrorTest()
    {
        var actual = await (
            from x in Result.OkAsync<decimal, Exception>(2)
            from y in Result.Error<decimal, Exception>(new ArgumentException()).AsTask()
            from z in Result.OkAsync<decimal, Exception>(y)
            select x + y + z
        );
        Assert.IsType<ErrorResult<decimal, Exception>>(actual);
        Assert.IsType<ArgumentException>(((ErrorResult<decimal, Exception>)actual).Value);
    }
}