using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kekka;

public static partial class ValueResultExtensions
{
    public static Task<ValueResult<TSuccess, TFailure>> AsTask<TSuccess, TFailure>(this ValueResult<TSuccess, TFailure> result) =>
        Task.FromResult(result);

    public static ValueTask<ValueResult<TSuccess, TFailure>> AsValueTask<TSuccess, TFailure>(this ValueResult<TSuccess, TFailure> result) =>
        new ValueTask<ValueResult<TSuccess, TFailure>>(result);

    public static ValueResult<IEnumerable<TSuccess>, TFailure> Sequence<TSuccess, TFailure>(this IEnumerable<ValueResult<TSuccess, TFailure>> source)
    {
        var success = new List<TSuccess>();
        foreach (var result in source)
        {
            if (result.TryGet(out var value, out var error))
            {
                success.Add(value);
            }
            else
            {
                return ValueResult.Error<IEnumerable<TSuccess>, TFailure>(error);
            }
        }
        return ValueResult.Ok<IEnumerable<TSuccess>, TFailure>(success);
    }
}

static partial class ValueResultExtensions
{
    public static ValueResult<TSuccess2, TFailure> Select<TSuccess1, TSuccess2, TFailure>(
        this in ValueResult<TSuccess1, TFailure> source,
        Func<TSuccess1, TSuccess2> selector)
    {
        if (source.TryGet(out var value, out var error))
        {
            return ValueResult.Ok<TSuccess2, TFailure>(selector(value));
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(error);
        }
    }

    public static ValueResult<TSuccess2, TFailure> SelectMany<TSuccess1, TSuccess2, TFailure>(
        this in ValueResult<TSuccess1, TFailure> source,
        Func<TSuccess1, ValueResult<TSuccess2, TFailure>> selector)
    {
        if (source.TryGet(out var value, out var error))
        {
            return selector(value);
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(error);
        }
    }

    public static ValueResult<TSuccess2, TFailure> SelectMany<TSuccess1, TCollection, TSuccess2, TFailure>(
        this ValueResult<TSuccess1, TFailure> source,
        Func<TSuccess1, ValueResult<TCollection, TFailure>> selector,
        Func<TSuccess1, TCollection, TSuccess2> resultSelector)
    {
        if (source.TryGet(out var value1, out var error1))
        {
            var result1 = selector(value1);
            if (result1.TryGet(out var value2, out var error2))
            {
                var result2 = resultSelector(value1, value2);
                return ValueResult.Ok<TSuccess2, TFailure>(result2);
            }
            else
            {
                return ValueResult.Error<TSuccess2, TFailure>(error2);
            }
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(error1);
        }
    }

    public static ValueResult<TSuccess, TFailure2> MapError<TSuccess, TFailure1, TFailure2>(
        this ValueResult<TSuccess, TFailure1> source,
        Func<TFailure1, TFailure2> selector)
    {
        if (source.TryGet(out var value, out var error))
        {
            return ValueResult.Ok<TSuccess, TFailure2>(value);
        }
        else
        {
            return ValueResult.Error<TSuccess, TFailure2>(selector(error));
        }
    }
}

static partial class ValueResultExtensions
{
    public static async Task<ValueResult<TSuccess2, TFailure>> Select<TSuccess1, TSuccess2, TFailure>(
        this Task<ValueResult<TSuccess1, TFailure>> source,
        Func<TSuccess1, TSuccess2> selector)
    {
        var result = await source;
        if (result.TryGet(out var value, out var error))
        {
            return ValueResult.Ok<TSuccess2, TFailure>(selector(value));
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(error);
        }
    }

    public static async Task<ValueResult<TSuccess2, TFailure>> SelectMany<TSuccess1, TSuccess2, TFailure>(
        this Task<ValueResult<TSuccess1, TFailure>> source,
        Func<TSuccess1, Task<ValueResult<TSuccess2, TFailure>>> selector)
    {
        var result = await source;
        if (result.TryGet(out var value, out var error))
        {
            return await selector(value);
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(error);
        }
    }

    public static async Task<ValueResult<TSuccess2, TFailure>> SelectMany<TSuccess1, TCollection, TSuccess2, TFailure>(
        this Task<ValueResult<TSuccess1, TFailure>> source,
        Func<TSuccess1, Task<ValueResult<TCollection, TFailure>>> selector,
        Func<TSuccess1, TCollection, TSuccess2> resultSelector)
    {
        var result1 = await source;
        if (result1.TryGet(out var value1, out var error1))
        {
            var result2 = await selector(value1);
            if (result2.TryGet(out var value2, out var error2))
            {
                var result3 = resultSelector(value1, value2);
                return ValueResult.Ok<TSuccess2, TFailure>(result3);
            }
            else
            {
                return ValueResult.Error<TSuccess2, TFailure>(error2);
            }
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(error1);
        }
    }

    public static async Task<ValueResult<TSuccess, TFailure2>> MapError<TSuccess, TFailure1, TFailure2>(
        this Task<ValueResult<TSuccess, TFailure1>> source,
        Func<TFailure1, TFailure2> selector)
    {
        var result = await source;
        if (result.TryGet(out var value, out var error))
        {
            return ValueResult.Ok<TSuccess, TFailure2>(value);
        }
        else
        {
            return ValueResult.Error<TSuccess, TFailure2>(selector(error));
        }
    }
}

static partial class ValueResultExtensions
{
    public static async ValueTask<ValueResult<TSuccess2, TFailure>> Select<TSuccess1, TSuccess2, TFailure>(
        this ValueTask<ValueResult<TSuccess1, TFailure>> source,
        Func<TSuccess1, TSuccess2> selector)
    {
        var result = await source;
        if (result.TryGet(out var value, out var error))
        {
            return ValueResult.Ok<TSuccess2, TFailure>(selector(value));
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(error);
        }
    }

    public static async ValueTask<ValueResult<TSuccess2, TFailure>> SelectMany<TSuccess1, TSuccess2, TFailure>(
        this ValueTask<ValueResult<TSuccess1, TFailure>> source,
        Func<TSuccess1, ValueTask<ValueResult<TSuccess2, TFailure>>> selector)
    {
        var result = await source;
        if (result.TryGet(out var value, out var error))
        {
            return await selector(value);
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(error);
        }
    }

    public static async ValueTask<ValueResult<TSuccess2, TFailure>> SelectMany<TSuccess1, TCollection, TSuccess2, TFailure>(
        this ValueTask<ValueResult<TSuccess1, TFailure>> source,
        Func<TSuccess1, ValueTask<ValueResult<TCollection, TFailure>>> selector,
        Func<TSuccess1, TCollection, TSuccess2> resultSelector)
    {
        var result1 = await source;
        if (result1.TryGet(out var value1, out var error1))
        {
            var result2 = await selector(value1);
            if (result2.TryGet(out var value2, out var error2))
            {
                var result3 = resultSelector(value1, value2);
                return ValueResult.Ok<TSuccess2, TFailure>(result3);
            }
            else
            {
                return ValueResult.Error<TSuccess2, TFailure>(error2);
            }
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(error1);
        }
    }

    public static async ValueTask<ValueResult<TSuccess, TFailure2>> MapError<TSuccess, TFailure1, TFailure2>(
        this ValueTask<ValueResult<TSuccess, TFailure1>> source,
        Func<TFailure1, TFailure2> selector)
    {
        var result = await source;
        if (result.TryGet(out var value, out var error))
        {
            return ValueResult.Ok<TSuccess, TFailure2>(value);
        }
        else
        {
            return ValueResult.Error<TSuccess, TFailure2>(selector(error));
        }
    }
}

