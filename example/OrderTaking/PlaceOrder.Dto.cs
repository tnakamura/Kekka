using Kekka;
using OrderTaking.Common;

namespace OrderTaking.PlaceOrder;

// ======================================================
// This file contains the logic for working with data transfer objects (DTOs)
//
// This represents the code in chapter 11, "Serialization"
//
// Each type of DTO is defined using primitive, serializable types
// and then there are `toDomain` and `fromDomain` functions defined for each DTO.
//
// ======================================================


// ==================================
// DTOs for PlaceOrder workflow
// ==================================

internal static class Utils
{
    /// <summary>
    /// Helper function to get the value from an Option, and if None, use the defaultValue
    /// Note that the defaultValue is the first parameter, unlike the similar `defaultArg`
    /// </summary>
    public static T DefaultIfNone<T>(T defaultValue, T? opt)
    {
        return opt ?? defaultValue;
    }
    // could also use
    // defaultArg opt defaultValue
}

//===============================================
// DTO for CustomerInfo
//===============================================

public record CustomerInfoDto(
    string FirstName,
    string LastName,
    string EmailAddress)
{
    /// <summary>
    /// Convert a CustomerInfo object into the corresponding DTO.
    /// Used when exporting from the domain to the outside world.
    /// </summary>
    public static CustomerInfoDto FromCustomerInfo(Common.CustomerInfo domainObj)
    {
        // this is a simple 1:1 copy
        return new CustomerInfoDto(
            FirstName: domainObj.Name.FirstName.Value,
            LastName: domainObj.Name.LastName.Value,
            EmailAddress: domainObj.EmailAddress.Value);
    }
};

/// <summary>
/// Functions for converting between the DTO and corresponding domain object
/// </summary>
internal static class CustomerInfoDtoExtensions
{
    /// <summary>
    /// Convert the DTO into a UnvalidatedCustomerInfo object.
    /// This always succeeds because there is no validation.
    /// Used when importing an OrderForm from the outside world into the domain.
    /// </summary>
    public static UnvalidatedCustomerInfo ToUnvalidatedCustomerInfo(this CustomerInfoDto dto)
    {
        // sometimes it's helpful to use an explicit type annotation
        // to avoid ambiguity between records with the same field names.
        var domainObj = new UnvalidatedCustomerInfo(
            // this is a simple 1:1 copy which always succeeds
            FirstName: dto.FirstName,
            LastName: dto.LastName,
            EmailAddress: dto.EmailAddress);
        return domainObj;
    }

    /// <summary>
    /// Convert the DTO into a CustomerInfo object
    /// Used when importing from the outside world into the domain, eg loading from a database
    /// </summary>
    public static Result<CustomerInfo, string> ToCustomerInfo(this CustomerInfoDto dto)
    {
        var result = from first in String50.Create("FirstName", dto.FirstName)
                     from last in String50.Create("LastName", dto.LastName)
                     from email in Common.EmailAddress.Create("EmailAddress", dto.EmailAddress)
                     select new CustomerInfo(
                         Name: new PersonalName(
                             FirstName: first,
                             LastName: last),
                         EmailAddress: email);
        return result;
    }
}

//===============================================
// DTO for Address
//===============================================

public record AddressDto(
    string AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string? AddressLine4,
    string City,
    string ZipCode)
{
    /// <summary>
    /// Convert a Address object into the corresponding DTO.
    /// Used when exporting from the domain to the outside world.
    /// </summary>
    public static AddressDto FromAddress(Common.Address domainObj)
    {
        // this is a simple 1:1 copy
        return new(
            AddressLine1: domainObj.AddressLine1.Value,
            AddressLine2: domainObj.AddressLine2?.Value ?? null,
            AddressLine3: domainObj.AddressLine3?.Value ?? null,
            AddressLine4: domainObj.AddressLine4?.Value ?? null,
            City: domainObj.City.Value,
            ZipCode: domainObj.ZipCode.Value);
    }
}

/// <summary>
/// Functions for converting between the DTO and corresponding domain object
/// </summary>
internal static class AddressDtoExtensions
{
    /// <summary>
    /// Convert the DTO into a UnvalidatedAddress
    /// This always succeeds because there is no validation.
    /// Used when importing an OrderForm from the outside world into the domain.
    /// </summary>
    public static UnvalidatedAddress ToUnvalidatedAddress(this AddressDto dto)
    {
        // this is a simple 1:1 copy
        return new(
            AddressLine1: dto.AddressLine1,
            AddressLine2: dto.AddressLine2,
            AddressLine3: dto.AddressLine3,
            AddressLine4: dto.AddressLine4,
            City: dto.City,
            ZipCode: dto.ZipCode);
    }

    /// <summary>
    /// Convert the DTO into a Address object
    /// Used when importing from the outside world into the domain, eg loading from a database.
    /// </summary>
    public static Result<Common.Address, string> ToAddress(this AddressDto dto)
    {
        var result =
            from addressLine1 in String50.Create("AddressLine1", dto.AddressLine1)
            from addressLine2 in String50.CreateOption("AddressLine2", dto.AddressLine2)
            from addressLine3 in String50.CreateOption("AddressLine3", dto.AddressLine3)
            from addressLine4 in String50.CreateOption("AddressLine4", dto.AddressLine4)
            from city in String50.Create("City", dto.City)
            from zipCode in ZipCode.Create("ZipCode", dto.City)
            select new Common.Address(
                AddressLine1: addressLine1,
                AddressLine2: addressLine2,
                AddressLine3: addressLine3,
                AddressLine4: addressLine4,
                City: city,
                ZipCode: zipCode);
        return result;
    }
}


//===============================================
// DTOs for OrderLines
//===============================================

/// <summary>
/// From the order form used as input
/// </summary>
public record OrderFormLineDto(
    string OrderLineId,
    string ProductCode,
    decimal Quantity);

/// <summary>
/// Functions relating to the OrderLine DTOs
/// </summary>
internal static class OrderLineDtoExtensions
{
    /// <summary>
    /// Convert the OrderFormLine into a UnvalidatedOrderLine
    /// This always succeeds because there is no validation.
    /// Used when importing an OrderForm from the outside world into the domain.
    /// </summary>
    public static UnvalidatedOrderLine ToUnvalidatedOrderLine(this OrderFormLineDto dto)
    {
        // this is a simple 1:1 copy
        return new(
            OrderLineId: dto.OrderLineId,
            ProductCode: dto.ProductCode,
            Quantity: dto.Quantity);
    }
}

//===============================================
// DTOs for PricedOrderLines
//===============================================

/// <summary>
/// Used in the output of the workflow
/// </summary>
public record PricedOrderLineDto(
    string OrderLineId,
    string ProductCode,
    decimal Quantity,
    decimal LinePrice)
{
    /// <summary>
    /// Convert a PricedOrderLine object into the corresponding DTO.
    /// Used when exporting from the domain to the outside world.
    /// </summary>
    public static PricedOrderLineDto FromDomain(PricedOrderLine domainObj)
    {
        // this is a simple 1:1 copy
        return new(
            OrderLineId: domainObj.OrderLineId.Value,
            ProductCode: domainObj.ProductCode.PrimitiveValue,
            Quantity: domainObj.Quantity.PrimitiveValue,
            LinePrice: domainObj.LinePrice.Value);
    }
}

//===============================================
// DTO for OrderForm
//===============================================

public record OrderFormDto(
    string OrderId,
    CustomerInfoDto CustomerInfo,
    AddressDto ShippingAddress,
    AddressDto BillingAddress,
    IList<OrderFormLineDto> Lines);

/// <summary>
/// Functions relating to the Order DTOs
/// </summary>
internal static class OrderFormDtoExtensions
{
    /// <summary>
    /// Convert the OrderForm into a UnvalidatedOrder
    /// This always succeeds because there is no validation.
    /// </summary>
    public static UnvalidatedOrder ToUnvalidatedOrder(this OrderFormDto dto)
    {
        return new(
            OrderId: dto.OrderId,
            CustomerInfo: dto.CustomerInfo.ToUnvalidatedCustomerInfo(),
            ShippingAddress: dto.ShippingAddress.ToUnvalidatedAddress(),
            BillingAddress: dto.BillingAddress.ToUnvalidatedAddress(),
            Lines: dto.Lines.Select(x => x.ToUnvalidatedOrderLine()).ToList()
        );
    }
}

//===============================================
// DTO for OrderPlaced event
//===============================================

/// <summary>
/// Event to send to shipping context
/// </summary>
public record OrderPlacedDto(
    string OrderId,
    CustomerInfoDto CustomerInfo,
    AddressDto ShippingAddress,
    AddressDto BillingAddress,
    decimal AmountToBill,
    IList<PricedOrderLineDto> Lines)
{
    /// <summary>
    /// Convert a OrderPlaced object into the corresponding DTO.
    /// Used when exporting from the domain to the outside world.
    /// </summary>
    public static OrderPlacedDto FromDomain(OrderPlaced domainObj)
    {
        return new(
            OrderId: domainObj.Order.OrderId.Value,
            CustomerInfo: CustomerInfoDto.FromCustomerInfo(domainObj.Order.CustomerInfo),
            ShippingAddress: AddressDto.FromAddress(domainObj.Order.ShippingAddress),
            BillingAddress: AddressDto.FromAddress(domainObj.Order.BillingAddress),
            AmountToBill: domainObj.Order.AmountToBill.Value,
            Lines: domainObj.Order.Lines.Select(x => PricedOrderLineDto.FromDomain(x)).ToList());
    }
}

//===============================================
// DTO for BillableOrderPlaced event
//===============================================

/// <summary>
/// Event to send to billing context
/// </summary>
public record BillableOrderPlacedDto(
    string OrderId,
    AddressDto BillingAddress,
    decimal AmountToBill)
{
    /// <summary>
    /// Convert a BillableOrderPlaced object into the corresponding DTO.
    /// Used when exporting from the domain to the outside world.
    /// </summary>
    public static BillableOrderPlacedDto FromDomain(BillableOrderPlaced domainObj)
    {
        return new(
            OrderId: domainObj.OrderId.Value,
            BillingAddress: AddressDto.FromAddress(domainObj.BillingAddress),
            AmountToBill: domainObj.AmountToBill.Value);
    }
}

//===============================================
// DTO for OrderAcknowledgmentSent event
//===============================================

/// Event to send to other bounded contexts
public record OrderAcknowledgmentSentDto(
    string OrderId,
    string EmailAddress)
{
    /// <summary>
    /// Convert a OrderAcknowledgmentSent object into the corresponding DTO.
    /// Used when exporting from the domain to the outside world.
    /// </summary>
    public static OrderAcknowledgmentSentDto FromDomain(OrderAcknowledgmentSent domainObj)
    {
        return new(
            OrderId: domainObj.OrderId.Value,
            EmailAddress: domainObj.EmailAddress.Value);
    }
}


//===============================================
// DTO for PlaceOrderEvent
//===============================================

/// <summary>
/// Use a dictionary representation of a PlaceOrderEvent, suitable for JSON
/// See "Serializing Records and Choice Types Using Maps" in chapter 11
/// </summary>
public class PlaceOrderEventDto : Dictionary<string, object>
{
    /// <summary>
    /// Convert a PlaceOrderEvent into the corresponding DTO.
    /// Used when exporting from the domain to the outside world.
    /// </summary>
    public static PlaceOrderEventDto FromDomain(PlaceOrderEvent domainObj)
    {
        if (domainObj is OrderPlaced orderPlaced)
        {
            return new PlaceOrderEventDto
            {
                ["OrderPlaced"] = OrderPlacedDto.FromDomain(orderPlaced),
            };
        }
        else if (domainObj is BillableOrderPlaced billableOrderPlaced)
        {
            return new PlaceOrderEventDto
            {
                ["BillableOrderPlaced"] = BillableOrderPlacedDto.FromDomain(billableOrderPlaced),
            };
        }
        else if (domainObj is OrderAcknowledgmentSent acknowledgmentSent)
        {
            return new PlaceOrderEventDto
            {
                ["OrderAcknowledgmentSent"] = OrderAcknowledgmentSentDto.FromDomain(acknowledgmentSent),
            };
        }
        else
        {
            throw new NotSupportedException();
        }
    }
}

//===============================================
// DTO for PlaceOrderError
//===============================================

public record PlaceOrderErrorDto(
    string Code,
    string Message)
{
    public static PlaceOrderErrorDto FromDomain(PlaceOrderError domainObj)
    {
        if (domainObj is ValidationError validationError)
        {
            return new PlaceOrderErrorDto(
                Code: "ValidationError",
                Message: validationError.Message);
        }
        else if (domainObj is PricingError pricingError)
        {
            return new PlaceOrderErrorDto(
                Code: "PricingError",
                Message: pricingError.Message);
        }
        else if (domainObj is RemoteServiceError remoteServiceError)
        {
            return new PlaceOrderErrorDto(
                Code: "RemoteServiceError",
                Message: $"{remoteServiceError.Service.Name}: {remoteServiceError.Exception.Message}");
        }
        else
        {
            throw new NotSupportedException();
        }
    }
}

