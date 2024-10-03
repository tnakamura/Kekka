namespace Kekka;

public abstract class Result<TSuccess, TFailure>
{
    private protected Result() { }
}

public sealed class Ok<TSuccess, TFailure> : Result<TSuccess, TFailure>
{
    internal Ok(TSuccess value)
    {
        Value = value;
    }

    public TSuccess Value { get; }
}

public sealed class Error<TSuccess, TFailure> : Result<TSuccess, TFailure>
{
    internal Error(TFailure value)
    {
        Value = value;
    }

    public TFailure Value { get; }
}

public static class Result
{
    public static Result<TSuccess, TFailure> Ok<TSuccess, TFailure>(TSuccess value)
    {
        return new Ok<TSuccess, TFailure>(value);
    }

    public static Result<TSuccess, TFailure> Error<TSuccess, TFailure>(TFailure error)
    {
        return new Error<TSuccess, TFailure>(error);
    }
}
