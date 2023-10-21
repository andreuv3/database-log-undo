using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Database.Log.Undo.Metadata
{
    public class MetadataFactory
    {
        public static Metadata BuildFromJsonFile(string jsonFilePath)
        {
            using var reader = new StreamReader(jsonFilePath);
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
}
