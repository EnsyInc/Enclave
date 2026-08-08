using EnsyInc.Enclave.Api.Exceptions;
using EnsyInc.Enclave.Api.Models;
using EnsyInc.Enclave.Api.Models.Mappers;
using EnsyInc.Enclave.Core.Errors;
using EnsyInc.Enclave.Services.Abstractions;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Enclave.Api.Controllers;

/// <summary>Manage the users belonging to an org.</summary>
[ApiController]
[Route("orgs/{orgId:guid}/users")]
[Produces("application/json")]
public sealed class UsersController(
    IUsersService usersService,
    IValidator<InviteUserRequest> inviteUserValidator,
    IValidator<InviteUsersRequest> inviteUsersValidator,
    IValidator<UpdateUserRequest> updateUserValidator)
    : ControllerBase
{
    /// <summary>Lists the users belonging to an org.</summary>
    /// <param name="orgId">The org's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The org's users.</response>
    /// <response code="404">No org exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetUsersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers(Guid orgId, CancellationToken ct)
    {
        var result = await usersService.ListUsers(orgId, ct);

        return result switch
        {
            { HasError: false } => Ok(new GetUsersResponse(result.Data.Select(x => x.ToPublicModel()))),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Gets a single user by id.</summary>
    /// <param name="orgId">The org's id.</param>
    /// <param name="id">The user's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The user.</response>
    /// <response code="404">No org exists with the given id, or no user with the given id exists in that org.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUser(Guid orgId, Guid id, CancellationToken ct)
    {
        var result = await usersService.GetUser(orgId, id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            { HasError: true, Error: UserNotFoundError } => NotFound(ErrorResponses.UserNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Invites a single new user into the org.</summary>
    /// <param name="orgId">The org's id.</param>
    /// <param name="request">The user to invite.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The user was invited. The response body and <c>Location</c> header describe the new user.</response>
    /// <response code="400">The request failed validation (e.g. an invalid email).</response>
    /// <response code="404">No org exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InviteUser(Guid orgId, InviteUserRequest request, CancellationToken ct)
    {
        await inviteUserValidator.ValidateAndThrowAsync(request, ct);

        var user = request.ToCoreModel(orgId);
        var result = await usersService.InviteUser(orgId, user, ct);

        return result switch
        {
            { HasError: false } => CreatedAtAction(nameof(GetUser), new { orgId, id = result.Data.Id }, result.Data.ToPublicModel()),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            { HasError: true, Error: UserNotFoundError } => NotFound(ErrorResponses.UserNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Invites a batch of new users into the org.</summary>
    /// <param name="orgId">The org's id.</param>
    /// <param name="request">The users to invite.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The invited users.</response>
    /// <response code="400">The request failed validation (e.g. an invalid email).</response>
    /// <response code="404">No org exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(GetUsersResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InviteUsers(Guid orgId, InviteUsersRequest request, CancellationToken ct)
    {
        await inviteUsersValidator.ValidateAndThrowAsync(request, ct);

        var users = request.Users.Select(u => u.ToCoreModel(orgId));
        var result = await usersService.InviteUsers(orgId, users, ct);

        return result switch
        {
            { HasError: false } => StatusCode(StatusCodes.Status201Created, new GetUsersResponse(result.Data.Select(x => x.ToPublicModel()))),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Updates an existing user's name and role.</summary>
    /// <param name="orgId">The org's id.</param>
    /// <param name="id">The user's id.</param>
    /// <param name="request">The new field values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The updated user.</response>
    /// <response code="400">The request failed validation (e.g. a missing name).</response>
    /// <response code="404">No org exists with the given id, or no user with the given id exists in that org.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser(Guid orgId, Guid id, UpdateUserRequest request, CancellationToken ct)
    {
        await updateUserValidator.ValidateAndThrowAsync(request, ct);

        var result = await usersService.UpdateUser(orgId, id, request.Name, request.Role, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            { HasError: true, Error: UserNotFoundError } => NotFound(ErrorResponses.UserNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Deactivates a user, preventing them from accessing the system.</summary>
    /// <param name="orgId">The org's id.</param>
    /// <param name="id">The user's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The deactivated user.</response>
    /// <response code="404">No org exists with the given id, or no user with the given id exists in that org.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeactivateUser(Guid orgId, Guid id, CancellationToken ct)
    {
        var result = await usersService.DeactivateUser(orgId, id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            { HasError: true, Error: UserNotFoundError } => NotFound(ErrorResponses.UserNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Reactivates a previously deactivated user.</summary>
    /// <param name="orgId">The org's id.</param>
    /// <param name="id">The user's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The reactivated user.</response>
    /// <response code="404">No org exists with the given id, or no user with the given id exists in that org.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReactivateUser(Guid orgId, Guid id, CancellationToken ct)
    {
        var result = await usersService.ReactivateUser(orgId, id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data.ToPublicModel()),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            { HasError: true, Error: UserNotFoundError } => NotFound(ErrorResponses.UserNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }

    /// <summary>Deletes a user. Idempotent: deleting a user that doesn't exist (or was already deleted) still succeeds.</summary>
    /// <param name="orgId">The org's id.</param>
    /// <param name="id">The user's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The user is deleted (or was already gone).</response>
    /// <response code="404">No org exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUser(Guid orgId, Guid id, CancellationToken ct)
    {
        var result = await usersService.SoftDeleteUser(orgId, id, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            { HasError: true, Error: OrgNotFoundError } => NotFound(ErrorResponses.OrgNotFoundError),
            _ => throw new UnhandledResultErrorException(),
        };
    }
}
