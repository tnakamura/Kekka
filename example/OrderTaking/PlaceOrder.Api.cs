using OrderTaking.Common;
using Kekka;
using System.Linq;
using OrderTaking.PlaceOrder;
using OrderTaking.PlaceOrder.Implementation;
using System.Text.Json;

namespace OrderTaking.PlaceOrder.Api;

// ======================================================
// This file contains the JSON API interface to the PlaceOrder workflow
//
// 1) The HttpRequest is turned into a DTO, which is then turned into a Domain object
// 2) The main workflow function is called
// 3) The output is turned into a DTO which is turned into a HttpResponse
// ======================================================

public record struct JsonString(string String);

/// <summary>
/// Very simplified version!
/// </summary>
public record HttpRequest(
    string Action,
    string Uri,
    JsonString Body);

/// <summary>
/// Very simplified version!
/// </summary>
public record HttpResponse(
    int HttpStatusCode,
    JsonString Body);

/// <summary>
/// An API takes a HttpRequest as input and returns a async response
/// </summary>
public delegate Task<HttpResponse> PlaceOrderApi(HttpRequest request);


// =============================
// JSON serialization
// =============================
public static class JsonSerialization
{

    public static JsonString SerializeJson<T>(T value) => new JsonString(JsonSerializer.Serialize<T>(value));

    public static T? DeserializeJson<T>(JsonString str) => JsonSerializer.Deserialize<T>(str.String);
}

// =============================
// Implementation
// =============================

public static class Workflow
{
    // setup dummy dependencies

    internal static readonly Implementation.CheckProductCodeExists checkProductExists = productCode => true;

    internal static readonly Implementation.CheckAddressExists checkAddressExists =
        unvalidatedAddress =>
        {
            var checkedAddress = new CheckedAddress(
                AddressLine1: unvalidatedAddress.AddressLine1,
                AddressLine2: unvalidatedAddress.AddressLine2,
                AddressLine3: unvalidatedAddress.AddressLine3,
                AddressLine4: unvalidatedAddress.AddressLine4,
                City: unvalidatedAddress.City,
                ZipCode: unvalidatedAddress.ZipCode);
            return Task.FromResult(Result.Ok<CheckedAddress, AddressValidationError>(checkedAddress));
        };

    internal static readonly Implementation.GetProductPrice getProductPrice =
        productCode => Price.UnsafeCreate(1M);  // dummy implementation

    internal static readonly Implementation.CreateOrderAcknowledgmentLetter createOrderAcknowledgmentLetter =
        pricedOrder =>
        {
            var letterTest = new Implementation.HtmlString("some text");
            return letterTest;
        };

    internal static readonly Implementation.SendOrderAcknowledgment sendOrderAcknowledgment =
        orderAcknowledgement => SendResult.Sent;


    // -------------------------------
    // workflow
    // -------------------------------

    /// This function converts the workflow output into a HttpResponse
    public static HttpResponse WorkflowResultToHttpReponse(
        Result<IList<PlaceOrderEvent>, PlaceOrderError> result)
    {
        if (result is Ok<IList<PlaceOrderEvent>, PlaceOrderError> events)
        {
            var dtos = events.Value.Select(x => PlaceOrderEventDto.FromDomain(x)).ToArray();
            var json = JsonSerialization.SerializeJson(dtos);
            return new HttpResponse(
                HttpStatusCode: 200,
                Body: json);
        }
        else if (result is Error<IList<PlaceOrderEvent>, PlaceOrderError> err)
        {
            var dto = PlaceOrderErrorDto.FromDomain(err.Value);
            var json = JsonSerialization.SerializeJson(dto);
            return new HttpResponse(
                HttpStatusCode: 401,
                Body: json);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public static readonly PlaceOrderApi PlaceOrderApi = async request =>
    {
        // following the approach in "A Complete Serialization Pipeline" in chapter 11

        // start with a string
        var orderFormJson = request.Body;
        var orderForm = JsonSerialization.DeserializeJson<OrderFormDto>(orderFormJson);
        // convert to domain object
        var unvalidatedOrder = orderForm!.ToUnvalidatedOrder();

        // setup the dependencies. See "Injecting Dependencies" in chapter 9
        var workflow =
            OverallWorkflow.PlaceOrder(
                checkProductExists, // dependency
                checkAddressExists, // dependency
                getProductPrice,    // dependency
                createOrderAcknowledgmentLetter,  // dependency
                sendOrderAcknowledgment); // dependency

        // now we are in the pure domain
        var asyncResult = await workflow(unvalidatedOrder);

        // now convert from the pure domain back to a HttpResponse
        return WorkflowResultToHttpReponse(asyncResult);
    };
}

