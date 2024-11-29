namespace Kekka;

public interface IResult<T, TError> : IOptional<T>
    where TError : notnull
{
    TError? Error { get; }
}

public interface IResult<T, TError, TSelf> : IResult<T, TError>, IOptional<T, TSelf>
    where TSelf : struct, IResult<T, TError, TSelf>
    where TError : notnull
{
}

