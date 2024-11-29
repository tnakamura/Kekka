# Kekka - Result Type for C#

Kekka - means result in Japanese - is a C# library that provides a functional-style Result type, enabling you to handle success and failure cases more elegantly in your applications. Inspired by the Railway Oriented Programming (ROP) concept, Kekka helps you structure your code in a way that keeps it clean and handles errors gracefully.

## Features

- Functional-style `Result` type
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

if (result1.TryGetValue(out var value))
{
    Console.WriteLine($"Success: {value}");
}
if (result2.TryGetError(out var error))
{
    Console.WriteLine($"Failure: {error}");
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
              select x + y + z;

if (result1.TryGetValue(out var value))
{
    Console.WriteLine($"result1: {value}");  // Output: result1: 6
}
```

#### Example 2: Handling failure

In the following example, one of the operations fails (`Result.Error<T, TError>`), and the chain stops immediately, returning the error.

```cs
using Kekka;

var result2 = from x in Result.Ok<decimal, Exception>(2)
              from y in Result.Error<decimal, Exception>(new Exception("Error!!"))
              from z in Result.Ok<decimal, Exception>(3)
              select x + y + z;

if (result2.TryGetError(out var error))
{
    Console.WriteLine($"result2: {error.Message}");  // Output: result2: Error!!
}
```

#### Asynchronous Support

You can also use asynchronous results with `Task<Result<T, TError>>` and chain them using the provided extension methods.

```cs
using Kekka;
using System.Threading.Tasks;

var result = await from x in Task.FromResult(Result.Ok<int, string>(10))
                   from y in Task.FromResult(Result.Ok<int, string>(x + 5))
                   select x + y;

if (result.TryGetValue(out var value))
{
    Console.WriteLine($"Async result: {value}");  // Output: Async result: 25
}
```

## License

This project is licensed under the [MIT License](https://opensource.org/license/MIT) - see the LICENSE file for details.

