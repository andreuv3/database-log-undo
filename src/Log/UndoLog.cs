using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace Log.Undo;

public class UndoLog
{
    private const string StartTransactionPattern = @"<start (\w+)>";
    private const string CommitTransactionPattern = @"<commit (\w+)>";
    private const string OperationPattern = @"<(\w+),(\d+),(\w+),(\d+)>";

    private readonly string[] _lines;
    private readonly ICollection<Transaction> _transactions;
    private readonly Database _database;
    private readonly Metadata _metadata;

    public UndoLog(string[] lines, Database database, Metadata metadata)
    {
        _lines = lines;
        _transactions = new List<Transaction>();
        _database = database;
        _metadata = metadata;
    }

    public void PerformUndo()
    {
        ParseLog();
        UndoUncommitedTransactions();
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

    private void UndoUncommitedTransactions()
    {
        var uncommitedTransactions = _transactions
            .Where(t => !t.Commited)
            .ToList();

        foreach (var transaction in uncommitedTransactions)
        {
            Console.WriteLine($"A transação {transaction.Id} está realizando UNDO");
            foreach (var operation in transaction.Operations)
            {
                int value = _database.Select(_metadata.TableName, operation.ColumnName, operation.TupleId);
                if (value != operation.OldValue)
                {
                    _database.Update(_metadata.TableName, operation.ColumnName, operation.OldValue, operation.TupleId);
                    Console.WriteLine($"Atualização na tabela {_metadata.TableName} na tupla id {operation.TupleId}: coluna {operation.ColumnName} foi revertida de {value} para {operation.OldValue}");
                }
            }
            Console.WriteLine($"UNDO da transazação {transaction.Id} finalizado");
        }
    }

    public static UndoLog Create(string logFilePath, Database database, Metadata metadata)
    {
        if (string.IsNullOrWhiteSpace(logFilePath))
        {
            throw new ArgumentNullException("Caminho para arquivo de log não informado");
        }

        if (database == null)
        {
            throw new ArgumentNullException("Banco de dados não informado");
        }

        if (metadata == null)
        {
            throw new ArgumentNullException("Metadados não informados");
        }

        using var reader = new StreamReader(logFilePath);
        string[] lines = reader
            .ReadToEnd()
            .Split("\n");

        return new UndoLog(lines, database, metadata);
    }
}
