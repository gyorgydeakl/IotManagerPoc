using IotManagerApi.Database;
using IotManagerApi.Dto;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace IotManagerApi.Services;

public class BatchJobService(JobClient jobClient, TimeProvider timeProvider)
{
    public async Task<JobResponse> ExecuteBatchJobAsync(ExecuteSingleTimeBatchJobRequest req, CancellationToken ct)
    {
        var tagsPatch = new TwinCollection();
        req.TagsToSet.ForEach(t => tagsPatch[t.Key] = t.Value);
        req.TagsToDelete.ForEach(key => tagsPatch[key] = null);

        var desiredPropertiesPatch = new TwinCollection();
        req.PropertiesToSet.ForEach(p => desiredPropertiesPatch[p.Key] = p.Value);
        req.PropertiesToDelete.ForEach(p => desiredPropertiesPatch[p] = null);

        var reportedPropertiesPatch = new TwinCollection();
        req.PropertiesToDelete.ForEach(p => reportedPropertiesPatch[p] = null);

        var jobId = Guid.NewGuid().ToString();
        var query = $"deviceId IN ['{string.Join("','", req.DeviceIds)}']";
        var start  = timeProvider.GetUtcNow().UtcDateTime.AddSeconds(1);
        var maxSec = (int)TimeSpan.FromMinutes(30).TotalSeconds;

        return await jobClient.ScheduleTwinUpdateAsync(
            jobId:jobId,
            queryCondition: query,
            twin: new Twin("*")
            {
                Tags = tagsPatch,
                Properties = new TwinProperties()
                {
                    Desired = desiredPropertiesPatch,
                    Reported = reportedPropertiesPatch,
                },
                ETag = "*"
            },
            startTimeUtc: start,
            maxExecutionTimeInSeconds: maxSec,
            cancellationToken: ct);
    }
}