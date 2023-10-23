using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace Log.Undo;

public class UndoLog
{
    private const string StartTransactionPattern = @"<start (\w+)>";
    private const string CommitTransactionPattern = @"<commit (\w+)>";
    private const string OperationPattern = @"<(\w+),(\d+),(\w+),(\d+)>";
    private const string StartCheckpointPattern = @"<START CKPT\(([^)]+)\)>";

    private readonly string[] _lines;
    private readonly Database _database;
    private readonly Metadata _metadata;
    private ICollection<Transaction> _transactions;
    private ICollection<Operation> _operations;

    public UndoLog(string[] lines, Database database, Metadata metadata)
    {
        _lines = lines;
        _transactions = new List<Transaction>();
        _operations = new List<Operation>();
        _database = database;
        _metadata = metadata;
    }

    public void PerformUndo()
    {
        ParseLog();
        RemoveCommitedTransactions();
        RemoveOperationsFromCommitedTransactions();
        UndoUncommitedTransactions();
    }

    private void ParseLog()
    {
        bool foundLastCheckpoint = false;
        var checkpointTransactions = new Dictionary<string, bool>();
        foreach (var line in _lines.Reverse())
        {
            if (foundLastCheckpoint && checkpointTransactions.All(t => t.Value))
            {
                break;
            }

            var match = Regex.Match(line, StartCheckpointPattern);
            if (match.Success && !foundLastCheckpoint)
            {
                foundLastCheckpoint = true;
                foreach (var transactionId in match.Groups[1].Value.Split(','))
                {
                    checkpointTransactions.Add(transactionId, false);
                }
                continue;
            }

            match = Regex.Match(line, StartTransactionPattern);
            if (match.Success)
            {
                string transactionId = match.Groups[1].Value;
                var transaction = _transactions.FirstOrDefault(t => t.Id == transactionId);
                if (transaction == null)
                {
                    transaction = new Transaction(transactionId);
                    _transactions.Add(transaction);
                }

                if (checkpointTransactions.ContainsKey(transactionId))
                {
                    checkpointTransactions[transactionId] = true;
                }
                continue;
            }

            match = Regex.Match(line, CommitTransactionPattern);
            if (match.Success)
            {
                string transactionId = match.Groups[1].Value;
                var transaction = new Transaction(transactionId, true);
                _transactions.Add(transaction);
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
                _operations.Add(operation);
                continue;
            }
        }
    }

    private void RemoveCommitedTransactions()
    {
        _transactions = _transactions
            .Where(t => !t.Commited)
            .ToList();
    }

    private void RemoveOperationsFromCommitedTransactions()
    {
        _operations = _operations
            .Where(o => _transactions.Select(t => t.Id).Contains(o.TransactionId))
            .ToList();
    }

    private void UndoUncommitedTransactions()
    {
        foreach (var transaction in _transactions)
        {
            foreach (var operation in _operations.Where(o => o.TransactionId == transaction.Id))
            {
                int value = _database.Select(_metadata.TableName, operation.ColumnName, operation.TupleId);
                if (value != operation.OldValue)
                {
                    _database.Update(_metadata.TableName, operation.ColumnName, operation.OldValue, operation.TupleId);
                }
            }
            Console.WriteLine($"A transação {transaction.Id} realizou UNDO");
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
