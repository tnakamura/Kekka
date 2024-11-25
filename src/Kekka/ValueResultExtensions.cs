using System;

namespace Kekka;

public static partial class ValueResultExtensions
{
    public static bool IsSucceeded<TSuccess, TFailure>(this in ValueResult<TSuccess, TFailure> result)
    {
        return IsSucceededCore(result);

        static bool IsSucceededCore<T>(in T result)
            where T : IResult<TSuccess, TFailure>
        {
            return result.IsSucceeded;
        }
    }

    public static TSuccess GetValue<TSuccess, TFailure>(this in ValueResult<TSuccess, TFailure> result)
    {
        return GetValueCore(result);

        static TSuccess GetValueCore<T>(in T result)
            where T : IOk<TSuccess, TFailure>
        {
            return result.Value;
        }
    }

    public static TFailure GetError<TSuccess, TFailure>(this in ValueResult<TSuccess, TFailure> result)
    {
        return GetErrorCore(result);

        static TFailure GetErrorCore<T>(in T result)
            where T : IError<TSuccess, TFailure>
        {
            return result.Error;
        }
    }
}

static partial class ValueResultExtensions
{
    public static ValueResult<TSuccess2, TFailure> Select<TSuccess1, TSuccess2, TFailure>(
        this in ValueResult<TSuccess1, TFailure> source,
        Func<TSuccess1, TSuccess2> selector)
    {
        if (source.IsSucceeded())
        {
            return ValueResult.Ok<TSuccess2, TFailure>(selector(source.GetValue()));
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(source.GetError());
        }
    }

    public static ValueResult<TSuccess2, TFailure> SelectMany<TSuccess1, TSuccess2, TFailure>(
        this in ValueResult<TSuccess1, TFailure> source,
        Func<TSuccess1, ValueResult<TSuccess2, TFailure>> selector)
    {
        if (source.IsSucceeded())
        {
            return selector(source.GetValue());
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(source.GetError());
        }
    }

    public static ValueResult<TSuccess2, TFailure> SelectMany<TSuccess1, TCollection, TSuccess2, TFailure>(
        this ValueResult<TSuccess1, TFailure> source,
        Func<TSuccess1, ValueResult<TCollection, TFailure>> selector,
        Func<TSuccess1, TCollection, TSuccess2> resultSelector)
    {
        if (source.IsSucceeded())
        {
            var result = selector(source.GetValue());
            if (result.IsSucceeded())
            {
                var result2 = resultSelector(source.GetValue(), result.GetValue());
                return ValueResult.Ok<TSuccess2, TFailure>(result2);
            }
            else
            {
                return ValueResult.Error<TSuccess2, TFailure>(result.GetError());
            }
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(source.GetError());
        }
    }
}

