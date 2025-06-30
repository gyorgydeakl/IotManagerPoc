namespace IotManagerApi.Endpoints;

public class CreateDeviceGroupRequest
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}
