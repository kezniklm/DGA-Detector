using MongoDB.Driver;

namespace DAL;

public class ApiDbContext
{
    public virtual IMongoDatabase Database { get; init; }

    public ApiDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        Database = client.GetDatabase(databaseName);
    }
    public ApiDbContext(IMongoClient client, string databaseName)
    {
        Database = client.GetDatabase(databaseName);
    }
}
