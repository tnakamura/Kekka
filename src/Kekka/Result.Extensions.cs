using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kekka;

public static partial class Result
{
    public static Result<T, TError> Ok<T, TError>(T value)
        where TError : notnull
    {
        return new Result<T, TError>(value);
    }

    public static Result<T, TError> Error<T, TError>(TError error)
        where TError : notnull
    {
        return new Result<T, TError>(error);
    }
}

static partial class Result
{
    public static Task<Result<T, TError>> AsTask<T, TError>(this in Result<T, TError> result)
        where TError : notnull
        => Task.FromResult(result);

    public static ValueTask<Result<T, TError>> AsValueTask<T, TError>(this in Result<T, TError> result)
        where TError : notnull
        => new ValueTask<Result<T, TError>>(result);

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

static partial class Result
{
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

static partial class Result
{
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

static partial class Result
{
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

