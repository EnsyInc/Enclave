using EnsyInc.Enclave.ServiceTests.Models;

using Microsoft.Data.SqlClient;

namespace EnsyInc.Enclave.ServiceTests.Helpers;

/// <summary>Direct-SQL escape hatch for seeding data the API can't produce on its own (e.g. specific states/timestamps).</summary>
public static class OrgSeeder
{
    public static async Task<Guid> InsertOrg(string connectionString, string name, OrgStatus status, CancellationToken ct)
    {
        var id = Guid.NewGuid();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Orgs (Id, Name, Status, CreatedAt)
            VALUES (@Id, @Name, @Status, GETUTCDATE());
            """;
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@Status", status.ToString());

        await command.ExecuteNonQueryAsync(ct);

        return id;
    }

    public static async Task DeleteOrg(string connectionString, Guid id, CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Orgs WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(ct);
    }
}
