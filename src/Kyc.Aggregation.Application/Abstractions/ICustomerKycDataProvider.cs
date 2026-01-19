using Kyc.Aggregation.Application.Models;

namespace Kyc.Aggregation.Application.Abstractions;

public interface ICustomerKycDataProvider
{
    Task<CustomerKycInput> 
        GetCustomerKycInputAsync(string ssn, CancellationToken ct = default);
}
