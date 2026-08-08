using EnsyInc.Enclave.Api.Exceptions;
using EnsyInc.Enclave.Api.Models;
using EnsyInc.Enclave.Api.Models.Mappers;
using EnsyInc.Enclave.Core.Errors;
using EnsyInc.Enclave.Services.Abstractions;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Enclave.Api.Controllers;

/// <summary>Review the queue of customer-submitted license requests (new-license and renewal). Admin only reviews existing requests here — submitting one is a customer-facing action.</summary>
[ApiController]
[Route("license-requests")]
[Produces("application/json")]
public sealed class LicenseRequestsController(
    ILicenseRequestsService licenseRequestsService,
    IValidator<ApproveLicenseRequestRequest> approveLicenseRequestValidator,
    IValidator<RejectLicenseRequestRequest> rejectLicenseRequestValidator)
    : ControllerBase
{
    /// <summary>Lists license requests, optionally filtered by org, product, and/or status.</summary>
    /// <param name="request">The request containing the filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The matching license requests.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetLicenseRequestsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLicenseRequests([FromQuery] GetLicenseRequestsRequest request, CancellationToken ct)
    {
        var result = await licenseRequestsService.ListLicenseRequests(request.OrgId, request.ProductId, request.Status, ct);

        return result switch
        {
            { HasError: false } => Ok(new GetLicenseRequestsResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Gets a single license request by id.</summary>
    /// <param name="id">The request's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The license request.</response>
    /// <response code="404">No license request exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetLicenseRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLicenseRequest(Guid id, CancellationToken ct)
    {
        var result = await licenseRequestsService.GetLicenseRequest(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: LicenseRequestNotFoundError } => NotFound(ErrorResponses.LicenseRequestNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>
    /// Approves a license request. For a new-license request this grants a new license using both dates; for a
    /// renewal it extends the existing license's expiry date only, leaving its start date untouched.
    /// </summary>
    /// <param name="id">The request's id.</param>
    /// <param name="request">The approval dates.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The approved license request.</response>
    /// <response code="400">The request failed validation, or a start date was required but not provided.</response>
    /// <response code="404">No license request exists with the given id, or (for a renewal) its referenced license no longer exists.</response>
    /// <response code="409">The license request has already been reviewed, or the org already has an active license for this product.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(GetLicenseRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ApproveLicenseRequest(Guid id, ApproveLicenseRequestRequest request, CancellationToken ct)
    {
        await approveLicenseRequestValidator.ValidateAndThrowAsync(request, ct);

        var result = await licenseRequestsService.ApproveLicenseRequest(id, request.Start, request.End, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: LicenseRequestNotFoundError } => NotFound(ErrorResponses.LicenseRequestNotFoundError),
            { HasError: true, Error: LicenseNotFoundError } => NotFound(ErrorResponses.LicenseNotFoundError),
            { HasError: true, Error: LicenseRequestNotPendingError } => Conflict(ErrorResponses.LicenseRequestNotPendingError),
            { HasError: true, Error: LicenseAlreadyExistsError e } => Conflict(ErrorResponses.LicenseAlreadyExistsError(e.ExistingLicenseId)),
            { HasError: true, Error: LicenseRequestStartDateRequiredError } => BadRequest(ErrorResponses.LicenseRequestStartDateRequiredError),
            { HasError: true, Error: LicenseRequestInvalidDateRangeError } => BadRequest(ErrorResponses.LicenseRequestInvalidDateRangeError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Rejects a license request.</summary>
    /// <param name="id">The request's id.</param>
    /// <param name="request">The rejection reason, if any.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The rejected license request.</response>
    /// <response code="400">The request failed validation (e.g. a rejection reason that's too long).</response>
    /// <response code="404">No license request exists with the given id.</response>
    /// <response code="409">The license request has already been reviewed.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(GetLicenseRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RejectLicenseRequest(Guid id, RejectLicenseRequestRequest request, CancellationToken ct)
    {
        await rejectLicenseRequestValidator.ValidateAndThrowAsync(request, ct);

        var result = await licenseRequestsService.RejectLicenseRequest(id, request.Reason, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: LicenseRequestNotFoundError } => NotFound(ErrorResponses.LicenseRequestNotFoundError),
            { HasError: true, Error: LicenseRequestNotPendingError } => Conflict(ErrorResponses.LicenseRequestNotPendingError),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
