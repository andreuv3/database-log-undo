using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace Log.Undo;

public class UndoLog
{
    private const string StartTransactionPattern = @"<start (\w+)>";
    private const string CommitTransactionPattern = @"<commit (\w+)>";
    private const string OperationPattern = @"<(\w+),(\d+),(\w+),(\d+)>";

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
        ParseLog();
    }

    private void ParseLog()
    {
        foreach (var line in _lines)
        {
            var match = Regex.Match(line, StartTransactionPattern);
            if (match.Success)
            {
                string transactionId = match.Groups[1].Value;
                var transaction = new Transaction(transactionId);
                _transactions.Add(transaction);
                continue;
            }

            match = Regex.Match(line, CommitTransactionPattern);
            if (match.Success)
            {
                string transactionId = match.Groups[1].Value;
                var transaction = _transactions.First(t => t.Id == transactionId);
                transaction.Commit();
                continue;
            }

            match = Regex.Match(line.Replace(" ", ""), OperationPattern);
            if (match.Success)
            {
                string transactionId = match.Groups[1].Value;
                int tupleId = int.Parse(match.Groups[2].Value);
                string columnName = match.Groups[3].Value;
                int oldValue = int.Parse(match.Groups[4].Value);
                var operation = new Operation(transactionId, tupleId, columnName, oldValue);
                var transaction = _transactions.First(t => t.Id == operation.TransactionId);
                transaction.AddOperation(operation);
            }
        }
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
