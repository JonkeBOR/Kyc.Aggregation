namespace Kyc.Aggregation.Application.Models;

/// <summary>
/// KYC form data returned from the Customer Data API.
/// </summary>
public class KycFormData
{
    /// <summary>
    /// Dictionary of KYC form fields keyed by field name.
    /// </summary>
    public required Dictionary<string, string> Items { get; set; } = new();
}
