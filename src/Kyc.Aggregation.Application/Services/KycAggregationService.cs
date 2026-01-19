using Kyc.Aggregation.Application.Exceptions;
using Kyc.Aggregation.Application.Interfaces;
using Kyc.Aggregation.Application.Models;
using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Services;

public class KycAggregationService() : IKycAggregationService
{
    public AggregatedKycDataDto AggregateData(
        string ssn,
        PersonalDetailsData personalDetails,
        ContactDetailsData? contactDetails,
        KycFormData? kycForm)
    {
        var taxCountry = ExtractTaxCountry(kycForm);
       
        var income = ExtractIncome(kycForm);

        var address = ExtractAddress(contactDetails);

        var phoneNumber = ExtractPhoneNumber(contactDetails);

        var email = ExtractEmail(contactDetails);

        return new AggregatedKycDataDto
        {
            Ssn = ssn,
            FirstName = personalDetails.FirstName,
            LastName = personalDetails.LastName,
            Address = address ?? throw new ValidationException("Address is required but not available"),
            PhoneNumber = phoneNumber,
            Email = email,
            TaxCountry = taxCountry ?? throw new ValidationException("Tax country is required but not available"),
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
