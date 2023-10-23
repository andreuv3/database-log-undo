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
    database.DropTableIfExists(metadata.TableName);
    database.CreateTable(metadata.TableName, metadata.Columns);
    database.Insert(metadata.TableName, metadata.ColumnsWithValues);

    string? logFilePath = configuration["LogFilePath"];
    var log = UndoLog.Create(logFilePath, database, metadata);
    log.PerformUndo();

    // TODO: show table data
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
