using EnsyInc.Enclave.Core.Models;

using JetBrains.Annotations;

namespace EnsyInc.Enclave.Api.Models;

/// <summary>
/// Request to get a list of products, optionally filtered by name.
/// </summary>
/// <param name="Name">When provided, only products whose name contains this value are returned. Case-insensitive.</param>
[PublicAPI]
public sealed record GetProductsRequest(
    string? Name);

/// <summary>Fields for creating a new product.</summary>
/// <param name="Name">The product's display name.</param>
/// <param name="Description">An optional free-text description.</param>
/// <param name="Status">The product's initial status.</param>
[PublicAPI]
public sealed record CreateProductRequest(
    string Name,
    string? Description,
    ProductStatus Status);

/// <summary>A single product.</summary>
/// <param name="Id">The product's unique identifier.</param>
/// <param name="Name">The product's display name.</param>
/// <param name="Description">An optional free-text description.</param>
/// <param name="Status">Whether the product is currently active or retired.</param>
/// <param name="CreatedAt">When the product was created, in UTC.</param>
/// <param name="UpdatedAt">When the product was last updated, in UTC. Null if it has never been updated.</param>
[PublicAPI]
public sealed record GetProductResponse(
    Guid Id,
    string Name,
    string? Description,
    ProductStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>A page of products.</summary>
/// <param name="Products">The matching products.</param>
[PublicAPI]
public sealed record GetProductsResponse(IEnumerable<GetProductResponse> Products);

/// <summary>Fields for updating an existing product.</summary>
/// <param name="Name">The product's display name.</param>
/// <param name="Description">An optional free-text description.</param>
/// <param name="Status">The product's status.</param>
[PublicAPI]
public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    ProductStatus Status);
