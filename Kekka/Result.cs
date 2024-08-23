using System;

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

public static class ResultExtensions
{
    private static Result<TSuccess, TFailure> Ok<TSuccess, TFailure>(this TSuccess value)
    {
        return Result.Ok<TSuccess, TFailure>(value);
    }

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
        return source.SelectMany(x => selector(x).SelectMany(y => resultSelector(x, y).Ok<TSuccess2, TFailure>()));
    }
}
