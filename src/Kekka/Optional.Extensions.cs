using System;
using System.Threading.Tasks;

namespace Kekka;

public static partial class Optional
{
    public static Optional<T> Some<T>(T value)
        where T : notnull
        => new Optional<T>(value);

    public static Optional<T> None<T>()
        where T : notnull
        => Optional<T>.None;
}

static partial class Optional
{
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

