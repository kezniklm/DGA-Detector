using MongoDB.Driver;

namespace DAL;

public class ApiDbContext
{
    public ApiDbContext(string connectionString, string databaseName)
    {
        MongoClient client = new MongoClient(connectionString);
        Database = client.GetDatabase(databaseName);
    }

    public ApiDbContext(IMongoClient client, string databaseName) => Database = client.GetDatabase(databaseName);

    public virtual IMongoDatabase Database { get; init; }
}
