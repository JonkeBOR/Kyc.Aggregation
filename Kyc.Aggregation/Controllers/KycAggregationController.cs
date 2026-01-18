using Kyc.Aggregation.Application.Features.KycAggregation.GetAggregatedKycData;
using Kyc.Aggregation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kyc.Aggregation.Controllers;

[ApiController]
[Route("kyc-data")]
public class KycAggregationController : ControllerBase
{
    private readonly IMediator _mediator;

    public KycAggregationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{ssn}")]
    public async Task<ActionResult<AggregatedKycDataDto>> GetAggregatedKycData(
        string ssn,
        CancellationToken cancellationToken)
    {
        var query = new GetAggregatedKycDataQuery { Ssn = ssn };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

public class ErrorResponse
{
    public required string Error { get; set; }
}
