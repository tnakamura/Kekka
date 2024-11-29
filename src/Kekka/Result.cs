using System.Diagnostics.CodeAnalysis;

namespace Kekka;

public readonly struct Result<T, TError> 
{
    private readonly bool _isOk;

    private readonly T? _value;

    private readonly TError? _error;

    public Result(T value)
    {
        _value = value;
        _isOk = true;
    }

    public Result(TError error)
    {
        _error = error;
        _isOk = false;
    }

    public bool IsOk => _isOk;

    public bool TryGetValue(
        [NotNullWhen(true)] out T? value)
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
        [NotNullWhen(true)] out TError? error)
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
        [NotNullWhen(true)] out T? value,
        [NotNullWhen(false)] out TError? error)
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

