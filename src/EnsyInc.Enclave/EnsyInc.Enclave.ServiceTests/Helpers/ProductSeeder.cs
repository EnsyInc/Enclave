using EnsyInc.Enclave.Core.Models;

using Microsoft.Data.SqlClient;

namespace EnsyInc.Enclave.ServiceTests.Helpers;

/// <summary>Direct-SQL escape hatch for seeding data the API can't produce on its own (e.g. specific states/timestamps).</summary>
public static class ProductSeeder
{
    public static async Task<Guid> InsertProduct(string connectionString, string name, string? description, ProductStatus status, CancellationToken ct)
    {
        var id = Guid.NewGuid();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Products (Id, Name, Description, Status, CreatedAt)
            VALUES (@Id, @Name, @Description, @Status, GETUTCDATE());
            """;
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", status.ToString());

        await command.ExecuteNonQueryAsync(ct);

        return id;
    }

    public static async Task DeleteProduct(string connectionString, Guid id, CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Products WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(ct);
    }
}
