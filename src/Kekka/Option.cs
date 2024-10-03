namespace Kekka;

public abstract class Option<T>
{
    private protected Option() { }
}

public sealed class Some<T> : Option<T>
{
    internal Some(T value)
    {
        Value = value;
    }

    public T Value { get; }
}

public sealed class None<T> : Option<T>
{
    internal static readonly None<T> Instance = new None<T>();

    private None() { }
}

public static class Option
{
    public static Option<T> Some<T>(T value) => new Some<T>(value);

    public static Option<T> None<T>() => Kekka.None<T>.Instance;
}
