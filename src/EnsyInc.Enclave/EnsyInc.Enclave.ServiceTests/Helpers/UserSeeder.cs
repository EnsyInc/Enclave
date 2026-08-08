using EnsyInc.Enclave.ServiceTests.Models;

using Microsoft.Data.SqlClient;

namespace EnsyInc.Enclave.ServiceTests.Helpers;

/// <summary>Direct-SQL escape hatch for seeding data the API can't produce on its own (e.g. specific states/timestamps).</summary>
public static class UserSeeder
{
    public static async Task<Guid> InsertUser(string connectionString, Guid orgId, string name, string email, UserStatus status, UserRole role, CancellationToken ct)
    {
        var id = Guid.NewGuid();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Users (Id, Name, Email, OrgId, Status, Role, CreatedAt)
            VALUES (@Id, @Name, @Email, @OrgId, @Status, @Role, GETUTCDATE());
            """;
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@OrgId", orgId);
        command.Parameters.AddWithValue("@Status", status.ToString());
        command.Parameters.AddWithValue("@Role", role.ToString());

        await command.ExecuteNonQueryAsync(ct);

        return id;
    }

    public static async Task DeleteUser(string connectionString, Guid id, CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Users WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(ct);
    }
}
