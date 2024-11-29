using System.Diagnostics.CodeAnalysis;

namespace Kekka;

public readonly struct Result<T, TError> : IResult<T, TError, Result<T, TError>>
    where TError : notnull
{
    private readonly bool _hasValue;

    private readonly T? _value;

    private readonly TError? _error;

    public Result(T value)
    {
        _value = value;
        _hasValue = true;
    }

    public Result(TError error)
    {
        _error = error;
        _hasValue = false;
    }

    public TError? Error => _error;

    public bool HasValue => _hasValue;

    public T? Value => _value;

    public bool TryGetValue(
        [NotNullWhen(true)] out T? value)
    {
        if (_hasValue)
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
        [NotNullWhen(true)] out TError? error)
    {
        if (_hasValue)
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
        [NotNullWhen(true)] out T? value,
        [NotNullWhen(false)] out TError? error)
    {
        if (_hasValue)
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

