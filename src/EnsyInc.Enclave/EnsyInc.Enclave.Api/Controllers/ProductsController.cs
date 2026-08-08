using EnsyInc.Enclave.Api.Models;
using EnsyInc.Enclave.Api.Models.Mappers;
using EnsyInc.Enclave.Core.Errors;
using EnsyInc.Enclave.Services.Abstractions;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace EnsyInc.Enclave.Api.Controllers;

/// <summary>Manage the catalog of licensable products.</summary>
[ApiController]
[Route("products")]
[Produces("application/json")]
public sealed class ProductsController(
    IProductsService productsService,
    IValidator<CreateProductRequest> createProductValidator,
    IValidator<UpdateProductRequest> updateProductValidator)
    : ControllerBase
{
    /// <summary>Lists products, optionally filtered by name.</summary>
    /// <param name="name">When provided, only products whose name contains this value are returned.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The matching products.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetProductsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProducts([FromQuery] string? name, CancellationToken ct)
    {
        var result = string.IsNullOrWhiteSpace(name)
            ? await productsService.ListProduct(ct)
            : await productsService.ListProductByName(name, ct);

        return result switch
        {
            { HasError: false } => Ok(new GetProductsResponse(result.Data.Select(x => x.ToPublicModel()))),
            _ => throw new("Unexpected error occurred."),
        };
    }

    /// <summary>Gets a single product by id.</summary>
    /// <param name="id">The product's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The product.</response>
    /// <response code="404">No product exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken ct)
    {
        var result = await productsService.GetProduct(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data!.ToPublicModel()),
            { HasError: true, Error: ProductNotFoundError } => NotFound(ErrorResponses.ProductNotFoundError),
            _ => throw new("Unexpected error occurred."),
        };
    }

    /// <summary>Creates a new product.</summary>
    /// <param name="request">The product to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">The product was created. The response body and <c>Location</c> header describe the new product.</response>
    /// <response code="400">The request failed validation (e.g. a missing name).</response>
    /// <response code="404">The product could not be created.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GetProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request, CancellationToken ct)
    {
        await createProductValidator.ValidateAndThrowAsync(request, ct);

        var product = request.ToCoreModel();
        var result = await productsService.CreateProduct(product);

        return result switch
        {
            { HasError: false } => CreatedAtAction(nameof(GetProduct), new { id = result.Data!.Id }, result.Data!.ToPublicModel()),
            { HasError: true, Error: ProductNotFoundError } => NotFound(ErrorResponses.ProductNotFoundError),
            _ => throw new("Unexpected error occurred."),
        };
    }

    /// <summary>Updates an existing product's name, description, and status.</summary>
    /// <param name="id">The product's id.</param>
    /// <param name="request">The new field values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The updated product.</response>
    /// <response code="400">The request failed validation (e.g. a missing name).</response>
    /// <response code="404">No product exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(GetProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        await updateProductValidator.ValidateAndThrowAsync(request, ct);

        var product = request.ToCoreModel(id);
        var result = await productsService.UpdateProduct(product, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data!.ToPublicModel()),
            { HasError: true, Error: ProductNotFoundError } => NotFound(ErrorResponses.ProductNotFoundError),
            _ => throw new("Unexpected error occurred."),
        };
    }

    /// <summary>Retires a product, taking it out of the catalog without deleting it.</summary>
    /// <param name="id">The product's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The retired product.</response>
    /// <response code="404">No product exists with the given id.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost("{id:guid}/retire")]
    [ProducesResponseType(typeof(GetProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RetireProduct(Guid id, CancellationToken ct)
    {
        var result = await productsService.RetireProduct(id, ct);

        return result switch
        {
            { HasError: false } => Ok(result.Data!.ToPublicModel()),
            { HasError: true, Error: ProductNotFoundError } => NotFound(ErrorResponses.ProductNotFoundError),
            _ => throw new("Unexpected error occurred."),
        };
    }

    /// <summary>Deletes a product. Idempotent: deleting a product that doesn't exist (or was already deleted) still succeeds.</summary>
    /// <param name="id">The product's id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">The product is deleted (or was already gone).</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken ct)
    {
        var result = await productsService.SoftDeleteProduct(id, ct);

        return result switch
        {
            { HasError: false } => NoContent(),
            _ => throw new("Unexpected error occurred."),
        };
    }
}
