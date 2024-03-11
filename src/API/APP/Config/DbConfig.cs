namespace APP.Config;

public record DbConfig
{
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
}
