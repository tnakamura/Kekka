using System;
using System.Threading.Tasks;

namespace Kekka;

public abstract class Result<TSuccess, TFailure>
{
    private protected Result() { }
}

public sealed class OkResult<TSuccess, TFailure> : Result<TSuccess, TFailure>
{
    internal OkResult(TSuccess value)
    {
        Value = value;
    }

    public TSuccess Value { get; }
}

public sealed class ErrorResult<TSuccess, TFailure> : Result<TSuccess, TFailure>
{
    internal ErrorResult(TFailure value)
    {
        Value = value;
    }

    public TFailure Value { get; }
}

public static class Result
{
    public static Result<TSuccess, TFailure> Ok<TSuccess, TFailure>(TSuccess value)
    {
        return new OkResult<TSuccess, TFailure>(value);
    }

    public static Result<TSuccess, TFailure> Error<TSuccess, TFailure>(TFailure value)
    {
        return new ErrorResult<TSuccess, TFailure>(value);
    }
}

public static partial class ResultExtensions
{
    public static Task<Result<TSuccess, TFailure>> AsTask<TSuccess, TFailure>(this Result<TSuccess, TFailure> result) =>
        Task.FromResult(result);
}

static partial class ResultExtensions
{
    public static Result<TSuccess2, TFailure> Select<TSuccess1, TSuccess2, TFailure>(
        this Result<TSuccess1, TFailure> source,
        Func<TSuccess1, TSuccess2> selector)
    {
        if (source is OkResult<TSuccess1, TFailure> ok)
        {
            return Result.Ok<TSuccess2, TFailure>(selector(ok.Value));
        }
        else if (source is ErrorResult<TSuccess1, TFailure> error)
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
        if (source is OkResult<TSuccess1, TFailure> ok)
        {
            return selector(ok.Value);
        }
        else if (source is ErrorResult<TSuccess1, TFailure> error)
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
        if (source is OkResult<TSuccess1, TFailure> ok)
        {
            var result = selector(ok.Value);
            if (result is OkResult<TCollection, TFailure> ok2)
            {
                var result2 = resultSelector(ok.Value, ok2.Value);
                return Result.Ok<TSuccess2, TFailure>(result2);
            }
            else if (result is ErrorResult<TCollection, TFailure> error2)
            {
                return Result.Error<TSuccess2, TFailure>(error2.Value);
            }
            else
            {
                throw new NotSupportedException($"{source.GetType().FullName} is not supported.");
            }
        }
        else if (source is ErrorResult<TSuccess1, TFailure> error)
        {
            return Result.Error<TSuccess2, TFailure>(error.Value);
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
        if (result is OkResult<TSuccess1, TFailure> ok)
        {
            return Result.Ok<TSuccess2, TFailure>(selector(ok.Value));
        }
        else if (result is ErrorResult<TSuccess1, TFailure> error)
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
        if (result is OkResult<TSuccess1, TFailure> ok)
        {
            return await selector(ok.Value);
        }
        else if (result is ErrorResult<TSuccess1, TFailure> error)
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
        if (result is OkResult<TSuccess1, TFailure> ok)
        {
            var result2 = await selector(ok.Value);
            if (result2 is OkResult<TCollection, TFailure> ok2)
            {
                var result3 = resultSelector(ok.Value, ok2.Value);
                return Result.Ok<TSuccess2, TFailure>(result3);
            }
            else if (result2 is ErrorResult<TCollection, TFailure> error2)
            {
                return Result.Error<TSuccess2, TFailure>(error2.Value);
            }
            else
            {
                throw new NotSupportedException($"{result2.GetType().FullName} is not supported.");
            }
        }
        else if (result is ErrorResult<TSuccess1, TFailure> error)
        {
            return Result.Error<TSuccess2, TFailure>(error.Value);
        }
        else
        {
            throw new NotSupportedException($"{result.GetType().FullName} is not supported.");
        }
    }
}
