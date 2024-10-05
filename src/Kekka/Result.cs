namespace Kekka;

/// <summary>
/// Represents the result of an operation,
/// which can either be a success (<see cref="Ok{TSuccess, TFailure}"/>)
/// or a failure (<see cref="Error{TSuccess, TFailure}"/>).
/// </summary>
/// <typeparam name="TSuccess">The type of the value returned in case of a successful result.</typeparam>
/// <typeparam name="TFailure">The type of the error returned in case of a failed result.</typeparam>
public abstract class Result<TSuccess, TFailure>
{
    /// <summary>
    /// Protected constructor to prevent external instantiation.
    /// Use <see cref="Result.Ok{TSuccess, TFailure}(TSuccess)"/> or
    /// <see cref="Result.Error{TSuccess, TFailure}(TFailure)"/> to create instances.
    /// </summary>
    private protected Result() { }
}

/// <summary>
/// Represents a successful result of an operation.
/// </summary>
/// <typeparam name="TSuccess">The type of the success value.</typeparam>
/// <typeparam name="TFailure">The type of the error value (not used in this case).</typeparam>
public sealed class Ok<TSuccess, TFailure> : Result<TSuccess, TFailure>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Ok{TSuccess, TFailure}"/>
    /// class with the specified success value.
    /// </summary>
    /// <param name="value">The value representing the success result.</param>
    internal Ok(TSuccess value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the success value of the result.
    /// </summary>
    public TSuccess Value { get; }
}

/// <summary>
/// Represents a failed result of an operation.
/// </summary>
/// <typeparam name="TSuccess">The type of the success value (not used in this case).</typeparam>
/// <typeparam name="TFailure">The type of the error value.</typeparam>
public sealed class Error<TSuccess, TFailure> : Result<TSuccess, TFailure>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Error{TSuccess, TFailure}"/>
    /// class with the specified error value.
    /// </summary>
    /// <param name="value">The value representing the error result.</param>
    internal Error(TFailure value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the error value of the result.
    /// </summary>
    public TFailure Value { get; }
}

/// <summary>
/// Provides static methods to create <see cref="Result{TSuccess, TFailure}"/> instances.
/// </summary>
public static class Result
{
    /// <summary>
    /// Creates a new successful result with the specified value.
    /// </summary>
    /// <typeparam name="TSuccess">The type of the success value.</typeparam>
    /// <typeparam name="TFailure">The type of the error value.</typeparam>
    /// <param name="value">The value representing the success result.</param>
    /// <returns>An <see cref="Ok{TSuccess, TFailure}"/> representing the successful result.</returns>
    public static Result<TSuccess, TFailure> Ok<TSuccess, TFailure>(TSuccess value)
    {
        return new Ok<TSuccess, TFailure>(value);
    }

    /// <summary>
    /// Creates a new failed result with the specified error value.
    /// </summary>
    /// <typeparam name="TSuccess">The type of the success value.</typeparam>
    /// <typeparam name="TFailure">The type of the error value.</typeparam>
    /// <param name="error">The value representing the error result.</param>
    /// <returns>An <see cref="Error{TSuccess, TFailure}"/> representing the failed result.</returns>
    public static Result<TSuccess, TFailure> Error<TSuccess, TFailure>(TFailure error)
    {
        return new Error<TSuccess, TFailure>(error);
    }
}
