using IotManagerApi.Database;

namespace IotManagerApi.Dto;

public record ExecuteSingleTimeBatchJobRequest
{
    public List<string> DeviceIds { get; init; } = [];
    public List<TagKeyValuePair> TagsToSet { get; init; } = [];
    public List<string> TagsToDelete { get; init; } = [];
    public List<PropertyKeyValuePair> PropertiesToSet { get; init; } = [];
    public List<string> PropertiesToDelete { get; init; } = [];
}