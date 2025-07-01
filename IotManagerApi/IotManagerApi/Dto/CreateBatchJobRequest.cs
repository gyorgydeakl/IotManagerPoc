using IotManagerApi.Database;

namespace IotManagerApi.Dto;

public record CreateBatchJobRequest
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required List<TagKeyValuePair> TagsToSet { get; init; }
    public required List<string> TagsToDelete { get; init; }
    public required List<PropertyKeyValuePair> PropertiesToSet { get; init; }
    public required List<string> PropertiesToDelete { get; init; }
    public required List<string> DeviceIds { get; init; } = [];
}