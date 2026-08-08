using EnsyInc.Enclave.ServiceTests.Models;

using Microsoft.Data.SqlClient;

namespace EnsyInc.Enclave.ServiceTests.Helpers;

/// <summary>Direct-SQL escape hatch for seeding data the API can't produce on its own (e.g. specific states/timestamps).</summary>
public static class LicenseSeeder
{
    public static async Task<Guid> InsertLicense(string connectionString, Guid orgId, Guid productId, DateTime start, DateTime end, LicenseStatus status, CancellationToken ct)
    {
        var id = Guid.NewGuid();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Licenses (Id, OrgId, ProductId, Start, [End], Status, CreatedAt)
            VALUES (@Id, @OrgId, @ProductId, @Start, @End, @Status, GETUTCDATE());
            """;
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@OrgId", orgId);
        command.Parameters.AddWithValue("@ProductId", productId);
        command.Parameters.AddWithValue("@Start", start);
        command.Parameters.AddWithValue("@End", end);
        command.Parameters.AddWithValue("@Status", status.ToString());

        await command.ExecuteNonQueryAsync(ct);

        return id;
    }

    public static async Task DeleteLicense(string connectionString, Guid id, CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Licenses WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(ct);
    }
}
