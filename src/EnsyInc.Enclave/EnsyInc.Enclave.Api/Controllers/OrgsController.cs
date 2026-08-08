using EnsyInc.Enclave.Api.Exceptions;
using EnsyInc.Enclave.Api.Models;
using EnsyInc.Enclave.Api.Models.Mappers;
using EnsyInc.Enclave.Core.Errors;
using EnsyInc.Enclave.Services.Abstractions;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Enclave.Api.Controllers;

/// <summary>Manage the org (customer) records used to grant licenses against.</summary>
[ApiController]
[Route("orgs")]
[Produces("application/json")]
public sealed class OrgsController(
    IOrgsService orgsService,
    IValidator<CreateOrgRequest> createOrgValidator,
    IValidator<UpdateOrgRequest> updateOrgValidator)
    : ControllerBase
{
    /// <summary>Lists orgs, optionally filtered by name.</summary>
    /// <param name="request">The request containing the name filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The matching orgs.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetOrgsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrgs([FromQuery] GetOrgsRequest request, CancellationToken ct)
    {
        var result = string.IsNullOrWhiteSpace(request.Name)
            ? await orgsService.ListOrgs(ct)
            : await orgsService.ListOrgsByName(request.Name, ct);

        return result switch
        {
            { HasError: false } => Ok(new GetOrgsResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Gets a single org by id.</summary>
    /// <param name="id">The org's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The org.</response>
    /// <response code="404">No org exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetOrgResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrg(Guid id, CancellationToken ct)
    {
        var result = await orgsService.GetOrg(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data!.ToPublicModel()),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Creates a new org.</summary>
    /// <param name="request">The org to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The org was created. The response body and <c>Location</c> header describe the new org.</response>
    /// <response code="400">The request failed validation (e.g. a missing name).</response>
    /// <response code="404">The org could not be created.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GetOrgResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrg(CreateOrgRequest request, CancellationToken ct)
    {
        await createOrgValidator.ValidateAndThrowAsync(request, ct);

        var org = request.ToCoreModel();
        var result = await orgsService.CreateOrg(org);

        return result switch
        {
            { HasError: false } => CreatedAtAction(nameof(GetOrg), new { id = result.Data!.Id }, result.Data!.ToPublicModel()),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Updates an existing org's name.</summary>
    /// <param name="id">The org's id.</param>
    /// <param name="request">The new field values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The updated org.</response>
    /// <response code="400">The request failed validation (e.g. a missing name).</response>
    /// <response code="404">No org exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GetOrgResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrg(Guid id, UpdateOrgRequest request, CancellationToken ct)
    {
        await updateOrgValidator.ValidateAndThrowAsync(request, ct);

        var org = request.ToCoreModel(id);
        var result = await orgsService.UpdateOrg(org, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data!.ToPublicModel()),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Deactivates an org, overriding its computed status until it's reactivated.</summary>
    /// <param name="id">The org's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The deactivated org.</response>
    /// <response code="404">No org exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(GetOrgResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeactivateOrg(Guid id, CancellationToken ct)
    {
        var result = await orgsService.DeactivateOrg(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data!.ToPublicModel()),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Reactivates a previously deactivated org, falling back to its computed status.</summary>
    /// <param name="id">The org's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The reactivated org.</response>
    /// <response code="404">No org exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(typeof(GetOrgResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReactivateOrg(Guid id, CancellationToken ct)
    {
        var result = await orgsService.ReactivateOrg(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data!.ToPublicModel()),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Deletes an org. Idempotent: deleting an org that doesn't exist (or was already deleted) still succeeds.</summary>
    /// <param name="id">The org's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The org is deleted (or was already gone).</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteOrg(Guid id, CancellationToken ct)
    {
        var result = await orgsService.SoftDeleteOrg(id, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
