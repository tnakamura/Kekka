using System.Runtime.CompilerServices;
using Kekka;

namespace OrderTaking.Common;

/// <summary>
/// Constrained to be 50 chars or less, not null
/// </summary>
public partial class String50
{
    private String50(string value) => Value = value;

    public string Value { get; }
}

/// <summary>
/// An email address
/// </summary>
public partial class EmailAddress
{
    private EmailAddress(string value) => Value = value;

    public string Value { get; }
}

/// <summary>
/// A zip code
/// </summary>
public partial class ZipCode
{
    private ZipCode(string value) => Value = value;

    public string Value { get; }
}

/// <summary>
/// An Id for Orders. Constrained to be a non-empty string < 10 chars
/// </summary>
public partial class OrderId
{
    private OrderId(string value) => Value = value;

    public string Value { get; }
}

/// <summary>
/// An Id for OrderLines. Constrained to be a non-empty string < 10 chars
/// </summary>
public partial class OrderLineId
{
    private OrderLineId(string value) => Value = value;

    public string Value { get; }
}

/// <summary>
/// A ProductCode is either a Widget or a Gizmo
/// </summary>
public partial class ProductCode
{
    private ProductCode() { }

    /// <summary>
    /// The codes for Widgets start with a "W" and then four digits
    /// </summary>
    public partial class WidgetCode : ProductCode
    {
        private WidgetCode(string value) => Value = value;

        public string Value { get; }
    }

    /// <summary>
    /// The codes for Gizmos start with a "G" and then three digits.
    /// </summary>
    public partial class GizmoCode : ProductCode
    {
        private GizmoCode(string value) => Value = value;

        public string Value { get; }
    }
}

/// <summary>
/// A Quantity is either a Unit or a Kilogram
/// </summary>
public partial class OrderQuantity
{
    private OrderQuantity() { }

    /// <summary>
    /// Constrained to be a integer between 1 and 1000
    /// </summary>
    public partial class UnitQuantity : OrderQuantity
    {
        public UnitQuantity(int value) => Value = value;

        public int Value { get; }
    }

    /// <summary>
    /// Constrained to be a decimal between 0.05 and 100.00
    /// </summary>
    public partial class KilogramQuantity : OrderQuantity
    {
        public KilogramQuantity(decimal value) => Value = value;

        public decimal Value { get; }
    }
}

/// <summary>
/// Constrained to be a decimal between 0.0 and 1000.00
/// </summary>
public partial class Price
{
    public Price(decimal value) => Value = value;

    public decimal Value { get; }
}

/// <summary>
/// Constrained to be a decimal between 0.0 and 10000.00
/// </summary>
public partial class BillingAmount
{
    public BillingAmount(decimal value) => Value = value;

    public decimal Value { get; }
}

/// <summary>
/// Represents a PDF attachment
/// </summary>
public partial class PdfAttachment
{
    public PdfAttachment(string name, byte[] bytes)
    {
        Name = name;
        Bytes = bytes;
    }

    public string Name { get; }

    public byte[] Bytes { get; }
}


/// <summary>
/// Useful functions for constrained types
/// </summary>
internal static class ConstrainedType
{
    /// <summary>
    /// Create a constrained string using the constructor provided
    /// Return Error if input is null, empty, or length > maxLen
    /// </summary>
    public static Result<T, string> CreateString<T>(
        string fieldName,
        Func<string, T> ctor,
        int maxLen,
        string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            var msg = $"{fieldName} must not be null or empty";
            return Result.Error<T, string>(msg);
        }
        else if (str.Length > maxLen)
        {
            var msg = $"{fieldName} must not be more than {maxLen} chars";
            return Result.Error<T, string>(msg);
        }
        else
        {
            return Result.Ok<T, string>(ctor(str));
        }
    }

    /// <summary>
    /// Create a optional constrained string using the constructor provided
    /// Return None if input is null, empty.
    /// Return error if length > maxLen
    /// Return Some if the input is valid
    /// </summary>
    public static Result<T?, string> CreateStringOption<T>(
        string fieldName,
        Func<string, T?> ctor,
        int maxLen,
        string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return Result.Ok<T?, string>(default);
        }
        else if (str.Length > maxLen)
        {
            var msg = $"{fieldName} must not be more than {maxLen} chars";
            return Result.Error<T?, string>(msg);
        }
        else
        {
            return Result.Ok<T?, string>(ctor(str));
        }
    }

    /// <summary>
    /// Create a constrained integer using the constructor provided
    /// Return Error if input is less than minVal or more than maxVal
    /// </summary>
    public static Result<T, string> CreateInt<T>(
        string fieldName,
        Func<int, T> ctor,
        int minVal,
        int maxVal,
        int i)
    {
        if (i < minVal)
        {
            var msg = $"{fieldName}: Must not be less than {minVal}";
            return Result.Error<T, string>(msg);
        }
        else if (i > maxVal)
        {
            var msg = $"{fieldName}: Must not be greater than {maxVal}";
            return Result.Error<T, string>(msg);
        }
        else
        {
            return Result.Ok<T, string>(ctor(i));
        }
    }

    /// <summary>
    /// Create a constrained decimal using the constructor provided
    /// Return Error if input is less than minVal or more than maxVal
    /// </summary>
    public static Result<T, string> CreateDecimal<T>(
        string fieldName,
        Func<decimal, T> ctor,
        decimal minVal,
        decimal maxVal,
        decimal i)
    {
        if (i < minVal)
        {
            var msg = $"{fieldName}: Must not be less than {minVal}";
            return Result.Error<T, string>(msg);
        }
        else if (i > maxVal)
        {
            var msg = $"{fieldName}: Must not be greater than {maxVal}";
            return Result.Error<T, string>(msg);
        }
        else
        {
            return Result.Ok<T, string>(ctor(i));
        }
    }

    /// <summary>
    /// Create a constrained string using the constructor provided
    /// Return Error if input is null. empty, or does not match the regex pattern
    /// </summary>
    public static Result<T, string> CreateLike<T>(
        string fieldName,
        Func<string, T> ctor,
        string pattern,
        string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            var msg = $"{fieldName} must not be null or empty";
            return Result.Error<T, string>(msg);
        }
        else if (System.Text.RegularExpressions.Regex.IsMatch(str, pattern))
        {
            var msg = $"{fieldName}: '{str}' must match the pattern '{pattern}'";
            return Result.Error<T, string>(msg);
        }
        else
        {
            return Result.Ok<T, string>(ctor(str));
        }
    }
}

partial class String50
{
    /// <summary>
    /// Create an String50 from a string
    /// Return Error if input is null, empty, or length > 50
    /// </summary>
    public static Result<String50, string> Create(string fieldName, string str) =>
        ConstrainedType.CreateString(fieldName, x => new String50(x), 50, str);

    /// <summary>
    /// Create an String50 from a string
    /// Return None if input is null, empty.
    /// Return error if length > maxLen
    /// Return Some if the input is valid
    /// </summary>
    public static Result<String50?, string> CreateOption(string fieldName, string? str) =>
        ConstrainedType.CreateStringOption(fieldName, x => new String50(x), 50, str);
}

partial class EmailAddress
{
    /// <summary>
    /// Create an EmailAddress from a string
    /// Return Error if input is null, empty, or doesn't have an "@" in it
    /// </summary>
    public static Result<EmailAddress, string> Create(string fieldName, string str)
    {
        var pattern = ".+@.+";// anything separated by an "@"
        return ConstrainedType.CreateLike(fieldName, x => new EmailAddress(x), pattern, str);
    }
}

partial class ZipCode
{
    /// <summary>
    /// Create a ZipCode from a string
    /// Return Error if input is null, empty, or doesn't have 5 digits
    /// </summary>
    public static Result<ZipCode, string> Create(string fieldName, string str)
    {
        var pattern = "\\d{5}";
        return ConstrainedType.CreateLike(fieldName, x => new ZipCode(x), pattern, str);
    }
}

partial class OrderId
{
    /// <summary>
    /// Create an OrderId from a string
    /// Return Error if input is null, empty, or length > 50
    /// </summary>
    public static Result<OrderId, string> Create(string fieldName, string str) =>
        ConstrainedType.CreateString(fieldName, x => new OrderId(x), 50, str);
}

partial class OrderLineId
{
    /// <summary>
    /// Create an OrderLineId from a string
    /// Return Error if input is null, empty, or length > 50
    /// </summary>
    public static Result<OrderLineId, string> Create(string fieldName, string str) =>
        ConstrainedType.CreateString(fieldName, x => new OrderLineId(x), 50, str);
}

partial class ProductCode
{
    partial class WidgetCode
    {
        /// <summary>
        /// Create an WidgetCode from a string
        /// Return Error if input is null. empty, or not matching pattern
        /// </summary>
        public static Result<WidgetCode, string> CreateWidgetCode(string fieldName, string code)
        {
            // The codes for Widgets start with a "W" and then four digits
            var pattern = "W\\d{4}";
            return ConstrainedType.CreateLike(fieldName, x => new WidgetCode(x), pattern, code);
        }
    }
}

partial class ProductCode
{
    partial class GizmoCode
    {
        /// <summary>
        /// Create an GizmoCode from a string
        /// Return Error if input is null, empty, or not matching pattern
        /// </summary>
        public static Result<GizmoCode, string> CreateGizmoCode(string fieldName, string code)
        {
            // The codes for Gizmos start with a "G" and then three digits.
            var pattern = "G\\d{3}";
            return ConstrainedType.CreateLike(fieldName, x => new GizmoCode(x), pattern, code);
        }
    }
}

partial class ProductCode
{
    public string PrimitiveValue =>
        this is WidgetCode w ? w.Value :
        this is GizmoCode g ? g.Value :
        string.Empty;

    /// <summary>
    /// Create an ProductCode from a string
    /// Return Error if input is null, empty, or not matching pattern
    /// </summary>
    public static Result<ProductCode, string> Create(string fieldName, string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            var msg = $"{fieldName}: Must not be null or empty";
            return Result.Error<ProductCode, string>(msg);
        }
        else if (code.StartsWith("W"))
        {
            var result = WidgetCode.CreateWidgetCode(fieldName, code);
            return result.Select<WidgetCode, ProductCode, string>(x => x);
        }
        else if (code.StartsWith("G"))
        {
            var result = GizmoCode.CreateGizmoCode(fieldName, code);
            return result.Select<GizmoCode, ProductCode, string>(x => x);
        }
        else
        {
            var msg = $"{fieldName}: Format not recognized '{code}'";
            return Result.Error<ProductCode, string>(msg);
        }
    }
}


partial class OrderQuantity
{
    partial class UnitQuantity
    {
        /// <summary>
        /// Create a UnitQuantity from a int
        /// Return Error if input is not an integer between 1 and 1000
        /// </summary>
        public static Result<UnitQuantity, string> CreateUnitQuantity(string fieldName, int v) =>
            ConstrainedType.CreateInt(fieldName, x => new UnitQuantity(x), 1, 1000, v);
    }
}

partial class OrderQuantity
{
    partial class KilogramQuantity
    {
        /// <summary>
        /// Create a KilogramQuantity from a decimal.
        /// Return Error if input is not a decimal between 0.05 and 100.00
        /// </summary>
        public static Result<KilogramQuantity, string> CreateKilogramQuantity(string fieldName, decimal v) =>
            ConstrainedType.CreateDecimal(fieldName, x => new KilogramQuantity(x), 0.05M, 100M, v);
    }
}

partial class OrderQuantity
{

    //let value qty =
    //    match qty with
    //    | Unit uq ->
    //        uq |> UnitQuantity.value |> decimal
    //    | Kilogram kq ->
    //        kq |> KilogramQuantity.value
    /// <summary>
    /// Return the value inside a OrderQuantity
    /// </summary>
    public decimal PrimitiveValue =>
        this is UnitQuantity uq ? uq.Value :
        this is KilogramQuantity kq ? kq.Value :
        default;

    /// <summary>
    /// Create a OrderQuantity from a productCode and quantity
    /// </summary>
    public static Result<OrderQuantity, string> Create(string fieldName, ProductCode productCode, decimal quantity)
    {
        if (productCode is ProductCode.WidgetCode)
        {
            var result = UnitQuantity.CreateUnitQuantity(fieldName, (int)quantity);
            return result.Select<UnitQuantity, OrderQuantity, string>(x => x);
        }
        else
        {
            var result = KilogramQuantity.CreateKilogramQuantity(fieldName, quantity);
            return result.Select<KilogramQuantity, OrderQuantity, string>(x => x);
        }
    }
}

partial class Price
{
    /// <summary>
    /// Create a Price from a decimal.
    /// Return Error if input is not a decimal between 0.0 and 1000.00
    /// </summary>
    public static Result<Price, string> Create(decimal v) =>
        ConstrainedType.CreateDecimal("Price", x => new Price(x), 0.0M, 1000M, v);

    /// Create a Price from a decimal.
    /// Throw an exception if out of bounds. This should only be used if you know the value is valid.
    //public static Result<Price, string> unsafeCreate v =
    //    create v
    //    |> function
    //        | Ok price ->
    //            price
    //        | Error err ->
    //            failwithf "Not expecting Price to be out of bounds: %s" err

    /// <summary>
    /// Multiply a Price by a decimal qty.
    /// Return Error if new price is out of bounds.
    /// </summary>
    public static Result<Price, string> operator *(Price qty, Price p) =>
        Create(qty.Value * p.Value);
}

partial class BillingAmount
{
    /// <summary>
    /// Create a BillingAmount from a decimal.
    /// Return Error if input is not a decimal between 0.0 and 10000.00
    /// </summary>
    public static Result<BillingAmount, string> Create(decimal v) =>
        ConstrainedType.CreateDecimal("BillingAmount", x => new BillingAmount(x), 0.0M, 10000M, v);

    /// <summary>
    /// Sum a list of prices to make a billing amount
    /// Return Error if total is out of bounds
    /// </summary>
    public static Result<BillingAmount, string> SumPrices(IEnumerable<Price> prices)
    {
        var total = prices.Select(x => x.Value).Sum();
        return Create(total);
    }
}
