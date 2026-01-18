using Kyc.Aggregation.Application.Models;
using Kyc.Aggregation.Application.Models.ExternalApis;

namespace Kyc.Aggregation.Application.Mappers;

/// <summary>
/// Maps PersonalDetailsResponseDto from the Customer Data API to PersonalDetailsData.
/// </summary>
public static class PersonalDetailsMapper
{
    public static PersonalDetailsData Map(PersonalDetailsResponseDto dto)
    {
        return new PersonalDetailsData
        {
            FirstName = dto.FirstName ?? throw new InvalidOperationException("FirstName is required"),
            LastName = dto.SurName ?? throw new InvalidOperationException("LastName is required")
        };
    }
}
