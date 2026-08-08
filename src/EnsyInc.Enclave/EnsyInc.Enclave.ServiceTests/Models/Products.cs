using System.Text.Json.Serialization;

namespace EnsyInc.Enclave.ServiceTests.Models;

public sealed record CreateProductRequest(
    string Name,
    string? Description,
    [property: JsonRequired] ProductStatus Status);

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    [property: JsonRequired] ProductStatus Status);

public sealed record GetProductResponse(
    Guid Id,
    string Name,
    string? Description,
    [property: JsonRequired] ProductStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record GetProductsResponse(IEnumerable<GetProductResponse> Products);
