using System.Data;
using Npgsql;

namespace Database.Log.Undo;

public class Database : IDisposable
{
    private readonly NpgsqlConnection _connection;

    public Database(NpgsqlConnection connection)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        _connection = connection;
        _connection.Open();
    }

    public async Task DropTableIfExistsAsync(string tableName)
    {
        string sql = $"DROP TABLE IF EXISTS {tableName};";
        using var command = new NpgsqlCommand(sql, _connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task CreateTableAsync(string table, string[] columns)
    {
        string columnsAsText = string.Join(',', columns.Select(c => $"{c} INTEGER"));
        string sql = $"CREATE TABLE {table} ({columnsAsText});";
        using var command = new NpgsqlCommand(sql, _connection);
        await command.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        if (_connection.State == ConnectionState.Open)
        {
            _connection.Close();
        }
        
        _connection.Dispose();
    }
}
