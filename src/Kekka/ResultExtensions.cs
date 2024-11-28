using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kekka;

public static partial class ResultExtensions
{
    public static Task<Result<TSuccess, TFailure>> AsTask<TSuccess, TFailure>(this Result<TSuccess, TFailure> result) =>
        Task.FromResult(result);

    public static ValueTask<Result<TSuccess, TFailure>> AsValueTask<TSuccess, TFailure>(this Result<TSuccess, TFailure> result) =>
        new ValueTask<Result<TSuccess, TFailure>>(result);

    public static Result<IEnumerable<TSuccess>, TFailure> Sequence<TSuccess, TFailure>(this IEnumerable<Result<TSuccess, TFailure>> source)
    {
        var success = new List<TSuccess>();
        foreach (var result in source)
        {
            if (result is Ok<TSuccess, TFailure> ok)
            {
                success.Add(ok.Value);
            }
            else if (result is Error<TSuccess, TFailure> error)
            {
                return Result.Error<IEnumerable<TSuccess>, TFailure>(error.Value);
            }
            else
            {
                throw new NotSupportedException($"{source.GetType().FullName} is not supported.");
            }
        }
        return Result.Ok<IEnumerable<TSuccess>, TFailure>(success);
    }
}

static partial class ResultExtensions
{
    public static Result<TSuccess2, TFailure> Select<TSuccess1, TSuccess2, TFailure>(
        this Result<TSuccess1, TFailure> source,
        Func<TSuccess1, TSuccess2> selector)
    {
        if (source is Ok<TSuccess1, TFailure> ok)
        {
            return Result.Ok<TSuccess2, TFailure>(selector(ok.Value));
        }
        else if (source is Error<TSuccess1, TFailure> error)
        {
            return Result.Error<TSuccess2, TFailure>(error.Value);
        }
        else
        {
            throw new NotSupportedException($"{source.GetType().FullName} is not supported.");
        }
    }

    public static Result<TSuccess2, TFailure> SelectMany<TSuccess1, TSuccess2, TFailure>(
        this Result<TSuccess1, TFailure> source,
        Func<TSuccess1, Result<TSuccess2, TFailure>> selector)
    {
        if (source is Ok<TSuccess1, TFailure> ok)
        {
            return selector(ok.Value);
        }
        else if (source is Error<TSuccess1, TFailure> error)
        {
            return Result.Error<TSuccess2, TFailure>(error.Value);
        }
        else
        {
            throw new NotSupportedException($"{source.GetType().FullName} is not supported.");
        }
    }

    public static Result<TSuccess2, TFailure> SelectMany<TSuccess1, TCollection, TSuccess2, TFailure>(
        this Result<TSuccess1, TFailure> source,
        Func<TSuccess1, Result<TCollection, TFailure>> selector,
        Func<TSuccess1, TCollection, TSuccess2> resultSelector)
    {
        if (source is Ok<TSuccess1, TFailure> ok)
        {
            var result = selector(ok.Value);
            if (result is Ok<TCollection, TFailure> ok2)
            {
                var result2 = resultSelector(ok.Value, ok2.Value);
                return Result.Ok<TSuccess2, TFailure>(result2);
            }
            else if (result is Error<TCollection, TFailure> error2)
            {
                return Result.Error<TSuccess2, TFailure>(error2.Value);
            }
            else
            {
                throw new NotSupportedException($"{source.GetType().FullName} is not supported.");
            }
        }
        else if (source is Error<TSuccess1, TFailure> error)
        {
            return Result.Error<TSuccess2, TFailure>(error.Value);
        }
        else
        {
            throw new NotSupportedException($"{source.GetType().FullName} is not supported.");
        }
    }

    public static Result<TSuccess, TFailure2> MapError<TSuccess, TFailure1, TFailure2>(
        this Result<TSuccess, TFailure1> source,
        Func<TFailure1, TFailure2> selector)
    {
        if (source is Ok<TSuccess, TFailure1> ok)
        {
            return Result.Ok<TSuccess, TFailure2>(ok.Value);
        }
        else if (source is Error<TSuccess, TFailure1> error)
        {
            return Result.Error<TSuccess, TFailure2>(selector(error.Value));
        }
        else
        {
            throw new NotSupportedException($"{source.GetType().FullName} is not supported.");
        }
    }

}

static partial class ResultExtensions
{
    public static async Task<Result<TSuccess2, TFailure>> Select<TSuccess1, TSuccess2, TFailure>(
        this Task<Result<TSuccess1, TFailure>> source,
        Func<TSuccess1, TSuccess2> selector)
    {
        var result = await source;
        if (result is Ok<TSuccess1, TFailure> ok)
        {
            return Result.Ok<TSuccess2, TFailure>(selector(ok.Value));
        }
        else if (result is Error<TSuccess1, TFailure> error)
        {
            return Result.Error<TSuccess2, TFailure>(error.Value);
        }
        else
        {
            throw new NotSupportedException($"{result.GetType().FullName} is not supported.");
        }
    }

    public static async Task<Result<TSuccess2, TFailure>> SelectMany<TSuccess1, TSuccess2, TFailure>(
        this Task<Result<TSuccess1, TFailure>> source,
        Func<TSuccess1, Task<Result<TSuccess2, TFailure>>> selector)
    {
        var result = await source;
        if (result is Ok<TSuccess1, TFailure> ok)
        {
            return await selector(ok.Value);
        }
        else if (result is Error<TSuccess1, TFailure> error)
        {
            return Result.Error<TSuccess2, TFailure>(error.Value);
        }
        else
        {
            throw new NotSupportedException($"{result.GetType().FullName} is not supported.");
        }
    }

    public static async Task<Result<TSuccess2, TFailure>> SelectMany<TSuccess1, TCollection, TSuccess2, TFailure>(
        this Task<Result<TSuccess1, TFailure>> source,
        Func<TSuccess1, Task<Result<TCollection, TFailure>>> selector,
        Func<TSuccess1, TCollection, TSuccess2> resultSelector)
    {
        var result = await source;
        if (result is Ok<TSuccess1, TFailure> ok)
        {
            var result2 = await selector(ok.Value);
            if (result2 is Ok<TCollection, TFailure> ok2)
            {
                var result3 = resultSelector(ok.Value, ok2.Value);
                return Result.Ok<TSuccess2, TFailure>(result3);
            }
            else if (result2 is Error<TCollection, TFailure> error2)
            {
                return Result.Error<TSuccess2, TFailure>(error2.Value);
            }
            else
            {
                throw new NotSupportedException($"{result2.GetType().FullName} is not supported.");
            }
        }
        else if (result is Error<TSuccess1, TFailure> error)
        {
            return Result.Error<TSuccess2, TFailure>(error.Value);
        }
        else
        {
            throw new NotSupportedException($"{result.GetType().FullName} is not supported.");
        }
    }

    public static async Task<Result<TSuccess, TFailure2>> MapError<TSuccess, TFailure1, TFailure2>(
        this Task<Result<TSuccess, TFailure1>> source,
        Func<TFailure1, TFailure2> selector)
    {
        var result = await source;
        if (result is Ok<TSuccess, TFailure1> ok)
        {
            return Result.Ok<TSuccess, TFailure2>(ok.Value);
        }
        else if (result is Error<TSuccess, TFailure1> error)
        {
            return Result.Error<TSuccess, TFailure2>(selector(error.Value));
        }
        else
        {
            throw new NotSupportedException($"{result.GetType().FullName} is not supported.");
        }
    }
}

static partial class ResultExtensions
{
    public static async ValueTask<Result<TSuccess2, TFailure>> Select<TSuccess1, TSuccess2, TFailure>(
        this ValueTask<Result<TSuccess1, TFailure>> source,
        Func<TSuccess1, TSuccess2> selector)
    {
        var result = await source;
        if (result is Ok<TSuccess1, TFailure> ok)
        {
            return Result.Ok<TSuccess2, TFailure>(selector(ok.Value));
        }
        else if (result is Error<TSuccess1, TFailure> error)
        {
            return Result.Error<TSuccess2, TFailure>(error.Value);
        }
        else
        {
            throw new NotSupportedException($"{result.GetType().FullName} is not supported.");
        }
    }

    public static async ValueTask<Result<TSuccess2, TFailure>> SelectMany<TSuccess1, TSuccess2, TFailure>(
        this ValueTask<Result<TSuccess1, TFailure>> source,
        Func<TSuccess1, ValueTask<Result<TSuccess2, TFailure>>> selector)
    {
        var result = await source;
        if (result is Ok<TSuccess1, TFailure> ok)
        {
            return await selector(ok.Value);
        }
        else if (result is Error<TSuccess1, TFailure> error)
        {
            return Result.Error<TSuccess2, TFailure>(error.Value);
        }
        else
        {
            throw new NotSupportedException($"{result.GetType().FullName} is not supported.");
        }
    }

    public static async ValueTask<Result<TSuccess2, TFailure>> SelectMany<TSuccess1, TCollection, TSuccess2, TFailure>(
        this ValueTask<Result<TSuccess1, TFailure>> source,
        Func<TSuccess1, ValueTask<Result<TCollection, TFailure>>> selector,
        Func<TSuccess1, TCollection, TSuccess2> resultSelector)
    {
        var result = await source;
        if (result is Ok<TSuccess1, TFailure> ok)
        {
            var result2 = await selector(ok.Value);
            if (result2 is Ok<TCollection, TFailure> ok2)
            {
                var result3 = resultSelector(ok.Value, ok2.Value);
                return Result.Ok<TSuccess2, TFailure>(result3);
            }
            else if (result2 is Error<TCollection, TFailure> error2)
            {
                return Result.Error<TSuccess2, TFailure>(error2.Value);
            }
            else
            {
                throw new NotSupportedException($"{result2.GetType().FullName} is not supported.");
            }
        }
        else if (result is Error<TSuccess1, TFailure> error)
        {
            return Result.Error<TSuccess2, TFailure>(error.Value);
        }
        else
        {
            throw new NotSupportedException($"{result.GetType().FullName} is not supported.");
        }
    }

    public static async ValueTask<Result<TSuccess, TFailure2>> MapError<TSuccess, TFailure1, TFailure2>(
        this ValueTask<Result<TSuccess, TFailure1>> source,
        Func<TFailure1, TFailure2> selector)
    {
        var result = await source;
        if (result is Ok<TSuccess, TFailure1> ok)
        {
            return Result.Ok<TSuccess, TFailure2>(ok.Value);
        }
        else if (result is Error<TSuccess, TFailure1> error)
        {
            return Result.Error<TSuccess, TFailure2>(selector(error.Value));
        }
        else
        {
            throw new NotSupportedException($"{result.GetType().FullName} is not supported.");
        }
    }
}


