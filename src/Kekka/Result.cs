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
    internal ErrorResult(TFailure error)
    {
        Error = error;
    }

    public TFailure Error { get; }
}

public static class Result
{
    public static Result<TSuccess, TFailure> Ok<TSuccess, TFailure>(TSuccess value)
    {
        return new OkResult<TSuccess, TFailure>(value);
    }

    public static Result<TSuccess, TFailure> Error<TSuccess, TFailure>(TFailure error)
    {
        return new ErrorResult<TSuccess, TFailure>(error);
    }
}
