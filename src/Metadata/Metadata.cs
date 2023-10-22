using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        
        var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int[]>>>(json);
        if (data == null)
        {
            throw new Exception("Não foi possível ler o arquivo de metadados");
        }

        string tableName = data.First().Key;
        var columns = data.First().Value;
        int tuplesCount = columns.First().Value.Length;

        return new Metadata(tableName, columns, tuplesCount);
    }
}
