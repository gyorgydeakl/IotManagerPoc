using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace IotManagerApi.Dto;

public record DeviceDto
{
    public required string DeviceId { get; init; }
    public required string ModelId { get; init; }
    public required DeviceConnectionState? ConnectionState { get; init; }
    public required DeviceStatus? Status { get; init; }
    public required string? StatusReason { get; init; }
    public required DateTime? LastActivityTime { get; init; }
    public required DeviceCapabilities Capabilities { get; init; }
    public required string Tags { get; init; }
    public required TwinProperties Properties { get; init; }
}
