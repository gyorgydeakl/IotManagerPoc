using IotManagerApi.Database;
using IotManagerApi.Dto;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace IotManagerApi.Services;

public class BatchJobService(JobClient jobClient, TimeProvider timeProvider)
{
    public async Task<JobResponse> ExecuteBatchJobAsync(ExecuteSingleTimeBatchJobRequest req, CancellationToken ct)
    {
        var twinCollectionPatch = new TwinCollection();
        req.TagsToSet.ForEach(t => twinCollectionPatch[t.Key] = t.Value);
        req.TagsToDelete.ForEach(key => twinCollectionPatch[key] = null);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        return await jobClient.ScheduleTwinUpdateAsync(
            jobId: Guid.NewGuid().ToString(),
            queryCondition: "deviceId IN [" + string.Join(",", req.DeviceIds.Select(id => $"'{id}'")) + "]",
            twin: new Twin("*")
            {
                Tags = twinCollectionPatch,
                ETag = "*"
            },
            startTimeUtc: now.AddSeconds(1),
            maxExecutionTimeInSeconds: TimeSpan.FromMinutes(30).Seconds,
            cancellationToken: ct);

    }
}