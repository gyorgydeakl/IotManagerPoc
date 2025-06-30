namespace IotManagerApi.Dto;

public record CreateDeviceRequest
{
    public required string DeviceId { get; init; }
};