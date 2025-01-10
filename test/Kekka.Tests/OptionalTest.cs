namespace Kekka.Tests;

public class OptionalTest
{
    [Fact]
    public void TryGetTest_when_has_value()
    {
        var actual = from x in Optional.Some("A")
                     from y in Optional.Some(x)
                     from z in Optional.Some(y)
                     select x + y + z;
        if (actual.TryGet(out var value))
        {
            Assert.Equal("AAA", value);
        }
        else
        {
            Assert.Fail();
        }
    }

    [Fact]
    public void TryGet_when_none()
    {
        var actual = from x in Optional.Some("A")
                     from y in Optional.None<string>()
                     from z in Optional.Some("C")
                     select x + y + z;
        Assert.False(actual.HasValue);
        Assert.False(actual.TryGet(out var value));
    }

    [Fact]
    public async Task TryGetTest_TaskSome()
    {
        var actual = await (
            from x in Task.FromResult(Optional.Some(2m))
            from y in Task.FromResult(Optional.Some(x))
            from z in Task.FromResult(Optional.Some(y))
            select x + y + z
        );
        if (actual.TryGet(out var value))
        {
            Assert.Equal(expected: 6, actual: value);
        }
        else
        {
            Assert.Fail();
        }
    }

    [Fact]
    public async Task TryGetTest_ValueTaskSome()
    {
        var actual = await (
            from x in new ValueTask<Optional<decimal>>(Optional.Some(2m))
            from y in new ValueTask<Optional<decimal>>(Optional.Some(x))
            from z in new ValueTask<Optional<decimal>>(Optional.Some(y))
            select x + y + z
        );
        if (actual.TryGet(out var value))
        {
            Assert.Equal(expected: 6, actual: value);
        }
        else
        {
            Assert.Fail();
        }
    }

    [Fact]
    public async Task TryGetTest_TaskNone()
    {
        var actual = await (
            from x in Task.FromResult(Optional.Some(2m))
            from y in Task.FromResult(Optional.None<decimal>())
            from z in Task.FromResult(Optional.Some(y))
            select x + y + z
        );

        Assert.False(actual.TryGet(out _));
    }

    [Fact]
    public async Task TryGetTest_ValueTaskNone()
    {
        var actual = await (
            from x in new ValueTask<Optional<decimal>>(Optional.Some(2m))
            from y in new ValueTask<Optional<decimal>>(Optional.None<decimal>())
            from z in new ValueTask<Optional<decimal>>(Optional.Some(y))
            select x + y + z
        );

        Assert.False(actual.TryGet(out _));
    }

    [Fact]
    public void TestEquals()
    {
        Assert.True(Optional.Some<decimal>(2).Equals(Optional.Some<decimal>(2)));
        Assert.False(Optional.Some<decimal>(2).Equals(Optional.Some<decimal>(3)));
        Assert.False(Optional.Some<decimal>(2).Equals(Optional.None<decimal>()));

        Assert.True(Optional.None<decimal>().Equals(Optional.None<decimal>()));
        Assert.True(Optional.None<decimal>().Equals(new Optional<decimal>()));
    }

    [Fact]
    public void TestOperators()
    {
        Assert.True(Optional.Some<decimal>(2) == Optional.Some<decimal>(2));
        Assert.True(Optional.Some<decimal>(2) != Optional.Some<decimal>(3));
        Assert.True(Optional.Some<decimal>(2) != Optional.None<decimal>());

        Assert.True(Optional.None<decimal>() == Optional.None<decimal>());
        Assert.True(Optional.None<decimal>() == new Optional<decimal>());
    }
}
