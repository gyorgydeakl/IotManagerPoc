using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace IotManagerApi.Database;

public class BatchJob
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required List<TagKeyValuePair> TagsToSet { get; init; }
    public required List<TagKey> TagsToDelete { get; init; }
    public required List<PropertyKeyValuePair> PropertiesToSet { get; init; }
    public required List<PropertyKey> PropertiesToDelete { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; set; }
    public List<DeviceId> DeviceIds { get; set; } = [];
}

public record DeviceId(string Value);
public record TagKeyValuePair(string Key, string Value);
public record TagKey(string Value);
public record PropertyKeyValuePair(string Key, JsonNode Value);
public record PropertyKey(string Value);