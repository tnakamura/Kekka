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
}
