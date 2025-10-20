using System.Net.Mime;

using EnsyInc.Enclave.Api.Models;
using EnsyInc.Enclave.Api.Models.Apps;

using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Enclave.Api.Controllers;

[ApiController]
[Route("apps")]
public class AppController : ControllerBase
{
    /// <summary>
    /// Registers a new application in the Enclave system.
    /// </summary>
    /// <param name="request">Details about the app to register</param>
    /// <param name="ct"></param>
    [HttpPost]
    [ProducesResponseType<GetAppResponse>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status503ServiceUnavailable, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> RegisterApp([FromBody] RegisterAppRequest request, CancellationToken ct)
    {
        await Task.Delay(10, ct);
        return Ok();
    }

    /// <summary>
    /// Unregisters an existing application from the Enclave system.
    /// </summary>
    /// <remarks>
    /// Application will be soft deleted and can be restored within 30 days.
    /// </remarks>
    /// <param name="id">The id of the application to unregister.</param>
    /// <param name="ct"></param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status503ServiceUnavailable, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> UnregisterApp([FromRoute] Guid id, CancellationToken ct)
    {
        await Task.Delay(10, ct);
        return Ok();
    }

    /// <summary>
    /// Retrieves the details of an existing application from the Enclave system.
    /// </summary>
    /// <param name="id">The id of the application to retrieve.</param>
    /// <param name="ct"></param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetAppResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status503ServiceUnavailable, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetApp([FromRoute] Guid id, CancellationToken ct)
    {
        await Task.Delay(10, ct);
        return Ok();
    }

    /// <summary>
    /// Retrieves the details of all applications from the Enclave system.
    /// </summary>
    /// <remarks>
    /// Will most likely be paginated in the future.
    /// </remarks>
    /// <param name="ct"></param>
    [HttpGet]
    [ProducesResponseType<GetAppsResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status503ServiceUnavailable, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetApps(CancellationToken ct)
    {
        await Task.Delay(10, ct);
        return Ok();
    }

    /// <summary>
    /// Updates the details of an existing application from the Enclave system.
    /// </summary>
    /// <remarks>
    /// Only properties that are not null in the request will be updated.
    /// If needed a more complex update mechanism can be implemented later.
    /// </remarks>
    /// <param name="id">The id of the application to update.</param>
    /// <param name="request">Updates to be applied to the application.</param>
    /// <param name="ct"></param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status503ServiceUnavailable, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> UpdateApp([FromRoute] Guid id, [FromBody] UpdateAppRequest request, CancellationToken ct)
    {
        await Task.Delay(10, ct);
        return Ok();
    }
}
