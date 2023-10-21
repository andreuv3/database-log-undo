using Npgsql;

namespace Database.Log.Undo;

public class DatabaseFactory
{
    public static Database BuildFromConnectionString(string connectionString)
    {
        var connection = new NpgsqlConnection(connectionString);
        return new Database(connection);
    }
}
