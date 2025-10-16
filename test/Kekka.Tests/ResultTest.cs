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

    #region Equality Tests

    [Fact]
    public void Equals_SameSuccessValues_ReturnsTrue()
    {
        var result1 = Result.Ok<int, string>(42);
        var result2 = Result.Ok<int, string>(42);

        Assert.True(result1.Equals(result2));
        Assert.True(result1 == result2);
        Assert.False(result1 != result2);
    }

    [Fact]
    public void Equals_DifferentSuccessValues_ReturnsFalse()
    {
        var result1 = Result.Ok<int, string>(42);
        var result2 = Result.Ok<int, string>(24);

        Assert.False(result1.Equals(result2));
        Assert.False(result1 == result2);
        Assert.True(result1 != result2);
    }

    [Fact]
    public void Equals_SameErrorValues_ReturnsTrue()
    {
        var result1 = Result.Error<int, string>("error");
        var result2 = Result.Error<int, string>("error");

        Assert.True(result1.Equals(result2));
        Assert.True(result1 == result2);
        Assert.False(result1 != result2);
    }

    [Fact]
    public void Equals_DifferentErrorValues_ReturnsFalse()
    {
        var result1 = Result.Error<int, string>("error1");
        var result2 = Result.Error<int, string>("error2");

        Assert.False(result1.Equals(result2));
        Assert.False(result1 == result2);
        Assert.True(result1 != result2);
    }

    [Fact]
    public void Equals_SuccessAndError_ReturnsFalse()
    {
        var success = Result.Ok<int, string>(42);
        var error = Result.Error<int, string>("error");

        Assert.False(success.Equals(error));
        Assert.False(success == error);
        Assert.True(success != error);
    }

    [Fact]
    public void Equals_WithObject_SameType_ReturnsTrue()
    {
        var result1 = Result.Ok<int, string>(42);
        var result2 = Result.Ok<int, string>(42);

        Assert.True(result1.Equals((object)result2));
    }

    [Fact]
    public void Equals_WithObject_DifferentType_ReturnsFalse()
    {
        var result = Result.Ok<int, string>(42);

        Assert.False(result.Equals("not a result"));
        Assert.False(result.Equals(42));
        Assert.False(result.Equals(null));
    }

    [Fact]
    public void GetHashCode_SameSuccessValues_ReturnsSameHashCode()
    {
        var result1 = Result.Ok<int, string>(42);
        var result2 = Result.Ok<int, string>(42);

        Assert.Equal(result1.GetHashCode(), result2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_SameErrorValues_ReturnsSameHashCode()
    {
        var result1 = Result.Error<int, string>("error");
        var result2 = Result.Error<int, string>("error");

        Assert.Equal(result1.GetHashCode(), result2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_ReturnsDifferentHashCodes()
    {
        var success = Result.Ok<int, string>(42);
        var error = Result.Error<int, string>("error");
        var differentSuccess = Result.Ok<int, string>(24);

        Assert.NotEqual(success.GetHashCode(), error.GetHashCode());
        Assert.NotEqual(success.GetHashCode(), differentSuccess.GetHashCode());
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void Ok_CreatesSuccessResult()
    {
        var result = Result.Ok<int, string>(42);

        Assert.True(result.HasValue);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
        Assert.True(result.TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Error_CreatesErrorResult()
    {
        var result = Result.Error<int, string>("error");

        Assert.False(result.HasValue);
        Assert.Equal(default(int), result.Value);
        Assert.Equal("error", result.Error);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal("error", error);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void HasValue_SuccessResult_ReturnsTrue()
    {
        var result = Result.Ok<int, string>(42);
        Assert.True(result.HasValue);
    }

    [Fact]
    public void HasValue_ErrorResult_ReturnsFalse()
    {
        var result = Result.Error<int, string>("error");
        Assert.False(result.HasValue);
    }

    [Fact]
    public void Value_SuccessResult_ReturnsValue()
    {
        var result = Result.Ok<int, string>(42);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Value_ErrorResult_ReturnsDefault()
    {
        var result = Result.Error<int, string>("error");
        Assert.Equal(default(int), result.Value);
    }

    [Fact]
    public void Error_SuccessResult_ReturnsNull()
    {
        var result = Result.Ok<int, string>(42);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Error_ErrorResult_ReturnsError()
    {
        var result = Result.Error<int, string>("error");
        Assert.Equal("error", result.Error);
    }

    #endregion

    #region Edge Cases and Null Handling

    [Fact]
    public void Ok_WithNullableReferenceType_HandlesCorrectly()
    {
        var result = Result.Ok<string?, Exception>("test");

        Assert.True(result.HasValue);
        Assert.Equal("test", result.Value);
    }

    [Fact]
    public void Equals_WithComplexTypes_WorksCorrectly()
    {
        var tuple1 = new ValueTuple<int, int, int>(1, 2, 3);
        var tuple2 = new ValueTuple<int, int, int>(1, 2, 3);
        var tuple3 = new ValueTuple<int, int, int>(3, 2, 1);

        var result1 = Result.Ok<ValueTuple<int, int, int>, string>(tuple1);
        var result2 = Result.Ok<ValueTuple<int, int, int>, string>(tuple2);
        var result3 = Result.Ok<ValueTuple<int, int, int>, string>(tuple3);

        Assert.True(result1.Equals(result2));

        Assert.False(result1.Equals(result3));
    }

    [Fact]
    public void Equals_WithCustomEquatableType_UsesCustomEquality()
    {
        var person1 = new Person { Name = "John", Age = 30 };
        var person2 = new Person { Name = "John", Age = 30 };
        var person3 = new Person { Name = "Jane", Age = 30 };

        var result1 = Result.Ok<Person, string>(person1);
        var result2 = Result.Ok<Person, string>(person2);
        var result3 = Result.Ok<Person, string>(person3);

        Assert.True(result1.Equals(result2));
        Assert.False(result1.Equals(result3));
    }

    #endregion

    #region Extension Method Tests

    [Fact]
    public void Select_SuccessResult_TransformsValue()
    {
        var result = Result.Ok<int, string>(5);
        var transformed = result.Select(x => x * 2);

        Assert.True(transformed.HasValue);
        Assert.Equal(10, transformed.Value);
    }

    [Fact]
    public void Select_ErrorResult_PreservesError()
    {
        var result = Result.Error<int, string>("error");
        var transformed = result.Select(x => x * 2);

        Assert.False(transformed.HasValue);
        Assert.Equal("error", transformed.Error);
    }

    [Fact]
    public void SelectMany_SuccessResult_ChainsCorrectly()
    {
        var result = Result.Ok<int, string>(5);
        var chained = result.SelectMany(x => Result.Ok<string, string>(x.ToString()));

        Assert.True(chained.HasValue);
        Assert.Equal("5", chained.Value);
    }

    [Fact]
    public void SelectMany_ErrorResult_ShortCircuits()
    {
        var result = Result.Error<int, string>("initial error");
        var chained = result.SelectMany(x => Result.Ok<string, string>(x.ToString()));

        Assert.False(chained.HasValue);
        Assert.Equal("initial error", chained.Error);
    }

    [Fact]
    public void SelectMany_SuccessResultButSelectorFails_ReturnsSecondError()
    {
        var result = Result.Ok<int, string>(5);
        var chained = result.SelectMany(x => Result.Error<string, string>("selector error"));

        Assert.False(chained.HasValue);
        Assert.Equal("selector error", chained.Error);
    }

    [Fact]
    public void MapError_SuccessResult_PreservesValue()
    {
        var result = Result.Ok<int, string>(42);
        var mapped = result.MapError(error => new Exception(error));

        Assert.True(mapped.HasValue);
        Assert.Equal(42, mapped.Value);
    }

    [Fact]
    public void MapError_ErrorResult_TransformsError()
    {
        var result = Result.Error<int, string>("error message");
        var mapped = result.MapError(error => new Exception(error));

        Assert.False(mapped.HasValue);
        Assert.IsType<Exception>(mapped.Error);
        Assert.Equal("error message", mapped.Error!.Message);
    }

    [Fact]
    public void AsTask_ReturnsCompletedTask()
    {
        var result = Result.Ok<int, string>(42);
        var task = result.AsTask();

        Assert.True(task.IsCompleted);
        Assert.Equal(42, task.Result.Value);
    }

    [Fact]
    public void AsValueTask_ReturnsCompletedValueTask()
    {
        var result = Result.Ok<int, string>(42);
        var valueTask = result.AsValueTask();

        Assert.True(valueTask.IsCompleted);
        Assert.Equal(42, valueTask.Result.Value);
    }

    [Fact]
    public void Sequence_AllSuccess_ReturnsCollectionOfValues()
    {
        var results = new[]
        {
            Result.Ok<int, string>(1),
            Result.Ok<int, string>(2),
            Result.Ok<int, string>(3)
        };

        var sequenced = results.Sequence();

        Assert.True(sequenced.HasValue);
        Assert.Equal(new[] { 1, 2, 3 }, sequenced.Value);
    }

    [Fact]
    public void Sequence_ContainsError_ReturnsFirstError()
    {
        var results = new[]
        {
            Result.Ok<int, string>(1),
            Result.Error<int, string>("error1"),
            Result.Error<int, string>("error2")
        };

        var sequenced = results.Sequence();

        Assert.False(sequenced.HasValue);
        Assert.Equal("error1", sequenced.Error);
    }

    [Fact]
    public void Sequence_EmptyCollection_ReturnsEmptyCollection()
    {
        var results = Array.Empty<Result<int, string>>();
        var sequenced = results.Sequence();

        Assert.True(sequenced.HasValue);
        Assert.Empty(sequenced.Value!);
    }

    #endregion

    #region LINQ Query Syntax Tests

    [Fact]
    public void LinqQuery_MultipleFromClauses_ChainsCorrectly()
    {
        var result = from x in Result.Ok<int, string>(5)
                     from y in Result.Ok<int, string>(3)
                     from z in Result.Ok<int, string>(2)
                     select x + y + z;

        Assert.True(result.HasValue);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public void LinqQuery_WithErrorInChain_ShortCircuits()
    {
        var result = from x in Result.Ok<int, string>(5)
                     from y in Result.Error<int, string>("chain error")
                     from z in Result.Ok<int, string>(2)
                     select x + y + z;

        Assert.False(result.HasValue);
        Assert.Equal("chain error", result.Error);
    }

    #endregion

    #region Pattern Matching Tests

    [Fact]
    public void PatternMatching_SwitchExpression_Success()
    {
        var result = Result.Ok<int, string>(42);

        var message = result switch
        {
            { HasValue: true, Value: var value } => $"Success: {value}",
            { HasValue: false, Error: var error } => $"Error: {error}"
        };

        Assert.Equal("Success: 42", message);
    }

    [Fact]
    public void PatternMatching_SwitchExpression_Error()
    {
        var result = Result.Error<int, string>("failed");

        var message = result switch
        {
            { HasValue: true, Value: var value } => $"Success: {value}",
            { HasValue: false, Error: var error } => $"Error: {error}"
        };

        Assert.Equal("Error: failed", message);
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
