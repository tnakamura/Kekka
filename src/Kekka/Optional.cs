using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Kekka;

public readonly struct Optional<T> : IEquatable<Optional<T>>, IEquatable<T>
    where T : notnull
{
    private readonly bool _hasValue;

    private readonly T? _value;

    internal Optional(T value)
    {
        _hasValue = true;
        _value = value;
    }

    public static Optional<T> None => default;

    public bool HasValue => _hasValue;

    public T? Value => _value;

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is Optional<T> other)
        {
            return Equals(other);
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _value?.GetHashCode() ?? 0;
    }

    /// <inheritdoc/>
    public bool Equals(Optional<T> other)
    {
        if (_value is not null && other._value is not null)
        {
            return EqualityComparer<T>.Default.Equals(_value, other._value);
        }
        if (_hasValue is false &&
            _value is null &&
            other._hasValue is false &&
            other._value is null)
        {
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public bool Equals(T other)
    {
        if (_value is not null)
        {
            return EqualityComparer<T>.Default.Equals(_value, other);
        }
        return false;
    }

    public bool TryGet([NotNullWhen(true)] out T? value)
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

    public static bool operator ==(Optional<T> left, Optional<T> right)
        => left.Equals(right);

    public static bool operator !=(Optional<T> left, Optional<T> right)
        => !(left == right);
}
