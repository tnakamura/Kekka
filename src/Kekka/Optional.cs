using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Kekka;

/// <summary>
/// Represents an optional value that can either contain a value or be empty (None).
/// This type provides a null-safe alternative to nullable reference types and enables
/// functional programming patterns for handling potentially missing values.
/// </summary>
/// <typeparam name="T">The type of the optional value. Must be a non-null reference type.</typeparam>
/// <remarks>
/// <para>
/// This is a readonly struct that implements the Option/Maybe monad pattern commonly found
/// in functional programming languages. It helps eliminate null reference exceptions by
/// making the absence of a value explicit in the type system.
/// </para>
/// <para>
/// Use <see cref="Optional.Some{T}(T)"/> to create an optional with a value,
/// or <see cref="Optional.None{T}()"/> to create an empty optional.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var some = Optional.Some("Hello");
/// var none = Optional.None&lt;string&gt;();
/// 
/// if (some.TryGet(out var value))
/// {
///     Console.WriteLine(value); // Prints: Hello
/// }
/// </code>
/// </example>
public readonly struct Optional<T> : IEquatable<Optional<T>>, IEquatable<T>
    where T : notnull
{
    private readonly bool _hasValue;

    private readonly T? _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Optional{T}"/> struct with a value.
    /// </summary>
    /// <param name="value">The value to wrap in the optional.</param>
    /// <remarks>
    /// This constructor is internal and should not be called directly.
    /// Use <see cref="Optional.Some{T}(T)"/> instead.
    /// </remarks>
    internal Optional(T value)
    {
        _hasValue = true;
        _value = value;
    }

    /// <summary>
    /// Gets an empty optional that contains no value.
    /// </summary>
    /// <value>
    /// An <see cref="Optional{T}"/> instance that represents the absence of a value.
    /// </value>
    /// <remarks>
    /// This property returns the default value of the <see cref="Optional{T}"/> struct,
    /// which represents an empty optional (None state).
    /// </remarks>
    public static Optional<T> None => default;

    /// <summary>
    /// Gets a value indicating whether this optional contains a value.
    /// </summary>
    /// <value>
    /// <c>true</c> if the optional contains a value; otherwise, <c>false</c>.
    /// </value>
    public bool HasValue => _hasValue;

    /// <summary>
    /// Gets the value contained in this optional, or the default value if empty.
    /// </summary>
    /// <value>
    /// The contained value if <see cref="HasValue"/> is <c>true</c>; 
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </value>
    /// <remarks>
    /// It is recommended to use <see cref="TryGet(out T?)"/> instead of this property
    /// to safely access the value and avoid potential null reference issues.
    /// </remarks>
    public T? Value => _value;

    /// <summary>
    /// Determines whether the specified object is equal to the current optional.
    /// </summary>
    /// <param name="obj">The object to compare with the current optional.</param>
    /// <returns>
    /// <c>true</c> if the specified object is equal to the current optional; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Two optionals are considered equal if they both contain values that are equal,
    /// or if they are both empty (None).
    /// </remarks>
    public override bool Equals(object? obj)
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

    /// <summary>
    /// Returns the hash code for this optional.
    /// </summary>
    /// <returns>
    /// A 32-bit signed integer hash code. If the optional contains a value,
    /// returns the hash code of that value; otherwise, returns 0.
    /// </returns>
    public override int GetHashCode()
    {
        return _value?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Determines whether the current optional is equal to another optional.
    /// </summary>
    /// <param name="other">An optional to compare with this optional.</param>
    /// <returns>
    /// <c>true</c> if the current optional is equal to the <paramref name="other"/> parameter;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Two optionals are considered equal if:
    /// <list type="bullet">
    /// <item>Both contain values and those values are equal according to the default equality comparer.</item>
    /// <item>Both are empty (None state).</item>
    /// </list>
    /// </remarks>
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

    /// <summary>
    /// Determines whether the current optional contains a value equal to the specified value.
    /// </summary>
    /// <param name="other">A value to compare with the value contained in this optional.</param>
    /// <returns>
    /// <c>true</c> if the current optional contains a value equal to <paramref name="other"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// If this optional is empty (None), this method always returns <c>false</c>,
    /// regardless of the value of <paramref name="other"/>.
    /// </remarks>
    public bool Equals(T? other)
    {
        if (_value is not null)
        {
            return EqualityComparer<T>.Default.Equals(_value, other);
        }
        return false;
    }

    /// <summary>
    /// Attempts to get the value from this optional.
    /// </summary>
    /// <param name="value">
    /// When this method returns, contains the value if the optional contains a value;
    /// otherwise, the default value for <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the optional contains a value and it was retrieved;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This is the recommended way to safely access the value contained in an optional.
    /// It follows the standard .NET TryGet pattern and provides compile-time null safety.
    /// </remarks>
    /// <example>
    /// <code>
    /// var optional = Optional.Some("Hello");
    /// if (optional.TryGet(out var value))
    /// {
    ///     Console.WriteLine($"Value: {value}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine("No value");
    /// }
    /// </code>
    /// </example>
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

    /// <summary>
    /// Determines whether two optional values are equal.
    /// </summary>
    /// <param name="left">The first optional to compare.</param>
    /// <param name="right">The second optional to compare.</param>
    /// <returns>
    /// <c>true</c> if the optionals are equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator ==(Optional<T> left, Optional<T> right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two optional values are not equal.
    /// </summary>
    /// <param name="left">The first optional to compare.</param>
    /// <param name="right">The second optional to compare.</param>
    /// <returns>
    /// <c>true</c> if the optionals are not equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator !=(Optional<T> left, Optional<T> right)
        => !(left == right);
}
