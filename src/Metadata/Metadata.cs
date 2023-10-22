using Newtonsoft.Json;

namespace Log.Undo;

public class Metadata
{
    private readonly string _tableName;
    private readonly IDictionary<string, int[]> _columns;
    private readonly int _tuplesCount;

    public string TableName => _tableName;
    public string[] Columns => _columns.Keys.ToArray();
    public IDictionary<string, int[]> ColumnsWithValues => _columns;

    public Metadata(string tableName, IDictionary<string, int[]> columns, int tuplesCount)
    {
        _tableName = tableName;
        _columns = columns;
        _tuplesCount = tuplesCount;
    }

    public static Metadata CreateFromJsonFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new Exception("Caminho para arquivo de metadados não encontrado");
        }

        using var reader = new StreamReader(path);
        string json = reader.ReadToEnd();
        dynamic data = JsonConvert.DeserializeObject(json)!;
        // TODO: read data from metadata json file
        
        string tableName = "test";
        var columns = new Dictionary<string, int[]>
        {
            { "id", new int[] { 1, 2, 3, 4 } },
            { "A", new int[] { 20, 20, 1, 1 } },
            { "B", new int[] { 55, 30, 1, 1 } },
            { "C", new int[] { 55, 30, 1, 1 } },
            { "D", new int[] { 55, 30, 1, 1 } },
        };
        int tuplesCount = 2;

        return new Metadata(tableName, columns, tuplesCount);
    }
}
