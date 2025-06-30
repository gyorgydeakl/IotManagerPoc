using IotManagerApi.Database;

namespace IotManagerApi.Dto;

public class CreateBatchJobRequest
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required List<TagKeyValuePair> TagsToSet { get; init; }
    public required List<TagKey> TagsToDelete { get; init; }
    public required List<string> DeviceIds { get; init; } = [];
}