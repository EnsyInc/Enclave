using EnsyInc.Enclave.Api.Exceptions;
using EnsyInc.Enclave.Api.Models;
using EnsyInc.Enclave.Api.Models.Mappers;
using EnsyInc.Enclave.Core.Errors;
using EnsyInc.Enclave.Services.Abstractions;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Enclave.Api.Controllers;

/// <summary>Manage the licenses granted to orgs for products.</summary>
[ApiController]
[Route("licenses")]
[Produces("application/json")]
public sealed class LicensesController(
    ILicensesService licensesService,
    IValidator<GrantLicenseRequest> grantLicenseValidator,
    IValidator<UpdateLicenseDatesRequest> updateLicenseDatesValidator)
    : ControllerBase
{
    /// <summary>Lists licenses, optionally filtered by org, product, and/or status.</summary>
    /// <param name="request">The request containing the filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The matching licenses.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetLicensesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLicenses([FromQuery] GetLicensesRequest request, CancellationToken ct)
    {
        var result = await licensesService.ListLicenses(request.OrgId, request.ProductId, request.Status, ct);

        return result switch
        {
            { HasError: false } => Ok(new GetLicensesResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Gets a single license by id.</summary>
    /// <param name="id">The license's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The license.</response>
    /// <response code="404">No license exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetLicenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLicense(Guid id, CancellationToken ct)
    {
        var result = await licensesService.GetLicense(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: LicenseNotFoundError } => NotFound(ErrorResponses.LicenseNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Grants a new license to an org for a product.</summary>
    /// <param name="request">The license to grant.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The license was granted. The response body and <c>Location</c> header describe the new license.</response>
    /// <response code="400">The request failed validation (e.g. an end date before the start date).</response>
    /// <response code="404">No org or product exists with the given id.</response>
    /// <response code="409">The org already has an active license for this product.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GetLicenseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GrantLicense(GrantLicenseRequest request, CancellationToken ct)
    {
        await grantLicenseValidator.ValidateAndThrowAsync(request, ct);

        var result = await licensesService.GrantLicense(request.OrgId, request.ProductId, request.Start, request.End, ct);

        return result switch
        {
            { HasError: false } => CreatedAtAction(nameof(GetLicense), new { id = result.Data.Id }, result.Data.ToPublicModel()),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            { HasError: true, Error: ProductNotFoundError } => NotFound(ErrorResponses.ProductNotFoundError),
            { HasError: true, Error: LicenseAlreadyExistsError e } => Conflict(ErrorResponses.LicenseAlreadyExistsError(e.ExistingLicenseId)),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Updates an existing license's start and end dates.</summary>
    /// <param name="id">The license's id.</param>
    /// <param name="request">The new date range.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The updated license.</response>
    /// <response code="400">The request failed validation (e.g. an end date before the start date).</response>
    /// <response code="404">No license exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GetLicenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLicenseDates(Guid id, UpdateLicenseDatesRequest request, CancellationToken ct)
    {
        await updateLicenseDatesValidator.ValidateAndThrowAsync(request, ct);

        var result = await licensesService.UpdateLicenseDates(id, request.Start, request.End, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: LicenseNotFoundError } => NotFound(ErrorResponses.LicenseNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Suspends a license, temporarily invalidating it.</summary>
    /// <param name="id">The license's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The suspended license.</response>
    /// <response code="404">No license exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(typeof(GetLicenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SuspendLicense(Guid id, CancellationToken ct)
    {
        var result = await licensesService.SuspendLicense(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: LicenseNotFoundError } => NotFound(ErrorResponses.LicenseNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Revokes a license, permanently invalidating it.</summary>
    /// <param name="id">The license's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The revoked license.</response>
    /// <response code="404">No license exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost("{id:guid}/revoke")]
    [ProducesResponseType(typeof(GetLicenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevokeLicense(Guid id, CancellationToken ct)
    {
        var result = await licensesService.RevokeLicense(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: LicenseNotFoundError } => NotFound(ErrorResponses.LicenseNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Deletes a license. Idempotent: deleting a license that doesn't exist (or was already deleted) still succeeds.</summary>
    /// <param name="id">The license's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The license is deleted (or was already gone).</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteLicense(Guid id, CancellationToken ct)
    {
        var result = await licensesService.SoftDeleteLicense(id, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
