namespace Kyc.Aggregation.Application.Models;

public sealed record CustomerKycInput(
    PersonalDetailsData? PersonalDetails,
    ContactDetailsData? ContactDetails,
    KycFormData? KycForm,
    DateTime RequestedAtUtc);
