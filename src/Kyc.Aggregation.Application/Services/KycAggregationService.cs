using Kyc.Aggregation.Application.Abstractions;
using Kyc.Aggregation.Application.Models;
using Kyc.Aggregation.Contracts;
using Microsoft.Extensions.Logging;

namespace Kyc.Aggregation.Application.Services;

/// <summary>
/// Service that aggregates customer data from multiple sources into a single KYC response.
/// </summary>
public interface IKycAggregationService
{
    /// <summary>
    /// Aggregates personal details, contact details, and KYC form data into a single DTO.
    /// </summary>
    AggregatedKycDataDto AggregateData(
        string ssn,
        PersonalDetailsData personalDetails,
        ContactDetailsData? contactDetails,
        KycFormData? kycForm);
}

public class KycAggregationService(ILogger<KycAggregationService> logger) : IKycAggregationService
{
    private readonly ILogger<KycAggregationService> _logger = logger;

    public AggregatedKycDataDto AggregateData(
        string ssn,
        PersonalDetailsData personalDetails,
        ContactDetailsData? contactDetails,
        KycFormData? kycForm)
    {
        _logger.LogDebug("Aggregating KYC data for SSN: {Ssn}", ssn);

        // Extract tax country from KYC form
        var taxCountry = ExtractTaxCountry(kycForm);
        
        // Extract income from KYC form
        var income = ExtractIncome(kycForm);

        // Extract address from contact details or KYC form
        var address = ExtractAddress(contactDetails);

        // Extract phone number from contact details
        var phoneNumber = ExtractPhoneNumber(contactDetails);

        // Extract email from contact details
        var email = ExtractEmail(contactDetails);

        return new AggregatedKycDataDto
        {
            Ssn = ssn,
            FirstName = personalDetails.FirstName,
            LastName = personalDetails.LastName,
            Address = address ?? throw new InvalidOperationException("Address is required but not available"),
            PhoneNumber = phoneNumber,
            Email = email,
            TaxCountry = taxCountry ?? throw new InvalidOperationException("Tax country is required but not available"),
            Income = income
        };
    }

    private string? ExtractTaxCountry(KycFormData? kycForm)
    {
        if (kycForm?.Items == null)
            return null;

        // Try both lowercase and Pascal case keys (API inconsistency)
        if (kycForm.Items.TryGetValue("tax_country", out var value))
            return value;
        
        if (kycForm.Items.TryGetValue("Tax_Country", out var pascalValue))
            return pascalValue;

        return null;
    }

    private int? ExtractIncome(KycFormData? kycForm)
    {
        if (kycForm?.Items == null)
            return null;

        // Try common field names for income
        var incomeKey = kycForm.Items.Keys.FirstOrDefault(k =>
            k.Equals("annual_income", StringComparison.OrdinalIgnoreCase) ||
            k.Equals("Annual_Income", StringComparison.OrdinalIgnoreCase));

        if (incomeKey != null && kycForm.Items.TryGetValue(incomeKey, out var value))
        {
            if (int.TryParse(value, out var income))
                return income;
        }

        return null;
    }

    private string? ExtractAddress(ContactDetailsData? contactDetails)
    {
        if (contactDetails == null)
            return null;

        return contactDetails.Address;
    }

    private string? ExtractPhoneNumber(ContactDetailsData? contactDetails)
    {
        if (contactDetails == null)
            return null;

        return contactDetails.PhoneNumber;
    }

    private string? ExtractEmail(ContactDetailsData? contactDetails)
    {
        if (contactDetails == null)
            return null;

        return contactDetails.Email;
    }
}
