using Kyc.Aggregation.Application.Features.GetAggregatedKycData;
using Kyc.Aggregation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kyc.Aggregation.Api.Controllers;

/// <summary>
/// Controller for KYC aggregation API endpoints.
/// </summary>
[ApiController]
[Route("api/kyc-data")]
public class KycAggregationController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Gets aggregated KYC data for a customer by SSN.
    /// </summary>
    /// <param name="ssn">Social Security Number of the customer</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Aggregated KYC data</returns>
    [HttpGet("{ssn}")]
    public async Task<ActionResult<AggregatedKycDataDto>> GetAggregatedKycData(
        string ssn,
        CancellationToken ct)
    {
        var query = new GetAggregatedKycDataQuery(ssn);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}
