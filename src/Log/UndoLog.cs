namespace Log.Undo;

public class UndoLog
{
    private readonly string[] _lines;
    private readonly Database _database;
    private readonly ICollection<Transaction> _transactions;

    public UndoLog(string[] lines, Database database)
    {
        _lines = lines;
        _database = database;
        _transactions = new List<Transaction>();
    }

    public void PerformUndo()
    {
    }

    public static UndoLog Create(string logFilePath, Database database)
    {
        if (string.IsNullOrWhiteSpace(logFilePath))
        {
            throw new ArgumentNullException("Caminho para arquivo de log não informado");
        }

        if (database == null)
        {
            throw new ArgumentNullException("Banco de dados não informado");
        }

        using var reader = new StreamReader(logFilePath);
        string[] lines = reader
            .ReadToEnd()
            .Split("\n");

        return new UndoLog(lines, database);
    }
}
