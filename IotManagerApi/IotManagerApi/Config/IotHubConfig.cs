namespace IotManagerApi.Config;

public class IotHubConfig
{
    public required string HostName { get; init; }
    public required string ConnectionString { get; init; }
}