using Kekka;
namespace OrderTaking.Common;

public record PersonalName(
    String50 FirstName,
    String50 LastName);

public record CustomerInfo(
    PersonalName Name,
    EmailAddress EmailAddress);

public record Address(
    String50 AddressLine1,
    Optional<String50> AddressLine2,
    Optional<String50> AddressLine3,
    Optional<String50> AddressLine4,
    String50 City,
    ZipCode ZipCode);

