using IotManagerApi.Database;
using IotManagerApi.Dto;
using IotManagerApi.Endpoints;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace IotManagerApi;

public static class Mapper
{
    public static DeviceDto ToDeviceData(this Twin twin)
    {
        return new DeviceDto
        {
            DeviceId = twin.DeviceId,
            ModelId = twin.ModelId,
            ConnectionState = twin.ConnectionState,
            Status = twin.Status,
            StatusReason = twin.StatusReason,
            LastActivityTime = twin.LastActivityTime,
            Capabilities = twin.Capabilities,
            Tags = twin.Tags.ToJson(),
            Properties = twin.Properties,
            Twin = twin
        };
    }

    public static Device ToDevice(this Twin twin)
    {
        return new Device(twin.DeviceId)
        {
            Capabilities = twin.Capabilities,
            Status = twin.Status ?? DeviceStatus.Disabled,
            StatusReason = twin.StatusReason,
            ETag = twin.ETag,
        };
    }

    public static BatchJobDto ToBatchJobDto(this BatchJob batchJob)
    {
        return new BatchJobDto
        {
            Id = batchJob.Id,
            Name = batchJob.Name,
            Description = batchJob.Description,
            TagsToSet = batchJob.TagsToSet,
            TagsToDelete = batchJob.TagsToDelete,
            CreatedAt = batchJob.CreatedAt,
            UpdatedAt = batchJob.UpdatedAt,
            DeviceIds = batchJob.DeviceIds
        };
    }
}