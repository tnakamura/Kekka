using System;

namespace Kekka;

public static partial class Optional
{
    public static Optional<T2> Select<T1, T2>(this Optional<T1> optional, Func<T1, T2> selector)
        where T1 : notnull
        where T2 : notnull
    {
        if (optional.TryGet(out var value))
        {
            return new Optional<T2>(selector(value));
        }
        else
        {
            return Optional<T2>.None;
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
            return Optional<T2>.None;
        }
    }

    public static Optional<T2> SelectMany<T1, TCollection, T2>(
        this Optional<T1> source,
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
                return new Optional<T2>(result2);
            }
            else
            {
                return Optional<T2>.None;
            }
        }
        else
        {
            return Optional<T2>.None;
        }
    }
}

