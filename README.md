# Kekka

The Kekka - means result in Japanese - provides a type-safe way to handle results that can either be successful (Ok) or contain an error (Error).
It is useful for scenarios where you need to represent the outcome of an operation without relying on exceptions or nullable types.

## Features

- Type-safe Result abstraction: Avoids ambiguity by clearly defining success and failure cases.
- Generic result handling: Supports both TSuccess and TFailure types to represent different data for success and failure.
- Clear API: Provides a straightforward Ok and Error factory method to create results.

## Installation

You can install the Kekka library via NuGet:

### Using .NET CLI

```bash
dotnet add package Kekka
```

### Using Package Manager Console

```bash
Install-Package Kekka
```

Once installed, you can include the library in your project by adding the following using directive:

```cs
using Kekka;
```

## Usage

### Creating a Result

You can create a result using the `Result.Ok` and `Result.Error` static methods.
Here's an example of how to use it:

```cs
using Kekka;

public class Example
{
    public Result<int, string> Divide(int dividend, int divisor)
    {
        if (divisor == 0)
        {
            return Result.Error<int, string>("Division by zero is not allowed.");
        }

        return Result.Ok<int, string>(dividend / divisor);
    }
}
```

In this example:

- The method `Divide` returns either an `Ok` result with the division result or an `Error` result with an error message (`string`).

### Handling Results

Once you have a Result object, you can safely check whether it is a success (`Ok`) or a failure (`Error`):

```cs
var result = example.Divide(10, 2);

if (result is Ok<int, string> okResult)
{
    Console.WriteLine($"Success: {okResult.Value}");
}
else if (result is Error<int, string> errorResult)
{
    Console.WriteLine($"Error: {errorResult.Value}");
}
```

This pattern ensures that you handle both success and failure cases explicitly, leading to more robust and predictable code.

### Railway Oriented Programming

```cs
using Kekka;

var result1 = from x in Result.Ok<decimal, Exception>(2)
              from y in Result.Ok<decimal, Exception>(x)
              from z in Result.Ok<decimal, Exception>(y)
              select x + y;
if (result1 is Ok<decimal, Exception> ok)
{
    Console.WriteLine($"result1: {ok.Value}");
}

var result2 = from x in Result.Ok<decimal, Exception>(2)
              from y in Result.Error<decimal, Exception>(new Exception("Error!!"))
              from z in Result.Ok<decimal, Exception>(3)
              select x + y + z;
if (result2 is Error<decimal, Exception> error)
{
    Console.WriteLine($"result2: {error.Value.Message}");
}
```

## API Reference

### `Result<TSuccess, TFailure>`

- The base abstract class representing a result that can either be a success or a failure.

### `Ok<TSuccess, TFailure>`

- A sealed class representing a successful result.
- **Property**:
    - `TSuccess Value`: The success value.

### `Error<TSuccess, TFailure>`

- A sealed class representing a failure result.
- **Property**:
    - `TFailure Value`: The failure value.

### `Result.Ok<TSuccess, TFailure>(TSuccess value)`

- A static method to create a successful result with the provided success value.

### `Result.Error<TSuccess, TFailure>(TFailure error)`

- A static method to create a failure result with the provided error value.

## License

This project is licensed under the [MIT License](https://opensource.org/license/MIT) - see the LICENSE file for details.
