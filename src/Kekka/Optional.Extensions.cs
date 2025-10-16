using System;
using System.Threading.Tasks;

namespace Kekka;

/// <summary>
/// Provides factory methods for creating <see cref="Optional{T}"/> instances.
/// </summary>
public static partial class Optional
{
    /// <summary>
    /// Creates an optional that contains the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value. Must be a non-null reference type.</typeparam>
    /// <param name="value">The value to wrap in an optional.</param>
    /// <returns>
    /// An <see cref="Optional{T}"/> that contains the specified value.
    /// </returns>
    /// <remarks>
    /// This is the preferred way to create an optional with a value.
    /// The value parameter must not be null since T is constrained to be non-null.
    /// </remarks>
    /// <example>
    /// <code>
    /// var optional = Optional.Some("Hello World");
    /// Console.WriteLine(optional.HasValue); // True
    /// </code>
    /// </example>
    public static Optional<T> Some<T>(T value)
        where T : notnull
        => new Optional<T>(value);

    /// <summary>
    /// Creates an empty optional that contains no value.
    /// </summary>
    /// <typeparam name="T">The type of the value that the optional could contain. Must be a non-null reference type.</typeparam>
    /// <returns>
    /// An <see cref="Optional{T}"/> that contains no value (None state).
    /// </returns>
    /// <remarks>
    /// This method creates an optional in the None state, representing the absence of a value.
    /// It is equivalent to using <see cref="Optional{T}.None"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var optional = Optional.None&lt;string&gt;();
    /// Console.WriteLine(optional.HasValue); // False
    /// </code>
    /// </example>
    public static Optional<T> None<T>()
        where T : notnull
        => Optional<T>.None;
}

static partial class Optional
{
    /// <summary>
    /// Projects the value of an optional into a new form using the specified selector function.
    /// This method implements the functor pattern for optionals.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source optional.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting optional.</typeparam>
    /// <param name="optional">The optional to transform.</param>
    /// <param name="selector">A function to transform the value if present.</param>
    /// <returns>
    /// An optional containing the transformed value if the source optional contains a value;
    /// otherwise, an empty optional.
    /// </returns>
    /// <remarks>
    /// This method enables LINQ's select syntax for optionals. If the source optional is empty,
    /// the selector function is not called and an empty optional is returned.
    /// </remarks>
    /// <example>
    /// <code>
    /// var optional = Optional.Some("hello");
    /// var result = optional.Select(s => s.ToUpper());
    /// // result contains "HELLO"
    /// 
    /// var empty = Optional.None&lt;string&gt;();
    /// var result2 = empty.Select(s => s.ToUpper());
    /// // result2 is empty (None)
    /// </code>
    /// </example>
    public static Optional<T2> Select<T1, T2>(
        this in Optional<T1> optional,
        Func<T1, T2> selector)
        where T1 : notnull
        where T2 : notnull
    {
        if (optional.TryGet(out var value))
        {
            return Some(selector(value));
        }
        else
        {
            return None<T2>();
        }
    }

    /// <summary>
    /// Projects the value of an optional to another optional and flattens the result.
    /// This method implements the monadic bind operation for optionals.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source optional.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting optional.</typeparam>
    /// <param name="optional">The optional to transform.</param>
    /// <param name="selector">A function that transforms the value to another optional.</param>
    /// <returns>
    /// The optional returned by the selector function if the source optional contains a value;
    /// otherwise, an empty optional.
    /// </returns>
    /// <remarks>
    /// This method enables LINQ's from syntax for optionals and prevents nested optionals.
    /// It's useful for chaining operations that might fail.
    /// </remarks>
    /// <example>
    /// <code>
    /// var optional = Optional.Some("42");
    /// var result = optional.SelectMany(s => 
    ///     int.TryParse(s, out var i) ? Optional.Some(i) : Optional.None&lt;int&gt;());
    /// // result contains 42
    /// </code>
    /// </example>
    public static Optional<T2> SelectMany<T1, T2>(
        this in Optional<T1> optional,
        Func<T1, Optional<T2>> selector)
        where T1 : notnull
        where T2 : notnull
    {
        if (optional.TryGet(out var value))
        {
            return selector(value);
        }
        else
        {
            return None<T2>();
        }
    }

    /// <summary>
    /// Projects the value of an optional to another optional, combines both values using a result selector,
    /// and flattens the result. This overload enables complex LINQ query expressions.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source optional.</typeparam>
    /// <typeparam name="TCollection">The type of the value in the intermediate optional.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting optional.</typeparam>
    /// <param name="source">The source optional.</param>
    /// <param name="selector">A function that transforms the source value to an intermediate optional.</param>
    /// <param name="resultSelector">A function that combines the source and intermediate values.</param>
    /// <returns>
    /// An optional containing the result of the result selector if both the source and intermediate
    /// optionals contain values; otherwise, an empty optional.
    /// </returns>
    /// <remarks>
    /// This overload is used by the C# compiler to support complex LINQ query expressions
    /// with multiple from clauses for optionals.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = from x in Optional.Some(5)
    ///              from y in Optional.Some(3)
    ///              select x + y;
    /// // result contains 8
    /// </code>
    /// </example>
    public static Optional<T2> SelectMany<T1, TCollection, T2>(
        this in Optional<T1> source,
        Func<T1, Optional<TCollection>> selector,
        Func<T1, TCollection, T2> resultSelector)
        where T1 : notnull
        where T2 : notnull
        where TCollection : notnull
    {
        if (source.TryGet(out var value1))
        {
            var result1 = selector(value1);
            if (result1.TryGet(out var value2))
            {
                var result2 = resultSelector(value1, value2);
                return Some(result2);
            }
            else
            {
                return None<T2>();
            }
        }
        else
        {
            return None<T2>();
        }
    }
}

static partial class Optional
{
    /// <summary>
    /// Asynchronously projects the value of an optional wrapped in a task into a new form.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source optional.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting optional.</typeparam>
    /// <param name="optional">The task containing the optional to transform.</param>
    /// <param name="selector">A function to transform the value if present.</param>
    /// <returns>
    /// A task containing an optional with the transformed value if the source optional contains a value;
    /// otherwise, a task containing an empty optional.
    /// </returns>
    /// <remarks>
    /// This method allows for asynchronous transformation of optional values while maintaining
    /// the optional semantics. The selector function is only called if the optional contains a value.
    /// </remarks>
    /// <example>
    /// <code>
    /// Task&lt;Optional&lt;string&gt;&gt; optionalTask = GetOptionalStringAsync();
    /// var result = await optionalTask.Select(s => s.ToUpper());
    /// </code>
    /// </example>
    public static async Task<Optional<T2>> Select<T1, T2>(
        this Task<Optional<T1>> optional,
        Func<T1, T2> selector)
        where T1 : notnull
        where T2 : notnull
    {
        if ((await optional.ConfigureAwait(false)).TryGet(out var value))
        {
            return Some(selector(value));
        }
        else
        {
            return None<T2>();
        }
    }

    /// <summary>
    /// Asynchronously projects the value of an optional wrapped in a task to another optional task
    /// and flattens the result.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source optional.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting optional.</typeparam>
    /// <param name="optional">The task containing the optional to transform.</param>
    /// <param name="selector">A function that asynchronously transforms the value to another optional.</param>
    /// <returns>
    /// A task containing the optional returned by the selector function if the source optional contains a value;
    /// otherwise, a task containing an empty optional.
    /// </returns>
    /// <remarks>
    /// This method enables asynchronous chaining of operations that return optionals,
    /// preventing nested task and optional structures.
    /// </remarks>
    /// <example>
    /// <code>
    /// Task&lt;Optional&lt;string&gt;&gt; optionalTask = GetOptionalStringAsync();
    /// var result = await optionalTask.SelectMany(async s => 
    /// {
    ///     var parsed = await ParseAsync(s);
    ///     return parsed.HasValue ? Optional.Some(parsed.Value) : Optional.None&lt;int&gt;();
    /// });
    /// </code>
    /// </example>
    public static async Task<Optional<T2>> SelectMany<T1, T2>(
        this Task<Optional<T1>> optional,
        Func<T1, Task<Optional<T2>>> selector)
        where T1 : notnull
        where T2 : notnull
    {
        if ((await optional.ConfigureAwait(false)).TryGet(out var value))
        {
            return await selector(value).ConfigureAwait(false);
        }
        else
        {
            return None<T2>();
        }
    }

    /// <summary>
    /// Asynchronously projects the value of an optional wrapped in a task to another optional task,
    /// combines both values using a result selector, and flattens the result.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source optional.</typeparam>
    /// <typeparam name="TCollection">The type of the value in the intermediate optional.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting optional.</typeparam>
    /// <param name="source">The task containing the source optional.</param>
    /// <param name="selector">A function that asynchronously transforms the source value to an intermediate optional.</param>
    /// <param name="resultSelector">A function that combines the source and intermediate values.</param>
    /// <returns>
    /// A task containing an optional with the result of the result selector if both the source and intermediate
    /// optionals contain values; otherwise, a task containing an empty optional.
    /// </returns>
    /// <remarks>
    /// This overload enables complex asynchronous LINQ query expressions with multiple from clauses for optionals.
    /// </remarks>
    public static async Task<Optional<T2>> SelectMany<T1, TCollection, T2>(
        this Task<Optional<T1>> source,
        Func<T1, Task<Optional<TCollection>>> selector,
        Func<T1, TCollection, T2> resultSelector)
        where T1 : notnull
        where T2 : notnull
        where TCollection : notnull
    {
        if ((await source.ConfigureAwait(false)).TryGet(out var value1))
        {
            var result1 = await selector(value1).ConfigureAwait(false);
            if (result1.TryGet(out var value2))
            {
                var result2 = resultSelector(value1, value2);
                return Some(result2);
            }
            else
            {
                return None<T2>();
            }
        }
        else
        {
            return None<T2>();
        }
    }
}

static partial class Optional
{
    /// <summary>
    /// Asynchronously projects the value of an optional wrapped in a value task into a new form.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source optional.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting optional.</typeparam>
    /// <param name="optional">The value task containing the optional to transform.</param>
    /// <param name="selector">A function to transform the value if present.</param>
    /// <returns>
    /// A value task containing an optional with the transformed value if the source optional contains a value;
    /// otherwise, a value task containing an empty optional.
    /// </returns>
    /// <remarks>
    /// This method provides high-performance asynchronous transformation of optional values
    /// using ValueTask to minimize allocations when the operation completes synchronously.
    /// </remarks>
    /// <example>
    /// <code>
    /// ValueTask&lt;Optional&lt;string&gt;&gt; optionalTask = GetOptionalStringValueTaskAsync();
    /// var result = await optionalTask.Select(s => s.ToUpper());
    /// </code>
    /// </example>
    public static async ValueTask<Optional<T2>> Select<T1, T2>(
        this ValueTask<Optional<T1>> optional,
        Func<T1, T2> selector)
        where T1 : notnull
        where T2 : notnull
    {
        if ((await optional.ConfigureAwait(false)).TryGet(out var value))
        {
            return Some(selector(value));
        }
        else
        {
            return None<T2>();
        }
    }

    /// <summary>
    /// Asynchronously projects the value of an optional wrapped in a value task to another optional value task
    /// and flattens the result.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source optional.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting optional.</typeparam>
    /// <param name="optional">The value task containing the optional to transform.</param>
    /// <param name="selector">A function that asynchronously transforms the value to another optional using ValueTask.</param>
    /// <returns>
    /// A value task containing the optional returned by the selector function if the source optional contains a value;
    /// otherwise, a value task containing an empty optional.
    /// </returns>
    /// <remarks>
    /// This method enables high-performance asynchronous chaining of operations that return optionals,
    /// using ValueTask to minimize allocations and improve performance in synchronous completion scenarios.
    /// </remarks>
    /// <example>
    /// <code>
    /// ValueTask&lt;Optional&lt;string&gt;&gt; optionalTask = GetOptionalStringValueTaskAsync();
    /// var result = await optionalTask.SelectMany(async s => 
    /// {
    ///     var parsed = await ParseValueTaskAsync(s);
    ///     return parsed.HasValue ? Optional.Some(parsed.Value) : Optional.None&lt;int&gt;();
    /// });
    /// </code>
    /// </example>
    public static async ValueTask<Optional<T2>> SelectMany<T1, T2>(
        this ValueTask<Optional<T1>> optional,
        Func<T1, ValueTask<Optional<T2>>> selector)
        where T1 : notnull
        where T2 : notnull
    {
        if ((await optional.ConfigureAwait(false)).TryGet(out var value))
        {
            return await selector(value).ConfigureAwait(false);
        }
        else
        {
            return None<T2>();
        }
    }

    /// <summary>
    /// Asynchronously projects the value of an optional wrapped in a value task to another optional value task,
    /// combines both values using a result selector, and flattens the result.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the source optional.</typeparam>
    /// <typeparam name="TCollection">The type of the value in the intermediate optional.</typeparam>
    /// <typeparam name="T2">The type of the value in the resulting optional.</typeparam>
    /// <param name="source">The value task containing the source optional.</param>
    /// <param name="selector">A function that asynchronously transforms the source value to an intermediate optional using ValueTask.</param>
    /// <param name="resultSelector">A function that combines the source and intermediate values.</param>
    /// <returns>
    /// A value task containing an optional with the result of the result selector if both the source and intermediate
    /// optionals contain values; otherwise, a value task containing an empty optional.
    /// </returns>
    /// <remarks>
    /// This overload enables complex high-performance asynchronous LINQ query expressions with multiple from clauses for optionals,
    /// using ValueTask to minimize allocations and improve performance.
    /// </remarks>
    public static async ValueTask<Optional<T2>> SelectMany<T1, TCollection, T2>(
        this ValueTask<Optional<T1>> source,
        Func<T1, ValueTask<Optional<TCollection>>> selector,
        Func<T1, TCollection, T2> resultSelector)
        where T1 : notnull
        where T2 : notnull
        where TCollection : notnull
    {
        if ((await source.ConfigureAwait(false)).TryGet(out var value1))
        {
            var result1 = await selector(value1).ConfigureAwait(false);
            if (result1.TryGet(out var value2))
            {
                var result2 = resultSelector(value1, value2);
                return Some(result2);
            }
            else
            {
                return None<T2>();
            }
        }
        else
        {
            return None<T2>();
        }
    }
}

