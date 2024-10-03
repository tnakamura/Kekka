# Kekka

Kekka - means result in Japanese - 

## Installation

```sh
> dotnet add package Kekka
```

## Quick start

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

## License

[MIT](https://opensource.org/license/MIT)

