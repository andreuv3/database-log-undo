using Log.Undo;
using Microsoft.Extensions.Configuration;

try
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("assets/appsettings.json", false, false)
        .Build();

    string? metadataFilePath = configuration["MetadataFilePath"];
    var metadata = Metadata.CreateFromJsonFile(metadataFilePath);

    string? connectionString = configuration.GetConnectionString("Undo");
    using var database = Database.CreateFromConnectionString(connectionString);
    await database.DropTableIfExistsAsync(metadata.TableName);
    await database.CreateTableAsync(metadata.TableName, metadata.Columns);
    await database.InsertAsync(metadata.TableName, metadata.ColumnsWithValues);

    string? logFilePath = configuration["LogFilePath"];
    var log = UndoLog.Create(logFilePath, database);
    log.PerformUndo();
}
catch (Exception ex)
{
    Console.WriteLine($"Ocorreu um erro durante a execução do log undo: {ex.Message}");
    Console.WriteLine(ex.StackTrace);

    if (ex.InnerException != null)
    {
        Console.WriteLine($"Mais detalhes do erro: {ex.InnerException.Message}");
        Console.WriteLine(ex.InnerException.StackTrace);
    }
}
