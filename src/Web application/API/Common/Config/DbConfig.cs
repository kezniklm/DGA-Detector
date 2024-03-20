namespace Common.Config;

public class DbConfig
{
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
    public required UserDatabaseConfig UserDatabaseConfig { get; set; }
}
