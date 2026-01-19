using Kyc.Aggregation.Application.Models;

namespace Kyc.Aggregation.Application.Interfaces;

public interface ICustomerKycDataProvider
{
    Task<CustomerKycInput> 
        GetCustomerKycInputAsync(string ssn, CancellationToken ct = default);
}
