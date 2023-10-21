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

    public async Task InsertAsync(string table, IDictionary<string, int[]> columns)
    {
        string columnsAsText = string.Join(',', columns.Keys);
        string valuesAsText = BuildValuesAsText(columns);
        string sql = $"INSERT INTO {table} ({columnsAsText}) VALUES {valuesAsText};";
        using var command = new NpgsqlCommand(sql, _connection);
        await command.ExecuteNonQueryAsync();
    }

    private string BuildValuesAsText(IDictionary<string, int[]> columns)
    {
        int tuplesCount = columns.First().Value.Length;
        string[] values = new string[tuplesCount];
        for (int i = 0; i < tuplesCount; i++)
        {
            var tupleValues = columns
                .Select(columnWithValues => columnWithValues.Value[i])
                .ToArray();
            
            values[i] = $"({string.Join(',', tupleValues)})";
        }
        return string.Join(',', values);
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
