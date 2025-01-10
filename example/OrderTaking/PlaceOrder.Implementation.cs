using Kekka;
using OrderTaking.Common;

namespace OrderTaking.PlaceOrder.Implementation;

// ======================================================
// This file contains the final implementation for the PlaceOrder workflow
//
// This represents the code in chapter 10, "Working with Errors"
//
// There are two parts:
// * the first section contains the (type-only) definitions for each step
// * the second section contains the implementations for each step
//   and the implementation of the overall workflow
// ======================================================


// ======================================================
// Section 1 : Define each step in the workflow using types
// ======================================================

// ---------------------------
// Validation step
// ---------------------------

// Product validation

public delegate bool CheckProductCodeExists(ProductCode productCode);

/// <summary>
/// Address validation
/// </summary>
public enum AddressValidationError
{
    InvalidFormat,
    AddressNotFound,
}

public record CheckedAddress(
    string AddressLine1,
    Optional<string> AddressLine2,
    Optional<string> AddressLine3,
    Optional<string> AddressLine4,
    string City,
    string ZipCode) : UnvalidatedAddress(
        AddressLine1: AddressLine1,
        AddressLine2: AddressLine2,
        AddressLine3: AddressLine3,
        AddressLine4: AddressLine4,
        City: City,
        ZipCode: ZipCode);

public delegate Task<Result<CheckedAddress, AddressValidationError>> CheckAddressExists(UnvalidatedAddress address);

// ---------------------------
// Validated Order
// ---------------------------

public record ValidatedOrderLine(
    OrderLineId OrderLineId,
    ProductCode ProductCode,
    OrderQuantity Quantity);

public record ValidatedOrder(
    OrderId OrderId,
    CustomerInfo CustomerInfo,
    Address ShippingAddress,
    Address BillingAddress,
    IList<ValidatedOrderLine> Lines);

public delegate Task<Result<ValidatedOrder, ValidationError>> ValidateOrder(
    CheckProductCodeExists checkProductCodeExists,  // dependency
    CheckAddressExists checkAddressExists,  // dependency
    UnvalidatedOrder unvalidatedOrder);    // input

// ---------------------------
// Pricing step
// ---------------------------

public delegate Price GetProductPrice(
    ProductCode productCode);

// priced state is defined Domain.WorkflowTypes

public delegate Result<PricedOrder, PricingError> PriceOrder(
    GetProductPrice getProductPrice,     // dependency
    ValidatedOrder validatedOrder);  // input

// ---------------------------
// Send OrderAcknowledgment
// ---------------------------

public record struct HtmlString(string String);

public record OrderAcknowledgment(
    EmailAddress EmailAddress,
    HtmlString Letter);

public delegate HtmlString CreateOrderAcknowledgmentLetter(PricedOrder pricedOrder);

/// Send the order acknowledgement to the customer
/// Note that this does NOT generate an Result-type error (at least not in this workflow)
/// because on failure we will continue anyway.
/// On success, we will generate a OrderAcknowledgmentSent event,
/// but on failure we won't.

public enum SendResult
{
    Sent,
    NotSent
}

public delegate SendResult SendOrderAcknowledgment(
    OrderAcknowledgment orderAcknowledgment);

public delegate Optional<PlaceOrderEvent> AcknowledgeOrder(
    CreateOrderAcknowledgmentLetter createOrderAcknowledgmentLetter,  // dependency
    SendOrderAcknowledgment sendOrderAcknowledgment,      // dependency
    PricedOrder pricedOrder);                  // input

// ---------------------------
// Create events
// ---------------------------

public delegate IList<PlaceOrderEvent> CreateEvents(
    PricedOrder pricedOrder,                           // input
    Optional<PlaceOrderEvent> orderAcknowledgmentSent);   // input (event from previous step)


// ======================================================
// Section 2 : Implementation
// ======================================================

// ---------------------------
// ValidateOrder step
// ---------------------------

public static class ValidateOrderStep
{
    public static Result<CustomerInfo, ValidationError> ToCustomerInfo(this UnvalidatedCustomerInfo unvalidatedCustomerInfo)
    {
        var result =
            from firstName in String50.Create("FirstName", unvalidatedCustomerInfo.FirstName).MapError(x => new ValidationError(x))
            from lastName in String50.Create("LastName", unvalidatedCustomerInfo.LastName).MapError(x => new ValidationError(x))
            from emailAddress in EmailAddress.Create("EmailAddress", unvalidatedCustomerInfo.EmailAddress).MapError(x => new ValidationError(x))
            select new CustomerInfo(
                Name: new PersonalName(
                    FirstName: firstName,
                    LastName: lastName),
                EmailAddress: emailAddress);
        return result;
    }

    public static Result<Address, ValidationError> ToAddress(CheckedAddress checkedAddress)
    {
        var result =
            from addressLine1 in String50.Create("AddressLine1", checkedAddress.AddressLine1).MapError(x => new ValidationError(x))
            from addressLine2 in String50.CreateOption("AddressLine2", checkedAddress.AddressLine2).MapError(x => new ValidationError(x))
            from addressLine3 in String50.CreateOption("AddressLine3", checkedAddress.AddressLine3).MapError(x => new ValidationError(x))
            from addressLine4 in String50.CreateOption("AddressLine4", checkedAddress.AddressLine4).MapError(x => new ValidationError(x))
            from city in String50.Create("City", checkedAddress.City).MapError(x => new ValidationError(x))
            from zipCode in ZipCode.Create("ZipCode", checkedAddress.ZipCode).MapError(x => new ValidationError(x))
            select new Address(
                AddressLine1: addressLine1,
                AddressLine2: addressLine2,
                AddressLine3: addressLine3,
                AddressLine4: addressLine4,
                City: city,
                ZipCode: zipCode);
        return result;
    }

    /// <summary>
    /// Call the checkAddressExists and convert the error to a ValidationError
    /// </summary>
    public static Task<Result<CheckedAddress, ValidationError>> ToCheckedAddress(CheckAddressExists checkAddress, UnvalidatedAddress address)
    {
        var result =
            from checkedAddress in checkAddress(address).MapError(addrError =>
            {
                return addrError switch
                {
                    AddressValidationError.AddressNotFound => new ValidationError("Address not found"),
                    AddressValidationError.InvalidFormat => new ValidationError("Address has bad format"),
                    _ => throw new NotSupportedException()
                };
            })
            select checkedAddress;
        return result;
    }

    public static Result<OrderId, ValidationError> ToOrderId(this string orderId) =>
        OrderId.Create("OrderId", orderId).MapError(x => new ValidationError(x));

    /// <summary>
    /// Helper function for validateOrder
    /// </summary>
    public static Result<OrderLineId, ValidationError> ToOrderLineId(this string orderId) =>
        OrderLineId.Create("OrderLineId", orderId).MapError(x => new ValidationError(x));

    /// Helper function for validateOrder
    public static Result<ProductCode, ValidationError> ToProductCode(CheckProductCodeExists checkProductCodeExists, string productCode)
    {
        // create a ProductCode -> Result<ProductCode,...> function
        // suitable for using in a pipeline
        Result<ProductCode, ValidationError> CheckProduct(ProductCode productCode)
        {
            if (checkProductCodeExists(productCode))
            {
                return Result.Ok<ProductCode, ValidationError>(productCode);
            }
            else
            {
                return Result.Error<ProductCode, ValidationError>(
                    new ValidationError($"Invalid: {productCode}"));
            }
        }

        // assemble the pipeline
        var result =
            from c in ProductCode.Create("ProductCode", productCode).MapError(e => new ValidationError(e))
            select CheckProduct(c) into s
            from a in s
            select a;
        return result;
    }

    /// <summary>
    /// Helper function for validateOrder
    /// </summary>
    public static Result<OrderQuantity, ValidationError> ToOrderQuantity(ProductCode productCode, decimal quantity) =>
        OrderQuantity.Create("OrderQuantity", productCode, quantity)
            .MapError(x => new ValidationError(x));

    /// <summary>
    /// Helper function for validateOrder
    /// </summary>
    public static Result<ValidatedOrderLine, ValidationError> ToValidatedOrderLine(
        CheckProductCodeExists checkProductExists,
        UnvalidatedOrderLine unvalidatedOrderLine)
    {
        var result =
            from orderLineId in ToOrderLineId(unvalidatedOrderLine.OrderLineId)
            from productCode in ToProductCode(checkProductExists, unvalidatedOrderLine.ProductCode)
            from quantity in ToOrderQuantity(productCode, unvalidatedOrderLine.Quantity)
            select new ValidatedOrderLine(orderLineId, productCode, quantity);
        return result;
    }

    public static readonly ValidateOrder ValidateOrder = new ValidateOrder(
        (checkProductCodeExists, checkAddressExists, unvalidatedOrder) =>
        {
            var result =
                from orderId in ToOrderId(unvalidatedOrder.OrderId).AsTask()
                from customerInfo in ToCustomerInfo(unvalidatedOrder.CustomerInfo).AsTask()
                from checkedShippingAddress in ToCheckedAddress(checkAddressExists, unvalidatedOrder.ShippingAddress)
                from shippingAddress in ToAddress(checkedShippingAddress).AsTask()
                from checkedBillingAddress in ToCheckedAddress(checkAddressExists, unvalidatedOrder.BillingAddress)
                from billingAddress in ToAddress(checkedBillingAddress).AsTask()
                from lines in unvalidatedOrder.Lines.Select(x => ToValidatedOrderLine(checkProductCodeExists, x)).Sequence().AsTask()
                select new ValidatedOrder(
                    OrderId: orderId,
                    CustomerInfo: customerInfo,
                    ShippingAddress: shippingAddress,
                    BillingAddress: billingAddress,
                    Lines: lines.ToList());
            return result;
        });
}

// ---------------------------
// PriceOrder step
// ---------------------------

public static class PriceOrderStep
{
    public static Result<PricedOrderLine, PricingError> ToPricedOrderLine(GetProductPrice getProductPrice, ValidatedOrderLine validatedOrderLine)
    {
        var result =
            from qty in Price.Create(validatedOrderLine.Quantity.PrimitiveValue).MapError(x => new PricingError(x))
            let p = getProductPrice(validatedOrderLine.ProductCode)
            select qty * p into linePrices
            from linePrice in linePrices.MapError(x => new PricingError(x))
            select new PricedOrderLine(
                OrderLineId: validatedOrderLine.OrderLineId,
                ProductCode: validatedOrderLine.ProductCode,
                Quantity: validatedOrderLine.Quantity,
                LinePrice: linePrice);
        return result;
    }

    public static readonly PriceOrder PriceOrder = new PriceOrder(
        (getProductPrice, validatedOrder) =>
        {
            var result =
                from lines in validatedOrder.Lines.Select(x => ToPricedOrderLine(getProductPrice, x)).Sequence()
                from amountBill in BillingAmount.SumPrices(lines.Select(x => x.LinePrice)).MapError(x => new PricingError(x))
                select new PricedOrder(
                    OrderId: validatedOrder.OrderId,
                    CustomerInfo: validatedOrder.CustomerInfo,
                    ShippingAddress: validatedOrder.ShippingAddress,
                    BillingAddress: validatedOrder.BillingAddress,
                    Lines: lines.ToList(),
                    AmountToBill: amountBill);
            return result;
        });
}


// ---------------------------
// AcknowledgeOrder step
// ---------------------------

public static class AcknowledgeOrderStep
{
    public static readonly AcknowledgeOrder AcknowledgeOrder = new AcknowledgeOrder(
        (createAcknowledgmentLetter, sendAcknowledgment, pricedOrder) =>
        {
            var letter = createAcknowledgmentLetter(pricedOrder);
            var acknowledgment = new OrderAcknowledgment(
                EmailAddress: pricedOrder.CustomerInfo.EmailAddress,
                Letter: letter);

            // if the acknowledgement was successfully sent,
            // return the corresponding event, else return None
            switch (sendAcknowledgment(acknowledgment))
            {
                case SendResult.Sent:
                    return Optional.Some<PlaceOrderEvent>(
                        new OrderAcknowledgmentSent(
                            OrderId: pricedOrder.OrderId,
                            EmailAddress: pricedOrder.CustomerInfo.EmailAddress));
                case SendResult.NotSent:
                    return Optional.None<PlaceOrderEvent>();
                default:
                    return Optional.None<PlaceOrderEvent>();
            }
        });
}

// ---------------------------
// Create events
// ---------------------------

public static class CreateEventsStep
{
    public static Optional<PlaceOrderEvent> CreateOrderPlacedEvent(PricedOrder placedOrder) =>
        Optional.Some<PlaceOrderEvent>(new OrderPlaced(placedOrder));

    public static Optional<PlaceOrderEvent> CreateBillingEvent(PricedOrder placedOrder)
    {
        var billingAmount = placedOrder.AmountToBill.Value;
        if (billingAmount > 0M)
        {
            return Optional.Some<PlaceOrderEvent>(
                new BillableOrderPlaced(
                    OrderId: placedOrder.OrderId,
                    BillingAddress: placedOrder.BillingAddress,
                    AmountToBill: placedOrder.AmountToBill));
        }
        else
        {
            return Optional.None<PlaceOrderEvent>();
        }
    }

    /// <summary>
    /// helper to convert an Option into a List
    /// </summary>
    public static IList<T> ListOfOption<T>(Optional<T> opt)
        where T : notnull
    {
        if (opt.TryGet(out var value))
        {
            return new List<T>() { value };
        }
        else
        {
            return new List<T>();
        }
    }

    public static readonly CreateEvents CreateEvents = new CreateEvents(
        (pricedOrder, acknowledgmentEventOpt) =>
        {
            var acknowledgmentEvents = ListOfOption(acknowledgmentEventOpt);
            var orderPlacedEvents = ListOfOption<PlaceOrderEvent>(CreateOrderPlacedEvent(pricedOrder));
            var billingEvents = ListOfOption<PlaceOrderEvent>(CreateBillingEvent(pricedOrder));
            // return all the events
            return acknowledgmentEvents.Concat(orderPlacedEvents).Concat(billingEvents).ToList();
        });
}


// ---------------------------
// overall workflow
// ---------------------------

public static class OverallWorkflow
{
    public static PlaceOrder PlaceOrder(
        CheckProductCodeExists checkProductExists, // dependency
        CheckAddressExists checkAddressExists, // dependency
        GetProductPrice getProductPrice,   // dependency
        CreateOrderAcknowledgmentLetter createOrderAcknowledgmentLetter,  // dependency
        SendOrderAcknowledgment sendOrderAcknowledgment // dependency
    )
    {
        // definition of function
        return new PlaceOrder((unvalidatedOrder) =>
        {
            var result =
                from validatedOrder in ValidateOrderStep.ValidateOrder(checkProductExists, checkAddressExists, unvalidatedOrder)
                    .MapError(x => PlaceOrderError.Validation(x.Message))
                from pricedOrder in PriceOrderStep.PriceOrder(getProductPrice, validatedOrder)
                    .AsTask()
                    .MapError(x => PlaceOrderError.Pricing(x.Message))
                let acknowledgementOption = AcknowledgeOrderStep.AcknowledgeOrder(createOrderAcknowledgmentLetter, sendOrderAcknowledgment, pricedOrder)
                let events = CreateEventsStep.CreateEvents(pricedOrder, acknowledgementOption)
                select events;
            return result;
        });
    }
}
