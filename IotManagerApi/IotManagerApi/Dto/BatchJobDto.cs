using FastEndpoints;
using IotManagerApi.Database;

namespace IotManagerApi.Dto;

public record BatchJobDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required List<TagKeyValuePair> TagsToSet { get; init; }
    public required List<TagKey> TagsToDelete { get; init; }
    public required List<PropertyKeyValuePair> PropertiesToSet { get; init; }
    public required List<PropertyKey> PropertiesToDelete { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public List<DeviceId> DeviceIds { get; set; } = [];
}

public record GetBatchJobRequest
{
    public required Guid JobId { get; init; }
}

public record ExecuteBatchJobRequest
{
    [RouteParam]
    public required Guid JobId { get; init; }
}