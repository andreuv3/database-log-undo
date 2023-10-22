using System.Data;
using Npgsql;

namespace Log.Undo;

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

    public void DropTableIfExists(string tableName)
    {
        string sql = $"DROP TABLE IF EXISTS {tableName};";
        Execute(sql);
    }

    public void CreateTable(string table, string[] columns)
    {
        string columnsAsText = string.Join(',', columns.Select(c => $"{c} INTEGER"));
        string sql = $"CREATE TABLE {table} ({columnsAsText});";
        Execute(sql);
    }

    public void Insert(string table, IDictionary<string, int[]> columns)
    {
        string columnsAsText = string.Join(',', columns.Keys);
        string valuesAsText = BuildValuesAsText(columns);
        string sql = $"INSERT INTO {table} ({columnsAsText}) VALUES {valuesAsText};";
        Execute(sql);
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

    public void Update(string table, string column, int value, int id)
    {
        string update = $"UPDATE {table} SET {column} = {value} WHERE id = {id};";
        Execute(update);
    }

    private void Execute(string sql)
    {
        using var command = new NpgsqlCommand(sql, _connection);
        command.ExecuteNonQuery();
    }

    public int Select(string table, string column, int id)
    {
        string sql = $"SELECT {column} FROM {table} WHERE id = {id};";
        using var command = new NpgsqlCommand(sql, _connection);
        using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
        reader.Read();
        return reader.GetInt32(column);
    }

    public void Dispose()
    {
        if (_connection.State == ConnectionState.Open)
        {
            _connection.Close();
        }
        
        _connection.Dispose();
    }

    public static Database CreateFromConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("String de conexão não encontrada");
        }

        var connection = new NpgsqlConnection(connectionString);
        return new Database(connection);
    }
}
