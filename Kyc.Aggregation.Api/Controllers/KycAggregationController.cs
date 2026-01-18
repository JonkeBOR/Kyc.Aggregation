using Kyc.Aggregation.Application.Features.KycAggregation.GetAggregatedKycData;
using Kyc.Aggregation.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kyc.Aggregation.Api.Controllers;

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
    [ProduceResponseType(typeof(AggregatedKycDataDto), StatusCodes.Status200OK)]
    [ProduceResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProduceResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProduceResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAggregatedKycData(
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
