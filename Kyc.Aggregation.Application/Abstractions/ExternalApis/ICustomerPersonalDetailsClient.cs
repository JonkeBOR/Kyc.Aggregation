namespace Kyc.Aggregation.Application.Abstractions.ExternalApis;

public class PersonalDetailsDto
{
    public string? FirstName { get; set; }

    public string? SurName { get; set; }
}

public interface ICustomerPersonalDetailsClient
{
    Task<PersonalDetailsDto?> GetPersonalDetailsAsync(string ssn, CancellationToken cancellationToken = default);
}
