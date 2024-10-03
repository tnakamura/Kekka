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
    string? AddressLine2,
    string? AddressLine3,
    string? AddressLine4,
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

public delegate OrderAcknowledgment? AcknowledgeOrder(
    CreateOrderAcknowledgmentLetter createOrderAcknowledgmentLetter,  // dependency
    SendOrderAcknowledgment sendOrderAcknowledgment,      // dependency
    PricedOrder pricedOrder);                  // input

// ---------------------------
// Create events
// ---------------------------

public delegate IList<PlaceOrderEvent> CreateEvents(
    PricedOrder pricedOrder,                           // input
    OrderAcknowledgmentSent? orderAcknowledgmentSent);   // input (event from previous step)


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
            from firstName in String50.Create("FirstName", unvalidatedCustomerInfo.FirstName).MapError(x=>new ValidationError(x))
            from lastName in String50.Create("LastName", unvalidatedCustomerInfo.LastName).MapError(x=>new ValidationError(x))
            from emailAddress in EmailAddress.Create("EmailAddress", unvalidatedCustomerInfo.EmailAddress).MapError(x=>new ValidationError(x))
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
}


/// Call the checkAddressExists and convert the error to a ValidationError
let toCheckedAddress (checkAddress:CheckAddressExists) address =
    address
    |> checkAddress
    |> AsyncResult.mapError (fun addrError ->
        match addrError with
        | AddressNotFound -> ValidationError "Address not found"
        | InvalidFormat -> ValidationError "Address has bad format"
        )

let toOrderId orderId =
    orderId
    |> OrderId.create "OrderId"
    |> Result.mapError ValidationError // convert creation error into ValidationError

/// Helper function for validateOrder
let toOrderLineId orderId =
    orderId
    |> OrderLineId.create "OrderLineId"
    |> Result.mapError ValidationError // convert creation error into ValidationError

/// Helper function for validateOrder
let toProductCode (checkProductCodeExists:CheckProductCodeExists) productCode =

    // create a ProductCode -> Result<ProductCode,...> function
    // suitable for using in a pipeline
    let checkProduct productCode  =
        if checkProductCodeExists productCode then
            Ok productCode
        else
            let msg = sprintf "Invalid: %A" productCode
            Error (ValidationError msg)

    // assemble the pipeline
    productCode
    |> ProductCode.create "ProductCode"
    |> Result.mapError ValidationError // convert creation error into ValidationError
    |> Result.bind checkProduct

/// Helper function for validateOrder
let toOrderQuantity productCode quantity =
    OrderQuantity.create "OrderQuantity" productCode quantity
    |> Result.mapError ValidationError // convert creation error into ValidationError

/// Helper function for validateOrder
let toValidatedOrderLine checkProductExists (unvalidatedOrderLine:UnvalidatedOrderLine) =
    result {
        let! orderLineId =
            unvalidatedOrderLine.OrderLineId
            |> toOrderLineId
        let! productCode =
            unvalidatedOrderLine.ProductCode
            |> toProductCode checkProductExists
        let! quantity =
            unvalidatedOrderLine.Quantity
            |> toOrderQuantity productCode
        let validatedOrderLine = {
            OrderLineId = orderLineId
            ProductCode = productCode
            Quantity = quantity
            }
        return validatedOrderLine
    }

let validateOrder : ValidateOrder =
    fun checkProductCodeExists checkAddressExists unvalidatedOrder ->
        asyncResult {
            let! orderId =
                unvalidatedOrder.OrderId
                |> toOrderId
                |> AsyncResult.ofResult
            let! customerInfo =
                unvalidatedOrder.CustomerInfo
                |> toCustomerInfo
                |> AsyncResult.ofResult
            let! checkedShippingAddress =
                unvalidatedOrder.ShippingAddress
                |> toCheckedAddress checkAddressExists
            let! shippingAddress =
                checkedShippingAddress
                |> toAddress
                |> AsyncResult.ofResult
            let! checkedBillingAddress =
                unvalidatedOrder.BillingAddress
                |> toCheckedAddress checkAddressExists
            let! billingAddress  =
                checkedBillingAddress
                |> toAddress
                |> AsyncResult.ofResult
            let! lines =
                unvalidatedOrder.Lines
                |> List.map (toValidatedOrderLine checkProductCodeExists)
                |> Result.sequence // convert list of Results to a single Result
                |> AsyncResult.ofResult
            let validatedOrder : ValidatedOrder = {
                OrderId  = orderId
                CustomerInfo = customerInfo
                ShippingAddress = shippingAddress
                BillingAddress = billingAddress
                Lines = lines
            }
            return validatedOrder
        }

// ---------------------------
// PriceOrder step
// ---------------------------

let toPricedOrderLine (getProductPrice:GetProductPrice) (validatedOrderLine:ValidatedOrderLine) =
    result {
        let qty = validatedOrderLine.Quantity |> OrderQuantity.value
        let price = validatedOrderLine.ProductCode |> getProductPrice
        let! linePrice =
            Price.multiply qty price
            |> Result.mapError PricingError // convert to PlaceOrderError
        let pricedLine : PricedOrderLine = {
            OrderLineId = validatedOrderLine.OrderLineId
            ProductCode = validatedOrderLine.ProductCode
            Quantity = validatedOrderLine.Quantity
            LinePrice = linePrice
            }
        return pricedLine
    }


let priceOrder : PriceOrder =
    fun getProductPrice validatedOrder ->
        result {
            let! lines =
                validatedOrder.Lines
                |> List.map (toPricedOrderLine getProductPrice)
                |> Result.sequence // convert list of Results to a single Result
            let! amountToBill =
                lines
                |> List.map (fun line -> line.LinePrice)  // get each line price
                |> BillingAmount.sumPrices                // add them together as a BillingAmount
                |> Result.mapError PricingError           // convert to PlaceOrderError
            let pricedOrder : PricedOrder = {
                OrderId  = validatedOrder.OrderId
                CustomerInfo = validatedOrder.CustomerInfo
                ShippingAddress = validatedOrder.ShippingAddress
                BillingAddress = validatedOrder.BillingAddress
                Lines = lines
                AmountToBill = amountToBill
            }
            return pricedOrder
        }


// ---------------------------
// AcknowledgeOrder step
// ---------------------------

let acknowledgeOrder : AcknowledgeOrder =
    fun createAcknowledgmentLetter sendAcknowledgment pricedOrder ->
        let letter = createAcknowledgmentLetter pricedOrder
        let acknowledgment = {
            EmailAddress = pricedOrder.CustomerInfo.EmailAddress
            Letter = letter
            }

        // if the acknowledgement was successfully sent,
        // return the corresponding event, else return None
        match sendAcknowledgment acknowledgment with
        | Sent ->
            let event = {
                OrderId = pricedOrder.OrderId
                EmailAddress = pricedOrder.CustomerInfo.EmailAddress
                }
            Some event
        | NotSent ->
            None

// ---------------------------
// Create events
// ---------------------------

let createOrderPlacedEvent (placedOrder:PricedOrder) : OrderPlaced =
    placedOrder

let createBillingEvent (placedOrder:PricedOrder) : BillableOrderPlaced option =
    let billingAmount = placedOrder.AmountToBill |> BillingAmount.value
    if billingAmount > 0M then
        {
        OrderId = placedOrder.OrderId
        BillingAddress = placedOrder.BillingAddress
        AmountToBill = placedOrder.AmountToBill
        } |> Some
    else
        None

/// helper to convert an Option into a List
let listOfOption opt =
    match opt with
    | Some x -> [x]
    | None -> []

let createEvents : CreateEvents =
    fun pricedOrder acknowledgmentEventOpt ->
        let acknowledgmentEvents =
            acknowledgmentEventOpt
            |> Option.map PlaceOrderEvent.AcknowledgmentSent
            |> listOfOption
        let orderPlacedEvents =
            pricedOrder
            |> createOrderPlacedEvent
            |> PlaceOrderEvent.OrderPlaced
            |> List.singleton
        let billingEvents =
            pricedOrder
            |> createBillingEvent
            |> Option.map PlaceOrderEvent.BillableOrderPlaced
            |> listOfOption

        // return all the events
        [
        yield! acknowledgmentEvents
        yield! orderPlacedEvents
        yield! billingEvents
        ]


// ---------------------------
// overall workflow
// ---------------------------

let placeOrder
    checkProductExists // dependency
    checkAddressExists // dependency
    getProductPrice    // dependency
    createOrderAcknowledgmentLetter  // dependency
    sendOrderAcknowledgment // dependency
    : PlaceOrder =       // definition of function

    fun unvalidatedOrder ->
        asyncResult {
            let! validatedOrder =
                validateOrder checkProductExists checkAddressExists unvalidatedOrder
                |> AsyncResult.mapError PlaceOrderError.Validation
            let! pricedOrder =
                priceOrder getProductPrice validatedOrder
                |> AsyncResult.ofResult
                |> AsyncResult.mapError PlaceOrderError.Pricing
            let acknowledgementOption =
                acknowledgeOrder createOrderAcknowledgmentLetter sendOrderAcknowledgment pricedOrder
            let events =
                createEvents pricedOrder acknowledgementOption
            return events
        }
