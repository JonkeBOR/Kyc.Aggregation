using Kyc.Aggregation.Application.Abstractions;
using Kyc.Aggregation.Application.Models;

namespace Kyc.Aggregation.Application.Mappers;

/// <summary>
/// Maps KycFormResponseDto from the Customer Data API to KycFormData.
/// </summary>
public static class KycFormMapper
{
    public static KycFormData? Map(KycFormResponseDto? dto)
    {
        if (dto?.Items == null)
            return null;

        var items = new Dictionary<string, string>();
        foreach (var item in dto.Items)
        {
            if (item.Key != null && item.Value != null)
            {
                items[item.Key] = item.Value;
            }
        }

        return new KycFormData { Items = items };
    }
}
