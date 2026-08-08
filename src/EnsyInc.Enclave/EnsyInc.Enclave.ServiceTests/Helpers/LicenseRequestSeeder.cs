using EnsyInc.Enclave.ServiceTests.Models;

using Microsoft.Data.SqlClient;

namespace EnsyInc.Enclave.ServiceTests.Helpers;

/// <summary>
/// Direct-SQL escape hatch for seeding data the API can't produce on its own. Admin has no endpoint to create a
/// license request (submitting one is a customer-facing action not built yet), so every license request used in
/// admin ServiceTests is seeded here.
/// </summary>
public static class LicenseRequestSeeder
{
    public static async Task<Guid> InsertLicenseRequest(
        string connectionString,
        Guid orgId,
        Guid productId,
        Guid userId,
        Guid? existingLicenseId,
        string? requestNotes,
        LicenseRequestStatus status,
        CancellationToken ct)
    {
        var id = Guid.NewGuid();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO LicenseRequests (Id, OrgId, ProductId, UserId, ExistingLicenseId, RequestNotes, Status, CreatedAt)
            VALUES (@Id, @OrgId, @ProductId, @UserId, @ExistingLicenseId, @RequestNotes, @Status, GETUTCDATE());
            """;
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@OrgId", orgId);
        command.Parameters.AddWithValue("@ProductId", productId);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@ExistingLicenseId", existingLicenseId.HasValue ? existingLicenseId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@RequestNotes", (object?)requestNotes ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", status.ToString());

        await command.ExecuteNonQueryAsync(ct);

        return id;
    }

    public static async Task DeleteLicenseRequest(string connectionString, Guid id, CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM LicenseRequests WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(ct);
    }
}
