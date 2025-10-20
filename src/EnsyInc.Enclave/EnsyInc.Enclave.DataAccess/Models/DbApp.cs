using EnsyInc.Enclave.Core.Models;

using EnsyNet.DataAccess.Abstractions.Models;

namespace EnsyInc.Enclave.DataAccess.Models;

public sealed record DbApp : DbEntity
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public static class AppExtensions
{
    public static DbApp ToDbApp(this App app) =>
        new()
        {
            Id = app.Id,
            Name = app.Name,
            Description = app.Description,
        };

    public static App ToCoreApp(this DbApp dbApp) =>
        new()
        {
            Id = dbApp.Id,
            Name = dbApp.Name,
            Description = dbApp.Description,
            CreatedAt = dbApp.CreatedAt,
            LastUpdatedAt = dbApp.UpdatedAt
        };
}
