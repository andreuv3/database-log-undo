﻿namespace Log.Undo;

public class Operation
{
    public string TransactionId { get; set; }
    public int TupleId { get; set; }
    public string ColumnName { get; set; }
    public int OldValue { get; set; }

    public Operation(string transactionId, int tupleId, string columnName, int oldValue)
    {
        TransactionId = transactionId;
        TupleId = tupleId;
        ColumnName = columnName;
        OldValue = oldValue;
    }
}
