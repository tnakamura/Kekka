// We are defining types and submodules, so we can use a namespace
// rather than a module at the top level
using Kekka;
using OrderTaking.Common;

namespace OrderTaking.PlaceOrder;

// ==================================
// This file contains the definitions of PUBLIC types (exposed at the boundary of the bounded context)
// related to the PlaceOrder workflow
// ==================================

// ------------------------------------
// inputs to the workflow

public record UnvalidatedCustomerInfo(
    string FirstName,
    string LastName,
    string EmailAddress);

public record UnvalidatedAddress(
    string AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string? AddressLine4,
    string City,
    string ZipCode);

public record UnvalidatedOrderLine(
    string OrderLineId,
    string ProductCode,
    decimal Quantity);

public record UnvalidatedOrder(
    string OrderId,
    UnvalidatedCustomerInfo CustomerInfo,
    UnvalidatedAddress ShippingAddress,
    UnvalidatedAddress BillingAddress,
    IList<UnvalidatedOrderLine> Lines);

// ------------------------------------
// outputs from the workflow (success case)

/// Event will be created if the Acknowledgment was successfully posted
public record OrderAcknowledgmentSent(
    OrderId OrderId,
    EmailAddress EmailAddress) : PlaceOrderEvent;


// priced state
public record PricedOrderLine(
    OrderLineId OrderLineId,
    ProductCode ProductCode,
    OrderQuantity Quantity,
    Price LinePrice);

public record PricedOrder(
    OrderId OrderId,
    CustomerInfo CustomerInfo,
    Address ShippingAddress,
    Address BillingAddress,
    BillingAmount AmountToBill,
    IList<PricedOrderLine> Lines);

/// Event to send to shipping context
public record OrderPlaced(PricedOrder Order) : PlaceOrderEvent;

/// Event to send to billing context
/// Will only be created if the AmountToBill is not zero
public record BillableOrderPlaced(
    OrderId OrderId,
    Address BillingAddress,
    BillingAmount AmountToBill) : PlaceOrderEvent;

/// The possible events resulting from the PlaceOrder workflow
/// Not all events will occur, depending on the logic of the workflow
public partial record PlaceOrderEvent { }


// ------------------------------------
// error outputs


/// All the things that can go wrong in this workflow
public record ValidationError(string Message) : PlaceOrderError;

public record PricingError(string Message) : PlaceOrderError;

public record ServiceInfo(
    string Name,
    System.Uri Endpoint);

public record RemoteServiceError(
   ServiceInfo Service,
    System.Exception Exception) : PlaceOrderError;

public record PlaceOrderError { }

// ------------------------------------
// the workflow itself

public delegate Task<Result<IList<PlaceOrderEvent>, PlaceOrderError>> PlaceOrder(UnvalidatedOrder order);

