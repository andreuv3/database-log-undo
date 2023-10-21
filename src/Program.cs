using Database.Log.Undo;
using Database.Log.Undo.Metadata;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, false)
    .Build();

string metadataFilePath = configuration["MetadataFilePath"]!;
var metadata = MetadataFactory.BuildFromJsonFile(metadataFilePath);

string connectionString = configuration.GetConnectionString("Undo")!;
using var database = DatabaseFactory.BuildFromConnectionString(connectionString);
await database.DropTableIfExistsAsync(metadata.TableName);
await database.CreateTableAsync(metadata.TableName, metadata.Columns);

Console.ReadKey();
