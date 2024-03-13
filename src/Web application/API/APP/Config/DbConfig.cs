namespace APP.Config;

public record DbConfig
{
    public required string ConnectionString { get; init; }
    public required string DatabaseName { get; init; }
}
