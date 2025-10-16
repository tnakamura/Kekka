using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Kekka;

/// <summary>
/// Represents a result that can be either a success value or an error.
/// This type enables Railway Oriented Programming patterns by allowing operations to be chained
/// while automatically short-circuiting on the first error encountered.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
/// <typeparam name="TError">The type of the error value. Must be a non-null reference type.</typeparam>
/// <remarks>
/// This is a readonly struct that provides a functional approach to error handling,
/// eliminating the need for exception-based control flow in many scenarios.
/// </remarks>
public readonly struct Result<T, TError> : IEquatable<Result<T, TError>>
    where TError : notnull
{
    private readonly bool _hasValue;

    private readonly T? _value;

    private readonly TError? _error;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T, TError}"/> struct with a success value.
    /// </summary>
    /// <param name="value">The success value.</param>
    internal Result(T value)
    {
        _value = value;
        _hasValue = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T, TError}"/> struct with an error value.
    /// </summary>
    /// <param name="error">The error value.</param>
    internal Result(TError error)
    {
        _error = error;
        _hasValue = false;
    }

    /// <summary>
    /// Gets the error value if the result represents a failure.
    /// </summary>
    /// <value>
    /// The error value if <see cref="HasValue"/> is <c>false</c>; otherwise, <c>null</c>.
    /// </value>
    public TError? Error => _error;

    /// <summary>
    /// Gets a value indicating whether this result represents a success.
    /// </summary>
    /// <value>
    /// <c>true</c> if the result contains a success value; otherwise, <c>false</c>.
    /// </value>
    public bool HasValue => _hasValue;

    /// <summary>
    /// Gets the success value if the result represents a success.
    /// </summary>
    /// <value>
    /// The success value if <see cref="HasValue"/> is <c>true</c>; otherwise, the default value of <typeparamref name="T"/>.
    /// </value>
    public T? Value => _value;

    /// <summary>
    /// Attempts to get the success value from the result.
    /// </summary>
    /// <param name="value">
    /// When this method returns, contains the success value if the result represents a success;
    /// otherwise, the default value for <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the result represents a success and the value was retrieved;
    /// otherwise, <c>false</c>.
    /// </returns>
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

    /// <summary>
    /// Attempts to get the error value from the result.
    /// </summary>
    /// <param name="error">
    /// When this method returns, contains the error value if the result represents a failure;
    /// otherwise, the default value for <typeparamref name="TError"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the result represents a failure and the error was retrieved;
    /// otherwise, <c>false</c>.
    /// </returns>
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

    /// <summary>
    /// Attempts to get either the success value or the error value from the result.
    /// </summary>
    /// <param name="value">
    /// When this method returns, contains the success value if the result represents a success;
    /// otherwise, the default value for <typeparamref name="T"/>.
    /// </param>
    /// <param name="error">
    /// When this method returns, contains the error value if the result represents a failure;
    /// otherwise, the default value for <typeparamref name="TError"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the result represents a success and the value was retrieved;
    /// <c>false</c> if the result represents a failure and the error was retrieved.
    /// </returns>
    /// <remarks>
    /// This method is useful for pattern matching scenarios where you need to handle both success and failure cases.
    /// </remarks>
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

    /// <inheritdoc/>
    public bool Equals(Result<T, TError> other)
    {
        if (_hasValue != other._hasValue)
        {
            return false;
        }
        if (_hasValue)
        {
            return EqualityComparer<T>.Default.Equals(_value!, other._value!);
        }
        else
        {
            return EqualityComparer<TError>.Default.Equals(_error!, other._error!);
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Result<T, TError> other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (_hasValue)
        {
            return HashCode.Combine(_hasValue, _value);
        }
        else
        {
            return HashCode.Combine(_hasValue, _error);
        }
    }

    /// <summary>
    /// Determines whether two <see cref="Result{T, TError}"/> instances are equal.
    /// </summary>
    public static bool operator ==(Result<T, TError> left, Result<T, TError> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="Result{T, TError}"/> instances are not equal.
    /// </summary>
    public static bool operator !=(Result<T, TError> left, Result<T, TError> right)
    {
        return !(left == right);
    }
}

