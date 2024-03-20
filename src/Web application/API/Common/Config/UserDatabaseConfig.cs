namespace Common.Config;

public class UserDatabaseConfig
{
    public required bool UseMySql { get; set; }
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
}
