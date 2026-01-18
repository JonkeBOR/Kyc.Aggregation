using Kyc.Aggregation.Application.Abstractions;
using Kyc.Aggregation.Application.Models;

namespace Kyc.Aggregation.Application.Mappers;

/// <summary>
/// Maps ContactDetailsResponseDto from the Customer Data API to ContactDetailsData.
/// </summary>
public static class ContactDetailsMapper
{
    public static ContactDetailsData? Map(ContactDetailsResponseDto? dto)
    {
        if (dto == null)
            return null;

        return new ContactDetailsData
        {
            Address = BuildAddress(dto.Address),
            Email = GetPreferredEmail(dto.Emails),
            PhoneNumber = GetPreferredPhoneNumber(dto.PhoneNumbers)
        };
    }

    private static string? BuildAddress(List<AddressDto>? addresses)
    {
        if (addresses == null || addresses.Count == 0)
            return null;

        var addr = addresses[0];
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(addr.Street))
            parts.Add(addr.Street);
        if (!string.IsNullOrEmpty(addr.PostalCode))
            parts.Add(addr.PostalCode);
        if (!string.IsNullOrEmpty(addr.City))
            parts.Add(addr.City);

        return string.Join(", ", parts);
    }

    private static string? GetPreferredEmail(List<EmailDto>? emails)
    {
        if (emails == null || emails.Count == 0)
            return null;

        var preferred = emails.FirstOrDefault(e => e.Preferred);
        return preferred?.EmailAddress ?? emails[0].EmailAddress;
    }

    private static string? GetPreferredPhoneNumber(List<PhoneNumberDto>? phoneNumbers)
    {
        if (phoneNumbers == null || phoneNumbers.Count == 0)
            return null;

        var preferred = phoneNumbers.FirstOrDefault(p => p.Preferred);
        return preferred?.Number ?? phoneNumbers[0].Number;
    }
}
