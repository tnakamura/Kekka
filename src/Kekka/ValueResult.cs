using System;

namespace Kekka;

public interface IResult<TSuccess, TFailure>
{
    bool IsSucceeded { get; }
}

public interface IOk<TSuccess, TFailure> : IResult<TSuccess, TFailure>
{
    TSuccess Value { get; }
}

public interface IError<TSuccess, TFailure> : IResult<TSuccess, TFailure>
{
    TFailure Error { get; }
}

public readonly struct ValueResult
{
    public static ValueResult<TSuccess, TFailure> Ok<TSuccess, TFailure>(TSuccess value)
    {
        return new ValueResult<TSuccess, TFailure>(value);
    }

    public static ValueResult<TSuccess, TFailure> Error<TSuccess, TFailure>(TFailure error)
    {
        return new ValueResult<TSuccess, TFailure>(error);
    }
}

public readonly struct ValueResult<TSuccess, TFailure> :
    IOk<TSuccess, TFailure>, IError<TSuccess, TFailure>
{
    public ValueResult(TSuccess value)
    {
        _value = value;
        _isSucceeded = true;
    }

    public ValueResult(TFailure error)
    {
        _error = error;
        _isSucceeded = false;
    }

    private readonly bool _isSucceeded;

    bool IResult<TSuccess, TFailure>.IsSucceeded => _isSucceeded;

    private readonly TSuccess? _value;

    TSuccess IOk<TSuccess, TFailure>.Value => _value ?? throw new InvalidOperationException();

    private readonly TFailure? _error;

    TFailure IError<TSuccess, TFailure>.Error => _error ?? throw new InvalidOperationException();
}

