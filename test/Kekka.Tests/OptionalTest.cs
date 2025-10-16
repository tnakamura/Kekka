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

    #region Factory Method Tests

    [Fact]
    public void Some_CreatesOptionalWithValue()
    {
        var optional = Optional.Some("test");

        Assert.True(optional.HasValue);
        Assert.Equal("test", optional.Value);
        Assert.True(optional.TryGet(out var value));
        Assert.Equal("test", value);
    }

    [Fact]
    public void None_CreatesEmptyOptional()
    {
        var optional = Optional.None<string>();

        Assert.False(optional.HasValue);
        Assert.Equal(default(string), optional.Value);
        Assert.False(optional.TryGet(out var value));
        Assert.Null(value);
    }

    [Fact]
    public void None_Property_CreatesEmptyOptional()
    {
        var optional = Optional<string>.None;

        Assert.False(optional.HasValue);
        Assert.Equal(default(string), optional.Value);
        Assert.False(optional.TryGet(out var value));
        Assert.Null(value);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void HasValue_SomeOptional_ReturnsTrue()
    {
        var optional = Optional.Some(42);
        Assert.True(optional.HasValue);
    }

    [Fact]
    public void HasValue_NoneOptional_ReturnsFalse()
    {
        var optional = Optional.None<int>();
        Assert.False(optional.HasValue);
    }

    [Fact]
    public void Value_SomeOptional_ReturnsValue()
    {
        var optional = Optional.Some(42);
        Assert.Equal(42, optional.Value);
    }

    [Fact]
    public void Value_NoneOptional_ReturnsDefault()
    {
        var optional = Optional.None<int>();
        Assert.Equal(default(int), optional.Value);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var opt1 = Optional.Some(42);
        var opt2 = Optional.Some(42);

        Assert.True(opt1.Equals(opt2));
        Assert.True(opt1 == opt2);
        Assert.False(opt1 != opt2);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var opt1 = Optional.Some(42);
        var opt2 = Optional.Some(24);

        Assert.False(opt1.Equals(opt2));
        Assert.False(opt1 == opt2);
        Assert.True(opt1 != opt2);
    }

    [Fact]
    public void Equals_SomeAndNone_ReturnsFalse()
    {
        var some = Optional.Some(42);
        var none = Optional.None<int>();

        Assert.False(some.Equals(none));
        Assert.False(some == none);
        Assert.True(some != none);
    }

    [Fact]
    public void Equals_BothNone_ReturnsTrue()
    {
        var none1 = Optional.None<string>();
        var none2 = Optional.None<string>();

        Assert.True(none1.Equals(none2));
        Assert.True(none1 == none2);
        Assert.False(none1 != none2);
    }

    [Fact]
    public void Equals_WithValue_SameValue_ReturnsTrue()
    {
        var optional = Optional.Some("test");

        Assert.True(optional.Equals("test"));
    }

    [Fact]
    public void Equals_WithValue_DifferentValue_ReturnsFalse()
    {
        var optional = Optional.Some("test");

        Assert.False(optional.Equals("other"));
    }

    [Fact]
    public void Equals_NoneWithValue_ReturnsFalse()
    {
        var optional = Optional.None<string>();

        Assert.False(optional.Equals("test"));
    }

    [Fact]
    public void Equals_WithObject_SameType_ReturnsTrue()
    {
        var opt1 = Optional.Some(42);
        var opt2 = Optional.Some(42);

        Assert.True(opt1.Equals((object)opt2));
    }

    [Fact]
    public void Equals_WithObject_DifferentType_ReturnsFalse()
    {
        var optional = Optional.Some(42);

        Assert.True(optional.Equals(42));
        Assert.False(optional.Equals("not an optional"));
        Assert.False(optional.Equals(null));
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHashCode()
    {
        var opt1 = Optional.Some("test");
        var opt2 = Optional.Some("test");

        Assert.Equal(opt1.GetHashCode(), opt2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_ReturnsDifferentHashCodes()
    {
        var opt1 = Optional.Some("test1");
        var opt2 = Optional.Some("test2");

        Assert.NotEqual(opt1.GetHashCode(), opt2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_None_ReturnsZero()
    {
        var optional = Optional.None<string>();

        Assert.Equal(0, optional.GetHashCode());
    }

    #endregion

    #region Extension Method Tests

    [Fact]
    public void Select_SomeOptional_TransformsValue()
    {
        var optional = Optional.Some(5);
        var transformed = optional.Select(x => x * 2);

        Assert.True(transformed.HasValue);
        Assert.Equal(10, transformed.Value);
    }

    [Fact]
    public void Select_NoneOptional_ReturnsNone()
    {
        var optional = Optional.None<int>();
        var transformed = optional.Select(x => x * 2);

        Assert.False(transformed.HasValue);
    }

    [Fact]
    public void SelectMany_SomeOptional_ChainsCorrectly()
    {
        var optional = Optional.Some(5);
        var chained = optional.SelectMany(x => Optional.Some(x.ToString()));

        Assert.True(chained.HasValue);
        Assert.Equal("5", chained.Value);
    }

    [Fact]
    public void SelectMany_NoneOptional_ReturnsNone()
    {
        var optional = Optional.None<int>();
        var chained = optional.SelectMany(x => Optional.Some(x.ToString()));

        Assert.False(chained.HasValue);
    }

    [Fact]
    public void SelectMany_SomeButSelectorReturnsNone_ReturnsNone()
    {
        var optional = Optional.Some(5);
        var chained = optional.SelectMany(x => Optional.None<string>());

        Assert.False(chained.HasValue);
    }

    #endregion

    #region LINQ Query Syntax Tests

    [Fact]
    public void LinqQuery_MultipleFromClauses_ChainsCorrectly()
    {
        var result = from x in Optional.Some(5)
                     from y in Optional.Some(3)
                     from z in Optional.Some(2)
                     select x + y + z;

        Assert.True(result.HasValue);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public void LinqQuery_WithNoneInChain_ReturnsNone()
    {
        var result = from x in Optional.Some(5)
                     from y in Optional.None<int>()
                     from z in Optional.Some(2)
                     select x + y + z;

        Assert.False(result.HasValue);
    }

    #endregion

    #region Pattern Matching Tests

    [Fact]
    public void PatternMatching_SwitchExpression_Some()
    {
        var optional = Optional.Some(42);

        var message = optional switch
        {
            { HasValue: true, Value: var value } => $"Some: {value}",
            { HasValue: false } => "None"
        };

        Assert.Equal("Some: 42", message);
    }

    [Fact]
    public void PatternMatching_SwitchExpression_None()
    {
        var optional = Optional.None<int>();

        var message = optional switch
        {
            { HasValue: true, Value: var value } => $"Some: {value}",
            { HasValue: false } => "None"
        };

        Assert.Equal("None", message);
    }

    [Fact]
    public void PatternMatching_WithPropertyPattern()
    {
        var optional = Optional.Some("test");

        if (optional is { Value: { } value })
        {
            Assert.Equal("test", value);
        }
        else
        {
            Assert.Fail("Expected Some but got None");
        }
    }

    #endregion

    #region Edge Cases and Complex Type Tests

    [Fact]
    public void Equals_WithComplexTypes_WorksCorrectly()
    {
        var tuple1 = new ValueTuple<int, string>(1, "test");
        var tuple2 = new ValueTuple<int, string>(1, "test");
        var tuple3 = new ValueTuple<int, string>(2, "test");

        var opt1 = Optional.Some(tuple1);
        var opt2 = Optional.Some(tuple2);
        var opt3 = Optional.Some(tuple3);

        Assert.True(opt1.Equals(opt2));
        Assert.False(opt1.Equals(opt3));
    }

    [Fact]
    public void Equals_WithCustomEquatableType_UsesCustomEquality()
    {
        var person1 = new Person { Name = "John", Age = 30 };
        var person2 = new Person { Name = "John", Age = 30 };
        var person3 = new Person { Name = "Jane", Age = 30 };

        var opt1 = Optional.Some(person1);
        var opt2 = Optional.Some(person2);
        var opt3 = Optional.Some(person3);

        Assert.True(opt1.Equals(opt2));
        Assert.False(opt1.Equals(opt3));
    }

    [Fact]
    public void DefaultOptional_IsSameAsNone()
    {
        var defaultOptional = default(Optional<string>);
        var noneOptional = Optional.None<string>();

        Assert.Equal(defaultOptional, noneOptional);
        Assert.False(defaultOptional.HasValue);
        Assert.False(noneOptional.HasValue);
    }

    #endregion

    #region Async Extension Method Tests

    [Fact]
    public async Task Select_TaskSome_TransformsValue()
    {
        var optionalTask = Task.FromResult(Optional.Some(5));
        var transformed = await optionalTask.Select(x => x * 2);

        Assert.True(transformed.HasValue);
        Assert.Equal(10, transformed.Value);
    }

    [Fact]
    public async Task Select_TaskNone_ReturnsNone()
    {
        var optionalTask = Task.FromResult(Optional.None<int>());
        var transformed = await optionalTask.Select(x => x * 2);

        Assert.False(transformed.HasValue);
    }

    [Fact]
    public async Task SelectMany_TaskSome_ChainsCorrectly()
    {
        var optionalTask = Task.FromResult(Optional.Some(5));
        var chained = await optionalTask.SelectMany(x =>
            Task.FromResult(Optional.Some(x.ToString())));

        Assert.True(chained.HasValue);
        Assert.Equal("5", chained.Value);
    }

    [Fact]
    public async Task SelectMany_TaskNone_ReturnsNone()
    {
        var optionalTask = Task.FromResult(Optional.None<int>());
        var chained = await optionalTask.SelectMany(x =>
            Task.FromResult(Optional.Some(x.ToString())));

        Assert.False(chained.HasValue);
    }

    [Fact]
    public async Task Select_ValueTaskSome_TransformsValue()
    {
        var optionalTask = new ValueTask<Optional<int>>(Optional.Some(5));
        var transformed = await optionalTask.Select(x => x * 2);

        Assert.True(transformed.HasValue);
        Assert.Equal(10, transformed.Value);
    }

    [Fact]
    public async Task SelectMany_ValueTaskSome_ChainsCorrectly()
    {
        var optionalTask = new ValueTask<Optional<int>>(Optional.Some(5));
        var chained = await optionalTask.SelectMany(x =>
            new ValueTask<Optional<string>>(Optional.Some(x.ToString())));

        Assert.True(chained.HasValue);
        Assert.Equal("5", chained.Value);
    }

    #endregion

    #region Helper Classes for Tests

    private class Person : IEquatable<Person>
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }

        public bool Equals(Person? other)
        {
            return other != null && Name == other.Name && Age == other.Age;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Person);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Age);
        }
    }

    #endregion
}
