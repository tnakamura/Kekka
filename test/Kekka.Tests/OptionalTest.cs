namespace Kekka.Tests;

public class OptionalTest
{
    [Fact]
    public void TryGetTest_when_has_value()
    {
        var actual = from x in new Optional<string>("A")
                     from y in new Optional<string>("B")
                     from z in new Optional<string>("C")
                     select x + y + z;
        if (actual.TryGet(out var value))
        {
            Assert.Equal("ABC", value);
        }
        else
        {
            Assert.Fail();
        }
    }

    [Fact]
    public void TryGet_when_none()
    {
        var actual = from x in new Optional<string>("A")
                     from y in Optional<string>.None
                     from z in new Optional<string>("C")
                     select x + y + z;
        Assert.False(actual.HasValue);
        Assert.False(actual.TryGet(out var value));
    }
}
