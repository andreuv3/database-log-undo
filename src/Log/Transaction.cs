namespace Log.Undo;

public class Transaction
{
    public string Id { get; set; }
    public ICollection<Operation> Operations { get; set; }
    public bool Commited { get; set; }
    
    public Transaction(string id, bool commited = false)
    {
        Id = id;
        Operations = new LinkedList<Operation>();
        Commited = commited;
    }

    public void AddOperation(Operation operation)
    {
        Operations.Add(operation);
    }

    public void Commit()
    {
        Commited = true;
    }
}
