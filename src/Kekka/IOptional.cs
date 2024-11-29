namespace Kekka;

public interface IOptional<T>
{
    bool HasValue { get; }

    T? Value { get; }
}

public interface IOptional<T, TSelf> : IOptional<T>
    where TSelf : struct, IOptional<T, TSelf>
{
}

