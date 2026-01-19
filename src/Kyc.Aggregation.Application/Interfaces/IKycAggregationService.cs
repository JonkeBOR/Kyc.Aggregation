using Kyc.Aggregation.Application.Models;
using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Interfaces;

public interface IKycAggregationService
{
    AggregatedKycDataDto AggregateData(
        string ssn,
        PersonalDetailsData personalDetails,
        ContactDetailsData? contactDetails,
        KycFormData? kycForm);
}
