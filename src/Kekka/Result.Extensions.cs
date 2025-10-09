using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kekka;

/// <summary>
/// Provides factory methods for creating <see cref="Result{T, TError}"/> instances.
/// These methods enable Railway Oriented Programming patterns by providing a clean API
/// for creating success and failure results.
/// </summary>
public static partial class Result
{
    /// <summary>
    /// Creates a successful result that contains the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="value">The success value to wrap in the result.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> that represents a successful operation with the specified value.
    /// </returns>
    /// <remarks>
    /// This is the preferred way to create a successful result. It represents the "success track"
    /// in Railway Oriented Programming patterns.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = Result.Ok&lt;int, string&gt;(42);
    /// Console.WriteLine(result.HasValue); // True
    /// Console.WriteLine(result.Value);    // 42
    /// </code>
    /// </example>
    public static Result<T, TError> Ok<T, TError>(T value)
        where TError : notnull
    {
        return new Result<T, TError>(value);
    }

    /// <summary>
    /// Creates a failed result that contains the specified error.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="error">The error value to wrap in the result.</param>
    /// <returns>
    /// A <see cref="Result{T, TError}"/> that represents a failed operation with the specified error.
    /// </returns>
    /// <remarks>
    /// This method creates a result in the error state, representing the "failure track"
    /// in Railway Oriented Programming patterns. Once a result is in the error state,
    /// subsequent operations will be short-circuited.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = Result.Error&lt;int, string&gt;("Something went wrong");
    /// Console.WriteLine(result.HasValue); // False
    /// Console.WriteLine(result.Error);    // "Something went wrong"
    /// </code>
    /// </example>
    public static Result<T, TError> Error<T, TError>(TError error)
        where TError : notnull
    {
        return new Result<T, TError>(error);
    }
}

/// <summary>
/// Provides utility extension methods for <see cref="Result{T, TError}"/> that enable
/// conversion to asynchronous types and collection operations.
/// </summary>
static partial class Result
{
    /// <summary>
    /// Converts a result to a <see cref="Task{TResult}"/> for use in asynchronous contexts.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="result">The result to convert to a task.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the completed operation with the result.
    /// </returns>
    /// <remarks>
    /// This method is useful when you need to integrate synchronous result values
    /// into asynchronous operation chains. The returned task is already completed.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = Result.Ok&lt;int, string&gt;(42);
    /// Task&lt;Result&lt;int, string&gt;&gt; taskResult = result.AsTask();
    /// var value = await taskResult;
    /// </code>
    /// </example>
    public static Task<Result<T, TError>> AsTask<T, TError>(this in Result<T, TError> result)
        where TError : notnull
        => Task.FromResult(result);

    /// <summary>
    /// Converts a result to a <see cref="ValueTask{TResult}"/> for high-performance asynchronous contexts.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="result">The result to convert to a value task.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that represents the completed operation with the result.
    /// </returns>
    /// <remarks>
    /// This method provides high-performance conversion to ValueTask, which minimizes allocations
    /// when the operation completes synchronously. Prefer this over <see cref="AsTask{T, TError}"/>
    /// in performance-critical scenarios.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = Result.Ok&lt;int, string&gt;(42);
    /// ValueTask&lt;Result&lt;int, string&gt;&gt; taskResult = result.AsValueTask();
    /// var value = await taskResult;
    /// </code>
    /// </example>
    public static ValueTask<Result<T, TError>> AsValueTask<T, TError>(this in Result<T, TError> result)
        where TError : notnull
        => new ValueTask<Result<T, TError>>(result);

    /// <summary>
    /// Converts a sequence of results into a single result containing a sequence of values.
    /// If any result in the sequence represents a failure, the first error encountered is returned.
    /// </summary>
    /// <typeparam name="T">The type of the success values.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="source">The sequence of results to process.</param>
    /// <returns>
    /// A result containing an <see cref="IEnumerable{T}"/> of all success values if all results are successful;
    /// otherwise, a result containing the first error encountered.
    /// </returns>
    /// <remarks>
    /// This method implements the "sequence" operation common in functional programming.
    /// It processes results in order and short-circuits on the first error, making it useful
    /// for validating collections where all items must be valid.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results = new[]
    /// {
    ///     Result.Ok&lt;int, string&gt;(1),
    ///     Result.Ok&lt;int, string&gt;(2),
    ///     Result.Ok&lt;int, string&gt;(3)
    /// };
    /// 
    /// var sequenced = results.Sequence();
    /// // sequenced contains [1, 2, 3]
    /// 
    /// var mixedResults = new[]
    /// {
    ///     Result.Ok&lt;int, string&gt;(1),
    ///     Result.Error&lt;int, string&gt;("error"),
    ///     Result.Ok&lt;int, string&gt;(3)
    /// };
    /// 
    /// var sequenced2 = mixedResults.Sequence();
    /// // sequenced2 contains error "error"
    /// </code>
    /// </example>
    public static Result<IEnumerable<T>, TError> Sequence<T, TError>(this IEnumerable<Result<T, TError>> source)
        where TError : notnull
    {
        var success = new List<T>();
        foreach (var result in source)
        {
            if (result.TryGet(out var value, out var error))
            {
                success.Add(value);
            }
            else
            {
                return Error<IEnumerable<T>, TError>(error);
            }
        }
        return Ok<IEnumerable<T>, TError>(success);
    }
}

/// <summary>
/// Provides LINQ extension methods for <see cref="Result{T, TError}"/> that enable
/// functional programming patterns and Railway Oriented Programming.
/// </summary>
static partial class Result
{
    /// <summary>
    /// Projects the success value of a result into a new form using the specified selector function.
    /// This method implements the functor pattern for results.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source result.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting result.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="source">The result to transform.</param>
    /// <param name="selector">A function to transform the success value if present.</param>
    /// <returns>
    /// A result containing the transformed value if the source result is successful;
    /// otherwise, a result containing the original error.
    /// </returns>
    /// <remarks>
    /// This method enables LINQ's select syntax for results. If the source result contains an error,
    /// the selector function is not called and the error is propagated to the result.
    /// This maintains the Railway Oriented Programming pattern where errors short-circuit the pipeline.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = Result.Ok&lt;int, string&gt;(5);
    /// var doubled = result.Select(x => x * 2);
    /// // doubled contains 10
    /// 
    /// var error = Result.Error&lt;int, string&gt;("failed");
    /// var processed = error.Select(x => x * 2);
    /// // processed contains error "failed"
    /// </code>
    /// </example>
    public static Result<T2, TError> Select<T1, T2, TError>(
        this in Result<T1, TError> source,
        Func<T1, T2> selector)
        where TError : notnull
    {
        if (source.TryGet(out var value, out var error))
        {
            return Ok<T2, TError>(selector(value));
        }
        else
        {
            return Error<T2, TError>(error);
        }
    }

    /// <summary>
    /// Projects the success value of a result to another result and flattens the result.
    /// This method implements the monadic bind operation for results.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source result.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting result.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="source">The result to transform.</param>
    /// <param name="selector">A function that transforms the success value to another result.</param>
    /// <returns>
    /// The result returned by the selector function if the source result is successful;
    /// otherwise, a result containing the original error.
    /// </returns>
    /// <remarks>
    /// This method enables LINQ's from syntax for results and prevents nested results.
    /// It's the core operation for chaining result-producing operations in Railway Oriented Programming.
    /// If either the source result or the result from the selector contains an error, that error is returned.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = Result.Ok&lt;string, string&gt;("42");
    /// var parsed = result.SelectMany(s => 
    ///     int.TryParse(s, out var i) ? Result.Ok&lt;int, string&gt;(i) : Result.Error&lt;int, string&gt;("Invalid number"));
    /// // parsed contains 42
    /// </code>
    /// </example>
    public static Result<T2, TError> SelectMany<T1, T2, TError>(
        this in Result<T1, TError> source,
        Func<T1, Result<T2, TError>> selector)
        where TError : notnull
    {
        if (source.TryGet(out var value, out var error))
        {
            return selector(value);
        }
        else
        {
            return Error<T2, TError>(error);
        }
    }

    /// <summary>
    /// Projects the success value of a result to another result, combines both values using a result selector,
    /// and flattens the result. This overload enables complex LINQ query expressions.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source result.</typeparam>
    /// <typeparam name="TCollection">The type of the value in the intermediate result.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting result.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="source">The source result.</param>
    /// <param name="selector">A function that transforms the source value to an intermediate result.</param>
    /// <param name="resultSelector">A function that combines the source and intermediate values.</param>
    /// <returns>
    /// A result containing the output of the result selector if both the source and intermediate
    /// results are successful; otherwise, a result containing the first error encountered.
    /// </returns>
    /// <remarks>
    /// This overload is used by the C# compiler to support complex LINQ query expressions
    /// with multiple from clauses for results. It maintains Railway Oriented Programming semantics
    /// by short-circuiting on the first error encountered.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = from x in Result.Ok&lt;int, string&gt;(5)
    ///              from y in Result.Ok&lt;int, string&gt;(3)
    ///              select x + y;
    /// // result contains 8
    /// </code>
    /// </example>
    public static Result<T2, TError> SelectMany<T1, TCollection, T2, TError>(
        this Result<T1, TError> source,
        Func<T1, Result<TCollection, TError>> selector,
        Func<T1, TCollection, T2> resultSelector)
        where TError : notnull
    {
        if (source.TryGet(out var value1, out var error1))
        {
            var result1 = selector(value1);
            if (result1.TryGet(out var value2, out var error2))
            {
                var result2 = resultSelector(value1, value2);
                return Ok<T2, TError>(result2);
            }
            else
            {
                return Error<T2, TError>(error2);
            }
        }
        else
        {
            return Error<T2, TError>(error1);
        }
    }

    /// <summary>
    /// Transforms the error value of a result using the specified selector function,
    /// while preserving the success value if present.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TError1">The type of the error value in the source result.</typeparam>
    /// <typeparam name="TError2">The type of the error value in the resulting result.</typeparam>
    /// <param name="source">The result whose error to transform.</param>
    /// <param name="selector">A function to transform the error value if present.</param>
    /// <returns>
    /// A result containing the original success value if the source result is successful;
    /// otherwise, a result containing the transformed error.
    /// </returns>
    /// <remarks>
    /// This method is useful for converting between different error types or adding context to errors.
    /// It allows you to transform errors while keeping the success path unchanged, which is common
    /// when adapting between different layers of an application.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = Result.Error&lt;int, string&gt;("Not found");
    /// var mappedError = result.MapError(error => new Exception(error));
    /// // mappedError contains Exception("Not found")
    /// 
    /// var success = Result.Ok&lt;int, string&gt;(42);
    /// var mappedSuccess = success.MapError(error => new Exception(error));
    /// // mappedSuccess still contains 42
    /// </code>
    /// </example>
    public static Result<T, TError2> MapError<T, TError1, TError2>(
        this Result<T, TError1> source,
        Func<TError1, TError2> selector)
        where TError1 : notnull
        where TError2 : notnull
    {
        if (source.TryGet(out var value, out var error))
        {
            return Ok<T, TError2>(value);
        }
        else
        {
            return Error<T, TError2>(selector(error));
        }
    }
}

/// <summary>
/// Provides asynchronous extension methods for <see cref="Task{TResult}"/> of <see cref="Result{T, TError}"/>
/// that enable LINQ query syntax and Railway Oriented Programming patterns with async/await.
/// </summary>
static partial class Result
{
    /// <summary>
    /// Asynchronously projects the success value of a result wrapped in a task into a new form.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source result.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting result.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="source">The task containing the result to transform.</param>
    /// <param name="selector">A function to transform the success value if present.</param>
    /// <returns>
    /// A task containing a result with the transformed value if the source result is successful;
    /// otherwise, a task containing a result with the original error.
    /// </returns>
    /// <remarks>
    /// This method allows for asynchronous transformation of result values while maintaining
    /// Railway Oriented Programming semantics. The selector function is only called if the result represents success.
    /// </remarks>
    /// <example>
    /// <code>
    /// Task&lt;Result&lt;int, string&gt;&gt; resultTask = GetNumberAsync();
    /// var doubled = await resultTask.Select(x => x * 2);
    /// </code>
    /// </example>
    public static async Task<Result<T2, TError>> Select<T1, T2, TError>(
        this Task<Result<T1, TError>> source,
        Func<T1, T2> selector)
        where TError : notnull
    {
        var result = await source.ConfigureAwait(false);
        if (result.TryGet(out var value, out var error))
        {
            return Ok<T2, TError>(selector(value));
        }
        else
        {
            return Error<T2, TError>(error);
        }
    }

    /// <summary>
    /// Asynchronously projects the success value of a result wrapped in a task to another result task
    /// and flattens the result.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source result.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting result.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="source">The task containing the result to transform.</param>
    /// <param name="selector">A function that asynchronously transforms the success value to another result.</param>
    /// <returns>
    /// A task containing the result returned by the selector function if the source result is successful;
    /// otherwise, a task containing a result with the original error.
    /// </returns>
    /// <remarks>
    /// This method enables asynchronous chaining of result-producing operations while maintaining
    /// Railway Oriented Programming patterns. It prevents nested task and result structures.
    /// </remarks>
    /// <example>
    /// <code>
    /// Task&lt;Result&lt;string, string&gt;&gt; resultTask = GetStringAsync();
    /// var parsed = await resultTask.SelectMany(async s => 
    /// {
    ///     var number = await ParseNumberAsync(s);
    ///     return number.HasValue ? Result.Ok&lt;int, string&gt;(number.Value) : Result.Error&lt;int, string&gt;("Parse failed");
    /// });
    /// </code>
    /// </example>
    public static async Task<Result<T2, TError>> SelectMany<T1, T2, TError>(
        this Task<Result<T1, TError>> source,
        Func<T1, Task<Result<T2, TError>>> selector)
        where TError : notnull
    {
        var result = await source.ConfigureAwait(false);
        if (result.TryGet(out var value, out var error))
        {
            return await selector(value).ConfigureAwait(false);
        }
        else
        {
            return Error<T2, TError>(error);
        }
    }

    /// <summary>
    /// Asynchronously projects the success value of a result wrapped in a task to another result task,
    /// combines both values using a result selector, and flattens the result.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source result.</typeparam>
    /// <typeparam name="TCollection">The type of the value in the intermediate result.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting result.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="source">The task containing the source result.</param>
    /// <param name="selector">A function that asynchronously transforms the source value to an intermediate result.</param>
    /// <param name="resultSelector">A function that combines the source and intermediate values.</param>
    /// <returns>
    /// A task containing a result with the output of the result selector if both the source and intermediate
    /// results are successful; otherwise, a task containing a result with the first error encountered.
    /// </returns>
    /// <remarks>
    /// This overload enables complex asynchronous LINQ query expressions with multiple from clauses for results.
    /// It maintains Railway Oriented Programming semantics in asynchronous contexts.
    /// </remarks>
    public static async Task<Result<T2, TError>> SelectMany<T1, TCollection, T2, TError>(
        this Task<Result<T1, TError>> source,
        Func<T1, Task<Result<TCollection, TError>>> selector,
        Func<T1, TCollection, T2> resultSelector)
        where TError : notnull
    {
        var result1 = await source.ConfigureAwait(false);
        if (result1.TryGet(out var value1, out var error1))
        {
            var result2 = await selector(value1).ConfigureAwait(false);
            if (result2.TryGet(out var value2, out var error2))
            {
                var result3 = resultSelector(value1, value2);
                return Ok<T2, TError>(result3);
            }
            else
            {
                return Error<T2, TError>(error2);
            }
        }
        else
        {
            return Error<T2, TError>(error1);
        }
    }

    /// <summary>
    /// Asynchronously transforms the error value of a result wrapped in a task using the specified selector function.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TError1">The type of the error value in the source result.</typeparam>
    /// <typeparam name="TError2">The type of the error value in the resulting result.</typeparam>
    /// <param name="source">The task containing the result whose error to transform.</param>
    /// <param name="selector">A function to transform the error value if present.</param>
    /// <returns>
    /// A task containing a result with the original success value if the source result is successful;
    /// otherwise, a task containing a result with the transformed error.
    /// </returns>
    /// <remarks>
    /// This method enables asynchronous error transformation while preserving Railway Oriented Programming patterns.
    /// It's useful for adapting error types between different layers in asynchronous applications.
    /// </remarks>
    /// <example>
    /// <code>
    /// Task&lt;Result&lt;int, string&gt;&gt; resultTask = GetNumberAsync();
    /// var mappedError = await resultTask.MapError(error => new Exception(error));
    /// </code>
    /// </example>
    public static async Task<Result<T, TError2>> MapError<T, TError1, TError2>(
        this Task<Result<T, TError1>> source,
        Func<TError1, TError2> selector)
        where TError1 : notnull
        where TError2 : notnull
    {
        var result = await source.ConfigureAwait(false);
        if (result.TryGet(out var value, out var error))
        {
            return Ok<T, TError2>(value);
        }
        else
        {
            return Error<T, TError2>(selector(error));
        }
    }
}

/// <summary>
/// Provides asynchronous extension methods for <see cref="ValueTask{TResult}"/> of <see cref="Result{T, TError}"/>
/// that enable LINQ query syntax and Railway Oriented Programming patterns with high-performance async operations.
/// </summary>
static partial class Result
{
    /// <summary>
    /// Asynchronously projects the success value of a result wrapped in a value task into a new form.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source result.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting result.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="source">The value task containing the result to transform.</param>
    /// <param name="selector">A function to transform the success value if present.</param>
    /// <returns>
    /// A value task containing a result with the transformed value if the source result is successful;
    /// otherwise, a value task containing a result with the original error.
    /// </returns>
    /// <remarks>
    /// This method provides high-performance asynchronous transformation of result values using ValueTask
    /// to minimize allocations when operations complete synchronously. It maintains Railway Oriented Programming semantics.
    /// </remarks>
    /// <example>
    /// <code>
    /// ValueTask&lt;Result&lt;int, string&gt;&gt; resultTask = GetNumberValueTaskAsync();
    /// var doubled = await resultTask.Select(x => x * 2);
    /// </code>
    /// </example>
    public static async ValueTask<Result<T2, TError>> Select<T1, T2, TError>(
        this ValueTask<Result<T1, TError>> source,
        Func<T1, T2> selector)
        where TError : notnull
    {
        var result = await source.ConfigureAwait(false);
        if (result.TryGet(out var value, out var error))
        {
            return Ok<T2, TError>(selector(value));
        }
        else
        {
            return Error<T2, TError>(error);
        }
    }

    /// <summary>
    /// Asynchronously projects the success value of a result wrapped in a value task to another result value task
    /// and flattens the result.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source result.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting result.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="source">The value task containing the result to transform.</param>
    /// <param name="selector">A function that asynchronously transforms the success value to another result using ValueTask.</param>
    /// <returns>
    /// A value task containing the result returned by the selector function if the source result is successful;
    /// otherwise, a value task containing a result with the original error.
    /// </returns>
    /// <remarks>
    /// This method enables high-performance asynchronous chaining of result-producing operations using ValueTask
    /// to minimize allocations. It maintains Railway Oriented Programming patterns while optimizing for performance.
    /// </remarks>
    /// <example>
    /// <code>
    /// ValueTask&lt;Result&lt;string, string&gt;&gt; resultTask = GetStringValueTaskAsync();
    /// var parsed = await resultTask.SelectMany(async s => 
    /// {
    ///     var number = await ParseNumberValueTaskAsync(s);
    ///     return number.HasValue ? Result.Ok&lt;int, string&gt;(number.Value) : Result.Error&lt;int, string&gt;("Parse failed");
    /// });
    /// </code>
    /// </example>
    public static async ValueTask<Result<T2, TError>> SelectMany<T1, T2, TError>(
        this ValueTask<Result<T1, TError>> source,
        Func<T1, ValueTask<Result<T2, TError>>> selector)
        where TError : notnull
    {
        var result = await source.ConfigureAwait(false);
        if (result.TryGet(out var value, out var error))
        {
            return await selector(value).ConfigureAwait(false);
        }
        else
        {
            return Error<T2, TError>(error);
        }
    }

    /// <summary>
    /// Asynchronously projects the success value of a result wrapped in a value task to another result value task,
    /// combines both values using a result selector, and flattens the result.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source result.</typeparam>
    /// <typeparam name="TCollection">The type of the value in the intermediate result.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting result.</typeparam>
    /// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
    /// <param name="source">The value task containing the source result.</param>
    /// <param name="selector">A function that asynchronously transforms the source value to an intermediate result using ValueTask.</param>
    /// <param name="resultSelector">A function that combines the source and intermediate values.</param>
    /// <returns>
    /// A value task containing a result with the output of the result selector if both the source and intermediate
    /// results are successful; otherwise, a value task containing a result with the first error encountered.
    /// </returns>
    /// <remarks>
    /// This overload enables complex high-performance asynchronous LINQ query expressions with multiple from clauses for results.
    /// It uses ValueTask to minimize allocations while maintaining Railway Oriented Programming semantics.
    /// </remarks>
    public static async ValueTask<Result<T2, TError>> SelectMany<T1, TCollection, T2, TError>(
        this ValueTask<Result<T1, TError>> source,
        Func<T1, ValueTask<Result<TCollection, TError>>> selector,
        Func<T1, TCollection, T2> resultSelector)
        where TError : notnull
    {
        var result1 = await source.ConfigureAwait(false);
        if (result1.TryGet(out var value1, out var error1))
        {
            var result2 = await selector(value1).ConfigureAwait(false);
            if (result2.TryGet(out var value2, out var error2))
            {
                var result3 = resultSelector(value1, value2);
                return Ok<T2, TError>(result3);
            }
            else
            {
                return Error<T2, TError>(error2);
            }
        }
        else
        {
            return Error<T2, TError>(error1);
        }
    }

    /// <summary>
    /// Asynchronously transforms the error value of a result wrapped in a value task using the specified selector function.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TError1">The type of the error value in the source result.</typeparam>
    /// <typeparam name="TError2">The type of the error value in the resulting result.</typeparam>
    /// <param name="source">The value task containing the result whose error to transform.</param>
    /// <param name="selector">A function to transform the error value if present.</param>
    /// <returns>
    /// A value task containing a result with the original success value if the source result is successful;
    /// otherwise, a value task containing a result with the transformed error.
    /// </returns>
    /// <remarks>
    /// This method enables high-performance asynchronous error transformation using ValueTask to minimize allocations.
    /// It maintains Railway Oriented Programming patterns while optimizing for performance in error handling scenarios.
    /// </remarks>
    /// <example>
    /// <code>
    /// ValueTask&lt;Result&lt;int, string&gt;&gt; resultTask = GetNumberValueTaskAsync();
    /// var mappedError = await resultTask.MapError(error => new Exception(error));
    /// </code>
    /// </example>
    public static async ValueTask<Result<T, TError2>> MapError<T, TError1, TError2>(
        this ValueTask<Result<T, TError1>> source,
        Func<TError1, TError2> selector)
        where TError1 : notnull
        where TError2 : notnull
    {
        var result = await source.ConfigureAwait(false);
        if (result.TryGet(out var value, out var error))
        {
            return Ok<T, TError2>(value);
        }
        else
        {
            return Error<T, TError2>(selector(error));
        }
    }
}

