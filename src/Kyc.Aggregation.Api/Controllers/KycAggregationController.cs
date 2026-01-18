using Kyc.Aggregation.Application.Features.KycAggregation.GetAggregatedKycData;
using Kyc.Aggregation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kyc.Aggregation.Api.Controllers;

/// <summary>
/// Controller for KYC aggregation API endpoints.
/// </summary>
[ApiController]
[Route("api/kyc-data")]
public class KycAggregationController : ControllerBase
{
    private readonly IMediator _mediator;

    public KycAggregationController(IMediator mediator)
    {
        _mediator = mediator;
    }

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
