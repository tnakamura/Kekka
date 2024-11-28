using System.Diagnostics.CodeAnalysis;

namespace Kekka;

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

public readonly struct ValueResult<TSuccess, TFailure>
{
    private readonly bool _isOk;

    private readonly TSuccess? _value;

    private readonly TFailure? _error;

    public ValueResult(TSuccess value)
    {
        _value = value;
        _isOk = true;
    }

    public ValueResult(TFailure error)
    {
        _error = error;
        _isOk = false;
    }

    public bool IsOk => _isOk;

    public bool TryGetValue(
        [NotNullWhen(true)] out TSuccess? value)
    {
        if (_isOk)
        {
            value = _value!;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public bool TryGetError(
        [NotNullWhen(true)] out TFailure? error)
    {
        if (_isOk)
        {
            error = default;
            return false;
        }
        else
        {
            error = _error!;
            return true;
        }
    }

    public bool TryGet(
        [NotNullWhen(true)] out TSuccess? value,
        [NotNullWhen(false)] out TFailure? error)
    {
        if (_isOk)
        {
            value = _value!;
            error = default;
            return true;
        }
        else
        {
            value = default;
            error = _error!;
            return false;
        }
    }
}

