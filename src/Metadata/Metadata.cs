namespace Database.Log.Undo.Metadata
{
    public class Metadata
    {
        private readonly string _tableName;
        private readonly IDictionary<string, int[]> _columns;
        private readonly int _tuplesCount;

        public string TableName => _tableName;
        public string[] Columns => _columns.Keys.ToArray();

        public Metadata(string tableName, IDictionary<string, int[]> columns, int tuplesCount)
        {
            _tableName = tableName;
            _columns = columns;
            _tuplesCount = tuplesCount;
        }
    }
}
