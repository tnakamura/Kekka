# Kekka - Result Type for C#

Kekka - means result in Japanese - is a C# library that provides a functional-style Result type, enabling you to handle success and failure cases more elegantly in your applications. Inspired by the Railway Oriented Programming (ROP) concept, Kekka helps you structure your code in a way that keeps it clean and handles errors gracefully.

## Features

- Functional-style `Result` type (`Ok`, `Error`)
- Extension methods for LINQ-style composition
- Support for asynchronous operations
- Tools for Railway Oriented Programming (ROP)

## Installation

You can install Kekka from NuGet:

```bash
dotnet add package Kekka
```

## Usage

### Basic `Result` Type Usage

```cs
using Kekka;

var result1 = Result.Ok<int, string>(10);  // Success case
var result2 = Result.Error<int, string>("Something went wrong");  // Error case

if (result1 is Ok<int, string> ok)
{
    Console.WriteLine($"Success: {ok.Value}");
}
else if (result2 is Error<int, string> error)
{
    Console.WriteLine($"Failure: {error.Value}");
}
```

### Railway Oriented Programming (ROP) with LINQ

Railway Oriented Programming is a concept introduced by Scott Wlaschin, designed to handle success and failure cases in a clear, chainable manner.
In Kekka, you can use LINQ and extension methods like `SelectMany` to compose multiple operations that can succeed or fail.

#### Example 1: All operations succeed

```cs
using Kekka;

var result1 = from x in Result.Ok<decimal, Exception>(2)
              from y in Result.Ok<decimal, Exception>(x)
              from z in Result.Ok<decimal, Exception>(y)
              select x + y;

if (result1 is Ok<decimal, Exception> ok)
{
    Console.WriteLine($"result1: {ok.Value}");  // Output: result1: 4
}
```

#### Example 2: Handling failure

In the following example, one of the operations fails (`Result.Error<TSuccess, TFaulure>`), and the chain stops immediately, returning the error.

```cs
using Kekka;

var result2 = from x in Result.Ok<decimal, Exception>(2)
              from y in Result.Error<decimal, Exception>(new Exception("Error!!"))
              from z in Result.Ok<decimal, Exception>(3)
              select x + y + z;

if (result2 is Error<decimal, Exception> error)
{
    Console.WriteLine($"result2: {error.Value.Message}");  // Output: result2: Error!!
}
```

#### Asynchronous Support

You can also use asynchronous results with `Task<Result<TSuccess, TFailure>>` and chain them using the provided extension methods.

```cs
using Kekka;
using System.Threading.Tasks;

var result = await from x in Task.FromResult(Result.Ok<int, string>(10))
                   from y in Task.FromResult(Result.Ok<int, string>(x + 5))
                   select x + y;

if (result is Ok<int, string> ok)
{
    Console.WriteLine($"Async result: {ok.Value}");  // Output: Async result: 25
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

## Extension Methods

The library provides several useful extension methods to work with `Result<TSuccess, TFailure>` types in a more functional way.

- `Select` - Map over the success value.
- `SelectMany` - Chain multiple Result instances.
- `MapError` - Map over the failure value.
- `ToAsyncResult` - Convert a Result to an asynchronous `Task<Result<TSuccess, TFaulure>>`.

## License

This project is licensed under the [MIT License](https://opensource.org/license/MIT) - see the LICENSE file for details.
