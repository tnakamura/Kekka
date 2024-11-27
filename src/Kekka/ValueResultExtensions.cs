using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Kekka;

public static partial class ValueResultExtensions
{
    public static Task<ValueResult<TSuccess, TFailure>> ToAsyncResult<TSuccess, TFailure>(this ValueResult<TSuccess, TFailure> result) =>
        Task.FromResult(result);

    public static ValueResult<IEnumerable<TSuccess>, TFailure> Sequence<TSuccess, TFailure>(this IEnumerable<ValueResult<TSuccess, TFailure>> source)
    {
        var success = new List<TSuccess>();
        foreach (var result in source)
        {
            if (result.IsSucceeded())
            {
                success.Add(result.GetValue());
            }
            else
            {
                return ValueResult.Error<IEnumerable<TSuccess>, TFailure>(result.GetError());
            }
        }
        return ValueResult.Ok<IEnumerable<TSuccess>, TFailure>(success);
    }
}

static partial class ValueResultExtensions
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

    public static bool TryGetValue<TSuccess, TFailure>(
        this in ValueResult<TSuccess, TFailure> result,
        [MaybeNullWhen(false)] out TSuccess? value)
    {
        if (result.IsSucceeded())
        {
            value = result.GetValue();
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }
    
    public static bool TryGetError<TSuccess, TFailure>(
        this in ValueResult<TSuccess, TFailure> result,
        [MaybeNullWhen(false)] out TFailure? error)
    {
        if (result.IsSucceeded())
        {
            error = default;
            return false;
        }
        else
        {
            error = result.GetError();
            return true;
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

    public static ValueResult<TSuccess, TFailure2> MapError<TSuccess, TFailure1, TFailure2>(
        this ValueResult<TSuccess, TFailure1> source,
        Func<TFailure1, TFailure2> selector)
    {
        if (source.IsSucceeded())
        {
            return ValueResult.Ok<TSuccess, TFailure2>(source.GetValue());
        }
        else
        {
            return ValueResult.Error<TSuccess, TFailure2>(selector(source.GetError()));
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
        if (result.IsSucceeded())
        {
            return ValueResult.Ok<TSuccess2, TFailure>(selector(result.GetValue()));
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(result.GetError());
        }
    }

    public static async Task<ValueResult<TSuccess2, TFailure>> SelectMany<TSuccess1, TSuccess2, TFailure>(
        this Task<ValueResult<TSuccess1, TFailure>> source,
        Func<TSuccess1, Task<ValueResult<TSuccess2, TFailure>>> selector)
    {
        var result = await source;
        if (result.IsSucceeded())
        {
            return await selector(result.GetValue());
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(result.GetError());
        }
    }

    public static async Task<ValueResult<TSuccess2, TFailure>> SelectMany<TSuccess1, TCollection, TSuccess2, TFailure>(
        this Task<ValueResult<TSuccess1, TFailure>> source,
        Func<TSuccess1, Task<ValueResult<TCollection, TFailure>>> selector,
        Func<TSuccess1, TCollection, TSuccess2> resultSelector)
    {
        var result = await source;
        if (result.IsSucceeded())
        {
            var result2 = await selector(result.GetValue());
            if (result2.IsSucceeded())
            {
                var result3 = resultSelector(result.GetValue(), result2.GetValue());
                return ValueResult.Ok<TSuccess2, TFailure>(result3);
            }
            else
            {
                return ValueResult.Error<TSuccess2, TFailure>(result2.GetError());
            }
        }
        else
        {
            return ValueResult.Error<TSuccess2, TFailure>(result.GetError());
        }
    }

    public static async Task<ValueResult<TSuccess, TFailure2>> MapError<TSuccess, TFailure1, TFailure2>(
        this Task<ValueResult<TSuccess, TFailure1>> source,
        Func<TFailure1, TFailure2> selector)
    {
        var result = await source;
        if (result.IsSucceeded())
        {
            return ValueResult.Ok<TSuccess, TFailure2>(result.GetValue());
        }
        else
        {
            return ValueResult.Error<TSuccess, TFailure2>(selector(result.GetError()));
        }
    }
}

